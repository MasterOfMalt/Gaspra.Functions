using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Gaspra.DatabaseUtility.Builders
{
    public record ScriptLine(int Order, string Line, int Indentation);

    public interface IScriptLineFactory
    {
        Task<IReadOnlyCollection<ScriptLine>> LinesFrom(int indentation, params string[] lines);
        Task<string> FormatLines(IReadOnlyCollection<ScriptLine> scriptLines);
    }

    public class ScriptLineFactory : IScriptLineFactory
    {
        public Task<IReadOnlyCollection<ScriptLine>> LinesFrom(int indentation, params string[] lines)
        {
            var order = 0;

            var scriptLines = lines
                .Select(l => new ScriptLine(++order, l, indentation))
                .ToList();

            IReadOnlyCollection<ScriptLine> collection = new ReadOnlyCollection<ScriptLine>(scriptLines);

            return Task.FromResult(collection);
        }

        public Task<string> FormatLines(IReadOnlyCollection<ScriptLine> scriptLines)
        {
            var indent = ' ';

            var script = string.Join(
                $"{Environment.NewLine}",
                scriptLines
                    .OrderBy(s => s.Order)
                    .Select(s => $"{new string(indent, 4*s.Indentation)}{s.Line}"));

            return Task.FromResult(script);
        }
    }

    public record ScriptOrder(int[] Values);

    public interface IScriptSection
    {
        ScriptOrder Order { get; }
        Task<string> Value();
    }

    public interface IScriptBuilder
    {
        Task<string> Generate(IReadOnlyCollection<IScriptSection> scriptSections);
    }

    public class ScriptBuilder : IScriptBuilder
    {
        public Task<string> Generate(IReadOnlyCollection<IScriptSection> scriptSections)
        {
            var deepestOrder = scriptSections
                .Select(s => s.Order.Values.Length)
                .OrderByDescending(s => s)
                .First();

            var flattenedOrders = scriptSections.Select(s =>
                {
                    var orderLengthAffix = new string('0', deepestOrder - s.Order.Values.Length);

                    var flatOrder = string.Join("", s.Order.Values) + orderLengthAffix;

                    var flattenedOrder = int.Parse(flatOrder);

                    return (flattenedOrder, s);
                })
                .ToList();

            var script = string.Join(
                $"{Environment.NewLine}",
                flattenedOrders
                    .OrderBy(f => f.flattenedOrder)
                    .Select(f => f.s)
                    .Select(async s => await s.Value())
                );

            return Task.FromResult(script);
        }
    }

    public class ScriptHeadSettings : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new ScriptOrder(new[] { 0, 0 });

        public ScriptHeadSettings(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public async Task<string> Value()
        {
            var scriptLines = await _scriptLineFactory.LinesFrom(
                0,
                "SET NOCOUNT ON",
                "GO",
                "SET ANSI_NULLS ON",
                "GO",
                "SET QUOTED_IDENTIFIER ON",
                "GO");

            return await _scriptLineFactory.FormatLines(scriptLines);
        }
    }
}
