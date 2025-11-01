using HarmonyLib;

namespace ClaimNewNotification
{
    /// <summary>
    /// Boots the Claim New Notification mod via AzeLib's load hooks.
    /// </summary>
    internal static class ClaimNewNotificationBootstrap
    {
        /// <summary>
        /// Configures non-Harmony services once runtime wiring is ready.
        /// </summary>
        [AzeLib.Attributes.OnLoad]
        public static void OnLoad()
        {
            // TODO: Wire supply-closet claim tracking and event subscriptions before enabling runtime patches.
        }

        /// <summary>
        /// Wires Harmony patches when the implementation lands.
        /// </summary>
        /// <param name="harmony">Harmony instance provided by AzeLib's bootstrapper.</param>
        [AzeLib.Attributes.OnLoad]
        public static void OnLoad(Harmony harmony)
        {
            // TODO: Re-run tools/oni_eventscan.py after the event hooks are implemented to refresh findings.json.
        }
    }
}
