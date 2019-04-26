using System.Linq;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.ClusterConfig.Helpers;

namespace Vostok.Configuration.Sources.ClusterConfig.Converters
{
    internal class ValueParser : ISettingsNodeConverter
    {
        private readonly ValueNodeParser parse;
        private readonly ValueNodeCondition condition;

        public ValueParser(ValueNodeParser parse, ValueNodeCondition condition = null)
        {
            this.parse = parse;
            this.condition = condition ?? (_ => true);
        }

        public bool NeedToConvert(ISettingsNode settings)
            => NodeTreeEnumerator
                .EnumerateTree(settings)
                .Any(node => node is ValueNode valueNode && condition(valueNode));

        public ISettingsNode Convert(ISettingsNode node)
        {
            switch (node)
            {
                case ValueNode valueNode:
                    return condition(valueNode) ? parse(valueNode.Value, valueNode.Name) : valueNode;

                case ArrayNode arrayNode:
                    return new ArrayNode(arrayNode.Name, arrayNode.Children.Select(Convert).ToArray());

                case ObjectNode objectNode:
                    return new ObjectNode(objectNode.Name, objectNode.Children.Select(Convert));

                default:
                    return node;
            }
        }
    }
}
