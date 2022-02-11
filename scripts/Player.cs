using Godot;
using System;

public class Player : Spatial
{
    private Level Level;
    private Globals Globals;

    private Spatial _playerSpatial;
    private Tween _playerTween;

    private CPUParticles _bounceParticles;
    private CPUParticles _gameOverParticles;
    private CPUParticles _bodyParticles;
    private CPUParticles _bodyParticles2;

    private AnimationPlayer _spatialAnimation;

    public override void _Ready()
    {
        Level = GetNode<Level>("/root/Level");
        Globals = GetNode<Globals>("/root/Globals");

        _playerTween = GetNode<Tween>("Tween");
        _playerSpatial = this.GetNode<Spatial>("Spatial");

        _spatialAnimation = GetNode<AnimationPlayer>("SpatialAnim");

        _bounceParticles = _playerSpatial.GetNode<CPUParticles>("Bounce");
        _gameOverParticles = _playerSpatial.GetNode<CPUParticles>("GameOver");
        _bodyParticles = _playerSpatial.GetNode<CPUParticles>("BodyP");
        _bodyParticles2 = _playerSpatial.GetNode<CPUParticles>("BodyCenterP");

        _playerTween.PlaybackProcessMode = Tween.TweenProcessMode.Idle;
    }

    public void AnimateMovement()
    {
        Vector3 newPosition = Globals.GetFuturePosition();
        newPosition.y = 2;
        _PlayTweenAnim("translation", newPosition, Globals.animationSpeed);
    }

    public void UpdateColor()
    {
        SpatialMaterial hueSmall = Globals.GetEmissionMaterial(0.025f);
        SpatialMaterial hueBig = Globals.GetEmissionMaterial(0.25f);

        _bodyParticles.Mesh.SurfaceSetMaterial(0, hueSmall);

        _gameOverParticles.Mesh.SurfaceSetMaterial(0, hueBig);
        _bounceParticles.Mesh.SurfaceSetMaterial(0, hueBig);
        _bodyParticles2.Mesh.SurfaceSetMaterial(0, hueBig);
    }

    public void RotateCameraBy(int rotations)
    {
        bool rotateClockwise = Globals.GetRandomBool();

        if (rotateClockwise)
        {
            Globals.cameraRotation += rotations;
        }
        else
        {
            Globals.cameraRotation -= rotations;
        }

        // cameraRotation = 3 -> DEFAULT
        if ((int)Globals.cameraRotation > 3)
        {
            Globals.cameraRotation -= 4;
        }

        if (Globals.cameraRotation < 0)
        {
            Globals.cameraRotation += 4;
        }

        Level.TogglePlayerControls(false);
        _PlayCameraRotationAnimation(rotateClockwise, rotations);
    }

    private void _PlayCameraRotationAnimation(bool clockwise, int rotations)
    {
        int rotateBy = 90 * rotations;
        // More rotations take longer
        float time = rotations * Globals.animationSpeed * 1.5f;

        Vector3 newRotation = this.RotationDegrees;

        if (clockwise)
        {
            newRotation.y += rotateBy;
        }
        else
        {
            newRotation.y -= rotateBy;
        }

        _PlayTweenAnim("rotation_degrees", newRotation, time);
    }

    private void _PlayTweenAnim(String type, Vector3 newVector, float time)
    {
        Tween.TransitionType trans = Tween.TransitionType.Sine;

        _playerTween.InterpolateProperty(this, type, null, newVector, time, trans);
        _playerTween.Start();
    }

    // Reenable controls
    private void _OnTweenAnimationFinished()
    {
        // Update global position at the end of animation
        Globals.playerPosition = this.Translation;

        Level.TogglePlayerControls(true);
    }

    public void PlaySpatialAnimation(String animation)
    {
        if (_spatialAnimation.IsPlaying())
        {
            _CheckGameOverAnimation(animation);
        }
        else
        {
            _spatialAnimation.Play(animation);
        }
    }

    private void _CheckGameOverAnimation(String animation)
    {
        if (animation == "camera_up" || animation == "camera_up_long")
        {
            _spatialAnimation.Stop(true);
            // Fix particles rarely emitting non stop after game over
            _bounceParticles.Emitting = false;
            _spatialAnimation.Play(animation);
        }
    }

    private void _OnSpatialAnimationFinished(String animationName)
    {
        if (animationName == "camera_up" || animationName == "camera_up_long")
        {
            GetTree().ReloadCurrentScene();
        }
        else
        {
            Level.TogglePlayerControls(true);
        }
    }
}
