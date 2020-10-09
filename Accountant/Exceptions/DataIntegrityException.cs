using System;
using System.Collections.Generic;
using System.Text;

namespace Accountant.Exceptions
{
    public sealed class DataIntegrityException : Exception
    {
        public DataIntegrityException(string message) : base(message)
        {
        }
    }
}
