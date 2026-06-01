using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingGarageManager
{
    public class TakenSpot : Spot
    {
        public override string StatusChar => "T";
        public override string Status => "Taken";
        public Vehicle Vehicle { get; set; }
        public TakenSpot(string spotID, Vehicle vehicle) : base(spotID)
        {
            this.Vehicle = vehicle; 
        }

        public override string ToFileString()
        {
            return $"{this.SpotID},T,{this.Vehicle?.LicensePlateNumber ?? "Unknown"}";
        }
    }
}