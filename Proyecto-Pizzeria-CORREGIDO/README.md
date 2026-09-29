# Proyecto Pizzería Distribuida

Proyecto educativo en C#/.NET 8 para simular el pedido y la entrega de una pizza mediante una arquitectura distribuida.

## Arquitectura

- **Pizzeria.Cliente:** aplicación de consola que consume la API REST.
- **Pizzeria.API:** backend con Minimal API. Registra pedidos y los envía a cocina mediante TCP sockets.
- **Pizzeria.Cocina:** servicio independiente que escucha pedidos por TCP y responde con una confirmación.
- **Pizzeria.Modelos:** biblioteca compartida con `Pizza`, `Cliente`, `Pedido` y `EstadoPedido`.

## Tecnologías

- C# / .NET 8
- Minimal APIs
- HTTP/REST
- Swagger/OpenAPI
- TCP sockets
- `async/await`
- Manejo de excepciones

## Cómo ejecutar

Abrir la solución `PizzeriaDistribuida.slnx` en Visual Studio.

Ejecutar primero **Pizzeria.Cocina** y después **Pizzeria.API**. Finalmente ejecutar **Pizzeria.Cliente**.

La API queda disponible en:

`http://localhost:5029`

Swagger:

`http://localhost:5029/swagger`

La cocina escucha en:

`127.0.0.1:5000`

## Flujo

1. El cliente consulta el menú mediante `GET /api/pizzas`.
2. El cliente envía el pedido mediante `POST /api/pedidos`.
3. La API registra el pedido con estado `EsperaDeConfirmacion`.
4. La API se comunica con Cocina mediante un socket TCP.
5. Cocina recibe el pedido, lo procesa y responde `OK`.
6. Si la comunicación fue correcta, la API cambia el pedido a `EnPreparacion`.
7. Si Cocina no está disponible, el pedido queda registrado y la API informa el problema.

## Estados del pedido

- `EsperaDeConfirmacion`
- `EnPreparacion`
- `EnViaje`
- `Entregado`

## Manejo de errores

Se contemplan errores de validación, API no disponible, timeout, cocina desconectada, errores de socket, errores de lectura/escritura y JSON inválido.
