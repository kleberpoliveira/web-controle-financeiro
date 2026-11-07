using ApiControleFinanceiro.Excecoes;

namespace ApiControleFinanceiro.Middleware
{
    public class TratamentoErrosMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostEnvironment _env;
        private readonly ILogger<TratamentoErrosMiddleware> _logger;

        public TratamentoErrosMiddleware(
            RequestDelegate next,
            IHostEnvironment env,
            ILogger<TratamentoErrosMiddleware> logger)
        {
            _next = next;
            _env = env;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (RegraNegocioException ex)
            {
                // Erro de regra de negócio: a mensagem é para o usuário
                await EscreverErro(context, StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                // Erro inesperado: registra no log e devolve 500
                _logger.LogError(ex, "Erro inesperado");

                string mensagem = _env.IsDevelopment()
                    ? ex.Message
                    : "Ocorreu um erro inesperado no servidor.";

                await EscreverErro(context, StatusCodes.Status500InternalServerError, mensagem);
            }
        }

        private static async Task EscreverErro(HttpContext context, int status, string mensagem)
        {
            if (context.Response.HasStarted) return;

            context.Response.StatusCode = status;
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsync(Json.Convert.Serialize(new { mensagem }));
        }
    }
}