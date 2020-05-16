using Gaspra.MergeSprocs.Models;
using Gaspra.MergeSprocs.Models.Tree;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
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
            var orderedBranches = dataStructure.DependencyTree.Branches.OrderBy(b => b.Depth);

            //calculate sets from the branches

            //calculate positions for each of the sets/ tables

            //draw everything
        }
    }
}
