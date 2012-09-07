using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMProxy
{
    public class FriendlyNameAttribute : Attribute
    {
        public string FriendlyName;

        public FriendlyNameAttribute(string friendlyName)
        {
            FriendlyName = friendlyName;
        }
    }
}
