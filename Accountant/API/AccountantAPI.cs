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

        public async Task<(AccountLookupResult lookup_result, ObjectReference<Account> reference)> TryGetAccountByName(string name)
        {
            try
            {
                return (AccountLookupResult.Success, await Plugin.Accounts.GetAccountByUsername(name).ConfigureAwait(false));
            }
            catch (EntryNotFoundException)
            {
                return (AccountLookupResult.NotFound, null);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Exception processing API call to retrieve account by name (name: {name}): {e}");
                return (AccountLookupResult.InternalError, null);
            }
        }

        public async Task<(AccountLookupResult lookup_result, ObjectReference<Account> reference)> TryGetAccountById(long id)
        {
            try
            {
                return (AccountLookupResult.Success, await Plugin.Accounts.GetAccountById(id).ConfigureAwait(false));
            }
            catch (EntryNotFoundException)
            {
                return (AccountLookupResult.NotFound, null);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Exception processing API call to retrieve account by id (id: {id}): {e}");
                return (AccountLookupResult.InternalError, null);
            }
        }
    }
}
