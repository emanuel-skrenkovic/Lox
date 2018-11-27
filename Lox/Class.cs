using System.Collections.Generic;

namespace Lox
{
    public class Class : ICallable, IInstance
    {
        private readonly string _name;

        private readonly IDictionary<string, Function> _methods;

        private readonly IDictionary<string, Function> _staticMethods;

        private readonly Instance _instance;

        private readonly Class _superclass;


        public string Name { get => _name; }

        public IDictionary<string, Function> Methods { get => _methods; }

        public IDictionary<string, Function> StaticMethods { get => _staticMethods; }

        public Class(string name, Class superclass, IDictionary<string, Function> methods, IDictionary<string, Function> staticMethods)
        {
            _name = name;
            _methods = methods;
            _staticMethods = staticMethods;
            _superclass = superclass;
            _instance = new Instance(this);
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

            if (_superclass != null)
                return _superclass.FindMethod(instance, name);

            return null;
        }

        public Function FindStaticMethod(string name)
        {
            if (StaticMethods.TryGetValue(name, out var method))
                return method.Bind(this._instance);

            return null;
        }

        public object Get(Token name) 
        {
            if (_instance.Fields.TryGetValue(name.Lexeme, out var value))
                return value;

            var methods = FindStaticMethod(name.Lexeme);
            if (methods != null)
                return methods;

            throw new RuntimeError(name, $"Undefined property '{name.Lexeme}'.");
        }

        public void Set(Token name, object value) => _instance.Set(name, value);

        public override string ToString() => _name;
    }
}