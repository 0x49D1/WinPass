using System;
using System.IO;
using KeePass.Storage;

using System.Windows;
//using System.Windows.Threading;
//using System.Windows.Navigation;
using System.Windows.Controls;

using Coding4Fun.Phone.Controls;
using KeePass.Utils;
using KeePass.I18n;
//using KeePass.Storage;

using Microsoft.Devices;

using Microsoft.Phone.Controls;

namespace KeePass.Sources
{
    internal class Synchronizer
    {
        private readonly IServiceAdapter _adapter;
        private readonly DatabaseInfo _db;
        private readonly SyncInfo _info;
        private readonly Func<DatabaseInfo, bool> _queryUpdate;

        private bool _aborted;
        private ReportUpdateResult _reporter;

        private bool Aborted
        {
            get { return _aborted || !_queryUpdate(_db); }
            set { _aborted = value; }
        }

        public Synchronizer(DatabaseInfo db,
            IServiceAdapter adapter,
            Func<DatabaseInfo, bool> queryUpdate)
        {
            if (adapter == null) throw new ArgumentNullException("adapter");
            if (db == null) throw new ArgumentNullException("db");
            if (queryUpdate == null) throw new ArgumentNullException("queryUpdate");

            _db = db;
            _adapter = adapter;
            _queryUpdate = queryUpdate;
            _info = _adapter.Initialize(db);

            _adapter.Error += _adapter_Error;
        }

        public void Synchronize(ReportUpdateResult reporter)
        {
            if (reporter == null)
                throw new ArgumentNullException("reporter");

            _reporter = reporter;
            Try(x => x.List(Listed));
        }

        private void ConflictUploaded(ListItem item,
            string path, string conflictPath)
        {
            if (Aborted)
                return;

            Report(new SyncCompleteInfo
            {
                Path = path,
                Modified = item.Timestamp,
                ConflictPath = conflictPath,
                Result = SyncResults.Conflict,
            });
        }

        private void Downloaded(ListItem item, byte[] bytes)
        {
            if (Aborted)
                return;

            Report(new SyncCompleteInfo
            {
                Database = bytes,
                Path = _info.Path,
                Modified = item.Timestamp,
                Result = SyncResults.Downloaded,
            });
        }

        private void Listed(ListItem item)
        {
            if (Aborted)
                return;

            if (item.Timestamp == null)
            {
                // File deleted from server, upload local file to server.

                Try(x => x.Upload(item, Uploaded));
                return;
            }

            if (item.Timestamp == _info.Modified)
            {
                if (!_info.HasLocalChanges)
                {
                    // Already up-to-date
                    Report(new SyncCompleteInfo
                    {
                        Path = _info.Path,
                        Result = SyncResults.NoChange,
                    });



                    return;
                }

                // Has local change, upload to server
                Try(x => x.Upload(item, Uploaded));

                return;
            }

            if (!_info.HasLocalChanges)
            {
                // Has changes from server
                Try(x => x.Download(item, Downloaded));
                return;
            }

            // Conflict
            Try(x => x.Conflict(item, ConflictUploaded));
        }

        private void OnSyncError(Exception ex)
        {
            if (Aborted)
                return;

            Aborted = true;
            Report(new SyncCompleteInfo
            {
                Path = _info.Path,
                Result = SyncResults.Failed,
            });
        }

        private void Report(SyncCompleteInfo result)
        {
            string msg = null;
            var details = _db.Details;
            var settings = AppSettings.Instance;

            switch (result.Result)
            {
                case SyncResults.NoChange:
                    // Toast wenn Sync erfolgreich
                    if (settings.SyncToast)
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            VibrateController vibrate = VibrateController.Default;
                            vibrate.Start(TimeSpan.FromMilliseconds(100));

                            new ToastPrompt
                            {
                                Title = Strings.Sync_Title,
                                Message = Strings.Sync_MessNoChange,
                                TextOrientation = Orientation.Vertical,
                            }.Show();                            
                        });
                    }
                    break;

                case SyncResults.Downloaded:
                    using (var buffer = new MemoryStream(result.Database))
                        _db.SetDatabase(buffer, details);

                    details.HasLocalChanges = false;
                    details.Modified = result.Modified;
                    _db.SaveDetails();

                    // Toast wenn Sync erfolgreich
                    if (settings.SyncToast)
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            VibrateController vibrate = VibrateController.Default;
                            vibrate.Start(TimeSpan.FromMilliseconds(100));

                            new ToastPrompt
                            {
                                Title = Strings.Sync_Title,
                                Message = Strings.Sync_MessDownSucc,
                                TextOrientation = Orientation.Vertical,
                            }.Show();
                        });
                    }
                    break;

                case SyncResults.Uploaded:
                    details.HasLocalChanges = false;
                    details.Modified = result.Modified;
                    _db.SaveDetails();

                    // Toast wenn Sync erfolgreich
                    if (settings.SyncToast)
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            VibrateController vibrate = VibrateController.Default;
                            vibrate.Start(TimeSpan.FromMilliseconds(100));

                            new ToastPrompt
                            {
                                Title = Strings.Sync_Title,
                                Message = Strings.Sync_MessUpSucc,
                                TextOrientation = Orientation.Vertical,
                            }.Show();
                        });
                    }
                    break;

                case SyncResults.Conflict:
                    details.Url = result.Path;
                    details.HasLocalChanges = false;
                    details.Modified = result.Modified;
                    _db.SaveDetails();

                    msg = string.Format(
                        Properties.Resources.Conflict,
                        result.ConflictPath);
                    break;

                case SyncResults.Failed:
                    msg = Properties.Resources
                        .DownloadError;
                    break;
            }

            _reporter(_db, result.Result, msg);
        }

        private void Try(Action<IServiceAdapter> action)
        {
            try
            {
                action(_adapter);
            }
            catch (Exception ex)
            {
                OnSyncError(ex);
            }
        }

        private void Uploaded(ListItem item)
        {
            if (Aborted)
                return;

            Report(new SyncCompleteInfo
            {
                Path = _info.Path,
                Modified = item.Timestamp,
                Result = SyncResults.Uploaded,
            });
        }

        private void _adapter_Error(
            object sender, SynchronizeErrorEventArgs e)
        {
            OnSyncError(e.Exception);
        }
    }
}