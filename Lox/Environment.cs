using System.Collections.Generic;

namespace Lox
{
    public class Environment
    {
        private readonly IDictionary<string, object> _values;

        public IDictionary<string, object> Values { get => _values; }

        public Environment()
        {
            _values = new Dictionary<string, object>();
        }

        public void Define(string name, object value)
        {
            Values[name] = value;
        }

        public void Assign(Token name, object value)
        {
            if (!Values.ContainsKey(name.Lexeme))
                throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");

            Values[name.Lexeme] = value; 
        }

        public object Get(Token name)
        {
            if (Values.ContainsKey(name.Lexeme))
                return Values[name.Lexeme];

            throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
        }
    }
}