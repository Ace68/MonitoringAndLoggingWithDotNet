namespace LoggingAndMonitoringWithDotNet.Contracts;

public class Beer
{
    public string BeerId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Quantity Quantity { get; set; } = default!;
    public Price Price { get; set; } = default!;
}