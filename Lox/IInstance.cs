namespace Lox
{
    public interface IInstance
    {
        void Set(Token name, object value);

        object Get(Token name);
    }
}