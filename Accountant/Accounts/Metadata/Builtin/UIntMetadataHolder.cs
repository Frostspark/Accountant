﻿using Dahomey.Json.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accountant.Accounts.Metadata.Builtin
{
    [JsonDiscriminator("uint")]
    public sealed class UIntMetadataHolder : MetadataHolder<uint>
    {
    }
}
