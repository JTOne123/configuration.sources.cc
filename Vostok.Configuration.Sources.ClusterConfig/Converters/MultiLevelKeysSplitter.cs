using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.SettingsTree;

namespace Vostok.Configuration.Sources.ClusterConfig.Converters
{
    internal class MultiLevelKeysSplitter : ISettingsNodeConverter
    {
        private const char Separator = '.';

        public ISettingsNode Convert(ISettingsNode node)
        {
            if (!EnumerateAllValueNodeNames(node).Any(name => name.Contains(Separator)))
                return node;

            return ConvertInternal(node);
        }

        private static ISettingsNode ConvertInternal(ISettingsNode node)
        {
            switch (node)
            {
                case ValueNode valueNode:
                    if (valueNode.Name == null || !valueNode.Name.Contains(Separator))
                        return valueNode;

                    var nameParts = valueNode.Name.Split(new[] {Separator}, StringSplitOptions.RemoveEmptyEntries);
                    if (nameParts.Length == 1)
                        return valueNode;

                    return TreeFactory.CreateTreeByMultiLevelKey(nameParts[0], nameParts.Skip(1).ToArray(), valueNode.Value);

                case ArrayNode arrayNode:
                    return new ArrayNode(arrayNode.Name, MergeRedundantObjectNodes(arrayNode.Children.Select(ConvertInternal)));

                case ObjectNode objectNode:
                    return new ObjectNode(objectNode.Name, MergeRedundantObjectNodes(objectNode.Children.Select(ConvertInternal)));

                default:
                    return node;
            }
        }

        private static IEnumerable<string> EnumerateAllValueNodeNames([CanBeNull] ISettingsNode startingNode)
        {
            var queue = new Queue<ISettingsNode>();

            queue.Enqueue(startingNode);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (node == null)
                    continue;

                if (node is ValueNode && node.Name != null)
                    yield return node.Name;

                foreach (var child in node.Children)
                    queue.Enqueue(child);
            }
        }

        // (iloktionov): We must merge redundant ObjectNodes with same names produced by TreeFactory to prevent data loss.
        // (iloktionov): We must also preserve original order (as much as it makes sense) to respect array elements ordering.
        private static List<ISettingsNode> MergeRedundantObjectNodes(IEnumerable<ISettingsNode> nodes)
        {
            var result = new List<ISettingsNode>();
            var builders = new Dictionary<string, ObjectNodeBuilder>(StringComparer.OrdinalIgnoreCase);
            var positions = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var node in nodes)
            {
                if (node is ObjectNode objectNode && node.Name != null && objectNode.ChildrenCount == 1)
                {
                    if (builders.TryGetValue(node.Name, out var builder))
                    {
                       builder.SetChild(objectNode.Children.Single());
                    }
                    else
                    {
                        builders[node.Name] = objectNode.ToBuilder();
                        positions[node.Name] = result.Count;
                        result.Add(node);
                    }
                }
                else result.Add(node);
            }

            foreach (var pair in positions)
            {
                result[pair.Value] = builders[pair.Key].Build();
            }

            return result;
        }
    }
}