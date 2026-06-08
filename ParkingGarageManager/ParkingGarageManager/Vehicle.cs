using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ParkingGarageManager
{
    public class Vehicle
    {
        public string LicensePlateNumber { get; private set; }
        public Dictionary<string, Visit> Visits { get; private set; }

        public Vehicle(string licensePlateNumber)
        {
            this.LicensePlateNumber = licensePlateNumber;
            this.Visits = new Dictionary<string, Visit>();
        }

        public void AddVisit(Visit visit)
        {
            if (this.Visits.ContainsKey(visit.ID))
            {
                throw new InvalidOperationException($"Visit ID: {visit.ID} already exists for License Plate Number {this.LicensePlateNumber}.");
            }

            this.Visits.Add(visit.ID, visit);
        }

        public string FileToString()
        {            
            return $"{this.LicensePlateNumber},[{string.Join(";", this.Visits.Keys)}]";
        }
    }
}