using Pizzeria.Cliente.Controllers;
using Pizzeria.Cliente.Models;
using Pizzeria.Cliente.Services;
using Pizzeria.Cliente.Views;

const string apiUrl = "http://localhost:5029";

using var http = new HttpClient
{
    BaseAddress = new Uri(apiUrl),
    Timeout = TimeSpan.FromSeconds(10)
};

var controller = new PedidoController(
    new PizzeriaApiClient(http),
    new ConsolaView());

await controller.RealizarPedidoAsync();
