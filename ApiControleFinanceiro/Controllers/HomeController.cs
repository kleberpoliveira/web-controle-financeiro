using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ApiControleFinanceiro.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            /*
            DataTable tb = TSQL.SPToTable(
                    GLO.Conexao.SqlServer, 
                    "sp_relatorio_geral", 
                    new SqlParameter("@data", new DateTime(DateTime.Now.Year, 1, 1))
            );*/

            return Ok(new { mensagem = "Olá Mundo! Conexão OK." });
        }

        [HttpPost]
        public IActionResult GetRelatorio([FromBody] HomeParameters jsonPar)
        {
            DataTable tb = TSQL.SPToTable(GLO.Conexao.SqlServer, "sp_relatorio_geral", new SqlParameter("@data", jsonPar.Data));
            
            string jsonResult = Json.Convert.Serialize(tb);
            return Content(jsonResult, "application/json");
        }
    }
}
