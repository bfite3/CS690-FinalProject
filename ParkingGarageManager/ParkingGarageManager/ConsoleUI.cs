using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.Swift;
using System.Threading.Tasks;

namespace ParkingGarageManager
{
    public class ConsoleUI
    {
        public Core Core { get; private set; }
        public ConsoleUI()
        {
            this.Core = new Core();
        }

        public void Show()
        {
            string userInput;
            try
            {
                do
                {
                    this.DisplayParkingGarage(this.Core.ParkingGarage.Spots);
                    userInput = this.DisplayUserPrompt().ToLower();

                    if (!userInput.Equals("q"))
                        this.processUserInput(userInput);

                } while (userInput != "q");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.WriteLine("Saving data...");
                this.Core.SaveVehicles();
                this.Core.SaveSpots();
                this.Core.SaveSubscribers();
            } 
        }

        public void DisplayParkingGarage(Dictionary<string, Spot> spots)
        {
                int consoleWidth = Console.WindowWidth > 0 ? Console.WindowWidth : 80;
                string divider = new string('=', consoleWidth);
                string separator = new string('-', consoleWidth);

                int available = spots.Values.Count(s => s is OpenSpot);
                int taken = spots.Values.Count(s => s is TakenSpot);
                int reserved = spots.Values.Count(s => s is ReservedSpot);
                int total = spots.Count;

                Console.WriteLine(divider);
                Console.WriteLine("PARKING GARAGE STATUS DISPLAY".PadLeft((consoleWidth + 29) / 2));
                Console.WriteLine(divider);
                if (this.Core.IsGarageFull())
                {
                    Console.WriteLine("THE GARAGE IS FULL");
                    Console.WriteLine(separator);
                }
                Console.WriteLine($"Available: {available} | Taken: {taken} | Reserved: {reserved} | Total: {total}");
                Console.WriteLine(separator);

                int columns = 3;
                int cellWidth = 24;
                List<Spot> spotList = spots.Values.ToList();

                for (int row = 0; row < spotList.Count; row += columns)
                {
                    for (int col = 0; col < columns && row + col < spotList.Count; col++)
                    {
                        Spot spot = spotList[row + col];

                        string label;
                        if (spot is TakenSpot takenSpot)
                        {
                            if (takenSpot.Vehicle != null)
                                label = takenSpot.Vehicle.LicensePlateNumber;
                            else
                                label = "UNKNOWN";
                        }
                        else
                        {
                            label = spot.Status.ToUpper();
                        }

                        string cell = $"[{spot.SpotID}: [{spot.StatusChar}] {label,-10}]";
                        Console.Write(cell.PadRight(cellWidth));
                    }
                    Console.WriteLine();
                }

                Console.WriteLine();
                Console.WriteLine("Legend: [O] = Open | [T] = Taken | [R] = Reserved");
                Console.WriteLine(divider);
        }

        public string DisplayUserPrompt()
        {
            string[] menuOptions =
            {
                "Check-in",
                "Check-out",
                "Check vehicle status",
                "Manage subscribers",
                "EOD report"
            };
            Console.WriteLine($"Select an option (1-{menuOptions.Length})");
            this.ListSelection(menuOptions);

           return this.Prompt("Press q to quit");
        }

        public void ListSelection(IEnumerable<string> selections)
        {
            int i = 1;
            foreach (string selection in selections)
            {
                Console.WriteLine($"{i++}. {selection}");
            }
        }

        public string Prompt(string message = "")
        {   
            if (message != "")
            {
                Console.WriteLine(message);
            }
            Console.Write("> ");
            string input = Console.ReadLine() ?? "";
            Console.WriteLine();
            return input;
        }

        public void processUserInput(string userInput)
        {
            switch (userInput)
            {
                case "1":
                    this.CheckIn();
                    break;
                case "2":
                    this.CheckOut();
                    break;
                case "3":
                    Console.WriteLine("Work in progress. Try again later.");
                    break;
                case "4":
                    string manageSubscribersInput = this.ManageSubscribersInput();
                    this.ManageSubscribers(manageSubscribersInput);
                    break;
                case "5":
                    Console.WriteLine("Work in progress. Try again later.");
                    break;
                default:
                    Console.WriteLine("Not a valid input. Try again.");
                    break;
            }
        }

