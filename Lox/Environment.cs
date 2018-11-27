using System.Collections.Generic;

namespace Lox
{
    public class Environment
    {
        private readonly Environment _enclosing;

        private readonly IDictionary<string, object> _values;

        public Environment Enclosing { get => _enclosing; }

        public IDictionary<string, object> Values { get => _values; }

        public object this[Token key] { get => Get(key); }

        public Environment() : this(null)
        {
        }

        public Environment(Environment enclosing)
        {
            _values = new Dictionary<string, object>();

            _enclosing = enclosing;
        }

        public void Define(string name, object value) => Values[name] = value;

        public void Assign(Token name, object value)
        {
            if (Values.ContainsKey(name.Lexeme))
            {
                Values[name.Lexeme] = value; 
                return;
            }

            if (_enclosing != null)
            {
                _enclosing.Assign(name, value);
                return;
            }

            throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
        }

        public void AssignAt(int distance, Token name, object value) => Ancestor(distance).Values[name.Lexeme] = value; // not safe

        public object Get(Token name)
        {
            if (Values.ContainsKey(name.Lexeme))
            {
                if (Values[name.Lexeme] == null)
                    throw new RuntimeError(name, $"Attempting to access unassigned variable '{name.Lexeme}'.");

                return Values[name.Lexeme];
            }

            if (_enclosing != null)
                return _enclosing[name];

            throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
        }

        public object GetAt(int distance, string name)
        {
            var ancestor = Ancestor(distance);

            ancestor.Values.TryGetValue(name, out var value);

            return value;
        } 

        public Environment Ancestor(int distance)
        {
            var environment = this;

            for (int i = 0; i < distance; i++)
                environment = environment._enclosing;

            return environment;
        }
    }
}