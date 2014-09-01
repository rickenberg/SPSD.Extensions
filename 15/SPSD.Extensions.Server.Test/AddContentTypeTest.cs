using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SPSD.Extensions.Server.Test
{
    [TestClass]
    public class AddContentTypeTest
    {
        [TestMethod]
        public void SimpleAddContentTypeExecute()
        {
            var cmd = new AddContentTypeCmdlet
            {
                Url = "http://demo2",
                SchemaPath = "ContentTypes\\BusinessContentType.xml"
            };
            var result = cmd.Invoke();

            Debug.WriteLine("***** Executing {0} at {1}", cmd.GetType(), DateTime.Now);

            foreach (var line in result)
                Debug.WriteLine("- {0}", line);

            Debug.WriteLine("***** Finished {0} at {1}", cmd.GetType(), DateTime.Now);
        }
    }
}
