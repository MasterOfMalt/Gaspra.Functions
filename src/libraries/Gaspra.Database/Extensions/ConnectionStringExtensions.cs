using System.Linq;

namespace Gaspra.Database.Extensions
{
    public static class ConnectionStringExtensions
    {
        public static string DatabaseName(this string connectionString)
        {
            var databaseSection = connectionString
                .Split(";")
                .Where(c => c.StartsWith("Database="))
                .FirstOrDefault();

            var databaseName = databaseSection
                .Split("=")
                .Last();

            return databaseName;
        }
    }
}
