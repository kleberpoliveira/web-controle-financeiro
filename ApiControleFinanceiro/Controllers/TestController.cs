using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiControleFinanceiro.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        public IEnumerable<Dados> Get()
        {
            var list = new List<Dados>();
            list.Add(new Dados() { Id = 1, Nome = "Nome 1" });
            list.Add(new Dados() { Id = 2, Nome = "Nome 2" });
            list.Add(new Dados() { Id = 3, Nome = "Nome 3" });
            list.Add(new Dados() { Id = 4, Nome = "Nome 4" });

            return list;
        }
    }
}
