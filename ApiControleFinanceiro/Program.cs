var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:8000") // URL do seu front-end
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var senhaBanco = builder.Configuration["Banco:Senha"]
    ?? throw new InvalidOperationException("Senha do banco não configurada (Banco:Senha). Use: dotnet user-secrets set \"Banco:Senha\" \"...\"");

ApiControleFinanceiro.GLO.Conexao.SqlServer = ApiControleFinanceiro.TSQL.MontaStringConexao("localhost", "controle_financeiro", "sa", senhaBanco);


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseCors("PermitirFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
