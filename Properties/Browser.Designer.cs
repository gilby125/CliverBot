﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Cliver.Bot.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
    public sealed partial class Browser : global::System.Configuration.ApplicationSettingsBase {
        
        private static Browser defaultInstance = ((Browser)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Browser())));
        
        public static Browser Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("60")]
        public int PageCompletedTimeoutInSeconds {
            get {
                return ((int)(this["PageCompletedTimeoutInSeconds"]));
            }
            set {
                this["PageCompletedTimeoutInSeconds"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool CloseWebBrowserDialogsAutomatically {
            get {
                return ((bool)(this["CloseWebBrowserDialogsAutomatically"]));
            }
            set {
                this["CloseWebBrowserDialogsAutomatically"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool SuppressScriptErrors {
            get {
                return ((bool)(this["SuppressScriptErrors"]));
            }
            set {
                this["SuppressScriptErrors"] = value;
            }
        }
    }
}
