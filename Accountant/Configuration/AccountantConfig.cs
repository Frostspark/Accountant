using Newtonsoft.Json;

using SharedUtils.Configuration;

using System;
using System.Collections.Generic;
using System.Text;

namespace Accountant.Configuration
{
    public class AccountantConfig : Config
    {
        [JsonProperty("allow-uuid-signin")]
        public bool AllowUUIDSignin;

        [JsonProperty("allow-guests")]
        public bool AllowGuests;

        public override void SetDefaults()
        {
            AllowUUIDSignin = false;
            AllowGuests = false;
        }
    }
}
