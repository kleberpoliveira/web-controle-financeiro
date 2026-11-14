using System.Text.Json.Serialization;

namespace ApiControleFinanceiro.Controllers.Compra
{
    public enum TipoDeCompra
    {
        CARTAO_CREDITO,
        DEBITO_EM_CONTA,
        PIX,
        OUTRO
    }

    public class CompraParameters
    {
        // Mantida como string no formato DD/MM/AAAA; o controller converte para DateTime
        public string Data { get; set; } = null!;
        public string Descricao { get; set; } = null!;
        public decimal Total { get; set; }
        public int IdPlanoConta { get; set; }

        // Aceita o texto "CARTAO_CREDITO", "DEBITO_EM_CONTA", "PIX" ou "OUTRO" vindo do front
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TipoDeCompra TipoDeCompra { get; set; }

        // Só usado quando TipoDeCompra = CARTAO_CREDITO
        public int? IdCartao { get; set; }

        // Só usado quando TipoDeCompra = DEBITO_EM_CONTA ou PIX
        public int? IdContaBancaria { get; set; }

        // Só usado no PIX: true = pagamento feito hoje (movimento com a data de hoje, conciliado);
        // false = movimento na data de vencimento da parcela, não conciliado
        public bool PixHoje { get; set; }

        public int QtdParcelas { get; set; }
    }
}