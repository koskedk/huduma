using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;

namespace Huduma.Billing.Domain
{
    public class Money:ValueObject
    {
        public double Value { get; private set; }
        public string Currency { get;  private set;}

        private Money(){}
        private Money(double value, string currency="KES")
        {
            Value = value;
            Currency = currency.ToUpper();
        }

        public static Money FromKes(double val)
        {
            if (val < 0)
                throw new ArgumentException("Postive values only");
            
            var cash=new Money(val, "KES");
            return cash;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
            yield return Currency;
        }

        public override string ToString()
        {
            return $"{Value} {Currency}";
        }
    }
}