namespace ApiControleFinanceiro.Excecoes
{
    // Use esta exceção para erros de regra de negócio (ex.: data inválida).
    // A mensagem dela é mostrada ao usuário, com status 400.
    public class RegraNegocioException : Exception
    {
        public RegraNegocioException(string mensagem) : base(mensagem) { }
    }
}