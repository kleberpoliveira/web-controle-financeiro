namespace ApiControleFinanceiro.Controllers
{
    public class HomeParameters
    {
        public DateTime Data { get; set; }
        public HomeParameters(DateTime data)
        {
            Data = data;
        }
    }
}
