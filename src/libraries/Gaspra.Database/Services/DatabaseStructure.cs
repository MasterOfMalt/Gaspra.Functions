using Gaspra.Database.Interfaces;
using Gaspra.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gaspra.Database.Services
{
    public class DatabaseStructure : IDatabaseStructure
    {
        public Task<DatabaseModel> CalculateStructure(IReadOnlyCollection<ColumnModel> columns, IReadOnlyCollection<ConstraintModel> constraints, IReadOnlyCollection<PropertyModel> properties)
        {
            throw new NotImplementedException();
        }
    }
}
