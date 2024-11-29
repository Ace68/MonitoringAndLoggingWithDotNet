namespace LoggingAndMonitoringWithDotNet.Contracts;

public class SalesOrder
{
    public string OrderId { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    
    public IEnumerable<Beer> Rows { get; set; } = [];
}