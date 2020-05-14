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

        public async Task Draw(Schema schema)
        {
            var drawTasks = new List<Task>();

            int rowNum = 0;

            int colNum = 0;

            foreach(var table in schema.Tables)
            {
                var miroShape = new MiroShape
                {
                    PosX = colNum * 500,
                    PosY = rowNum * 600,
                    Text = $"{table.Name}",
                    Style = new Dictionary<string, object>
                    {
                        { "backgroundColor", "#ff7b00" },
                        { "borderWidth", 4.0 },
                        { "fontSize", 18 },
                        { "textColor", "#242322" },
                        { "borderColor", "#242322" },
                        { "backgroundOpacity", 0.75 }
                    }
                };

                drawTasks.Add(miroEndpoints.DrawWidget(miroShape.ToDictionary()));

                ++colNum;

                if(colNum >= 5)
                {
                    colNum = 0;
                    ++rowNum;
                }
            }

            await Task.WhenAll(drawTasks);

            var widgetsJson = await miroEndpoints.GetWidgets();

            var widgets = JsonConvert.DeserializeObject<Widgets>(widgetsJson);

            foreach(var table in schema.Tables)
            {
                var dependencies = TableDependencies.From(table, schema);

                var links = dependencies
                    .ChildrenTables
                    .ToList();

                var tableId = widgets.Data.Where(w => w.Text.Equals(table.Name)).FirstOrDefault();

                if (tableId != null)
                {
                    foreach (var link in links)
                    {
                        var linkId = widgets.Data.Where(w => w.Text.Equals(link.Name)).FirstOrDefault();

                        if (linkId != null)
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
                                    { "lineStartType", "none" },
                                    { "lineType", "bezier" }
                                }
                            };

                            drawTasks.Add(miroEndpoints.DrawWidget(miroLine.ToDictionary()));
                        }
                    }
                }
            }


            await Task.WhenAll(drawTasks);
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
                {"thisparameter", "willfuckthecall" },
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
