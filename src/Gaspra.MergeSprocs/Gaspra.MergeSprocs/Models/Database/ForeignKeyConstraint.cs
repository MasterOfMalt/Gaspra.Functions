using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Gaspra.MergeSprocs.Models.Database
{
    public class ForeignKeyConstraint : IEquatable<ForeignKeyConstraint>
    {
        public Guid CorrelationId { get; set; }
        public IEnumerable<string> ConstrainedTo { get; set; }

        public ForeignKeyConstraint(
            Guid correlationId,
            IEnumerable<string> constrainedTo)
        {
            CorrelationId = correlationId;
            ConstrainedTo = constrainedTo;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ForeignKeyConstraint);
        }

        public bool Equals([AllowNull] ForeignKeyConstraint other)
        {
            return other != null &&
                   CorrelationId.Equals(other.CorrelationId);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CorrelationId);
        }

        public static bool operator ==(ForeignKeyConstraint left, ForeignKeyConstraint right)
        {
            return EqualityComparer<ForeignKeyConstraint>.Default.Equals(left, right);
        }

        public static bool operator !=(ForeignKeyConstraint left, ForeignKeyConstraint right)
        {
            return !(left == right);
        }
    }
}
