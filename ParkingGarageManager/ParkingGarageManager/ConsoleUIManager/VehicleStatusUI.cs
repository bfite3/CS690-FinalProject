using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingGarageManager.ConsoleUIManager
{
    public class VehicleStatusUI
    {
        public ConsoleUI ConsoleUI { get; private set; }

        public VehicleStatusUI(ConsoleUI consoleUI)
        {
            this.ConsoleUI = consoleUI;
        }

        public void CheckVehicleStatus()
        {
            do
            {
                Console.WriteLine("CHECK VEHICLE STATUS");
                string licensePlateNumber = this.ConsoleUI.Prompt("Enter license plate number or enter to go back:");

                if (licensePlateNumber.Equals(""))
                    return;

                try
                {
                    Vehicle vehicle = this.ConsoleUI.Core.VehicleManager.FindVehicle(licensePlateNumber);
                    Spot? spot = this.ConsoleUI.Core.VehicleManager.FindSpotByVehicle(vehicle);
                    Subscriber? subscriber = this.ConsoleUI.Core.SubscriberManager.FindSubscriber(licensePlateNumber);

                    this.DisplayVehicleStatus(vehicle, spot, subscriber);
                }
                catch (KeyNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Try again.");
                }                
            } while(true);
        }

        public void DisplayVehicleStatus(Vehicle vehicle, Spot? spot, Subscriber? subscriber)
        {
            int consoleWidth = Console.WindowWidth > 0 ? Console.WindowWidth : 80;
            string divider = new string('=', consoleWidth);
            string separator = new string('-', consoleWidth);

            Visit? lastVisit = vehicle.Visits.Values
                .OrderByDescending(v => v.EntryTime)
                .FirstOrDefault();

            Visit? activeVisit = vehicle.Visits.Values
                .FirstOrDefault(v => v.LeaveTime == null);

            List<PendingPayment> pendingPayments = vehicle.Visits.Values
                .Where(v => v.Payment is PendingPayment)
                .Select(v => (PendingPayment)v.Payment!)
                .ToList();

            decimal totalPending = pendingPayments.Sum(p => p.AmountOwed - p.AmountPaid);

            Console.WriteLine(divider);
            Console.WriteLine("VEHICLE STATUS".PadLeft((consoleWidth + 14) / 2));
            Console.WriteLine(divider);
            Console.WriteLine($"License Plate:     {vehicle.LicensePlateNumber}");
            Console.WriteLine(separator);
            Console.WriteLine($"Currently Parked:  {(spot != null ? $"Yes — Spot {spot.SpotID}" : "No")}");
            Console.WriteLine($"Entry Time:        {(activeVisit != null ? activeVisit.EntryTime.ToString("yyyy-MM-dd HH:mm:ss") : lastVisit != null ? $"{lastVisit.EntryTime:yyyy-MM-dd HH:mm:ss} (last visit)" : "NA")}");
            Console.WriteLine($"Last Exit Time:    {(lastVisit?.LeaveTime != null ? lastVisit.LeaveTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : "NA")}");
            Console.WriteLine(separator);
            Console.WriteLine($"Pending Payments:  {(pendingPayments.Count > 0 ? $"Yes — ${totalPending:F2} owed" : "None")}");
            Console.WriteLine(separator);
            Console.WriteLine($"Subscriber:        {(subscriber != null ? $"Yes — {subscriber.Name}" : "No")}");
            Console.WriteLine(divider);

            this.ConsoleUI.Prompt("Press enter to continue.");
        }
    }
}