using Dahomey.Json.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accountant.Accounts.Metadata.Builtin
{
    [JsonDiscriminator("ushort")]
    public sealed class UShortMetadataHolder : MetadataHolder<ushort>
    {
    }
}
