using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingGarageManager
{
    public class Visit
    {
        public string ID { get; private set; }
        public DateTime EntryTime { get; private set; }
        public DateTime? LeaveTime { get; private set; }
        public string? TotalHours { get; private set; }
        public decimal Rate { get; private set; }
        public Payment? Payment { get; private set; }

        public Visit(string id, DateTime entryTime, decimal rate, Payment? payment = null, DateTime? leaveTime = null, string? totalHours = null)
        {
            this.ID = id;
            this.EntryTime = entryTime;
            this.LeaveTime = leaveTime;
            this.Rate = rate;
            this.Payment = payment;
            this.TotalHours = totalHours;
        }

        public PendingPayment? EndVisit(string paymentID, bool isSubscriber)
        {
            DateTime rawDateTime = DateTime.Now;
            DateTime currentDateTime = new DateTime(rawDateTime.Year, rawDateTime.Month, rawDateTime.Day, rawDateTime.Hour, rawDateTime.Minute, rawDateTime.Second);
            this.LeaveTime = currentDateTime;

            DateTime chargedDate = currentDateTime;

            if (isSubscriber)
            {
                DateTime paidDate = currentDateTime;
                this.Payment = new CompletedPayment(paymentID, chargedDate, paidDate, amountPaid: 0m);
                return null;
            }
            else
            {
                double totalSeconds = (this.LeaveTime.Value - this.EntryTime).TotalSeconds;
                double roundedHours = Math.Ceiling(totalSeconds / 3600.0);
                this.TotalHours = roundedHours.ToString();
                decimal amountOwed = (decimal) roundedHours * this.Rate;

                PendingPayment pendingPayment = new PendingPayment(paymentID, chargedDate, amountOwed);
                this.Payment = pendingPayment;

                return pendingPayment;
            }
        }

        public void UpdatePayment(Payment payment)
        {
            this.Payment = payment;
        }

        public string ToFileString()
        {
           return $"{ID},{EntryTime:yyyy-MM-dd HH:mm:ss},{LeaveTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "null"},{this.TotalHours ?? "null"},{this.Rate},{this.Payment?.ID ?? "null"}"; 
        }
    }
}