using System.Collections.Generic;

namespace Lox
{
    public interface ICallable
    {
        int Arity { get; }

        object Call(Interpreter interpreter, IList<object> arguments); 
    }
}