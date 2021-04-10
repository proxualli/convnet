using dnncore;
using System.Collections.ObjectModel;

namespace Convnet.Properties
{
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase
    {
        [global::System.Configuration.UserScopedSetting()]
        [global::System.Configuration.SettingsSerializeAs(global::System.Configuration.SettingsSerializeAs.Binary)]
        [global::System.Configuration.DefaultSettingValue("")]
        public ObservableCollection<DNNTrainingResult> TrainingLog
        {
            get
            {
                return this[nameof(TrainingLog)] as ObservableCollection<DNNTrainingResult>;
            }
            set
            {
                this[nameof(TrainingLog)] = value;
            }
        }

        [global::System.Configuration.UserScopedSetting()]
        [global::System.Configuration.SettingsSerializeAs(global::System.Configuration.SettingsSerializeAs.Binary)]
        [global::System.Configuration.DefaultSettingValue("")]
        public DNNTrainingRate TrainRate
        {
            get
            {
                return this[nameof(TrainRate)] as DNNTrainingRate;
            }
            set
            {
                this[nameof(TrainRate)] = value;
            }
        }

        [global::System.Configuration.UserScopedSetting()]
        [global::System.Configuration.SettingsSerializeAs(global::System.Configuration.SettingsSerializeAs.Binary)]
        [global::System.Configuration.DefaultSettingValue("")]
        public DNNTrainingRate TestRate
        {
            get
            {
                return this[nameof(TestRate)] as DNNTrainingRate;
            }
            set
            {
                this[nameof(TestRate)] = value;
            }
        }
    }
}
