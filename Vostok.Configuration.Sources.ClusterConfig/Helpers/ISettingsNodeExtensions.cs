using System.Linq;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.ClusterConfig.Helpers
{
    internal static class ISettingsNodeExtensions
    {
        public static ISettingsNode WithName(this ISettingsNode node, string name)
        {
            switch (node)
            {
                case ValueNode valueNode:
                    return new ValueNode(name, valueNode.Value);

                case ArrayNode arrayNode:
                    return new ArrayNode(name, arrayNode.Children.ToArray());

                case ObjectNode objectNode:
                    return new ObjectNode(name, objectNode.Children);
                
                default:
                    return node;
            }
        }
    }
}