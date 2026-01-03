namespace InfrastructureTests
{
    public static class TestCategories
    {
        private const string pipelineExclusionToken = "Exclude";

        public const string LocalOnly = pipelineExclusionToken;
        public const string Ignored = pipelineExclusionToken;
        public const string CertificateDependent = pipelineExclusionToken;
        public const string FullSystemTest = pipelineExclusionToken;

        public const string Flaky = "Flaky";
    }
}
