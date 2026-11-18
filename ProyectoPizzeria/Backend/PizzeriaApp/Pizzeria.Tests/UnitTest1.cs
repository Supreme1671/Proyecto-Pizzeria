using Pizzeria.API.Services;
using Pizzeria.Modelos;

namespace Pizzeria.Tests;

public class PedidoValidatorTests
{
    private readonly List<Pizza> menu =
    [
        new() { Id = 1, Nombre = "Muzzarella", Precio = 8000m }
    ];

    [Fact]
    public void PedidoValido_NoDevuelveError()
    {
        var pedido = PedidoValido();

        var error = new PedidoValidator().Validar(pedido, menu);

        Assert.Null(error);
    }

    [Fact]
    public void PedidoSinCliente_DebeDevolverError()
    {
        var pedido = PedidoValido();
        pedido.Cliente.Nombre = " ";

        var error = new PedidoValidator().Validar(pedido, menu);

        Assert.Equal("El pedido debe incluir el nombre del cliente.", error);
    }

    [Fact]
    public void PedidoSinPizzas_DebeDevolverError()
    {
        var pedido = PedidoValido();
        pedido.Pizzas.Clear();

        var error = new PedidoValidator().Validar(pedido, menu);

        Assert.Equal("El pedido debe contener al menos una pizza.", error);
    }

    [Fact]
    public void PizzaInexistente_DebeDevolverError()
    {
        var pedido = PedidoValido();
        pedido.Pizzas[0].Id = 999;

        var error = new PedidoValidator().Validar(pedido, menu);

        Assert.Equal("El pedido contiene una pizza que no existe en el menú.", error);
    }

    [Fact]
    public void PedidoNuevo_DebeIniciarEnEsperaDeConfirmacion()
    {
        var pedido = new Pedido();

        Assert.Equal(EstadoPedido.EsperaDeConfirmacion, pedido.Estado);
    }

    private static Pedido PedidoValido() => new()
    {
        Cliente = new Pizzeria.Modelos.Cliente { Nombre = "Ana" },
        Pizzas = [new Pizza { Id = 1 }]
    };
}
