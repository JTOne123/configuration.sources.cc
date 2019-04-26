using JetBrains.Annotations;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.ClusterConfig.Converters
{
    [PublicAPI]
    public interface ISettingsNodeConverter
    {
        bool NeedToConvert([CanBeNull] ISettingsNode settings);

        [CanBeNull]
        ISettingsNode Convert([CanBeNull] ISettingsNode settings);
    }
}