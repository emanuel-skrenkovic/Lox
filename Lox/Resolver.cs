using System.Collections.Generic;
using System.Linq;

namespace Lox
{
    public class Resolver : Matcher
    {
        private readonly Interpreter _interpreter; 

        private readonly Stack<Dictionary<string, bool>> _scopes = new Stack<Dictionary<string, bool>>();

        private FunctionType _currentFunction = FunctionType.NONE;

        private ClassType _currentClass = ClassType.NONE;

        private enum FunctionType 
        {
            NONE,
            FUNCTION,
            METHOD,
            STATIC,
            INITIALIZER,
        }

        private enum ClassType
        {
            NONE,
            CLASS,
            SUBCLASS,
        }

        public Resolver(Interpreter interpreter)
        {
            _interpreter = interpreter;
        }

        public void Resolve(IList<Stmt> statements)
        {
            foreach (var stmt in statements)
                Resolve(stmt);
        }

        public object Resolve(Expr expression) => MatchExpression(expression);

        public void Resolve(Stmt statement) => MatchStatement(statement);

        protected override void MatchDeclarationStmt(DeclarationStmt stmt)
        {
            Declare(stmt.Name);

            if (stmt.Initializer != null)
                Resolve(stmt.Initializer);

            Define(stmt.Name); 
        }

        protected override void MatchBlockStmt(BlockStmt stmt)
        {
            BeginScope();

            Resolve(stmt.Statements);

            EndScope();
        }

        protected override void MatchVariableStmt(VariableStmt stmt)
        {
            Declare(stmt.Name);

            if (stmt.Initializer != null)
                Resolve(stmt.Initializer);

            Define(stmt.Name);
        }

        protected override void MatchFunctionStmt(FunctionStmt stmt)
        {
            Declare(stmt.Name);
            Define(stmt.Name);

            ResolveFunction(stmt, FunctionType.FUNCTION);
        }

        protected override void MatchClassStmt(ClassStmt stmt)
        {
            var enclosingClass = _currentClass;
            _currentClass = ClassType.CLASS;

            Declare(stmt.Name);

            if (stmt.Superclass != null)
            {
                _currentClass = ClassType.SUBCLASS;
                Resolve(stmt.Superclass);
            }

            Define(stmt.Name);

            if (stmt.Superclass != null)
            {
                BeginScope();
                _scopes.Peek()["super"] = true;
            }

            BeginScope();
            _scopes.Peek()["this"] = true;

            foreach (var method in stmt.Methods)
            {
                var declaration = FunctionType.METHOD;

                if (method.Name.Lexeme == "init")
                    declaration = FunctionType.INITIALIZER;

                ResolveFunction(method, declaration);
            }

            foreach (var method in stmt.StaticMethods)
            {
                var declaration = FunctionType.STATIC;

                if (method.Name.Lexeme == "init")
                    Lox.Error(method.Name, "Cannot declare init as static.");

                ResolveFunction(method, declaration);
            }

            if (stmt.Superclass != null)
                EndScope();

            EndScope();

            _currentClass = enclosingClass;
        }

        protected override void MatchIfStmt(IfStmt stmt)
        {
            Resolve(stmt.Condition);
            Resolve(stmt.Then);

            if (stmt.Else != null)
                Resolve(stmt.Else);
        }

        protected override void MatchPrintStmt(PrintStmt stmt) => Resolve(stmt.Expression);

        protected override void MatchReturnStmt(ReturnStmt stmt)
        {
            if (_currentFunction == FunctionType.NONE)
                Lox.Error(stmt.Keyword, "Cannot return from top-level code.");

            if (stmt.Value != null)
            {
                if (_currentFunction == FunctionType.INITIALIZER)
                    Lox.Error(stmt.Keyword, "Cannot return a value from an initializer.");

                Resolve(stmt.Value);
            }
        }

        protected override void MatchWhileStmt(WhileStmt stmt)
        {
            Resolve(stmt.Condition);
            Resolve(stmt.Body);
        }

        protected override void MatchExpressionStmt(ExpressionStmt stmt) => Resolve(stmt.Expression);

        protected override void MatchLoopControlStmt(LoopControlStmt stmt) { }

        protected override object MatchGetExpr(GetExpr expr) => Resolve(expr.Object);

