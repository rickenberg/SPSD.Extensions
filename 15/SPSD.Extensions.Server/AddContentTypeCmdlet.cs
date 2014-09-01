using System.Management.Automation;
using System.Xml.Linq;
using Microsoft.SharePoint;

namespace SPSD.Extensions.Server
{
    [Cmdlet(VerbsCommon.Add, "SPContentType")]
    public class AddContentTypeCmdlet : Cmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string Url { get; set; }

        [Parameter]
        public string SchemaPath { get; set; }

        protected override void ProcessRecord()
        {
            var schema = XDocument.Load(SchemaPath);
            XNamespace ns = "http://schemas.microsoft.com/sharepoint/";

            using (var site = new SPSite(Url))
            {
                var web = site.OpenWeb();

                // Process fields.
                foreach (var field in schema.Root.Descendants(ns+"Field"))
                {
                    WriteObject("Processing " + field.Attribute("Name").Value);

                    var fieldName = web.Fields.Add(field.Attribute("Name").Value, SPFieldType.Text, false);
                    var spField = web.Fields.GetFieldByInternalName(fieldName);
                    spField.StaticName = field.Attribute("StaticName").Value;
                    spField.Title = field.Attribute("DisplayName").Value;
                    spField.Update();
                }

                // Process content types.
            }

            return;

            using (var site = new SPSite(Url))
            {
                var web = site.RootWeb;

                var field = web.Fields["URL"];

                var id = new SPContentTypeId("0x01010075425CE93BDC404F8B042629FC235785");
                var termsAndConditionsType = new SPContentType(id, web.ContentTypes, "TermsAndConditionsType");
                web.ContentTypes.Add(termsAndConditionsType);
                termsAndConditionsType = web.ContentTypes[id];
                termsAndConditionsType.Group = "Custom Content Types";
                termsAndConditionsType.Description = "Custom Content Type for Terms and Conditions";
                termsAndConditionsType.Update();
                var l = new SPFieldLink(field) { DisplayName = "My URL" };
                termsAndConditionsType.FieldLinks.Add(l);
                termsAndConditionsType.Update();
            }
        }
    }
}
