namespace Lox
{
	public interface IExpressionMatcher
	{
		object MatchExpression(Expr expression);
	}
}
