namespace Pizzeria.Modelos;

public class Pedido
{
    public int Id { get; set; }
    public Cliente Cliente { get; set; } = new();
    public List<Pizza> Pizzas { get; set; } = new();
    public EstadoPedido Estado { get; set; } = EstadoPedido.EsperaDeConfirmacion;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}
