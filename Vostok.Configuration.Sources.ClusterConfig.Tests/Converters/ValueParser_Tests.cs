using System;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.ClusterConfig.Converters;
using Vostok.Configuration.Sources.ClusterConfig.Tests.Helpers;

namespace Vostok.Configuration.Sources.ClusterConfig.Tests.Converters
{
    [TestFixture]
    internal class ValueParser_Tests : TreeConstructionSet
    {
        private Func<string, string, ISettingsNode> parser;
        private ValueParser converter;

        [SetUp]
        public void SetUp()
        {
            parser = (value, name) => Object(name, Value("xx", "parsed"));

            converter = new ValueParser(parser);
        }

        [Test]
        public void Should_parse_single_ValueNode()
        {
            var original = Value("x", "y");

            var parsed = converter.Convert(original);

            var expected = Object("x", Value("xx", "parsed"));

            parsed.Should().Be(expected);
        }

        [Test]
        public void Should_venture_into_arrays()
        {
            var original = Array(Value("x", "y"));

            var parsed = converter.Convert(original);

            var expected = Array(Object("x", Value("xx", "parsed")));

            parsed.Should().Be(expected);
        }

        [Test]
        public void Should_venture_into_objects()
        {
            var original = Object(Value("x", "y"));

            var parsed = converter.Convert(original);

            var expected = Object(Object("x", Value("xx", "parsed")));

            parsed.Should().Be(expected);
        }
    }
}