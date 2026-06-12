using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingGarageManager
{
    public class VehicleManager
    {
        public Core Core { get; private set; }

        public VehicleManager(Core core)
        {
            this.Core = core;
        }

        public void AddVehicle(string licensePlateNumber, List<string>? visitIDs = null, Dictionary<string, Visit>? visitsCSV = null)
        {
            if (this.Core.Vehicles.ContainsKey(licensePlateNumber))
                return;

            Vehicle newVehicle = new Vehicle(licensePlateNumber);
            this.Core.Vehicles.Add(licensePlateNumber, newVehicle);

            visitIDs?.ForEach(vid => 
            {
                if (visitsCSV != null && visitsCSV.TryGetValue(vid, out Visit? visit)) 
                    newVehicle.AddVisit(visit);
                else
                {
                    Console.WriteLine($"Visit ID: {vid} not found. Check visit-data.txt");
                    Console.WriteLine("Press enter to continue");
                    Console.ReadLine();
                } 
            });
        }

        public Vehicle FindVehicle(string licensePlateNumber)
        {
            if (!this.Core.Vehicles.TryGetValue(licensePlateNumber, out Vehicle? vehicle))
                throw new KeyNotFoundException($"No vehicle found for license plate: {licensePlateNumber}");

            return vehicle;
        }

        public Spot? FindSpotByVehicle(Vehicle vehicle)
        {
            Spot? spot = this.Core.ParkingGarage.Spots.Values
                .FirstOrDefault(spot => spot is TakenSpot takenSpot 
                    && takenSpot.Vehicle?.LicensePlateNumber == vehicle.LicensePlateNumber);

            return spot;
        }
    }
}