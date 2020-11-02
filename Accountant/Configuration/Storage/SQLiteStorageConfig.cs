using Dahomey.Json.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Accountant.Configuration.Storage
{
    [JsonDiscriminator("sqlite")]
    public class SQLiteStorageConfig : StorageConfig
    {
        public string Database;

        public override void SetDefaults()
        {
            Database = "db.sqlite";
        }
    }
}
