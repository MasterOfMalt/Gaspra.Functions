using Gaspra.MergeSprocs.Extensions;
using Gaspra.MergeSprocs.Miro.Extensions;
using Gaspra.MergeSprocs.Miro.Models;
using Gaspra.MergeSprocs.Models;
using Gaspra.MergeSprocs.Models.Database;
using Gaspra.MergeSprocs.Models.Merge;
using Gaspra.MergeSprocs.Models.Tree;
using Newtonsoft.Json;
using Refit;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gaspra.MergeSprocs.Miro
{
    public class DrawDataStructure
    {
        private readonly HttpClient httpClient;
        private readonly IMiroEndpoints miroEndpoints;
        private readonly string boardId = "o9J_ksC_qs4=";
        private readonly string baseAddress = "https://api.miro.com/v1/boards";

        public DrawDataStructure()
        {
            httpClient = new HttpClient() { BaseAddress = new Uri($"{baseAddress}/{boardId}") };
            miroEndpoints = RestService.For<IMiroEndpoints>(httpClient);
        }

        public async Task DrawToMiro(DataStructure dataStructure)
        {
            var orderedBranches = dataStructure
                .DependencyTree
                .Branches
                .OrderBy(b => b.Depth);

            var maxDepth = orderedBranches
                .Last()
                .Depth;

            var tableGroups = new List<TableGroup>();

            for(var depth = maxDepth; depth > 0; depth--)
            {
                //select a table
                foreach(var tableGuid in orderedBranches.Where(b => b.Depth.Equals(depth)).Select(b => b.TableGuid))
                {
                    if(!tableGroups.ChildrenContainsGuid(tableGuid))
                    {
                        //get parent
                        var parentTableGuid = dataStructure
                            .DependencyTree
                            .GetParentOf(dataStructure
                                .Schema
                                .GetTableFrom(tableGuid));

                        if(parentTableGuid != null && parentTableGuid != Guid.Empty)
                        {
                            //get all the children fro parent table
                            var childrenTables = dataStructure
                                .DependencyTree
                                .GetChildrenOf(dataStructure
                                    .Schema
                                    .GetTableFrom(parentTableGuid));

                            tableGroups.Add(new TableGroup
                            {
                                Parent = parentTableGuid,
                                Children = childrenTables,
                                Depth = depth
                            });
                        }
                    }
                }
            }

            foreach(var tableGroup in tableGroups)
            {
                tableGroup.RelatedGroups = tableGroups.RelatedTo(tableGroup.Parent);
            }

            var tableDrawn = new List<(string, PointF)>();

            var centerPoint = new PointF(0, 0);

            foreach (var tableGroup in tableGroups.OrderBy(g => g.Depth))
            {
                var parentTable = dataStructure.Schema.GetTableFrom(tableGroup.Parent);

                if(!tableDrawn.Select(td => td.Item1).Contains(parentTable.Name))
                {
                    tableDrawn.Add((await DrawWidget(centerPoint, parentTable), centerPoint));
                }

                centerPoint = tableDrawn.Where(td => td.Item1.Equals(parentTable.Name)).FirstOrDefault().Item2;

                var circularPoints = MiroExtensions.GetCircularPoints(9000 - (tableGroup.Depth * 2500), centerPoint, ((2 * Math.PI) - 1) / tableGroup.Children.Count());

                var circularCount = 0;

                foreach (var child in tableGroup.Children)
                {
                    var childTable = dataStructure.Schema.GetTableFrom(child);

                    if(!tableDrawn.Select(td => td.Item1).Contains(childTable.Name))
                    {
                        tableDrawn.Add((await DrawWidget(circularPoints[circularCount], childTable), circularPoints[circularCount]));

                        circularCount++;
                    }
                }

                centerPoint.X += 10000;
            }

            /*
             * lines
             */
            var widgetsJson = await miroEndpoints.GetWidgets();

            var widgets = JsonConvert.DeserializeObject<Widgets>(widgetsJson);

            var linksDrawn = new List<(string, string)>();

            foreach (var table in dataStructure.Schema.Tables)
            {
                var dependencies = TableDependencies.From(table, dataStructure.Schema);

                var links = dependencies
                    .ConstrainedToTables
                    .ToList();

                var tableId = widgets.Data.Where(w => w.Text.Equals($"<b>{table.Name}</b>")).FirstOrDefault();

                if (tableId != null)
                {
                    foreach (var link in links)
                    {
                        var linkId = widgets.Data.Where(w => w.Text.Equals($"<b>{link.Name}</b>")).FirstOrDefault();

                        var linksDrawnContains = linksDrawn
                            .Where(l =>
                                (l.Item1.Equals(tableId.Text) && l.Item2.Equals(linkId.Text) ||
                                (l.Item1.Equals(linkId.Text) && l.Item2.Equals(tableId.Text))));

                        if (linkId != null && !linksDrawnContains.Any())
                        {
                            var miroLine = new MiroLine
                            {
                                StartId = tableId.Id,
                                EndId = linkId.Id,
                                Style = new Dictionary<string, object>
                                {
                                    { "borderColor", "#ff7b00" },
                                    { "borderStyle", "normal" },
                                    { "borderWidth", 4.0 },
                                    { "lineEndType", "none" },
                                    { "lineStartType", "opaque_rhombus" },
                                    { "lineType", "bezier" }
                                }
                            };

                            await MiroDrawWidget(miroLine.ToDictionary(), $"line: {tableId.Text} -> {linkId.Text}");

                            linksDrawn.Add((tableId.Text, linkId.Text));
                        }
                    }
                }
            }
        }

        private async Task<string> DrawWidget(PointF position, Table table)
        {
            var miroShapeTableHeader = new MiroShape
            {
                PosX = (int)position.X,
                PosY = (int)position.Y,
                Width = 300,
                Height = 40,
                Text = $"<b>{table.Name}</b>",
                Style = new Dictionary<string, object>
                {
                    { "backgroundColor", "#f09800" },
                    { "borderOpacity", 0.0 },
                    { "fontSize", 16 },
                    { "textColor", "#ebebeb" },
                    { "textAlign", "left" }
                }
            };

            var miroShapeTableColumns = new MiroShape
            {
                PosX = (int)position.X,
                PosY = (int)position.Y + ((table.Columns.Count() * 25) / 2) + 20,
                Width = 300,
                Height = table.Columns.Count() * 25,
                Text = $"{string.Join("", table.Columns.Select(c => $"<p>{c.Name} [{c.DataType}]</p>"))}",
                Style = new Dictionary<string, object>
                {
                    { "backgroundColor", "#00246b" },
                    { "borderOpacity", 0.0 },
                    { "fontSize", 16 },
                    { "textColor", "#ebebeb" },
                    { "textAlign", "left" },
                    { "textAlignVertical", "top" }
                }
            };


            await MiroDrawWidget(miroShapeTableHeader.ToDictionary(), table.Name);
            await MiroDrawWidget(miroShapeTableColumns.ToDictionary(), $"{table.Name} columns");

            return table.Name;
        }

        public async Task MiroDrawWidget(Dictionary<string, object> widget, string name)
        {
            try
            {
                await miroEndpoints.DrawWidget(widget);

                Console.WriteLine($"drawn widget: {name}");
            }
            catch (Exception ex)
            {
                var exception = ex;

                Console.WriteLine($"waiting due to: {ex.Message}");

                Thread.Sleep(5000);

                await MiroDrawWidget(widget, name);
            }
        }

    }

    public class TableGroup
    {
        public Guid Parent { get; set; }
        public IEnumerable<Guid> Children { get; set; }
        public int Depth { get; set; }
        public IEnumerable<TableGroup> RelatedGroups { get; set; }
        public PointF Positioned { get; set; }

        public TableGroup()
        {
            RelatedGroups = new List<TableGroup>();
        }

    }

    public static class TableGroupExtensions
    {
        public static bool ChildrenContainsGuid(this IEnumerable<TableGroup> tableGroups, Guid tableGuid)
        {
            var containsTableGuid = false;

            foreach(var tableGroup in tableGroups)
            {
                if (tableGroup.Children.Contains(tableGuid))
                {
                    containsTableGuid = true;
                }
            }

            return containsTableGuid;
        }

        public static IEnumerable<TableGroup> RelatedTo(this IEnumerable<TableGroup> tableGroups, Guid tableGuid)
        {
            var relatedTo = new List<TableGroup>();

            foreach(var tableGroup in tableGroups)
            {
                if(tableGroup.Children.Contains(tableGuid))
                {
                    relatedTo.Add(tableGroup);
                }
            }

            return relatedTo;
        }
    }

    public class DrawPosition
    {
        public Guid TableGuid { get; set; }

        public PointF Position { get; set; }
    }
}
