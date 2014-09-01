using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SPSD.Extensions.Client.Test
{
    [TestClass]
    public class AddWorkflowDefinitionSpoTest
    {
        [TestMethod]
        public void SimpleDeployListWorkflow()
        {
            var cmd = new AddWorkflowDefinitionCmdlet
                {
                    SiteUrl = SpoTestContext.SiteUrl,
                    Username = SpoTestContext.Username,
                    Password = SpoTestContext.Password,
                    Publish = true,
                    Description = "Gets the users full name from site users and updates the contact list item.",
                    DisplayName = "Full Name",
                    Id = "98FAD635-A48B-49F8-9775-1A846E51E2FD",
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
                SiteUrl = SpoTestContext.SiteUrl,
                Username = SpoTestContext.Username,
                Password = SpoTestContext.Password,
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
                SiteUrl = SpoTestContext.SiteUrl,
                Username = SpoTestContext.Username,
                Password = SpoTestContext.Password,
                Publish = true,
                Description = "Writes a message to workflow history",
                DisplayName = "Simple Site WF",
                Id = "E6120B11-F257-4057-A324-FE2E22CAC290",
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
