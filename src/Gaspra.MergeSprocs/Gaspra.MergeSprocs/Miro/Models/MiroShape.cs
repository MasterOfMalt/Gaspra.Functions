using System;
using System.Collections.Generic;
using System.Text;

namespace Gaspra.MergeSprocs.Miro.Models
{
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

            if (Style != null)
            {
                shapeAsDict.Add("style", Style);
            }

            return shapeAsDict;
        }
    }
}
