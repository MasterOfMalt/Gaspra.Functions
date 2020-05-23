using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Gaspra.MergeSprocs.Models.Database
{
    /*
     * Clean up the child/ parent, this could probably be better represented
     * with a flag saying the constraint is the parent, or just constrained to
     */
    public class ForeignKeyConstraint : IEquatable<ForeignKeyConstraint>
    {
        public Guid CorrelationId { get; set; }
        public IEnumerable<string> ChildConstraints { get; set; }
        public IEnumerable<string> ParentConstraints { get; set; }
        public IEnumerable<string> ConstrainedTo { get; set; }
        public ForeignKeyConstraint(
            Guid correlationId,
            IEnumerable<string> childConstraints,
            IEnumerable<string> parentConstraints)
        {
            CorrelationId = correlationId;
            ChildConstraints = childConstraints;
            ParentConstraints = parentConstraints;

            //todo: clean up this mess
            var constrainedList = new List<string>();
            var childList = childConstraints.ToList();
            var parentList = parentConstraints.ToList();

            constrainedList.AddRange(childList);
            constrainedList.AddRange(parentList);

            ConstrainedTo = constrainedList
                .Distinct();
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
