namespace Gaspra.SqlGenerator.Models
{
    public class MergeScript
    {
        public string Name { get; }
        public string Script { get; }

        public MergeScript(string name, string script)
        {
            Name = name;
            Script = script;
        }
    }
}
