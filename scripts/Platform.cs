using Godot;

public class Platform : Spatial
{
    private Globals Globals;
    private Tween _tween;

    private MeshInstance _border;

    Tween.TransitionType _trans = Tween.TransitionType.Sine;

    private bool _start = true;

    public override void _Ready()
    {
        Globals = GetNode<Globals>("/root/Globals");
        _tween = GetNode<Tween>("Tween");
        _border = GetNode<MeshInstance>("Border");

        _tween.PlaybackProcessMode = Tween.TweenProcessMode.Physics;

        _Recolor();
    }

    public void PlayEndingAnimation()
    {
        SpatialMaterial material = (SpatialMaterial)_border.GetSurfaceMaterial(0);

        _tween.InterpolateProperty(material, "emission_energy", 1, 0, 0.25f, _trans);
        _tween.Start();
    }

    private void _Recolor()
    {
        SpatialMaterial material = Globals.GetEmissionMaterial(0);
        material.EmissionEnergy = 0;

        _border.SetSurfaceMaterial(0, material);

        _PlayAnimation(material);
    }

    private void _PlayAnimation(SpatialMaterial material)
    {
        _tween.InterpolateProperty(material, "emission_energy", 0, 1, 0.25f, _trans);
        _tween.Start();
    }

    private void _OnTweenCompleted()
    {
        if (!_start)
        {
            this.GetParent().QueueFree();
        }

        _start = false;
    }
}