using System.Collections.Generic;

namespace Lox
{
    public class Class : ICallable
    {
        private readonly string _name;

        private readonly IDictionary<string, Function> _methods;

        public string Name { get => _name; }

        public IDictionary<string, Function> Methods { get => _methods; }

        public Class(string name, Dictionary<string, Function> methods)
        {
            _name = name;
            _methods = methods;
        }

        public int Arity 
        {
            get 
            {
                if (Methods.TryGetValue("init", out var initializer))
                    return initializer.Arity;
                
                return 0;
            }
        }

        public object Call(Interpreter interpreter, IList<object> arguments)
        {
            var instance = new Instance(this);

            if (Methods.TryGetValue("init", out var initializer))
                initializer.Bind(instance).Call(interpreter, arguments);

            return instance;
        }

        public Function FindMethod(Instance instance, string name)
        {
            if (Methods.TryGetValue(name, out var method))
                return method.Bind(instance);

            return null;
        }

        public override string ToString() => _name;
    }
}