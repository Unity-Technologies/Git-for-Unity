﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitHub.Unity;
using NSubstitute;
using System.Threading;

namespace TestUtils
{
    class SubstituteFactory
    {
        public SubstituteFactory()
        {}

        public IEnvironment CreateEnvironment(CreateEnvironmentOptions createEnvironmentOptions = null)
        {
            createEnvironmentOptions = createEnvironmentOptions ?? new CreateEnvironmentOptions();

            var userPath = createEnvironmentOptions.UserProfilePath.ToNPath();
            var localAppData = userPath.Parent.Combine("LocalAppData").ToString();
            var appData = userPath.Parent.Combine("AppData").ToString();

            var environment = Substitute.For<IEnvironment>();
            environment.RepositoryPath.Returns(createEnvironmentOptions.RepositoryPath);
            environment.ExtensionInstallPath.Returns(createEnvironmentOptions.Extensionfolder);
            environment.UserProfilePath.Returns(createEnvironmentOptions.UserProfilePath);
            environment.UnityProjectPath.Returns(createEnvironmentOptions.UnityProjectPath);
            environment.GetSpecialFolder(System.Environment.SpecialFolder.LocalApplicationData).Returns(localAppData);
            environment.GetSpecialFolder(System.Environment.SpecialFolder.ApplicationData).Returns(appData);
            return environment;
        }

