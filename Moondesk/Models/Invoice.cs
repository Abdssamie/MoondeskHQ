namespace AquaPP.Models;

public class Invoice
{
    public int Id { get; set; }
    public string? BillingName { get; set; }
    public int AmountPaid { get; set; }
    public bool Paid { get; set; }

    public Invoice(int id, string? billingName, int amountPaid, bool paid)
    {
        Id = id;
        BillingName = billingName;
        AmountPaid = amountPaid;
        Paid = paid;
    }
}
