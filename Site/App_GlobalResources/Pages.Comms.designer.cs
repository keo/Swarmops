//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Resources.Pages {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option or rebuild the Visual Studio project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Web.Application.StronglyTypedResourceProxyBuilder", "11.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Comms {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Comms() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Resources.Pages.Comms", global::System.Reflection.Assembly.Load("App_GlobalResources"));
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Who are located in.
        /// </summary>
        internal static string SendMassMessage_Geography {
            get {
                return ResourceManager.GetString("SendMassMessage_Geography", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Your Message.
        /// </summary>
        internal static string SendMassMessage_HeaderMessage {
            get {
                return ResourceManager.GetString("SendMassMessage_HeaderMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Send a message to a group of people in the organization. You can send the message by mail (good for newsletters and general information) or to their phones (very intrusive, and also costs a bit)..
        /// </summary>
        internal static string SendMassMessage_Info {
            get {
                return ResourceManager.GetString("SendMassMessage_Info", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This message would have been sent to the recipients. For the time being, this is a mock-up interface..
        /// </summary>
        internal static string SendMassMessage_MockMessageResult {
            get {
                return ResourceManager.GetString("SendMassMessage_MockMessageResult", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No Recipients|One Recipient|{0:N0} Recipients.
        /// </summary>
        internal static string SendMassMessage_RecipientCount {
            get {
                return ResourceManager.GetString("SendMassMessage_RecipientCount", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Send a mass message to all.
        /// </summary>
        internal static string SendMassMessage_RecipientType {
            get {
                return ResourceManager.GetString("SendMassMessage_RecipientType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Send.
        /// </summary>
        internal static string SendMassMessage_SendMessage {
            get {
                return ResourceManager.GetString("SendMassMessage_SendMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Your message is now being distributed to recipients. You will be notified when transmission is complete..
        /// </summary>
        internal static string SendMassMessage_SendMessageResult {
            get {
                return ResourceManager.GetString("SendMassMessage_SendMessageResult", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Test.
        /// </summary>
        internal static string SendMassMessage_TestMessage {
            get {
                return ResourceManager.GetString("SendMassMessage_TestMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The message has been sent to you, so you can see what it looks like before going live..
        /// </summary>
        internal static string SendMassMessage_TestMessageResult {
            get {
                return ResourceManager.GetString("SendMassMessage_TestMessageResult", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Send Mass Message.
        /// </summary>
        internal static string SendMassMessage_Title {
            get {
                return ResourceManager.GetString("SendMassMessage_Title", resourceCulture);
            }
        }
    }
}