using System;

namespace Lox
{
    public class Return : Exception
    {
        private readonly object _value;

        public object Value { get => _value; }

        public Return(object value)
        {
            _value = value;
        }
    }
}