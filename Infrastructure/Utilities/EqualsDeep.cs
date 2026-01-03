namespace Infrastructure.Utilities
{
    public record Difference(string PropertyName, object Expected, object Actual);
}
