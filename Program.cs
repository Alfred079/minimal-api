var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/login", (MinimalApi.DTOs.LoginDTO loginDTO) =>
{
    if (loginDTO.Email == "adm@teste.com" && loginDTO.Senha == "123456")
    {
        return Results.Ok("Login efetuado com sucesso");
    }
    else
    {
        return Results.Unauthorized();
    }
});


app.Run();

