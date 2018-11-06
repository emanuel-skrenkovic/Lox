using System;

namespace Lox
{
    public static class Lox
    {
        public static void RuntimeError(RuntimeError error)
        {
            Report(error.Token.Line, "", error.Message);
        }

        public static void Error(Token token, string message)
        {
            if (token.Type == TokenType.EOF)
                Report(token.Line, " at end", message);                
            else
                Report(token.Line, $" at '{token.Lexeme}'", message);
        }
        
        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        public static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine($"[line {line} Error{where}: {message}");

            // _hadError = true;
        }
    }
}