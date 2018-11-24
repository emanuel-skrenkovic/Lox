using System;
using System.IO;
using Lox;

namespace ConsoleLox
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please input path to file");
                return;
            }

            var source = File.ReadAllText(args[0]);

            var scanner = new Scanner(source);
            scanner.ScanTokens(source);

            Parser parser = new Parser(scanner.Tokens);
            var stmts = parser.Parse();

            var interpreter = new Interpreter();

            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);

            interpreter.Interpret(stmts);
        }
    }
}
