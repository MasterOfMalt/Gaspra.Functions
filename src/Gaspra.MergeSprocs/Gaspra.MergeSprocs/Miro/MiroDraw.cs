using Gaspra.MergeSprocs.Miro;
using Gaspra.MergeSprocs.Models.Database;
using Refit;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using System.Threading;
using System.Drawing;
using Gaspra.MergeSprocs.Models.Merge;
using Gaspra.MergeSprocs.Miro.Models;
using Gaspra.MergeSprocs.Miro.Extensions;

namespace Gaspra.MergeSprocs.Miro
{
    public class MiroDraw
    {
        private readonly HttpClient httpClient;
        private readonly IMiroEndpoints miroEndpoints;

        private readonly string boardId = "o9J_ksC_qs4=";
        private readonly string baseAddress = "https://api.miro.com/v1/boards";

        public MiroDraw()
        {
            httpClient = new HttpClient() { BaseAddress = new Uri($"{baseAddress}/{boardId}") };
            miroEndpoints = RestService.For<IMiroEndpoints>(httpClient);
        }

        private IEnumerable<(int depth, Table table)> TableObjectsConnected(Schema schema, TableTree tree, Table table, int depth)
        {
            var connectedTables = new List<(int depth, Table table)>();

            var nextDepth = depth + 1;

            var connected = tree.Branches.Where(b => b.depth.Equals(nextDepth) && b.dependencies.ConstrainedToTables.Any(c => c.Name.Equals(table.Name)));

            connectedTables.AddRange(connected.Select(b => (nextDepth, b.dependencies.CurrentTable)));

            foreach (var connect in connected)
            {
                connectedTables.AddRange(TableObjectsConnected(schema, tree, connect.dependencies.CurrentTable, nextDepth));
            }

            return connectedTables.Distinct();
        }

        public async Task Draw(Schema schema, TableTree tree)
        {
            int colNum = 0;

            var maxDepth = tree.Branches.Select(b => b.depth).OrderByDescending(b => b).First();

            var setsOfConnections = new List<List<(int depth, Table table)>>();

            foreach(var table in tree.Branches.Where(b => b.depth.Equals(1)).Select(b => b.dependencies.CurrentTable))
            {
                var tableConnections = TableObjectsConnected(schema, tree, table, 1).ToList();

                tableConnections.Add((1, table));

                setsOfConnections.Add(tableConnections);
            }

            var tableDrawn = new List<string>();

            var setCount = 0;

            foreach (var set in setsOfConnections)
            {
                var depthRange = set.Select(s => s.depth).OrderByDescending(s => s).First();

                var highestDepthSet = set.GroupBy(s => s.depth).Select(s => s).Select(s => (s.Key, s.Count()));

                for(var depth = 1; depth < depthRange+1; depth++)
                {
                    var tablesAtDepth = set.Where(s => s.depth.Equals(depth)).Select(s => s.table);

                    var circularPoints = MiroExtensions.GetCircularPoints(1000 * (depth - 1), new PointF(setCount, 0), ((2*Math.PI)-1) / tablesAtDepth.Count());

                    var tableCount = 0;

                    foreach (var table in tablesAtDepth)
                    {
                        if (!tableDrawn.Contains(table.Name))
                        {
                            var miroShapeTableHeader = new MiroShape
                            {
                                PosX = (int)circularPoints[tableCount].X,
                                PosY = (int)circularPoints[tableCount].Y,
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

                            await DrawWidget(miroShapeTableHeader.ToDictionary(), table.Name);

                            tableDrawn.Add(table.Name);

                            /*
                                font size 16 can draw 16 rows per 400 height. About 25px line height

                                posy offset is (header+body / 2)
                             */

                            var miroShapeTableColumns = new MiroShape
                            {
                                PosX = (int)circularPoints[tableCount].X,
                                PosY = (int)circularPoints[tableCount].Y + ((table.Columns.Count() * 25) / 2) + 20,
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

                            await DrawWidget(miroShapeTableColumns.ToDictionary(), $"{table.Name} columns");

                            ++colNum;

                            tableCount++;
                        }
                    }

                    colNum = 0;
                }

                setCount+=5000;
            }

            var widgetsJson = await miroEndpoints.GetWidgets();

            var widgets = JsonConvert.DeserializeObject<Widgets>(widgetsJson);

            var linksDrawn = new List<(string, string)>();

            foreach(var table in schema.Tables)
            {
                var dependencies = TableDependencies.From(table, schema);

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

                            await DrawWidget(miroLine.ToDictionary(), $"line: {tableId.Text} -> {linkId.Text}");

                            linksDrawn.Add((tableId.Text, linkId.Text));
                        }
                    }
                }
            }
        }


        public async Task DrawWidget(Dictionary<string, object> widget, string name)
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

                await DrawWidget(widget, name);
            }
        }
    }
}
