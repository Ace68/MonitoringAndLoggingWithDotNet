using LoggingAndMonitoringWithDotNet.Contracts;

namespace LoggingAndMonitoringWithDotNet.Endpoints;

public static class BrewUpEndpoints
{
  public static IEndpointRouteBuilder MapBrewUpEndpoints(this IEndpointRouteBuilder endpoints)
  {
    var group = endpoints.MapGroup("/v1/brewup/")
        .WithTags("Sales");

    group.MapGet("/orders", HandleGetOrders)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status200OK)
        .WithName("GetOrders");

    group.MapGet("/beers", HandleGetBeers)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status200OK)
      .WithName("GetBeers");

    return endpoints;
  }

  private static Task<IResult> HandleGetOrders(
      ILoggerFactory loogerFactory,
      CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();

    var logger = loogerFactory.CreateLogger("brewup-orders");

    logger.LogWarning("Getting BrewUp Orders");
    var orders = new List<SalesOrder>
        {
            new()
            {
                OrderId = Guid.NewGuid().ToString(),
                OrderNumber = "2024131000",
                CustomerId = Guid.NewGuid().ToString(),
                CustomerName = "Il Grottino del Muflone",
                Rows = new List<Beer>
                {
                    new()
                    {
                        BeerId = Guid.NewGuid().ToString(),
                        Name = "Muflone IPA",
                        Quantity = new Quantity(20, "Lt"),
                        Price = new Price(5, "EUR")
                    },
                    new()
                    {
                        BeerId = Guid.NewGuid().ToString(),
                        Name = "Muflone Weiss",
                        Quantity = new Quantity(10, "Lt"),
                        Price = new Price(5, "EUR")
                    }
                }
            },
            new()
            {
                OrderId = Guid.NewGuid().ToString(),
                OrderNumber = "2024131100",
                CustomerId = Guid.NewGuid().ToString(),
                CustomerName = "La Tana del Muflone",
                Rows = new List<Beer>
                {
                    new()
                    {
                        BeerId = Guid.NewGuid().ToString(),
                        Name = "Muflone IPA",
                        Quantity = new Quantity(20, "Lt"),
                        Price = new Price(5, "EUR")
                    },
                    new()
                    {
                        BeerId = Guid.NewGuid().ToString(),
                        Name = "Muflone Weiss",
                        Quantity = new Quantity(10, "Lt"),
                        Price = new Price(5, "EUR")
                    }
                }
            }
        };

    return Task.FromResult(Results.Ok(orders));
  }

  private static Task<IResult> HandleGetBeers(
      ILoggerFactory loogerFactory,
    CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();

    var logger = loogerFactory.CreateLogger("brewup-beers");
    
    logger.LogWarning("Getting BrewUp Beers");
    
    var beers = new List<Beer>
      {
        new()
        {
          BeerId = Guid.NewGuid().ToString(),
          Name = "Muflone IPA",
          Quantity = new Quantity(20, "Lt"),
          Price = new Price(5, "EUR")
        },
        new()
        {
          BeerId = Guid.NewGuid().ToString(),
          Name = "Muflone Weiss",
          Quantity = new Quantity(10, "Lt"),
          Price = new Price(5, "EUR")
        }
      };

    return Task.FromResult(Results.Ok(beers));
  }
}