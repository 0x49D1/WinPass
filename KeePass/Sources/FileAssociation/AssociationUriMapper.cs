using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using Windows.Phone.Storage.SharedAccess;

namespace KeePass.Sources
{
    class AssociationUriMapper : UriMapperBase
    {
        private string tempUri;

        public override Uri MapUri(Uri uri)
        {
            tempUri = uri.ToString();

            // File association launch
            if (tempUri.Contains("/FileTypeAssociation"))
            {
                // Get the file ID (after "fileToken=").
                int fileIDIndex = tempUri.IndexOf("fileToken=") + 10;
                string fileID = tempUri.Substring(fileIDIndex);

                // Get the file name.
                string incomingFileName = SharedStorageAccessManager.GetSharedFileName(fileID);

                // Get the file extension.
                string incomingFileType = Path.GetExtension(incomingFileName).ToLower();

                switch (incomingFileType)
                {
                    case ".kdbx":
                        return new Uri("/Sources/Download.xaml?folder=&fileToken=" + fileID, UriKind.Relative);
                    default:
                        // Otherwise perform normal launch.
                        return new Uri("/MainPage.xaml", UriKind.Relative);
                }
            }
            return uri;
        }
    }
}
