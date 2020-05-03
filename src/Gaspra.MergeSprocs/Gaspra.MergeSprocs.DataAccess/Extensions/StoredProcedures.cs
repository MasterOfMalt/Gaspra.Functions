using System.IO;

namespace Gaspra.MergeSprocs.DataAccess.Extensions
{
    public static class StoredProcedures
    {
        public static string GetTableInformation()
        {
            return File
                .ReadAllText(@"C:\Git\Gaspra.DatabaseUtility\src\Gaspra.MergeSprocs\.externalSprocs\Get_TableInformation.sql");
        }
    }
}
