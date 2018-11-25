using System.Collections.Generic;

namespace Lox
{
    public class Instance
    {
        private readonly Class _class;

        private readonly IDictionary<string, object> _fields = new Dictionary<string, object>();

        public Instance(Class klass)
        {
            _class = klass;
        }

        public object Get(Token name)
        {
            if (_fields.TryGetValue(name.Lexeme, out var value))
                return value; 

            var method = _class.FindMethod(this, name.Lexeme);
            if (method != null)
                return method;

            throw new RuntimeError(name, $"Undefined property '{name.Lexeme}'.");
        }

        public void Set(Token name, object value) => _fields[name.Lexeme] = value;
        
        public override string ToString() => $"{_class.Name} instance";
    }
}