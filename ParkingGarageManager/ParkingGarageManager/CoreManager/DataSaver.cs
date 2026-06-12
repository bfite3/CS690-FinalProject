using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingGarageManager
{
    public class DataSaver
    {
        public Core Core { get; private set; }
        public FileManager PaymentDataManager { get; private set; }
        public FileManager VisitDataManager { get; private set; }
        public FileManager SpotDataManager { get; private set; }
        public FileManager VehicleDataManager { get; private set; }
        public FileManager SubscriberDataManager { get; private set; }

        public DataSaver(Core core)
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

        public void SaveAll()
        {
            this.SavePayments();
            this.SaveVisits();
            this.SaveVehicles();
            this.SaveSpots();
            this.SaveSubscribers();
        }

        public void SavePayments()
        {
            this.PaymentDataManager?.SaveData(this.Core.Vehicles.Values
                .SelectMany(vehicle => vehicle.Visits.Values)
                .Select(visit => visit.Payment)
                .Where(payment => payment != null)
                .OrderBy(payment => payment!.ID)
                .Select(payment => payment!.ToFileString())
            );
        }

        public void SaveVisits()
        {
            this.VisitDataManager?.SaveData(this.Core.Vehicles.Values
                .SelectMany(vehicle => vehicle.Visits.Values)
                .OrderBy(visit => visit.ID)
                .Select(visit => visit.ToFileString())
            );
        }

        public void SaveVehicles()
        {
            this.VehicleDataManager?.SaveData(this.Core.Vehicles.Values.Select(v => v.FileToString()));
        }

        public void SaveSpots()
        {
            this.SpotDataManager?.SaveData(this.Core.ParkingGarage.Spots.Values.Select(s => s.ToFileString()));
        }

        public void SaveSubscribers()
        {
            this.SubscriberDataManager?.SaveData(this.Core.Subscribers.Values.Select(s => s.FileToString()));
        }
    }
}