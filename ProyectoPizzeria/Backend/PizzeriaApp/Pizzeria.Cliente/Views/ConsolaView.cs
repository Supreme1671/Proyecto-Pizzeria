using Pizzeria.Cliente.Models;
using Pizzeria.Modelos;

namespace Pizzeria.Cliente.Views;

public sealed class ConsolaView
{
    public void MostrarEncabezado()
    {
        Console.WriteLine("=== CLIENTE DE PIZZERÍA ===");
        Console.WriteLine("Consultando menú...\n");
    }

    public void MostrarMenu(IReadOnlyCollection<Pizza> menu)
    {
        foreach (var pizza in menu)
            Console.WriteLine($"{pizza.Id}. {pizza.Nombre} - ${pizza.Precio:N2}");
    }

    public string? SolicitarIdPizza()
    {
        Console.Write("\nIngrese el ID de la pizza: ");
        return Console.ReadLine();
    }

    public string? SolicitarDato(string etiqueta)
    {
        Console.Write($"{etiqueta}: ");
        return Console.ReadLine()?.Trim();
    }

    public void MostrarError(string mensaje)
    {
        Console.WriteLine(mensaje);
    }

    public void MostrarPedidoCreado(PedidoCreadoResponse resultado)
    {
        Console.WriteLine();
        if (resultado.Pedido is not { } pedido)
            throw new InvalidDataException("La confirmación de la API no contiene el pedido.");
        if (string.IsNullOrWhiteSpace(resultado.Mensaje))
            throw new InvalidDataException("La confirmación de la API no contiene un mensaje.");

        Console.WriteLine(resultado.Mensaje);
        Console.WriteLine($"Pedido #{pedido.Id} - Estado: {pedido.Estado}");
        Console.WriteLine($"Total: ${pedido.Pizzas.Sum(pizza => pizza.Precio):N2}");
    }
}
