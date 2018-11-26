using System;
using System.Collections.Generic;

namespace Lox
{
    public class Scanner
    {
        private static readonly IDictionary<string, TokenType> _keywords = new Dictionary<string, TokenType>
        {
            ["and"] = TokenType.AND,
            ["class"] = TokenType.CLASS,
            ["else"] = TokenType.ELSE,
            ["false"] = TokenType.FALSE,
            ["for"] = TokenType.FOR,
            ["fun"] = TokenType.FUN,
            ["if"] = TokenType.IF,
            ["nil"] = TokenType.NIL,
            ["or"] = TokenType.OR,
            ["print"] = TokenType.PRINT,
            ["return"] = TokenType.RETURN,
            ["super"] = TokenType.SUPER,
            ["this"] = TokenType.THIS,
            ["true"] = TokenType.TRUE,
            ["var"] = TokenType.VAR,
            ["while"] = TokenType.WHILE,
            ["break"] = TokenType.BREAK,
            ["continue"] = TokenType.CONTINUE,
            ["static"] = TokenType.STATIC,
        };

        private readonly IDictionary<char, Action> _scanActions;

        private readonly string _source;

        private readonly IList<Token> _tokens = new List<Token>();

        private int _start = 0;
        private int _current = 0;
        private int _line = 1;

        private static bool _hadError = false;

        public IList<Token> Tokens { get => _tokens; }

        public Scanner(string source)
        {
            _source = source;
            _hadError = false;

            _scanActions = new Dictionary<char, Action>
            {
                ['('] = LeftParen,
                [')'] = RightParen,
                ['{'] = LeftBrace,
                ['}'] = RightBrace,
                [','] = Comma,
                ['.'] = Dot,
                ['-'] = Minus,
                ['+'] = Plus,
                [';'] = Semicolon,
                ['*'] = Star,
                ['!'] = Bang,
                ['='] = Equals,
                ['<'] = LessThan,
                ['>'] = GreaterThan,
                ['/'] = Slash,
                [' '] = Space,
                ['\r'] = CarriageReturn,
                ['\t'] = Tab,
                ['\n'] = Newline,
                ['"'] = String,
                ['?'] = QuestionMark,
                [':'] = Colon
            };
        }

        public void ScanTokens(string source)
        {
            if (_hadError) 
                System.Environment.Exit(65);

            while (!IsAtEnd())
            {
                _start = _current;
                ScanToken();
            }

            _tokens.Add(new Token(TokenType.EOF, "", null, _line));
        }

        private void ScanToken()
        {
            var c = Advance();

            if (_scanActions.TryGetValue(c, out var scan))
                scan();
            else
            {
                if (Char.IsDigit(c))
                    Number();
                else if (char.IsLetterOrDigit(c))
                    Identifier();
                else
                    Lox.Error(_line, "Unexpected character");
            }
        }

        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }        

        private void AddToken(TokenType type, object literal)
        {
            var text = _source.Substring(_start, _current - _start);
            _tokens.Add(new Token(type, text, literal, _line));
        }

        private void LeftParen() => AddToken(TokenType.LEFT_PAREN);
        
        private void RightParen() => AddToken(TokenType.RIGHT_PAREN);

        private void LeftBrace() => AddToken(TokenType.LEFT_BRACE);

        private void RightBrace() => AddToken(TokenType.RIGHT_BRACE);

        private void Comma() => AddToken(Match('=') ? TokenType.COMMA_EQUAL : TokenType.COMMA);

        private void Dot() => AddToken(TokenType.DOT);

        private void Minus() => AddToken(TokenType.MINUS);

        private void Plus() => AddToken(TokenType.PLUS);

        private void Semicolon() => AddToken(TokenType.SEMICOLON);

        private void Star() => AddToken(TokenType.STAR);

        private void Bang() => AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);

        private void Equals() => AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);

        private void LessThan() => AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);

        private void GreaterThan() => AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);

        private void CarriageReturn() { }

        private void Tab() { }

        private void Space() { }

        private void Newline() => _line++;

        private void QuestionMark()
        {
            AddToken(TokenType.TERN_THEN);
            Advance();
        }

        private void Colon()
        {
            // TODO clean up
            if (Match('='))
            {
                AddToken(TokenType.COLON_EQUAL);
            }
            else 
            {
                AddToken(TokenType.TERN_ELSE);
                Advance();
            }
        }

        private void Slash()
        {
            if (Match('/'))
            {
                while (Peek() != '\n' && !IsAtEnd())
                    Advance();
            }
            else if (Match('*'))
            {
                while ((Peek() != '*' && PeekNext() != '/') && !IsAtEnd())
                    Advance();

                // lol
                Advance();
                Advance();
            }
            else
                AddToken(TokenType.SLASH);
        }

        private void String()
        {
            while (Peek() != '"' && ! IsAtEnd()) 
            {
                if (Peek() == '\n') 
                    _line++;

                Advance();
            }

            if (IsAtEnd()) 
            {
                Lox.Error(_line, "Error: unterminated string");
                return;
            }

            Advance();

            var value = _source.Substring(_start + 1, _current - _start - 2);
            AddToken(TokenType.STRING, value);
        }

        private void Number()
        {
            while (Char.IsDigit(Peek())) 
                Advance();

            if (Peek() == '.' && Char.IsDigit(PeekNext()))
            {
                Advance();

                while (Char.IsDigit(Peek())) 
                    Advance();
            }

            double.TryParse(_source.Substring(_start, _current - _start), out var num);
            AddToken(TokenType.NUMBER, num);
        }

        private void Identifier()
        {
            while (Char.IsLetterOrDigit(Peek())) 
                Advance();

            var text = _source.Substring(_start, _current - _start).ToLower();

            if (!_keywords.TryGetValue(text, out var type))
                type = TokenType.IDENTIFIER;

            AddToken(type);
        }

        private char Advance()
        {
            _current++;
            return _source[_current - 1];
        }

        private char Peek()
        {
            if (IsAtEnd()) 
                return '\0';

            return _source[_current];
        }

        private char PeekNext()
        {
            if (_current + 1 >= _source.Length) 
                return '\0';

            return _source[_current + 1];
        }

        private bool Match(char expected)
        {
            if (IsAtEnd()) 
                return false;
                
            if (_source[_current] != expected) 
                return false;

            _current++;

            return true;
        }

        private bool IsAtEnd() => _current >= _source.Length;
    }
}