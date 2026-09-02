using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Pizzeria.Cocina
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("=== SERVICIO DE COCINA Y REPARTO ===");

            TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 5000);
            server.Start();
            Console.WriteLine("Cocina escuchando en el puerto 5000...");

            while (true)
            {
                using TcpClient client = await server.AcceptTcpClientAsync();
                using NetworkStream stream = client.GetStream();

                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string pedidoJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Console.WriteLine($"\n[NUEVO PEDIDO RECIBIDO VÍA SOCKET]");
                Console.WriteLine($"Datos: {pedidoJson}");
                Console.WriteLine("Estado actualizado: En preparación...");
            }
        }
    }
}