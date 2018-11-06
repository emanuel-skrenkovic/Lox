using System;
using System.Collections.Generic;

namespace Lox
{
    public class Interpreter
    {
        private readonly Environment env = new Environment();

        public Interpreter() { }

        public void Interpret(IList<Stmt> stmts)
        {
            try 
            {
                foreach (var s in stmts)
                    Execute(s);
            }
            catch (RuntimeError err)
            {
                Lox.RuntimeError(err);
            }
        }

        private void Execute(Stmt stmt)
        {
            switch (stmt)
            {
                case DeclarationStmt declarationStmt:
                    DeclarationStmt(declarationStmt);
                    break;

                case VariableStmt variableStmt:
                    VariableStmt(variableStmt);
                    break;

                case ExpressionStmt exprStmt:
                    ExpressionStmt(exprStmt);
                    break;

                case PrintStmt printStmt:
                    PrintStmt(printStmt);
                    break; 
            }
        }

        private object EvaluateExpr(Expr expr)
        {
            try 
            {
                switch (expr)
                {
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

        private void Declaration(DeclarationStmt stmt)
        {
           var value = stmt?.Initializer != null 
                ? EvaluateExpr(stmt.Initializer) 
                : null;

            env.Define(stmt.Name.Lexeme, value);
        }

        private void DeclarationStmt(DeclarationStmt stmt)
        {
            Declaration(stmt);
        }

        private void VariableStmt(VariableStmt stmt)
        {
            // var value = stmt?.Initializer != null 
            //     ? EvaluateExpr(stmt.Initializer) 
            //     : null;

            // env.Define(stmt.Name.Lexeme, value);

            Declaration(stmt.Declaration);
        }

        private void ExpressionStmt(ExpressionStmt stmt)
        {
            EvaluateExpr(stmt.Expression);
        }

        private void PrintStmt(PrintStmt stmt)
        {
            var value = EvaluateExpr(stmt.Expression);

            Console.WriteLine(Stringify(value));
        }

        private object Assignment(Assignment expr)
        {
            var value = EvaluateExpr(expr.Value);

            env.Assign(expr.Name, value);

            return value;
        }

        private object Literal(Literal expr) => expr.Value;

        private object Grouping(Grouping expr) => EvaluateExpr(expr.Expression);

        private object Variable(Variable expr) => env.Get(expr.Name);

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