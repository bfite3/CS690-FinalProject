using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingGarageManager
{
    public abstract class Subscriber
    {
        public string ID { get; private set; }
        public DateOnly SubscribeStartDate { get; private set; }
        public List<string> LicensePlateNumbers { get; private set; }
        public string DriversLicenseNumber { get; private set; }
        public string Name { get; private set; }
        public string Email {get; private set; }

        public Subscriber(string id, DateOnly startDate, List<string>? licensePlateNumbers, string driversLicenseNumber, string name, string email)
        {
            this.ID = id;
            this.SubscribeStartDate = startDate;
            this.DriversLicenseNumber = driversLicenseNumber;
            this.Name = name;
            this.Email = email;

            this.LicensePlateNumbers = new List<string>();
            licensePlateNumbers?.ForEach(this.AddLicensePlateNumber);
        }

        public void AddLicensePlateNumber(string licensePlateNumber)
        {
            if (!this.LicensePlateNumbers.Contains(licensePlateNumber))
            {
                this.LicensePlateNumbers.Add(licensePlateNumber);
            }
        }

        public void UpdateDetails(string? name = null, string? email = null, string? driversLicenseNumber = null, List<string>? licensePlateNumbers = null)
        {
            if (name != null) this.Name = name;
            if (email != null) this.Email = email;
            if (driversLicenseNumber != null) this.DriversLicenseNumber = driversLicenseNumber;

            licensePlateNumbers?.ForEach(this.AddLicensePlateNumber);
        }

        public abstract string FileToString();
    }
}