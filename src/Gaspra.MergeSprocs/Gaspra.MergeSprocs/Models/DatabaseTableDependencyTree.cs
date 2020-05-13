using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gaspra.MergeSprocs.Models
{
    public class DatabaseTableDependencyTree
    {
        public DatabaseTable ParentTable;

        public int Depth;

        public IEnumerable<DatabaseTableDependencyTree> ChildrenTables;

        public DatabaseTableDependencyTree(DatabaseTable parent, IEnumerable<DatabaseTable> tables, IEnumerable<DatabaseTable> ignore = null, int depth = 1)
        {
            ParentTable = parent;

            Depth = depth;

            var fkRefs = tables
                .Where(t => t
                    .ForeignKeys.Any(f =>
                        f.ReferenceSchema.Equals(parent.Schema) &&
                        f.ReferenceTable.Equals(parent.Name))).ToList();

            fkRefs.AddRange(parent.ForeignKeys.SelectMany(f => tables.Where(t => t.Name.Equals(f.ReferenceTable))));

            var deeper = ++depth;

            var ignoreList = new List<DatabaseTable>() { ParentTable };

            if (ignore != null)
            {
                ignoreList.AddRange(ignore);
            }

            var children = fkRefs
                .Where(f => !ignoreList.Any(i => i.Equals(f)))
                .Select(t => new DatabaseTableDependencyTree(t, tables, ignoreList, deeper));

            ChildrenTables = children;
        }

        public int TreeMaxDepth()
        {
            var depth = Depth;

            foreach (var child in ChildrenTables)
            {
                var childDepth = child.TreeMaxDepth();

                if (childDepth > depth)
                {
                    depth = childDepth;
                }
            }

            return depth;
        }

        public IEnumerable<DatabaseTable> GetAllTablesWithDepth(int depth)
        {
            var databaseTables = new List<DatabaseTable>();

            if (Depth.Equals(depth))
            {
                databaseTables.Add(ParentTable);
            }
            else
            {
                foreach (var child in ChildrenTables)
                {
                    databaseTables.AddRange(child.GetAllTablesWithDepth(depth));
                }
            }

            return databaseTables;
        }

        public IEnumerable<DatabaseTableDependencyTree> GetAllDependenciesWithDepth(int depth)
        {
            var dependencies = new List<DatabaseTableDependencyTree>();

            if (Depth.Equals(depth))
            {
                dependencies.Add(this);
            }
            else
            {
                foreach (var child in ChildrenTables)
                {
                    dependencies.AddRange(child.GetAllDependenciesWithDepth(depth));
                }
            }

            return dependencies;
        }

        public bool ChildTableIncludesForeignKey(DatabaseForeignKeyConstraint foreignKey)
        {
            var hasForeignKey = false;

            foreach(var child in ChildrenTables)
            {
                if(child.ParentTable.Columns.Any(c => c.Name.Equals(foreignKey.ReferenceColumn)))
                {
                    hasForeignKey = true;
                }
            }

            if(!hasForeignKey)
            {
                foreach (var child in ChildrenTables)
                {
                    child.ChildTableIncludesForeignKey(foreignKey);
                }
            }

            return hasForeignKey;
        }

        public IEnumerable<DatabaseColumn> GetMergeColumns()
        {
            var columns = ParentTable
                .Columns
                .Where(c => !ParentTable.ForeignKeys.Any(f =>
                        f.ConstraintColumn.Equals(c.Name)
                        && ChildTableIncludesForeignKey(f)))
                .Where(c => !c.IdentityColumn)
                .ToList();

            foreach (var child in ChildrenTables.Where(c => ParentTable.ForeignKeys.Any(f => f.ReferenceTable.Equals(c.ParentTable.Name))))
            {
                var childColumns = child
                    .GetMergeColumns();

                columns
                    .AddRange(childColumns);
            }

            return columns;
        }

        public DatabaseTable GetTableOfColumn(DatabaseColumn column)
        {
            DatabaseTable databaseTable = null;

            foreach (var child in ChildrenTables)
            {
                if (child.ParentTable.Columns.Any(c => c.Name.Equals(column.Name)))
                {
                    databaseTable = child.ParentTable;
                }
            }

            if (databaseTable == null)
            {
                foreach (var child in ChildrenTables)
                {
                    child.GetTableOfColumn(column);
                }
            }

            return databaseTable;
        }

        public MergeProcedureVariables GetMergeProcedureVariables(IEnumerable<DatabaseTable> allTables)
        {
            var variables = new MergeProcedureVariables
            {
                SchemaName = ParentTable.Schema,
                TableName = ParentTable.Name,
                ProcedureName = $"Merge{ParentTable.Name}",
                TableTypeName = $"TT_{ParentTable.Name}",
                TableTypeVariableName = $"{ParentTable.Name}",
                DatabaseTable = ParentTable,
                Dependencies = ChildrenTables
            };

            /*calculate merge identifying columns*/
            var identifierColumns = new List<DatabaseColumn>();

            if (ParentTable.ExtendedProperties != null && ParentTable.ExtendedProperties.Any())
            {
                var mergeIdentifier = ParentTable.ExtendedProperties.Where(t => t.PropertyName.Equals("MergeIdentifier")).FirstOrDefault();

                if (mergeIdentifier != null)
                {
                    var identifierColumn = ParentTable
                        .Columns
                        .Where(c => c.Name.Equals(mergeIdentifier.Value))
                        .FirstOrDefault();

                    identifierColumns
                        .Add(identifierColumn);
                }
            }

            if(ParentTable.Columns.Count().Equals(2))
            {
                identifierColumns.Add(ParentTable.Columns.Where(c => !c.IdentityColumn).FirstOrDefault());
            }

            if (ParentTable.ForeignKeys != null && ParentTable.ForeignKeys.Any())
            {
                var fkColumns = ParentTable.Columns.Where(c => ParentTable.ForeignKeys.Any(f => f.ConstraintColumn.Equals(c.Name)));

                identifierColumns.AddRange(fkColumns);
            }

            variables.MergeIdentifierColumns = identifierColumns;

            /*calculate the values needed in the table type*/
            var tableTypeColumns = GetMergeColumns().ToList();

            variables.TableTypeColumns = tableTypeColumns;

            /*calculate the joining tables*/
            if (tableTypeColumns.Any(c => !ParentTable.Columns.Any(t => c.Name.Equals(t.Name))))
            {
                var columnsNotInParent = tableTypeColumns
                    .Where(c => !ParentTable.Columns.Any(t => c.Name.Equals(t.Name)));

                var join = columnsNotInParent.Select(c => (GetTableOfColumn(c), c)).ToList();

                variables.JoiningColumns = join;
            }

            if (tableTypeColumns.Any(c => c.Name.Equals("OrderFactId") && !ParentTable.Name.Equals("OrderFact")))
            {
                var columns = tableTypeColumns.ToList();

                var columnToRemove = columns.Where(c => c.Name.Equals("OrderFactId")).FirstOrDefault();

                columns.Remove(columnToRemove);

                columns.Add(
                    allTables.Where(t => t.Name.Equals("OrderFact")).FirstOrDefault().Columns.Where(c => c.Name.Equals("OrderId")).FirstOrDefault()
                    );

                variables.TableTypeColumns = columns;

                if(variables.JoiningColumns != null)
                {
                    var joinVariables = variables.JoiningColumns.ToList();

                    var variableToRemove = joinVariables.Where(t => t.Item2.Name.Equals("OrderFactId")).FirstOrDefault();

                    joinVariables.Remove(variableToRemove);

                    joinVariables.Add((
                        allTables.Where(t => t.Name.Equals("OrderFact")).FirstOrDefault(),
                        allTables.Where(t => t.Name.Equals("OrderFact")).FirstOrDefault().Columns.Where(c => c.Name.Equals("OrderId")).FirstOrDefault()
                        ));

                    variables.JoiningColumns = joinVariables;
                }
                else
                {
                    var joinVariables = new List<(DatabaseTable, DatabaseColumn)>();

                    joinVariables.Add((
                        allTables.Where(t => t.Name.Equals("OrderFact")).FirstOrDefault(),
                        allTables.Where(t => t.Name.Equals("OrderFact")).FirstOrDefault().Columns.Where(c => c.Name.Equals("OrderId")).FirstOrDefault()
                        ));

                    variables.JoiningColumns = joinVariables;
                }
            }




            //if (!ParentTable.Name.Equals("OrderFact") && ParentTable.Columns.Any(c => c.Name.Equals("OrderFactId"))) //need to automate this
            //{
            //    tableTypeColumns.Add(allTables.Where(t => t.Name.Equals("OrderFact")).FirstOrDefault().Columns.Where(c => c.Name.Equals("OrderId")).FirstOrDefault());

            //    var join = variables.JoiningColumns.ToList();

            //    join.Add(( //need to automate this
            //        allTables.Where(t => t.Name.Equals("OrderFact")).FirstOrDefault(),
            //        allTables.Where(t => t.Name.Equals("OrderFact")).FirstOrDefault().Columns.Where(c => c.Name.Equals("OrderId")).FirstOrDefault()
            //        ));

            //    variables.JoiningColumns = join;
            //}



            return variables;
        }
    }
}
