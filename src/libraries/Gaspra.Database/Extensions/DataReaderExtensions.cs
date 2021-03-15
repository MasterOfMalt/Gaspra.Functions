using Gaspra.Database.Models;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Gaspra.Database.Extensions
{
    public static class DataReaderExtensions
    {
        public static IReadOnlyCollection<ColumnModel> ReadColums(this SqlDataReader dataReader)
        {
            return new List<ColumnModel>();
        }

        public static IReadOnlyCollection<ConstraintModel> ReadConstraints(this SqlDataReader dataReader)
        {
            return new List<ConstraintModel>();
        }

        public static IReadOnlyCollection<PropertyModel> ReadProperties(this SqlDataReader dataReader)
        {
            return new List<PropertyModel>();
        }
    }
}