using HarmonyLib;
using KMod;

namespace ClaimNewNotification
{
    /// <summary>
    /// Entry point for the Claim New Notification mod. Wires Harmony patches and runtime services once implementation lands.
    /// </summary>
    public sealed class ClaimNewNotificationMod : UserMod2
    {
        /// <inheritdoc />
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            // TODO: Wire supply-closet claim tracking and event subscriptions before enabling runtime patches.
            // TODO: Re-run tools/oni_eventscan.py after the event hooks are implemented to refresh findings.json.
        }
    }
}
