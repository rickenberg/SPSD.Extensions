using Microsoft.SharePoint.Client;
using System.IO;
using System.Linq;
using System.Management.Automation;
using ClientFile = Microsoft.SharePoint.Client.File;
using LocalFile = System.IO.File;

namespace SPSD.Extensions.Client
{
    [Cmdlet(VerbsCommon.Add, "SPFile")]
    public class AddFileCmdlet : CmdletBase
    {
        [Parameter(ValueFromPipeline = true)]
        public FileInfo SourceFile { get; set; }
        [Parameter(ValueFromPipeline = true)]
        public DirectoryInfo SourceDirectory { get; set; }
        [Parameter]
        public string RelativeDocLibUrl { get; set; }
        [Parameter]
        public string FilePath { get; set; }
        [Parameter]
        public bool NoOverwrite { get; set; }
        [Parameter]
        public bool AddUpdateOnly { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            WriteObject(string.Format("Deploying file(s) to {0}/{1} (NoOverwrite: {2}, AddUpdateOnly: {3})", SiteUrl, RelativeDocLibUrl, NoOverwrite, AddUpdateOnly));

            if (SourceDirectory != null)
            {
                ProcessDirectory(SourceDirectory);
                return;
            }

            if (SourceFile != null)
            {
                ProcessFile(SourceFile, string.Empty);
                return;
            }

            if (!LocalFile.Exists(FilePath))
            {
                throw new FileNotFoundException("Input file(s) not found");
            }

            ProcessFile(new FileInfo(FilePath), string.Empty);
        }

        private void ProcessDirectory(DirectoryInfo directory)
        {
            var path = directory.FullName.Replace(SourceDirectory.FullName, string.Empty);
            path = path.Replace('\\', '/');

            // Process files in directory.
            foreach (var file in directory.GetFiles())
            {
                ProcessFile(file, path);
            }

            // Process sub directories.
            foreach (var subDirectory in directory.GetDirectories())
            {
                ProcessDirectory(subDirectory);
            }
        }

        private void ProcessFile(FileInfo file, string path)
        {
            var relativeTargetUrl = BuildTargetUrl(path);
            var serverRelativeTargetUrl = BuildServerRelativeTargetUrl(relativeTargetUrl, file);

            if (AddUpdateOnly && !IsFileNewer(serverRelativeTargetUrl, file))
            {
                WriteObject(string.Format("Skipped {0} (not newer)", file.Name));
                return;
            }

            EnsureFolderStructure(relativeTargetUrl);
            EnsureCheckout(serverRelativeTargetUrl);
            var uploadedFile = UploadFile(serverRelativeTargetUrl, file, NoOverwrite);
            if (uploadedFile == null)
            {
                return;
            }

            EnsurePublish(uploadedFile, string.Empty);
            WriteObject(string.Format("Deployed {0} to {1}", file.Name, SiteUrl));
        }

        private string BuildTargetUrl(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return RelativeDocLibUrl.Trim('/');
            }

            path = path.Trim('/');
            return RelativeDocLibUrl.Trim('/') + "/" + path;
        }

        private string BuildServerRelativeTargetUrl(string relativeTargetUrl, FileInfo file)
        {
            var web = _clientContext.Web;
            _clientContext.Load(web);
            _clientContext.ExecuteQuery();

            return _clientContext.Web.ServerRelativeUrl + relativeTargetUrl + "/" + file.Name;
        }

        private void EnsureFolderStructure(string relativeTargetUrl)
        {

            var depth = relativeTargetUrl.Split('/').Length;

            var folder = _clientContext.Web.GetFolderByServerRelativeUrl(relativeTargetUrl);

            _clientContext.Load(folder);

            try
            {
                _clientContext.ExecuteQuery();
            }
            catch (ServerException ex)
            {
                if (ex.ServerErrorTypeName.Equals(typeof(FileNotFoundException).FullName))
                {
                    try
                    {
                        _clientContext.Web.Folders.Add(relativeTargetUrl);
                        _clientContext.ExecuteQuery();
                        WriteObject(string.Format("Folder created: {0}", relativeTargetUrl));
                        return;
                    }
                    catch
                    {
                    }

                    if (depth > 0)
                    {
                        EnsureFolderStructureRecursive(relativeTargetUrl, 1, depth);
                    }
                }
            }
        }

