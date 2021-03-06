﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using Swarmops.Common.Enums;
using Swarmops.Logic.Security;
using Swarmops.Logic.Structure;
using Swarmops.Logic.Support;
using Swarmops.Logic.Support.LogEntries;
using Swarmops.Logic.Swarm;
using Swarmops.Common.Exceptions;
using Swarmops.Frontend;
using Swarmops.Logic.Cache;


namespace Swarmops.Frontend.Automation
{
    public partial class SwarmFunctions : DataV5Base
    {
        protected void Page_Load (object sender, EventArgs e)
        {

        }

        [WebMethod]
        public static AjaxCallResult AssignPosition (int personId, int positionId, int durationMonths, int geographyId)
        {
            AuthenticationData authData = GetAuthenticationDataAndCulture();
            Position position = Position.FromIdentity (positionId);
            Person person = Person.FromIdentity (personId);
            Geography geography = (geographyId == 0 ? null : Geography.FromIdentity (geographyId));

            if (position.PositionLevel == PositionLevel.Geography ||
                position.PositionLevel == PositionLevel.GeographyDefault)
            {
                position.AssignGeography (geography);
            }

            if ((position.OrganizationId > 0 && authData.CurrentOrganization.Identity != position.OrganizationId) || person.Identity < 0)
            {
                throw new UnauthorizedAccessException();
            }
            if (position.PositionLevel == PositionLevel.SystemWide && !authData.Authority.HasAccess (new Access (AccessAspect.Administration)))
            {
                // Authority check for systemwide
                throw new UnauthorizedAccessException();
            }
            if ((position.GeographyId == Geography.RootIdentity || position.GeographyId == 0) &&
                !authData.Authority.HasAccess (new Access (authData.CurrentOrganization, AccessAspect.Administration)))
            {
                // Authority check for org-global
                throw new UnauthorizedAccessException();
            }
            if (
                !authData.Authority.HasAccess (new Access (authData.CurrentOrganization, geography,
                    AccessAspect.Administration)))
            {
                // Authority check for org/geo combo
                throw new UnauthorizedAccessException();
            }

            if (position.MaxCount > 0 && position.Assignments.Count >= position.MaxCount)
            {
                return new AjaxCallResult
                {
                    Success = false,
                    DisplayMessage = Resources.Controls.Swarm.Positions_NoMorePeopleOnPosition
                };
            }

            // Deliberate: no requirement for membership (or equivalent) in order to be assigned to position.
            // Find the current user position used to assign.

            PositionAssignments currentUserAssignments = authData.CurrentUser.PositionAssignments;

            // Get the one this user is currently using to assign - it's either a system level position,
            // one with a parent organization (TODO), or one with this organization

            Position activePosition = null;

            foreach (PositionAssignment currentUserAssignment in currentUserAssignments)
            {
                if (currentUserAssignment.OrganizationId == 0 && currentUserAssignment.Active)
                {
                    activePosition = currentUserAssignment.Position;
                    break; // a system-level active position has priority over org-level
                }
                if (currentUserAssignment.OrganizationId == authData.CurrentOrganization.Identity &&
                    currentUserAssignment.Active)
                {
                    activePosition = currentUserAssignment.Position;
                }
            }

            if (activePosition == null)
            {
                return new AjaxCallResult
                {
                    Success = false,
                    DisplayMessage = "Error: No authority to assign a position"
                };
            }

            DateTime? expiresUtc = null;

            if (durationMonths > 0)
            {
                expiresUtc = DateTime.UtcNow.AddMonths (durationMonths);
            }

            try
            {
                PositionAssignment.Create (position, geography, person, authData.CurrentUser, activePosition,
                    expiresUtc, string.Empty);
            }
            catch (DatabaseConcurrencyException)
            {
                return new AjaxCallResult {Success = false, DisplayMessage = Resources.Global.Error_DatabaseConcurrency};
            }

            return new AjaxCallResult {Success = true};
        }


