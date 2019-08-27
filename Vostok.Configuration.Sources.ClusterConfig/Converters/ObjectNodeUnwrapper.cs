using System.Linq;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.ClusterConfig.Helpers;

namespace Vostok.Configuration.Sources.ClusterConfig.Converters
{
    internal class ObjectNodeUnwrapper : ISettingsNodeConverter
    {
        public bool NeedToConvert(ISettingsNode settings)
            => NodeTreeEnumerator.EnumerateTree(settings).Any(IsRedundantObjectNode);

        public ISettingsNode Convert(ISettingsNode node)
        {
            switch (node)
            {
                case ValueNode valueNode:
                    return valueNode;

                case ArrayNode arrayNode:
                    return new ArrayNode(arrayNode.Name, arrayNode.Children.Select(Convert).ToArray());

                case ObjectNode objectNode:
                    if (IsRedundantObjectNode(objectNode))
                    {
                        var singleChild = objectNode.Children.Single();
                        //Make no difference between empty and nonexistent file.
                        if (string.IsNullOrEmpty(singleChild.Value) && !singleChild.Children.Any())
                            return new ObjectNode(objectNode.Name);

                        return objectNode.Children.Single().WithName(objectNode.Name);
                    }

                    return new ObjectNode(objectNode.Name, objectNode.Children.Select(Convert));

                default:
                    return node;
            }
        }

        private static bool IsRedundantObjectNode(ISettingsNode node)
            => node is ObjectNode objectNode &&
               objectNode.ChildrenCount == 1 &&
               string.IsNullOrEmpty(objectNode.Children.Single().Name);
    }
}