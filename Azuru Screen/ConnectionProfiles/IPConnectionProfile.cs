using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ASU
{
    public class IPConnectionProfile
    {
        public string Name;
        public string Address;
        public int Port;

        public IPConnectionProfile(string Name, string Address, int Port)
        {
            this.Name = Name;
            this.Address = Address;
            this.Port = Port;
        }

        public string ToJSON()
        {
            return "{\"name\": \"" + this.Name + "\", \"address\": \"" + this.Address + "\", \"port\": " + this.Port.ToString() + "}";
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