        [WebMethod]
        public static AjaxCallResult TerminatePositionAssignment (int assignmentId)
        {
            AuthenticationData authData = GetAuthenticationDataAndCulture();

            PositionAssignment assignment = PositionAssignment.FromIdentity (assignmentId);

            if (assignment.OrganizationId == 0)
            {
                if (!authData.Authority.HasAccess (new Access (AccessAspect.Administration))) // System-wide admin
                {
                    throw new UnauthorizedAccessException();
                }
            }
            else // Org-specific assignment
            {
                if (assignment.GeographyId == 0)
                {
                    if (!authData.Authority.HasAccess (new Access(authData.CurrentOrganization, AccessAspect.Administration)))
                    {
                        throw new UnauthorizedAccessException();
                    }
                }
                else // Org- and geo-specific assignment
                {
                    if (
                        !authData.Authority.HasAccess (new Access (authData.CurrentOrganization,
                            assignment.Position.Geography, AccessAspect.Administration)))
                    {
                        throw new UnauthorizedAccessException();
                    }
                }
            }

            // Ok, go ahead and terminate

            try
            {
                assignment.Terminate(authData.CurrentUser, authData.CurrentUser.GetPrimaryPosition(authData.CurrentOrganization), string.Empty);
            }
            catch (DatabaseConcurrencyException)
            {
                return new AjaxCallResult {Success = false, DisplayMessage = Resources.Global.Error_DatabaseConcurrency};
            }

            return new AjaxCallResult {Success = true};
        }

        [WebMethod]
        static public AssignmentData GetAssignmentData (int assignmentId)
        {
            AuthenticationData authData = GetAuthenticationDataAndCulture();
            PositionAssignment assignment = PositionAssignment.FromIdentity (assignmentId);

            if (authData.Authority.CanAccess (assignment, AccessType.Read))
            {
                return new AssignmentData
                {
                    Success = true,
                    AssignedPersonCanonical = assignment.Person.Canonical,
                    AssignedPersonId = assignment.PersonId,
                    PositionAssignmentId = assignment.Identity,
                    PositionId = assignment.PositionId,
                    PositionLocalized = assignment.Position.Localized()
                };
            }
            else
            {
                return new AssignmentData {Success = false};
            }

        }

        public class AssignmentData : AjaxCallResult
        {
            public int PositionAssignmentId { get; set; }
            public int PositionId { get; set; }
            public string PositionLocalized { get; set; }
            public int AssignedPersonId { get; set; }
            public string AssignedPersonCanonical { get; set; }
        }

        public class AvatarData : AjaxCallResult
        {
            public int PersonId { get; set; }
            public string Canonical { get; set; }
            public string Avatar16Url { get; set; }
            public string Avatar24Url { get; set; }
        }

        [WebMethod]
        public static AvatarData GetPersonAvatar (int personId)
        {
            AuthenticationData authData = GetAuthenticationDataAndCulture();

            Person person = Person.FromIdentity (personId);
            if (!authData.Authority.CanSeePerson (person))
            {
                throw new ArgumentException(); // can't see, for whatever reason
            }

            return new AvatarData
            {
                PersonId = personId,
                Success = true,
                Canonical = person.Canonical,
                Avatar16Url = person.GetSecureAvatarLink (16),
                Avatar24Url = person.GetSecureAvatarLink (24)
            };
        }

        [WebMethod]
        public static AjaxCallResult GetGeographyName (int geographyId)
        {
            // This is not sensitive, so no access control, just culture setting

            GetAuthenticationDataAndCulture();
            return new AjaxCallResult {Success = true, DisplayMessage = Geography.FromIdentity (geographyId).Name};
        }

        [Serializable]
        public class PersonEditorData : AjaxCallResult
        {
            // Personal Details tab

            public string Name { get; set; }
            public string Mail { get; set; }
            public string Phone { get; set; }
            public string TwitterId { get; set; }

            // Security tab

            public bool TwoFactorActive { get; set; }

            // Accounts tab

            // (no data)

            // other tabs - fill in as we go
        }

        [WebMethod]
        public static PersonEditorData GetPersonEditorData (int personId)
        {
            AuthenticationData authData = GetAuthenticationDataAndCulture();
            #pragma warning disable 219
            bool self = false;
            #pragma warning restore 219

            if (personId == 0) // request self record
            {
                self = true; // may make use of this later
                personId = authData.CurrentUser.Identity;
            }

            Person person = Person.FromIdentity (personId);

            if (!authData.Authority.CanSeePerson (person) && !self)
            {
                throw new ArgumentException(); // can't see the requested person, for whatever reason
            }

            return new PersonEditorData
            {
                Success = true,
                Name = person.Name,
                Mail = person.Mail,
                Phone = person.Phone,
                TwitterId = person.TwitterId,
                TwoFactorActive = !string.IsNullOrEmpty(person.BitIdAddress)
            };
        }

