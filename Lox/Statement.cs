using System.Collections.Generic;

namespace Lox
{
    public class Stmt
    {
        
    }

    public class DeclarationStmt : Stmt
    {
        private readonly Token _name;

        private readonly Expr _initializer;

        public Token Name { get => _name; }

        public Expr Initializer { get => _initializer; }

        public DeclarationStmt(Token name, Expr initializer)
        {
            _name = name;
            _initializer = initializer;
        }
    }

    public class VariableStmt : Stmt
    {
        private readonly DeclarationStmt _declaration;

        public DeclarationStmt Declaration { get => _declaration; }

        public Token Name { get => _declaration.Name; }

        public Expr Initializer { get => _declaration.Initializer; }

        public VariableStmt(Token name, Expr initializer)
        {
            _declaration = new DeclarationStmt(name, initializer);
        }
    }

    public class ExpressionStmt : Stmt
    {
        private readonly Expr _expr;

        public Expr Expression { get => _expr; }

        public ExpressionStmt(Expr expr)
        {
            _expr = expr;
        }
    }

    public class PrintStmt : Stmt
    {
        private readonly Expr _expression; 

        public Expr Expression { get => _expression; }

        public PrintStmt(Expr expression)
        {
            _expression = expression;
        }
    }

    public class BlockStmt : Stmt
    {
        private readonly IList<Stmt> _statements;

        public IList<Stmt> Statements { get => _statements; }

        public BlockStmt()
        {
            _statements = new List<Stmt>();
        }

        public BlockStmt(IList<Stmt> statements)
        {
            _statements = statements ?? new List<Stmt>();
        }
    }

    public class IfStmt : Stmt
    {
        private readonly Expr _condition;

        private readonly Stmt _thenBlock;

        private readonly Stmt _elseBlock;

        public Expr Condition { get => _condition; }

        public Stmt Then { get => _thenBlock; }

        public Stmt Else { get => _elseBlock; }

        public IfStmt(Expr condition, Stmt thenBlock) : this (condition, thenBlock, null) { }

        public IfStmt(Expr condition, Stmt thenBlock, Stmt elseBlock)
        {
            _condition = condition;
            _thenBlock = thenBlock;
            _elseBlock = elseBlock;
        }
    }

    public class WhileStmt : Stmt
    {
        private readonly Expr _condition;        

        private readonly Stmt _body;

        public Expr Condition { get => _condition; }

        public Stmt Body { get => _body; }

        public WhileStmt(Expr condition, Stmt body)
        {
            _condition = condition;
            _body = body;
        }
    }

    public class LoopControlStmt : Stmt
    {
        private readonly Token _type;

        public Token Type { get => _type; }

        public LoopControlStmt(Token type)
        {
            _type = type;
        }
    }

    public class FunctionStmt : Stmt
    {
        private readonly Token _name;

        private readonly IList<Token> _params;

        private readonly BlockStmt _body;

        public Token Name { get => _name; }

        public IList<Token> Params { get => _params; }

        public BlockStmt Body { get => _body; }

        public FunctionStmt(Token name, IList<Token> parameters, BlockStmt body)
        {
            _name = name;
            _params = parameters;
            _body = body;
        }
    }

    public class ReturnStmt : Stmt
    {
        private readonly Token _keyword;

        private readonly Expr _value; 

        public Token Keyword { get => _keyword; }

        public Expr Value { get => _value; }

        public ReturnStmt(Token keyword, Expr value)
        {
            _keyword = keyword;
            _value = value;
        }
    }

    public class ClassStmt : Stmt
    {
        private readonly Token _name;

        private readonly VariableExpr _superclass;

        private readonly IList<FunctionStmt> _methods;

        public Token Name { get => _name; }
        
        public VariableExpr Superclass { get => _superclass; }

        public IList<FunctionStmt> Methods { get => _methods; }

        public ClassStmt(Token name, VariableExpr superclass, IList<FunctionStmt> methods)
        {
            _name = name;
            _superclass = superclass;
            _methods = methods;
        }
    }
}