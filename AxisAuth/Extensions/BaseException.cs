using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AxisAuth.Extensions
{
    /// <summary>
    /// Custom exception that extends Exception
    /// </summary>
    public class BaseException : Exception
    {
        public BaseException(string message) : base(message)
        {

        }
    }
}
