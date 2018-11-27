using System;
using System.Collections.Generic;
using System.Linq;

namespace Lox
{
    public class Interpreter
    {
        private readonly static Environment _globals = new Environment();

        private readonly IDictionary<Expr, int> _locals = new Dictionary<Expr, int>();

        private Environment _env = _globals;

        public Environment Globals { get => _globals; }

        public Interpreter() 
        { 
            DefineGlobals();
        }

        private void DefineGlobals()
        {
            ICallable clock = new Clock();
            _globals.Define("clock", clock);
        }

        public void Interpret(IList<Stmt> stmts)
        {
            try 
            {
                bool shouldBreak = false;
                bool shouldContinue = false;
                foreach (var s in stmts)
                    Execute(s, ref shouldBreak, ref shouldContinue);
            }
            catch (RuntimeError err)
            {
                Lox.RuntimeError(err);
            }
        }

        private void Execute(Stmt stmt, ref bool shouldBreak, ref bool shouldContinue)
        {
            switch (stmt)
            {
                case ClassStmt classStmt:
                    ClassStmt(classStmt);
                    break;

                case IfStmt ifStmt:
                    IfStmt(ifStmt, ref shouldBreak, ref shouldContinue);
                    break;

                case DeclarationStmt declarationStmt:
                    DeclarationStmt(declarationStmt);
                    break;

                case VariableStmt variableStmt:
                    VariableStmt(variableStmt);
                    break;

                case ExpressionStmt exprStmt:
                    ExpressionStmt(exprStmt);
                    break;

                case BlockStmt blockStmt:
                    BlockStmt(blockStmt, new Environment(_env), ref shouldBreak, ref shouldContinue);
                    break;

                case PrintStmt printStmt:
                    PrintStmt(printStmt);
                    break; 
                
                case WhileStmt whileStmt:
                    WhileStmt(whileStmt, ref shouldBreak, ref shouldContinue);
                    break;

                case LoopControlStmt loopControlStmt:
                    LoopControlStmt(loopControlStmt, ref shouldBreak, ref shouldContinue);
                    break;

                case FunctionStmt functionStmt:
                    FunctionStmt(functionStmt);
                    break;

                case ReturnStmt returnStmt:
                    ReturnStmt(returnStmt);
                    break;
            }
        }

        private object EvaluateExpr(Expr expr)
        {
            try 
            {
                switch (expr)
                {
                    case LogicalExpr logical:
                        return Logical(logical);

                    case AssignmentExpr assignment:
                        return Assignment(assignment);

                    case TernaryExpr ternary:
                        return Ternary(ternary);

                    case BinaryExpr binary:
                        return Binary(binary);

                    case CallExpr call:
                        return Call(call);

                    case UnaryExpr unary:
                        return Unary(unary);

                    case GroupingExpr grouping:
                        return Grouping(grouping);

                    case VariableExpr variable:
                        return Variable(variable);

                    case LiteralExpr literal:
                        return Literal(literal);

                    case FunctionExpr function:
                        return Function(function);

                    case GetExpr get:
                        return Get(get);

                    case SetExpr set:
                        return Set(set);

                    case ThisExpr thisExpr:
                        return This(thisExpr);

                    case SuperExpr superExpr:
                        return Super(superExpr);

                    default:
                        return null;
                }
            }
            catch (RuntimeError err)
            {
                Lox.RuntimeError(err);
                return null;
            }
        }

        internal void Resolve(Expr expr, int depth) => _locals.Add(expr, depth);

        private void ClassStmt(ClassStmt stmt)
        {
            object superclass = null;

            if (stmt.Superclass != null)
            {
                superclass = EvaluateExpr(stmt.Superclass);

                if (!(superclass is Class))
                    throw new RuntimeError(stmt.Superclass.Name, "Superclass must be a class.");
            }

            _env.Define(stmt.Name.Lexeme, null);

            if (stmt.Superclass != null)
            {
                _env = new Environment(_env);
                _env.Define("super", superclass);
            }

            var methods = new Dictionary<string, Function>();

            foreach (var method in stmt.Methods)
                methods[method.Name.Lexeme] = new Function(method, _env, method.Name.Lexeme == "init");

            IDictionary<string, Function> staticMethods = stmt.StaticMethods
                .ToDictionary(m => m.Name.Lexeme, m => new Function(m, _env, false));

            var klass = new Class(stmt.Name.Lexeme, (Class)superclass, methods, staticMethods);

            if (superclass != null)
                _env = _env.Enclosing;

            _env.Assign(stmt.Name, klass);
        }

