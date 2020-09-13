using System;
using System.Collections.Generic;
using System.Text;

namespace MGTModel.Helpers
{
    public class PayloadResult
    {
        public bool Succeeded { get; set; }
        public List<MessageItem> Messages { get; set; }

        public static PayloadResult ResultGood(List<MessageItem> Messages)
        {
            return new PayloadResult
            {
                Succeeded = true,
                Messages = Messages
            };
        }

        public static PayloadResult ResultBad(List<MessageItem> Messages)
        {
            return new PayloadResult
            {
                Succeeded = false,
                Messages = Messages
            };
        }
    }
}
