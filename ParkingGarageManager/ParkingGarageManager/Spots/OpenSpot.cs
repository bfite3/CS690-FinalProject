using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingGarageManager
{
    public class OpenSpot : Spot
    {
        public override string StatusChar => "O";
        public override string Status => "Open";
        public OpenSpot(string spotID) : base(spotID)
        {
            
        }

        public override string ToFileString()
        {
            return $"{this.SpotID},O,null";
        }
    }
}