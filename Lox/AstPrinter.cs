using System.Text;

namespace Lox
{
    public static  class AstPrinter
    {
        public static string Print(Expr expr)
        {
            switch (expr)
            {
                case TernaryExpr ternary:
                    return Parenthesize("conditional", ternary.Cond, ternary.Left, ternary.Right);

                case BinaryExpr binary:
                    return Parenthesize(binary.Operator.Lexeme, binary.Left, binary.Right);

                case GroupingExpr grouping:
                    return Parenthesize("group", grouping.Expression);

                case LiteralExpr literal:
                    if (literal.Value == null) 
                        return "nil";

                    return literal.Value.ToString();

                case UnaryExpr unary:
                    return Parenthesize(unary.Operator.Lexeme, unary.Right);
                
                default: return "empty";
            }
        }

        private static string Parenthesize(string name, params Expr[] exprs)
        {
            var sb = new StringBuilder();

            sb.Append("(").Append(name);

            foreach (var expr in exprs)
            {
                sb.Append(" ");
                sb.Append(Print(expr));
            }
            sb.Append(")");

            return sb.ToString();
        }
    }
}