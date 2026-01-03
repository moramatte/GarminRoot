namespace Infrastructure.Logger
{
	public interface ILogIdentifiable
	{
		string Name { get; set; }
		string StatusText { get; }
	}
}
