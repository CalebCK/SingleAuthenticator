using System;

namespace AxisAccounting.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }

    public class ErrorResponseViewModel
    {
        public string Error { get; set; }
    }
}
