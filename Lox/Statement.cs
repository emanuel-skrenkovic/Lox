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
}