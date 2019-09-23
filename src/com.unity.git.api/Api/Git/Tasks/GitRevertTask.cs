using System.Threading;

namespace Unity.VersionControl.Git.Tasks
{
    public class GitRevertTask : ProcessTask<string>
    {
        private const string TaskName = "git revert";
        private readonly string arguments;

        public GitRevertTask(string changeset, bool allowMergeCommits,
            CancellationToken token, IOutputProcessor<string> processor = null)
            : base(token, processor ?? new SimpleOutputProcessor())
        {
            Guard.ArgumentNotNull(changeset, "changeset");
            Name = TaskName;
            var merge = allowMergeCommits ? "-m" : string.Empty;
            arguments = $"revert --no-edit {merge} {changeset}";
        }

        public override string ProcessArguments { get { return arguments; } }
        public override TaskAffinity Affinity { get { return TaskAffinity.Exclusive; } }
        public override string Message { get; set; } = "Reverting commit...";
    }
}
