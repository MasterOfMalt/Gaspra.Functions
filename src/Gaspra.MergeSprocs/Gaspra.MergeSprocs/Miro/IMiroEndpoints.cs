using Refit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gaspra.MergeSprocs.Miro
{
    [Headers("Authorization: Bearer 86715ffa-a567-4a89-b714-ac7d9b2d60dd")]
    public interface IMiroEndpoints
    {
        [Get("/widgets/")]
        Task<string> GetWidgets();

        [Post("/widgets")]
        Task DrawWidget([Body]Dictionary<string, object> data);
    }
}
