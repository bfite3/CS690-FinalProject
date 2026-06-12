using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingGarageManager
{
    public class SubscriberManager
    {
        public Core Core { get; private set; }

        public SubscriberManager(Core core)
        {
            this.Core = core;
        }

        public void AddSubscriber(DateOnly startDate, string driversLicenseNumber, string name, string email, string? subscriberID = null, DateOnly? endDate = null, bool isSubscribed = true, List<string>? licensePlateNumbers = null, List<string>? spotIDs = null, bool isLoading = false)
        {
            if (this.Core.Subscribers.Values.Any(s => s.DriversLicenseNumber == driversLicenseNumber))
            {
                throw new InvalidOperationException($"A subscriber with driver's license number {driversLicenseNumber} already exists.");
            }

            if (this.Core.Subscribers.Values.SelectMany(s => s.LicensePlateNumbers)
                .Any(lp => licensePlateNumbers?.Contains(lp) ?? false))
            {
                throw new InvalidOperationException($"A subscriber with an entered license plate already exists. Try again");
            }

            subscriberID ??= (this.Core.Subscribers.Count + 1).ToString();

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
                

            this.Core.Subscribers.TryAdd(subscriberID, subscriber);
            licensePlateNumbers?.ForEach(lp => this.Core.VehicleManager.AddVehicle(lp));

            if (subscriber is ActiveSubscriber activeSubscriber)
            {
                spotIDs?.ForEach(activeSubscriber.AddSpotID);
                if (!isLoading)
                    spotIDs?.ForEach(spot => this.Core.SpotManager.SetSpotReserved(spot, activeSubscriber.ID));
            }
        }

        public void ValidateSubscriberUpdate(Subscriber subscriber, string driversLicenseNumber, List<string> licensePlateNumbers)
        {
            if (this.Core.Subscribers.Values.Any(s => s.DriversLicenseNumber == driversLicenseNumber && s.ID != subscriber.ID))
            {
                throw new InvalidOperationException($"A subscriber with driver's license number {driversLicenseNumber} already exists.");
            }

            if (this.Core.Subscribers.Values.Where(s => s.ID != subscriber.ID).SelectMany(s => s.LicensePlateNumbers)
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
                removedSpotIDs.ForEach(this.Core.SpotManager.SetSpotOpen);
                spotIDs.ForEach(spot => this.Core.SpotManager.SetSpotReserved(spot, subscriber.ID));
                licensePlateNumbers.ForEach(lp => this.Core.VehicleManager.AddVehicle(lp));
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

            licensePlateNumbers.ForEach(lp => this.Core.VehicleManager.AddVehicle(lp));
            subscriber.UpdateDetails(name, email, driversLicenseNumber, licensePlateNumbers);
        }

        public Subscriber? FindSubscriber(string? licensePlateNumber = null, string? driversLicenseNumber = null)
        {   
            if (licensePlateNumber == null && driversLicenseNumber == null)
                throw new ArgumentException("Must provide either a license plate number or driver's license number.");
                
            if (licensePlateNumber != null)
            {
                return this.Core.Subscribers.Values
                    .FirstOrDefault(s => s.LicensePlateNumbers.Contains(licensePlateNumber));
            } 
            else if (driversLicenseNumber != null)
            {
                return this.Core.Subscribers.Values
                    .FirstOrDefault(s => s.DriversLicenseNumber == driversLicenseNumber);
            }
            return null;
        }

        public void AddLicensePlateNumber(List<string> licensePlateNumbers, string newLicensePlateNumber, Subscriber? subscriber = null)
        {
            if (licensePlateNumbers.Contains(newLicensePlateNumber))
                throw new InvalidOperationException($"License plate number {newLicensePlateNumber} already exists on that account.");

            if (this.Core.Subscribers.Values.Any(s => s.LicensePlateNumbers.Contains(newLicensePlateNumber) && (subscriber == null || s.ID != subscriber.ID)))
                throw new InvalidOperationException($"License plate number {newLicensePlateNumber} is already associated with another subscriber");

            licensePlateNumbers.Add(newLicensePlateNumber);
        }

        public void AddSpotID(List<string> spotIDs, string newSpotID, Subscriber? subscriber = null)
        {
            if(spotIDs.Contains(newSpotID))
                throw new InvalidOperationException($"Spot ID {newSpotID} already exists on the account.");

            if (this.Core.ParkingGarage.Spots[newSpotID] is TakenSpot && (subscriber == null || subscriber is ActiveSubscriber activeSubscriber && !activeSubscriber.SpotIDs.Contains(newSpotID)))
                throw new InvalidOperationException($"Spot ID {newSpotID} is currently Taken and cannot be added to the account. Try again later."); 

            if (this.Core.ParkingGarage.Spots[newSpotID] is ReservedSpot reservedSpot && (subscriber == null || reservedSpot.SubscriberID != subscriber.ID))
                throw new InvalidOperationException($"Spot ID {newSpotID} is currently Reserved and cannot be added to the account."); 

            spotIDs.Add(newSpotID);
        }

        public bool SubscriberHasCheckedInVehicle(Subscriber subscriber)
        {
            return subscriber.LicensePlateNumbers.Any(this.Core.CheckInManager.VehicleAlreadyCheckedIn);
        }

        public void ExpireSubscriber(ActiveSubscriber subscriber)
        {
            DateOnly endDate = DateOnly.FromDateTime(DateTime.Now);
            List<string> subscriberSpots = subscriber.SpotIDs;

            subscriberSpots.ForEach(this.Core.SpotManager.SetSpotOpen);
            Subscriber expiredSubscriber = new ExpiredSubscriber(subscriber.ID, subscriber.SubscribeStartDate, endDate, subscriber.LicensePlateNumbers, subscriber.DriversLicenseNumber, subscriber.Name, subscriber.Email);
            this.Core.Subscribers[subscriber.ID] = expiredSubscriber;
        }

        public void ReactivateSubscriber(ExpiredSubscriber expiredSubscriber)
        {
            DateOnly startDate = DateOnly.FromDateTime(DateTime.Now);
            Subscriber activeSubscriber = new ActiveSubscriber(expiredSubscriber.ID, startDate, expiredSubscriber.LicensePlateNumbers, expiredSubscriber.DriversLicenseNumber, expiredSubscriber.Name, expiredSubscriber.Email);
            this.Core.Subscribers[expiredSubscriber.ID] = activeSubscriber;
        }
    }
}