        private void EnsureFolderStructureRecursive(string relativeTargetUrl, int position, int depth)
        {
            var currentRelativeTargetUrl = relativeTargetUrl;

            var segments = relativeTargetUrl.Split('/');
            currentRelativeTargetUrl = segments.Take(position + 1).Aggregate((a, b) => a + "/" + b);

            var folder = _clientContext.Web.GetFolderByServerRelativeUrl(currentRelativeTargetUrl);

            _clientContext.Load(folder);

            try
            {
                _clientContext.ExecuteQuery();
            }
            catch (ServerException ex)
            {
                if (ex.ServerErrorTypeName.Equals(typeof(FileNotFoundException).FullName))
                {
                    try
                    {
                        _clientContext.Web.Folders.Add(currentRelativeTargetUrl);
                        _clientContext.ExecuteQuery();
                        WriteObject(string.Format("Folder created: {0}", currentRelativeTargetUrl));
                    }
                    catch
                    {
                    }

                    if (position < depth)
                    {
                        EnsureFolderStructureRecursive(relativeTargetUrl, ++position, depth);
                    }

                    return;
                }
            }

            if (position < depth)
            {
                EnsureFolderStructureRecursive(relativeTargetUrl, ++position, depth);
            }
        }

        private bool IsFileNewer(string relativeTargetUrl, FileInfo file)
        {
            var spfile = _clientContext.Web.GetFileByServerRelativeUrl(relativeTargetUrl);
            _clientContext.Load(spfile, f => f.TimeLastModified);

            try
            {
                _clientContext.ExecuteQuery();
                return spfile.TimeLastModified <= file.LastWriteTimeUtc;
            }
            catch (ServerException ex)
            {
                if (ex.ServerErrorTypeName.Equals(typeof (FileNotFoundException).FullName))
                {
                    return true;
                }

                // Trouble getting file.
                throw ex;
            }
        }

        private ClientFile UploadFile(string relativeTargetUrl, FileInfo file, bool noOverwrite)
        {
            using (var stream = file.OpenRead())
            {
                var information = new FileCreationInformation
                {
                    Content = ReadFully(stream),
                    Url = relativeTargetUrl,
                    Overwrite = !noOverwrite
                };
                var uploadFile = _clientContext.Web.RootFolder.Files.Add(information);
                _clientContext.Load(uploadFile);
                try
                {
                    _clientContext.ExecuteQuery();

                }
                catch (ServerException ex)
                {
                    // File already exists.
                    if (ex.ServerErrorCode == -2130575257 && noOverwrite)
                    {
                        WriteObject(string.Format("Skipped {0} (no overwrite)", file.Name));
                        return null;
                    }

                    throw ex;
                }
                return uploadFile;
            }
        }

        private void EnsureCheckout(string serverRelativeTargetUrl)
        {
            // Load dependent data.
            var file = _clientContext.Web.GetFileByServerRelativeUrl(serverRelativeTargetUrl);
            _clientContext.Load(file);
            try
            {
                _clientContext.ExecuteQuery();
            }
            catch (ServerException ex)
            {
                if (ex.ServerErrorTypeName.Equals(typeof(FileNotFoundException).FullName))
                {
                    return;
                }
            }

            var parentList = file.ListItemAllFields.ParentList;
            if (!parentList.IsPropertyAvailable("ForceCheckout") || !file.IsPropertyAvailable("Level") || !_clientContext.Web.IsPropertyAvailable("CurrentUser"))
            {
                _clientContext.Load(parentList);
                _clientContext.Load(_clientContext.Web.CurrentUser);
                _clientContext.ExecuteQuery();
            }

            if (parentList.ForceCheckout && file.Level != FileLevel.Checkout)
            {
                if (file.Level == FileLevel.Checkout)
                {
                    if (file.CheckedOutByUser.Id != _clientContext.Web.CurrentUser.Id)
                    {
                        file.UndoCheckOut();
                    }
                }
                file.CheckOut();
                file.RefreshLoad();
                _clientContext.ExecuteQuery();
            }
        }

        private void EnsurePublish(ClientFile file, string publishComment)
        {
            // Load dependent data.
            var parentList = file.ListItemAllFields.ParentList;
            if (!parentList.IsPropertyAvailable("EnableMinorVersion") || !parentList.IsPropertyAvailable("EnableModeration") || !file.IsPropertyAvailable("Level") || !file.IsPropertyAvailable("ListItemAllFields"))
            {
                _clientContext.Load(parentList);
                _clientContext.Load(file);
                _clientContext.ExecuteQuery();
            }

            var isDirty = false;
            if (file.Level == FileLevel.Checkout)
            {
                file.CheckIn(string.Empty, CheckinType.MajorCheckIn);
                isDirty = true;
            }
            if (parentList.EnableMinorVersions && file.Level != FileLevel.Published)
            {
                file.Publish(publishComment);
                isDirty = true;
            }
            if (isDirty)
            {
                file.RefreshLoad();
                _clientContext.ExecuteQuery();
            }
        }

        private static byte[] ReadFully(Stream input)
        {
            var buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}
