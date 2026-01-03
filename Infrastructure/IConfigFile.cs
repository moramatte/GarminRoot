namespace Infrastructure
{
    public interface IVersionableConfigFile
    {
        int LatestVersion { get;  }

        int Version { get; set; }

        void Migrate();
    }
}
