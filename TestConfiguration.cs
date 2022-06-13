using System;


namespace vMotion.Api.Specs;

internal static class TestConfiguration
{
    private static readonly string ConnectionString = Environment.GetEnvironmentVariable("MONGODB_URI") ?? "mongodb://localhost";

    private static readonly string _dbName = $"TestRun_{DateTime.Now:yyyyMMdd_HHmmss}";

    internal static string GetDbName(string i = "0000") => _dbName + $"_{i}";

    internal static string GetConnectionString(string i = "0000")
    {
        var builder = new UriBuilder(ConnectionString)
        {
            Path = GetDbName(i)
        };

        return builder.ToString();
    }

    internal static string GetConnectionUrl(string i = "0000")
    {
        var builder = new UriBuilder(GetConnectionString(i));

        return builder.ToString();
    }

}
