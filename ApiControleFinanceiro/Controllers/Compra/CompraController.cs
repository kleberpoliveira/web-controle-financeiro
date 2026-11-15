using ApiControleFinanceiro.Excecoes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Globalization;

namespace ApiControleFinanceiro.Controllers.Compra
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompraController : ControllerBase
    {
        [HttpPost("LancarCompra")]
        public void Post([FromBody] CompraParameters jsonPar)
        {
            if (DateTime.TryParseExact(jsonPar.Data, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dataConvertida))
            {
                // A variável 'dataConvertida' agora é um DateTime válido (ex: 25/12/2026 00:00:00)
                ValidarCompra(jsonPar);
                Gravar(jsonPar, dataConvertida);
            }
            else
            {
                throw new RegraNegocioException("Formato de data inválido. Use o padrão DD/MM/AAAA.");
            }
        }

        private static void Gravar(CompraParameters p, DateTime dataCompra)
        {
            bool ehCartao = p.TipoDeCompra == TipoDeCompra.CARTAO_CREDITO;

            // PIX e débito em conta criam um movimento na conta bancária para cada parcela
            bool gravaMovimento = p.TipoDeCompra == TipoDeCompra.PIX || p.TipoDeCompra == TipoDeCompra.DEBITO_EM_CONTA;

            // PIX lançado com a data de hoje: movimento com a data de hoje e conciliado
            bool pagoHoje = p.TipoDeCompra == TipoDeCompra.PIX && p.PixHoje;

            string tipoOperacao = p.TipoDeCompra == TipoDeCompra.PIX ? "PIX_SAIDA" : "DEBITO_EM_CONTA";

            string conexao = GLO.Conexao.SqlServer;

            // O plano de conta precisa existir
            int qtdPlano = TSQL.ExecuteScalar<int>(conexao,
                "select count(*) from plano_conta where id_plano_conta = @id",
                TSQL.Par("@id", p.IdPlanoConta));
            if (qtdPlano == 0)
            {
                throw new RegraNegocioException("Plano de conta não encontrado.");
            }

            // Quando há movimento, a conta bancária precisa existir
            if (gravaMovimento)
            {
                int qtdConta = TSQL.ExecuteScalar<int>(conexao,
                    "select count(*) from conta_bancaria where id_conta_bancaria = @id",
                    TSQL.Par("@id", p.IdContaBancaria));
                if (qtdConta == 0)
                {
                    throw new RegraNegocioException("Conta bancária não encontrada.");
                }
            }

            // No cartão, o cartão precisa existir e precisamos do dia de fechamento
            int diaFechamento = 0;
            if (ehCartao)
            {
                object? resultado = TSQL.ExecuteScalar(conexao,
                    "select dia_fechamento from cartao where id_cartao = @id",
                    TSQL.Par("@id", p.IdCartao));
                if (resultado == null || resultado == DBNull.Value)
                {
                    throw new RegraNegocioException("Cartão não encontrado.");
                }
                diaFechamento = System.Convert.ToInt32(resultado);
            }

            decimal total = Math.Round(p.Total, 2);
            List<decimal> valores = CompraCalculo.DividirValor(total, p.QtdParcelas);

            // Tudo numa transação: se algo falhar antes do Close(), nada é gravado
            using (TSQL tsql = new TSQL(conexao, true, true))
            {
                // 1) Cabeçalho da compra
                int idCab = tsql.ExecuteScalar<int>(Comando(@"
                    insert into cf_cab (data, descricao, total)
                    values (@data, @descricao, @total);
                    select cast(scope_identity() as int);",
                    TSQL.Par("@data", dataCompra),
                    TSQL.Par("@descricao", p.Descricao.Trim()),
                    TSQL.Par("@total", total)));

                // 2) Parcelas
                for (int numero = 1; numero <= p.QtdParcelas; numero++)
                {
                    decimal valor = valores[numero - 1];

                    // No cartão o vencimento respeita o fechamento; nos demais é sempre dia 10 do mês seguinte
                    DateTime vencimento = ehCartao
                        ? CompraCalculo.VencimentoCartao(dataCompra, diaFechamento, numero)
                        : CompraCalculo.VencimentoPadrao(dataCompra, numero);

                    // Movimento na conta bancária (PIX e débito em conta), um por parcela
                    int? idMovto = null;
                    if (gravaMovimento)
                    {
                        // Pago hoje: data de hoje e conciliado. Senão: data do vencimento e não conciliado.
                        DateTime dataMovto = pagoHoje ? DateTime.Today : vencimento;

                        idMovto = tsql.ExecuteScalar<int>(Comando(@"
                            insert into conta_bancaria_movto (id_conta_bancaria, data, tipo_movimento, tipo_opercao, conciliado, valor)
                            values (@id_conta_bancaria, @data, 'D', @tipo_opercao, @conciliado, @valor);
                            select cast(scope_identity() as int);",
                            TSQL.Par("@id_conta_bancaria", p.IdContaBancaria),
                            TSQL.Par("@data", dataMovto),
                            TSQL.Par("@tipo_opercao", tipoOperacao),
                            TSQL.Par("@conciliado", pagoHoje),
                            TSQL.Par("@valor", valor)));
                    }

                    // Fatura do cartão (busca; se não existir, cria), uma por vencimento
                    int? idFatura = null;
                    if (ehCartao)
                    {
                        idFatura = BuscarOuCriarFatura(tsql, p.IdCartao!.Value, vencimento);
                    }

                    int idParc = tsql.ExecuteScalar<int>(Comando(@"
                        insert into cf_parc (id_cf_cab, numero, id_conta_bancaria_movto, id_fatura, valor, vencimento)
                        values (@id_cf_cab, @numero, @id_movto, @id_fatura, @valor, @vencimento);
                        select cast(scope_identity() as int);",
                        TSQL.Par("@id_cf_cab", idCab),
                        TSQL.Par("@numero", numero),
                        TSQL.Par("@id_movto", idMovto),
                        TSQL.Par("@id_fatura", idFatura),
                        TSQL.Par("@valor", valor),
                        TSQL.Par("@vencimento", vencimento)));

                    // 3) Plano de conta da parcela (100% do valor da parcela)
                    tsql.ExecuteNonQuery(Comando(@"
                        insert into cf_parc_plano_conta (id_plano_conta, id_cf_parc, valor)
                        values (@id_plano_conta, @id_cf_parc, @valor)",
                        TSQL.Par("@id_plano_conta", p.IdPlanoConta),
                        TSQL.Par("@id_cf_parc", idParc),
                        TSQL.Par("@valor", valor)));
                }

                tsql.Close(); // confirma tudo (commit)
            }
        }

        // Procura a fatura do cartão com o vencimento informado; se não existir, cria.
        // Roda dentro da transação da compra.
        private static int BuscarOuCriarFatura(TSQL tsql, int idCartao, DateTime vencimento)
        {
            int idFatura = tsql.ExecuteScalar<int>(Comando(@"
                select top 1 id_fatura
                  from fatura
                 where id_cartao = @id_cartao
                   and data_vencimento = @vencimento
                 order by id_fatura",
                TSQL.Par("@id_cartao", idCartao),
                TSQL.Par("@vencimento", vencimento)));

            if (idFatura != 0)
            {
                return idFatura;
            }

            return tsql.ExecuteScalar<int>(Comando(@"
                insert into fatura (id_cartao, data_vencimento)
                values (@id_cartao, @vencimento);
                select cast(scope_identity() as int);",
                TSQL.Par("@id_cartao", idCartao),
                TSQL.Par("@vencimento", vencimento)));
        }

        private static SqlCommand Comando(string sql, params SqlParameter[] par)
        {
            SqlCommand cmd = new SqlCommand(sql);
            cmd.Parameters.AddRange(par);
            return cmd;
        }

        private static void ValidarCompra(CompraParameters p)
        {
            if (string.IsNullOrWhiteSpace(p.Descricao))
            {
                throw new RegraNegocioException("Informe a descrição da compra.");
            }

            if (p.Total <= 0)
            {
                throw new RegraNegocioException("O total deve ser maior que zero.");
            }

            if (p.QtdParcelas < 1)
            {
                throw new RegraNegocioException("A quantidade de parcelas deve ser no mínimo 1.");
            }

            if (p.IdPlanoConta <= 0)
            {
                throw new RegraNegocioException("Selecione o plano de conta.");
            }

            switch (p.TipoDeCompra)
            {
                case TipoDeCompra.CARTAO_CREDITO:
                    if (p.IdCartao == null)
                    {
                        throw new RegraNegocioException("Selecione o cartão de crédito.");
                    }
                    break;

                case TipoDeCompra.DEBITO_EM_CONTA:
                    if (p.IdContaBancaria == null)
                    {
                        throw new RegraNegocioException("Selecione a conta bancária para o débito em conta.");
                    }
                    break;

                case TipoDeCompra.PIX:
                    if (p.IdContaBancaria == null)
                    {
                        throw new RegraNegocioException("Selecione a conta bancária para o PIX.");
                    }

                    // PIX lançado com a data de hoje é um pagamento feito na hora: só pode ter 1 parcela
                    if (p.PixHoje && p.QtdParcelas != 1)
                    {
                        throw new RegraNegocioException("PIX lançado com a data de hoje só pode ter 1 parcela.");
                    }
                    break;
            }
        }
    }
}