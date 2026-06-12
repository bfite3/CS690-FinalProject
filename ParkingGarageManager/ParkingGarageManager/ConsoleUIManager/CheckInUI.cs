using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingGarageManager.ConsoleUIManager
{
    public class CheckInUI
    {
        public ConsoleUI ConsoleUI { get; private set; }

        public CheckInUI(ConsoleUI consoleUI)
        {
            this.ConsoleUI = consoleUI;
        }

        public void CheckIn()
        {
            do
            {
                Console.WriteLine("CHECK IN");
                string? licensePlateNumber = this.ConsoleUI.Prompt("Enter license plate number or enter to go back:");

                if (licensePlateNumber.Equals(""))
                {
                    return;
                }

                if (this.ConsoleUI.Core.CheckInManager.VehicleAlreadyCheckedIn(licensePlateNumber))
                {
                    Console.WriteLine("That vehicle is already checked in.");
                    continue;
                }

                Subscriber? subscriber = this.ConsoleUI.Core.SubscriberManager.FindSubscriber(licensePlateNumber: licensePlateNumber);

                if (subscriber is ActiveSubscriber activeSubscriber)
                {
                    this.subscriberCheckIn(activeSubscriber, licensePlateNumber);
                    return;
                }
                else
                {
                    if (this.ConsoleUI.Core.SpotManager.IsGarageFull()) {
                        Console.WriteLine("The garage is full. Please come back later.");
                        this.ConsoleUI.Prompt("Press enter to continue");
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
            List <string> availableSpotIDs = subscriber.SpotIDs.Where(spot => this.ConsoleUI.Core.ParkingGarage.Spots[spot] is not TakenSpot takenSpot).OrderBy(s => s).ToList();

            switch(availableSpotIDs.Count)
            {
                case 0:
                    Console.WriteLine("No available reserved spots open for that subscriber.");
                    this.ConsoleUI.Prompt("Press enter to continue");
                    break;
                case 1:
                    this.ConsoleUI.Core.CheckInManager.CheckInSpot(availableSpotIDs[0], licensePlateNumber, isSubscriber: true);
                    Console.WriteLine($"{licensePlateNumber} is checked-in. Please proceed to spot {availableSpotIDs[0]}");
                    this.ConsoleUI.Prompt("Press enter to continue");
                    break;
                case > 1:
                    do
                    {
                        Console.WriteLine($"Multiple available spots found for that subscriber. Choose which spot (1-{availableSpotIDs.Count} or enter to go back)");
                        this.ConsoleUI.ListSelection(availableSpotIDs);
                        string spotInput = this.ConsoleUI.Prompt();

                        if (spotInput.Equals(""))
                            return;

                        if (int.TryParse(spotInput, out int input))
                        {
                            if (input >= 1 && input <= availableSpotIDs.Count)
                            {
                                this.ConsoleUI.Core.CheckInManager.CheckInSpot(availableSpotIDs[input - 1], licensePlateNumber, isSubscriber: true);
                                Console.WriteLine($"{licensePlateNumber} is checked-in. Please proceed to spot {availableSpotIDs[input - 1]}");
                                this.ConsoleUI.Prompt("Press enter to continue");
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
                spotInput = this.ConsoleUI.Prompt("2. Requested spot");
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
                        string spotID = this.ConsoleUI.Core.CheckInManager.FirstOpenSpotCheckIn(licensePlateNumber);
                        Console.WriteLine($"{licensePlateNumber} is checked-in. Please proceed to spot {spotID}");
                        this.ConsoleUI.Prompt("Press enter to continue");

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
                string requestedSpot = this.ConsoleUI.Prompt("Enter requested spot or enter to go back:");

                if (requestedSpot == "")
                    return;

                try
                {
                    this.ConsoleUI.Core.CheckInManager.CheckInSpot(requestedSpot, licensePlateNumber);
                    Console.WriteLine($"{licensePlateNumber} is checked-in. Please proceed to spot {requestedSpot}");
                    this.ConsoleUI.Prompt("Press enter to continue");
                    return;
                }
                catch (InvalidOperationException ex) { Console.WriteLine(ex.Message); }
                catch (KeyNotFoundException ex) { Console.WriteLine(ex.Message); }
                catch (ArgumentException ex) { Console.WriteLine(ex.Message); }
            }
        }
    }
}