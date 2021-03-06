﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Web.UI;
using Swarmops.Logic.Support;

namespace Swarmops.Frontend.Controls.v5.UI
{
    public partial class ExternalScripts : ControlV5Base
    {
        public string Package { get; set; }
        public new string Controls { get; set; }

        protected void Page_Load (object sender, EventArgs e)
        {
            string externalScriptUrl = "//hostedscripts.falkvinge.net";

            string testFolderName = Server.MapPath ("~/Scripts/ExternalScripts");
            if (Directory.Exists (testFolderName))
            {
                externalScriptUrl = "/Scripts/ExternalScripts";
            }
            else if (Debugger.IsAttached ||
                     PilotInstallationIds.IsPilot (PilotInstallationIds.DevelopmentSandbox))
            {
                externalScriptUrl += "/staging";
                // use staging area for new script versions on Sandbox and for all debugging
            }

            // If we're debugging a seriously experimental new version of JEasyUI, look for it in /Scripts/Experimental
            // (a folder which doesn't commit to the github repo)

            if (File.Exists (Server.MapPath ("~/Scripts/Experimental/easyui/jquery.easyui.min.js")))
            {
                externalScriptUrl = "/Scripts/Experimental";
            }

            if (Package == "easyui")
            {
                StringBuilder scriptRef = new StringBuilder();

                scriptRef.Append("<script src=\"" + externalScriptUrl +
                                  "/easyui/jquery.easyui.min.js\" type=\"text/javascript\"></script>\r\n");
                scriptRef.Append("<link rel=\"stylesheet\" type=\"text/css\" href=\"" + externalScriptUrl +
                                  "/easyui/themes/icon.css\" />\r\n");
                scriptRef.Append("<link rel=\"stylesheet\" type=\"text/css\" href=\"" + externalScriptUrl +
                                  "/easyui/themes/default/easyui.css\" />\r\n");  // Supposed to contain all CSS

                if (Thread.CurrentThread.CurrentCulture.TextInfo.IsRightToLeft)
                {
                    scriptRef.Append("<script src=\"" + externalScriptUrl +
                                      "/easyui/extensions/easyui-rtl.js\" type=\"text/javascript\"></script>\r\n");
                    scriptRef.Append("<link rel=\"stylesheet\" type=\"text/css\" href=\"" + externalScriptUrl +
                                      "/easyui/extensions/easyui-rtl.css\" />\r\n");
                }

                /* -- with the inclusion of the catchall CSS file, this code _should_ no longer be necessary...
                string[] controlNames = Controls.Split(',');
                foreach (string controlName in controlNames)
                {
                    string controlNameLower = controlName.Trim().ToLowerInvariant();
                    if (controlNameLower != "unknown")
                    {
                        scriptRef.AppendFormat (
                            "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + externalScriptUrl +
                            "/easyui/themes/default/{0}.css\" />\r\n",
                            controlNameLower);
                    }
                }*/

                this.LiteralReference.Text = scriptRef.ToString();
            }
        }
    }
}