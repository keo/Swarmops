using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using NBitcoin;
using Resources;
using Swarmops.Interface.Support;
using Swarmops.Logic.Financial;
using Swarmops.Logic.Security;
using Swarmops.Logic.Structure;
using Swarmops.Logic.Support;
using Swarmops.Logic.Swarm;

namespace Swarmops
{
    public partial class MasterV5 : MasterV5Base
    {
        protected void Page_Init (object sender, EventArgs e)
        {
            try
            {
                this._authority = CommonV5.GetAuthenticationDataAndCulture(HttpContext.Current).Authority;
            }
            catch (Exception)
            {
                // if this fails FOR WHATEVER REASON then we're not authenticated

                this._authority = null;
                FormsAuthentication.SignOut();
                Response.Redirect("/", true);
            }

            // BEGIN TEST CODE



            // END TEST CODE


            if (this._authority.Organization.Identity == 3 &&
                PilotInstallationIds.IsPilot (PilotInstallationIds.PiratePartySE))
            {
                this._authority = null;
                FormsAuthentication.SignOut();
                Response.Redirect("/", true);
            }
        }

        protected void Page_Load (object sender, EventArgs e)
        {
            // Event subscriptions

            // Titles and other page elements

            Page.Title = "Swarmops - " + CurrentPageTitle;

            this.ExternalScriptEasyUI.Controls = EasyUIControlsUsed.ToString();
            this.IncludedScripts.Controls = IncludedControlsUsed.ToString();

            this.LiteralSidebarInfo.Text = CurrentPageInfoBoxLiteral;

            // Set logo image. If custom image is installed, use it instead.

            this.ImageLogo.ImageUrl = "~/Images/Logo-Stock.png";
            if (File.Exists (Server.MapPath ("~/Images/Logo-Custom.png")))
            {
                this.ImageLogo.ImageUrl = "~/Images/Logo-Custom.png";
            }

            // Check for SSL and force it

            // Special case for CloudFlare deployments - there is a case where somebody will get their connections de-SSLed at the server

            string cloudFlareVisitorScheme = Request.Headers["CF-Visitor"];
            bool cloudFlareSsl = false;

            if (!string.IsNullOrEmpty (cloudFlareVisitorScheme))
            {
                if (cloudFlareVisitorScheme.Contains ("\"scheme\":\"https\""))
                {
                    cloudFlareSsl = true;
                }
            }

            // TODO: Same thing for Pound deployments

            // Rewrite if applicable

            if (Request.Url.ToString().StartsWith ("http://") && !cloudFlareSsl && CurrentUser.Identity > 0)
                // only check client-side as many server sites de-SSL the connection before reaching the web server
            {
                if (!Request.Url.ToString().StartsWith ("http://dev.swarmops.com/") &&
                    !Request.Url.ToString().StartsWith("http://dev-opentest.swarmops.com") &&
                    !Request.Url.ToString().StartsWith("http://localhost:"))
                {
                    if (SystemSettings.RequireSsl)
                    {
                        Response.Redirect (Request.Url.ToString().Replace ("http:", "https:"));
                    }
                }
            }

            Localize();

            _cacheVersionMark = Logic.Support.Formatting.SwarmopsVersion;
            if (_cacheVersionMark.StartsWith ("Debug"))
            {
                _cacheVersionMark = DateTime.UtcNow.ToString ("yyyy-MM-dd HH:mm:ss.ffff");
            }
            _cacheVersionMark = SHA1.Hash (_cacheVersionMark).Replace(" ", "").Substring (0, 8);

            this.LabelCurrentUserName.Text = CurrentAuthority.Person.Name;
            this.LabelCurrentOrganizationName.Text = CurrentAuthority.Organization.Name;

            this.LabelActionPlaceholder1.Text = "Action shortcut 1 (TODO)";
            this.LabelActionPlaceholder2.Text = "Action shortcut 2 (TODO)";
            this.LabelNoTodoItems.Text = Resources.Global.Global_NoActionItems;

            // Set up todo items

            DashboardTodos todos = DashboardTodos.ForAuthority (CurrentAuthority);

            this.RepeaterTodoItems.DataSource = todos;
            this.RepeaterTodoItems.DataBind();

            if (todos.Count == 0)
            {
                // If no todos, hide the entire Todos box
                this.LiteralDocumentReadyHook.Text += @"$('div#divDashboardTodo').hide();";
            }

            // Set up main menu 

            SetupMenuItems();

            this.ImageCultureIndicator.Style[HtmlTextWriterStyle.MarginTop] = "-3px";
            this.ImageCultureIndicator.Style[HtmlTextWriterStyle.MarginRight] = "3px";
            this.ImageCultureIndicator.Style[HtmlTextWriterStyle.Cursor] = "pointer";

            SetupDropboxes();

            // Check for message to display

            HttpCookie dashMessage = Request.Cookies["DashboardMessage"];

            if (dashMessage != null && dashMessage.Value.Length > 0)
            {
                this.LiteralDocumentReadyHook.Text +=
                    string.Format ("alertify.alert(decodeURIComponent('{0}'));", dashMessage.Value);
                DashboardMessage.Reset();
            }
            else
            {
                this.LiteralDocumentReadyHook.Text = string.Empty;
            }

            // Enable support for RTL languages

            if (Thread.CurrentThread.CurrentCulture.TextInfo.IsRightToLeft)
            {
                this.LiteralBodyAttributes.Text = @"dir='rtl' class='rtl'";
            }

            // If we're running as an open-something identity, remove the Preferences div

            if (CurrentUser.Identity < 0)
            {
                this.LiteralDocumentReadyHook.Text += @" $('#divUserPreferences').hide();";
            }

        }

