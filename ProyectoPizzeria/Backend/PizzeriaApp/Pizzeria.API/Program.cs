using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Pizzeria.Modelos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var pedidos = new ConcurrentDictionary<int, Pedido>();
var menu = new List<Pizza>
{
    new() { Id = 1, Nombre = "Muzzarella", Precio = 8000m },
    new() { Id = 2, Nombre = "Fugazzeta", Precio = 9500m },
    new() { Id = 3, Nombre = "Napolitana", Precio = 9000m }
};

var siguienteId = 0;

app.MapGet("/api/pizzas", () => Results.Ok(menu))
    .WithName("ObtenerMenu")
    .WithOpenApi();

app.MapGet("/api/pedidos", () => Results.Ok(pedidos.Values.OrderBy(p => p.Id)))
    .WithName("ObtenerPedidos")
    .WithOpenApi();

app.MapGet("/api/pedidos/{id:int}", (int id) =>
{
    return pedidos.TryGetValue(id, out var pedido)
        ? Results.Ok(pedido)
        : Results.NotFound(new { mensaje = "No existe un pedido con ese ID." });
})
.WithName("ObtenerPedido")
.WithOpenApi();

app.MapPost("/api/pedidos", async (Pedido? nuevoPedido, CancellationToken cancellationToken) =>
{
    if (nuevoPedido is null)
        return Results.BadRequest(new { mensaje = "El cuerpo del pedido es obligatorio." });

    if (nuevoPedido.Cliente is null || string.IsNullOrWhiteSpace(nuevoPedido.Cliente.Nombre))
        return Results.BadRequest(new { mensaje = "El pedido debe incluir el nombre del cliente." });

    if (nuevoPedido.Pizzas is null || nuevoPedido.Pizzas.Count == 0)
        return Results.BadRequest(new { mensaje = "El pedido debe contener al menos una pizza." });

    var pizzasMenu = nuevoPedido.Pizzas
        .Select(p => menu.FirstOrDefault(m => m.Id == p.Id))
        .ToList();

    if (pizzasMenu.Any(p => p is null))
        return Results.BadRequest(new { mensaje = "El pedido contiene una pizza que no existe en el menú." });

    var pizzasValidas = pizzasMenu.Cast<Pizza>().ToList();

    nuevoPedido.Id = Interlocked.Increment(ref siguienteId);
    nuevoPedido.Pizzas = pizzasValidas;
    nuevoPedido.Estado = EstadoPedido.EsperaDeConfirmacion;
    nuevoPedido.FechaCreacion = DateTime.Now;

    pedidos[nuevoPedido.Id] = nuevoPedido;

    var cocinaRespondio = await NotificarCocinaPorSocketAsync(nuevoPedido, cancellationToken);
    if (cocinaRespondio)
    {
        nuevoPedido.Estado = EstadoPedido.EnPreparacion;
        pedidos[nuevoPedido.Id] = nuevoPedido;
    }

    return Results.Created($"/api/pedidos/{nuevoPedido.Id}", new
    {
        pedido = nuevoPedido,
        cocinaConectada = cocinaRespondio,
        mensaje = cocinaRespondio
            ? "Pedido recibido y enviado a cocina."
            : "Pedido registrado, pero no se pudo contactar a cocina."
    });
})
.WithName("CrearPedido")
.WithOpenApi();

app.MapPatch("/api/pedidos/{id:int}/estado", (int id, EstadoPedido estado) =>
{
    if (!pedidos.TryGetValue(id, out var pedido))
        return Results.NotFound(new { mensaje = "No existe un pedido con ese ID." });

    pedido.Estado = estado;
    return Results.Ok(pedido);
})
.WithName("ActualizarEstadoPedido")
.WithOpenApi();

app.Run();

static async Task<bool> NotificarCocinaPorSocketAsync(Pedido pedido, CancellationToken cancellationToken)
{
    try
    {
        using var client = new TcpClient();
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(3));

        await client.ConnectAsync("127.0.0.1", 5000, timeout.Token);
        await using var stream = client.GetStream();
        using var writer = new StreamWriter(stream, new UTF8Encoding(false), leaveOpen: true)
        {
            AutoFlush = true
        };
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);

        var json = JsonSerializer.Serialize(pedido);
        await writer.WriteLineAsync(json);

        var respuesta = await reader.ReadLineAsync(timeout.Token);
        return string.Equals(respuesta, "OK", StringComparison.OrdinalIgnoreCase);
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("[Socket] Tiempo de espera agotado al contactar a cocina.");
        return false;
    }
    catch (SocketException ex)
    {
        Console.WriteLine($"[Socket] Cocina no disponible: {ex.Message}");
        return false;
    }
    catch (IOException ex)
    {
        Console.WriteLine($"[Socket] Error de comunicación: {ex.Message}");
        return false;
    }
}
