using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.ClusterConfig.Converters;
using Vostok.Configuration.Sources.Extensions.Observable;

namespace Vostok.Configuration.Sources.ClusterConfig
{
    /// <summary>
    /// A source that loads settings from ClusterConfig.
    /// </summary>
    [PublicAPI]
    public class ClusterConfigSource : IConfigurationSource
    {
        private readonly IClusterConfigClient client;
        private readonly IList<ISettingsNodeConverter> converters;

        public ClusterConfigSource([NotNull] ClusterConfigSourceSettings settings)
            : this(settings.Client, settings.Prefix, SelectConverters(settings).ToArray())
        {
        }

        internal ClusterConfigSource(
            [NotNull] IClusterConfigClient client,
            [NotNull] string prefix,
            [NotNull] IList<ISettingsNodeConverter> converters)
        {
            this.client = client;
            this.converters = converters;

            Prefix = prefix;
        }

        public string Prefix { get; }

        public ClusterConfigSource ScopeTo(params string[] scope)
            => ScopeTo(string.Join("/", scope));

        public ClusterConfigSource ScopeTo(string innerPrefix)
            => new ClusterConfigSource(client, $"{Prefix.TrimEnd('/')}/{innerPrefix.TrimStart('/')}", converters);

        /// <inheritdoc />
        public IObservable<(ISettingsNode settings, Exception error)> Observe() =>
            client
                .Observe(Prefix)
                .Select(settings => converters.Aggregate(settings, (s, converter) => converter.Convert(s)))
                .Select(settings => (settings, null as Exception));

        private static IEnumerable<ISettingsNodeConverter> SelectConverters(ClusterConfigSourceSettings settings)
        {
            if (settings.SplitMultiLevelKeys)
                yield return new MultiLevelKeysSplitter();

            yield return new ObjectNodeUnwrapper();

            if (settings.ValuesParser != null)
                yield return new ValueParser(settings.ValuesParser);
        }
    }
}
