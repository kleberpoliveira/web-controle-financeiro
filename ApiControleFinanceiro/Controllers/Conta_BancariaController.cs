using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace ApiControleFinanceiro.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Conta_BancariaController : ControllerBase
    {
        [HttpGet("Listar")]
        public IActionResult Listar()
        {
            DataTable tb = TSQL.ToTable(GLO.Conexao.SqlServer, "select id_conta_bancaria, descricao from conta_bancaria", "conta_bancaria");

            string jsonResult = Json.Convert.Serialize(tb);
            return Content(jsonResult, "application/json");
        }
    }
}