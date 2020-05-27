namespace Gaspra.DatabaseProcesses.Extensions
{
    public static class StoredProcedureExtensions
    {
        public static string GetRunningProcesses()
        {
            return @"
SELECT
    session_id as sessionid,
    start_time as startime,
    status,
    command,
    DB_NAME(database_id) as [database],
    OBJECT_SCHEMA_NAME(objectid, database_id) as [schema],
    OBJECT_NAME(objectid) as [object],
    wait_time as waittime
FROM
    [sys].[dm_exec_requests] AS der
CROSS APPLY
    [sys].[dm_exec_sql_text](der.sql_handle) AS dest
";
        }
    }
}
