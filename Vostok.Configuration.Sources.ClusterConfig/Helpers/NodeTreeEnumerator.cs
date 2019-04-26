using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.ClusterConfig.Helpers
{
    internal static class NodeTreeEnumerator
    {
        [NotNull]
        public static IEnumerable<ISettingsNode> EnumerateTree([CanBeNull] ISettingsNode startingNode)
        {
            var queue = new Queue<ISettingsNode>();

            queue.Enqueue(startingNode);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (node == null)
                    continue;

                yield return node;

                foreach (var child in node.Children)
                    queue.Enqueue(child);
            }
        }
    }
}