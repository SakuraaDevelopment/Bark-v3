using System.Collections.Generic;
using System.Linq;
using Bark.Modules;
using GorillaLocomotion;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace Bark.Extensions;

public static class PlayerExtensions
{
    public static void AddForce(this GTPlayer self, Vector3 v)
    {
        self.GetComponent<Rigidbody>().velocity += v;
    }

    public static void SetVelocity(this GTPlayer self, Vector3 v)
    {
        self.GetComponent<Rigidbody>().velocity = v;
    }

    public static PhotonView PhotonView(this VRRig rig)
    {
        // Access private photonView via reflection
        return Traverse.Create(rig).Field("photonView").GetValue<PhotonView>();
    }

    public static bool HasProperty(this VRRig rig, string key)
    {
        return rig?.OwningNetPlayer?.HasProperty(key) ?? false;
    }

    public static bool ModuleEnabled(this VRRig rig, string mod)
    {
        return rig?.OwningNetPlayer?.ModuleEnabled(mod) ?? false;
    }

    public static T GetProperty<T>(this NetPlayer? player, string key)
    {
        return (T)player?.GetPlayerRef().CustomProperties[key];
    }

    public static bool HasProperty(this NetPlayer player, string key)
    {
        return player?.GetPlayerRef().CustomProperties.ContainsKey(key) ?? false;
    }

    public static bool ModuleEnabled(this NetPlayer player, string mod)
    {
        if (!player.HasProperty(BarkModule.enabledModulesKey)) return false;

        var enabledMods = player.GetProperty<Dictionary<string, bool>>(BarkModule.enabledModulesKey);
        return enabledMods.TryGetValue(mod, out var enabled) && enabled;
    }

    public static VRRig? Rig(this NetPlayer? player)
    {
        return GorillaParent.instance.vrrigs.FirstOrDefault(rig => rig.OwningNetPlayer == player);
    }
}