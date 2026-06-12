using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.Swift;
using System.Threading.Tasks;
using ParkingGarageManager.ConsoleUIManager;

namespace ParkingGarageManager
{
    public class ConsoleUI
    {
        public Core Core { get; private set; }
        public CheckInUI CheckInUI { get; private set; }
        public CheckOutUI CheckOutUI { get; private set; }
        public SubscriberUI SubscriberUI { get; private set; }
        public VehicleStatusUI VehicleStatusUI { get; private set; }
        public ReportManager ReportManager { get; private set; }
        public ConsoleUI()
        {   
            this.Core = new Core();
            this.CheckInUI = new CheckInUI(this);
            this.CheckOutUI = new CheckOutUI(this);
            this.SubscriberUI = new SubscriberUI(this);
            this.VehicleStatusUI = new VehicleStatusUI(this);
            this.ReportManager = new ReportManager(this.Core);
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
                new DataSaver(this.Core).SaveAll();
                Console.WriteLine("Data saved!");
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
                if (this.Core.SpotManager.IsGarageFull())
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
                    this.CheckInUI.CheckIn();
                    break;
                case "2":
                    this.CheckOutUI.CheckOut();
                    break;
                case "3":
                    this.VehicleStatusUI.CheckVehicleStatus();
                    break;
                case "4":
                    string manageSubscribersInput = this.SubscriberUI.ManageSubscribersInput();
                    this.SubscriberUI.ManageSubscribers(manageSubscribersInput);
                    break;
                case "5":
                    this.ReportManager.PrintEODReport();
                    this.Prompt("Press enter to continue.");
                    break;
                default:
                    Console.WriteLine("Not a valid input. Try again.");
                    break;
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

        public static void RaiseGate()
        {
            Console.WriteLine("The gate is raised.");
            Console.WriteLine("Have a wonderful day!");
        }
    }
}