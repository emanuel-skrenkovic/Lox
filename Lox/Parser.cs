using System;
using System.Collections.Generic;
using System.Linq;

namespace Lox
{
    public class Parser
    {
        private class ParseError : Exception {}

        private readonly IList<Token> _tokens; 

        private int _current;

        private bool IsAtEnd  { get => IsEndOfFile(); }

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

        private Stmt IfStatement(bool canBreak = false)
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
            var condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after 'if' condition.");

            var thenBranch = Statement(canBreak);

            if (Match(TokenType.ELSE))
            {
                var elseBranch = Statement(canBreak);
                return new IfStmt(condition, thenBranch, elseBranch);
            }
            
            return new IfStmt(condition, thenBranch);
        }

        private Stmt ReturnStatement(bool canBreak = false)
        {
            var keyword = Previous();

            Expr value = null;
            if (!Check(TokenType.SEMICOLON))
                value = Expression();

            Consume(TokenType.SEMICOLON, "EXPECT ';' after return value"); 

            return new ReturnStmt(keyword, value);
        }

        private Stmt Declaration(bool canBreak = false)
        {
            if (Match(TokenType.CLASS))
                return ClassDeclaration();

            if (Match(TokenType.FUN))
                return FunctionStmt("function");

            if (Check(Peek(), TokenType.IDENTIFIER) && Check(Next(), TokenType.COLON_EQUAL))
                return DeclareAssignStmt();

            if (Match(TokenType.VAR))
                return VariableStmt();

            return Statement(canBreak);
        }

        private Stmt ClassDeclaration()
        {
            var name = Consume(TokenType.IDENTIFIER, "Expect class name.");
            Consume(TokenType.LEFT_BRACE, "Expect '{' before class body.");

            var methods = new List<FunctionStmt>();

            while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd)
                methods.Add((FunctionStmt)FunctionStmt("method"));

            Consume(TokenType.RIGHT_BRACE, "Expect '}' after class body.");

