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

        public bool TryGetAccountByName(string name, out ObjectReference<Account> acc)
        {
            acc = default;

            try
            {
                acc = Plugin.Accounts.GetAccountByUsername(name);
                return true;
            }
            catch (EntryNotFoundException)
            {
                return false;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Error processing API call to retrieve account by name: {e}");
                return false;
            }
        }
    }
}
