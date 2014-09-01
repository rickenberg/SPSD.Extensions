using System;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Microsoft.SharePoint.Client.WorkflowServices;

namespace SPSD.Extensions.Client
{
    [Cmdlet(VerbsCommon.Add, "SPWorkflowDefinition")]
    public class AddWorkflowDefinitionCmdlet : CmdletBase
    {
        [Parameter(Mandatory = true)]
        public bool Publish { get; set; }

        /// <summary>
        /// Pipe an input XAML file from PowerShell.
        /// </summary>
        [Parameter(ValueFromPipeline = true)]
        public FileInfo SourceFile { get; set; }
        /// <summary>
        /// Path to input file (XAML).
        /// </summary>
        [Parameter]
        public string FilePath { get; set; }

        [Parameter]
        public string Description { get; set; }
        [Parameter(Mandatory = true)]
        public string DisplayName { get; set; }
        [Parameter]
        public string DraftVersion { get; set; }
        /// <summary>
        /// Gets or sets a FieldML string that defines the fields of the workflow's initiation and association forms.
        /// </summary>
        [Parameter]
        public string FormField { get; set; }
        /// <summary>
        /// Gets or sets the Guid identifier for the workflow definition.
        /// </summary>
        [Parameter(Mandatory = true)]
        public string Id { get; set; }
        /// <summary>
        /// If set to true, and the InitiationUrl property value is not already set, a default initiation form is automatically generated for the workflow when SaveDefinition(WorkflowDefinition) is called.
        /// </summary>
        [Parameter]
        public bool RequiresInitiationForm { get; set; }
        /// <summary>
        /// If this value is a null reference (Nothing in Visual Basic) or empty, the workflow does not have an initiation form.
        /// </summary>
        [Parameter]
        public string InitiationUrl { get; set; }
        /// <summary>
        /// If set to true, and the AssociationUrl property value is not already set, a default association form is automatically generated for the workflow when SaveDefinition(WorkflowDefinition) is called.
        /// </summary>
        [Parameter]
        public bool RequiresAssociationForm { get; set; }
        /// <summary>
        /// If this property value is a null reference (Nothing in Visual Basic) or empty, the workflow does not have an association form.
        /// </summary>
        [Parameter]
        public string AssociationUrl { get; set; }
        /// <summary>
        /// Used to set the RestrictToScope to the GUID of the list specified here.
        /// It is important to set this property when working with list scoped workflows. If you do not set the RestrictToScope (via guid or list name) the workflow will be reusable
        /// and loose all it's bindings to list specific columns.
        /// </summary>
        [Parameter]
        public string RestrictToListName { get; set; }
        /// <summary>
        /// Used in conjunction with the RestrictToType property to restrict the scope of the workflow definition. For example, if the RestrictToType property value is "List" then setting the RestrictToScope value to a particular list id limits the definition to an association only to the specified list. If the RestrictToType is "List" but the RestrictToScope is a null reference (Nothing in Visual Basic) or empty, then the definition in associable to any list.
        /// </summary>
        [Parameter]
        public string RestrictToScope { get; set; }
        /// <summary>
        /// Possible values include "List", "Site", "", or a null reference (Nothing in Visual Basic). For example, a value of "List" indicates this definition is a list workflow and can only be associated to a SharePoint list. An Empty or a null reference (Nothing in Visual Basic) value indicates this definition is a universal template and is able to associate with any event source type.
        /// </summary>
        [Parameter]
        public string RestrictToType { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            WriteObject(string.Format("Deploying workflow definition {0} to {1}", DisplayName, SiteUrl));

            if (SourceFile != null)
            {
                ProcessFile(SourceFile);
                return;
            }

            if (!File.Exists(FilePath))
            {
                throw new FileNotFoundException("Input file not found");
            }

            ProcessFile(new FileInfo(FilePath));
        }

        protected void ProcessFile(FileInfo xamlFile)
        {
            // Load and validate XAML.
            var workflowServicesManager = new WorkflowServicesManager(_clientContext, _clientContext.Web);
            var workflowDeploymentService = workflowServicesManager.GetWorkflowDeploymentService();
            var xaml = File.ReadAllText(xamlFile.FullName);
            var validationResult = workflowDeploymentService.ValidateActivity(xaml);
            _clientContext.ExecuteQuery();
            if (!string.IsNullOrEmpty(validationResult.Value))
            {
                throw new PSSnapInException(string.Format("XAML validation failed: {0}.", validationResult.Value));
            }

            var workflowDefinition = workflowDeploymentService.GetDefinition(new Guid(Id));
            _clientContext.Load(workflowDefinition, w => w);
            _clientContext.ExecuteQuery();

            // Create new definition if not exists.
            if (workflowDefinition.ServerObjectIsNull == true)
            {
                workflowDefinition = new WorkflowDefinition(_clientContext);
            }

            // Determine list name before setting other values (requires a round-trip to server)
            workflowDefinition.RestrictToScope = RestrictToScope ?? GetRestrictToScopeFromListName();
            workflowDefinition.AssociationUrl = AssociationUrl;
            workflowDefinition.Description = Description;
            workflowDefinition.DisplayName = DisplayName;
            workflowDefinition.DraftVersion = DraftVersion;
            workflowDefinition.FormField = FormField;
            workflowDefinition.Id = new Guid(Id);
            workflowDefinition.InitiationUrl = InitiationUrl;
            workflowDefinition.RequiresAssociationForm = RequiresAssociationForm;
            workflowDefinition.RequiresInitiationForm = RequiresInitiationForm;
            workflowDefinition.RestrictToType = RestrictToType;
            workflowDefinition.Xaml = xaml;

            // Save changes.
            workflowDeploymentService.SaveDefinition(workflowDefinition);
            _clientContext.ExecuteQuery();
            WriteObject("Workflow definition saved.");

            // Publish the workflow.
            if (Publish)
            {
                workflowDeploymentService.PublishDefinition(workflowDefinition.Id);
                _clientContext.ExecuteQuery();
                WriteObject("Workflow definition published.");
            }
        }

        private string GetRestrictToScopeFromListName()
        {
            // Get list ids.
            if (!string.IsNullOrEmpty(RestrictToListName))
            {
                var restrictToList = _clientContext.Web.Lists.GetByTitle(RestrictToListName);
                _clientContext.Load(restrictToList, l => l.Id, l => l.Title);
                _clientContext.ExecuteQuery();
                return restrictToList.Id.ToString();
            }

            return null;
        }
    }
}
