using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingGarageManager
{
    public abstract class Spot
    {
        public string SpotID { get; set; }
        public abstract string StatusChar { get; }
        public abstract string Status { get; }

        public Spot(string spotID)
        {
            this.SpotID = spotID;
        }

        public abstract string ToFileString();
    }
}