namespace Bark.Modules.Multiplayer;

internal class PateronSupporter : BarkModule
{
    protected override void Start()
    {
        base.Start();
    }

    public override string GetDisplayName()
    {
        return "Supporter";
    }

    public override string Tutorial()
    {
        return "Thanks you so much for showing your support";
    }

    protected override void Cleanup()
    {
    }
}