using SharedUtils.Configuration;

using System;
using System.Collections.Generic;
using System.Text;

namespace Accountant.Configuration.Localisation
{
    public class MessageConfig : IConfigSection
    {
        public GenericMessages Generic;
        public LogonMessages Logon;
        public RegistrationMessages Registration;

        public void SetDefaults()
        {
            Generic = new GenericMessages();
            Logon = new LogonMessages();
            Registration = new RegistrationMessages();

            Generic.SetDefaults();
            Logon.SetDefaults();
            Registration.SetDefaults();
        }
    }
}
