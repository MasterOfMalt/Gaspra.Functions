using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gaspra.Functions.Interfaces
{
    public interface IFunction
    {
        Task Run();
    }

    public class TestOne : IFunction
    {
        public async Task Run()
        {
            await Task.Run(() =>
            {
                Console.WriteLine("test one");
            });
        }
    }

    public class TestTwo : IFunction
    {
        public async Task Run()
        {
            await Task.Run(() =>
            {
                Console.WriteLine("test two");
            });
        }
    }
}
