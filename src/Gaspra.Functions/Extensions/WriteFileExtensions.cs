using System.IO;

namespace Gaspra.Functions.Extensions
{
    public static class WriteFileExtensions
    {
        public static bool TryWriteFile(this string fileContents, string fileName, string output = @"*\.output")
        {
            var outputDirectory = output.Replace("*", $"{Directory.GetCurrentDirectory()}");

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            if (fileContents.Length == 0)
            {
                return false;
            }
            else
            {
                File.WriteAllText($@"{outputDirectory}\{fileName}", fileContents);

                return true;
            }
        }
    }
}
