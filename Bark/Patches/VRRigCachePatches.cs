using System;
using System.Collections.Generic;

namespace Bark.Patches;

public class VRRigCachePatches
{
    public static Action<NetPlayer, VRRig> OnRigCached;

    private static readonly HashSet<VRRig> _knownRigs = new HashSet<VRRig>();
    private static readonly List<VRRig> _tempRigList = new List<VRRig>();
    private static bool _subscribed;

    public static void Subscribe()
    {
        if (_subscribed) return;
        VRRigCache.OnActiveRigsChanged += OnActiveRigsChanged;
        _subscribed = true;
    }

    public static void Unsubscribe()
    {
        if (!_subscribed) return;
        VRRigCache.OnActiveRigsChanged -= OnActiveRigsChanged;
        _subscribed = false;
        _knownRigs.Clear();
    }

    private static void OnActiveRigsChanged()
    {
        if (!VRRigCache.isInitialized) return;

        _tempRigList.Clear();
        VRRigCache.Instance.GetActiveRigs(_tempRigList);

        var currentRigs = new HashSet<VRRig>();
        foreach (var rig in _tempRigList)
        {
            if (rig == null) continue;
            currentRigs.Add(rig);

            if (!_knownRigs.Contains(rig))
            {
                var player = rig.OwningNetPlayer;
                if (player != null)
                {
                    OnRigCached?.Invoke(player, rig);
                }
            }
        }

        _knownRigs.Clear();
        foreach (var r in currentRigs)
            _knownRigs.Add(r);
    }
}
