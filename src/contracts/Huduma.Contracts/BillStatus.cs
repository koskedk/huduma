namespace Huduma.Contracts;

public interface BillStatus
{
    Guid BillNo { get;  }
    string Client { get; }
    double Amount { get; }
}