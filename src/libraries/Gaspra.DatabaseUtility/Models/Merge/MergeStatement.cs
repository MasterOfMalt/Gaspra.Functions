namespace Gaspra.DatabaseUtility.Models.Merge
{
    public class MergeStatement
    {
        public string Statement { get; set; }
        public MergeVariables Variables { get; set; }

        public MergeStatement(
            string statement,
            MergeVariables variables)
        {
            Statement = statement;
            Variables = variables;
        }
    }
}
