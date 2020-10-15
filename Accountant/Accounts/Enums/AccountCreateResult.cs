using System;
using System.Collections.Generic;
using System.Text;

namespace Accountant.Accounts.Enums
{
    public enum AccountCreateResult
    {
        Success,
        PluginBlocked,
        AlreadyExists,
        StorageError
    }
}
