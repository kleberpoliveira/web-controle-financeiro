using Microsoft.AspNetCore.Mvc;
using System.Globalization; 
using ApiControleFinanceiro.Excecoes;
using System.ComponentModel;

namespace ApiControleFinanceiro.Controllers.Compra
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompraController : ControllerBase
    {
        [HttpPost("LancarCompra")]
        public void Post([FromBody] CompraParameters jsonPar)
        {


            if (DateTime.TryParseExact(jsonPar.Data, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dataConvertida))
            {
                // A variável 'dataConvertida' agora é um DateTime válido (ex: 25/12/2026 00:00:00)
                // Pode usar 'dataConvertida' para gravar no banco de dados




            }
            else
            {
                throw new RegraNegocioException("Formato de data inválido. Use o padrão DD/MM/AAAA.");
            }

        }
    }
}
