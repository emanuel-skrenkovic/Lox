using System;
using System.Collections.Generic;

namespace Lox
{
    public class Interpreter
    {
        private Environment _env = new Environment();

        public Interpreter() { }

        public void Interpret(IList<Stmt> stmts)
        {
            try 
            {
                bool test = false;
                foreach (var s in stmts)
                    Execute(s, ref test);
            }
            catch (RuntimeError err)
            {
                Lox.RuntimeError(err);
            }
        }

        private void Execute(Stmt stmt, ref bool shouldBreak)
        {
            switch (stmt)
            {
                case IfStmt ifStmt:
                    IfStmt(ifStmt, ref shouldBreak);
                    break;

                case DeclarationStmt declarationStmt:
                    DeclarationStmt(declarationStmt, ref shouldBreak);
                    break;

                case VariableStmt variableStmt:
                    VariableStmt(variableStmt, ref shouldBreak);
                    break;

                case ExpressionStmt exprStmt:
                    ExpressionStmt(exprStmt, ref shouldBreak);
                    break;

                case BlockStmt blockStmt:
                    BlockStmt(blockStmt, new Environment(_env), ref shouldBreak);
                    break;

                case PrintStmt printStmt:
                    PrintStmt(printStmt, ref shouldBreak);
                    break; 
                
                case WhileStmt whileStmt:
                    WhileStmt(whileStmt, ref shouldBreak);
                    break;

                case BreakStmt breakStmt:
                    BreakStmt(breakStmt, ref shouldBreak);
                    break;
            }
        }

        private object EvaluateExpr(Expr expr)
        {
            try 
            {
                switch (expr)
                {
                    case Logical logical:
                        return Logical(logical);

                    case Assignment assignment:
                        return Assignment(assignment);

                    case Ternary ternary:
                        return Ternary(ternary);

                    case Binary binary:
                        return Binary(binary);

                    case Unary unary:
                        return Unary(unary);

                    case Grouping grouping:
                        return Grouping(grouping);

                    case Variable variable:
                        return Variable(variable);

                    case Literal literal:
                        return Literal(literal);

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

        private void IfStmt(IfStmt stmt, ref bool shouldBreak)
        {
            if (IsTruthy(EvaluateExpr(stmt.Condition)))
                Execute(stmt.Then, ref shouldBreak);
            else if (stmt.Else != null)
                Execute(stmt.Else, ref shouldBreak);
        }

        private void Declaration(DeclarationStmt stmt)
        {
           var value = stmt?.Initializer != null 
                ? EvaluateExpr(stmt.Initializer) 
                : null;

            _env.Define(stmt.Name.Lexeme, value);
        }

        private void DeclarationStmt(DeclarationStmt stmt, ref bool shouldBreak) => Declaration(stmt);

        private void VariableStmt(VariableStmt stmt, ref bool shouldBreak) => Declaration(stmt.Declaration);

        private void ExpressionStmt(ExpressionStmt stmt, ref bool shouldBreak) => EvaluateExpr(stmt.Expression);

        private void BlockStmt(BlockStmt stmt, Environment environment, ref bool shouldBreak) 
        {
            var prevEnv = _env;
            try
            {
                _env = environment;

                foreach (var s in stmt.Statements)
                    Execute(s, ref shouldBreak);
            }
            finally
            {
                _env = prevEnv;
            }
        }

        private void PrintStmt(PrintStmt stmt, ref bool shouldBreak)
        {
            var value = EvaluateExpr(stmt.Expression);

            Console.WriteLine(Stringify(value));
        }

        private void WhileStmt(WhileStmt stmt, ref bool shouldBreak)
        {
            while (IsTruthy(EvaluateExpr(stmt.Condition)) && !shouldBreak)
                Execute(stmt.Body, ref shouldBreak);
        }

        private void BreakStmt(BreakStmt stmt, ref bool shouldBreak)
        {
            shouldBreak = true;
        }

        private object Logical(Logical expr)
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

        private object Assignment(Assignment expr)
        {
            var value = EvaluateExpr(expr.Value);

            _env.Assign(expr.Name, value);

            return value;
        }

        private object Literal(Literal expr) => expr.Value;

        private object Grouping(Grouping expr) => EvaluateExpr(expr.Expression);

        private object Variable(Variable expr) => _env[expr.Name];

        private object Unary(Unary expr)
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

        private object Binary(Binary expr)
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
        
        private object Ternary(Ternary expr)
        {
            var cond = EvaluateExpr(expr.Cond);

            return IsTruthy(cond) ? EvaluateExpr(expr.Left) : EvaluateExpr(expr.Right);
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