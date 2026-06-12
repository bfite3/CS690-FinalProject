using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingGarageManager
{
    public class CheckInManager
    {
        public Core Core { get; private set; }

        public CheckInManager(Core core)
        {
            this.Core = core;
        }

        public void CheckInSpot(string spotID, string licensePlateNumber, bool isSubscriber = false)
        {
            if (!this.Core.ParkingGarage.Spots.ContainsKey(spotID))
            {
                throw new KeyNotFoundException($"Spot ID: {spotID} does not exist.");
            }

            if (this.Core.ParkingGarage.Spots[spotID] is TakenSpot)
            {
                throw new InvalidOperationException($"Spot {spotID} is already taken.");
            } 
            else if (this.Core.ParkingGarage.Spots[spotID] is ReservedSpot reservedSpot)
            {
                string subscriberID = reservedSpot.SubscriberID;

                bool spotMatchesLicensePlateNumber = this.Core.Subscribers[subscriberID].LicensePlateNumbers.Contains(licensePlateNumber);

                if (!spotMatchesLicensePlateNumber)
                    throw new ArgumentException($"Spot ID {spotID} is a reserved spot and license plate {licensePlateNumber} does not match.");
            } 
            
            this.Core.VehicleManager.AddVehicle(licensePlateNumber);
            this.CreateVisit(licensePlateNumber, spotID);
            this.Core.SpotManager.SetSpotTaken(spotID, licensePlateNumber);
        }

        public string FirstOpenSpotCheckIn(string licensePlateNumber)
        {
            string? openSpotID = this.Core.SpotManager.FirstOpenSpotID();

            if (openSpotID == null)
            {
                throw new InvalidOperationException("No open spots available. Garage is full.");
            }
            this.CheckInSpot(openSpotID, licensePlateNumber);
            return openSpotID;
        }

        public void CreateVisit(string licensePlateNumber, string spotID)
        {
            string maxVisitID = this.Core.Vehicles.Values
                .SelectMany(vehicle => vehicle.Visits.Values)
                .Max(visit => visit.ID) ?? "0";

            string visitID = (int.Parse(maxVisitID) + 1).ToString();
            DateTime entryTime = DateTime.Now;
            Visit newVisit = new Visit(visitID, spotID, entryTime, Core.HourlyRate);

            if (this.Core.Vehicles.TryGetValue(licensePlateNumber, out Vehicle? vehicle))
                vehicle.AddVisit(newVisit);
            else
                throw new KeyNotFoundException($"No vehicle for license plate number {licensePlateNumber} found in the system. Add vehicle then check-in again.");            
        }

        public bool VehicleAlreadyCheckedIn(string licensePlateNumber)
        {
            return this.Core.ParkingGarage.Spots.Values.Any(s => s is TakenSpot takenSpot && takenSpot.Vehicle?.LicensePlateNumber == licensePlateNumber);
        }
    }
}