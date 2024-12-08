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
            .WithName("GetSalesOrders");
        
        group.MapGet("/beers", HandleGetBeers)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status200OK)
            .WithName("GetSalesBeers");

        return endpoints;
    }
    
    private static async Task<IResult> HandleGetOrders(IHttpClientFactory httpClientFactory,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var client = httpClientFactory.CreateClient("BrewUpOrders");
            
        var response = await client.GetAsync(
            "v1/brewup/orders", cancellationToken);

        return response.IsSuccessStatusCode 
            ? Results.Ok(await response.Content.ReadAsStringAsync(cancellationToken))
            : Results.StatusCode(500);
    }
    
    private static async Task<IResult> HandleGetBeers(IHttpClientFactory httpClientFactory,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var client = httpClientFactory.CreateClient("BrewUpBeers");
            
        var response = await client.GetAsync(
            "v1/brewup/beers", cancellationToken);

        return response.IsSuccessStatusCode 
            ? Results.Ok(await response.Content.ReadAsStringAsync(cancellationToken))
            : Results.StatusCode(500);
    }
}