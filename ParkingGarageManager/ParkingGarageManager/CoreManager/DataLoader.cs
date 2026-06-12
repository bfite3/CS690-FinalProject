using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingGarageManager
{
    public class DataLoader
    {
        public Core Core { get; private set; }
        public FileManager PaymentDataManager { get; private set; }
        public FileManager VisitDataManager { get; private set; }
        public FileManager SpotDataManager { get; private set; }
        public FileManager VehicleDataManager { get; private set; }
        public FileManager SubscriberDataManager { get; private set; }

        public DataLoader(Core core)
        {
            this.Core = core;

            string dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");

            if (!Directory.Exists(dataPath))
                Directory.CreateDirectory(dataPath);

            this.PaymentDataManager = new FileManager(Path.Combine(dataPath, "payment-data.txt"));
            this.VisitDataManager = new FileManager(Path.Combine(dataPath,"visit-data.txt"));
            this.SpotDataManager = new FileManager(Path.Combine(dataPath,"spot-data.txt"));
            this.VehicleDataManager = new FileManager(Path.Combine(dataPath,"vehicle-data.txt"));
            this.SubscriberDataManager = new FileManager(Path.Combine(dataPath,"subscriber-data.txt"));
        }

        public void LoadAll()
        {
            Dictionary<string, Payment> paymentsCSV = new Dictionary<string, Payment>();
            Dictionary<string, Visit> visitsCSV = new Dictionary<string, Visit>();

            this.LoadPayments(paymentsCSV);
            this.LoadVisits(visitsCSV, paymentsCSV);
            this.InitializeSpotData();
            this.LoadVehicles(visitsCSV);
            this.LoadSpots();
            this.LoadSubscribers();
        }

        public void InitializeSpotData()
        {
            string[]? spotData = this.SpotDataManager?.ReturnData();
            if (spotData == null || spotData.Length == 0)
            {
                List<string> defaultSpots = new List<string>
                {
                    "A01,O,null",
                    "A02,O,null",
                    "A03,O,null",
                    "B01,O,null",
                    "B02,O,null",
                    "B03,O,null",
                    "C01,O,null",
                    "C02,O,null",
                    "C03,O,null",
                    "D01,O,null",
                    "D02,O,null",
                    "D03,O,null"
                };
                this.SpotDataManager?.SaveData(defaultSpots);
            }
        }

        public void LoadPayments(Dictionary<string, Payment> paymentsCSV)
        {
            string[]? paymentData = this.PaymentDataManager?.ReturnData();

            if (paymentData == null)
                return;

            foreach(var payment in paymentData)
            {
                string[] splitPayment = payment.Split(",");

                string paymentID = splitPayment[0];
                DateTime chargedDate = DateTime.Parse(splitPayment[1]);
                decimal amountOwed = decimal.Parse(splitPayment[3]);
                decimal amountPaid = decimal.Parse(splitPayment[4]);
                string status = splitPayment[5];

                Payment newPayment;
                if(status == "Completed")
                {   
                    DateTime paidDate = DateTime.Now;
                    if(DateTime.TryParse(splitPayment[2], out DateTime vPaidDate))
                        paidDate = vPaidDate;
                    else
                    {
                       Console.WriteLine($"{paymentID} has an invalid paidDate. Check payments.txt and try again.");
                       Console.WriteLine("Press enter to continue."); 
                    }
                        
                    newPayment = new CompletedPayment(paymentID, chargedDate, paidDate, amountPaid, amountOwed);
                }
                else
                {
                    newPayment = new PendingPayment(paymentID, chargedDate, amountOwed, amountPaid);
                }
                paymentsCSV.Add(paymentID, newPayment);
            }
        }

        public void LoadVisits(Dictionary<string, Visit> visitsCSV, Dictionary<string, Payment> paymentsCSV)
        {
            string[]? visitData = this.VisitDataManager?.ReturnData();

            if (visitData == null)
                return;

            foreach (var visit in visitData)
            {
                string[] splitVisit = visit.Split(",");

                string visitID = splitVisit[0];
                string spotID = splitVisit[1];
                DateTime entryTime = DateTime.Parse(splitVisit[2]);
                DateTime? leaveTime = splitVisit[3] == "null" ? null : DateTime.Parse(splitVisit[2]);
                string? totalHours = splitVisit[4] == "null" ? null : splitVisit[3];
                decimal rate = decimal.Parse(splitVisit[5]);
                string paymentID = splitVisit[6];

                if(!paymentsCSV.TryGetValue(paymentID, out Payment? payment))
                {
                    Console.WriteLine($"Visit ID {visitID} does not have a payment.");
                }

                Visit newVisit = new Visit(id: visitID, spotID: spotID, entryTime: entryTime, totalHours: totalHours, rate: rate, payment: payment, leaveTime: leaveTime);

                visitsCSV.Add(visitID, newVisit);
            }
        }

        public void LoadVehicles(Dictionary<string, Visit> visitsCSV)
        {
            string[]? vehicleData = this.VehicleDataManager?.ReturnData();

            if (vehicleData == null)
                return;
                
            foreach (var vehicle in vehicleData)
            {
                string[] splitVehicle = vehicle.Split(",");
                string licensePlateNumber = splitVehicle[0];

                string visitID = splitVehicle[1];
                List<string> visitIDs = visitID == "[]"
                    ? new List<string>()
                    : visitID.Trim('[', ']').Split(';').ToList();

                this.Core.VehicleManager.AddVehicle(licensePlateNumber, visitIDs, visitsCSV);
            }
        }

        public void LoadSpots()
        {
            string[]? spotData = SpotDataManager?.ReturnData();

            if (spotData == null)
                return;

            foreach (var line in spotData)
            {
                var splitLine = line.Split(",");
                var spotID = splitLine[0];
                var spotStatus = splitLine[1];
                var spotDetails = splitLine[2];


                switch (spotStatus)
                {
                    case "O":
                        this.Core.ParkingGarage.Spots.Add(spotID, new OpenSpot(spotID));
                        break;
                    case "T":
                        if (this.Core.Vehicles.TryGetValue(spotDetails, out Vehicle? vehicle))
                            this.Core.ParkingGarage.Spots.Add(spotID, new TakenSpot(spotID, vehicle));
                        else
                        {
                            Console.WriteLine($"Vehicle {spotDetails} not found for spot {spotID}. Check vehicle-data.txt.");
                            Console.WriteLine("Press enter to continue");
                            Console.ReadLine();
                        }
                        break;
                    case "R":
                        this.Core.ParkingGarage.Spots.Add(spotID, new ReservedSpot(spotID, spotDetails));
                        break;
                    default:
                        Console.WriteLine($"Invalid spot status for {spotID}. Review and correct spot-data.txt.");
                        break;
                }
            }
        }

        public void LoadSubscribers()
        {
            string[]? subscriberData = this.SubscriberDataManager?.ReturnData();
            

            if (subscriberData == null)
                return;

            foreach (var subscriber in subscriberData)
            {
                string[] splitSubscriber = subscriber.Split(",");
                string subscriberID = splitSubscriber[0];
                DateOnly startDate = DateOnly.Parse(splitSubscriber[1].Trim('\''));

                string isSubscribedString = splitSubscriber[3];
                bool isSubscribedBool = isSubscribedString == "1";

                DateOnly? endDate = isSubscribedBool ? null : DateOnly.Parse(splitSubscriber[2].Trim('\''));

                string spotID = splitSubscriber[4];
                List<string> spotIDs = spotID == "[]"
                    ? new List<string>()
                    : spotID.Trim('[', ']').Split(';').ToList();

                string licensePlateNumber = splitSubscriber[5];
                List<string> licensePlateNumbers = licensePlateNumber == "[]"
                    ? new List<string>()
                    : licensePlateNumber.Trim('[', ']').Split(';').ToList();

                string driversLicenseNumber = splitSubscriber[6];

                string name = splitSubscriber[7];
                string email = splitSubscriber[8];
                
                try
                {
                    this.Core.SubscriberManager.AddSubscriber(startDate, driversLicenseNumber, name, email, subscriberID: subscriberID, endDate: endDate, isSubscribed: isSubscribedBool, licensePlateNumbers: licensePlateNumbers, spotIDs: spotIDs, isLoading: true);
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Check subscriber-data.txt and load subscribers again.");
                }
            }
        }
    }
}