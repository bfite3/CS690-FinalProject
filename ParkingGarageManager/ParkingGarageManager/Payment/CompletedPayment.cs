using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingGarageManager
{
    public class CompletedPayment : Payment
    {
        public override string Status => "Completed";
        public DateTime PaidDate { get; private set; }

        public CompletedPayment(string id, DateTime chargedDate, DateTime paidDate, decimal amountPaid, decimal amountOwed = 0m)
            : base(id, chargedDate, amountOwed, amountPaid)
        {
            this.PaidDate = paidDate;
        }

        public override string ToFileString()
        {
            return $"{this.ID},{this.ChargedDate},{this.PaidDate},{this.AmountOwed},{this.AmountPaid},{this.Status}";
        }
    }
}