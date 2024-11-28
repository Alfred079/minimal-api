

using System.Net;
using System.Text;
using System.Text.Json;
using Api.Test.Helpers;
using Microsoft.AspNetCore.WebUtilities;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.ModelViews;
using MinimalApi.DTOs;
using Test.Mocks;

namespace Test.Resquests;

[TestClass]
public class AdministradorResquestTest
{

    [ClassInitialize]
    public static void ClassInit(TestContext testContext)
    {
        Setup.ClassInit(testContext);
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        Setup.ClassCleanup();
    }

    [TestMethod]
    public async Task TestarGetSetPropriedades()
    {
        //Arange
        var loginDTO = new LoginDTO
        {
            Email = "adm@test.com",
            Senha = "123456"
        };

        var content = new StringContent(JsonSerializer.Serialize(loginDTO), Encoding.UTF8, "Application/json");

        //Act
        var reponse = await Setup.client.PostAsync("/administradores/login", content);

        //Assert
        Assert.AreEqual(HttpStatusCode.OK, reponse.StatusCode);

        var result = await reponse.Content.ReadAsStringAsync();
        var admLogado = JsonSerializer.Deserialize<AdministradorLogado>(result, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.IsNotNull(admLogado?.Email ?? "");
        Assert.IsNotNull(admLogado?.Perfil ?? "");
        Assert.IsNotNull(admLogado?.Token ?? "");

    }




[TestMethod]
public void TestInclusaoAdministrador()
{
    //Arrange
    var servicoMock = new AdministradorServicoMock();

        var novoAdm = new Administrador
        {
            Email = "novo@teste.com",
            Senha = "123456",
            Perfil = "Adm"
        };

    // Act
    var resultado = servicoMock.Incluir(novoAdm);

    // Assert

    Assert.IsNotNull(resultado);
    Assert.AreEqual(3, resultado?.Id);
    Assert.AreEqual("novo@teste.com", resultado?.Email);
    Assert.AreEqual("Adm", resultado?.Perfil);
}


}