        private void SetupDropboxes()
        {
        }

        private void Localize()
        {
            this.LabelSidebarInfo.Text = Global.Sidebar_Information;
            this.LabelSidebarActions.Text = Global.Sidebar_Actions;
            this.LabelSidebarTodo.Text = Global.Sidebar_Todo;

            string cultureString = Thread.CurrentThread.CurrentCulture.ToString();
            string cultureStringLower = cultureString.ToLowerInvariant();

            if (cultureStringLower == "en-us")
            {
                cultureString = "en-US";
                cultureStringLower = "en-us";
                Thread.CurrentThread.CurrentCulture = new CultureInfo (cultureString);

                HttpCookie cookieCulture = new HttpCookie ("PreferredCulture");
                cookieCulture.Value = cultureString;
                cookieCulture.Expires = DateTime.Now.AddDays (365);
                Response.Cookies.Add (cookieCulture);
            }

            string flagName = "uk";

            if (!cultureStringLower.StartsWith ("en") && cultureString.Length > 3)
            {
                flagName = cultureStringLower.Substring (3);
            }

            if (cultureStringLower.StartsWith ("ar"))
            {
                flagName = "Arabic";
            }

            if (cultureStringLower == "af-za") // "South African Afrikaans", a special placeholder for localization code
            {
                flagName = "txl";
                InitTranslation();
            }
            else
            {
                this.LiteralCrowdinScript.Text = string.Empty;
            }

            this.ImageCultureIndicator.ImageUrl = "~/Images/Flags/" + flagName + "-24px.png";

            this.LinkLogout.Text = Global.CurrentUserInfo_Logout;
            this.LabelPreferences.Text = Global.CurrentUserInfo_Preferences;
            // this.LiteralCurrentlyLoggedIntoSwitch.Text = string.Format(Resources.Global.Master_SwitchOrganizationDialog, _currentOrganization.Name);
        }

        protected string _cacheVersionMark;  // this is just a cache buster for style sheets on new versions

        private void SetupMenuItems()
        {
            // RadMenuItemCollection menuItems = this.MainMenu.Items;
            // SetupMenuItemsRecurse(menuItems, true);
        }

