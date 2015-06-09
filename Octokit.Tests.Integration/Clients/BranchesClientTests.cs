﻿using System;
using System.Threading.Tasks;
using Octokit;
using Octokit.Tests.Integration;
using Xunit;

public class BranchesClientTests
{
    public class TheGetBranchesMethod : IDisposable
    {
        readonly Repository _repository;
        readonly IGitHubClient _github;

        public TheGetBranchesMethod()
        {
            _github = Helper.GetAuthenticatedClient();
            var repoName = Helper.MakeNameWithTimestamp("public-repo");
            _repository = _github.Repository.Create(new NewRepository(repoName) { AutoInit = true }).Result;
        }

        [IntegrationTest]
        public async Task ReturnsBranches()
        {
            var branches = await _github.Repository.GetAllBranches(_repository.Owner.Login, _repository.Name);
            
            Assert.NotEmpty(branches);
            Assert.Equal(branches[0].Name, "master");
        }

        public void Dispose()
        {
            Helper.DeleteRepo(_repository);
        }
    }
}