        private void IfStmt(IfStmt stmt, ref bool shouldBreak, ref bool shouldContinue)
        {
            if (IsTruthy(EvaluateExpr(stmt.Condition)))
                Execute(stmt.Then, ref shouldBreak, ref shouldContinue);
            else if (stmt.Else != null)
                Execute(stmt.Else, ref shouldBreak, ref shouldContinue);
        }

        private void Declaration(DeclarationStmt stmt)
        {
           var value = stmt?.Initializer != null 
                ? EvaluateExpr(stmt.Initializer) 
                : null;

            _env.Define(stmt.Name.Lexeme, value);
        }

        private void DeclarationStmt(DeclarationStmt stmt) => Declaration(stmt);

        private void VariableStmt(VariableStmt stmt) => Declaration(stmt.Declaration);

        private void ExpressionStmt(ExpressionStmt stmt) => EvaluateExpr(stmt.Expression);

        internal void BlockStmt(BlockStmt stmt, Environment environment, ref bool shouldBreak, ref bool shouldContinue) 
            => ExecuteBlock(stmt.Statements, environment, ref shouldBreak, ref shouldContinue);

        private void ExecuteBlock(IList<Stmt> statements, Environment environment, ref bool shouldBreak, ref bool shouldContinue) 
        {
            var prevEnv = _env;
            try
            {
                _env = environment;

                foreach (var s in statements)
                {
                    Execute(s, ref shouldBreak, ref shouldContinue);

                    if (shouldBreak || shouldContinue)
                        break;
                }
            }
            finally
            {
                _env = prevEnv;
            }
        }

        private void PrintStmt(PrintStmt stmt)
        {
            var value = EvaluateExpr(stmt.Expression);

            Console.WriteLine(Stringify(value));
        }

        private void WhileStmt(WhileStmt stmt, ref bool shouldBreak, ref bool shouldContinue)
        {
            while (IsTruthy(EvaluateExpr(stmt.Condition)) && !shouldBreak)
                Execute(stmt.Body, ref shouldBreak, ref shouldContinue);

            if (shouldBreak)
                shouldBreak = false;

            if (shouldContinue)
                shouldContinue = false;        
        }

        private void LoopControlStmt(LoopControlStmt stmt, ref bool shouldBreak, ref bool shouldContinue)
        {
            shouldBreak = stmt.Type.Type == TokenType.BREAK;
            shouldContinue = !shouldBreak;
        }

        private void FunctionStmt(FunctionStmt stmt)
        {
            var function = new Function(stmt, _env);

            _env.Define(stmt.Name.Lexeme, function);
        }

        private void ReturnStmt(ReturnStmt stmt)
        {
            var value = EvaluateExpr(stmt.Value) ?? null;

            throw new Return(value);
        }

        private object Logical(LogicalExpr expr)
        {
            var left = EvaluateExpr(expr.Left);

            if (expr.Operator.Type == TokenType.OR)
            {
                if (IsTruthy(left))
                    return left;
            }
            else
            {
                if (!IsTruthy(left))
                    return left;
            }

            return EvaluateExpr(expr.Right);
        }

        private object Assignment(AssignmentExpr expr)
        {
            var value = EvaluateExpr(expr.Value);

            if (_locals.TryGetValue(expr, out var distance))
                _env.AssignAt(distance, expr.Name, value);                
            else
                _globals.Assign(expr.Name, value);

            return value;
        }

        private object Literal(LiteralExpr expr) => expr.Value;

        private object Grouping(GroupingExpr expr) => EvaluateExpr(expr.Expression);

        private object Variable(VariableExpr expr) => LookUpVariable(expr.Name, expr);

        private object Call(CallExpr expr)
        {
            var callee = EvaluateExpr(expr.Callee);

            var arguments = expr.Arguments.Select(EvaluateExpr).ToList();

            if (!(callee is ICallable))
                throw new RuntimeError(expr.Paren, "Can only call functions and classes.");

            var function = (ICallable)callee;

            if (arguments.Count != function.Arity)
                throw new RuntimeError(expr.Paren, $"Expected {function.Arity} arguments but got {arguments.Count}.");

            return function.Call(this, arguments);
        }

