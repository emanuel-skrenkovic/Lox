using System;

namespace Lox
{
    public class RuntimeError : Exception
    {
        private readonly Token _token; 

        public Token Token { get => _token; }

        public RuntimeError(Token token, string message) : base(message)
        {
            _token = token;
        }
    }
}