        protected override object MatchSetExpr(SetExpr expr)
        {
            Resolve(expr.Value);
            Resolve(expr.Object);

            return null;
        }

        protected override object MatchVariableExpr(VariableExpr expr)
        {
            if (_scopes.Count != 0)
            {
                var scope = _scopes.Peek();

                bool? initialized = null;

                if (scope.TryGetValue(expr.Name.Lexeme, out var temp))
                    initialized = temp;

                if (initialized == false)
                {
                    Lox.Error(expr.Name, "Cannot read local variable in its own initializer.");
                }
            }

            ResolveLocal(expr, expr.Name);

            return null;
        }

        protected override object MatchAssignExpr(AssignmentExpr expr)
        {
            Resolve(expr.Value);

            ResolveLocal(expr, expr.Name);

            return null;
        }

        protected override object MatchTernaryExpr(TernaryExpr expr)
        {
            Resolve(expr.Cond);
            Resolve(expr.Left);
            Resolve(expr.Right);

            return null;
        }

        protected override object MatchBinaryExpr(BinaryExpr expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);

            return null;
        }

        protected override object MatchCallExpr(CallExpr expr)
        {
            Resolve(expr.Callee);

            foreach (var arg in expr.Arguments)
                Resolve(arg);

            return null;
        }

        protected override object MatchFunctionExpr(FunctionExpr expr)
        {
            FunctionExpr(expr, FunctionType.FUNCTION);

            return null;
        }

        private void FunctionExpr(FunctionExpr expr, FunctionType type)
        {
            var enclosingFunction = _currentFunction;

            _currentFunction = type;

            BeginScope();
            
            foreach (var parameter in expr.Params)
            {
                Declare(parameter);
                Define(parameter);
            }

            Resolve(expr.Body.Statements);
            
            EndScope();

            _currentFunction = enclosingFunction;

        }

        protected override object MatchThisExpr(ThisExpr expr)
        {
            if (_currentClass == ClassType.NONE)
                Lox.Error(expr.Keyword, "Cannot use 'this' outside of a class.");

           ResolveLocal(expr, expr.Keyword);

           return null;
        } 

        protected override object MatchSuperExpr(SuperExpr expr) 
        {
            if (_currentClass == ClassType.NONE)
                Lox.Error(expr.Keyword, "Cannot use 'super' outside of a class.");
            else if (_currentClass != ClassType.SUBCLASS)
                Lox.Error(expr.Keyword, "Cannot use 'super' in a class with no supeclass.");

            ResolveLocal(expr, expr.Keyword);

            return null;
        } 
        protected override object MatchGroupingExpr(GroupingExpr expr) => Resolve(expr.Expression);

        protected override object MatchLiteralExpr(LiteralExpr expr) => null;

        protected override object MatchLogicalExpr(LogicalExpr expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);

            return null;
        }

        protected override object MatchUnaryExpr(UnaryExpr expr) => Resolve(expr.Right);

        private void Declare(Token name)
        {
            if (_scopes.Count == 0)
                return;

            var scope = _scopes.Peek();

            if (scope.ContainsKey(name.Lexeme))
                Lox.Error(name, $"Variable named {name.Lexeme} already declared in this scope.");

            scope[name.Lexeme] = false;
        }

        private void Define(Token name)
        {
            if (_scopes.Count == 0)
                return;

            _scopes.Peek()[name.Lexeme] = true;
        }

        private void ResolveFunction(FunctionStmt function, FunctionType type)
        {
            var enclosingFunction = _currentFunction;

            _currentFunction = type;

            BeginScope();

            foreach (var param in function.Params)
            {
                Declare(param);
                Define(param);
            }

            Resolve(function.Body.Statements);
            
            EndScope();

            _currentFunction = enclosingFunction;
        }

        private void ResolveLocal(Expr expression, Token name)
        {
            for (int i = 0; i < _scopes.Count; i++)
            {
                if (_scopes.ElementAt(i).ContainsKey(name.Lexeme))
                {
                    _interpreter.Resolve(expression, i);
                    return;
                }
            }
        }
       
        private void BeginScope() => _scopes.Push(new Dictionary<string, bool>());

        private void EndScope() => _scopes.Pop();
    }
}