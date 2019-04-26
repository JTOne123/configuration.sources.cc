using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Sources.ClusterConfig.Converters;
using Vostok.Configuration.Sources.ClusterConfig.Tests.Helpers;

namespace Vostok.Configuration.Sources.ClusterConfig.Tests.Converters
{
    [TestFixture]
    internal class ObjectNodeUnwrapper_Tests : TreeConstructionSet
    {
        private readonly ObjectNodeUnwrapper converter = new ObjectNodeUnwrapper();

        [Test]
        public void Should_unwrap_ObjectNode_with_single_child()
        {
            var original = Object("key", Array("", "a", "b"));

            converter.NeedToConvert(original).Should().BeTrue();

            var converted = converter.Convert(original);
            
            var expected = Array("key", "a", "b");

            converted.Should().Be(expected);
        }

        [Test]
        public void Should_leave_null_unchanged()
        {
            converter.NeedToConvert(null).Should().BeFalse();
        }

        [Test]
        public void Should_leave_ValueNode_unchanged()
        {
            var node = Value("value");

            converter.NeedToConvert(node).Should().BeFalse();
        }

        [Test]
        public void Should_leave_ArrayNode_unchanged()
        {
            var node = Array(null as string, Value("", "value"));

            converter.NeedToConvert(node).Should().BeFalse();
        }

        [Test]
        public void Should_leave_ObjectNode_with_single_named_child_unchanged()
        {
            var node = Object(null as string, Value("a", "b"));

            converter.NeedToConvert(node).Should().BeFalse();
        }

        [Test]
        public void Should_leave_ObjectNode_with_multiple_children_unchanged()
        {
            var node = Object(null as string, Value("a", "b"), Value("", "c"));

            converter.NeedToConvert(node).Should().BeFalse();
        }

        [Test]
        public void Should_convert_ArrayNode_recursively()
        {
            var original = Array(Value("value1"), Object(Value("", "value2")));

            converter.NeedToConvert(original).Should().BeTrue();

            var converted = converter.Convert(original);

            var expected = Array(Value("value1"), Value("value2"));

            converted.Should().Be(expected);
        }

        [Test]
        public void Should_convert_ObjectNode_recursively()
        {
            var original = Object(Value("key1", "value1"), Object("key2", Value("", "value2")));

            converter.NeedToConvert(original).Should().BeTrue();

            var converted = converter.Convert(original);

            var expected = Object(Value("key1", "value1"), Value("key2", "value2"));

            converted.Should().Be(expected);
        }
    }
}