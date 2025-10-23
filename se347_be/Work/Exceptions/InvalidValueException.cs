using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace se347_be.Exceptions
{
    public class InvalidValueException : Exception
    {
        public InvalidValueException(string message) : base(message)
        {
        }
    }
}