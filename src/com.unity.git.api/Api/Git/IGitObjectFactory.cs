namespace Unity.VersionControl.Git
{
    public interface IGitObjectFactory
    {
        GitStatusEntry CreateGitStatusEntry(string path, GitFileStatus indexStatus, GitFileStatus workTreeStatus, string originalPath = null);
        GitLock CreateGitLockEntry(string id, string path, GitUser owner, string lockedAt);
    }
}
