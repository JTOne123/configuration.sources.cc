using JetBrains.Annotations;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.ClusterConfig
{
    [PublicAPI]
    public delegate ISettingsNode ValueNodeParser([CanBeNull] string value, [CanBeNull] string name);
}