﻿using System.Linq;
using SiteServer.CMS.Database.Core;
using SiteServer.CMS.Database.Models;
using Xunit;

namespace SiteServer.CMS.Tests.Repositories
{
    [TestCaseOrderer("SiteServer.CMS.Tests.PriorityOrderer", "SiteServer.CMS.Tests")]
    public class AccessTokenRepositoryTest: IClassFixture<EnvironmentFixture>
    {
        public EnvironmentFixture Fixture { get; }

        public AccessTokenRepositoryTest(EnvironmentFixture fixture)
        {
            Fixture = fixture;
        }

        [Fact, TestPriority(0)]
        public void BasicTest()
        {
            var accessTokenInfo = new AccessTokenInfo();

            DataProvider.AccessToken.Insert(accessTokenInfo);
            Assert.True(accessTokenInfo.Id > 0);
            var token = accessTokenInfo.Token;
            Assert.False(string.IsNullOrWhiteSpace(token));

            accessTokenInfo = DataProvider.AccessToken.Get(accessTokenInfo.Id);
            Assert.NotNull(accessTokenInfo);

            accessTokenInfo.Title = "title";
            var updated = DataProvider.AccessToken.Update(accessTokenInfo);
            Assert.True(updated);

            DataProvider.AccessToken.Regenerate(accessTokenInfo);
            Assert.NotEqual(token, accessTokenInfo.Token);

            var deleted = DataProvider.AccessToken.Delete(accessTokenInfo.Id);
            Assert.True(deleted);
        }

        [Fact, TestPriority(0)]
        public void IsTitleExists()
        {
            const string testTitle = "IsTitleExists";

            var exists = DataProvider.AccessToken.IsTitleExists(testTitle);

            Assert.False(exists);

            var accessTokenInfo = new AccessTokenInfo
            {
                Title = testTitle
            };
            DataProvider.AccessToken.Insert(accessTokenInfo);

            exists = DataProvider.AccessToken.IsTitleExists(testTitle);

            Assert.True(exists);

            var deleted = DataProvider.AccessToken.Delete(accessTokenInfo.Id);
            Assert.True(deleted);
        }

        [Fact, TestPriority(0)]
        public void GetAccessTokenInfoList()
        {
            var accessTokenInfo = new AccessTokenInfo
            {
                Title = "title"
            };
            DataProvider.AccessToken.Insert(accessTokenInfo);

            var list = DataProvider.AccessToken.GetAll();

            Assert.True(list.Any());

            var deleted = DataProvider.AccessToken.Delete(accessTokenInfo.Id);
            Assert.True(deleted);
        }
    }
}
