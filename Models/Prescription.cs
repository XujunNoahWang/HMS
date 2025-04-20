namespace HMS.Models;

// To represent a prescription
public class Prescription
{
    public long Id { get; set; }
    public long VisitId { get; set; }
    public string Type { get; set; }
    public string Name { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}