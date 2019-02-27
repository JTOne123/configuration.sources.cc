using System;
using System.Linq;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.ClusterConfig.Converters
{
    internal class ValueParser : ISettingsNodeConverter
    {
        private readonly Func<string, string, ISettingsNode> parse;

        public ValueParser(Func<string, string, ISettingsNode> parse)
        {
            this.parse = parse;
        }

        public ISettingsNode Convert(ISettingsNode node)
        {
            switch (node)
            {
                case ValueNode valueNode:
                    return parse(valueNode.Value, valueNode.Name);

                case ArrayNode arrayNode:
                    return new ArrayNode(arrayNode.Name, arrayNode.Children.Select(Convert).ToArray());

                case ObjectNode objectNode:
                    if (objectNode.ChildrenCount == 1 && objectNode.Children.Single() is ValueNode nestedValueNode && string.IsNullOrEmpty(nestedValueNode.Name))
                        return parse(nestedValueNode.Value, objectNode.Name);

                    return new ObjectNode(objectNode.Name, objectNode.Children.Select(Convert));

                default:
                    return node;
            }
        }
    }
}