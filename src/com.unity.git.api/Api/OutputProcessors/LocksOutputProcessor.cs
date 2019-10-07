using System;
using System.Collections.Generic;

namespace Unity.VersionControl.Git
{
    class LocksOutputProcessor : BaseOutputListProcessor<GitLock>
    {
        private readonly IGitObjectFactory gitObjectFactory;

        public LocksOutputProcessor(IGitObjectFactory gitObjectFactory)
        {
            this.gitObjectFactory = gitObjectFactory;
        }

        public override void LineReceived(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                //Do Nothing
                return;
            }

            try
            {
                var locks = line.FromJson<Dictionary<string, object>[]>(lowerCase: true);
                foreach (var entry in locks)
                {
                    string id, path, lockedAt;
                    id = path = lockedAt = null;
                    GitUser owner = GitUser.Default;

                    if (entry.ContainsKey("id")) id = (string)entry["id"];
                    if (entry.ContainsKey("path")) path = (string)entry["path"];
                    if (entry.ContainsKey("locked_at")) lockedAt = (string)entry["locked_at"];
                    if (entry.ContainsKey("owner"))
                    {
                        try
                        {
                            owner = entry["owner"].FromObject<GitUser>(true);
                        }
                        catch {}
                    }

                    if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(path))
                    {
                        var lck = gitObjectFactory.CreateGitLockEntry(id, path, owner, lockedAt);
                        RaiseOnEntry(lck);
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.Error(ex, $"Failed to parse lock line {line}");
            }
        }
    }
}
