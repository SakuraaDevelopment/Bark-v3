using Bark.GUI;
using GorillaNetworking;

namespace Bark.Modules.Misc;

public class Lobby : BarkModule
{
    public static readonly string DisplayName = "Bark Code";
    private int timesPressed;


    protected override void Start()
    {
        base.Start();
        timesPressed = 0;
    }

    protected override void OnEnable()
    {
        if (!MenuController.Instance.Built) return;
        base.OnEnable();
        timesPressed++;
        if (timesPressed >= 3)
        {
            PhotonNetworkController.Instance.AttemptToJoinSpecificRoom("BARK", JoinType.Solo);
            timesPressed = 0;
            return;
        }

        enabled = false;
    }

    public override string GetDisplayName()
    {
        return DisplayName;
    }

    public override string Tutorial()
    {
        return "Join Bark Code after Pressing 3 times";
    }

    protected override void Cleanup()
    {
    }
}