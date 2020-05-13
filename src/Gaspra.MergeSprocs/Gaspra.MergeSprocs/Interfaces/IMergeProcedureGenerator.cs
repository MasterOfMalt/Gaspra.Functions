using Gaspra.MergeSprocs.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gaspra.MergeSprocs.Interfaces
{
    public interface IMergeProcedureGenerator
    {
        IEnumerable<MergeProcedureVariables> Generate(IEnumerable<DatabaseTable> tables);

        string GenerateMergeProcedure(MergeProcedureVariables variables);
    }
}
