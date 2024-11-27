using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Enuns;
using minimal_api.Dominio.Interfaces;
using minimal_api.Dominio.ModelViews;
using minimal_api.Dominio.Servicos;
using MinimalApi.DTOs;
using MinimalApi.Infraestrutura.Db;

#region builder
var builder = WebApplication.CreateBuilder(args);
var key = builder.Configuration.GetSection("Jwt").ToString();
if (string.IsNullOrEmpty(key)) key = "123456";

builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option =>
{
    option.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
}
);

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAdministradorServico, AdminstradorServico>();
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT aqui:"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme{
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }, new string[]{}
        }
    });
} );


builder.Services.AddDbContext<DbContexto>(options => {
    options.UseMySql(
        builder.Configuration.GetConnectionString("MySql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySql"))
    );
});

var app = builder.Build();
#endregion 


#region Home
app.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
#endregion


#region Adminstradores
string GerarTokenJWT(Administrador administrador){

    if (string.IsNullOrEmpty(key)) return string.Empty;

    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>()
    {
        new Claim("Email", administrador.Email),
        new Claim("Perfl", administrador.Perfil),
         new Claim(ClaimTypes.Role, administrador.Perfil),
    };

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddDays(1),
        signingCredentials : credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}


// Rota de login
app.MapPost("administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) =>
{
    var administrador = administradorServico.Login(loginDTO);
    if (administrador != null)
    {
        string token = GerarTokenJWT(administrador);

        return Results.Ok(new AdministradorLogado
        {
            Email = administrador.Email,
            Perfil = administrador.Perfil,
            Token = token
        });
    }
    else
        return Results.Unauthorized();

}).WithTags("Administradores");


app.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorServico administradorServico) =>
{
    var adms = new List<AdministradorModelView>();
    var administradores = administradorServico.Todos(pagina);

    foreach (var adm in administradores)
    {
        adms.Add(new AdministradorModelView
        {
            Id = adm.Id,
            Email = adm.Email,
            Perfil = adm.Perfil
        });
    }
    return Results.Ok(adms);
}).RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm"})
.WithTags("Administradores");


app.MapGet("/Administradores/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) =>
{
   var administrador = administradorServico.BuscaPorId(id);
    if (administrador == null) return Results.NotFound();
    return Results.Ok(new AdministradorModelView
    {
        Id = administrador.Id,
        Email = administrador.Email,
        Perfil = administrador.Perfil
    });
}).RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm"})
.WithTags("Administradores");


    app.MapPost("administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) =>
    {

        var validacao = new AdminstradorModelView
        {
            Mensagem = new List<string>()
        };

        if (string.IsNullOrEmpty(administradorDTO.Email))
            validacao.Mensagem.Add("O campo email é obrigatório");

        if (string.IsNullOrEmpty(administradorDTO.Senha))
            validacao.Mensagem.Add("O campo senha é obrigatório");

        if (administradorDTO.Perfil == null)
            validacao.Mensagem.Add("O campo perfil é obrigatório");


        if (validacao.Mensagem.Count > 0)
            return Results.BadRequest(validacao);

        var adm = new Administrador
        {
            Email = administradorDTO.Email,
            Senha = administradorDTO.Senha,
            Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Editor.ToString()
        };

        administradorServico.Incluir(adm);

        return Results.Created($"/Administrador/{adm.Id}", (new AdministradorModelView
        {
            Id = adm.Id,
            Email = adm.Email,
            Perfil = adm.Perfil
        }));

    }).RequireAuthorization()
    .RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm"})
    .WithTags("Administradores"); ;


    #endregion

#region Veiculos

    AdminstradorModelView validaDTO(VeiculoDTO veiculoDTO)
    {
        var validacao = new AdminstradorModelView
        {
            Mensagem = new List<string>()
        };

        if (string.IsNullOrEmpty(veiculoDTO.Nome))
            validacao.Mensagem.Add("O nome do veículo é obrigatório");

        if (string.IsNullOrEmpty(veiculoDTO.Marca))
            validacao.Mensagem.Add("A Marca do veículo é obrigatório");

        if (veiculoDTO.Ano < 1950)
            validacao.Mensagem.Add("Veículo é Muito antigo, somente a partir de 1950");

        return validacao;
    }




    app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
    {


        var validacao = validaDTO(veiculoDTO);
        if (validacao.Mensagem.Count > 0)
            return Results.BadRequest(validacao);

        var veiculo = new Veiculo
        {
            Nome = veiculoDTO.Nome,
            Marca = veiculoDTO.Marca,
            Ano = veiculoDTO.Ano
        };

        veiculoServico.Incluir(veiculo);

        return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
    }).RequireAuthorization()
    .RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm,Editor"})
    .WithTags("Veiculos");



    app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) =>
    {
        var veiculos = veiculoServico.Todos(pagina);

        return Results.Ok(veiculos);
    }).RequireAuthorization().WithTags("Veiculos");



    app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
    {
        var veiculo = veiculoServico.BuscaPorId(id);

        if (veiculo == null) return Results.NotFound();

        return Results.Ok(veiculo);
    }).RequireAuthorization()
    .RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm,Editor"})
    .WithTags("Veiculos");



    app.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
    {
        var veiculo = veiculoServico.BuscaPorId(id);
        if (veiculo == null) return Results.NotFound();


        var validacao = validaDTO(veiculoDTO);
        if (validacao.Mensagem.Count > 0)
            return Results.BadRequest(validacao);

        veiculo.Nome = veiculoDTO.Nome;
        veiculo.Marca = veiculoDTO.Marca;
        veiculo.Ano = veiculoDTO.Ano;

        return Results.Ok(veiculo);
    }).RequireAuthorization()
    .RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm"}).WithTags("Veiculos");



    app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
    {
        var veiculo = veiculoServico.BuscaPorId(id);
        if (veiculo == null) return Results.NotFound();

        veiculoServico.Apagar(veiculo);
        return Results.NoContent();

    }).RequireAuthorization()
    .RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm"}).WithTags("Veiculos");


    #endregion


#region App
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseAuthentication();
    app.UseAuthorization();

    app.Run();

    #endregion
