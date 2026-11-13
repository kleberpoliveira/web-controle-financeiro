using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace ApiControleFinanceiro.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartaoController : ControllerBase
    {
        [HttpGet("Listar")]
        public IActionResult Listar()
        {
            DataTable tb = TSQL.ToTable(GLO.Conexao.SqlServer, "select ID_CARTAO, NOME from cartao", "cartao");

            string jsonResult = Json.Convert.Serialize(tb);
            return Content(jsonResult, "application/json");
        }
    }
}