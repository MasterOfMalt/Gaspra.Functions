using System;

namespace Gaspra.DatabaseProcesses.Extensions
{
    public static class DataReaderExtensions
    {
        public static T GetValue<T>(this object data)
        {
            if (data.GetType().Equals(typeof(DBNull)))
            {
                return default;
            }

            var nullableType = Nullable.GetUnderlyingType(typeof(T));

            if (nullableType != null)
            {
                return (T)Convert.ChangeType(data, nullableType);
            }

            return (T)data;
        }

        public static DateTime? GetDateTimeOrNull(this object data)
        {
            if (data.GetType().Equals(typeof(DBNull)))
            {
                return null;
            }

            var dateValue = (DateTime)data;

            if (DateTime.MinValue.Equals(dateValue))
            {
                return null;
            }

            return dateValue;
        }
    }
}
