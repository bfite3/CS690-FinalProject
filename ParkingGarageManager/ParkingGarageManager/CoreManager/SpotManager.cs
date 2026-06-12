using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingGarageManager
{
    public class SpotManager
    {
        public Core Core { get; private set; }

        public SpotManager(Core core)
        {
            this.Core = core;
        }

        public void SetSpotOpen(string spotID)
        {
            if (!this.Core.ParkingGarage.Spots.ContainsKey(spotID))
            {
                throw new KeyNotFoundException($"Spot ID: {spotID} does not exist");
            }

            this.Core.ParkingGarage.Spots[spotID] = new OpenSpot(spotID);
        }

        public void SetSpotReserved(string spotID, string subscriberID)
        {
            if (!this.Core.ParkingGarage.Spots.ContainsKey(spotID))
            {
                throw new KeyNotFoundException($"Spot ID: {spotID} does not exist");
            }

            this.Core.ParkingGarage.Spots[spotID] = new ReservedSpot(spotID, subscriberID);
        }

        public void SetSpotTaken(string spotID, string licensePlateNumber)
        {
            if (!this.Core.ParkingGarage.Spots.ContainsKey(spotID))
                throw new KeyNotFoundException($"Spot ID: {spotID} does not exist");

            if (!this.Core.Vehicles.TryGetValue(licensePlateNumber, out Vehicle? vehicle))
                throw new KeyNotFoundException($"Vehicle {licensePlateNumber} does not exist");

            this.Core.ParkingGarage.Spots[spotID] = new TakenSpot(spotID, vehicle);
        }

        public bool IsGarageFull()
        {
            return FirstOpenSpotID() == null;
        }

        public string FirstOpenSpotID()
        {
            return this.Core.ParkingGarage.Spots.FirstOrDefault(kvp => kvp.Value is OpenSpot).Key;
        }

        public void RestoreSpot(string spotID, ActiveSubscriber? subscriber = null)
        {
            if (subscriber != null)
                this.SetSpotReserved(spotID, subscriber.ID);
            else
                this.SetSpotOpen(spotID);
        }
    }
}