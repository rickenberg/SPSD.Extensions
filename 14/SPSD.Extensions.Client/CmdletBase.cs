using Microsoft.SharePoint.Client;
using System.Management.Automation;

namespace SPSD.Extensions.Client
{
    public class CmdletBase : Cmdlet
    {
        protected ClientContext _clientContext;

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string SiteUrl { get; set; }
        [Parameter]
        public string Username { get; set; }
        [Parameter]
        public string Password { get; set; }

        protected override void ProcessRecord()
        {
            _clientContext = new ClientContext(SiteUrl);
            _clientContext.ExecutingWebRequest += ExecutingWebRequestHandler;

            if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password) && Username.Contains("\\"))
            {
                var domain = Username.Split("\\")[0];
                var username = Username.Split("\\")[1];
                var credentials = new NetworkCredential(username, Password, domain); 
                _clientContext = credentials;
            }
        }
        
        /// <summary>
        /// Disable FORMS authentication to work-around auth error when activated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ExecutingWebRequestHandler(object sender, WebRequestEventArgs e)
        {
            e.WebRequestExecutor.WebRequest.Headers.Add("X-FORMS_BASED_AUTH_ACCEPTED", "f");
        }
    }
}
