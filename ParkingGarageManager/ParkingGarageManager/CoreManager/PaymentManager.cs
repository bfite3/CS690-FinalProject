using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingGarageManager
{
    public class PaymentManager
    {
        public Core Core { get; private set; }

        public PaymentManager(Core core)
        {
            this.Core = core;
        }

        public PaymentResult ProcessPayment(PendingPayment payment, decimal amountPaid)
        {
            PaymentResult paymentResult = payment.ProcessPayment(amountPaid);
            if (paymentResult.IsSuccess)
            {
                DateTime paidDate = DateTime.Now;
                CompletedPayment newCompletedPayment = new CompletedPayment(payment.ID, payment.ChargedDate, paidDate, amountPaid: payment.AmountOwed);

                this.Core.Vehicles.Values.SelectMany(vehicle => vehicle.Visits.Values)
                    .FirstOrDefault(visit => visit.Payment?.ID == payment.ID)?.UpdatePayment(newCompletedPayment);
            }

            return paymentResult;
        }

        public void RefundPayments(List<PendingPayment> pendingPayments)
        {
            pendingPayments.ForEach(p => p.Refund());
        }
    }
}