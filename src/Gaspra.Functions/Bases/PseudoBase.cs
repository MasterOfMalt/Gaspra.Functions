using ConsoleAppFramework;
using Gaspra.Pseudo;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gaspra.Functions.Bases
{
    public class PseudoBase : ConsoleAppBase
    {
        private readonly IWrite write;

        public PseudoBase(IWrite write)
        {
            this.write = write;
        }

        [Command("Pseudo")]
        public void Run(
            [Option("w", "write to pseduo")]string pseudoWrite
            )
        {
            write.Output(pseudoWrite);
        }
    }
}
