namespace Huduma.Contracts
{
     public interface ClientBilled
     {
          Guid BillNo { get;  }
          string Client { get; }
          double Amount { get; }
     }
}