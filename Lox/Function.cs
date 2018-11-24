using System.Collections.Generic;
using System.Linq;

namespace Lox
{
    public class Function : ICallable
    {
        private readonly FunctionStmt _declaration;

        private readonly Environment _closure;

        public int Arity { get => _declaration.Params.Count; }

        public Function(FunctionStmt declaration, Environment closure)
        {
            _declaration = declaration;
            _closure = closure;
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
                return returnValue.Value;
            }

            return null;
        }

        public override string ToString() => $"<fn {_declaration.Name?.Lexeme ?? "anonymous"}>";
    }
}