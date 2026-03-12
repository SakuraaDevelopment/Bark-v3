using Bark.GUI;
using Bark.Networking;
using Bark.Patches;
using Bark.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;
using NetworkPlayer = NetPlayer;

namespace Bark.Modules.Movement;

public class ShadowFly : BarkModule
{
    private static GameObject? localWings;
    public static string DisplayName = "Shadow Fly";

    protected override void Start()
    {
        base.Start();

        if (localWings == null)
        {
            localWings = Instantiate(Plugin.AssetBundle?.LoadAsset<GameObject>("ShadowWings"), VRRig.LocalRig.transform);
            localWings.transform.localScale = Vector3.one;
        }
        
        localWings.SetActive(false);
        VRRigCachePatches.OnRigCached += OnRigCached;
    }

    protected override void OnEnable()
    {
        if (!MenuController.Instance.Built) return;
        base.OnEnable();
        localWings.SetActive(true);
    }

    protected override void Cleanup()
    {
        if (localWings)
            localWings.SetActive(false);
    }

    private void OnRigCached(NetPlayer player, VRRig rig) => rig?.gameObject?.GetComponent<NetShadWing>()?.Obliterate();
    public override string Tutorial() => "- Cool wings for a tier 3 supporter";
    public override string GetDisplayName() => DisplayName;

    private class NetShadWing : MonoBehaviour
    {
        private GameObject netWings;
        private NetworkedPlayer networkedPlayer;
        
        private void OnEnable()
        {
            networkedPlayer = gameObject.GetComponent<NetworkedPlayer>();
            netWings = Instantiate(localWings, networkedPlayer.rig.transform);
            netWings.SetActive(true);
        }
        
        private void OnDisable() => netWings.Obliterate();
        private void OnDestroy() => netWings.Obliterate();
    }
}