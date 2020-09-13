using System;
using System.Collections.Generic;
using System.Text;

namespace MGTModel.Helpers
{
    public class MessageItem
    {
        public string Code { get; set; } // WrongConfirmation
        public string Message { get; set; } // Message to translate: ORDER_NOT_FOUND
        public string Detail { get; set; } // Detail to transaction: Order not found
    }
}
