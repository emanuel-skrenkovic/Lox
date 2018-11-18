using System;
using System.Collections.Generic;

namespace Lox
{
    public class Clock : ICallable
    {
        public int Arity { get => 0; }        

        public object Call(Interpreter interpreter, IList<object> arguments) => (double)DateTime.Now.Millisecond;

        public override string ToString() => "<native fn>";
    }
}