using System;
using System.Management.Automation;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.WorkflowServices;

namespace SPSD.Extensions.Client
{
    [Cmdlet(VerbsCommon.Add, "SPWorkflowSubscription")]
    public class AddWorkflowSubscriptionCmdlet : CmdletBase
    {
        private Guid _eventSourceId;
        private string _taskListId;
        private string _historyListId;

        /// <summary>
        /// Gets or sets the unique identifier of the workflow subscription for the specified event source. 
        /// </summary>
        [Parameter(Mandatory = true)]
        public string Id { get; set; }
        /// <summary>
        /// Gets or sets the name of the workflow subscription for the specified event source. 
        /// </summary>
        [Parameter(Mandatory = true)]
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the unique ID of the workflow definition to activate. 
        /// Must be the ID of a workflow definition that is deployed to the current site (SPWeb).
        /// </summary>
        [Parameter(Mandatory = true)]
        public string DefinitionId { get; set; }
        /// <summary>
        /// If set to true task and history lists that do not exists are created during deployment.
        /// </summary>
        [Parameter]
        public bool CreateLists { get; set; }
        /// <summary>
        /// Gets or sets a Boolean value that enables or disables the workflow subscription. When disabled, new instances of the subscription cannot be started, but existing instances will continue to run. 
        /// </summary>
        [Parameter]
        public bool Enabled { get; set; }
        /// <summary>
        /// Gets or sets the logical source instance name of the event. 
        /// This can be the name of a SharePoint list or null for a site workflow.
        /// </summary>
        [Parameter]
        public string EventSourceName { get; set; }
        /// <summary>
        /// Gets or sets the list of event types for which the subscription is listening. For SharePoint events, these will map to a value in the SPEventReceiverType enumeration.
        /// E.g. ItemAdding or ItemUpdated. Also the value WorkflowStart is a valid event type and indicates that the workflow can be started manually.
        /// </summary>
        [Parameter]
        public string[] EventTypes { get; set; }
        /// <summary>
        /// Name of the history list used with the workflow.
        /// </summary>
        [Parameter]
        public string HistoryListName { get; set; }
        /// <summary>
        /// Boolean value that specifies whether multiple workflow instances can be started manually on the same list item at the same time. This property can be used for list workflows only. 
        /// </summary>
        [Parameter]
        public bool ManualStartBypassesActivationLimit { get; set; }
        /// <summary>
        /// Gets or sets the name of the workflow status field on the specified list. 
        /// </summary>
        [Parameter]
        public string StatusFieldName { get; set; }
        /// <summary>
        /// Name of the task list for the workflow.
        /// </summary>
        [Parameter]
        public string TaskListName { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            WriteObject(string.Format("Deploying workflow subscription {0} to {1}", Name, SiteUrl));

            var workflowServicesManager = new WorkflowServicesManager(_clientContext, _clientContext.Web);
            var workflowSubscriptionService = workflowServicesManager.GetWorkflowSubscriptionService();

            // Get list ids.
            GetIdsFromSharePoint();

            var workflowSubscription = workflowSubscriptionService.GetSubscription(new Guid(Id));
            _clientContext.Load(workflowSubscription, w => w);
            _clientContext.ExecuteQuery();
            if (workflowSubscription.ServerObjectIsNull == true)
            {
                workflowSubscription = new WorkflowSubscription(_clientContext);
            }
            workflowSubscription.Id = new Guid(Id);
            workflowSubscription.DefinitionId = new Guid(DefinitionId);
            workflowSubscription.Name = Name;
            workflowSubscription.EventSourceId = _eventSourceId;
            workflowSubscription.Enabled = Enabled;
            workflowSubscription.EventTypes = EventTypes ?? new string[] {};
            workflowSubscription.ManualStartBypassesActivationLimit = ManualStartBypassesActivationLimit;
            if (!string.IsNullOrEmpty(StatusFieldName))
            {
                workflowSubscription.StatusFieldName = StatusFieldName;
            }

            if (!string.IsNullOrEmpty(_taskListId))
            {
                workflowSubscription.SetProperty("TaskListId", _taskListId);
            }
            if (!string.IsNullOrEmpty(_historyListId))
            {
                workflowSubscription.SetProperty("HistoryListId", _historyListId);
            }

            if (string.IsNullOrEmpty(EventSourceName))
            {
                // We assume we are deploying a site workflow (no event source specified).
                workflowSubscriptionService.PublishSubscription(workflowSubscription);
            }
            else
            {
                // We assume we are deploying a list workflow otherwise.
                workflowSubscriptionService.PublishSubscriptionForList(workflowSubscription, _eventSourceId);
            }
            _clientContext.ExecuteQuery();
            WriteObject("Workflow subscription published.");
        }

        private void GetIdsFromSharePoint()
        {
            if (!string.IsNullOrEmpty(EventSourceName))
            {
                // List workflow. Resolve the list id.
                var eventSourceList = _clientContext.Web.Lists.GetByTitle(EventSourceName);
                _clientContext.Load(eventSourceList, l => l.Id, l => l.Title);
                _clientContext.ExecuteQuery();
                _eventSourceId = new Guid(eventSourceList.Id.ToString());
            }
            else
            {
                // Site workflow. Get the site id.
                _clientContext.Load(_clientContext.Web, w => w.Id);
                _clientContext.ExecuteQuery();
                _eventSourceId = _clientContext.Web.Id;
            }

            if (!string.IsNullOrEmpty(TaskListName))
            {
                var taskList = _clientContext.Web.Lists.GetByTitle(TaskListName);
                _clientContext.Load(taskList, l => l.Id, l => l.Title);
                if (taskList != null)
                {
                    try
                    {
                        _clientContext.ExecuteQuery();
                        _taskListId = taskList.Id.ToString();
                    }
                    catch
                    {
                        // Try to create the list.
                        if (CreateLists)
                        {
                            CreateTaskList();
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(HistoryListName))
            {
                var historyList = _clientContext.Web.Lists.GetByTitle(HistoryListName);
                _clientContext.Load(historyList, l => l.Id, l => l.Title);
                if (historyList != null)
                {
                    try
                    {
                        _clientContext.ExecuteQuery();
                        _historyListId = historyList.Id.ToString();

                    }
                    catch
                    {
                        // Try to create the list.
                        if (CreateLists)
                        {
                            CreateHistoryList();
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
        }

        private void CreateTaskList()
        {
            var creationInfo = new ListCreationInformation
            {
                Title = TaskListName,
                TemplateType = (int) ListTemplateType.TasksWithTimelineAndHierarchy,
                QuickLaunchOption = QuickLaunchOptions.On
            };
            var list = _clientContext.Web.Lists.Add(creationInfo);
            list.ContentTypes.AddExistingContentType(_clientContext.Web.ContentTypes.GetById("0x0108003365C4474CAE8C42BCE396314E88E51F"));
            _clientContext.Load(list, l => l.Id, l => l.Title);
            _clientContext.ExecuteQuery();
            _taskListId = list.Id.ToString();
        }


        private void CreateHistoryList()
        {
            var creationInfo = new ListCreationInformation
            {
                Title = HistoryListName,
                TemplateType = (int)ListTemplateType.WorkflowHistory,
                QuickLaunchOption = QuickLaunchOptions.Off
            };
            var list = _clientContext.Web.Lists.Add(creationInfo);
            list.Hidden = true;
            list.Update();
            _clientContext.Load(list, l => l.Id, l => l.Title);
            _clientContext.ExecuteQuery();
            _historyListId = list.Id.ToString();
        }

    }
}
