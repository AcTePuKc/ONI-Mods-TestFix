using AzeLib.Attributes;

namespace BetterLogicOverlay.LogicSettingDisplay
{
    class LogicBroadcasterSetting : LogicLabelSetting
    {
        [MyCmpGet] private LogicBroadcaster logicBroadcaster;

        public override string GetSetting() => GetString(logicBroadcaster);

        protected string GetString(KMonoBehaviour l) => l.GetProperName();

        public class Receiver : LogicBroadcasterSetting
        {
            [MyCmpGet] private LogicBroadcastReceiver logicBroadcastReceiver;

            public override string GetSetting() => GetString(logicBroadcastReceiver.GetChannel());
        }
    }
}
