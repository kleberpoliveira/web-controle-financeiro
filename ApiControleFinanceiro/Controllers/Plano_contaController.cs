using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ApiControleFinanceiro.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Plano_ContaController : ControllerBase
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

            return Ok(new { mensagem = "Olá Mundo!" });
        }

        [HttpGet("Listar")]
        public IActionResult Listar()
        {
            
            DataTable tb = TSQL.ToTable(GLO.Conexao.SqlServer,"select * from plano_conta", "plano_conta");

            string jsonResult = Newtonsoft.Json.JsonConvert.SerializeObject(tb);
            return Content(jsonResult, "application/json");
        }
    }
}
