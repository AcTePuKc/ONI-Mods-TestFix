using AzeLib;
using STRINGS;
using StringsUI = global::STRINGS.UI;

namespace SuppressNotifications
{
    public class MYSTRINGS : AStrings<MYSTRINGS>
    {
        public static LocString STATUS_LABEL = "<b>Status Items:</b>";
        public static LocString NOTIFICATION_LABEL = "<b>Notifications:</b>";

        public class SUPPRESSBUTTON
        {
            public static LocString NAME = "Suppress Current";
            public static LocString TOOLTIP = StringsUI.FormatAsKeyWord("Suppress") + " the following items.";
        }

        public class CLEARBUTTON
        {
            public static LocString NAME = "Clear Suppressed";
            public static LocString TOOLTIP = StringsUI.FormatAsKeyWord("Stop suppressing") + " the following items.";
        }

        public class BUILDINGS
        {
            public static LocString DAMAGE_BAR = "Damage Bar";
        }
    }
}
