using Deprecated.Gaspra.DatabaseUtility.Models.Database;
using Deprecated.Gaspra.DatabaseUtility.Models.Tree;

namespace Deprecated.Gaspra.DatabaseUtility.Models
{
    public class DataStructure
    {
        public Schema Schema { get; set; }
        public DependencyTree DependencyTree { get; set; }

        public DataStructure(
            Schema schema,
            DependencyTree dependencyTree)
        {
            Schema = schema;
            DependencyTree = dependencyTree;
        }
    }
}
