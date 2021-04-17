using Accountant.Accounts;
using Accountant.Accounts.Enums;

using SharedUtils.References;
using SharedUtils.Storage.Exceptions;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Accountant.Commands.Utilities
{
    internal static class AccountUtilities
    {
        /// <summary>
        /// Wrapper around <see cref="AccountManager.GetAccountByUsername(string)"/> that uses the try-do pattern without throwing exceptions.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="refn"></param>
        /// <returns></returns>

        internal static async ValueTask<(FindAccountResult result, ObjectReference<Account> reference)> TryFindAccount(string name)
        {
            try
            {
                return (FindAccountResult.Found, await AccountantPlugin.Instance.Accounts.GetAccountByUsername(name));
            }
            catch (EntryNotFoundException)
            {
                return (FindAccountResult.NotFound, null);
            }
            catch
            {
                return (FindAccountResult.Error, null);
            }
        }
    }
}
