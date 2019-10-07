using System;
using System.Collections.Generic;
using System.Linq;

namespace Unity.VersionControl.Git
{
    public class GitObjectFactory : IGitObjectFactory
    {
        private readonly IEnvironment environment;
        private readonly Dictionary<NPath, NPath> gitToAssetPathMappings = new Dictionary<NPath, NPath>();

        public GitObjectFactory(IEnvironment environment)
        {
            this.environment = environment;
        }

        public GitStatusEntry CreateGitStatusEntry(string path, GitFileStatus indexStatus, GitFileStatus workTreeStatus = GitFileStatus.None, string originalPath = null)
        {
            var absolutePath = new NPath(path).MakeAbsolute();
            var relativePath = absolutePath.RelativeTo(environment.RepositoryPath);
            var projectPath = absolutePath.RelativeTo(environment.UnityProjectPath);
            projectPath = GitToProjectPath(projectPath);

            return new GitStatusEntry(relativePath, absolutePath, projectPath, indexStatus, workTreeStatus, originalPath?.ToNPath());
        }

        public GitLock CreateGitLockEntry(string id, string path, GitUser owner, string lockedAt)
        {
            var absolutePath = new NPath(path).MakeAbsolute();
            var relativePath = absolutePath.RelativeTo(environment.RepositoryPath);
            var projectPath = absolutePath.RelativeTo(environment.UnityProjectPath);
            projectPath = GitToProjectPath(projectPath);

            return new GitLock(id, relativePath, owner, lockedAt, projectPath);
        }

        private NPath GitToProjectPath(NPath projectPath)
        {
            if (!projectPath.ToString().StartsWith(".."))
                return projectPath;

            if (gitToAssetPathMappings.ContainsKey(projectPath))
            {
                projectPath = gitToAssetPathMappings[projectPath];
            }
            else
            {
                var entry = environment.UnityInlinePackagesPaths.FirstOrDefault(x => projectPath.IsChildOf(x.Key));
                if (entry.Key.IsInitialized)
                {
                    var newPath = entry.Value.Combine(projectPath.RelativeTo(entry.Key));
                    gitToAssetPathMappings.Add(projectPath, newPath);
                    projectPath = newPath;
                }
            }

            return projectPath;
        }
    }
}
