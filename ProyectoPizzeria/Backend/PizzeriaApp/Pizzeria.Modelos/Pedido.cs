using Pizzeria.Modelos;

namespace Backend.PizzeriaApp.Pizzeria.Modelos;

public class Pedido
{
    public int Id { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public List<Pizza> Pizzas { get; set; } = new();
    public EstadoPedido Estado { get; set; } = EstadoPedido.EsperaDeConfirmacion; //[cite: 1]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}
