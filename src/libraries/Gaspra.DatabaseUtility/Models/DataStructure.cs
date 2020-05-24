using Gaspra.DatabaseUtility.Models.Database;
using Gaspra.DatabaseUtility.Models.Tree;

namespace Gaspra.DatabaseUtility.Models
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
