using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncLeetcodeGithub.Model
{
    // export from cookie-editor https://chrome.google.com/webstore/detail/cookie-editor/hlkenndednhfkekhgcdicdfddnkalmdm
    internal class CookieItemFromExtension
    {
        public string Domain { get; set; }
        public double ExpirationDate { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string SameSite { get; set; }
        public bool Secure { get; set; }
        public bool Session { get; set; }
        public bool HostOnly { get; set; }
        public bool HttpOnly { get; set; }
        public string StoreId { get; set; }
        public string Value { get; set; }
    }

    // Tự export thông qua class CookieUpdater
    internal class CookieItem
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Domain { get; set; }
        public string Path { get; set; }
        public bool Secure { get; set; }
        public bool HttpOnly { get; set; }
        public string SameSite { get; set; }
        public double Expiry { get; set; }
    }
}
