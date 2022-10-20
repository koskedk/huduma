namespace Huduma.Billing.Domain
{
    public class HudumaOptions
    {
        #region TransportBus
    
        public const string BusKey="TransportBus";
        public  string BusHost {get;set;}
        public  string BusVhost {get;set;}
        public  string BusUser {get;set;}
        public  string BusPass {get;set;}
    
        #endregion
    }
}