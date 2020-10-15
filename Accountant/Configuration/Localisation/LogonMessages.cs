using SharedUtils.Configuration;

using System;
using System.Collections.Generic;
using System.Text;

namespace Accountant.Configuration.Localisation
{
    public class LogonMessages : IConfigSection
    {
        public string UsernameInvalid;
        public string PasswordInvalid;
        public string AutologinDisabledLocally;
        public string AutologinNotWhitelisted;
        public string AutologinInvalidUUID;
        public string AutologinDisabledGlobally;
        public string LoggedInSuccessfully;

        public void SetDefaults()
        {
            UsernameInvalid = "An account by the name {0} does not exist.";
            PasswordInvalid = "The password you provided is not valid.";
            AutologinDisabledGlobally = "Automatic logons are disabled on this server.";
            AutologinNotWhitelisted = "You must log into this account manually at least once to gain auto-login ability.";
            AutologinInvalidUUID = "Automatic logon failed: Invalid UUID.";
            AutologinDisabledLocally = "This account cannot be logged into automatically.";
            LoggedInSuccessfully = "Logged in as {0}.";
        }
    }
}
