using STRINGS;

namespace PeterHan.PLib.Core;

public static class PLibStrings
{
	public static LocString BUTTON_MANUAL = LocString.op_Implicit("MANUAL CONFIG");

	public static LocString BUTTON_RESET = LocString.op_Implicit("RESET TO DEFAULT");

	public static LocString BUTTON_OK = OPTIONS_SCREEN.BACK;

	public static LocString BUTTON_OPTIONS = MAINMENU.OPTIONS;

	public static LocString DIALOG_TITLE = LocString.op_Implicit("Options for {0}");

	public static LocString KEY_HOME = LocString.op_Implicit("Home");

	public static LocString KEY_END = LocString.op_Implicit("End");

	public static LocString KEY_DELETE = LocString.op_Implicit("Delete");

	public static LocString KEY_PAGEUP = LocString.op_Implicit("Page Up");

	public static LocString KEY_PAGEDOWN = LocString.op_Implicit("Page Down");

	public static LocString KEY_SYSRQ = LocString.op_Implicit("SysRq");

	public static LocString KEY_PRTSCREEN = LocString.op_Implicit("Print Screen");

	public static LocString KEY_PAUSE = LocString.op_Implicit("Pause");

	public static LocString KEY_ARROWLEFT = LocString.op_Implicit("Left Arrow");

	public static LocString KEY_ARROWUP = LocString.op_Implicit("Up Arrow");

	public static LocString KEY_ARROWRIGHT = LocString.op_Implicit("Right Arrow");

	public static LocString KEY_ARROWDOWN = LocString.op_Implicit("Down Arrow");

	public static LocString KEY_CATEGORY_TITLE = LocString.op_Implicit("Mods");

	public static LocString LABEL_B = LocString.op_Implicit("B");

	public static LocString LABEL_G = LocString.op_Implicit("G");

	public static LocString LABEL_R = LocString.op_Implicit("R");

	public static LocString MOD_ASSEMBLY_VERSION = LocString.op_Implicit("Assembly Version: {0}");

	public static LocString MOD_HOMEPAGE = LocString.op_Implicit("Mod Homepage");

	public static LocString MOD_VERSION = LocString.op_Implicit("Mod Version: {0}");

	public static LocString RESTART_CANCEL = RESTART.CANCEL;

	public static LocString RESTART_OK = RESTART.OK;

	public static LocString MAINMENU_UPDATE = LocString.op_Implicit("\n\n<color=#FFCC00>{0:D} mods may be out of date</color>");

	public static LocString MAINMENU_UPDATE_1 = LocString.op_Implicit("\n\n<color=#FFCC00>1 mod may be out of date</color>");

	public static LocString OUTDATED_TOOLTIP = LocString.op_Implicit("This mod is out of date!\nNew version: <b>{0}</b>\n\nUpdate local mods manually, or use <b>Mod Updater</b> to force update Steam mods");

	public static LocString OUTDATED_WARNING = LocString.op_Implicit("<b><style=\"logic_off\">Outdated!</style></b>");

	public static LocString RESTART_REQUIRED = LocString.op_Implicit("Oxygen Not Included must be restarted for these options to take effect.");

	public static LocString TOOLTIP_BLUE = LocString.op_Implicit("Blue");

	public static LocString TOOLTIP_CANCEL = LocString.op_Implicit("Discard changes.");

	public static LocString TOOLTIP_GREEN = LocString.op_Implicit("Green");

	public static LocString TOOLTIP_HOMEPAGE = LocString.op_Implicit("Visit the mod's website.");

	public static LocString TOOLTIP_HUE = LocString.op_Implicit("Hue");

	public static LocString TOOLTIP_MANUAL = LocString.op_Implicit("Opens the folder containing the full mod configuration.");

	public static LocString TOOLTIP_NEXT = LocString.op_Implicit("Next");

	public static LocString TOOLTIP_OK = LocString.op_Implicit("Save these options. Some mods may require a restart for the options to take effect.");

	public static LocString TOOLTIP_PREVIOUS = LocString.op_Implicit("Previous");

	public static LocString TOOLTIP_RED = LocString.op_Implicit("Red");

	public static LocString TOOLTIP_RESET = LocString.op_Implicit("Resets the mod configuration to default values.");

	public static LocString TOOLTIP_SATURATION = LocString.op_Implicit("Saturation");

	public static LocString TOOLTIP_TOGGLE = LocString.op_Implicit("Show or hide this options category");

	public static LocString TOOLTIP_VALUE = LocString.op_Implicit("Value");

	public static LocString TOOLTIP_VERSION = LocString.op_Implicit("The currently installed version of this mod.\n\nCompare this version with the mod's Release Notes to see if it is outdated.");
}
