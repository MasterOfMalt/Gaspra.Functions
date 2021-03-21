namespace Gaspra.Database.Models.QueryResults
{
    public class ConstraintResult
    {
        public string ConstraintSchema { get; set; }
        public string ConstraintName { get; set; }
        public string ConstraintTable { get; set; }
        public string ConstraintColumn { get; set; }
        public string ReferencedTable { get; set; }
        public string ReferencedColumn { get; set; }
    }
}
