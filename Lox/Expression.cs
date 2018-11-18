using System.Collections.Generic;

namespace Lox
{
    public abstract class Expr
    {
    }

    public class TernaryExpr : Expr
    {
        protected readonly Expr _cond;

        protected readonly Expr _left;

        protected readonly Expr _right;

        public Expr Cond { get => _cond; }

        public Expr Left { get => _left; }

        public Expr Right { get => _right; }

        public TernaryExpr(Expr cond, Expr left, Expr right)
        {
            _cond = cond;
            _left = left;
            _right = right;
        }
    }

    public class BinaryExpr : Expr
    {
        protected readonly Expr _left;

        protected readonly Expr _right;

        protected readonly Token _operator;

        public Expr Left { get => _left; }

        public Expr Right { get => _right; }

        public Token Operator { get => _operator; }


        public BinaryExpr(Expr left, Token oper, Expr right)
        {
            _left = left;
            _operator = oper;
            _right = right;
        }
    }

    public class GroupingExpr : Expr
    {
        private readonly Expr _expression;

        public Expr Expression { get => _expression; }

        public GroupingExpr(Expr expression)
        {
            _expression = expression;
        }
    }

    public class LiteralExpr : Expr
    {
        private readonly object _value;

        public object Value { get => _value; }

        public LiteralExpr(object value)
        {
            _value = value;
        }
    }

    public class VariableExpr : Expr
    {
        private readonly Token _name;

        public Token Name { get => _name; }

        public VariableExpr(Token name)
        {
            _name = name;
        }
    }

    public class UnaryExpr : Expr
    {
        private readonly Token _operator;

        private readonly Expr _right;

        public Token Operator { get => _operator; }

        public Expr Right { get => _right; }

        public UnaryExpr(Token oper, Expr right)
        {
            _operator = oper;
            _right = right;
        }
    }

    public class AssignmentExpr : Expr
    {
        private readonly Token _name;

        private readonly Expr _value;

        public Token Name { get => _name; }

        public Expr Value { get => _value; }

        public AssignmentExpr(Token name, Expr value)
        {
            _name = name;
            _value = value;
        }
    }

    public class LogicalExpr : Expr
    {
        private readonly Expr _left;

        private readonly Token _operator;

        private readonly Expr _right;

        public Expr Left { get => _left; }

        public Token Operator { get => _operator; }

        public Expr Right { get => _right; }

        public LogicalExpr (Expr left, Token oper, Expr right)
        {
            _left = left;
            _operator = oper;
            _right = right;
        }
    }

    public class CallExpr : Expr
    {
        private readonly Expr _callee;

        private readonly Token _paren;

        private readonly IList<Expr> _arguments;

        public Expr Callee { get => _callee; }

        public Token Paren { get => _paren; }

        public IList<Expr> Arguments { get => _arguments; }

        public CallExpr(Expr callee, Token paren, IList<Expr> arguments)
        {
            _callee = callee;
            _paren = paren;
            _arguments = arguments;
        }
    }

    public class FunctionExpr : Expr
    {
        private readonly IList<Token> _params;

        private readonly BlockStmt _body;

        public IList<Token> Params { get => _params; }

        public BlockStmt Body { get => _body; }

        public FunctionExpr(IList<Token> parameters, BlockStmt body)
        {
            _params = parameters;
            _body = body;
        }  
    }
}