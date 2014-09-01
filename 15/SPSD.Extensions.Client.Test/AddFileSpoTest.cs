using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SPSD.Extensions.Client.Test
{
    /// <summary>
    /// Integration tests for the Add-SPFile commandlet for SharePoint Online.
    /// </summary>
    [TestClass]
    public class AddFileSpoTest
    {
        [TestMethod]
        public void SimpleFileExecute()
        {
            var cmd = new AddFileCmdlet
            {
                SiteUrl = SpoTestContext.SiteUrl,
                Username = SpoTestContext.Username,
                Password = SpoTestContext.Password,
                SourceFile = new FileInfo("files\\document1.docx"),
                RelativeDocLibUrl = SpoTestContext.RelativeDocLibUrl
            };
            var result = cmd.Invoke();

            Debug.WriteLine("***** Executing {0} at {1}", cmd.GetType(), DateTime.Now);

            foreach (var line in result)
                Debug.WriteLine("- {0}", line);

            Debug.WriteLine("***** Finished {0} at {1}", cmd.GetType(), DateTime.Now);
        }

        [TestMethod]
        public void SubFolder1LevelExecute()
        {
            var cmd = new AddFileCmdlet
            {
                SiteUrl = SpoTestContext.SiteUrl,
                Username = SpoTestContext.Username,
                Password = SpoTestContext.Password,
                SourceFile = new FileInfo("files\\document1.docx"),
                RelativeDocLibUrl = SpoTestContext.RelativeDocLibUrl + "/Folder1-1"
            };
            var result = cmd.Invoke();

            Debug.WriteLine("***** Executing {0} at {1}", cmd.GetType(), DateTime.Now);

            foreach (var line in result)
                Debug.WriteLine("- {0}", line);

            Debug.WriteLine("***** Finished {0} at {1}", cmd.GetType(), DateTime.Now);
        }

        [TestMethod]
        public void SubFolder2LevelExecute()
        {
            var cmd = new AddFileCmdlet
            {
                SiteUrl = SpoTestContext.SiteUrl,
                Username = SpoTestContext.Username,
                Password = SpoTestContext.Password,
                SourceFile = new FileInfo("files\\document1.docx"),
                RelativeDocLibUrl = SpoTestContext.RelativeDocLibUrl + "/Folder2-1/Folder2-2",
                NoOverwrite = false,
                AddUpdateOnly = false
            };
            var result = cmd.Invoke();

            Debug.WriteLine("***** Executing {0} at {1}", cmd.GetType(), DateTime.Now);

            foreach (var line in result)
                Debug.WriteLine("- {0}", line);

            Debug.WriteLine("***** Finished {0} at {1}", cmd.GetType(), DateTime.Now);
        }

        [TestMethod]
        public void SimpleDirectoryExecute()
        {
            var cmd = new AddFileCmdlet
            {
                SiteUrl = SpoTestContext.SiteUrl,
                Username = SpoTestContext.Username,
                Password = SpoTestContext.Password,
                SourceDirectory = new DirectoryInfo("files"),
                RelativeDocLibUrl = SpoTestContext.RelativeDocLibUrl
            };
            var result = cmd.Invoke();

            Debug.WriteLine("***** Executing {0} at {1}", cmd.GetType(), DateTime.Now);

            foreach (var line in result)
                Debug.WriteLine("- {0}", line);

            Debug.WriteLine("***** Finished {0} at {1}", cmd.GetType(), DateTime.Now);
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void NoInputFileExecute()
        {
            var cmd = new AddFileCmdlet
            {
                SiteUrl = SpoTestContext.SiteUrl,
                Username = SpoTestContext.Username,
                Password = SpoTestContext.Password,
                RelativeDocLibUrl = SpoTestContext.RelativeDocLibUrl
            };
            var result = cmd.Invoke();

            Debug.WriteLine("***** Executing {0} at {1}", cmd.GetType(), DateTime.Now);

            foreach (var line in result)
                Debug.WriteLine("- {0}", line);

            Debug.WriteLine("***** Finished {0} at {1}", cmd.GetType(), DateTime.Now);
        }
    }
}
