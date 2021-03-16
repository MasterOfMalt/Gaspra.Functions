using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Gaspra.Database.Interfaces
{
    public interface ICorrelatedModel
    {
        Guid CorrelationId { get; }
    }

    public abstract class CorrelatedModel : ICorrelatedModel, IEquatable<CorrelatedModel>
    {
        public Guid CorrelationId { get; }

        public CorrelatedModel()
        {
            CorrelationId = Guid.NewGuid();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CorrelatedModel);
        }

        public bool Equals(CorrelatedModel other)
        {
            return other != null &&
                   CorrelationId.Equals(other.CorrelationId);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CorrelationId);
        }

        public static bool operator ==(CorrelatedModel left, CorrelatedModel right)
        {
            return EqualityComparer<CorrelatedModel>.Default.Equals(left, right);
        }

        public static bool operator !=(CorrelatedModel left, CorrelatedModel right)
        {
            return !(left == right);
        }
    }

    public class CorrelatedModelComparison : IEqualityComparer<ICorrelatedModel>
    {
        public static IEqualityComparer<ICorrelatedModel> Instance { get; } = new CorrelatedModelComparison();

        private CorrelatedModelComparison() { }

        public bool Equals([AllowNull] ICorrelatedModel x, [AllowNull] ICorrelatedModel y)
        {
            if (x == null || y == null)
            {
                return false;
            }
            return x.CorrelationId.Equals(y.CorrelationId);
        }

        public int GetHashCode([DisallowNull] ICorrelatedModel obj)
        {
            return HashCode.Combine(obj.CorrelationId);
        }
    }
}
