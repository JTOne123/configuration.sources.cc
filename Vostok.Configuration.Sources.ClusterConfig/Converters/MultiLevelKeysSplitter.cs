using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.ClusterConfig.Helpers;
using Vostok.Configuration.Sources.SettingsTree;

namespace Vostok.Configuration.Sources.ClusterConfig.Converters
{
    internal class MultiLevelKeysSplitter : ISettingsNodeConverter
    {
        private const char Separator = '.';

        public bool NeedToConvert(ISettingsNode settings)
            => NodeTreeEnumerator.EnumerateTree(settings)
                .Where(node => node is ValueNode || IsArrayNodeOfValueNodes(node))
                .Where(node => node.Name != null)
                .Select(node => node.Name)
                .Any(name => name.Contains(Separator));

        public ISettingsNode Convert(ISettingsNode node)
        {
            string[] nameParts;

            switch (node)
            {
                case ValueNode valueNode:
                    if (!TrySplitName(valueNode, out nameParts))
                        return valueNode;

                    return TreeFactory.CreateTreeByMultiLevelKey(nameParts[0], nameParts.Skip(1).ToArray(), valueNode.Value);

                case ArrayNode arrayNode:
                    var children = MergeRedundantObjectNodes(arrayNode.Children.Select(Convert));

                    if (!IsArrayNodeOfValueNodes(arrayNode) || !TrySplitName(arrayNode, out nameParts))
                        return new ArrayNode(arrayNode.Name, children);

                    return TreeFactory.CreateTreeByMultiLevelKey(nameParts[0], nameParts.Skip(1), new ArrayNode(nameParts.Last(), children));

                case ObjectNode objectNode:
                    return new ObjectNode(objectNode.Name, MergeRedundantObjectNodes(objectNode.Children.Select(Convert)));

                default:
                    return node;
            }
        }

        // (iloktionov): We must merge redundant ObjectNodes with same names produced by TreeFactory to prevent data loss.
        // (iloktionov): We must also preserve original order (as much as it makes sense) to respect array elements ordering.
        private static List<ISettingsNode> MergeRedundantObjectNodes(IEnumerable<ISettingsNode> nodes)
        {
            var result = new List<ISettingsNode>();
            var lists = new Dictionary<string, List<ISettingsNode>>(StringComparer.OrdinalIgnoreCase);
            var positions = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var node in nodes)
            {
                if (node is ObjectNode objectNode && node.Name != null && objectNode.ChildrenCount == 1)
                {
                    if (!lists.TryGetValue(node.Name, out var list))
                    {
                        lists[node.Name] = list = new List<ISettingsNode>();
                        positions[node.Name] = result.Count;
                        result.Add(node);
                    }

                    list.Add(objectNode.Children.Single());
                }
                else result.Add(node);
            }

            foreach (var pair in positions)
            {
                result[pair.Value] = new ObjectNode(pair.Key, MergeRedundantObjectNodes(lists[pair.Key]));
            }

            return result;
        }

        private static bool IsArrayNodeOfValueNodes([CanBeNull] ISettingsNode node)
            => node is ArrayNode && node.Children.All(child => child is ValueNode);

        private static bool TrySplitName([NotNull] ISettingsNode node, out string[] parts)
        {
            parts = null;

            if (string.IsNullOrEmpty(node.Name))
                return false;

            if (!node.Name.Contains(Separator))
                return false;

            parts = node.Name.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries);

            return parts.Length > 1;
        }
    }
}
