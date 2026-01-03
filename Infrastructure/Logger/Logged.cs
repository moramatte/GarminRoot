namespace Infrastructure.Logger
{
	public record Logged(object Caller, LogLevel Level, string Message);
}
