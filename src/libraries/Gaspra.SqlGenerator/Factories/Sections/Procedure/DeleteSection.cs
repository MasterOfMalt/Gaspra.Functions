using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections.Procedure
{
    // public class DeleteSection : IScriptSection
    // {
    //     private readonly IScriptLineFactory _scriptLineFactory;
    //
    //     public ScriptOrder Order { get; } = new(new[] { 1, 2, 6 });
    //
    //     public DeleteSection(IScriptLineFactory scriptLineFactory)
    //     {
    //         _scriptLineFactory = scriptLineFactory;
    //     }
    //
    //     public Task<bool> Valid(IMergeScriptVariableSet variableSet)
    //     {
    //         var matchOn = variableSet.MergeIdentifierColumns.Select(c => c.Name);
    //
    //         var deleteOn = variableSet.DeleteIdentifierColumns.Select(c => c.Name);
    //
    //         var deleteOnFactId = matchOn.Where(m => !deleteOn.Any(d => d.Equals(m))).FirstOrDefault();
    //
    //         return Task.FromResult(!string.IsNullOrWhiteSpace(deleteOnFactId) && deleteOn.Any());
    //     }
    //
    //     public async Task<string> Value(IMergeScriptVariableSet variableSet)
    //     {
    //         var mergeStatement = new List<string>
    //         {
    //             $"INSERT INTO",
    //             $"    @MergeResult",
    //             $"SELECT DISTINCT",
    //             $"    'DELETE',",
    //             $"    {variableSet.Table.Name}.{variableSet.Table.Name}Id"
    //             //$"DELETE",
    //             //$"    Delete{variables.Table.Name}",
    //             //$"FROM"
    //         };
    //
    //         var matchOn = variableSet.MergeIdentifierColumns.Select(c => c.Name);
    //
    //         var deleteOn = variableSet.DeleteIdentifierColumns.Select(c => c.Name);
    //
    //         var deleteOnFactId = matchOn.Where(m => !deleteOn.Any(d => d.Equals(m))).FirstOrDefault();
    //
    //         var columnLines = variableSet.Table.Columns.Where(c => matchOn.Any(m => m.Equals(c.Name))).Where(c => c.Constraints != null);
    //
    //         foreach (var columnLine in columnLines)
    //         {
    //             var line = $"    ,{variableSet.Table.Name}.{columnLine.Name}";
    //
    //             mergeStatement.Add(line);
    //         }
    //
    //         mergeStatement.AddRange(new List<string>
    //         {
    //             $"FROM",
    //             $"    [{variableSet.Schema.Name}].[{variableSet.Table.Name}] {variableSet.Table.Name}",
    //             $"    INNER JOIN @MergeResult mergeResultInner ON {variableSet.Table.Name}.{matchOn.Where(m => !deleteOn.Any(d => d.Equals(m))).FirstOrDefault()} = mergeResultInner.{matchOn.Where(m => !deleteOn.Any(d => d.Equals(m))).FirstOrDefault()}",
    //             $"    LEFT JOIN @MergeResult mergeResultOuter ON {variableSet.Table.Name}.{variableSet.Table.Name}Id = mergeResultOuter.{variableSet.Table.Name}Id",
    //             $"WHERE",
    //             $"    mergeResultOuter.{variableSet.Table.Name}Id IS NULL",
    //             $"",
    //             $"DELETE",
    //             $"    {variableSet.Table.Name}",
    //             $"FROM",
    //             $"    [Analytics].[{variableSet.Table.Name}] {variableSet.Table.Name}",
    //             $"    INNER JOIN @MergeResult mr ON {variableSet.Table.Name}.{variableSet.Table.Name}Id=mr.{variableSet.Table.Name}Id",
    //             $"WHERE",
    //             $"    mr.MergeAction='DELETE'",
    //         });
    //
    //         var scriptLines = await _scriptLineFactory.LinesFrom(
    //             1,
    //             mergeStatement.ToArray()
    //             );
    //
    //         return await _scriptLineFactory.StringFrom(scriptLines);
    //     }
    // }
}
