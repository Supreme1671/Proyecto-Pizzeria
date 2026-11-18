using Pizzeria.Modelos;

namespace Pizzeria.API.Services;

public sealed class PedidoValidator
{
    public string? Validar(Pedido? pedido, IReadOnlyCollection<Pizza> menu)
    {
        if (pedido is null)
            return "El cuerpo del pedido es obligatorio.";

        if (pedido.Cliente is null || string.IsNullOrWhiteSpace(pedido.Cliente.Nombre))
            return "El pedido debe incluir el nombre del cliente.";

        if (pedido.Pizzas is null || pedido.Pizzas.Count == 0)
            return "El pedido debe contener al menos una pizza.";

        if (pedido.Pizzas.Any(pizza => menu.All(opcion => opcion.Id != pizza.Id)))
            return "El pedido contiene una pizza que no existe en el menú.";

        return null;
    }
}
