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

        [HttpGet("Listar")]
        public IActionResult Listar()
        {
            
            DataTable tb = TSQL.ToTable(GLO.Conexao.SqlServer,"select * from plano_conta", "plano_conta");

            string jsonResult = Newtonsoft.Json.JsonConvert.SerializeObject(tb);
            return Content(jsonResult, "application/json");
        }
    }
}
