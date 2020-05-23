using Gaspra.MergeSprocs.Models.Database;
using Gaspra.MergeSprocs.Models.Tree;

namespace Gaspra.MergeSprocs.Models
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
