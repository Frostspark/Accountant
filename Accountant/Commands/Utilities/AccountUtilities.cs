using Accountant.Accounts;

using SharedUtils.References;
using SharedUtils.Storage.Exceptions;

using System;
using System.Collections.Generic;
using System.Text;

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

        internal static bool TryFindAccount(string name, out ObjectReference<Account> refn)
        {
            refn = null;

            try
            {
                refn = AccountantPlugin.Instance.Accounts.GetAccountByUsername(name);

                return true;
            }
            catch (EntryNotFoundException)
            {
                return false;
            }
        }
    }
}
