using System.Collections.Generic;

namespace Lox
{
    public abstract class Expr
    {
    }

    public class Ternary : Expr
    {
        protected readonly Expr _cond;

        protected readonly Expr _left;

        protected readonly Expr _right;

        public Expr Cond { get => _cond; }

        public Expr Left { get => _left; }

        public Expr Right { get => _right; }

        public Ternary(Expr cond, Expr left, Expr right)
        {
            _cond = cond;
            _left = left;
            _right = right;
        }
    }

    public class Binary : Expr
    {
        protected readonly Expr _left;

        protected readonly Expr _right;

        protected readonly Token _operator;

        public Expr Left { get => _left; }

        public Expr Right { get => _right; }

        public Token Operator { get => _operator; }


        public Binary(Expr left, Token oper, Expr right)
        {
            _left = left;
            _operator = oper;
            _right = right;
        }
    }

    public class Grouping : Expr
    {
        private readonly Expr _expression;

        public Expr Expression { get => _expression; }

        public Grouping(Expr expression)
        {
            _expression = expression;
        }
    }

    public class Literal : Expr
    {
        private readonly object _value;

        public object Value { get => _value; }

        public Literal(object value)
        {
            _value = value;
        }
    }

    public class Variable : Expr
    {
        private readonly Token _name;

        public Token Name { get => _name; }

        public Variable(Token name)
        {
            _name = name;
        }
    }

    public class Unary : Expr
    {
        private readonly Token _operator;

        private readonly Expr _right;

        public Token Operator { get => _operator; }

        public Expr Right { get => _right; }

        public Unary(Token oper, Expr right)
        {
            _operator = oper;
            _right = right;
        }
    }

    public class Assignment : Expr
    {
        private readonly Token _name;

        private readonly Expr _value;

        public Token Name { get => _name; }

        public Expr Value { get => _value; }

        public Assignment(Token name, Expr value)
        {
            _name = name;
            _value = value;
        }
    }

    public class Logical : Expr
    {
        private readonly Expr _left;

        private readonly Token _operator;

        private readonly Expr _right;

        public Expr Left { get => _left; }

        public Token Operator { get => _operator; }

        public Expr Right { get => _right; }

        public Logical (Expr left, Token oper, Expr right)
        {
            _left = left;
            _operator = oper;
            _right = right;
        }
    }

    public class Call : Expr
    {
        private readonly Expr _callee;

        private readonly Token _paren;

        private readonly IList<Expr> _arguments;

        public Expr Callee { get => _callee; }

        public Token Paren { get => _paren; }

        public IList<Expr> Arguments { get => _arguments; }

        public Call(Expr callee, Token paren, IList<Expr> arguments)
        {
            _callee = callee;
            _paren = paren;
            _arguments = arguments;
        }
    }
}