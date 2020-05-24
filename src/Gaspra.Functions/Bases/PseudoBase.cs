using ConsoleAppFramework;
using Gaspra.Pseudo;
using System.Threading.Tasks;

namespace Gaspra.Functions.Bases
{
    public class PseudoBase : ConsoleAppBase
    {
        private readonly IWrite write;

        public PseudoBase(IWrite write)
        {
            this.write = write;
        }

        [Command("pseudo")]
        public async Task Run(
            [Option("w", "write to pseduo")]string pseudoWrite
            )
        {
            await write.Output(pseudoWrite);
        }
    }
}
