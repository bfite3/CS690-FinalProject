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
        public FileManager SpotDataManager { get; private set; }
        public FileManager VehicleDataManager { get; private set; }
        public FileManager SubscriberDataManager { get; private set; }
        public Dictionary<string, Vehicle> Vehicles { get; private set; }
        public Dictionary<string, Subscriber> Subscribers { get; private set; }
        public Core()
        {
            this.ParkingGarage = new ParkingGarage("Main Parking Garage", 12);
            this.SpotDataManager = new FileManager("data/spot-data.txt");
            this.VehicleDataManager = new FileManager("data/vehicle-data.txt");
            this.SubscriberDataManager = new FileManager("data/subscriber-data.txt");
            this.Vehicles = new Dictionary<string, Vehicle>();
            this.Subscribers = new Dictionary<string, Subscriber>();
            this.InitializeSpotData();
            this.LoadVehicles();
            this.LoadSpots();
            this.LoadSubscribers();
        }

        public void InitializeSpotData()
        {
            string[] spotData = this.SpotDataManager.ReturnData();
            if (spotData.Length == 0)
            {
                List<string> defaultSpots = new List<string>
                {
                    "A01,O,null",
                    "A02,O,null",
                    "A03,O,null",
                    "B01,O,null",
                    "B02,O,null",
                    "B03,O,null",
                    "C01,O,null",
                    "C02,O,null",
                    "C03,O,null",
                    "D01,O,null",
                    "D02,O,null",
                    "D03,O,null"
                };
                this.SpotDataManager.SaveData(defaultSpots);
            }
        }

        public void LoadVehicles()
        {
            string[] vehicleData = this.VehicleDataManager.ReturnData();
            foreach (var vehicle in vehicleData)
            {
                string[] splitVehicle = vehicle.Split(",");
                string licensePlateNumber = splitVehicle[0];

 
                this.AddVehicle(licensePlateNumber);
            }
        }

        public void LoadSpots()
        {
            string[] spotData = SpotDataManager.ReturnData();
            foreach (var line in spotData)
            {
                var splitLine = line.Split(",");
                var spotID = splitLine[0];
                var spotStatus = splitLine[1];
                var spotDetails = splitLine[2];

                switch (spotStatus)
                {
                    case "O":
                        this.ParkingGarage.Spots.Add(spotID, new OpenSpot(spotID));
                        break;
                    case "T":
                        this.ParkingGarage.Spots.Add(spotID, new TakenSpot(spotID, new Vehicle(spotDetails)));
                        break;
                    case "R":
                        this.ParkingGarage.Spots.Add(spotID, new ReservedSpot(spotID, spotDetails));
                        break;
                    default:
                        Console.WriteLine($"Invalid spot status for {spotID}. Review and correct spot-data.txt.");
                        break;
                }
            }
        }

        public void LoadSubscribers()
        {
            string[] subscriberData = this.SubscriberDataManager.ReturnData();
            foreach (var subscriber in subscriberData)
            {
                string[] splitSubscriber = subscriber.Split(",");
                string subscriberID = splitSubscriber[0];
                DateOnly startDate = DateOnly.Parse(splitSubscriber[1].Trim('\''));

                string isSubscribedString = splitSubscriber[3];
                bool isSubscribedBool = isSubscribedString == "1";

                DateOnly? endDate = isSubscribedBool ? null : DateOnly.Parse(splitSubscriber[2].Trim('\''));

                string spotID = splitSubscriber[4];
                List<string> spotIDs = spotID == "[]"
                    ? new List<string>()
                    : spotID.Trim('[', ']').Split(';').ToList();

                string licensePlateNumber = splitSubscriber[5];
                List<string> licensePlateNumbers = licensePlateNumber == "[]"
                    ? new List<string>()
                    : licensePlateNumber.Trim('[', ']').Split(';').ToList();

                string driversLicenseNumber = splitSubscriber[6];

                string name = splitSubscriber[7];
                string email = splitSubscriber[8];
                
                try
                {
                    this.AddSubscriber(startDate, driversLicenseNumber, name, email, subscriberID: subscriberID, endDate: endDate, isSubscribed: isSubscribedBool, licensePlateNumbers: licensePlateNumbers, spotIDs: spotIDs);
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Check subscriber-data.txt and load subscribers again.");
                }
            }
        }

        public void SaveVehicles()
        {
            this.VehicleDataManager.SaveData(this.Vehicles.Values.Select(v => v.FileToString()));
        }

        public void SaveSpots()
        {
            this.SpotDataManager.SaveData(this.ParkingGarage.Spots.Values.Select(s => s.ToFileString()));
        }

        public void SaveSubscribers()
        {
            this.SubscriberDataManager.SaveData(this.Subscribers.Values.Select(s => s.FileToString()));
        }

        public void AddVehicle(string licensePlateNumber)
        {
            if (this.Vehicles.ContainsKey(licensePlateNumber))
                return;

            Vehicle newVehicle = new Vehicle(licensePlateNumber);
            this.Vehicles.Add(licensePlateNumber, newVehicle);
        }

        public void SetSpotOpen(string spotID)
        {
            if (!this.ParkingGarage.Spots.ContainsKey(spotID))
            {
                throw new KeyNotFoundException($"Spot ID: {spotID} does not exist");
            }

            this.ParkingGarage.Spots[spotID] = new OpenSpot(spotID);
        }

        public void SetSpotReserved(string spotID, string subscriberID)
        {
            if (!this.ParkingGarage.Spots.ContainsKey(spotID))
            {
                throw new KeyNotFoundException($"Spot ID: {spotID} does not exist");
            }

            this.ParkingGarage.Spots[spotID] = new ReservedSpot(spotID, subscriberID);
        }

        public void SetSpotTaken(string spotID, string licensePlateNumber)
        {
            if (!this.ParkingGarage.Spots.ContainsKey(spotID))
                throw new KeyNotFoundException($"Spot ID: {spotID} does not exist");

            if (!this.Vehicles.TryGetValue(licensePlateNumber, out Vehicle? vehicle))
                throw new KeyNotFoundException($"Vehicle {licensePlateNumber} does not exist");

            this.ParkingGarage.Spots[spotID] = new TakenSpot(spotID, vehicle);
        }

        public void AddSubscriber(DateOnly startDate, string driversLicenseNumber, string name, string email, string? subscriberID = null, DateOnly? endDate = null, bool isSubscribed = true, List<string>? licensePlateNumbers = null, List<string>? spotIDs = null)
        {
            if (this.Subscribers.Values.Any(s => s.DriversLicenseNumber == driversLicenseNumber))
            {
                throw new InvalidOperationException($"A subscriber with driver's license number {driversLicenseNumber} already exists.");
            }

            if (this.Subscribers.Values.SelectMany(s => s.LicensePlateNumbers)
                .Any(lp => licensePlateNumbers?.Contains(lp) ?? false))
            {
                throw new InvalidOperationException($"A subscriber with an entered license plate already exists. Try again");
            }

            subscriberID ??= (this.Subscribers.Count + 1).ToString();

            Subscriber subscriber;
            if (isSubscribed)
            {
                subscriber = new ActiveSubscriber(subscriberID, startDate, licensePlateNumbers, driversLicenseNumber, name, email);
            } 
            else
            {
                if (endDate == null)
                    throw new ArgumentException("End date is required for an expired subscriber. Check subscriber-data.txt and reload.");

                subscriber = new ExpiredSubscriber(subscriberID, startDate, endDate.Value, licensePlateNumbers, driversLicenseNumber, name, email);
            }
                

            this.Subscribers.TryAdd(subscriberID, subscriber);
            licensePlateNumbers?.ForEach(this.AddVehicle);

            if (subscriber is ActiveSubscriber activeSubscriber)
            {
                spotIDs?.ForEach(activeSubscriber.AddSpotID);
                spotIDs?.ForEach(spot => this.SetSpotReserved(spot, activeSubscriber.ID));
            }
        }

        public void ValidateSubscriberUpdate(Subscriber subscriber, string driversLicenseNumber, List<string> licensePlateNumbers)
        {
            if (this.Subscribers.Values.Any(s => s.DriversLicenseNumber == driversLicenseNumber && s.ID != subscriber.ID))
            {
                throw new InvalidOperationException($"A subscriber with driver's license number {driversLicenseNumber} already exists.");
            }

            if (this.Subscribers.Values.Where(s => s.ID != subscriber.ID).SelectMany(s => s.LicensePlateNumbers)
                .Any(lp => licensePlateNumbers?.Contains(lp) ?? false))
            {
                throw new InvalidOperationException($"A subscriber with an entered license plate already exists. Try again");
            }
        }

        public void UpdateSubscriber(ActiveSubscriber subscriber, string name, string email, string driversLicenseNumber, List<string> licensePlateNumbers, List<string> spotIDs)
        {
            this.ValidateSubscriberUpdate(subscriber, driversLicenseNumber, licensePlateNumbers);

            List<string> removedSpotIDs = subscriber.SpotIDs.Except(spotIDs).ToList();
            try
            {
                removedSpotIDs.ForEach(this.SetSpotOpen);
                spotIDs?.ForEach(spot => this.SetSpotReserved(spot, subscriber.ID));
                licensePlateNumbers?.ForEach(this.AddVehicle);
                subscriber.UpdateDetails(name, email, driversLicenseNumber, licensePlateNumbers, spotIDs);
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
        }

        public void UpdateSubscriber(ExpiredSubscriber subscriber, string name, string email, string driversLicenseNumber, List<string> licensePlateNumbers)
        {
            this.ValidateSubscriberUpdate(subscriber, driversLicenseNumber, licensePlateNumbers);

            licensePlateNumbers?.ForEach(this.AddVehicle);
            subscriber.UpdateDetails(name, email, driversLicenseNumber, licensePlateNumbers);
        }

        public bool IsGarageFull()
        {
            return FirstOpenSpotID() == null;
        }

        public string FirstOpenSpotID()
        {
            return this.ParkingGarage.Spots.FirstOrDefault(kvp => kvp.Value is OpenSpot).Key;
        }

        public Subscriber? FindSubscriber(string? licensePlateNumber = null, string? driversLicenseNumber = null)
        {   
            if (licensePlateNumber == null && driversLicenseNumber == null)
                throw new ArgumentException("Must provide either a license plate number or driver's license number.");
                
            if (licensePlateNumber != null)
            {
                return this.Subscribers.Values
                    .FirstOrDefault(s => s.LicensePlateNumbers.Contains(licensePlateNumber));
            } else if (driversLicenseNumber != null)
            {
                return this.Subscribers.Values
                    .FirstOrDefault(s => s.DriversLicenseNumber == driversLicenseNumber);
            }
            return null;
        }

        public void CheckInSpot(string spotID, string licensePlateNumber, bool isSubscriber = false)
        {
            if (!this.ParkingGarage.Spots.ContainsKey(spotID))
            {
                throw new KeyNotFoundException($"Spot ID: {spotID} does not exist.");
            }

            if (this.ParkingGarage.Spots[spotID] is TakenSpot)
            {
                throw new InvalidOperationException($"Spot {spotID} is already taken.");
            } 
            else if (this.ParkingGarage.Spots[spotID] is ReservedSpot reservedSpot)
            {
                string subscriberID = reservedSpot.SubscriberID;

                bool spotMatchesLicensePlateNumber = this.Subscribers[subscriberID].LicensePlateNumbers.Contains(licensePlateNumber);

                if (!spotMatchesLicensePlateNumber)
                    throw new ArgumentException($"Spot ID {spotID} is a reserved spot and license plate {licensePlateNumber} does not match.");
            } 
            
            this.AddVehicle(licensePlateNumber);
            this.SetSpotTaken(spotID, licensePlateNumber);
        }

        public string FirstOpenSpotCheckIn(string licensePlateNumber)
        {
            string? openSpotID = this.FirstOpenSpotID();

            if (openSpotID == null)
            {
                throw new InvalidOperationException("No open spots available. Garage is full.");
            }
            this.CheckInSpot(openSpotID, licensePlateNumber);
            return openSpotID;
        }

        public void CheckOutSpot(string licensePlateNumber)
        {
            string? spotID = this.ParkingGarage.Spots
                .FirstOrDefault(kvp => kvp.Value is TakenSpot takenSpot 
                    && takenSpot.Vehicle?.LicensePlateNumber == licensePlateNumber).Key;
            
            if (spotID == null)
                throw new KeyNotFoundException($"No spot found for license plate: {licensePlateNumber}.");

            Subscriber? subscriber = this.Subscribers.Values
                .FirstOrDefault(s => s.LicensePlateNumbers.Contains(licensePlateNumber));
                

            if (subscriber is ActiveSubscriber activeSubscriber)
                this.SetSpotReserved(spotID, activeSubscriber.ID);
            else
                this.SetSpotOpen(spotID);
        }

        public void ReactivateSubscriber(ExpiredSubscriber expiredSubscriber)
        {
            DateOnly startDate = DateOnly.FromDateTime(DateTime.Now);
            Subscriber activeSubscriber = new ActiveSubscriber(expiredSubscriber.ID, startDate, expiredSubscriber.LicensePlateNumbers, expiredSubscriber.DriversLicenseNumber, expiredSubscriber.Name, expiredSubscriber.Email);
            this.Subscribers[expiredSubscriber.ID] = activeSubscriber;
        }

        public void ExpireSubscriber(ActiveSubscriber subscriber)
        {
            DateOnly endDate = DateOnly.FromDateTime(DateTime.Now);
            List<string> subscriberSpots = subscriber.SpotIDs;

            subscriberSpots.ForEach(this.SetSpotOpen);
            Subscriber expiredSubscriber = new ExpiredSubscriber(subscriber.ID, subscriber.SubscribeStartDate, endDate, subscriber.LicensePlateNumbers, subscriber.DriversLicenseNumber, subscriber.Name, subscriber.Email);
            this.Subscribers[subscriber.ID] = expiredSubscriber;
        }

        public void AddLicensePlateNumber(List<string> licensePlateNumbers, string newLicensePlateNumber)
        {
            if (licensePlateNumbers.Contains(newLicensePlateNumber))
                throw new InvalidOperationException($"License plate number {newLicensePlateNumber} already exists on that account.");

            if (this.Subscribers.Values.Any(s => s.LicensePlateNumbers.Contains(newLicensePlateNumber)))
                throw new InvalidOperationException($"License plate number {newLicensePlateNumber} is already associated with another subscriber");

            licensePlateNumbers.Add(newLicensePlateNumber);
        }

        public void AddSpotID(List<string> spotIDs, string newSpotID)
        {
            if(spotIDs.Contains(newSpotID))
                throw new InvalidOperationException($"Spot ID {newSpotID} already exists on the account.");

            if (this.ParkingGarage.Spots[newSpotID] is TakenSpot)
                throw new InvalidOperationException($"Spot ID {newSpotID} is currently Taken and cannot be added to the account. Try again later."); 

            if (this.ParkingGarage.Spots[newSpotID] is ReservedSpot)
                throw new InvalidOperationException($"Spot ID {newSpotID} is currently Reserved and cannot be added to the account."); 

            spotIDs.Add(newSpotID);
        }

        public bool VehicleAlreadyCheckedIn(string licensePlateNumber)
        {
            return this.ParkingGarage.Spots.Values.Any(s => s is TakenSpot takenSpot && takenSpot.Vehicle?.LicensePlateNumber == licensePlateNumber);
        }

        public bool SubscriberHasCheckedInVehicle(Subscriber subscriber)
        {
            return subscriber.LicensePlateNumbers.Any(this.VehicleAlreadyCheckedIn);
        }
    }
}