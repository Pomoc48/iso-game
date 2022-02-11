using Godot;

public class Platform : Spatial
{
    private Globals Globals;
    private Tween _tween;
    private MeshInstance _border;

    Tween.TransitionType _trans = Tween.TransitionType.Sine;

    private bool _end = false;
    private float _desaturateValue = 0.1f;

    public override void _Ready()
    {
        Globals = GetNode<Globals>("/root/Globals");
        _tween = GetNode<Tween>("Tween");
        _border = GetNode<MeshInstance>("Border");

        _Recolor();
    }

    public void PlayFadeOutAnimation()
    {
        _end = true;
        SpatialMaterial material = (SpatialMaterial)_border.GetSurfaceMaterial(0);

        _tween.StopAll();
        _PlayTweenAnimation(material, 1, 0);
    }

    private void _PlayFadeInAnimation(SpatialMaterial material)
    {
        _PlayTweenAnimation(material, 0, 1);
    }

    private void _Recolor()
    {
        SpatialMaterial material = Globals.GetEmissionMaterial(0);
        material.EmissionEnergy = 0;

        _border.SetSurfaceMaterial(0, material);
        _PlayFadeInAnimation(material);
    }

    private void _PlayTweenAnimation(SpatialMaterial material, float start, float end)
    {
        float speed = Globals.animationSpeed;
        speed -= 0.05f;

        _tween.InterpolateProperty(material, "emission_energy", start, end, speed, _trans);
        _tween.Start();
    }

    private void _OnTweenCompleted()
    {
        if (_end)
        {
            this.GetParent().QueueFree();
        }
    }
}
