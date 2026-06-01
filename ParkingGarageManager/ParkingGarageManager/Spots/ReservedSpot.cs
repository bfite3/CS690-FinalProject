using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingGarageManager
{
    public class ReservedSpot : Spot
    {
        public override string StatusChar => "R";
        public override string Status => "Reserved";
        public string SubscriberID { get; private set; }
        public ReservedSpot(string spotID, string subscriberID) : base(spotID)
        {
            this.SubscriberID = subscriberID;
        }

        public override string ToFileString()
        {
            return $"{this.SpotID},R,{this.SubscriberID}";
        }
    }
}