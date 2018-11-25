using System.Collections.Generic;
using System.Linq;

namespace Lox
{
    public class Function : ICallable
    {
        private readonly FunctionStmt _declaration;

        private readonly Environment _closure;

        private readonly bool _isInitializer;

        public int Arity { get => _declaration.Params.Count; }

        public bool IsInitializer { get => _isInitializer; }

        public Function(FunctionStmt declaration, Environment closure, bool isInitializer = false)
        {
            _declaration = declaration;
            _closure = closure;
            _isInitializer = isInitializer;
        }

        public object Call(Interpreter interpreter, IList<object> arguments)
        {
            var environment = new Environment(_closure);

            var paramArgPairs = _declaration.Params
                .Zip(arguments, (parameter, argument) => new { Parameter = parameter, Argument = argument })
                .ToList();

            foreach (var call in paramArgPairs)
                environment.Define(call.Parameter.Lexeme, call.Argument);

            var shouldContinue = false;
            var shouldBreak = false;

            try 
            {
                interpreter.BlockStmt(_declaration.Body, environment, ref shouldBreak, ref shouldContinue);
            }
            catch (Return returnValue)
            {
                if (IsInitializer)
                    return _closure.GetAt(0, "this");

                return returnValue.Value;
            }

            if (IsInitializer)
                return _closure.GetAt(0, "this");

            return null;
        }

        public Function Bind(Instance instance)
        {
            var environment = new Environment(_closure);
            environment.Define("this", instance);

            return new Function(_declaration, environment, IsInitializer);
        }

        public override string ToString() => $"<fn {_declaration.Name?.Lexeme ?? "anonymous"}>";
    }
}