        public void CheckIn()
        {
            do
            {
                Console.WriteLine("CHECK IN");
                string? licensePlateNumber = this.Prompt("Enter license plate number or enter to go back:");

                if (licensePlateNumber.Equals(""))
                {
                    return;
                }

                if (this.Core.VehicleAlreadyCheckedIn(licensePlateNumber))
                {
                    Console.WriteLine("That vehicle is already checked in.");
                    continue;
                }

                Subscriber? subscriber = this.Core.FindSubscriber(licensePlateNumber: licensePlateNumber);

                if (subscriber is ActiveSubscriber activeSubscriber)
                {
                    this.subscriberCheckIn(activeSubscriber, licensePlateNumber);
                    return;
                }
                else
                {
                    if (this.Core.IsGarageFull()) {
                        Console.WriteLine("The garage is full. Please come back later.");
                        this.Prompt("Press enter to continue");
                        return;
                    }
                    this.nonSubscriberCheckIn(licensePlateNumber);
                    return;
                }
                
            } while (true);
        }

        public void subscriberCheckIn(ActiveSubscriber subscriber, string licensePlateNumber)
        {
            Console.WriteLine($"{licensePlateNumber} is a subscriber. Retrieving spot(s).");
            List <string> availableSpotIDs = subscriber.SpotIDs.Where(spot => this.Core.ParkingGarage.Spots[spot] is not TakenSpot takenSpot).OrderBy(s => s).ToList();

            switch(availableSpotIDs.Count)
            {
                case 0:
                    Console.WriteLine("No available reserved spots open for that subscriber.");
                    this.Prompt("Press enter to continue");
                    break;
                case 1:
                    this.Core.CheckInSpot(availableSpotIDs[0], licensePlateNumber, isSubscriber: true);
                    Console.WriteLine($"{licensePlateNumber} is checked-in. Please proceed to spot {availableSpotIDs[0]}");
                    this.Prompt("Press enter to continue");
                    break;
                case > 1:
                    do
                    {
                        Console.WriteLine($"Multiple available spots found for that subscriber. Choose which spot (1-{availableSpotIDs.Count} or enter to go back)");
                        this.ListSelection(availableSpotIDs);
                        string spotInput = this.Prompt();

                        if (spotInput.Equals(""))
                            return;

                        if (int.TryParse(spotInput, out int input))
                        {
                            if (input >= 1 && input <= availableSpotIDs.Count)
                            {
                                this.Core.CheckInSpot(availableSpotIDs[input - 1], licensePlateNumber, isSubscriber: true);
                                Console.WriteLine($"{licensePlateNumber} is checked-in. Please proceed to spot {availableSpotIDs[input - 1]}");
                                this.Prompt("Press enter to continue");
                                return;
                            }
                        }
                        Console.WriteLine("Invalid selection. Try again.");
                    } while (true);
            }
        }

        public void nonSubscriberCheckIn(string licensePlateNumber)
        {
            string spotInput;
            bool isValid;
            do
            {
                Console.WriteLine("Please choose an option (1 or 2) or enter to go back:");
                Console.WriteLine("1. First open spot");
                spotInput = this.Prompt("2. Requested spot");
                isValid = spotInput == "1" || spotInput == "2" || spotInput == "";

                if (!isValid)
                {
                    Console.WriteLine("Not a valid option. Try again.");
                }

            } while (!isValid);

            switch(spotInput)
            {
                case "1":
                    try
                    {
                        string spotID = this.Core.FirstOpenSpotCheckIn(licensePlateNumber);
                        Console.WriteLine($"{licensePlateNumber} is checked-in. Please proceed to spot {spotID}");
                        this.Prompt("Press enter to continue");

                    } catch (InvalidOperationException ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine("Come back later.");
                    }
                    break;

                case "2":
                    this.RequestedSpotCheckIn(licensePlateNumber);
                    break;

                case "":
                    return;
            }
        }

        public void RequestedSpotCheckIn(string licensePlateNumber)
        {
            while (true)
            {
                string requestedSpot = this.Prompt("Enter requested spot or enter to go back:");

                if (requestedSpot == "")
                    return;

                try
                {
                    this.Core.CheckInSpot(requestedSpot, licensePlateNumber);
                    Console.WriteLine($"{licensePlateNumber} is checked-in. Please proceed to spot {requestedSpot}");
                    this.Prompt("Press enter to continue");
                    return;
                }
                catch (InvalidOperationException ex) { Console.WriteLine(ex.Message); }
                catch (KeyNotFoundException ex) { Console.WriteLine(ex.Message); }
                catch (ArgumentException ex) { Console.WriteLine(ex.Message); }
            }
        }