        private object Function(FunctionExpr expr)
        {
            var stmt = new FunctionStmt(null, expr.Params, expr.Body);

            return new Function(stmt, _env); 
        }

        private object Get(GetExpr expr)
        {
            var obj = EvaluateExpr(expr.Object);

            if (obj is IInstance instance)
                return instance.Get(expr.Name);

            throw new RuntimeError(expr.Name, "Only instances have properties.");
        }

        private object Set(SetExpr expr)
        {
            var obj= EvaluateExpr(expr.Object);

            if (!(obj is IInstance instance))
                throw new RuntimeError(expr.Name, "Only instances have fields.");

            var value = EvaluateExpr(expr.Value);

            instance.Set(expr.Name, value);

            return value;
        }

        private object This(ThisExpr expr) => LookUpVariable(expr.Keyword, expr);

        private object Super(SuperExpr expr)
        {
            _locals.TryGetValue(expr, out var distance);

            var superclass = (Class)_env.GetAt(distance, "super");
            var obj = (Instance)_env.GetAt(distance - 1, "this");
            var method = superclass.FindMethod(obj, expr.Method.Lexeme);

            if (method == null)
                throw new RuntimeError(expr.Method, $"Undefined property '{expr.Method.Lexeme}'.");

            return method;
        }

        private object Unary(UnaryExpr expr)
        {
            var right = EvaluateExpr(expr.Right);

            switch (expr.Operator.Type)
            {
                case TokenType.BANG:
                    return !IsTruthy(right); 

                case TokenType.MINUS:
                    CheckNumberOperand(expr.Operator, right);
                    return -(double)right;
            }

            return null;
        }

        private object Binary(BinaryExpr expr)
        {
            var left = EvaluateExpr(expr.Left);
            var right = EvaluateExpr(expr.Right);

            switch (expr.Operator.Type)
            {
                case TokenType.BANG_EQUAL:
                    CheckNumberOperands(expr.Operator, left, right);
                    return !IsEqual(left, right);

                case TokenType.EQUAL_EQUAL:
                    CheckNumberOperands(expr.Operator, left, right);
                    return IsEqual(left, right);

                case TokenType.GREATER:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left > (double)right;
                
                case TokenType.GREATER_EQUAL:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left >= (double)right;

                case TokenType.LESS:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left < (double)right;

                case TokenType.LESS_EQUAL:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left <= (double)right;

                case TokenType.MINUS:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left - (double)right;

                case TokenType.PLUS:
                    if (left is double dl && right is double dr) 
                        return dl + dr;

                    if (left is string sl && right is string sr)
                        return sl + sr;

                    throw new RuntimeError(expr.Operator, "Operands must be two numbers or two strings");

                case TokenType.SLASH:
                    CheckNumberOperands(expr.Operator, left, right);
                    CheckDivisionByZero(expr.Operator, (double)right);
                    return (double)left / (double)right;

                case TokenType.STAR:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left * (double)right;

                default:
                    return null;
            }
        }
        
        private object Ternary(TernaryExpr expr)
        {
            var cond = EvaluateExpr(expr.Cond);

            return IsTruthy(cond) ? EvaluateExpr(expr.Left) : EvaluateExpr(expr.Right);
        }

        private object LookUpVariable(Token name, Expr expr)
        {
            if (_locals.TryGetValue(expr, out var distance))
                return _env.GetAt(distance, name.Lexeme);
            else
                return _globals.Get(name);
        }

        private void CheckNumberOperand(Token oper, object operand)
        {
            if (operand is double)
                return;

            throw new RuntimeError(oper, "Operand must be a number");
        }

        private void CheckNumberOperands(Token oper, object left, object right)
        {
            if (left is double && right is double)
                return;

            throw new RuntimeError(oper, "Operands must be numbers");
        }

        private void CheckDivisionByZero(Token oper, double dividend)
        {
            if (dividend == 0)
                throw new RuntimeError(oper, "Division by zero");
        }

        private bool IsEqual(object a, object b)
        {
            if (a == null && b == null) 
                return true;

            if (a == null)
                return false;

            return a.Equals(b);
        }

        private bool IsTruthy(object obj)
        {
            if (obj == null)
                return false;

            if (obj is bool)
                return (bool)obj;

            return true; 
        }

        private string Stringify(object obj)
        {
            if (obj == null)
                return "nil";

            if (obj is double d)
                return d.ToString();

            return obj.ToString();
        }
    }
}