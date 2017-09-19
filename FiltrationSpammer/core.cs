﻿using System;
using System.Collections.Generic;
using System.Text;
using FiltrationSpammer.Properties;

namespace FiltrationSpammer
{
    class core
    {
        public static string accountID
        {
            get { return Settings.Default["accountid"].ToString(); }
            set { Settings.Default["accountid"] = value; }
        }

        public static string authToken
        {
            get { return Settings.Default["authtoken"].ToString(); }
            set { Settings.Default["authtoken"] = value; }
        }

        public static string spamMessage
        {
            get { return Settings.Default["spammessage"].ToString(); }
            set { Settings.Default["spammessage"] = value; }
        }
    }
}
