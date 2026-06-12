using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingGarageManager.ConsoleUIManager
{
    public class CheckOutUI
    {
        public ConsoleUI ConsoleUI { get; private set; }

        public CheckOutUI(ConsoleUI consoleUI)
        {
            this.ConsoleUI = consoleUI;
        }

        public void CheckOut()
        {
            do
            {
                Console.WriteLine("CHECKOUT");
                string licensePlateNumber = this.ConsoleUI.Prompt("Enter license plate number or enter to go back:");

                if (licensePlateNumber.Equals(""))
                    return;
                
                try
                {
                    var (pendingPayments, pendingVisits, spotID, subscriber) = this.ConsoleUI.Core.CheckOutManager.CheckOutSpot(licensePlateNumber);
                    if (subscriber is ActiveSubscriber activeSubscriber)
                    {
                        this.SubscriberCheckOut(licensePlateNumber, spotID, activeSubscriber);
                    }
                    else
                    {
                        this.NonSubscriberCheckOut(pendingPayments, pendingVisits, spotID);
                    }
                    
                    this.ConsoleUI.Prompt("Press enter to continue.");
                    return;
                }
                catch (KeyNotFoundException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            } while (true);
        }

        public void SubscriberCheckOut(string licensePlateNumber, string spotID, ActiveSubscriber activeSubscriber)
        {
            this.ConsoleUI.Core.SpotManager.RestoreSpot(spotID, activeSubscriber);
            Console.WriteLine($"{licensePlateNumber} has been checked out.");
            Console.WriteLine($"{licensePlateNumber} is a subscriber. Monthly payment cleared.");
            ConsoleUI.RaiseGate();
        }

        public void NonSubscriberCheckOut(List<PendingPayment> pendingPayments, List<Visit> pendingVisits, string spotID)
        {
            bool paymentSuccessful = this.ProcessPayment(pendingPayments, pendingVisits);

            if (paymentSuccessful)
            {
                this.ConsoleUI.Core.SpotManager.RestoreSpot(spotID);
                ConsoleUI.RaiseGate();
            }
            else
            {   
                int iLastIndex = pendingVisits.Count - 1;
                pendingVisits[iLastIndex].ResetLeaveTime();
                this.ConsoleUI.Core.PaymentManager.RefundPayments(pendingPayments);
                Console.WriteLine("Payment cancelled. All amounts have been refunded. The most recent visit Leave Time has been reset.");
                Console.WriteLine("The vehicle remains checked in.");
            }  
        }

        public bool ProcessPayment(List<PendingPayment> pendingPayments, List<Visit> pendingVisits)
        {
            int consoleWidth = Console.WindowWidth > 0 ? Console.WindowWidth : 80;
            string divider = new string('=', consoleWidth);
            string separator = new string('-', consoleWidth);

            for (int i = 0; i < pendingPayments.Count; i++)
            {
                do
                {
                    decimal amountOwed = pendingPayments[i].AmountOwed;
                    decimal amountPaid = pendingPayments[i].AmountPaid;
                    decimal remainingBalance = amountOwed - amountPaid;

                    Console.WriteLine(divider);
                    Console.WriteLine($"PAYMENT {i + 1} OF {pendingPayments.Count}".PadLeft((consoleWidth + 13) / 2));
                    Console.WriteLine(divider);
                    Console.WriteLine($"  Entry Time:        {pendingVisits[i].EntryTime:yyyy-MM-dd HH:mm:ss}");
                    Console.WriteLine($"  Leave Time:        {pendingVisits[i].LeaveTime:yyyy-MM-dd HH:mm:ss}");
                    Console.WriteLine($"  Total Hours:       {pendingVisits[i].TotalHours}");
                    Console.WriteLine(separator);
                    Console.WriteLine($"  Amount Owed:       {amountOwed:C}");
                    Console.WriteLine($"  Amount Paid:       {amountPaid:C}");
                    Console.WriteLine($"  Remaining Balance: {remainingBalance:C}");
                    Console.WriteLine(divider);

                    string amountPaidPrompt = this.ConsoleUI.Prompt("Enter amount received from customer (0.00) or enter to cancel:");

                    if (amountPaidPrompt.Equals(""))
                        return false;

                    if (!decimal.TryParse(amountPaidPrompt, out decimal vAmountPaid))
                    {
                        Console.WriteLine("Please enter a valid USD currency amount. (0.00)");
                        continue;
                    }

                    PaymentResult paymentResult = this.ConsoleUI.Core.PaymentManager.ProcessPayment(pendingPayments[i], vAmountPaid);
                    if (paymentResult.IsSuccess)
                    {
                        Console.WriteLine(divider);
                        Console.WriteLine(paymentResult.Message);
                        if (paymentResult.ChangeDue > 0)
                            Console.WriteLine($"  Change Due:        {paymentResult.ChangeDue:C}");
                        Console.WriteLine(divider);
                        break;
                    }
                    else
                    {
                        Console.WriteLine(paymentResult.Message);
                    }
                } while (true);
            }

            return true;
        }
    }
}