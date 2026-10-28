using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiControleFinanceiro.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { mensagem = "Olá Mundo!" });
        }

        [HttpGet("{i}")]
        public IActionResult Get(int i )
        {
            return Ok(new { mensagem = $"Olá Mundo! Você enviou o valor: {i}" });
        }
    }
}
