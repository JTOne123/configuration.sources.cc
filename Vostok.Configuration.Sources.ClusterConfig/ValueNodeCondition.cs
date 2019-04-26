using JetBrains.Annotations;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.ClusterConfig
{
    [PublicAPI]
    public delegate bool ValueNodeCondition([NotNull] ValueNode node);
}