using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Accountant.Accounts.Metadata
{
    public abstract class MetadataHolder
    {
        internal MetadataHolder()
        {

        }
    }

    public abstract class MetadataHolder<T> : MetadataHolder
    {
        [JsonPropertyName("value")]
        public T Value { get; set; }
    }
}
