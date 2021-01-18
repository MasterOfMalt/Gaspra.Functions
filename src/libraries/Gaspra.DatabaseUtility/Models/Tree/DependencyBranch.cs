using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Gaspra.DatabaseUtility.Models.Tree
{
    public class DependencyBranch : IEquatable<DependencyBranch>
    {
        public int Depth { get; set; }
        public Guid TableGuid { get; set; }
        public string TableName { get; set; }

        public DependencyBranch(
            int depth,
            Guid tableGuid)
        {
            Depth = depth;
            TableGuid = tableGuid;
        }
        public DependencyBranch(
            int depth,
            Guid tableGuid,
            string tableName)
        {
            Depth = depth;
            TableGuid = tableGuid;
            TableName = tableName;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DependencyBranch);
        }

        public bool Equals([AllowNull] DependencyBranch other)
        {
            return other != null &&
                   TableGuid.Equals(other.TableGuid);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TableGuid);
        }

        public static bool operator ==(DependencyBranch left, DependencyBranch right)
        {
            return EqualityComparer<DependencyBranch>.Default.Equals(left, right);
        }

        public static bool operator !=(DependencyBranch left, DependencyBranch right)
        {
            return !(left == right);
        }
    }
}
