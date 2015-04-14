using System;

namespace KeePass.Utils
{
    internal static class ApiKeys
    {
#warning 7Pass needs API Keys to use web services
        public const string DROPBOX_KEY = "YOUR_DROPBOX_KEY";
        public const string DROPBOX_SECRET = "YOUR_DROPBOX_SECRET";
        public const string MIXPANEL_TOKEN = "YOUR_MIXPANEL_TOKEN";
        public const string ONEDRIVE_SECRET = "ONEDRIVE_SECRET";
        public const string ONEDRIVE_CLIENT_ID = "ONEDRIVE_CLIENT_ID";
        public const string ONEDRIVE_REDIRECT = "https://login.live.com/oauth20_desktop.srf";
    }
}