        public IFileSystem CreateFileSystem(CreateFileSystemOptions createFileSystemOptions = null)
        {
            createFileSystemOptions = createFileSystemOptions ?? new CreateFileSystemOptions();

            var fileSystem = Substitute.For<IFileSystem>();
            var realFileSystem = new FileSystem();
            var logger = Logging.GetLogger("TestFileSystem");

            fileSystem.DirectorySeparatorChar.Returns(realFileSystem.DirectorySeparatorChar);
            fileSystem.GetCurrentDirectory().Returns(createFileSystemOptions.CurrentDirectory);

            fileSystem.Combine(Args.String, Args.String).Returns(info => {
                var path1 = (string)info[0];
                var path2 = (string)info[1];
                var combine = realFileSystem.Combine(path1, path2);
                logger.Trace(@"Combine(""{0}"", ""{1}"") -> ""{2}""", path1, path2, combine);
                return combine;
            });

            fileSystem.Combine(Args.String, Args.String, Args.String).Returns(info => {
                var path1 = (string)info[0];
                var path2 = (string)info[1];
                var path3 = (string)info[2];
                var combine = realFileSystem.Combine(path1, path2, path3);
                logger.Trace(@"Combine(""{0}"", ""{1}"", ""{2}"") -> ""{3}""", path1, path2, path3, combine);
                return combine;
            });

            fileSystem.FileExists(Args.String).Returns(info => {
                var path = (string)info[0];

                var result = false;
                if (createFileSystemOptions.FilesThatExist != null)
                {
                    result = createFileSystemOptions.FilesThatExist.Contains(path);
                }

                logger.Trace(@"FileExists(""{0}"") -> {1}", path, result);
                return result;
            });

            fileSystem.WhenForAnyArgs(system => system.FileCopy(Args.String, Args.String, Args.Bool))
                      .Do(
                          info => {
                              logger.Trace(@"FileCopy(""{0}"", ""{1}"", ""{2}"")", (string)info[0], (string)info[1],
                                  (bool)info[2]);
                          });

            fileSystem.DirectoryExists(Args.String).Returns(info => {
                var path1 = (string)info[0];

                var result = true;
                if (createFileSystemOptions.DirectoriesThatExist != null)
                {
                    result = createFileSystemOptions.DirectoriesThatExist.Contains(path1);
                }

                logger.Trace(@"DirectoryExists(""{0}"") -> {1}", path1, result);
                return result;
            });

            fileSystem.ExistingPathIsDirectory(Args.String).Returns(info => {
                var path = (string)info[0];

                var result = false;
                if (createFileSystemOptions.DirectoriesThatExist != null)
                {
                    result = createFileSystemOptions.DirectoriesThatExist.Contains(path);
                }

                logger.Trace(@"ExistingPathIsDirectory(""{0}"") -> {1}", path, result);
                return result;
            });

            fileSystem.ReadAllLines(Args.String).Returns(info => {
                var path = (string)info[0];

                IList<string> result = null;

                if (createFileSystemOptions.FileContents != null)
                {
                    if (createFileSystemOptions.FileContents.TryGetValue(path, out result))
                    {}
                }

                var resultLength = result != null ? $"{result.Count} lines" : "ERROR";

                logger.Trace(@"ReadAllLines(""{0}"") -> {1}", path, resultLength);

                return result;
            });

            fileSystem.ReadAllText(Args.String).Returns(info => {
                var path = (string)info[0];

                string result = null;
                IList<string> fileContent = null;

                if (createFileSystemOptions.FileContents != null)
                {
                    if (createFileSystemOptions.FileContents.TryGetValue(path, out fileContent))
                    {
                        result = string.Join(string.Empty, fileContent.ToArray());
                    }
                }

                var resultLength = fileContent != null ? $"{fileContent.Count} lines" : "ERROR";

                logger.Trace(@"ReadAllText(""{0}"") -> {1}", path, resultLength);

                return result;
            });

            var randomFileIndex = 0;
            fileSystem.GetRandomFileName().Returns(info => {
                string result = null;
                if (createFileSystemOptions.RandomFileNames != null)
                {
                    result = createFileSystemOptions.RandomFileNames[randomFileIndex];

                    randomFileIndex++;
                    randomFileIndex = randomFileIndex % createFileSystemOptions.RandomFileNames.Count;
                }

                logger.Trace(@"GetRandomFileName() -> {0}", result);

                return result;
            });

            fileSystem.GetTempPath().Returns(info => {
                logger.Trace(@"GetTempPath() -> {0}", createFileSystemOptions.TemporaryPath);

                return createFileSystemOptions.TemporaryPath;
            });

            fileSystem.GetFiles(Args.String).Returns(info => {
                var path = (string)info[0];

                string[] result = null;
                if (createFileSystemOptions.ChildFiles != null)
                {
                    var key = new ContentsKey(path);
                    if (createFileSystemOptions.ChildFiles.ContainsKey(key))
                    {
                        result = createFileSystemOptions.ChildFiles[key].ToArray();
                    }
                }

                var resultLength = result != null ? $"{result.Length} items" : "ERROR";

                logger.Trace(@"GetFiles(""{0}"") -> {1}", path, resultLength);

                return result;
            });

            fileSystem.GetFiles(Args.String, Args.String).Returns(info => {
                var path = (string)info[0];
                var pattern = (string)info[1];

                string[] result = null;
                if (createFileSystemOptions.ChildFiles != null)
                {
                    var key = new ContentsKey(path, pattern);
                    if (createFileSystemOptions.ChildFiles.ContainsKey(key))
                    {
                        result = createFileSystemOptions.ChildFiles[key].ToArray();
                    }
                }

                var resultLength = result != null ? $"{result.Length} items" : "ERROR";

                logger.Trace(@"GetFiles(""{0}"", ""{1}"") -> {2}", path, pattern, resultLength);

                return result;
            });

            fileSystem.GetFiles(Args.String, Args.String, Args.SearchOption).Returns(info => {
                var path = (string)info[0];
                var pattern = (string)info[1];
                var searchOption = (SearchOption)info[2];

                string[] result = null;
                if (createFileSystemOptions.ChildFiles != null)
                {
                    var key = new ContentsKey(path, pattern, searchOption);
                    if (createFileSystemOptions.ChildFiles.ContainsKey(key))
                    {
                        result = createFileSystemOptions.ChildFiles[key].ToArray();
                    }
                }

                var resultLength = result != null ? $"{result.Length} items" : "ERROR";

                logger.Trace(@"GetFiles(""{0}"", ""{1}"", {2}) -> {3}", path, pattern, searchOption, resultLength);

                return result;
            });

            fileSystem.GetDirectories(Args.String).Returns(info => {
                var path = (string)info[0];

                string[] result = null;
                if (createFileSystemOptions.ChildDirectories != null)
                {
                    var key = new ContentsKey(path);
                    if (createFileSystemOptions.ChildDirectories.ContainsKey(key))
                    {
                        result = createFileSystemOptions.ChildDirectories[key].ToArray();
                    }
                }

                var resultLength = result != null ? $"{result.Length} items" : "ERROR";

                logger.Trace(@"GetDirectories(""{0}"") -> {1}", path, resultLength);

                return result;
            });

            fileSystem.GetDirectories(Args.String, Args.String).Returns(info => {
                var path = (string)info[0];
                var pattern = (string)info[1];

                string[] result = null;
                if (createFileSystemOptions.ChildDirectories != null)
                {
                    var key = new ContentsKey(path, pattern);
                    if (createFileSystemOptions.ChildDirectories.ContainsKey(key))
                    {
                        result = createFileSystemOptions.ChildDirectories[key].ToArray();
                    }
                }

                var resultLength = result != null ? $"{result.Length} items" : "ERROR";

                logger.Trace(@"GetDirectories(""{0}"", ""{1}"") -> {2}", path, pattern, resultLength);

                return result;
            });

            fileSystem.GetDirectories(Args.String, Args.String, Args.SearchOption).Returns(info => {
                var path = (string)info[0];
                var pattern = (string)info[1];
                var searchOption = (SearchOption)info[2];

                string[] result = null;
                if (createFileSystemOptions.ChildDirectories != null)
                {
                    var key = new ContentsKey(path, pattern, searchOption);
                    if (createFileSystemOptions.ChildDirectories.ContainsKey(key))
                    {
                        result = createFileSystemOptions.ChildDirectories[key].ToArray();
                    }
                }

                var resultLength = result != null ? $"{result.Length} items" : "ERROR";

                logger.Trace(@"GetDirectories(""{0}"", ""{1}"", {2}) -> {3}", path, pattern, searchOption, resultLength);

                return result;
            });

            fileSystem.GetFullPath(Args.String).Returns(info => Path.GetFullPath((string)info[0]));

            return fileSystem;
        }

