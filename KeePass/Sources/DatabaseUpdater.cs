using System;
using KeePass.Sources.DropBox;
using KeePass.Sources.OneDrive;
using KeePass.Sources.Web;
using KeePass.Sources.WebDav;
using KeePass.Storage;

namespace KeePass.Sources
{
    internal static class DatabaseUpdater
    {
        public const string DROPBOX_UPDATER = "DropBox";
        public const string WEBDAV_UPDATER = "WebDAV";
        public const string WEB_UPDATER = "Web";
        public const string ONEDRIVE_UPDATER = "SkyDrive";

        public static void Update(this DatabaseInfo info,
            Func<DatabaseInfo, bool> queryUpdate,
            ReportUpdateResult report)
        {
            switch (info.Details.Source)
            {
                case DROPBOX_UPDATER:
                    var dropbox = new Synchronizer(info,
                        new DropBoxAdapter(), queryUpdate);

                    dropbox.Synchronize(report);
                    break;

                case WEBDAV_UPDATER:
                    var webdav = new Synchronizer(info,
                        new WebDavAdapter(), queryUpdate);

                    webdav.Synchronize(report);
                    break;

                case ONEDRIVE_UPDATER:
                    var oneDrive = new Synchronizer(info,
                        new OneDriveAdapter(), queryUpdate);

                    oneDrive.Synchronize(report);
                    break;

                case WEB_UPDATER:
                    WebUpdater.Update(info,
                        queryUpdate, report);
                    break;
            }
        }
    }
}