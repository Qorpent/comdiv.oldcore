﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.225
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Comdiv.MVC.Resources {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
    public sealed partial class RequestParameterNames : global::System.Configuration.ApplicationSettingsBase {
        
        private static RequestParameterNames defaultInstance = ((RequestParameterNames)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new RequestParameterNames())));
        
        public static RequestParameterNames Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("requestKey")]
        public string Report_RequestKey {
            get {
                return ((string)(this["Report_RequestKey"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("preserveRequestInSession")]
        public string Report_KeepRequest {
            get {
                return ((string)(this["Report_KeepRequest"]));
            }
        }
    }
}
