﻿namespace ScriptsDialog.Properties
{
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase
    {
        [global::System.Configuration.UserScopedSetting()]
        [global::System.Configuration.SettingsSerializeAs(global::System.Configuration.SettingsSerializeAs.Xml)]
        [global::System.Configuration.DefaultSettingValue("")]
        public ScriptParameters Parameters
        {
            get
            {
                return this[nameof(Parameters)] as ScriptParameters;
            }
            set
            {
                this[nameof(Parameters)] = value;
            }
        }
    }
}
