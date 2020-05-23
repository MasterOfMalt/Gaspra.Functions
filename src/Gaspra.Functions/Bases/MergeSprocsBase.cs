using ConsoleAppFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gaspra.Functions.Bases
{
    public class MergeSprocsBase : ConsoleAppBase
    {
        [Command("MergeSproc")]
        public void Run(
            [Option("c", "write to console")]string console
            )
        {
            Console.WriteLine($"{nameof(MergeSprocsBase)} -- {console}");
        }
    }
}
