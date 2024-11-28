

using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Servicos;
using MinimalApi.Infraestrutura.Db;

namespace Test.Domain.Entidades;

[TestClass]
public class AdministradorServicoTest
{

    private DbContexto CriarContextoDeTeste()
    {
        var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var path = Path.GetFullPath(Path.Combine(assemblyPath ?? "", "..", "..", ".."));
        var builder = new ConfigurationBuilder()
            .SetBasePath(path ?? Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        var configuration = builder.Build();

        return new DbContexto(configuration);

     
    }

    [TestMethod]
    public void TestandoSalvarAdministrador()
    {
        //Arange
        var context =  CriarContextoDeTeste();
        context.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");

        var adm = new Administrador();
        adm.Email = "teste@teste.com";
        adm.Senha = "teste";
        adm.Perfil = "Adm";

        
        var administradorServico = new AdminstradorServico(context);

        //Act
        administradorServico.Incluir(adm);
        

        //Assert
        Assert.AreEqual(1, administradorServico.Todos(1).Count());

    }

      public void TestandoBucaPorId()
    {
        //Arange
        var context =  CriarContextoDeTeste();
        context.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");

        var adm = new Administrador();
        adm.Email = "teste@teste.com";
        adm.Senha = "teste";
        adm.Perfil = "Adm";

        
        var administradorServico = new AdminstradorServico(context);

        //Act
        administradorServico.Incluir(adm);
        var admDoBanco =administradorServico.BuscaPorId(adm.Id);
        

        //Assert
        Assert.AreEqual(1, admDoBanco?.Id);

    }

    [TestMethod]
    public void TestandoLogin()
    {
        //Arange
        var context = CriarContextoDeTeste();
        context.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");

        var adm = new Administrador()
        {
            Email = "teste@teste.com",
            Senha = "teste",
            Perfil = "Adm"
        };
        context.Administradores.Add(adm);
        context.SaveChanges();

        var administradorServico = new AdminstradorServico(context);

       
        // Act
    var loginDTO = new MinimalApi.DTOs.LoginDTO
    {
        Email = "teste@teste.com",
        Senha = "teste"
    };

    var resultado = administradorServico.Login(loginDTO);

    // Assert
    Assert.IsNotNull(resultado); // Verifica se um administrador foi retornado
    Assert.AreEqual(adm.Email, resultado.Email); // Verifica se o email é o esperado
    Assert.AreEqual(adm.Senha, resultado.Senha); // Verifica se a senha é a esperada
    }

 [TestMethod]
public void TestandoTodos()
{
    // Arrange: Criar contexto de teste e limpar a tabela Administradores
    var context = CriarContextoDeTeste();
    context.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");

    // Adicionando 15 registros para garantir que temos mais de uma página
    for (int i = 1; i <= 15; i++)
    {
        context.Administradores.Add(new Administrador
        {
            Email = $"teste{i}@teste.com",
            Senha = "teste",
            Perfil = "Adm"
        });
    }
    context.SaveChanges();

    // Verificar se o número de registros no banco é o esperado (15 registros)
    var totalRegistros = context.Administradores.Count();
    Assert.AreEqual(15, totalRegistros, "Número total de registros no banco está incorreto.");

    // Instanciando o serviço que implementa a lógica de paginação
    var administradorServico = new AdminstradorServico(context);

    // Act: Testando a primeira página (deve retornar 10 administradores)
    var pagina1 = administradorServico.Todos(1);
    Assert.AreEqual(10, pagina1.Count, "Página 1 não tem 10 itens como esperado.");
    Assert.AreEqual("teste1@teste.com", pagina1.First().Email, "Primeiro item na página 1 está incorreto.");
    Assert.AreEqual("teste10@teste.com", pagina1.Last().Email, "Último item na página 1 está incorreto.");

    // Act: Testando a segunda página (deve retornar 5 administradores)
    var pagina2 = administradorServico.Todos(2);
    Assert.AreEqual(5, pagina2.Count, "Página 2 não tem 5 itens como esperado.");
    Assert.AreEqual("teste11@teste.com", pagina2.First().Email, "Primeiro item na página 2 está incorreto.");
    Assert.AreEqual("teste15@teste.com", pagina2.Last().Email, "Último item na página 2 está incorreto.");
}


}