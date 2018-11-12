using System;
using System.Collections.Generic;

namespace Lox
{
    public class Parser
    {
        private class ParseError : Exception {}

        private readonly IList<Token> _tokens; 

        private int _current;

        private bool IsAtEnd 
        {
            get => IsEndOfFile();
        }

        public Parser(IList<Token> tokens)
        {
            _tokens = tokens;
        }

        public IList<Stmt> Parse()
        {
            try 
            {
                var stmts = new List<Stmt>();

                while (!IsAtEnd)
                    stmts.Add(Declaration());

                return stmts;
            }
            catch (ParseError)
            {
                return null;
            }
        }

        private Stmt IfStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
            var condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after 'if' condition.");

            var thenBranch = Statement();

            if (Match(TokenType.ELSE))
            {
                var elseBranch = Statement();
                return new IfStmt(condition, thenBranch, elseBranch);
            }
            
            return new IfStmt(condition, thenBranch);
        }

        private Stmt Declaration()
        {
            if (Check(Peek(), TokenType.IDENTIFIER) && Check(Next(), TokenType.COLON_EQUAL))
                return DeclarationStmt();

            if (Match(TokenType.VAR))
                return VariableStmt();

            return Statement();
        }

        private Stmt DeclarationStmt()
        {
            Advance();

            var name = Previous();

            // advance through ':=' since it was already matched
            Advance();

            Expr initializer = Expression();

            Consume(TokenType.SEMICOLON, "Expect ';' after varaible declaration.");

            return new DeclarationStmt(name, initializer);
        }

        private Stmt VariableStmt()
        {
            var name = Consume(TokenType.IDENTIFIER, "Expect variable name");

            Expr initializer = null;

            if (Match(TokenType.EQUAL))
                initializer = Expression();

            Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");

            return new VariableStmt(name, initializer);
        }

        private Stmt Statement()
        {
            if (Match(TokenType.FOR))
                return ForStatement();

            if (Match(TokenType.IF))
                return IfStatement();

            if (Match(TokenType.PRINT))
                return PrintStatement();

            if (Match(TokenType.WHILE))
                return WhileStatement();
            
            if (Match(TokenType.LEFT_BRACE))
                return BlockStatement();

            return ExpressionStatement();
        }

        private Stmt ForStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");

            Stmt initializer;

            if (Match(TokenType.SEMICOLON))
                initializer = null;
            else if (Match(TokenType.IDENTIFIER) && Check(Peek(), TokenType.COLON_EQUAL))
                initializer = DeclarationStmt();
            else if (Match(TokenType.VAR))
                initializer = VariableStmt();
            else 
                initializer = ExpressionStatement();

            Expr condition = null;
            if (!Check(TokenType.SEMICOLON))
                condition = Expression();

            Consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");

            Expr increment = null;
            if (!Check(TokenType.RIGHT_PAREN))
                increment = Expression();

            Consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses.");

            var body = Statement();

            if (increment != null)
                body = new BlockStmt(new List<Stmt> { body, new ExpressionStmt(increment) });

            if (condition != null)
                body = new WhileStmt(condition, body);

            if (initializer != null)
                body = new BlockStmt(new List<Stmt> { initializer, body });

