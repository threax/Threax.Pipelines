using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Threax.DockerBuildConfig;

namespace Threax.K8sDeploy.Controller
{
    class CloneController : IController
    {
        string localRefRoot = "refs/heads/";
        String remoteRefRoot = "refs/remotes/origin/";

        private BuildConfig appConfig;
        private ILogger logger;

        public CloneController(BuildConfig appConfig, ILogger<CloneController> logger)
        {
            this.appConfig = appConfig;
            this.logger = logger;
        }

        public Task Run()
        {
            CloneGitRepo(appConfig.RepoUrl, appConfig.ClonePath);

            return Task.CompletedTask;
        }

        private void CloneGitRepo(string repo, string clonePath, string repoUser = null, string repoPass = null)
        {
            if (Directory.Exists(clonePath))
            {
                logger.LogInformation($"Pulling changes to {clonePath}");
                var path = Path.Combine(clonePath, ".git");
                var branch = appConfig.Branch;
                using (var gitRepository = new Repository(path))
                {
                    var signature = new Signature("bot", "bot@bot", DateTime.Now);
                    var result = Commands.Pull(gitRepository, signature, new PullOptions()
                    {
                        FetchOptions = new FetchOptions()
                        {
                            CredentialsProvider = (u, user, cred) => new UsernamePasswordCredentials()
                            {
                                Username = repoUser,
                                Password = repoPass
                            }
                        }
                    });

                    if (gitRepository.Head.FriendlyName != branch)
                    {
                        var checkoutBranch = 
                            gitRepository.Branches.Where(i => i.FriendlyName == branch).FirstOrDefault() ??
                            gitRepository.Branches.Where(i => i.FriendlyName == $"origin/{branch}").FirstOrDefault()
                            ?? throw new InvalidOperationException($"Cannot find branch named '{branch}' or 'origin/{branch}' on repo '{clonePath}'");
                        Checkout(gitRepository, branch, signature);

                        //Pull once more just to be sure we have all updates
                        result = Commands.Pull(gitRepository, signature, new PullOptions()
                        {
                            FetchOptions = new FetchOptions()
                            {
                                CredentialsProvider = (u, user, cred) => new UsernamePasswordCredentials()
                                {
                                    Username = repoUser,
                                    Password = repoPass
                                }
                            }
                        });
                    }
                }
            }
            else
            {
                logger.LogInformation($"Cloning {repo} to {clonePath}");
                Repository.Clone(repo, clonePath, new CloneOptions()
                {
                    BranchName = appConfig.Branch,
                    CredentialsProvider = (u, user, cred) => new UsernamePasswordCredentials()
                    {
                        Username = repoUser,
                        Password = repoPass
                    }
                });
            }
        }

        public void Checkout(Repository repo, String name, LibGit2Sharp.Signature sig)
        {
            var localRef = localRefRoot + name;
            LibGit2Sharp.Branch branch = repo.Branches[localRef];

            var remoteRef = remoteRefRoot + name;
            var remoteBranch = repo.Branches[remoteRef];

            //Found a local branch, use it
            if (branch != null)
            {
                LibGit2Sharp.Commands.Checkout(repo, branch);
                if (remoteBranch != null && remoteBranch.Tip != repo.Head.Tip)
                {
                    repo.Merge(remoteBranch, sig, new LibGit2Sharp.MergeOptions());
                }
                return; //Was able to do a simple checkout to a local branch
            }

            //No local branch, use the remote branch and create a new local branch
            if (remoteBranch != null)
            {
                //Since we already know there is not a local branch, create it
                var localBranch = repo.Branches.Add(name, remoteBranch.Tip);
                LinkBranchToRemote(repo, localBranch);
                LibGit2Sharp.Commands.Checkout(repo, localBranch);
                return; //Was able to find branch in remote repo. Checkout to it
            }

            throw new InvalidOperationException($"Cannot find branch {name} in current local or remote branches. Do you need to create the branch or pull in updates?");
        }

        private void LinkBranchToRemote(Repository repo, LibGit2Sharp.Branch branch)
        {
            var remote = repo.Network.Remotes["origin"];
            if (remote != null)
            {
                repo.Branches.Update(branch, b => b.Remote = remote.Name, b => b.UpstreamBranch = branch.CanonicalName);
            }
        }
    }
}
