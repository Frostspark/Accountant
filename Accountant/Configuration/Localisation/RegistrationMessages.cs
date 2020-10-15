using SharedUtils.Configuration;

using System;
using System.Collections.Generic;
using System.Text;

namespace Accountant.Configuration.Localisation
{
    public class RegistrationMessages : IConfigSection
    {
        public string UsernameTaken;
        public string CreationError;
        public string AccountCreated;

        public void SetDefaults()
        {
            UsernameTaken = "An account by the name {0} already exists.";
            CreationError = "The account could not be created due to a server error - please try again.";
            AccountCreated = "Account {0} has been successfully created.";
        }
    }
}
