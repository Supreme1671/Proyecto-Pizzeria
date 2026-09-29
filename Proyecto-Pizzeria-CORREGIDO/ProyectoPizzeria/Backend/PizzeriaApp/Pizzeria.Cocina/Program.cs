using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Pizzeria.Modelos;

namespace Pizzeria.Cocina;

internal static class Program
{
    private const int Puerto = 5000;

    private static async Task Main()
    {
        Console.WriteLine("=== SERVICIO DE COCINA ===");
        Console.WriteLine($"Escuchando en 127.0.0.1:{Puerto}...");

        var servidor = new TcpListener(IPAddress.Loopback, Puerto);
        servidor.Start();

        try
        {
            while (true)
            {
                var cliente = await servidor.AcceptTcpClientAsync();
                _ = AtenderClienteAsync(cliente);
            }
        }
        catch (Exception ex) when (ex is SocketException or ObjectDisposedException)
        {
            Console.WriteLine($"Error del servidor de cocina: {ex.Message}");
        }
        finally
        {
            servidor.Stop();
        }
    }

    private static async Task AtenderClienteAsync(TcpClient cliente)
    {
        using (cliente)
        {
            try
            {
                await using var stream = cliente.GetStream();
                using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
                using var writer = new StreamWriter(stream, new UTF8Encoding(false), leaveOpen: true)
                {
                    AutoFlush = true
                };

                var pedidoJson = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(pedidoJson))
                {
                    await writer.WriteLineAsync("ERROR");
                    return;
                }

                var pedido = JsonSerializer.Deserialize<Pedido>(pedidoJson);
                if (pedido is null)
                {
                    await writer.WriteLineAsync("ERROR");
                    return;
                }

                Console.WriteLine();
                Console.WriteLine("[NUEVO PEDIDO RECIBIDO VÍA SOCKET]");
                Console.WriteLine($"Pedido: #{pedido.Id}");
                Console.WriteLine($"Cliente: {pedido.Cliente.Nombre}");
                Console.WriteLine($"Pizzas: {string.Join(", ", pedido.Pizzas.Select(p => p.Nombre))}");
                Console.WriteLine("Estado: En preparación");

                await writer.WriteLineAsync("OK");
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"[Cocina] JSON inválido: {ex.Message}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"[Cocina] Error de comunicación: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Cocina] Error inesperado: {ex.Message}");
            }
        }
    }
}
