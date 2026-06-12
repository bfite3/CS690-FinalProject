using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingGarageManager
{
    public class ReportManager
    {
        public Core Core { get; private set; }

        public ReportManager(Core core)
        {
            this.Core = core;
        }

        public void PrintEODReport()
        {
            int consoleWidth = Console.WindowWidth > 0 ? Console.WindowWidth : 80;
            string divider = new string('=', consoleWidth);
            string separator = new string('-', consoleWidth);
            DateTime today = DateTime.Today;

            List<Visit> todaysVisits = this.Core.Vehicles.Values
                .SelectMany(v => v.Visits.Values)
                .Where(v => v.EntryTime.Date == today)
                .ToList();

            int totalEntered = todaysVisits.Count;
            int totalExited = todaysVisits.Count(v => v.LeaveTime != null);
            int currentlyOpen = this.Core.ParkingGarage.Spots.Values.Count(s => s is OpenSpot);
            int currentlyTaken = this.Core.ParkingGarage.Spots.Values.Count(s => s is TakenSpot);
            int currentlyReserved = this.Core.ParkingGarage.Spots.Values.Count(s => s is ReservedSpot);

            List<Visit> subscriberVisits = todaysVisits.Where(v =>
            {
                Vehicle? vehicle = this.Core.Vehicles.Values
                    .FirstOrDefault(vh => vh.Visits.ContainsKey(v.ID));

                return vehicle != null && this.Core.SubscriberManager.FindSubscriber(licensePlateNumber: vehicle.LicensePlateNumber) is ActiveSubscriber;
            }).ToList();

            List<Visit> nonSubscriberVisits = todaysVisits.Except(subscriberVisits).ToList();

            int subscriberEntered = subscriberVisits.Count;
            int subscriberExited = subscriberVisits.Count(v => v.LeaveTime != null);
            int nonSubscriberEntered = nonSubscriberVisits.Count;
            int nonSubscriberExited = nonSubscriberVisits.Count(v => v.LeaveTime != null);

            decimal subscriberRevenue = subscriberVisits
                .Where(v => v.Payment is CompletedPayment)
                .Sum(v => ((CompletedPayment)v.Payment!).AmountPaid);

            decimal nonSubscriberRevenue = nonSubscriberVisits
                .Where(v => v.Payment is CompletedPayment)
                .Sum(v => ((CompletedPayment)v.Payment!).AmountPaid);

            decimal totalRevenue = subscriberRevenue + nonSubscriberRevenue;

            List<(string ID, string message)> discrepancies = new List<(string, string)>();
            

            foreach(Visit visit in todaysVisits)
            {
                Vehicle? vehicle = this.Core.Vehicles.Values.FirstOrDefault(v => v.Visits.ContainsKey(visit.ID));

                if (vehicle == null)
                    discrepancies.Add((visit.ID, $"No vehicle found for Visit ID {visit.ID}"));

                if (visit.LeaveTime != null && visit.Payment is PendingPayment pendingPayment)
                    discrepancies.Add((visit.ID, $"Completed visit ID has a pending payment of {pendingPayment.AmountOwed - pendingPayment.AmountPaid:C} for License Plate Number {vehicle?.LicensePlateNumber ?? "Unknown"}"));
            }

            foreach (Visit visit in todaysVisits)
            {
                Vehicle? vehicle = this.Core.Vehicles.Values.FirstOrDefault(v => v.Visits.ContainsKey(visit.ID));

                if (vehicle == null)
                {
                    discrepancies.Add((visit.ID, $"No vehicle found for Visit ID {visit.ID}"));
                    continue;
                }

                if (this.Core.Subscribers.Values
                    .FirstOrDefault(s => s is ActiveSubscriber active && active.SpotIDs.Contains(visit.SpotID)) is not ActiveSubscriber activeSubscriber)
                    continue;

                if (!activeSubscriber.LicensePlateNumbers.Contains(vehicle.LicensePlateNumber))
                    discrepancies.Add((visit.ID, $"Visit ID {visit.ID} was for a Reserved spot and given to a customer who was not subscribed to that spot."));
            }

            Console.WriteLine(divider);
            Console.WriteLine("END OF DAY REPORT".PadLeft((consoleWidth + 17) / 2));
            Console.WriteLine($"{today:yyyy-MM-dd}".PadLeft((consoleWidth + 10) / 2));
            Console.WriteLine(divider);

            Console.WriteLine(" VEHICLE SUMMARY");
            Console.WriteLine(separator);
            Console.WriteLine($"  Total Vehicles Entered:    {totalEntered}");
            Console.WriteLine($"  Total Vehicles Exited:     {totalExited}");
            Console.WriteLine(separator);
            Console.WriteLine($"  Subscribers Entered:       {subscriberEntered}");
            Console.WriteLine($"  Subscribers Exited:        {subscriberExited}");
            Console.WriteLine(separator);
            Console.WriteLine($"  Non-Subscribers Entered:   {nonSubscriberEntered}");
            Console.WriteLine($"  Non-Subscribers Exited:    {nonSubscriberExited}");
            Console.WriteLine(divider);

            Console.WriteLine("  SPOT SUMMARY");
            Console.WriteLine(separator);
            Console.WriteLine($"  Currently Open:            {currentlyOpen}");
            Console.WriteLine($"  Currently Taken:           {currentlyTaken}");
            Console.WriteLine($"  Currently Reserved:        {currentlyReserved}");
            Console.WriteLine(divider);

            Console.WriteLine("  REVENUE SUMMARY");
            Console.WriteLine(separator);
            Console.WriteLine($"  Subscriber Revenue:        {subscriberRevenue:C}");
            Console.WriteLine($"  Non-Subscriber Revenue:    {nonSubscriberRevenue:C}");
            Console.WriteLine(separator);
            Console.WriteLine($"  Total Revenue:             {totalRevenue:C}");
            Console.WriteLine(divider);

            Console.WriteLine("  DISCREPANCIES");
            Console.WriteLine(separator);
            if (discrepancies.Count == 0)
            {
                Console.WriteLine("  No discrepancies found.");
            }
            else
            {
                foreach (var (ID, message) in discrepancies)
                    Console.WriteLine($"  [{ID}] {message}");
            }

            Console.WriteLine(divider);
        }
    }
}