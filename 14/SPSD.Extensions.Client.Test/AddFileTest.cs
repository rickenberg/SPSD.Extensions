using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SPSD.Extensions.Client.Test
{
    /// <summary>
    /// Integration tests for the Add-SPFile commandlet for SharePoint 2013 on-premises.
    /// </summary>
    [TestClass]
    public class AddFileTest
    {
        // TODO: Change the values to match your environment.
        private const string SiteUrl = "http://intranet";
        private const string Username = "pwdev\\spuser";
        private const string Password = "W1ndows";
        private const string RelativeDocLibUrl = "Shared Documents";

        [TestMethod]
        public void SimpleFileExecute()
        {
            var cmd = new AddFileCmdlet
            {
                SiteUrl = SiteUrl,
                Username = Username,
                Password = Password,
                SourceFile = new FileInfo("files\\document1.docx"),
                RelativeDocLibUrl = RelativeDocLibUrl
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
                SiteUrl = SiteUrl,
                Username = Username,
                Password = Password,
                SourceFile = new FileInfo("files\\document1.docx"),
                RelativeDocLibUrl = RelativeDocLibUrl + "/Folder1-1"
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
                SiteUrl = SiteUrl,
                Username = Username,
                Password = Password,
                SourceFile = new FileInfo("files\\document1.docx"),
                RelativeDocLibUrl = RelativeDocLibUrl + "/Folder2-1/Folder2-2",
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
                SiteUrl = SiteUrl,
                Username = Username,
                Password = Password,
                SourceDirectory = new DirectoryInfo("files"),
                RelativeDocLibUrl = RelativeDocLibUrl
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
                SiteUrl = SiteUrl,
                Username = Username,
                Password = Password,
                RelativeDocLibUrl = RelativeDocLibUrl
            };
            var result = cmd.Invoke();

            Debug.WriteLine("***** Executing {0} at {1}", cmd.GetType(), DateTime.Now);

            foreach (var line in result)
                Debug.WriteLine("- {0}", line);

            Debug.WriteLine("***** Finished {0} at {1}", cmd.GetType(), DateTime.Now);
        }
    }
}
