using System;
using System.Globalization;
using Unity.VersionControl.Git;

namespace Unity.VersionControl.Git
{
    [Serializable]
    public struct GitLock
    {
        public static GitLock Default = new GitLock();

        public string id;
        public string path;
        public GitUser owner;
        public string lockedAtString;
        public string projectPath;

        public DateTimeOffset LockedAt
        {
            get
            {
                DateTimeOffset dt;
                if (!DateTimeOffset.TryParseExact(lockedAtString.ToEmptyIfNull(), Constants.Iso8601Formats,
                        CultureInfo.InvariantCulture, Constants.DateTimeStyle, out dt))
                {
                    LockedAt = DateTimeOffset.MinValue;
                    return DateTimeOffset.MinValue;
                }
                return dt;
            }
            set
            {
                lockedAtString = value.ToUniversalTime().ToString(Constants.Iso8601FormatZ, CultureInfo.InvariantCulture);
            }
        }

        [NotSerialized] public string ID => id ?? String.Empty;
        [NotSerialized] public NPath Path => path?.ToNPath() ?? NPath.Default;
        [NotSerialized] public GitUser Owner => owner;
        [NotSerialized] public NPath ProjectPath => projectPath?.ToNPath() ?? NPath.Default;


        public GitLock(string id, NPath path, GitUser owner, string lockedAt, NPath projectPath)
        {
            this.id = id;
            this.path = path.IsInitialized ? path.ToString() : null;
            this.owner = owner;
            this.lockedAtString = lockedAt;
            this.projectPath = projectPath;
        }

        public override bool Equals(object other)
        {
            if (other is GitLock)
                return Equals((GitLock)other);
            return false;
        }

        public bool Equals(GitLock other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + ID.GetHashCode();
            hash = hash * 23 + Path.GetHashCode();
            hash = hash * 23 + Owner.GetHashCode();
            hash = hash * 23 + LockedAt.GetHashCode();
            return hash;
        }

        public static bool operator ==(GitLock lhs, GitLock rhs)
        {
            return lhs.ID == rhs.ID && lhs.Path == rhs.Path && lhs.Owner == rhs.Owner && lhs.LockedAt == rhs.LockedAt;
        }

        public static bool operator !=(GitLock lhs, GitLock rhs)
        {
            return !(lhs == rhs);
        }
        public override string ToString()
        {
            return $"{{ID:{ID}, path:{Path}, owner:{{{Owner}}}, locked_at:'{LockedAt}'}}";
        }
    }
}