        public void CheckOut()
        {
            do
            {
                Console.WriteLine("CHECKOUT");
                string licensePlateNumber = this.Prompt("Enter license plate number or enter to go back:");

                if (licensePlateNumber.Equals(""))
                {
                    return;
                }
                
                try
                {
                    this.Core.CheckOutSpot(licensePlateNumber);
                    Console.WriteLine($"{licensePlateNumber} has been checked out.");
                    this.Prompt("Press enter to continue.");
                    return;
                }
                catch (KeyNotFoundException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            } while (true);
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
                this.ListSelection(menuOptions);
                input = this.Prompt();

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
                DisplaySubscriberDetails(name: name, email: email, driversLicenseNumber: driversLicenseNumber, licensePlateNumbers: licensePlateNumbers, spotIDs: spotIDs, startDate: subscribedStartDate, endDate: subscribedEndDate, isSubscribed: subscriptionStatus);
                Console.WriteLine();
                Console.WriteLine($"Select an option (1-{menuOptions.Length}) or enter to go back");
                this.ListSelection(menuOptions);
                input = this.Prompt();

                switch (input)
                {
                    case "1":
                        string namePrompt = this.Prompt("Enter name or enter to go back:");
                        if (namePrompt != "")
                        {
                            name = namePrompt;
                        }
                        break;
                    case "2":
                        string emailPrompt = this.Prompt("Enter email or enter to go back:");
                        if (emailPrompt != "")
                        {
                            email = emailPrompt;
                        }
                        break;
                    case "3":
                        string driversLicenseNumberPrompt = this.Prompt("Enter driver's license number or enter to go back:");
                        if (driversLicenseNumberPrompt != "")
                        {
                            driversLicenseNumber = driversLicenseNumberPrompt;
                        }
                        break;
                    case "4":
                        this.ManageLicensePlateNumbers(licensePlateNumbers);
                        break;
                    case "5":
                        if (existingSubscriber is ExpiredSubscriber)
                        {
                            if (this.SaveSubscriber(existingSubscriber, name, email, driversLicenseNumber, licensePlateNumbers, spotIDs))
                                return;
                        } else
                        {
                            this.ManageSpotIDs(spotIDs);
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
                string driversLicenseNumber = this.Prompt("Enter subscriber's driver's license number or enter to go back:");

                if (driversLicenseNumber.Equals(""))
                    return;

                Subscriber? subscriber = this.Core.FindSubscriber(driversLicenseNumber: driversLicenseNumber);

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

        public void ManageLicensePlateNumbers(List<string> licensePlateNumbers)
        {
            string addOrRemove = licensePlateNumbers.Count == 0 ? "1" : this.AddOrRemovePrompt("LICENSE PLATE NUMBER");

            string licensePlateNumber;
            switch (addOrRemove)
            {
                case "1":
                    do
                    {
                        licensePlateNumber = this.Prompt("Enter license plate number or enter to go back:");
                        if (licensePlateNumber.Equals(""))
                            return;

                        try
                        {
                            this.Core.AddLicensePlateNumber(licensePlateNumbers, licensePlateNumber);
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

                        this.ListSelection(licensePlateNumbersSorted);
                        string lpInput = this.Prompt();

                        if (lpInput.Equals(""))
                            return;

                        if (int.TryParse(lpInput, out int input))
                        {
                            if (input >= 1 && input <= licensePlateNumberCount)
                            {
                                licensePlateNumbers.RemoveAll(lp => lp == licensePlateNumbersSorted[input - 1]);
                                return;
                            }
                        }
                        Console.WriteLine("Invalid selection. Try again.");
                    } while(true);
            }
        }

        public void ManageSpotIDs(List<string> spotIDs)
        {
            string addOrRemove = spotIDs.Count == 0 ? "1" : this.AddOrRemovePrompt("SPOT ID");

            string spotID;
            switch (addOrRemove)
            {
                case "1":
                    do
                    {
                        spotID = this.Prompt("Enter spot ID or enter to go back:");

                        if (spotID.Equals(""))
                            return;

                        try
                        {
                            this.Core.AddSpotID(spotIDs, spotID);
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

                        this.ListSelection(spotIDsSorted);
                        string spotIDInput = this.Prompt();

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

        public string AddOrRemovePrompt(string message)
        {
            string addOrRemove;
            bool validInput = false;
            do
            {
                Console.WriteLine(message);
                Console.WriteLine("Choose an option (1/2):");
                Console.WriteLine("1. Add");
                Console.WriteLine("2. Remove");;
                addOrRemove = this.Prompt("Enter to go back");

                validInput = addOrRemove == "1" || addOrRemove == "2" || addOrRemove == "";

                if (!validInput)
                {
                    Console.WriteLine("Not a valid input. Try again.");
                }

            } while (!validInput);
            
            return addOrRemove;
        }

        public bool SaveSubscriber(Subscriber? existingSubscriber, string name, string email, string driversLicenseNumber, List<string> licensePlateNumbers, List<string> spotIDs)
        {
            if (name == "" || email == "" || driversLicenseNumber == "" || licensePlateNumbers.Count == 0 || (existingSubscriber is ActiveSubscriber && spotIDs.Count == 0))
            {
                Console.WriteLine("A field is empty. Ensure all fields have a value and try again.");
                this.Prompt("Press enter to continue.");
                return false;
            }
            else
            {
                try
                {
                    DateOnly startDate = existingSubscriber?.SubscribeStartDate ?? DateOnly.FromDateTime(DateTime.Now);
                    if (existingSubscriber == null)
                    {
                        this.Core.AddSubscriber(startDate, driversLicenseNumber, name, email, licensePlateNumbers: licensePlateNumbers, spotIDs: spotIDs);
                        Console.WriteLine("Subscriber successfully added.");
                    }
                    else if (existingSubscriber is ActiveSubscriber activeSub)
                    {
                        this.Core.UpdateSubscriber(activeSub, name, email, driversLicenseNumber, licensePlateNumbers, spotIDs);
                        Console.WriteLine("Active subscriber successfully updated");
                    }
                    else if (existingSubscriber is ExpiredSubscriber expiredSub)
                    {
                        this.Core.UpdateSubscriber(expiredSub, name, email, driversLicenseNumber, licensePlateNumbers);
                        Console.WriteLine("Expired subscriber successfully updated.");
                    }
                    this.Prompt("Press enter to continue");
                    return true;
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine(ex.Message);
                    Prompt("Press enter to continue.");
                    return false;
                }
                catch (KeyNotFoundException ex)
                {
                    Console.WriteLine(ex.Message);
                    Prompt("Press enter to continue.");
                    return false;
                }
            }
        }

        public void ReactivateSubscriber()
        {
            do
            {
                Console.WriteLine("REACTIVATE SUBSCRIBER");
                string driversLicenseNumber = this.Prompt("Enter driver's license number or enter to go back:");

                 if (driversLicenseNumber.Equals(""))
                    return;

                Subscriber? subscriber = this.Core.FindSubscriber(driversLicenseNumber: driversLicenseNumber);
                
                if (subscriber == null)
                {
                    Console.WriteLine("That driver's license number is not in the system. Add them as a subscriber.");
                    this.Prompt("Press enter to continue");
                    continue;
                }

                if (subscriber is not ExpiredSubscriber expiredSubscriber)
                {
                    Console.WriteLine("Already an active subscriber.");
                    this.Prompt("Press enter to continue");
                    continue;
                }

                if (this.Core.SubscriberHasCheckedInVehicle(subscriber))
                {
                    Console.WriteLine("A vehicle on the account is checked in. Check the vehicle out and try again.");
                    this.Prompt("Press enter to continue");
                    continue;
                }

                do
                {
                    this.DisplaySubscriberDetails(subscriber);

                    Console.WriteLine("NOTICE: Once reactivated spot IDs must be added through the 'Manage subscribers' -> 'Edit subscriber details' menu.");
                    string input = this.Prompt("Are you sure you want to reactivate this subscriber? (y/n) or enter to go back:").ToLower();

                    if (input.Equals("") || input.Equals("n"))
                        break;

                    if (input.Equals("y"))
                    {
                        this.Core.ReactivateSubscriber(expiredSubscriber);
                        Console.WriteLine("Subscriber successfully reactivated.");
                        this.Prompt("Press enter to continue");
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
                string driversLicenseNumber = this.Prompt("Enter driver's license number or enter to go back:");

                if (driversLicenseNumber.Equals(""))
                    return;

                Subscriber? subscriber = this.Core.FindSubscriber(driversLicenseNumber: driversLicenseNumber);
                
                if (subscriber == null)
                {
                    Console.WriteLine("That driver's license number is not a subscriber. Try again.");
                    this.Prompt("Press enter to continue");
                    continue;
                }

                if (this.Core.SubscriberHasCheckedInVehicle(subscriber))
                {
                    Console.WriteLine("That subscriber has a checked-in vehicle. Check the vehicle out and try again.");
                    this.Prompt("Press enter to continue");
                    continue;
                }

                if (subscriber is not ActiveSubscriber activeSubscriber)
                {
                    Console.WriteLine("Already an expired subscriber.");
                    this.Prompt("Press enter to continue");
                    continue;
                }

                do
                {
                    this.DisplaySubscriberDetails(subscriber);

                    string input = this.Prompt("Are you sure you want to expire this subscriber? (y/n) or enter to go back:").ToLower();

                    if (input.Equals("") || input.Equals("n"))
                        break;

                    if (input.Equals("y"))
                    {
                        this.Core.ExpireSubscriber(activeSubscriber);
                        Console.WriteLine("Subscriber successfully expired.");
                        this.Prompt("Press enter to continue");
                        return;
                    }
                    
                    Console.WriteLine("Invalid selection. Try again.");
                } while(true);
            } while (true);
        }
    }
}