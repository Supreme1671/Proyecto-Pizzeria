using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Pizzeria.Modelos;

namespace Pizzeria.Tests;

public sealed class ApiFunctionalTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient cliente;

    public ApiFunctionalTests(WebApplicationFactory<Program> factory)
    {
        cliente = factory.CreateClient();
    }

    [Fact]
    public async Task GetMenu_DebeDevolverPizzas()
    {
        var pizzas = await cliente.GetFromJsonAsync<List<Pizza>>("/api/pizzas");

        Assert.NotNull(pizzas);
        Assert.NotEmpty(pizzas);
    }

    [Fact]
    public async Task PostPedidoSinCliente_DebeResponderBadRequest()
    {
        var pedido = new Pedido
        {
            Cliente = new Pizzeria.Modelos.Cliente(),
            Pizzas = [new Pizza { Id = 1 }]
        };

        var respuesta = await cliente.PostAsJsonAsync("/api/pedidos", pedido);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task PostPedidoValido_DebeRegistrarPedido()
    {
        var pedido = new Pedido
        {
            Cliente = new Pizzeria.Modelos.Cliente
            {
                Nombre = "Prueba funcional",
                Direccion = "Calle 1",
                Telefono = "123"
            },
            Pizzas = [new Pizza { Id = 1 }]
        };

        var respuesta = await cliente.PostAsJsonAsync("/api/pedidos", pedido);

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<PedidoCreadoRespuesta>();
        Assert.NotNull(cuerpo?.Pedido);
        Assert.True(cuerpo.Pedido.Id > 0);
    }

    private sealed class PedidoCreadoRespuesta
    {
        public Pedido? Pedido { get; set; }
        public bool CocinaConectada { get; set; }
        public string? Mensaje { get; set; }
    }
}
