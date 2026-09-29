using System.Net.Http.Json;
using Pizzeria.Modelos;

namespace Pizzeria.Cliente;

internal static class Program
{
    private const string ApiUrl = "http://localhost:5029";

    private static async Task Main()
    {
        using var http = new HttpClient
        {
            BaseAddress = new Uri(ApiUrl),
            Timeout = TimeSpan.FromSeconds(10)
        };

        Console.WriteLine("=== CLIENTE DE PIZZERÍA ===");
        Console.WriteLine("Consultando menú...\n");

        try
        {
            var menu = await http.GetFromJsonAsync<List<Pizza>>("/api/pizzas");
            if (menu is null || menu.Count == 0)
            {
                Console.WriteLine("No hay pizzas disponibles.");
                return;
            }

            foreach (var pizza in menu)
                Console.WriteLine($"{pizza.Id}. {pizza.Nombre} - ${pizza.Precio:N2}");

            Console.Write("\nIngrese el ID de la pizza: ");
            if (!int.TryParse(Console.ReadLine(), out var pizzaId))
            {
                Console.WriteLine("ID inválido.");
                return;
            }

            var pizzaSeleccionada = menu.FirstOrDefault(p => p.Id == pizzaId);
            if (pizzaSeleccionada is null)
            {
                Console.WriteLine("La pizza seleccionada no existe.");
                return;
            }

            Console.Write("Nombre del cliente: ");
            var nombre = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(nombre))
            {
                Console.WriteLine("El nombre es obligatorio.");
                return;
            }

            Console.Write("Dirección: ");
            var direccion = Console.ReadLine()?.Trim() ?? string.Empty;

            Console.Write("Teléfono: ");
            var telefono = Console.ReadLine()?.Trim() ?? string.Empty;

            var pedido = new Pedido
            {
                Cliente = new Pizzeria.Modelos.Cliente
                {
                    Nombre = nombre,
                    Direccion = direccion,
                    Telefono = telefono
                },
                Pizzas = new List<Pizza> { pizzaSeleccionada }
            };

            var response = await http.PostAsJsonAsync("/api/pedidos", pedido);
            var contenido = await response.Content.ReadAsStringAsync();

            Console.WriteLine();
            Console.WriteLine(response.IsSuccessStatusCode
                ? "Pedido enviado correctamente."
                : "No se pudo realizar el pedido.");
            Console.WriteLine(contenido);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"No se pudo conectar con la API: {ex.Message}");
            Console.WriteLine("Verifique que la API esté ejecutándose en http://localhost:5029.");
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("La solicitud tardó demasiado y fue cancelada.");
        }
    }
}
