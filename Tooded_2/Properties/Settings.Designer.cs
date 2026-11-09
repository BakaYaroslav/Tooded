// Settings.settings is the source of this user-scoped setting.
namespace Tooded_2.Properties;

[global::System.Runtime.CompilerServices.CompilerGenerated]
internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase
{
    private static readonly Settings instance = (Settings)Synchronized(new Settings());
    public static Settings Default => instance;
    [global::System.Configuration.UserScopedSetting]
    [global::System.Configuration.DefaultSettingValue("et-EE")]
    public string UserLanguage
    {
        get => (string)this[nameof(UserLanguage)];
        set => this[nameof(UserLanguage)] = value;
    }
}
