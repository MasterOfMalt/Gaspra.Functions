using Gaspra.MergeSprocs.Extensions;
using Gaspra.MergeSprocs.Interfaces;
using Gaspra.MergeSprocs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gaspra.MergeSprocs.Generators
{
    public class MergeProcedureGenerator : IMergeProcedureGenerator
    {
        private readonly ILogger logger;

        public MergeProcedureGenerator(
            ILogger<MergeProcedureGenerator> logger)
        {
            this.logger = logger;
        }

        public IEnumerable<MergeProcedureVariables> Generate(IEnumerable<DatabaseTable> tables)
        {
            var factTables = tables
                .Where(t =>
                    t.ExtendedProperties.Any(e => e.PropertyName.Equals("MergeIdentifier")));

            var tablesWithoutLinks = tables     //todo: easier to work without this cyclic dependency for now
                .Where(t =>                     //todo: easier to work without this cyclic dependency for now
                    !t.Name.EndsWith("Link"));  //todo: easier to work without this cyclic dependency for now

            var dependencyTrees = new List<DatabaseTableDependencyTree>();

            foreach(var factTable in factTables)
            {
                dependencyTrees.Add(new DatabaseTableDependencyTree(factTable, tablesWithoutLinks));
            }

            var mergeProcedures = new List<MergeProcedureVariables>();

            try
            {
                foreach (var dependencyTree in dependencyTrees)
                {
                    var maxDepth = dependencyTree.TreeMaxDepth();

                    for (var depth = maxDepth; depth > 0; depth--)
                    {
                        /*logging*/
                        var tablesAtDepth = dependencyTree.GetAllTablesWithDepth(depth);

                        foreach (var table in tablesAtDepth)
                        {
                            logger.LogDebug($"{table.Name}");
                        }
                        /*logging*/

                        var dependenciesAtDepth = dependencyTree.GetAllDependenciesWithDepth(depth);

                        foreach (var dependency in dependenciesAtDepth)
                        {
                            mergeProcedures.Add(dependency.GetMergeProcedureVariables(tables));
                        }

                    }
                }
            } catch (Exception ex)
            {
                logger.LogInformation(ex.Message);
            }

            return mergeProcedures;
        }

        public string GenerateMergeProcedure(MergeProcedureVariables variables)
        {
            var sproc = variables.BuildMergeSproc();

            return sproc;
        }
    }
}