            return body;
        }

        private PrintStmt PrintStatement()
        {
            var value = Expression();

            Consume(TokenType.SEMICOLON, "Expect ';' after value.");

            return new PrintStmt(value);
        }

        private WhileStmt WhileStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'."); 
            var condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect')' after condition.");

            var body = Statement();

            return new WhileStmt(condition, body);
        }

        private ExpressionStmt ExpressionStatement()
        {
            var expr = Expression();

            Consume(TokenType.SEMICOLON, "Expect ';' after value.");

            return new ExpressionStmt(expr);
        }

        private BlockStmt BlockStatement()
        {
            IList<Stmt> statements = new List<Stmt>();

            while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd)
                statements.Add(Declaration());

            Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");

            return new BlockStmt(statements);
        }

        private Expr Expression() => Assignment();

        private Expr Assignment()
        {
            var expr = LogicOr();

            if (Match(TokenType.EQUAL))
            {
                var equals = Previous();
                var value = Assignment();

                if (expr is Variable var)
                {
                    var name = var.Name;
                    return new Assignment(name, value);
                }

                Error(equals, "Invalid assignment target");
            }

            return expr;
        }

        private Expr LogicOr()
        {
            var expr = LogicAnd();

            while (Match(TokenType.OR))
            {
                var oper = Previous();
                var right = LogicAnd();
                expr = new Logical(expr, oper, right);
            } 

            return expr;
        }

        private Expr LogicAnd()
        {
            var expr = Conditional();

            while (Match(TokenType.AND))
            {
                var oper = Previous();
                var right = Conditional();
                expr = new Logical(expr, oper, right);
            }

            return expr;
        }        

        // private Expr Comma()
        // {
        //     var expr = Conditional();

        //     while (Match(TokenType.COMMA))
        //     {
        //         var oper = Previous();

        //         var right = Conditional();

        //         expr = new Binary(expr, oper, right);
        //     }

        //     return expr;
        // }

        private Expr Conditional()
        {
            var expr = Equality();

            while (Match(TokenType.TERN_THEN))
            {
                var left = Expression();

                if (!Match(TokenType.TERN_ELSE))
                    throw Error(Peek(), "Expect tern else");

                var right = Equality();

                expr = new Ternary(expr, left, right);
            }

            return expr;
        }

        private Expr Equality()
        {
            var expr = Comparison();

            while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                var oper = Previous();
                var right = Comparison();

                expr = new Binary(expr, oper, right);
            }

            return expr;
        }

        private Expr Comparison()
        {
            var expr = Addition();

            while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                var oper = Previous();
                var right = Addition();
                expr = new Binary(expr, oper, right);
            }

            return expr;
        }

        private Expr Addition()
        {
            var expr = Multiplication();

            while (Match(TokenType.PLUS, TokenType.MINUS))
            {
                var oper = Previous();
                var right = Multiplication();
                expr = new Binary(expr, oper, right);
            }

            return expr;
        }

        private Expr Multiplication()
        {
            var expr = Unary();

            while (Match(TokenType.SLASH, TokenType.STAR))
            {
                var oper = Previous();
                var right = Unary();
                expr = new Binary(expr, oper, right);
            }

            return expr;
        }

        private Expr Unary()
        {
            if (Match(TokenType.BANG, TokenType.MINUS))
            {
                var oper = Previous();
                var right = Unary();

                return new Unary(oper, right);
            }

            return Primary();
        }

        private Expr Primary()
        {
            if (Match(TokenType.FALSE)) 
                return new Literal(false);
            if (Match(TokenType.TRUE)) 
                return new Literal(true);
            if (Match(TokenType.NIL))
                return new Literal(null);

            if (Match(TokenType.NUMBER, TokenType.STRING)) 
                return new Literal(Previous().Literal);

            if (Match(TokenType.IDENTIFIER))
                return new Variable(Previous());

            if (Match(TokenType.LEFT_PAREN))
            {
                var expr = Expression();
                Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
                return new Grouping(expr);
            }

            throw Error(Peek(), "Expect expression");
        }

        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd)
            {
                if (Previous().Type == TokenType.SEMICOLON)
                    return;

                switch (Peek().Type)
                {
                    case TokenType.CLASS:
                    case TokenType.FUN:
                    case TokenType.VAR:
                    case TokenType.FOR:
                    case TokenType.IF:
                    case TokenType.WHILE:
                    case TokenType.PRINT:
                    case TokenType.RETURN:
                        return;
                }

                Advance();
            }
        }

        private Token Consume(TokenType type, string message)
        {
            if (Check(type)) 
                return Advance();

            throw Error(Peek(), message);
        }

        private ParseError Error(Token token, string message)
        {
            Lox.Error(token, message);

            return new ParseError();
        }

        private bool Match(params TokenType[] types)
        {
            foreach (var type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        private Token Advance()
        {
            if (!IsAtEnd) 
                _current++;

            return Previous();
        }

        private bool Check(TokenType type) => Check(Peek(), type);

        private bool Check(Token token, TokenType type)
        {
            if (IsAtEnd)
                return false;

            return token.Type == type;
        }

        private Token Peek() => _tokens[_current];

        private Token Previous() => _tokens[_current - 1];

        private Token Next() => _current + 1 > _tokens.Count ? _tokens[_tokens.Count] :_tokens[_current + 1];

        private bool IsEndOfFile() => Peek().Type == TokenType.EOF;
    }
}