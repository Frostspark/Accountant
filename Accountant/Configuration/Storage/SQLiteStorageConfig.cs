using System;
using System.Collections.Generic;
using System.Text;

namespace Accountant.Configuration.Storage
{
    public class SQLiteStorageConfig : StorageConfig
    {
        public string Database;

        public override void SetDefaults()
        {
            Database = "db.sqlite";
        }
    }
}
