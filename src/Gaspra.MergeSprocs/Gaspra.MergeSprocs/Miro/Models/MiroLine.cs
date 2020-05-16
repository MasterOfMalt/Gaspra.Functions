using System;
using System.Collections.Generic;
using System.Text;

namespace Gaspra.MergeSprocs.Miro.Models
{
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

            if (Style != null)
            {
                lineAsDict.Add("style", Style);
            }

            return lineAsDict;
        }
    }
}
