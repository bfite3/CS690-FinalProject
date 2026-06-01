using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingGarageManager
{
    public class Vehicle
    {
        public string LicensePlateNumber { get; private set; }
        public List<Visit> Visits { get; private set; }

        public Vehicle(string licensePlateNumber)
        {
            this.LicensePlateNumber = licensePlateNumber;
            this.Visits = new List<Visit>();
        }

        public string FileToString()
        {            
            return $"{this.LicensePlateNumber}";
        }
    }
}