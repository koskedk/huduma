namespace Huduma.Contracts
{
    public interface PaymentReceived
    {
        Guid BillNo { get; }
        double Amount { get; }
    }
}