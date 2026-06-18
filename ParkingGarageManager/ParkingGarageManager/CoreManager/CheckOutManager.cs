using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingGarageManager
{
    public class CheckOutManager
    {
        public Core Core { get; private set; }

        public CheckOutManager(Core core)
        {
            this.Core = core;
        }

        public (List<PendingPayment>, List<Visit>, string spotID, Subscriber? subscriber) CheckOutSpot(string licensePlateNumber)
        {
            Vehicle vehicle = this.Core.VehicleManager.FindVehicle(licensePlateNumber);

            Spot? spot = this.Core.VehicleManager.FindSpotByVehicle(vehicle);
                    
            if (spot == null)
                throw new KeyNotFoundException($"No spot found for license plate: {licensePlateNumber}.");

            string spotID = spot.SpotID;

            Subscriber? subscriber = this.Core.SubscriberManager.FindSubscriber(licensePlateNumber: licensePlateNumber);

            bool isSubscriber = subscriber is ActiveSubscriber;

            (List<PendingPayment> pendingPayments, List<Visit> pendingVisits) = this.ProcessVisit(vehicle, isSubscriber);       

            return (pendingPayments, pendingVisits, spotID, subscriber);
        }

        public (List<PendingPayment>, List<Visit>) ProcessVisit(Vehicle vehicle, bool isSubscriber)
        {
            string maxPaymentID = this.Core.Vehicles.Values
                .SelectMany(vehicle => vehicle.Visits.Values)
                .Select(visit => visit.Payment)
                .Where(payment => payment != null)
                .Select(payment => int.Parse(payment!.ID))
                .DefaultIfEmpty(0)
                .Max()
                .ToString();

            int paymentIDCounter = int.Parse(maxPaymentID) + 1;

            List<Visit> pendingVisits = vehicle.Visits.Values
                .Where(v => v.Payment == null || v.Payment.AmountOwed > v.Payment.AmountPaid).ToList();

            List<Visit> pendingVisitsToReturn = new List<Visit>();
            List<PendingPayment>? pendingPayments = new List<PendingPayment>();
            foreach (Visit v in pendingVisits)
            {
                PendingPayment? payment = v.EndVisit(paymentIDCounter.ToString(), isSubscriber);
                if (payment != null)
                {
                    pendingPayments.Add(payment);
                    pendingVisitsToReturn.Add(v);
                }

                paymentIDCounter++;
            }

            return (pendingPayments, pendingVisitsToReturn);
        }
    }
}