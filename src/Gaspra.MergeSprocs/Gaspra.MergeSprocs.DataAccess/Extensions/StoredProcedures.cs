using System.IO;

namespace Gaspra.MergeSprocs.DataAccess.Extensions
{
    public static class StoredProcedures
    {
        public static string GetColumnInformation()
        {
            return File
                .ReadAllText($@"{Directory.GetCurrentDirectory()}\StoredProcedures\Get_ColumnInformation.sql");
        }

        public static string GetFKConstraintInformation()
        {
            return File
                .ReadAllText($@"{Directory.GetCurrentDirectory()}\StoredProcedures\Get_FKConstraints.sql");
        }

        public static string GetExtendedProperties()
        {
            return File
                .ReadAllText($@"{Directory.GetCurrentDirectory()}\StoredProcedures\Get_ExtendedProperties.sql");
        }
    }
}
