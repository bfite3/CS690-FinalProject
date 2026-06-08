using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingGarageManager
{
    public class PaymentResult
    {
        public bool IsSuccess { get; private set; }
        public string Message { get; private set; }
        public decimal ChangeDue { get; private set; }

        public PaymentResult(bool isSuccess, string message, decimal changeDue = 0m)
        {
            this.IsSuccess = isSuccess;
            this.Message = message;
            this.ChangeDue = changeDue;
        }
    }
}