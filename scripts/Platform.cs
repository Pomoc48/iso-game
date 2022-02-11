using Godot;
using System;

public class Platform : Spatial
{
    private Globals Globals;
    private Tween _tween;

    private bool _start = true;

    public override void _Ready()
    {
        Globals = GetNode<Globals>("/root/Globals");

        _tween = GetNode<Tween>("Tween");
        _tween.PlaybackProcessMode = Tween.TweenProcessMode.Physics;

        _Recolor();
    }

    private void _Recolor()
    {
        SpatialMaterial material = Globals.GetEmissionMaterial(0);
        material.EmissionEnergy = 0;

        this.GetNode<MeshInstance>("Border").SetSurfaceMaterial(0, material);

        _PlayAnimation(material);
    }

    private void _PlayAnimation(SpatialMaterial material)
    {
        Tween.TransitionType trans = Tween.TransitionType.Sine;

        _tween.InterpolateProperty(material, "emission_energy", 0, 1, 0.25f, trans);
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