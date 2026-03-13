using System.Collections.Generic;

namespace Bark.Helpers;

public static class RigHelper
{
    private static readonly List<VRRig> _rigs = new List<VRRig>();

    public static List<VRRig> GetActiveRigs()
    {
        _rigs.Clear();
        if (VRRigCache.isInitialized)
            VRRigCache.Instance.GetActiveRigs(_rigs);
        return _rigs;
    }
}
