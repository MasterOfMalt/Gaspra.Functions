using System;
using System.Collections.Generic;
using System.Text;

namespace Gaspra.MergeSprocs.Models.Database
{
    public class ExtendedProperty
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public ExtendedProperty(
            string name,
            string value)
        {
            Name = name;
            Value = value;
        }
    }
}
