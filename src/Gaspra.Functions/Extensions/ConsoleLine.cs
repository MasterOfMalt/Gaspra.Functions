using System;
using System.Text;

namespace Gaspra.Functions.Extensions
{
    public class ConsoleLine
    {
        public StringBuilder LineBuilder { get; set; }

        public int CursorTop { get; set; }

        public ConsoleLine(string text)
        {
            LineBuilder = new StringBuilder(text);

            CursorTop = Console.CursorTop;

            Console.WriteLine(LineBuilder);
        }

        public void Rewrite(string line)
        {
            //clear the line builder and write a new line
            Clear();

            LineBuilder.Append(line);

            Console.SetCursorPosition(0, CursorTop);

            Console.WriteLine(LineBuilder);
        }

        public void Clear()
        {
            //clear the line
            LineBuilder.Append('\b', LineBuilder.Length);

            LineBuilder.Append(' ', LineBuilder.Length);

            Console.SetCursorPosition(0, CursorTop);

            Console.WriteLine(LineBuilder);

            LineBuilder.Clear();
        }
    }
}
