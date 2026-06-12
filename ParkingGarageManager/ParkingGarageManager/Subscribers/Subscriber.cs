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

        public void UpdateDetails(string name, string email, string driversLicenseNumber, List<string> licensePlateNumbers)
        {
            this.Name = name;
            this.Email = email;
            this.DriversLicenseNumber = driversLicenseNumber;
            this.LicensePlateNumbers = licensePlateNumbers;
        }

        public abstract string FileToString();
    }
}