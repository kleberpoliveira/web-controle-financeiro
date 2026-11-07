using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace ApiControleFinanceiro.Controllers.Compra
{
    public class CompraParameters
    {
        public string Data { get; set; } = null!;
        public string Descricao { get; set; } = null!;
        public decimal Total { get; set; }
        public int QtdParcelas { get; set; }
    }
}
