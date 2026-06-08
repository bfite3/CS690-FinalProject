using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingGarageManager
{
    public abstract class Payment
    {
        public string ID { get; private set; }
        public DateTime ChargedDate { get; private set; }
        public decimal AmountOwed { get; private set; }
        public decimal AmountPaid { get; protected set; }
        public abstract string Status { get; }

        public Payment(string id, DateTime chargedDate, decimal amountOwed, decimal amountPaid)
        {
            this.ID = id;
            this.ChargedDate = chargedDate;
            this.AmountOwed = amountOwed;
            this.AmountPaid = amountPaid;
        }

        public abstract string ToFileString();
    }
}