        public IZipHelper CreateSharpZipLibHelper()
        {
            return Substitute.For<IZipHelper>();
        }

        public IGitObjectFactory CreateGitObjectFactory(string gitRepoPath)
        {
            var gitObjectFactory = Substitute.For<IGitObjectFactory>();

            gitObjectFactory.CreateGitStatusEntry(Args.String, Args.GitFileStatus, Args.String, Args.Bool)
                            .Returns(info => {
                                var path = (string)info[0];
                                var status = (GitFileStatus)info[1];
                                var originalPath = (string)info[2];
                                var staged = (bool)info[3];

                                return new GitStatusEntry(path, gitRepoPath + @"\" + path, null, status, originalPath,
                                    staged);
                            });

            gitObjectFactory.CreateGitLock(Args.String, Args.String).Returns(info => {
                var path = (string)info[0];
                var user = (string)info[1];

                return new GitLock(path, gitRepoPath + @"\" + path, user);
            });

            return gitObjectFactory;
        }

        public IProcessEnvironment CreateProcessEnvironment(string root)
        {
            var processEnvironment = Substitute.For<IProcessEnvironment>();
            processEnvironment.FindRoot(Args.String).Returns(root);
            return processEnvironment;
        }

        public IPlatform CreatePlatform()
        {
            return Substitute.For<IPlatform>();
        }

        public IRepositoryProcessRunner CreateRepositoryProcessRunner(
            CreateRepositoryProcessRunnerOptions options = null)
        {
            var logger = Logging.GetLogger("TestRepositoryProcessRunner");

            options = options ?? new CreateRepositoryProcessRunnerOptions();

            var repositoryProcessRunner = Substitute.For<IRepositoryProcessRunner>();

            repositoryProcessRunner.PrepareGitPull(Arg.Any<ITaskResultDispatcher<string>>(), Args.String, Args.String)
                                   .Returns(info => {
                                       var resultDispatcher = (ITaskResultDispatcher<string>)info[0];
                                       var remote = (string)info[1];
                                       var branch = (string)info[2];

                                       object result = null;

                                       logger.Trace(@"PrepareGitPull({0}, ""{1}"", ""{2}"") -> {3}",
                                           resultDispatcher != null ? "[instance]" : "[null]", remote, branch,
                                           result != null ? result : "[null]");

                                       throw new NotImplementedException();
                                   });

            repositoryProcessRunner.PrepareGitPush(Arg.Any<ITaskResultDispatcher<string>>(), Args.String, Args.String)
                                   .Returns(info => {
                                       var resultDispatcher = (ITaskResultDispatcher<string>)info[0];
                                       var remote = (string)info[1];
                                       var branch = (string)info[2];

                                       object result = null;

                                       logger.Trace(@"PrepareGitPush({0}, ""{1}"", ""{2}"") -> {3}",
                                           resultDispatcher != null ? "[instance]" : "[null]", remote, branch,
                                           result != null ? result : "[null]");

                                       throw new NotImplementedException();
                                   });

            repositoryProcessRunner.RunGitConfigGet(Arg.Any<ITaskResultDispatcher<string>>(), Args.String,
                Args.GitConfigSource).Returns(info => {
                var resultDispatcher = (ITaskResultDispatcher<string>)info[0];
                var key = (string)info[1];
                var gitConfigSource = (GitConfigSource)info[2];

                string result;
                var containsKey =
                    options.GitConfigGetResults.TryGetValue(
                        new CreateRepositoryProcessRunnerOptions.GitConfigGetKey {
                            Key = key,
                            GitConfigSource = gitConfigSource
                        }, out result);

                if (containsKey)
                {
                    resultDispatcher.ReportSuccess(result);
                }
                else
                {
                    resultDispatcher.ReportFailure();
                }

                logger.Trace(@"RunGitConfigGet({0}, ""{1}"", GitConfigSource.{2}) -> {3}",
                    resultDispatcher != null ? "[instance]" : "[null]", key,
                    gitConfigSource.ToString(), containsKey ? $@"Success" : "Failure");

                return Task.Factory.StartNew(() => true);
            });

            var gitStatsResultsEnumerator = options.GitStatusResults?.GetEnumerator();
            repositoryProcessRunner.PrepareGitStatus(Arg.Any<ITaskResultDispatcher<GitStatus>>()).Returns(info => {
                var resultDispatcher = (ITaskResultDispatcher<GitStatus>)info[0];

                GitStatus? result = null;
                if (gitStatsResultsEnumerator != null)
                {
                    if (gitStatsResultsEnumerator.MoveNext())
                    {
                        result = gitStatsResultsEnumerator.Current;
                    }
                }

                if (result != null)
                {
                    resultDispatcher.ReportSuccess(result.Value);
                }
                else
                {
                    resultDispatcher.ReportFailure();
                }

                logger.Trace(@"RunGitStatus({0}) -> {1}", resultDispatcher != null ? "[instance]" : "[null]",
                    result != null ? $"Success: \"{result.Value}\"" : "Failure");
                var task = Args.GitStatusTask;
                task.TaskResult.Returns(result);
                return task;
            });

            var gitListLocksEnumerator = options.GitListLocksResults?.GetEnumerator();
            repositoryProcessRunner.PrepareGitListLocks(Arg.Any<ITaskResultDispatcher<IEnumerable<GitLock>>>())
                .Returns(info => {
                    var resultDispatcher = (ITaskResultDispatcher<IEnumerable<GitLock>>)info[0];

                    IEnumerable<GitLock> result = null;
                    if (gitListLocksEnumerator != null)
                    {
                        if (gitListLocksEnumerator.MoveNext())
                        {
                            result = gitListLocksEnumerator.Current;
                        }
                        else
                        {
                            result = new List<GitLock>();
                        }
                    }

                    if (result.Any())
                    {
                        resultDispatcher.ReportSuccess(result);
                    }
                    else
                    {
                        resultDispatcher.ReportFailure();
                    }

                    logger.Trace(@"RunGitListLocks({0}) -> {1}",
                        resultDispatcher != null ? "[instance]" : "[null]",
                        result != null ? $"Success" : "Failure");

                    var task = Args.GitListLocksTask;
                    task.TaskResult.Returns(result);
                    return task;
                });

            return repositoryProcessRunner;
        }

        public IRepositoryWatcher CreateRepositoryWatcher()
        {
            return Substitute.For<IRepositoryWatcher>();
        }

        public IGitConfig CreateGitConfig()
        {
            return Substitute.For<IGitConfig>();
        }

        public struct ContentsKey
        {
            public readonly string Path;
            public readonly string Pattern;
            public readonly SearchOption? SearchOption;

            public ContentsKey(string path, string pattern, SearchOption? searchOption)
            {
                Path = path;
                Pattern = pattern;
                SearchOption = searchOption;
            }

            public ContentsKey(string path, string pattern) : this(path, pattern, null)
            {}

            public ContentsKey(string path) : this(path, null)
            {}
        }
    }
}