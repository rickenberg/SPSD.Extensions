using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SPSD.Extensions.Client.Test
{
    [TestClass]
    public class AddWorkflowDefinitionTest
    {
        [TestMethod]
        public void SimpleDeployListWorkflow()
        {
            var cmd = new AddWorkflowDefinitionCmdlet
                {
                    SiteUrl = OnPremTestContext.SiteUrl,
                    Username = OnPremTestContext.Username,
                    Password = OnPremTestContext.Password,
                    Publish = true,
                    Description = "Gets the users full name from site users and updates the contact list item.",
                    DisplayName = "Full Name (List)",
                    Id = "C571BD89-205C-44A6-8134-2CAA3C4DDFF4",
                    RestrictToType = "List",
                    RestrictToListName = "Contacts",
                    FilePath = "workflows\\full-name-http.xaml"
                };
            var result = cmd.Invoke();

            Debug.WriteLine("***** Executing {0} at {1}", cmd.GetType(), DateTime.Now);

            foreach (var line in result)
                Debug.WriteLine("- {0}", line);

            Debug.WriteLine("***** Finished {0} at {1}", cmd.GetType(), DateTime.Now);
        }

        [TestMethod]
        public void SimpleDeployListWorkflowReusable()
        {
            var cmd = new AddWorkflowDefinitionCmdlet
            {
                SiteUrl = OnPremTestContext.SiteUrl,
                Username = OnPremTestContext.Username,
                Password = OnPremTestContext.Password,
                Publish = true,
                Description = "Gets the users full name from site users and updates the contact list item.",
                DisplayName = "Full Name (Reusable)",
                Id = "F7C4E3EC-E4AB-4DD3-A9B5-FF69C75D0DFB",
                RestrictToType = "List",
                FilePath = "workflows\\full-name-http.xaml"
            };
            var result = cmd.Invoke();

            Debug.WriteLine("***** Executing {0} at {1}", cmd.GetType(), DateTime.Now);

            foreach (var line in result)
                Debug.WriteLine("- {0}", line);

            Debug.WriteLine("***** Finished {0} at {1}", cmd.GetType(), DateTime.Now);
        }

        [TestMethod]
        public void SimpleDeploySiteWorkflow()
        {
            var cmd = new AddWorkflowDefinitionCmdlet
            {
                SiteUrl = OnPremTestContext.SiteUrl,
                Username = OnPremTestContext.Username,
                Password = OnPremTestContext.Password,
                Publish = true,
                Description = "Writes a message to the history list",
                DisplayName = "Simple WF Site",
                Id = "E6120B11-F257-4057-A324-FE2E22CAC28E",
                RestrictToType = "Site",
                FilePath = "workflows\\simple-site.xaml"
            };
            var result = cmd.Invoke();

            Debug.WriteLine("***** Executing {0} at {1}", cmd.GetType(), DateTime.Now);

            foreach (var line in result)
                Debug.WriteLine("- {0}", line);

            Debug.WriteLine("***** Finished {0} at {1}", cmd.GetType(), DateTime.Now);
        }
    }
}
