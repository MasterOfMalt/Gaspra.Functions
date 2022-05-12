namespace Gaspra.SqlGenerator.Models
{
    public class SqlScript
    {
        public string Name { get; }
        public string Script { get; }

        public SqlScript(string name, string script)
        {
            Name = name;
            Script = script;
        }
    }
}