            return new ClassStmt(name, null, methods);
        }

        private Stmt FunctionStmt(string kind)
        {
            var name = Consume(TokenType.IDENTIFIER, $"Expect {kind} name.");

            Consume(TokenType.LEFT_PAREN, $"Expect '(' after {kind} name.");

            var parameters = new List<Token>();

            if (!Check(TokenType.RIGHT_PAREN))
            {
                do 
                {
                    if (parameters.Count >= 8)
                        Error(Peek(), "Cannot have more than 8 parameters.");

                    parameters.Add(Consume(TokenType.IDENTIFIER, "Expect parameter name.")); 
                } while (Match(TokenType.COMMA));
            }

            Consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");

            Consume(TokenType.LEFT_BRACE, $"Expect '{TokenType.LEFT_BRACE}' before {kind} body.");
            var body = BlockStatement();

            return new FunctionStmt(name, parameters, body);
        }

        private Stmt DeclareAssignStmt()
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

        private Stmt Statement(bool canBreak = false)
        {
            if (Match(TokenType.FOR))
                return ForStatement(canBreak);

            if (Match(TokenType.IF))
                return IfStatement(canBreak);

            if (Match(TokenType.RETURN))
                return ReturnStatement(canBreak);

            if (Match(TokenType.PRINT))
                return PrintStatement(canBreak);

            if (Match(TokenType.WHILE))
                return WhileStatement(canBreak);

            if (Match(TokenType.CONTINUE))
                return LoopControlStatement(canBreak);

            if (Match(TokenType.BREAK))
                return LoopControlStatement(canBreak);
            
            if (Match(TokenType.LEFT_BRACE))
                return BlockStatement(canBreak);

            return ExpressionStatement(canBreak);
        }

        private Stmt ForStatement(bool canBreak = false)
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");

            Stmt initializer;

            if (Match(TokenType.SEMICOLON))
                initializer = null;
            else if (Match(TokenType.IDENTIFIER) && Check(Peek(), TokenType.COLON_EQUAL))
                initializer = DeclareAssignStmt();
            else if (Match(TokenType.VAR))
                initializer = VariableStmt();
            else 
                initializer = ExpressionStatement(canBreak);

            Expr condition = null;
            if (!Check(TokenType.SEMICOLON))
                condition = Expression();

            Consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");

            Expr increment = null;
            if (!Check(TokenType.RIGHT_PAREN))
                increment = Expression();

            Consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses.");

            var body = Statement(canBreak);

            if (increment != null)
                body = new BlockStmt(new List<Stmt> { body, new ExpressionStmt(increment) });

            if (condition != null)
                body = new WhileStmt(condition, body);

            if (initializer != null)
                body = new BlockStmt(new List<Stmt> { initializer, body });

            return body;
        }

        private PrintStmt PrintStatement(bool canBreak = false)
        {
            var value = Expression();

            Consume(TokenType.SEMICOLON, "Expect ';' after value.");

            return new PrintStmt(value);
        }

        private WhileStmt WhileStatement(bool canBreak = false)
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'."); 
            var condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect')' after condition.");

            var body = Statement(true);

            return new WhileStmt(condition, body);
        }

        private LoopControlStmt LoopControlStatement(bool canBreak = false)
        {
            var oper = Previous();

            Consume(TokenType.SEMICOLON, "Expect ';' after break statement.");

            if (!canBreak)
                Error(Previous(), "Cannot break outside of loops.");
            
            return new LoopControlStmt(oper);
        }

        private ExpressionStmt ExpressionStatement(bool canBreak = false)
        {
            var expr = Expression();

            Consume(TokenType.SEMICOLON, "Expect ';' after value.");

            return new ExpressionStmt(expr);
        }

        private BlockStmt BlockStatement(bool canBreak = false)
        {
            IList<Stmt> statements = new List<Stmt>();

            while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd)
                statements.Add(Declaration(canBreak));

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

                if (expr is VariableExpr var)
                {
                    var name = var.Name;
                    return new AssignmentExpr(name, value);
                }
                else if (expr is GetExpr get)
                    return new SetExpr(get.Object, get.Name, value);

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
                expr = new LogicalExpr(expr, oper, right);
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
                expr = new LogicalExpr(expr, oper, right);
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

                expr = new TernaryExpr(expr, left, right);
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

                expr = new BinaryExpr(expr, oper, right);
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
                expr = new BinaryExpr(expr, oper, right);
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
                expr = new BinaryExpr(expr, oper, right);
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
                expr = new BinaryExpr(expr, oper, right);
            }

            return expr;
        }

        private Expr Unary()
        {
            if (Match(TokenType.BANG, TokenType.MINUS))
            {
                var oper = Previous();
                var right = Unary();

                return new UnaryExpr(oper, right);
            }

            return Call();
        }

        private Expr Call()
        {
            var expr = Primary();

            while (true)
            {
                if (Match(TokenType.LEFT_PAREN))
                    expr = FinishCall(expr);
                else if (Match(TokenType.DOT))
                {
                    var name = Consume(TokenType.IDENTIFIER, "Expect property name after '.'.");
                    expr = new GetExpr(expr, name);
                }
                else
                    break;
            }

            return expr;
        }

        private Expr FinishCall(Expr callee)
        {
            IList<Expr> arguments = new List<Expr>();

            if (!Check(TokenType.RIGHT_PAREN))
            {
                do 
                {
                    if (arguments.Count >= 8)
                        Error(Peek(), "Cannot have more than 8 arguments.");

                    arguments.Add(Expression());
                } while (Match(TokenType.COMMA));
            }

            var paren = Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments.");

            return new CallExpr(callee, paren, arguments);
        }

        private Expr Primary()
        {
            if (Match(TokenType.FALSE)) 
                return new LiteralExpr(false);
            if (Match(TokenType.TRUE)) 
                return new LiteralExpr(true);
            if (Match(TokenType.NIL))
                return new LiteralExpr(null);

            if (Match(TokenType.NUMBER, TokenType.STRING)) 
                return new LiteralExpr(Previous().Literal);

            if (Match(TokenType.THIS))
                return new ThisExpr(Previous());

            if (Match(TokenType.IDENTIFIER))
                return new VariableExpr(Previous());

            if (Match(TokenType.FUN))
                return Function();

            if (Match(TokenType.LEFT_PAREN))
            {
                var expr = Expression();
                Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
                return new GroupingExpr(expr);
            }

            throw Error(Peek(), "Expect expression");
        }

        private Expr Function()
        {
            Consume(TokenType.LEFT_PAREN, $"Expect '(' after anonymous function.");

            var name = Previous();

            var parameters = new List<Token>();

            if (!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if (parameters.Count >= 8)
                        Error(Peek(), "Cannot have more than 8 parameters.");

                    parameters.Add(Consume(TokenType.IDENTIFIER, "Expect parameter name."));
                } while (Match(TokenType.COMMA));
            }

            Consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");

            Consume(TokenType.LEFT_BRACE, $"Expect '{TokenType.LEFT_BRACE}' before anonymous function body.");
            var body = BlockStatement();

            return new FunctionExpr(parameters, body);
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