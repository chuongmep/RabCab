using RabCab.Utilities.Engine.Enumerators;

namespace RabCab.Utilities.Settings
{
    public static class SettingsUser
    {
        public static Enums.RoundTolerance UserTol { set; get; } = Enums.RoundTolerance.ThreeDecimals;
    }
}