using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingGarageManager
{
    public class ParkingGarage
    {
        public string Name {get; }
        public int TotalCapacity {get; private set;}
        public Dictionary<string, Spot> Spots {get; private set;}

        public ParkingGarage(string name, int totalCapacity)
        {
            this.Name = name;
            this.TotalCapacity = totalCapacity;
            this.Spots = new Dictionary<string, Spot>();
        }

        public void AddSpot(Spot spot)
        {
            if (this.Spots.ContainsKey(spot.SpotID))
            {
                throw new InvalidOperationException($"Spot ID: {spot.SpotID} already exists.");
            }

            this.Spots.Add(spot.SpotID, spot);
        }
    }
}