using System;

namespace DomainBase
{
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message)
        {

        }
    }
}
