namespace ApiControleFinanceiro.Controllers.Compra
{
    // Cálculos de valor e vencimento das parcelas (não acessa o banco)
    public static class CompraCalculo
    {
        // Divide o total em parcelas. A diferença de centavos vai para a PRIMEIRA parcela.
        // Ex.: 100,00 em 3x => 33,34 / 33,33 / 33,33
        public static List<decimal> DividirValor(decimal total, int qtdParcelas)
        {
            decimal valorBase = Math.Truncate(total / qtdParcelas * 100m) / 100m;
            decimal diferenca = total - (valorBase * qtdParcelas);

            List<decimal> valores = new List<decimal>();
            for (int i = 0; i < qtdParcelas; i++)
            {
                valores.Add(i == 0 ? valorBase + diferenca : valorBase);
            }

            return valores;
        }

        // Vencimento padrão: dia 10 do mês da compra + numeroParcela meses.
        // Ex.: compra em 13/11/2026 => parcela 1 = 10/12/2026, parcela 2 = 10/01/2027
        public static DateTime VencimentoPadrao(DateTime dataCompra, int numeroParcela)
        {
            DateTime mes = new DateTime(dataCompra.Year, dataCompra.Month, 1).AddMonths(numeroParcela);
            return new DateTime(mes.Year, mes.Month, 10);
        }

        // Vencimento no cartão: respeita o dia de fechamento.
        // Compra até o dia de fechamento (inclusive) cai na fatura do mês; depois, na do mês seguinte.
        public static DateTime VencimentoCartao(DateTime dataCompra, int diaFechamento, int numeroParcela)
        {
            int deslocamento = dataCompra.Day <= diaFechamento ? 0 : 1;
            return VencimentoPadrao(dataCompra, numeroParcela + deslocamento);
        }
    }
}