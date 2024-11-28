
using minimal_api.Dominio.Entidades;

namespace Test.Domain.Entidades;

[TestClass]
public class VeiculoTeste
{
    [TestMethod]
    public void TestarGetSetPropriedades()
    {
        //Arange
        var veiculo = new Veiculo();


        //Act
        veiculo.Id = 1;
        veiculo.Nome = "Ractis";
        veiculo.Marca = "Toyota";
        veiculo.Ano = 2010;    


        //Assert
        Assert.AreEqual(1, veiculo.Id);
        Assert.AreEqual("Ractis", veiculo.Nome);
        Assert.AreEqual("Toyota", veiculo.Marca);
        Assert.AreEqual( 2010, veiculo.Ano);
    }
}