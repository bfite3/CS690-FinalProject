using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingGarageManager
{
    public class ExpiredSubscriber : Subscriber
    {
        public DateOnly? SubscribeEndDate { get; private set; }

        public ExpiredSubscriber(string id, DateOnly startDate, DateOnly endDate, List<string>? licensePlateNumbers, string driversLicenseNumber, string name, string email)
            : base(id, startDate, licensePlateNumbers, driversLicenseNumber, name, email)
        {
            this.SubscribeEndDate = endDate;
        }

        public override string FileToString()
        {
            int isSubscribed = 0;
            return $"{this.ID},{this.SubscribeStartDate},{this.SubscribeEndDate},{isSubscribed},[],[{string.Join(";", this.LicensePlateNumbers)}],{this.DriversLicenseNumber},{this.Name},{this.Email}";
        }
    }
}