using Pizzeria.Modelos;

namespace Pizzeria.Cliente.Models;

public sealed class PedidoCreadoResponse
{
    public Pedido? Pedido { get; init; }
    public bool CocinaConectada { get; init; }
    public string? Mensaje { get; init; }
}