        /*
        private bool SetupMenuItemsRecurse(RadMenuItemCollection menuItems, bool topLevel)
        {
            // string thisPageUrl = Request.Url.Segments[Request.Url.Segments.Length - 1].ToLower();
            bool anyItemEnabled = false;

            foreach (RadMenuItem item in menuItems)
            {
                int itemUserLevel = Convert.ToInt32(item.Attributes["UserLevel"]);
                string authorization = item.Attributes["Permission"];
                string resourceKey = item.Attributes["GlobalResourceKey"];
                string url = item.NavigateUrl;
                string dynamic = item.Attributes["Dynamic"];

                item.Visible = itemUserLevel <= 4;   // TODO: Replace with user's actual level
                bool enabled = topLevel;

                if (item.IsSeparator)
                {
                    continue;
                }

                if (dynamic == "true")
                {
                    switch (item.Attributes["Template"])
                    {
                        case "Build#":
                            item.Text = GetBuildIdentity(); // only dynamically constructed atm -- if more, switch on "template" field
                            break;
                        case "CloseLedgers":
                            int year = DateTime.Today.Year;
                            int booksClosedUntil = _currentOrganization.Parameters.FiscalBooksClosedUntilYear;

                            if (_currentOrganization.Parameters.EconomyEnabled && booksClosedUntil < year - 1)
                            {
                                item.Text = String.Format(Resources.Menu5.Menu5_Ledgers_CloseBooks, booksClosedUntil + 1);
                            }
                            else
                            {
                                enabled = false;  // maybe even invisible?
                                url = string.Empty;
                                item.Text = String.Format(Resources.Menu5.Menu5_Ledgers_CannotCloseBooks, booksClosedUntil + 1);
                            }
                            break;
                        default:
                            throw new InvalidOperationException("No case for dynamic menu item" + item.Attributes["Template"]);
                    }
                }
                else
                {
                    item.Text = GetGlobalResourceObject("Menu5", resourceKey).ToString();
                }

                if (item.Visible)
                {
                    enabled |= SetupMenuItemsRecurse(item.Items, false);
                    enabled |= !String.IsNullOrEmpty(url);
                }

                item.Enabled = enabled;
                if (enabled)
                {
                    anyItemEnabled = true;
                }
            }

            return anyItemEnabled;
        }*/


        /*
                if (Convert.ToInt32(item.Attributes["UserLevel"]) < 4)   // TODO: user's menu level
                {
                    item.Visible = false;
                }*/

        /*
                item.Enabled = true;
                if (string.IsNullOrEmpty(item.Attributes["Permission"]) == false)
                {
                    string permString = item.Attributes["Permission"].ToString();
                    PermissionSet allowedFor = new PermissionSet(permString);
                }
                else
                {
                    RadMenuItem permissionParent = item;
                    while (permissionParent.Parent is RadMenuItem)
                    {
                        if (string.IsNullOrEmpty(permissionParent.Attributes["Permission"]) == false)
                        {
                            item.Enabled = permissionParent.Enabled;
                            item.Attributes["Permission"] = permissionParent.Attributes["Permission"].ToString();
                            break;
                        }
                        permissionParent = permissionParent.Parent as RadMenuItem;
                    }
                }
                if (string.IsNullOrEmpty(item.NavigateUrl) && item.Items.Count == 0)
                {
                    item.Enabled = false;
                }*/

        /*
                string[] currentItemUrlSplit = item.NavigateUrl.ToLower().Split(new char[] { '/', '?' }, StringSplitOptions.RemoveEmptyEntries);
                if (Array.Exists<string>(currentItemUrlSplit,
                                delegate(String s) { if (s == thisPageUrl) return true; else return false; })
                    )
                {
                    if (string.IsNullOrEmpty(item.Attributes["Permission"]) == false)
                    {
                        string permString = item.Attributes["Permission"].ToString();
                        ((PageV5Base)(this.Page)).pagePermissionDefault = new PermissionSet(permString);
                    }
                }
                if (item.Items.Count > 0)
                {
                    bool enabledSubItems = SetupMenuItemsEnabling(authority, enableCache, item.Items);
                    if (enabledSubItems)
                    {
                        item.Enabled = true;
                    }
                    else
                    {
                        item.Enabled = false;
                    }
                }
                else if (string.IsNullOrEmpty(item.NavigateUrl))
                {
                    item.Enabled = false;
                }*/


        /*
        string CollectItemID(IRadMenuItemContainer item)
        {
            if (item.Owner == null)
            {
                return "";
            }
            else
            {
                return CollectItemID(item.Owner) + "_" + ((RadMenuItem)item).ID;
            }
        }*/


        protected void LinkLogout_Click (object sender, EventArgs e)
        {
            FormsAuthentication.SignOut();
            FormsAuthentication.RedirectToLoginPage();
        }

        private void InitTranslation()
        {
            string crowdinCode =
                "<!-- Crowdin JIPT Begin -->\r\n" +
                "<script type=\"text/javascript\">\r\n" +
                "  var _jipt = [];\r\n" +
                "  _jipt.push(['project', 'activizr']);\r\n" +
                "</script>\r\n" +
                "<script type=\"text/javascript\" src=\"//cdn.crowdin.com/jipt/jipt.js\"></script>\r\n" +
                "<!-- Crowdin JIPT End -->\r\n";

            this.LiteralCrowdinScript.Text = crowdinCode;
            // Page.ClientScript.RegisterStartupScript(this.GetType(), "crowdin", crowdinCode, false);
        }
    }
}