        [WebMethod]
        public static AjaxInputCallResult SetPersonEditorData(int personId, string field, string newValue)
        {
            if (newValue == null || field == null)
            {
                throw new ArgumentNullException();
            }

            AuthenticationData authData = GetAuthenticationDataAndCulture();
            bool self = false;

            // Are we modifying ourselves?

            if (personId == 0) // request self record
            {
                self = true; // may make use of this later
                personId = authData.CurrentUser.Identity;
            }

            // Preliminary input validation

            if (string.IsNullOrEmpty (newValue))
            {
                if (field != "TwitterId") // These fields may be set to empty; default is disallow
                {
                    return new AjaxInputCallResult
                    {
                        Success = false,
                        ObjectIdentity = personId,
                        DisplayMessage = Resources.Global.Global_FieldCannotBeEmpty,
                        FailReason = AjaxInputCallResult.ErrorInvalidFormat,
                        NewValue = GetPersonValue (personId, field)
                    };
                }
            }

            // Verify authority to see and change personal data

            Person affectedPerson = Person.FromIdentity (personId);

            if (!self)
            {
                if (!authData.Authority.CanSeePerson (affectedPerson) ||
                    !authData.Authority.HasAccess (new Access (authData.CurrentOrganization, affectedPerson.Geography,
                        AccessAspect.PersonalData)))
                {
                    throw new UnauthorizedAccessException();
                }
            }

            string oldValue;
            string displayMessage = string.Empty;
            while (newValue.Contains ("  "))
            {
                newValue = newValue.Trim().Replace ("  ", " "); // double, triple, quadruple spaces reduced to one
            }

            switch (field)
            {
                case "Name":
                    oldValue = affectedPerson.Name;
                    affectedPerson.Name = newValue;
                    break;
                case "Mail":
                    oldValue = affectedPerson.Mail;
                    affectedPerson.Mail = newValue;
                    break;
                case "Phone":
                    oldValue = affectedPerson.Phone;
                    affectedPerson.Phone = newValue;
                    if (!Regex.IsMatch (newValue, @"^[0-9 \(\)\-\+]+$"))
                    {
                        // using characters not typically seen in a phone number? Warn
                        displayMessage = Resources.Global.Master_EditPersonWarning_Phone;
                    }
                    break;
                case "TwitterId":
                    if (newValue.StartsWith ("@"))
                    {
                        newValue = newValue.Substring (1);
                    }
                    oldValue = affectedPerson.TwitterId;
                    affectedPerson.TwitterId = newValue;
                    break;
                default:
                    throw new ArgumentException("Unrecognized field in /Automation/SwarmFunctions.SetPersonEditorData");
            }

            SwarmopsLogEntry logEntry = SwarmopsLog.CreateEntry (affectedPerson, new PersonalDataChangedLogEntry
            {
                ActingPersonId = authData.CurrentUser.PersonId,
                AffectedPersonId = affectedPerson.PersonId,
                Field = field,
                IpAddress = SupportFunctions.GetMostLikelyRemoteIPAddress(),
                OldValue = oldValue,
                NewValue = newValue
            });

            if (!self)
            {
                logEntry.CreateAffectedObject (authData.CurrentUser);
            }

            return new AjaxInputCallResult
            {
                ObjectIdentity = personId,
                Success = true,
                NewValue = newValue,
                DisplayMessage = displayMessage
            };
        }

        private static string GetPersonValue (int personId, string field)
        {
            Person person = Person.FromIdentity (personId);
            switch (field)
            {
                case "Name":
                    return person.Name;
                case "Mail":
                    return person.Mail;
                case "Phone":
                    return person.Phone;
                case "TwitterId":
                    return person.TwitterId;
                default:
                    throw new NotImplementedException();
            }
        }

        [WebMethod]
        public static bool ValidatePassword(int personId, string password)
        {
            AuthenticationData authData = GetAuthenticationDataAndCulture();

            // We can only verify password for personId zero (the self)

            if (personId != 0 && personId != authData.CurrentUser.Identity)
            {
                return false;
            }

            return authData.CurrentUser.ValidatePassword(password);
        }

