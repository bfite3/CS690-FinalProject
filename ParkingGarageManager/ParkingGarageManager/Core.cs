using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ParkingGarageManager
{
    public class Core
    {
        public ParkingGarage ParkingGarage { get; private set; }
        public Dictionary<string, Vehicle> Vehicles { get; private set; }
        public Dictionary<string, Subscriber> Subscribers { get; private set; }
        public CheckInManager CheckInManager { get; private set; }
        public CheckOutManager CheckOutManager { get; private set; }
        public SubscriberManager SubscriberManager { get; private set; }
        public VehicleManager VehicleManager { get; private set; }
        public SpotManager SpotManager { get; private set; }
        public PaymentManager PaymentManager { get; private set; }
        public const decimal HourlyRate = 5.00m;
        public Core(bool skipLoad = false)
        {
            this.ParkingGarage = new ParkingGarage("Main Parking Garage", 12);
            this.Vehicles = new Dictionary<string, Vehicle>();
            this.Subscribers = new Dictionary<string, Subscriber>();
            
            this.CheckInManager = new CheckInManager(this);
            this.CheckOutManager = new CheckOutManager(this);
            this.SubscriberManager = new SubscriberManager(this);
            this.VehicleManager = new VehicleManager(this);
            this.SpotManager = new SpotManager(this);
            this.PaymentManager = new PaymentManager(this);

            if (!skipLoad)
            {
                var loader = new DataLoader(this);
                loader.LoadAll();
            }
            
        }
    }
}