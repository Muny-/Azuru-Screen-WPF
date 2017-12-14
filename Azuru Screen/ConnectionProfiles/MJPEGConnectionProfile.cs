using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASU
{
    public class MJPEGConnectionProfile
    {
        public string Name
        {
            get;
            set;
        }

        public bool IsAuthenticated;

        public string Username;
        public string Password;

        public string Address;

        public MJPEGConnectionProfile(string Name, string Address)
        {
            this.Name = Name;
            this.IsAuthenticated = false;
            this.Username = "";
            this.Password = "";
            this.Address = Address;
        }

        public MJPEGConnectionProfile(string Name, string Address, bool IsAuthenticated, string Username, string Password)
        {
            this.Name = Name;
            this.IsAuthenticated = IsAuthenticated;
            this.Username = Username;
            this.Password = Password;
            this.Address = Address;
        }

        public MJPEGConnectionProfile(string Name, string Address, string Username, string Password)
        {
            this.Name = Name;
            this.IsAuthenticated = true;
            this.Username = Username;
            this.Password = Password;
            this.Address = Address;
        }

        public string ToJSON()
        {
            return "{\"name\": \"" + this.Name + "\", \"address\": \"" + this.Address + "\", \"is_authenticated\": " + this.IsAuthenticated.ToString().ToLower() + ", \"username\": \"" + this.Username + "\", \"password\": \"" + this.Password + "\"}";
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
