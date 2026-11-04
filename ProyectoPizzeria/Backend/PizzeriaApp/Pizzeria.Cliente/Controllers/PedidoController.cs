using System.Text.Json;
using Pizzeria.Cliente.Services;
using Pizzeria.Cliente.Views;
using Pizzeria.Modelos;

namespace Pizzeria.Cliente.Controllers;

public sealed class PedidoController(PizzeriaApiClient api, ConsolaView vista)
{
    public async Task RealizarPedidoAsync(CancellationToken cancellationToken = default)
    {
        vista.MostrarEncabezado();

        try
        {
            var menu = await api.ObtenerMenuAsync(cancellationToken);
            if (menu.Count == 0)
            {
                vista.MostrarError("No hay pizzas disponibles.");
                return;
            }

            vista.MostrarMenu(menu);
            if (!int.TryParse(vista.SolicitarIdPizza(), out var pizzaId))
            {
                vista.MostrarError("ID inválido.");
                return;
            }

            var pizzaSeleccionada = menu.FirstOrDefault(pizza => pizza.Id == pizzaId);
            if (pizzaSeleccionada is null)
            {
                vista.MostrarError("La pizza seleccionada no existe.");
                return;
            }

            var nombre = vista.SolicitarDato("Nombre del cliente");
            if (string.IsNullOrWhiteSpace(nombre))
            {
                vista.MostrarError("El nombre es obligatorio.");
                return;
            }

            var pedido = new Pedido
            {
                Cliente = new Pizzeria.Modelos.Cliente
                {
                    Nombre = nombre,
                    Direccion = vista.SolicitarDato("Dirección") ?? string.Empty,
                    Telefono = vista.SolicitarDato("Teléfono") ?? string.Empty
                },
                Pizzas = [pizzaSeleccionada]
            };

            var resultado = await api.CrearPedidoAsync(pedido, cancellationToken);
            vista.MostrarPedidoCreado(resultado);
        }
        catch (HttpRequestException ex)
        {
            vista.MostrarError($"No se pudo completar la solicitud a la API: {ex.Message}");
            if (ex.StatusCode is null)
                vista.MostrarError("Verifique que la API esté ejecutándose en http://localhost:5029.");
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            vista.MostrarError("La solicitud tardó demasiado y fue cancelada.");
        }
        catch (JsonException ex)
        {
            vista.MostrarError($"La API devolvió una respuesta inválida: {ex.Message}");
        }
        catch (InvalidDataException ex)
        {
            vista.MostrarError($"No se pudo interpretar la respuesta de la API: {ex.Message}");
        }
    }
}
