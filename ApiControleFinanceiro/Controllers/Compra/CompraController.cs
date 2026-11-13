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
            // TEMPORÁRIO: por enquanto só o PIX sem conta é gravado (3b e 3c vêm depois)
            bool pixSemConta = (p.TipoDeCompra == TipoDeCompra.PIX && p.IdContaBancaria == null) || p.TipoDeCompra == TipoDeCompra.OUTRO;
            if (!pixSemConta)
            {
                throw new RegraNegocioException("A gravação deste tipo de compra ainda não foi implementada.");
            }

            string conexao = GLO.Conexao.SqlServer;

            // O plano de conta precisa existir
            int qtdPlano = TSQL.ExecuteScalar<int>(conexao,
                "select count(*) from plano_conta where id_plano_conta = @id",
                TSQL.Par("@id", p.IdPlanoConta));
            if (qtdPlano == 0)
            {
                throw new RegraNegocioException("Plano de conta não encontrado.");
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
                    DateTime vencimento = CompraCalculo.VencimentoPadrao(dataCompra, numero);

                    int idParc = tsql.ExecuteScalar<int>(Comando(@"
                        insert into cf_parc (id_cf_cab, numero, id_conta_bancaria_movto, id_fatura, valor, vencimento)
                        values (@id_cf_cab, @numero, @id_movto, @id_fatura, @valor, @vencimento);
                        select cast(scope_identity() as int);",
                        TSQL.Par("@id_cf_cab", idCab),
                        TSQL.Par("@numero", numero),
                        TSQL.Par("@id_movto", null),
                        TSQL.Par("@id_fatura", null),
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
                    // PIX com conta escolhida é um pagamento feito na hora: só pode ter 1 parcela
                    if (p.IdContaBancaria != null && p.QtdParcelas != 1)
                    {
                        throw new RegraNegocioException("PIX com conta selecionada só pode ser lançado em 1 parcela.");
                    }
                    break; 
            }
        }
    }
}