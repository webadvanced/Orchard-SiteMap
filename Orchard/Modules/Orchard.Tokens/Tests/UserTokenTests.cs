using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Autofac;
using NUnit.Framework;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Tokens.Implementation;
using Orchard.Tokens.Providers;
using Orchard.UI.Notify;

namespace Orchard.Tokens.Tests
{
    [TestFixture]
    public class UserTokenTests
    {
        private IContainer _container;
        private ITokenizer _tokenizer;

        [SetUp]
        public void Init()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<StubOrchardServices>().As<IOrchardServices>();
            builder.RegisterType<TokenManager>().As<ITokenManager>();
            builder.RegisterType<Tokenizer>().As<ITokenizer>();
            builder.RegisterType<UserTokens>().As<ITokenProvider>();
            _container = builder.Build();
            _tokenizer = _container.Resolve<ITokenizer>();
        }

        [Test]
        public void TestUserTokens()
        {
            var str = _tokenizer.Replace("{User.Name},{User.Email},{User.Id}", new { User = new TestUser { UserName = "Joe", Email = "test@test.com", Id = 88 } });
            Assert.That(str, Is.EqualTo("Joe,test@test.com,88"));
            str = _tokenizer.Replace("{User.Name},{User.Email},{User.Id}", null);
            Assert.That(str, Is.EqualTo("Fake,Fake@fake.com,5"));
        }

    }
}
