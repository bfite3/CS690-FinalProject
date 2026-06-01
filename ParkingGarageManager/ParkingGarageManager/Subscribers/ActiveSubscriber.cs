using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingGarageManager
{
    public class ActiveSubscriber : Subscriber
    {
        public List<string> SpotIDs { get; private set; }

         public ActiveSubscriber(string id, DateOnly startDate, List<string>? licensePlateNumbers, string driversLicenseNumber, string name, string email)
            : base(id, startDate, licensePlateNumbers, driversLicenseNumber, name, email)
        {
            this.SpotIDs = new List<string>();
        }

        public void AddSpotID(string spotID)
        {
            if (!this.SpotIDs.Contains(spotID))
            {
                this.SpotIDs.Add(spotID);
            }
        }

        public void UpdateDetails(string? name = null, string? email = null, string? driversLicenseNumber = null, List<string>? licensePlateNumbers = null, List<string>? spotIDs = null)
        {
            base.UpdateDetails(name, email, driversLicenseNumber, licensePlateNumbers);
            spotIDs?.ForEach(this.AddSpotID);
        }

        public override string FileToString()
        {
            string endDate = "null";
            int isSubscribed = 1; 

            return $"{this.ID},{this.SubscribeStartDate},{endDate},{isSubscribed},[{string.Join(";", this.SpotIDs)}],[{string.Join(";", this.LicensePlateNumbers)}],{this.DriversLicenseNumber},{this.Name},{this.Email}";
        }
    }
}