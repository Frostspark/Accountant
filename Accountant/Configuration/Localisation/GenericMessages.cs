using SharedUtils.Configuration;

using System;
using System.Collections.Generic;
using System.Text;

namespace Accountant.Configuration.Localisation
{
    public class GenericMessages : IConfigSection
    {
        public string AuthenticationRequired;
        public string AuthenticationAlreadyGranted;

        public void SetDefaults()
        {
            AuthenticationRequired = "You must be logged in to do this.";
            AuthenticationAlreadyGranted = "You are already logged in.";
        }
    }
}
