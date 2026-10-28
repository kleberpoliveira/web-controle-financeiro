using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiControleFinanceiro.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TesteController : ControllerBase
    {
        public String Get()
        {
            return "Olá Mundo!";
        }
    }
}
