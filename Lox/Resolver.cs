using System.Collections.Generic;
using System.Linq;

namespace Lox
{
    public class Resolver
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

        public void Resolve(Expr expression)
        {
            switch (expression)
            {
                case GetExpr getExpr:
                    ResolveGetExpr(getExpr);
                    break;

                case SetExpr setExpr:
                    ResolveSetExpr(setExpr);
                    break;

                case VariableExpr variableExpr:
                    ResolveVariableExpr(variableExpr);
                    break;

                case LogicalExpr logical:
                    ResolveLogicalExpr(logical);
                    break;

                case AssignmentExpr assignment:
                    ResolveAssignExpr(assignment);
                    break;

                case TernaryExpr ternary:
                    ResolveTernaryExpr(ternary);
                    break;

                case BinaryExpr binary:
                    ResolveBinaryExpr(binary);
                    break;

                case CallExpr call:
                    ResolveCallExpr(call);
                    break;

                case UnaryExpr unary:
                    ResolveUnaryExpr(unary);
                    break;

                case GroupingExpr grouping:
                    ResolveGroupingExpr(grouping);
                    break;

                case LiteralExpr literal:
                    ResolveLiteralExpr(literal);
                    break;

                case FunctionExpr function:
                    ResolveFunctionExpr(function, FunctionType.FUNCTION);
                    break;

                case ThisExpr thisExpr:
                    ResolveThisExpr(thisExpr);
                    break;

                case SuperExpr superExpr:
                    ResolveSuperExpr(superExpr);
                    break;
            }
        }

        public void Resolve(Stmt statement)
        {
            switch (statement)
            {
                case ClassStmt classStmt:
                    ResolveClassStmt(classStmt);
                    break;

                case IfStmt ifStmt:
                    ResolveIfStmt(ifStmt);
                    break;

                case DeclarationStmt declarationStmt:
                    ResolveDeclarationStmt(declarationStmt);
                    break;

                case VariableStmt variableStmt:
                    ResolveVariableStmt(variableStmt);
                    break;

                case BlockStmt blockStmt:
                    ResolveBlockStmt(blockStmt);
                    break;

                case ExpressionStmt exprStmt:
                    ResolveExpressionStmt(exprStmt);
                    break;

                case PrintStmt printStmt:
                    ResolvePrintStmt(printStmt);
                    break; 
                
                case WhileStmt whileStmt:
                    ResolveWhileStmt(whileStmt);
                    break;

                case LoopControlStmt loopControlStmt:
                    ResolveLoopControlStmt(loopControlStmt);
                    break;

                case FunctionStmt functionStmt:
                    ResolveFunctionStmt(functionStmt);
                    break;

                case ReturnStmt returnStmt:
                    ResolveReturnStmt(returnStmt);
                    break;
            }
        }

        private void ResolveDeclarationStmt(DeclarationStmt stmt)
        {
            Declare(stmt.Name);

            if (stmt.Initializer != null)
                Resolve(stmt.Initializer);

            Define(stmt.Name); 
        }

        private void ResolveBlockStmt(BlockStmt stmt)
        {
            BeginScope();

            Resolve(stmt.Statements);

            EndScope();
        }

        private void ResolveVariableStmt(VariableStmt stmt)
        {
            Declare(stmt.Name);

            if (stmt.Initializer != null)
                Resolve(stmt.Initializer);

            Define(stmt.Name);
        }

        private void ResolveFunctionStmt(FunctionStmt stmt)
        {
            Declare(stmt.Name);
            Define(stmt.Name);

            ResolveFunction(stmt, FunctionType.FUNCTION);
        }

        private void ResolveClassStmt(ClassStmt stmt)
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

        private void ResolveIfStmt(IfStmt stmt)
        {
            Resolve(stmt.Condition);
            Resolve(stmt.Then);

            if (stmt.Else != null)
                Resolve(stmt.Else);
        }

        private void ResolvePrintStmt(PrintStmt stmt) => Resolve(stmt.Expression);

        private void ResolveReturnStmt(ReturnStmt stmt)
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

        private void ResolveWhileStmt(WhileStmt stmt)
        {
            Resolve(stmt.Condition);
            Resolve(stmt.Body);
        }

        private void ResolveExpressionStmt(ExpressionStmt stmt) => Resolve(stmt.Expression);

        private void ResolveLoopControlStmt(LoopControlStmt stmt) { }

        private void ResolveGetExpr(GetExpr expr) => Resolve(expr.Object);

        private void ResolveSetExpr(SetExpr expr)
        {
            Resolve(expr.Value);
            Resolve(expr.Object);
        }

        private void ResolveVariableExpr(VariableExpr expr)
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
        }

        private void ResolveAssignExpr(AssignmentExpr expr)
        {
            Resolve(expr.Value);

            ResolveLocal(expr, expr.Name);
        }

        private void ResolveTernaryExpr(TernaryExpr expr)
        {
            Resolve(expr.Cond);
            Resolve(expr.Left);
            Resolve(expr.Right);
        }

        private void ResolveBinaryExpr(BinaryExpr expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);
        }

        private void ResolveCallExpr(CallExpr expr)
        {
            Resolve(expr.Callee);

            foreach (var arg in expr.Arguments)
                Resolve(arg);
        }

        private void ResolveFunctionExpr(FunctionExpr expr, FunctionType type)
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

        private void ResolveThisExpr(ThisExpr expr)
        {
            if (_currentClass == ClassType.NONE)
                Lox.Error(expr.Keyword, "Cannot use 'this' outside of a class.");

           ResolveLocal(expr, expr.Keyword);
        } 

        private void ResolveSuperExpr(SuperExpr expr) 
        {
            if (_currentClass == ClassType.NONE)
                Lox.Error(expr.Keyword, "Cannot use 'super' outside of a class.");
            else if (_currentClass != ClassType.SUBCLASS)
                Lox.Error(expr.Keyword, "Cannot use 'super' in a class with no supeclass.");

            ResolveLocal(expr, expr.Keyword);
        } 
        private void ResolveGroupingExpr(GroupingExpr expr) => Resolve(expr.Expression);

        private void ResolveLiteralExpr(LiteralExpr expr) { }

        private void ResolveLogicalExpr(LogicalExpr expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);
        }

        private void ResolveUnaryExpr(UnaryExpr expr) => Resolve(expr.Right);

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