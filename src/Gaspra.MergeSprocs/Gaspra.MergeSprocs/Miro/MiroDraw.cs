using Gaspra.MergeSprocs.Miro;
using Gaspra.MergeSprocs.Models.Database;
using Refit;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Gaspra.MergeSprocs.Miro
{
    public class MiroDraw
    {
        private readonly HttpClient httpClient;
        private readonly IMiroEndpoints miroEndpoints;

        private readonly string boardId = "o9J_ktODx2w=";
        private readonly string baseAddress = "https://api.miro.com/v1/boards";

        public MiroDraw()
        {
            httpClient = new HttpClient() { BaseAddress = new Uri($"{baseAddress}/{boardId}") };
            miroEndpoints = RestService.For<IMiroEndpoints>(httpClient);
        }

        public async Task Draw(Schema schema)
        {
            var drawTasks = new List<Task>();

            foreach(var table in schema.Tables)
            {
                drawTasks.Add(miroEndpoints.DrawWidget(payload(table.Name)));
            }

            await Task.WhenAll(drawTasks);
        }

        private Dictionary<string, string> payload(string name)
        {
            var dict = new Dictionary<string, string> {
                { "type", "card" },
                { "title", name }
            };

            return dict;
        }
    }
}
