using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingGarageManager
{
    public class PendingPayment : Payment
    {
        public override string Status => "Pending";

        public PendingPayment(string id, DateTime chargedDate, decimal amountOwed, decimal amountPaid = 0m)
            : base(id, chargedDate, amountOwed, amountPaid)
        {
            
        }

        public override string ToFileString()
        {
            return $"{this.ID},{this.ChargedDate},null,{this.AmountOwed},{this.AmountPaid},{this.Status}";
        }

        public PaymentResult ProcessPayment(decimal amountPaid)
        {
            if (amountPaid < 0)
                return new PaymentResult(isSuccess: false, message: "Invalid payment amount. Amount must be positive.");

            this.AmountPaid += amountPaid;

            if (this.AmountPaid < this.AmountOwed)
                return new PaymentResult(isSuccess: false, message: "There is still a remaining balance.");

            if (this.AmountPaid > this.AmountOwed)
            {
                decimal changeDue = this.AmountPaid - this.AmountOwed;
                return new PaymentResult(isSuccess: true, changeDue: changeDue, message: "Payment complete. Change is due.");
            }

            return new PaymentResult(isSuccess: true, changeDue: 0, message: "Payment complete.");
        }

        public void Refund()
        {
            this.AmountPaid = 0m;
        }
    }
}