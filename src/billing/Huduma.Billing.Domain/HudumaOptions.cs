namespace Huduma.Billing.Domain
{
    public class HudumaTransportOptions
    {
        #region TransportBus
    
        public const string Key="TransportBus";
        public  string Host {get;set;}
        public  string Vhost {get;set;}
        public  string User {get;set;}
        public  string Pass {get;set;}
    
        #endregion
    }
}