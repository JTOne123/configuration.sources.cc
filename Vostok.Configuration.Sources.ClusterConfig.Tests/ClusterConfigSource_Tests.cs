using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NUnit.Framework;
using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Commons.Testing.Observable;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.ClusterConfig.Converters;

namespace Vostok.Configuration.Sources.ClusterConfig.Tests
{
    [TestFixture]
    internal class ClusterConfigSource_Tests
    {
        private const string Prefix = "prefix";
        private IClusterConfigClient client;
        private ClusterConfigSource source;
        private ISettingsNodeConverter[] converters;

        [SetUp]
        public void SetUp()
        {
            client = Substitute.For<IClusterConfigClient>();
            converters = Enumerable.Range(0, 2).Select(_ => Substitute.For<ISettingsNodeConverter>()).ToArray();

            foreach (var converter in converters)
                converter.NeedToConvert(null).ReturnsForAnyArgs(true);
            
            source = new ClusterConfigSource(client, Prefix, converters);
        }
        
        [Test]
        public void Should_Observe_prefix_using_client()
        {
            source.Observe();
            client.Received(1).Observe(Prefix);
        }
        
        [Test]
        public void Should_leave_node_from_client_unchanged_when_no_converters()
        {
            source = new ClusterConfigSource(client, Prefix, new List<ISettingsNodeConverter>());
            
            var node = Substitute.For<ISettingsNode>();
            client.Observe(Prefix).Returns(Observable.Return(node));
            
            var value = source.Observe().WaitFirstValue(100.Milliseconds());
            
            value.settings.Should().BeSameAs(node);
            value.error.Should().BeNull();
        }

        [Test]
        public void Should_apply_several_converters_sequentially()
        {
            var nodes = Enumerable.Range(0, 3).Select(_ => Substitute.For<ISettingsNode>()).ToArray();
            
            client.Observe(Prefix).Returns(Observable.Return(nodes[0]));
            for (var i = 0; i < 2; i++)
                converters[i].Convert(nodes[i]).Returns(nodes[i + 1]);

            var value = source.Observe().WaitFirstValue(100.Milliseconds());
            value.settings.Should().BeSameAs(nodes[2]);
            value.error.Should().BeNull();
        }

        [Test]
        public void Should_skip_converters_that_do_not_need_to_convert_the_node()
        {
            var nodes = Enumerable.Range(0, 2).Select(_ => Substitute.For<ISettingsNode>()).ToArray();

            client.Observe(Prefix).Returns(Observable.Return(nodes[0]));

            converters[0].NeedToConvert(nodes[0]).Returns(false);
            converters[1].Convert(nodes[0]).Returns(nodes[1]);

            var value = source.Observe().WaitFirstValue(100.Milliseconds());
            value.settings.Should().BeSameAs(nodes[1]);
            value.error.Should().BeNull();

            converters[0].DidNotReceiveWithAnyArgs().Convert(null);
        }
    }
}