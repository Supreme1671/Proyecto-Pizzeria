using Backend.PizzeriaApp.Pizzeria.Modelos;
using Pizzeria.Modelos;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Pizzeria.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            // Base de datos temporal en memoria
            List<Pedido> pedidos = new();
            List<Pizza> menu = new()
            {
                new Pizza { Id = 1, Nombre = "Muzzarella", Precio = 8000 },
                new Pizza { Id = 2, Nombre = "Fugazzeta", Precio = 9500 },
                new Pizza { Id = 3, Nombre = "Napolitana", Precio = 9000 }
            };

            // 1. Endpoint para ver el menú de pizzas
            app.MapGet("/api/pizzas", () => Results.Ok(menu));

            // 2. Endpoint para ver los pedidos registrados
            app.MapGet("/api/pedidos", () => Results.Ok(pedidos));

            // 3. Endpoint para realizar un pedido nuevo
            app.MapPost("/api/pedidos", async (Pedido nuevoPedido) =>
            {
                nuevoPedido.Id = pedidos.Count + 1;
                nuevoPedido.Estado = EstadoPedido.EsperaDeConfirmacion; // Estado inicial
                pedidos.Add(nuevoPedido);

                // Intentar notificar a la Cocina mediante Sockets
                _ = Task.Run(() => NotificarCocinaPorSocket(nuevoPedido));

                return Results.Created($"/api/pedidos/{nuevoPedido.Id}", nuevoPedido);
            });

            await app.RunAsync();
        }

        // Función auxiliar para enviar el pedido a la Cocina usando Sockets TCP
        static async Task NotificarCocinaPorSocket(Pedido pedido)
        {
            try
            {
                using TcpClient client = new TcpClient();
                // Conectar al puerto donde escuchará la cocina
                await client.ConnectAsync("127.0.0.1", 5000);

                using NetworkStream stream = client.GetStream();
                string mensajeJson = JsonSerializer.Serialize(pedido);
                byte[] buffer = Encoding.UTF8.GetBytes(mensajeJson);

                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error Socket API] No se pudo notificar a la cocina: {ex.Message}");
            }
        }
    }
}