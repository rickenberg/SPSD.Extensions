using System;
using System.Management.Automation;
using System.Net;
using System.Security;
using Microsoft.SharePoint.Client;

namespace SPSD.Extensions.Client
{
    public class CmdletBase : Cmdlet
    {
        protected ClientContext _clientContext;
        protected bool _isSharePointOnline;

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

            // On-prem authentication.
            // I assume that you do not use user names with '@' for on-prem purposes but rather something like 'mydomain\user1'.
            if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password) && !Username.Contains("@"))
            {
                _clientContext.Credentials = new NetworkCredential(Username, Password);
            }

            // SharePoint Online authentication.
            if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password) && Username.Contains("@"))
            {
                _isSharePointOnline = true;
                var seucrePassword = new SecureString();
                foreach (var c in Password)
                {
                    seucrePassword.AppendChar(c);
                }
                _clientContext.Credentials = new SharePointOnlineCredentials(Username, seucrePassword);
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
