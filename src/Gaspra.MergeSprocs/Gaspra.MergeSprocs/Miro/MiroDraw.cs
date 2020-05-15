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

        public async Task Draw(Schema schema, TableTree tree)
        {
            int rowNum = 0;

            int colNum = 0;

            var maxDepth = tree.Branches.Select(b => b.depth).OrderByDescending(b => b).First();

            for(var depth = 1; depth < maxDepth+1; depth++)
            {
                var tablesAtDepth = tree.Branches.Where(b => b.depth.Equals(depth)).Select(b => b.dependencies.CurrentTable);

                foreach (var table in tablesAtDepth)
                {
                    var miroShapeTableHeader = new MiroShape
                    {
                        PosX = colNum * 500,
                        PosY = depth * 600,
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

                    /*
                        font size 16 can draw 16 rows per 400 height. About 25px line height

                        posy offset is (header+body / 2)
                     */

                    var miroShapeTableColumns = new MiroShape
                    {
                        PosX = colNum * 500,
                        PosY = (depth * 600) + ((table.Columns.Count() * 25) / 2) + 20,
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
                }

                colNum = 0;
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



    public class MiroShape
    {
        public string Type { get; set; } = "shape";
        public int PosX { get; set; } = 0;
        public int PosY { get; set; } = 0;
        public int Width { get; set; } = 300;
        public int Height { get; set; } = 400;
        public string Text { get; set; } = "";
        public Dictionary<string, object> Style { get; set; } = null;

        public Dictionary<string, object> ToDictionary()
        {
            var shapeAsDict = new Dictionary<string, object>
            {
                { "type", Type },
                { "x", PosX },
                { "y", PosY },
                { "width", Width },
                { "height", Height },
                { "text", Text }
            };

            if(Style != null)
            {
                shapeAsDict.Add("style", Style);
            }

            return shapeAsDict;
        }
    }

    public class MiroLine
    {
        public string Type { get; set; } = "line";
        public string StartId { get; set; }
        public string EndId { get; set; }
        public Dictionary<string, object> Style { get; set; } = null;

        public Dictionary<string, object> ToDictionary()
        {
            var lineAsDict = new Dictionary<string, object>
            {
                { "type", Type },
                { "startWidget", new Dictionary<string, object> { { "id", StartId } } },
                { "endWidget", new Dictionary<string, object> { { "id", EndId } } }
            };

            if(Style != null)
            {
                lineAsDict.Add("style", Style);
            }

            return lineAsDict;
        }
    }
}
