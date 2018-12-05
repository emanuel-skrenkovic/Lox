namespace Lox
{
    public interface IStatementMatcher
    {
         void MatchStatement(Stmt statement);
    }
}