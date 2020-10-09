using SharedUtils.Configuration;

using System;
using System.Collections.Generic;
using System.Text;

namespace Accountant.Configuration.Storage
{
    public abstract class StorageConfig : IConfigSection
    {
        public abstract void SetDefaults();
    }
}
