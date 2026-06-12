using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingGarageManager.ConsoleUIManager
{
    public class SubscriberUI
    {
        public ConsoleUI ConsoleUI { get; private set; }

        public SubscriberUI(ConsoleUI consoleUI)
        {
            this.ConsoleUI = consoleUI;
        }

        public string ManageSubscribersInput()
        {
            string[] menuOptions =
            {
              "Add new subscriber",
              "Edit subscriber details",
              "Re-activate subscriber",
              "Expire subscriber",
            };

            string input;
            bool isValid;
            do
            {
                Console.WriteLine($"Select an option (1-{menuOptions.Length}) or enter to go back");
                this.ConsoleUI.ListSelection(menuOptions);
                input = this.ConsoleUI.Prompt();

                isValid = input == "" || (int.TryParse(input, out int number) && number >= 1 && number <= menuOptions.Length);

                if (!isValid)
                    Console.WriteLine("Not a valid option. Try again.");

            } while (!isValid);

            return input;
        }

        public void ManageSubscribers(string input)
        {
            switch (input)
            {
                case "1":
                    this.AddSubscriberForm();
                    break;
                case "2":
                    this.EditSubscriber();
                    break;
                case "3":
                    this.ReactivateSubscriber();
                    break;
                case "4":
                    this.ExpireSubscriber();
                    break;
            }
        }

        public void SubscriberForm(Subscriber? existingSubscriber = null)
        {
            string name = existingSubscriber?.Name ?? "";
            string email = existingSubscriber?.Email ?? "";
            string driversLicenseNumber = existingSubscriber?.DriversLicenseNumber ?? "";
            DateOnly? subscribedStartDate = existingSubscriber?.SubscribeStartDate;

            DateOnly? subscribedEndDate = existingSubscriber is ExpiredSubscriber expiredSubscriber
                ? expiredSubscriber.SubscribeEndDate
                : null;

            bool? subscriptionStatus = existingSubscriber == null ? null : existingSubscriber is ActiveSubscriber;
            List<string> licensePlateNumbers = existingSubscriber?.LicensePlateNumbers != null ? new List<string>(existingSubscriber.LicensePlateNumbers) : new List<string>();

            List<string> spotIDs = existingSubscriber is ActiveSubscriber activeSubscriber
                ? new List<string>(activeSubscriber.SpotIDs)
                : new List<string>();

            string[] menuOptions = existingSubscriber is ExpiredSubscriber
            ? new[] {
              "Name",
              "Email",
              "Driver's License Number",
              "License Plate Number",
              "Save"
            }
            : new [] {
              "Name",
              "Email",
              "Driver's License Number",
              "License Plate Number",
              "Spot ID",
              "Save"
            };

            string input;
            do
            {
                this.DisplaySubscriberDetails(name: name, email: email, driversLicenseNumber: driversLicenseNumber, licensePlateNumbers: licensePlateNumbers, spotIDs: spotIDs, startDate: subscribedStartDate, endDate: subscribedEndDate, isSubscribed: subscriptionStatus);
                Console.WriteLine();
                Console.WriteLine($"Select an option (1-{menuOptions.Length}) or enter to go back");
                this.ConsoleUI.ListSelection(menuOptions);
                input = this.ConsoleUI.Prompt();

                switch (input)
                {
                    case "1":
                        string namePrompt = this.ConsoleUI.Prompt("Enter name or enter to go back:");
                        if (namePrompt != "")
                        {
                            name = namePrompt;
                        }
                        break;
                    case "2":
                        string emailPrompt = this.ConsoleUI.Prompt("Enter email or enter to go back:");
                        if (emailPrompt != "")
                        {
                            email = emailPrompt;
                        }
                        break;
                    case "3":
                        string driversLicenseNumberPrompt = this.ConsoleUI.Prompt("Enter driver's license number or enter to go back:");
                        if (driversLicenseNumberPrompt != "")
                        {
                            driversLicenseNumber = driversLicenseNumberPrompt;
                        }
                        break;
                    case "4":
                        this.ManageLicensePlateNumbers(existingSubscriber, licensePlateNumbers);
                        break;
                    case "5":
                        if (existingSubscriber is ExpiredSubscriber)
                        {
                            if (this.SaveSubscriber(existingSubscriber, name, email, driversLicenseNumber, licensePlateNumbers, spotIDs))
                                return;
                        } else
                        {
                            this.ManageSpotIDs(existingSubscriber, spotIDs);
                        }
                        break;
                    case "6":
                        bool saved = this.SaveSubscriber(existingSubscriber, name, email, driversLicenseNumber, licensePlateNumbers, spotIDs);
                        if (saved)
                            return;
                        break;
                    case "":
                        return;
                    default:
                        Console.WriteLine("Not a valid option. Try again.");
                        break;
                }
            } while (true);
        }

        public void AddSubscriberForm() => SubscriberForm();

        public void EditSubscriber()
        {
            do
            {
                Console.WriteLine("EDIT SUBSCRIBER");
                string driversLicenseNumber = this.ConsoleUI.Prompt("Enter subscriber's driver's license number or enter to go back:");

                if (driversLicenseNumber.Equals(""))
                    return;

                Subscriber? subscriber = this.ConsoleUI.Core.SubscriberManager.FindSubscriber(driversLicenseNumber: driversLicenseNumber);

                if (subscriber != null)
                {
                    this.EditSubscriberForm(subscriber);
                    return;
                }

                Console.WriteLine("That driver's license number was not found in the system. Try again.");
            } while (true);
        }

        public void EditSubscriberForm(Subscriber subscriber) => SubscriberForm(existingSubscriber: subscriber);


        public void DisplaySubscriberDetails(string subscriberID = "", DateOnly? startDate = null, DateOnly? endDate = null, bool? isSubscribed = null, List<string>? spotIDs = null, List<string>? licensePlateNumbers = null, string driversLicenseNumber = "", string name = "", string email = "")
        {
            spotIDs ??= new List<string>();
            licensePlateNumbers ??= new List<string>();

            Console.WriteLine("===== Subscriber Details =====");
            Console.WriteLine($"1. Name:                  {(name == "" ? "Not set" : name)}");
            Console.WriteLine($"2. Email:                 {(email == "" ? "Not set" : email)}");
            Console.WriteLine($"3. Driver's License:      {(driversLicenseNumber == "" ? "Not set" : driversLicenseNumber)}");
            Console.WriteLine($"4. License Plate Number:  {(licensePlateNumbers.Count == 0 ? "Not set" : string.Join(", ", licensePlateNumbers))}");
            if (isSubscribed == true || isSubscribed == null)
                Console.WriteLine($"5. Spot ID:               {(spotIDs.Count == 0 ? "Not set" : string.Join(", ", spotIDs))}");
            if (isSubscribed != null)
            {
                Console.WriteLine("==============================");
                Console.WriteLine($"   Subscription Status:   {(isSubscribed == null ? "Not set" : isSubscribed == true ? "Subscribed" : "Expired")}");
                Console.WriteLine($"   Start Date:            {(startDate == null ? "Not set" : startDate.ToString())}");
                Console.WriteLine($"   End Date:              {(endDate == null ? "NA" : endDate.ToString())}");
                Console.WriteLine("==============================");
            }

        }

        public void DisplaySubscriberDetails(Subscriber subscriber)
        {
            this.DisplaySubscriberDetails(
                name: subscriber.Name,
                email: subscriber.Email,
                driversLicenseNumber: subscriber.DriversLicenseNumber,
                licensePlateNumbers: subscriber.LicensePlateNumbers,
                spotIDs: subscriber is ActiveSubscriber activeSubscriber ? activeSubscriber.SpotIDs : null,
                startDate: subscriber.SubscribeStartDate,
                endDate: subscriber is ExpiredSubscriber expiredSubscriber ? expiredSubscriber.SubscribeEndDate : null,
                isSubscribed: subscriber is ActiveSubscriber
            );
        }

        public void ManageLicensePlateNumbers(Subscriber? subscriber, List<string> licensePlateNumbers)
        {
            string addOrRemove = licensePlateNumbers.Count == 0 ? "1" : this.ConsoleUI.AddOrRemovePrompt("LICENSE PLATE NUMBER");

            string licensePlateNumber;
            switch (addOrRemove)
            {
                case "1":
                    do
                    {
                        licensePlateNumber = this.ConsoleUI.Prompt("Enter license plate number or enter to go back:");
                        if (licensePlateNumber.Equals(""))
                            return;

                        try
                        {
                            this.ConsoleUI.Core.SubscriberManager.AddLicensePlateNumber(licensePlateNumbers, licensePlateNumber, subscriber: subscriber);
                            return;
                        } catch (InvalidOperationException ex)
                        {
                            Console.WriteLine(ex.Message);
                            Console.WriteLine("Try again.");
                        }
                    } while (true);
                case "2":
                    do
                    {
                        List<string> licensePlateNumbersSorted = licensePlateNumbers.OrderBy(lp => lp).ToList();
                        int licensePlateNumberCount = licensePlateNumbersSorted.Count;
                        Console.WriteLine($"Select which License Plate Number to remove (1-{licensePlateNumberCount}) or enter to go back:");

                        this.ConsoleUI.ListSelection(licensePlateNumbersSorted);
                        string lpInput = this.ConsoleUI.Prompt();

                        if (lpInput.Equals(""))
                            return;

                        if (int.TryParse(lpInput, out int input))
                        {
                            if (input >= 1 && input <= licensePlateNumberCount)
                            {
                                Console.WriteLine(licensePlateNumbersSorted[input - 1]);
                                licensePlateNumbers.RemoveAll(lp => lp == licensePlateNumbersSorted[input - 1]);
                                return;
                            }
                        }
                        Console.WriteLine("Invalid selection. Try again.");
                    } while(true);
            }
        }

        public void ManageSpotIDs(Subscriber? subscriber, List<string> spotIDs)
        {
            string addOrRemove = spotIDs.Count == 0 ? "1" : this.ConsoleUI.AddOrRemovePrompt("SPOT ID");

            string spotID;
            switch (addOrRemove)
            {
                case "1":
                    do
                    {
                        spotID = this.ConsoleUI.Prompt("Enter spot ID or enter to go back:");

                        if (spotID.Equals(""))
                            return;

                        try
                        {
                            this.ConsoleUI.Core.SubscriberManager.AddSpotID(spotIDs, spotID, subscriber: subscriber);
                            return;
                        } catch (InvalidOperationException ex)
                        {
                            Console.WriteLine(ex.Message);
                            Console.WriteLine("Try again.");
                        }
                    } while (true);

                case "2":
                    do
                    {
                        List<string> spotIDsSorted = spotIDs.OrderBy(sid => sid).ToList();
                        int spotIDsCount = spotIDs.Count;
                        Console.WriteLine($"Select which spot ID to remove (1-{spotIDsCount}) or enter to go back:");

                        this.ConsoleUI.ListSelection(spotIDsSorted);
                        string spotIDInput = this.ConsoleUI.Prompt();

                        if (spotIDInput.Equals(""))
                            return;

                        if (int.TryParse(spotIDInput, out int input))
                        {
                            if (input >= 1 && input <= spotIDsCount)
                            {
                                spotIDs.RemoveAll(sid => sid == spotIDsSorted[input - 1]);
                                return;
                            }
                        }

                        Console.WriteLine("Invalid selection. Try again.");

                    } while(true);
            }
        }

        public bool SaveSubscriber(Subscriber? existingSubscriber, string name, string email, string driversLicenseNumber, List<string> licensePlateNumbers, List<string> spotIDs)
        {
            if (name == "" || email == "" || driversLicenseNumber == "" || licensePlateNumbers.Count == 0 || (existingSubscriber is ActiveSubscriber && spotIDs.Count == 0))
            {
                Console.WriteLine("A field is empty. Ensure all fields have a value and try again.");
                this.ConsoleUI.Prompt("Press enter to continue.");
                return false;
            }
            else
            {
                try
                {
                    DateOnly startDate = existingSubscriber?.SubscribeStartDate ?? DateOnly.FromDateTime(DateTime.Now);
                    if (existingSubscriber == null)
                    {
                        this.ConsoleUI.Core.SubscriberManager.AddSubscriber(startDate, driversLicenseNumber, name, email, licensePlateNumbers: licensePlateNumbers, spotIDs: spotIDs);
                        Console.WriteLine("Subscriber successfully added.");
                    }
                    else if (existingSubscriber is ActiveSubscriber activeSub)
                    {
                        this.ConsoleUI.Core.SubscriberManager.UpdateSubscriber(activeSub, name, email, driversLicenseNumber, licensePlateNumbers, spotIDs);
                        Console.WriteLine("Active subscriber successfully updated");
                    }
                    else if (existingSubscriber is ExpiredSubscriber expiredSub)
                    {
                        this.ConsoleUI.Core.SubscriberManager.UpdateSubscriber(expiredSub, name, email, driversLicenseNumber, licensePlateNumbers);
                        Console.WriteLine("Expired subscriber successfully updated.");
                    }
                    this.ConsoleUI.Prompt("Press enter to continue");
                    return true;
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine(ex.Message);
                    this.ConsoleUI.Prompt("Press enter to continue.");
                    return false;
                }
                catch (KeyNotFoundException ex)
                {
                    Console.WriteLine(ex.Message);
                    this.ConsoleUI.Prompt("Press enter to continue.");
                    return false;
                }
            }
        }

        public void ReactivateSubscriber()
        {
            do
            {
                Console.WriteLine("REACTIVATE SUBSCRIBER");
                string driversLicenseNumber = this.ConsoleUI.Prompt("Enter driver's license number or enter to go back:");

                 if (driversLicenseNumber.Equals(""))
                    return;

                Subscriber? subscriber = this.ConsoleUI.Core.SubscriberManager.FindSubscriber(driversLicenseNumber: driversLicenseNumber);
                
                if (subscriber == null)
                {
                    Console.WriteLine("That driver's license number is not in the system. Add them as a subscriber.");
                    this.ConsoleUI.Prompt("Press enter to continue");
                    continue;
                }

                if (subscriber is not ExpiredSubscriber expiredSubscriber)
                {
                    Console.WriteLine("Already an active subscriber.");
                    this.ConsoleUI.Prompt("Press enter to continue");
                    continue;
                }

                if (this.ConsoleUI.Core.SubscriberManager.SubscriberHasCheckedInVehicle(subscriber))
                {
                    Console.WriteLine("A vehicle on the account is checked in. Check the vehicle out and try again.");
                    this.ConsoleUI.Prompt("Press enter to continue");
                    continue;
                }

                do
                {
                    this.DisplaySubscriberDetails(subscriber);

                    Console.WriteLine("NOTICE: Once reactivated spot IDs must be added through the 'Manage subscribers' -> 'Edit subscriber details' menu.");
                    string input = this.ConsoleUI.Prompt("Are you sure you want to reactivate this subscriber? (y/n) or enter to go back:").ToLower();

                    if (input.Equals("") || input.Equals("n"))
                        break;

                    if (input.Equals("y"))
                    {
                        this.ConsoleUI.Core.SubscriberManager.ReactivateSubscriber(expiredSubscriber);
                        Console.WriteLine("Subscriber successfully reactivated.");
                        this.ConsoleUI.Prompt("Press enter to continue");
                        return;
                    }
                    
                    Console.WriteLine("Invalid selection. Try again.");
                } while(true);


            } while (true);
        }

        public void ExpireSubscriber()
        {
            do
            {
                Console.WriteLine("EXPIRE SUBSCRIBER");
                string driversLicenseNumber = this.ConsoleUI.Prompt("Enter driver's license number or enter to go back:");

                if (driversLicenseNumber.Equals(""))
                    return;

                Subscriber? subscriber = this.ConsoleUI.Core.SubscriberManager.FindSubscriber(driversLicenseNumber: driversLicenseNumber);
                
                if (subscriber == null)
                {
                    Console.WriteLine("That driver's license number is not a subscriber. Try again.");
                    this.ConsoleUI.Prompt("Press enter to continue");
                    continue;
                }

                if (this.ConsoleUI.Core.SubscriberManager.SubscriberHasCheckedInVehicle(subscriber))
                {
                    Console.WriteLine("That subscriber has a checked-in vehicle. Check the vehicle out and try again.");
                    this.ConsoleUI.Prompt("Press enter to continue");
                    continue;
                }

                if (subscriber is not ActiveSubscriber activeSubscriber)
                {
                    Console.WriteLine("Already an expired subscriber.");
                    this.ConsoleUI.Prompt("Press enter to continue");
                    continue;
                }

                do
                {
                    this.DisplaySubscriberDetails(subscriber);

                    string input = this.ConsoleUI.Prompt("Are you sure you want to expire this subscriber? (y/n) or enter to go back:").ToLower();

                    if (input.Equals("") || input.Equals("n"))
                        break;

                    if (input.Equals("y"))
                    {
                        this.ConsoleUI.Core.SubscriberManager.ExpireSubscriber(activeSubscriber);
                        Console.WriteLine("Subscriber successfully expired.");
                        this.ConsoleUI.Prompt("Press enter to continue");
                        return;
                    }
                    
                    Console.WriteLine("Invalid selection. Try again.");
                } while(true);
            } while (true);
        }
    }
}