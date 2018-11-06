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

            // args = new string[1];

            // args[0] = @"./TestCases/assign";

            var source = File.ReadAllText(args[0]);

            var scanner = new Scanner(source);
            scanner.ScanTokens(source);

            Parser parser = new Parser(scanner.Tokens);
            var stmts = parser.Parse();
            // Console.WriteLine(AstPrinter.Print(expression));
            var interpreter = new Interpreter();

            interpreter.Interpret(stmts);
        }
    }
}
