using System;
using System.Collections.Generic;
using System.Text;

namespace Accountant.Accounts.Enums
{
    public enum AccountDeleteResult
    {
        Success,
        PluginBlocked,
        NotFound,
        StorageError
    }
}
