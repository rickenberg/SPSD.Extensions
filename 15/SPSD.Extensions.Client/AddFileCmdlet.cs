using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Microsoft.SharePoint.Client;
using File = Microsoft.SharePoint.Client.File;

namespace SPSD.Extensions.Client
{
    [Cmdlet(VerbsCommon.Add, "SPFile")]
    public class AddFileCmdlet : CmdletBase
    {
        /// <summary>
        /// Pipe an input file from PowerShell.
        /// </summary>
        [Parameter(ValueFromPipeline = true)]
        public FileInfo SourceFile { get; set; }
        /// <summary>
        /// Pipe an input directory from PowerShell (all files are processed).
        /// </summary>
        [Parameter(ValueFromPipeline = true)]
        public DirectoryInfo SourceDirectory { get; set; }
        /// <summary>
        /// Path to input file.
        /// </summary>
        [Parameter]
        public string FilePath { get; set; }
        /// <summary>
        /// Relative URL (from site/SPWeb) to the target document library.
        /// </summary>
        [Parameter(Mandatory = true)]
        public string RelativeDocLibUrl { get; set; }
        /// <summary>
        /// Set this to true if you do not want to overwrite existing files.
        /// </summary>
        [Parameter]
        public bool NoOverwrite { get; set; }
        /// <summary>
        /// Set this to true if you only want to upload new and modified files.
        /// </summary>
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

            if (!System.IO.File.Exists(FilePath))
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
                if (ex.ServerErrorTypeName.Equals(typeof(System.IO.FileNotFoundException).FullName))
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
                if (ex.ServerErrorTypeName.Equals(typeof(FileNotFoundException).FullName))
                {
                    return true;
                }

                // Trouble getting file.
                throw ex;
            }
        }

        private File UploadFile(string relativeTargetUrl, FileInfo file, bool noOverwrite)
        {
            using (var stream = file.OpenRead())
            {
                var information = new FileCreationInformation
                {
                    ContentStream = stream,
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
                if (ex.ServerErrorTypeName.Equals(typeof(System.IO.FileNotFoundException).FullName))
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

        private void EnsurePublish(File file, string publishComment)
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
            if (parentList.EnableModeration && Convert.ToInt32(file.ListItemAllFields["_ModerationStatus"]) != 0)
            {
                file.Approve(string.Empty);
                isDirty = true;
            }

            if (isDirty)
            {
                file.RefreshLoad();
                _clientContext.ExecuteQuery();
            }
        }
    }
}
