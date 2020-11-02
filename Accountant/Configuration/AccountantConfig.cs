using Accountant.Configuration.Storage;

using SharedUtils.Configuration;

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Accountant.Configuration
{
    public class AccountantConfig : Config
    {
        [JsonPropertyName("allow-uuid-signin")]
        public bool AllowUUIDSignin;

        [JsonPropertyName("allow-guests")]
        public bool AllowGuests;

        [JsonPropertyName("guest-whitelisted-commands")]
        public string[] GuestCommands;

        [JsonPropertyName("storage")]
        public StorageConfig Storage;

        public override void SetDefaults()
        {
            AllowUUIDSignin = false;
            AllowGuests = false;
            Storage = new SQLiteStorageConfig();
            Storage.SetDefaults();
            GuestCommands = new[] { "login", "register", "help" };
        }
    }
}
