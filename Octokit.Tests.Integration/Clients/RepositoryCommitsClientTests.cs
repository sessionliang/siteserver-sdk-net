﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Octokit;
using Octokit.Tests.Integration;
using Xunit;

public class RepositoryCommitsClientTests
{
    public class TestsWithExistingRepositories
    {
        readonly IRepositoryCommitsClient _fixture;

        public TestsWithExistingRepositories()
        {
            var client = Helper.GetAuthenticatedClient();

            _fixture = client.Repository.Commits;
        }

        [IntegrationTest]
        public async Task CanGetCommit()
        {
            var commit = await _fixture.Get("octokit", "octokit.net", "65a22f4d2cff94a286ac3e96440c810c5509196f");
            Assert.NotNull(commit);
        }

        [IntegrationTest]
        public async Task CanGetCommitWithFiles()
        {
            var commit = await _fixture.Get("octokit", "octokit.net", "65a22f4d2cff94a286ac3e96440c810c5509196f");

            Assert.True(commit.Files.Any(file => file.Filename.EndsWith("IConnection.cs")));
        }

        [IntegrationTest]
        public async Task CanGetListOfCommits()
        {
            var list = await _fixture.GetAll("shiftkey", "ReactiveGit");
            Assert.NotEmpty(list);
        }

        [IntegrationTest]
        public async Task CanGetListOfCommitsBySha()
        {
            var request = new CommitRequest { Sha = "08b363d45d6ae8567b75dfa45c032a288584afd4" };
            var list = await _fixture.GetAll("octokit", "octokit.net", request);
            Assert.NotEmpty(list);
        }

        [IntegrationTest]
        public async Task CanGetListOfCommitsByPath()
        {
            var request = new CommitRequest { Path = "Octokit.Reactive/IObservableGitHubClient.cs" };
            var list = await _fixture.GetAll("octokit", "octokit.net", request);
            Assert.NotEmpty(list);
        }

        [IntegrationTest]
        public async Task CanGetListOfCommitsByAuthor()
        {
            var request = new CommitRequest { Author = "kzu" };
            var list = await _fixture.GetAll("octokit", "octokit.net", request);
            Assert.NotEmpty(list);
        }

        [IntegrationTest]
        public async Task CanGetListOfCommitsByDateRange()
        {
            var offset = new TimeSpan(1, 0, 0);
            var since = new DateTimeOffset(2014, 1, 1, 0, 0, 0, offset);
            var until = new DateTimeOffset(2014, 1, 8, 0, 0, 0, offset);

            var request = new CommitRequest { Since = since, Until = until };
            var list = await _fixture.GetAll("octokit", "octokit.net", request);
            Assert.NotEmpty(list);
        }
    }

    public class TestsWithNewRepository : IDisposable
    {
        readonly IGitHubClient _client;
        readonly IRepositoryCommitsClient _fixture;
        readonly Repository _repository;

        public TestsWithNewRepository()
        {
            _client = Helper.GetAuthenticatedClient();

            _fixture = _client.Repository.Commits;

            var repoName = Helper.MakeNameWithTimestamp("source-repo");

            _repository = _client.Repository.Create(new NewRepository(repoName) { AutoInit = true }).Result;
        }

        [IntegrationTest]
        public async Task CanCompareReferences()
        {
            await CreateTheWorld();

            var result = await _fixture.Compare(Helper.UserName, _repository.Name, "master", "my-branch");

            Assert.Equal(1, result.TotalCommits);
            Assert.Equal(1, result.Commits.Count);
            Assert.Equal(1, result.AheadBy);
            Assert.Equal(0, result.BehindBy);
        }

        [IntegrationTest]
        public async Task CanCompareReferencesOtherWayRound()
        {
            await CreateTheWorld();

            var result = await _fixture.Compare(Helper.UserName, _repository.Name, "my-branch", "master");

            Assert.Equal(0, result.TotalCommits);
            Assert.Equal(0, result.Commits.Count);
            Assert.Equal(0, result.AheadBy);
            Assert.Equal(1, result.BehindBy);
        }

        [IntegrationTest]
        public async Task ReturnsUrlsToResources()
        {
            await CreateTheWorld();

            var result = await _fixture.Compare(Helper.UserName, _repository.Name, "my-branch", "master");

            Assert.NotNull(result.DiffUrl);
            Assert.NotNull(result.HtmlUrl);
            Assert.NotNull(result.PatchUrl);
            Assert.NotNull(result.PermalinkUrl);
        }

        [IntegrationTest]
        public async Task CanCompareUsingSha()
        {
            await CreateTheWorld();

            var master = await _client.GitDatabase.Reference.Get(Helper.UserName, _repository.Name, "heads/master");
            var branch = await _client.GitDatabase.Reference.Get(Helper.UserName, _repository.Name, "heads/my-branch");

            var result = await _fixture.Compare(Helper.UserName, _repository.Name, master.Object.Sha, branch.Object.Sha);

            Assert.Equal(1, result.Commits.Count);
            Assert.Equal(1, result.AheadBy);
            Assert.Equal(0, result.BehindBy);
        }

        async Task CreateTheWorld()
        {
            var master = await _client.GitDatabase.Reference.Get(Helper.UserName, _repository.Name, "heads/master");

            // create new commit for master branch
            var newMasterTree = await CreateTree(new Dictionary<string, string> { { "README.md", "Hello World!" } });
            var newMaster = await CreateCommit("baseline for pull request", newMasterTree.Sha, master.Object.Sha);

            // update master
            await _client.GitDatabase.Reference.Update(Helper.UserName, _repository.Name, "heads/master", new ReferenceUpdate(newMaster.Sha));

            // create new commit for feature branch
            var featureBranchTree = await CreateTree(new Dictionary<string, string> { { "README.md", "I am overwriting this blob with something new" } });
            var newFeature = await CreateCommit("this is the commit to merge into the pull request", featureBranchTree.Sha, newMaster.Sha);

            // create branch
            await _client.GitDatabase.Reference.Create(Helper.UserName, _repository.Name, new NewReference("refs/heads/my-branch", newFeature.Sha));
        }

        async Task<TreeResponse> CreateTree(IDictionary<string, string> treeContents)
        {
            var collection = new List<NewTreeItem>();

            foreach (var c in treeContents)
            {
                var baselineBlob = new NewBlob
                {
                    Content = c.Value,
                    Encoding = EncodingType.Utf8
                };
                var baselineBlobResult = await _client.GitDatabase.Blob.Create(Helper.UserName, _repository.Name, baselineBlob);

                collection.Add(new NewTreeItem
                {
                    Type = TreeType.Blob,
                    Mode = FileMode.File,
                    Path = c.Key,
                    Sha = baselineBlobResult.Sha
                });
            }

            var newTree = new NewTree();
            foreach (var item in collection)
            {
                newTree.Tree.Add(item);
            }

            return await _client.GitDatabase.Tree.Create(Helper.UserName, _repository.Name, newTree);
        }

        async Task<Commit> CreateCommit(string message, string sha, string parent)
        {
            var newCommit = new NewCommit(message, sha, parent);
            return await _client.GitDatabase.Commit.Create(Helper.UserName, _repository.Name, newCommit);
        }

        public void Dispose()
        {
            _client.Repository.Delete(_repository.Owner.Login, _repository.Name);
        }
    }
}