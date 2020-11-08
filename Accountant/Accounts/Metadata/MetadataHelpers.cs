using Dahomey.Json;
using Dahomey.Json.Serialization.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Accountant.Accounts.Metadata
{
    internal static class MetadataHelpers
    {
        internal static JsonSerializerOptions SetupMetadataSerializer(AccountantPlugin plugin)
        {
            JsonSerializerOptions jso = new JsonSerializerOptions();
            jso.SetupExtensions();

            var registry = jso.GetDiscriminatorConventionRegistry();

            registry.ClearConventions();

            registry.RegisterConvention(new DefaultDiscriminatorConvention<string>(jso));

            registry.DiscriminatorPolicy = DiscriminatorPolicy.Always;

            plugin.MetadataRegistry.PushRegisteredTypes(registry);

            return jso;
        }
    }
}
