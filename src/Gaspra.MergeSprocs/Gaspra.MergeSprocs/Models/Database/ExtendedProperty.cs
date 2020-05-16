using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Gaspra.MergeSprocs.Models.Database
{
    public class ExtendedProperty : IEquatable<ExtendedProperty>
    {
        public Guid CorrelationId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        public ExtendedProperty(
            Guid correlationId,
            string name,
            string value)
        {
            CorrelationId = correlationId;
            Name = name;
            Value = value;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ExtendedProperty);
        }

        public bool Equals([AllowNull] ExtendedProperty other)
        {
            return other != null &&
                   CorrelationId.Equals(other.CorrelationId);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CorrelationId);
        }

        public static bool operator ==(ExtendedProperty left, ExtendedProperty right)
        {
            return EqualityComparer<ExtendedProperty>.Default.Equals(left, right);
        }

        public static bool operator !=(ExtendedProperty left, ExtendedProperty right)
        {
            return !(left == right);
        }
    }
}
