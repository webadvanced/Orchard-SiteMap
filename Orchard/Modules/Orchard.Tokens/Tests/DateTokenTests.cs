using System;
using Autofac;
using NUnit.Framework;
using Orchard.Services;
using Orchard.Tokens.Implementation;
using Orchard.Tokens.Providers;

namespace Orchard.Tokens.Tests
{
    [TestFixture]
    public class DateTokenTests
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
            builder.RegisterType<DateTokens>().As<ITokenProvider>();
            builder.RegisterType<Clock>().As<IClock>();
            _container = builder.Build();
            _tokenizer = _container.Resolve<ITokenizer>();
        }

        [Test]
        public void TestDateTokens()
        {
            Assert.That(_tokenizer.Replace("{Date}", null), Is.EqualTo(DateTime.Now.ToString()));
            Assert.That(_tokenizer.Replace("{Date}", new { Date = new DateTime(1978, 11, 15, 0, 0, 0, DateTimeKind.Utc) }), Is.EqualTo(new DateTime(1978, 11, 15, 0, 0, 0, DateTimeKind.Utc).ToLocalTime().ToString()));
        }

        [Test]
        public void TestFormat()
        {
            Assert.That(_tokenizer.Replace("{Date.Format:yyyy}", null), Is.EqualTo(DateTime.Now.ToString("yyyy")));
        }

        [Test]
        public void TestSince()
        {
            var date = DateTime.UtcNow.Subtract(TimeSpan.FromHours(25));
            Assert.That(_tokenizer.Replace("{Date.Since}", new { Date = date }), Is.EqualTo("1 day ago"));
        }

    }
}
