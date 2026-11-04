using System.Net.Http.Json;
using System.Text.Json;
using Pizzeria.Cliente.Models;
using Pizzeria.Modelos;

namespace Pizzeria.Cliente.Services;

public sealed class PizzeriaApiClient(HttpClient http)
{
    public async Task<IReadOnlyList<Pizza>> ObtenerMenuAsync(CancellationToken cancellationToken = default)
    {
        using var response = await http.GetAsync("/api/pizzas", cancellationToken);
        await AsegurarRespuestaExitosaAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<List<Pizza>>(cancellationToken: cancellationToken)
            ?? throw new JsonException("La API devolvió un menú vacío o inválido.");
    }

    public async Task<PedidoCreadoResponse> CrearPedidoAsync(
        Pedido pedido,
        CancellationToken cancellationToken = default)
    {
        using var response = await http.PostAsJsonAsync("/api/pedidos", pedido, cancellationToken);
        await AsegurarRespuestaExitosaAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<PedidoCreadoResponse>(cancellationToken: cancellationToken)
            ?? throw new JsonException("La API devolvió una confirmación vacía o inválida.");
    }

    private static async Task AsegurarRespuestaExitosaAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        var detalle = await response.Content.ReadAsStringAsync(cancellationToken);
        var mensaje = string.IsNullOrWhiteSpace(detalle)
            ? $"La API respondió {(int)response.StatusCode} ({response.StatusCode})."
            : $"La API respondió {(int)response.StatusCode} ({response.StatusCode}): {detalle}";

        throw new HttpRequestException(mensaje, null, response.StatusCode);
    }
}
