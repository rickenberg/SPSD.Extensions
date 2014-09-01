using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SPSD.Extensions.Client.Test
{
    [TestClass]
    public class AddWorkflowSubscriptionTest
    {
        [TestMethod]
        public void SimpleExecute()
        {
            var cmd = new AddWorkflowSubscriptionCmdlet
            {
                SiteUrl = OnPremTestContext.SiteUrl,
                Username = OnPremTestContext.Username,
                Password = OnPremTestContext.Password,
                DefinitionId = "C571BD89-205C-44A6-8134-2CAA3C4DDFF2",
                Enabled = true,
                EventSourceName = "Contacts",
                EventTypes = new[] { "WorkflowStart" },
                Id = "07B5BCE2-EB1B-439D-A692-07309ACACD61",
                Name = "Simple WF",
            };
            var result = cmd.Invoke();

            Debug.WriteLine("***** Executing {0} at {1}", cmd.GetType(), DateTime.Now);

            foreach (var line in result)
                Debug.WriteLine("- {0}", line);

            Debug.WriteLine("***** Finished {0} at {1}", cmd.GetType(), DateTime.Now);
        }

        [TestMethod]
        public void SimpleExecuteWithTasks()
        {
            var cmd = new AddWorkflowSubscriptionCmdlet
            {
                SiteUrl = OnPremTestContext.SiteUrl,
                Username = OnPremTestContext.Username,
                Password = OnPremTestContext.Password,
                DefinitionId = "C571BD89-205C-44A6-8134-2CAA3C4DDFF2",
                Enabled = true,
                EventSourceName = "Contacts",
                EventTypes = new[] { "WorkflowStart" },
                Id = "07B5BCE2-EB1B-439D-A692-07309ACACD60",
                Name = "Simple WF with Tasks",
                TaskListName = "Workflow Tasks"
            };
            var result = cmd.Invoke();

            Debug.WriteLine("***** Executing {0} at {1}", cmd.GetType(), DateTime.Now);

            foreach (var line in result)
                Debug.WriteLine("- {0}", line);

            Debug.WriteLine("***** Finished {0} at {1}", cmd.GetType(), DateTime.Now);
        }

        [TestMethod]
        public void SimpleExecuteWithHistory()
        {
            var cmd = new AddWorkflowSubscriptionCmdlet
            {
                SiteUrl = OnPremTestContext.SiteUrl,
                Username = OnPremTestContext.Username,
                Password = OnPremTestContext.Password,
                DefinitionId = "C571BD89-205C-44A6-8134-2CAA3C4DDFF2",
                Enabled = true,
                EventSourceName = "Contacts",
                HistoryListName = "Workflow History",
                Id = "07B5BCE2-EB1B-439D-A692-07309ACACD62",
                Name = "Simple WF with History",
            };
            var result = cmd.Invoke();

            Debug.WriteLine("***** Executing {0} at {1}", cmd.GetType(), DateTime.Now);

            foreach (var line in result)
                Debug.WriteLine("- {0}", line);

            Debug.WriteLine("***** Finished {0} at {1}", cmd.GetType(), DateTime.Now);
        }

        [TestMethod]
        public void SimpleExecuteWithTasksAndHistory()
        {
            var cmd = new AddWorkflowSubscriptionCmdlet
                {
                    SiteUrl = OnPremTestContext.SiteUrl,
                    Username = OnPremTestContext.Username,
                    Password = OnPremTestContext.Password,
                    DefinitionId = "C571BD89-205C-44A6-8134-2CAA3C4DDFF2",
                    Enabled = true,
                    EventSourceName = "Contacts",
                    HistoryListName = "Workflow History",
                    Id = "07B5BCE2-EB1B-439D-A692-07309ACACD64",
                    Name = "Simple WF with Tasks and History",
                    TaskListName = "Workflow Tasks"
                };
            var result = cmd.Invoke();

            Debug.WriteLine("***** Executing {0} at {1}", cmd.GetType(), DateTime.Now);

            foreach (var line in result)
                Debug.WriteLine("- {0}", line);

            Debug.WriteLine("***** Finished {0} at {1}", cmd.GetType(), DateTime.Now);
        }

        [TestMethod]
        public void SimpleExecuteWithTasksAndHistoryWithListCreate()
        {
            var cmd = new AddWorkflowSubscriptionCmdlet
            {
                SiteUrl = OnPremTestContext.SiteUrl,
                Username = OnPremTestContext.Username,
                Password = OnPremTestContext.Password,
                CreateLists = true,
                DefinitionId = "C571BD89-205C-44A6-8134-2CAA3C4DDFF2",
                Enabled = true,
                EventSourceName = "Contacts",
                HistoryListName = "Workflow History 1",
                Id = "07B5BCE2-EB1B-439D-A692-07309ACACD63",
                Name = "Simple WF with Tasks and History (create lists)",
                TaskListName = "Workflow Tasks 1"
            };
            var result = cmd.Invoke();

            Debug.WriteLine("***** Executing {0} at {1}", cmd.GetType(), DateTime.Now);

            foreach (var line in result)
                Debug.WriteLine("- {0}", line);

            Debug.WriteLine("***** Finished {0} at {1}", cmd.GetType(), DateTime.Now);
        }

        [TestMethod]
        public void SimpleExecuteSite()
        {
            var cmd = new AddWorkflowSubscriptionCmdlet
            {
                SiteUrl = OnPremTestContext.SiteUrl,
                Username = OnPremTestContext.Username,
                Password = OnPremTestContext.Password,
                DefinitionId = "E6120B11-F257-4057-A324-FE2E22CAC28E",
                Enabled = true,
                EventTypes = new [] { "WorkflowStart" },
                Id = "07B5BCE2-EB1B-439D-A692-07309ACACD70",
                Name = "Simple Site WF",
                HistoryListName = "Workflow History"
            };
            var result = cmd.Invoke();

            Debug.WriteLine("***** Executing {0} at {1}", cmd.GetType(), DateTime.Now);

            foreach (var line in result)
                Debug.WriteLine("- {0}", line);

            Debug.WriteLine("***** Finished {0} at {1}", cmd.GetType(), DateTime.Now);
        }
    }
}
