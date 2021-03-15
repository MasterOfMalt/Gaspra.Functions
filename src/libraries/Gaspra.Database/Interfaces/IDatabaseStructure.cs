using Gaspra.Database.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gaspra.Database.Interfaces
{
    public interface IDatabaseStructure
    {
        Task<DatabaseModel> CalculateStructure(
            IReadOnlyCollection<ColumnModel> columns,
            IReadOnlyCollection<ConstraintModel> constraints,
            IReadOnlyCollection<PropertyModel> properties);
    }
}
