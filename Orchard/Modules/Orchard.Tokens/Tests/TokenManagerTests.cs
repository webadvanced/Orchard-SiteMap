using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Orchard.Tokens.Implementation;

namespace Orchard.Tokens.Tests
{
    [TestFixture]
    public class TokenManagerTests
    {
        private IContainer _container;
        private ITokenManager _tokenManager;

        [SetUp]
        public void Init()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<TokenManager>().As<ITokenManager>();
            builder.RegisterType<TestTokenProvider>().As<ITokenProvider>();
            _container = builder.Build();
            _tokenManager = _container.Resolve<ITokenManager>();
        }

        [Test]
        public void TestEvaluate()
        {
            var tokens = _tokenManager.Evaluate("Site", new Dictionary<string, string> { { "Global1", "Site.Global1" }, { "Global2", "Site.Global2" } }, null);
            Assert.That(tokens["Site.Global1"], Is.EqualTo("[global1]"));
            Assert.That(tokens["Site.Global2"], Is.EqualTo("[global2]"));
        }

        [Test]
        public void TestDescribe()
        {
            var allTokens = _tokenManager.Describe(null);
            Assert.That(allTokens.Count(), Is.EqualTo(3));
            Assert.That(allTokens.Any(d => d.Name.ToString() == "Site"));
            Assert.That(allTokens.Any(d => d.Name.ToString() == "User"));
            Assert.That(allTokens.Any(d => d.Name.ToString() == "Date"));

            var tokens = allTokens.Single(d => d.Name.ToString() == "Site").Tokens;
            Assert.That(string.Join(",", tokens.Select(td => td.Target)), Is.EqualTo("Site,Site,Site,Site"));
            Assert.That(string.Join(",", tokens.Select(td => td.Token)), Is.EqualTo("Global1,Global2,Global3,CurrentUser"));
            Assert.That(string.Join(",", tokens.Select(td => td.Name.ToString())), Is.EqualTo("Global1,Global2,Global3,Current User"));
            Assert.That(string.Join(",", tokens.Select(td => td.Description.ToString())), Is.EqualTo("description of token1,description of token2,description of token3,The current user"));
            Assert.That(string.Join(",", tokens.Select(td => td.ChainTarget ?? "")), Is.EqualTo(",,,User"));

            tokens = allTokens.Single(d => d.Name.ToString() == "User").Tokens;
            Assert.That(string.Join(",", tokens.Select(td => td.Target)), Is.EqualTo("User,User"));
            Assert.That(string.Join(",", tokens.Select(td => td.Token)), Is.EqualTo("Name,Birthdate"));
            Assert.That(string.Join(",", tokens.Select(td => td.Name.ToString())), Is.EqualTo("Name,Birthdate"));
            Assert.That(string.Join(",", tokens.Select(td => td.Description.ToString())), Is.EqualTo("Their user name,Date of birth"));
            Assert.That(string.Join(",", tokens.Select(td => td.ChainTarget ?? "")), Is.EqualTo(",DateTime"));

            tokens = allTokens.Single(d => d.Name.ToString() == "Date").Tokens;
            Assert.That(string.Join(",", tokens.Select(td => td.Target)), Is.EqualTo("Date"));
            Assert.That(string.Join(",", tokens.Select(td => td.Token)), Is.EqualTo("Now"));
            Assert.That(string.Join(",", tokens.Select(td => td.Name.ToString())), Is.EqualTo("Now"));
            Assert.That(string.Join(",", tokens.Select(td => td.Description.ToString())), Is.EqualTo("Current system date in short date format. You can chain a .NET DateTime format string to customize."));
            Assert.That(string.Join(",", tokens.Select(td => td.ChainTarget ?? "")), Is.EqualTo(""));
        }

        [Test]
        public void TestDescribeFilter()
        {
            var tokens = _tokenManager.Describe(null);
            Assert.That(tokens.Count(), Is.EqualTo(3));
            tokens = _tokenManager.Describe(new[] { "Site" });
            Assert.That(tokens.Count(), Is.EqualTo(1));
            Assert.That(tokens.First().Name.ToString(), Is.EqualTo("Site"));
            tokens = _tokenManager.Describe(new[] { "Site", "User" });
            Assert.That(tokens.Count(), Is.EqualTo(2));
            Assert.That(tokens.Any(d => d.Name.ToString() == "Site"));
            Assert.That(tokens.Any(d => d.Name.ToString() == "User"));
        }

    }
}
