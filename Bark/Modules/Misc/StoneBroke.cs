using Bark.Gestures;
using Bark.GUI;
using Bark.Networking;
using Bark.Patches;
using Bark.Extensions;
using Photon.Pun;
using UnityEngine;
using UnityEngine.XR;

namespace Bark.Modules.Misc;

internal class StoneBroke : BarkModule
{
    public static GameObject wawa;

    public static InputTracker? inputL;
    public static InputTracker? inputR;
    private Awsomepnix LocalP;

    private void Awake()
    {
        VRRigCachePatches.OnRigCached += OnRigCached;
    }

    protected override void Start()
    {
        base.Start();
        wawa = Plugin.AssetBundle.LoadAsset<GameObject>("bs");
    }

    protected override void OnEnable()
    {
        if (!MenuController.Instance.Built) return;
        base.OnEnable();
        LocalP = GorillaTagger.Instance.offlineVRRig.AddComponent<Awsomepnix>();
    }

    public override string GetDisplayName()
    {
        return "StoneBroke :3";
    }

    public override string Tutorial()
    {
        return "MuskEnjoyer";
    }

    private void OnRigCached(NetPlayer player, VRRig rig)
    {
        if (rig?.gameObject?.GetComponent<Awsomepnix>() != null)
        {
            rig?.gameObject?.GetComponent<Awsomepnix>()?.ps.Obliterate();
            rig?.gameObject?.GetComponent<Awsomepnix>()?.Obliterate();
        }
    }

    protected override void Cleanup()
    {
        LocalP?.ps.Obliterate();
        LocalP?.Obliterate();
    }

    private class Awsomepnix : MonoBehaviour
    {
        public GameObject ps;
        private NetworkedPlayer wa;

        private void Start()
        {
            ps = Instantiate(wawa, gameObject.transform);
            wa = gameObject.GetComponent<NetworkedPlayer>();

            wa.OnGripPressed += Boom;
        }

        private void OnDestroy()
        {
            wa.OnGripPressed -= Boom;
        }

        private void LocalBoom(InputTracker tracker)
        {
            ps.GetComponentInChildren<AudioSource>().Play();
        }

        private void Boom(NetworkedPlayer player, bool arg2)
        {
            ps.GetComponentInChildren<AudioSource>().Play();
        }
    }
}