        [WebMethod]
        public static AjaxCallResult ChangePassword(int personId, string oldPassword, string newPassword)
        {
            if (!ValidatePassword(personId, oldPassword))
            {
                // oldPassword field isn't correct

                // TODO: LOG THIS, THIS SHOULD NOT HAPPEN (UI FILTERING THIS CASE)

                // TODO: ALSO NOTIFY SECURITY OFFICERS

                // TODO: THROTTLE AND BLACKLIST?

                return new AjaxCallResult
                {
                    Success = false,
                    DisplayMessage = Resources.Global.Master_EditPersonNewPassword_CannotChange_SecurityError
                };
            }

            AuthenticationData authData = GetAuthenticationDataAndCulture();

            if (authData.CurrentUser.Identity != personId && personId != 0)
            {
                // TODO: Same thing with this case

                return new AjaxCallResult
                {
                    Success = false,
                    DisplayMessage = Resources.Global.Master_EditPersonNewPassword_CannotChange_SecurityError
                };
            }

            if (newPassword.Length < 6) // TODO: Make per-installation and per-organization policy for this
            {
                return new AjaxCallResult
                {
                    Success = false,
                    DisplayMessage = Resources.Global.Master_EditPersonNewPassword_CannotChange_TooWeak
                };
            }

            authData.CurrentUser.SetPassword(newPassword);
            // TODO: LOG
            // TODO: NOTIFY
            authData.CurrentUser.Quarantines.Withdrawal.QuarantineFor(new TimeSpan(2, 0, 0, 0)); // Quarantine for two days

            string displayMessage = Resources.Global.Master_EditPersonNewPassword_Changed;

            if (string.IsNullOrEmpty(authData.CurrentUser.BitIdAddress))
            {
                displayMessage += @"<br/><br/>" + Resources.Global.Master_EditPerson_PleaseEnable2FA;
            }

            return new AjaxCallResult
            {
                Success = true,
                DisplayMessage = displayMessage
            };
        }

        [WebMethod]
        public static AjaxCallResult TestBitIdRegister()
        {
            AuthenticationData authData = GetAuthenticationDataAndCulture();

            if (!string.IsNullOrEmpty(authData.CurrentUser.BitIdAddress))
            {
                return new AjaxCallResult
                {
                    Success = true,
                    DisplayMessage = Resources.Global.Master_BitIdRegistered
                };
            }
            else
            {
                return new AjaxCallResult
                {
                    Success = false
                };
            }
        }

        [WebMethod]
        public static AjaxCallResult RemoveBitId(int personId, string password)
        {
            AuthenticationData authData = GetAuthenticationDataAndCulture();

            if (personId == 0) // self
            {
                personId = authData.CurrentUser.Identity;

                if (!ValidatePassword(personId, password))
                {
                    // oldPassword field isn't correct

                    // TODO: LOG THIS, THIS SHOULD NOT HAPPEN (UI FILTERING THIS CASE)

                    // TODO: ALSO NOTIFY SECURITY OFFICERS

                    // TODO: THROTTLE AND BLACKLIST?

                    return new AjaxCallResult
                    {
                        Success = false,
                        DisplayMessage = Resources.Global.Master_EditPersonNewPassword_CannotChange_SecurityError
                    };
                }

                authData.CurrentUser.BitIdAddress = string.Empty;

                return new AjaxCallResult {Success = true};
            }
            else
            {
                // Validate rights to remove BitId
                // Log, notify, and chainnotify

                throw new NotImplementedException();
            }


        }


        [WebMethod]
        public static AjaxCallResult TerminateImpersonation()
        {
            AuthenticationData authData = GetAuthenticationDataAndCulture();

            if (!authData.Authority.ImpersonationActive)
            {
                return new AjaxCallResult {Success = false}; // no impersonation active. Race condition?
            }

            int realUserPersonId = authData.Authority.Impersonation.ImpersonatedByPersonId;
            Person impersonator = Person.FromIdentity(realUserPersonId);

            // Terminate impersonation and set new authority cookie from the impersonator data.
            // VERY SECURITY SENSITIVE: The identity as impersonator will be the new user.

            // TODO: LOG LOG LOG LOG

            SwarmopsLogEntry logEntry = SwarmopsLog.CreateEntry(authData.CurrentUser,
                new ImpersonationLogEntry
                {
                    ImpersonatorPersonId = impersonator.Identity,
                    Started = false
                });
            logEntry.CreateAffectedObject(impersonator); // link impersonator to log entry for searchability

            DateTime utcNow = DateTime.UtcNow;

            Authority authority =
                Authority.FromLogin(impersonator, authData.CurrentOrganization);
            FormsAuthentication.SetAuthCookie(authority.ToEncryptedXml(), false);
            HttpContext.Current.Response.AppendCookie(new HttpCookie("DashboardMessage", CommonV5.JavascriptEscape(String.Format(Resources.Pages.Admin.CommenceImpersonation_Ended, utcNow))));

            // returning Success will force a reload, resetting dashboard to original user

            return new AjaxCallResult {Success = true};
        }

    }
}