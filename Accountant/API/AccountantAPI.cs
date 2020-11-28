using Accountant.Accounts;

using SharedUtils.References;
using SharedUtils.Storage.Exceptions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accountant.API
{
    public class AccountantAPI
    {
        private AccountantPlugin Plugin;

        internal AccountantAPI(AccountantPlugin plugin)
        {
            Plugin = plugin;
        }

        public AccountLookupResult TryGetAccountByName(string name, out ObjectReference<Account> acc)
        {
            acc = default;

            try
            {
                acc = Plugin.Accounts.GetAccountByUsername(name);
                return AccountLookupResult.Success;
            }
            catch (EntryNotFoundException)
            {
                return AccountLookupResult.NotFound;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Exception processing API call to retrieve account by name (name: {name}): {e}");
                return AccountLookupResult.InternalError;
            }
        }
    }
}
