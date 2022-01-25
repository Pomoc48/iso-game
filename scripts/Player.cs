using Godot;
using System;

public class Player : Spatial
{
    private Globals Globals;
    private Statistics Statistics;
    private Platforms Platforms;
    private Interface Interface;
    private Decorations Decorations;

    private Random _random = new();

    private Spatial _playerSpatial;
    private MeshInstance _playerMesh;
    private Tween _playerTween;

    private CPUParticles _bounceParticles;
    private CPUParticles _gameOverParticles;

    private AnimationPlayer _spatialAnimation;

    private bool _canPlayerMove = false;
    private bool _isPlayerDead = false;

    private float _lifeLossRate = 0.04f;
    private float _lifeGainRate = 2.0f;
    private float _animationSpeed = 0.25f;

    private int _speedupCounter = 0;
    private int _difficultyCycle = 0;
    private int _maxDifficultyCycle = 20;
    private int _increaseScoreBy = 1;
    private int _frameCount = 0;
    private int _frameCountPM = 0;
    private int _faliedCountPM = 0;

    private String[] _inputMap = {
        "ui_up",
        "ui_right",
        "ui_down", 
        "ui_left",
    };

    public override void _Ready()
    {
        Globals = GetNode<Globals>("/root/Globals");
        Statistics = GetNode<Statistics>("/root/Level/Interface/Main/StatsButton");
        Platforms = GetNode<Platforms>("/root/Level/Platforms");
        Interface = GetNode<Interface>("/root/Level/Interface");
        Decorations = GetNode<Decorations>("/root/Level/Decorations");

        _playerTween = GetNode<Tween>("Tween");
        _playerSpatial = this.GetNode<Spatial>("Spatial");
        _playerMesh = _playerSpatial.GetNode<MeshInstance>("Mesh");

        _spatialAnimation = GetNode<AnimationPlayer>("SpatialAnim");

        _bounceParticles = _playerSpatial.GetNode<CPUParticles>("Bounce");
        _gameOverParticles = _playerSpatial.GetNode<CPUParticles>("GameOver");

        Globals.NewGame();
        Globals.playerPosition = this.Translation;

        _difficultyCycle = Globals.GetNextCycle(_maxDifficultyCycle);
    }

    public override void _Process(float delta)
    {
        if (_canPlayerMove && Input.IsActionPressed(_inputMap[0])
            && Input.IsActionPressed(_inputMap[1]))
        {
            CheckMove(Direction.RightUp);
        }

        if (_canPlayerMove && Input.IsActionPressed(_inputMap[1])
            && Input.IsActionPressed(_inputMap[2]))
        {
            CheckMove(Direction.RightDown);
        }

        if (_canPlayerMove && Input.IsActionPressed(_inputMap[2])
            && Input.IsActionPressed(_inputMap[3]))
        {
            CheckMove(Direction.LeftDown);
        }

        if (_canPlayerMove && Input.IsActionPressed(_inputMap[3])
            && Input.IsActionPressed(_inputMap[0]))
        {
            CheckMove(Direction.LeftUp);
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        if ((Globals.playerHealth <= 0) && !_isPlayerDead)
        {
            _GameOver();
        }

        if (!Globals.perspectiveMode && !Globals.firstMove && (_frameCountPM % 2) == 0)
        {
            _LooseHealthOnTick();
        }

        _CalculateFrames();
    }

    public void CheckMove(Direction direction)
    {
        if (!_canPlayerMove)
        {
            return;
        }

        _canPlayerMove = false;

        // Calculation based on camera rotation
        Globals.animationDirection = Globals.RetranslateDirection(direction);

        if (Globals.IsMoveValid())
        {
            _CorrectMove();
        }
        else
        {
            _WrongMove(direction);
        }
    }

    private void _LooseHealthOnTick()
    {
        Globals.playerHealth -= _lifeLossRate / 2;
        Interface.CalculateHealthBar();
    }

    private void _CalculateFrames()
    {
        _frameCount++;
        if (_frameCount >= 120)
        {
            _frameCount = 0;
            Decorations.Create();
        }

        if (Globals.perspectiveMode)
        {
            _CalculatePerspectiveFrames();
        }
    }

    private void _CalculatePerspectiveFrames()
    {
        _frameCountPM++;

        if ((_frameCountPM % 2) != 0)
        {
            return;
        }

        Interface.CalculatePerspectiveBar(_frameCountPM);

        if (_frameCountPM == Globals.FIVE_SEC_IN_FRAMES)
        {
            _frameCountPM = 0;
            _DisablePerspectiveMode();
        }
    }

    private void _CorrectMove()
    {
        _AnimatePlayerMovement();
        
        Globals.sessionScore += _increaseScoreBy;
        Interface.UpdateScore();
        
        _RollPerspectiveMode();

        Platforms.Generate();

        _UpdatePlayerColor();
        Interface.UpdateHealthbarColor();

        _GivePlayerHealth();
        _DifficultyIncrease();
    }

    private void _AnimatePlayerMovement()
    {
        Vector3 oldPosition = this.Translation;
        Vector3 newPosition = Globals.GetFuturePosition();

        Globals.platformHeight += Globals.INCREASE_HEIGHT_BY;
        newPosition.y += Globals.INCREASE_HEIGHT_BY;

        _PlayTweenAnim("translation", oldPosition, newPosition, _animationSpeed);
    }

    private void _UpdatePlayerColor()
    {
        SpatialMaterial newHue = new SpatialMaterial();

        newHue.EmissionEnabled = true;
        newHue.Emission = Globals.emissionColor;
        
        _playerMesh.SetSurfaceMaterial(0, newHue);
        _bounceParticles.Mesh.SurfaceSetMaterial(0, newHue);
        _gameOverParticles.Mesh.SurfaceSetMaterial(0, newHue);
    }

    private void _RollPerspectiveMode()
    {
        if (!Globals.perspectiveMode)
        {
            int chance = _random.Next(100);
            _CheckPerspectiveModeChances(chance);
        }
    }

    private void _CheckPerspectiveModeChances(int chance)
    {
        // 1% chance to activate special mode after 20 moves
        if (chance < 1 && _faliedCountPM >= 20)
        {
            _EnablePerspectiveMode();
            return;
        }

        _faliedCountPM++;

        // Quadruple the chances after unlucky 100 moves
        if (chance < 4 && _faliedCountPM >= 100)
        {
            _EnablePerspectiveMode();
        }
    }

    private void _DifficultyIncrease()
    {
        _speedupCounter++;

        if (_speedupCounter < _difficultyCycle)
        {
            return;
        }

        _speedupCounter = 0;

        _lifeLossRate += 0.01f;
        _lifeGainRate += 0.25f;

        // Cap player speed
        if (_animationSpeed > 0.15f)
        {
            _animationSpeed -= 0.005f;
        }

        _GenerateNewDifficultyCycle();
        _RotateCameraBy(Globals.GetRandomRotationAmmount());
    }

    private void _GenerateNewDifficultyCycle()
    {
        if ((_maxDifficultyCycle -= 1) < 5)
        {
            _maxDifficultyCycle = 5;
        }

        _difficultyCycle = Globals.GetNextCycle(_maxDifficultyCycle);
    }

    private void _RotateCameraBy(int rotations)
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

        _canPlayerMove = false;
        _PlayCameraRotationAnimation(rotateClockwise, rotations);
    }

    private void _PlayCameraRotationAnimation(bool clockwise, int rotations)
    {
        int rotateBy = 90 * rotations;
        // More rotations take longer
        float time = rotations * _animationSpeed * 1.5f;

        Vector3 oldRotRad = this.RotationDegrees;
        Vector3 newRot = oldRotRad;

        if (clockwise)
        {
            newRot.y += rotateBy;
        }
        else
        {
            newRot.y -= rotateBy;
        }

        _PlayTweenAnim("rotation_degrees", oldRotRad, newRot, time);
    }

    private void _PlayTweenAnim(String type, Vector3 oldVector, Vector3 newVector, float time)
    {
        Tween.TransitionType trans = Tween.TransitionType.Quad;
        Tween.EaseType ease = Tween.EaseType.InOut;

        _playerTween.InterpolateProperty(this, type, oldVector, newVector, time, trans, ease);
        _playerTween.Start();
    }

    // Reenable controls
    private void _OnTweenAnimationFinished()
    {
        // Update global position at the end of animation
        Globals.playerPosition = this.Translation;

        _canPlayerMove = true;
    }

    private void _GivePlayerHealth()
    {
        if ((Globals.playerHealth += _lifeGainRate) > Globals.FULL_HEALTH)
        {
            Globals.playerHealth = Globals.FULL_HEALTH;
        }
    }

    private void _RestorePlayerHealth()
    {
        Globals.playerHealth = Globals.FULL_HEALTH;
    }

    private void _WrongMove(Direction direction)
    {
        if (!Globals.perspectiveMode)
        {
            _TakePlayerHealth();
        }

        if (!_isPlayerDead)
        {
            _spatialAnimation.Play("bounce" + ((int)direction).ToString());
        }
    }

    private void _TakePlayerHealth()
    {
        if (!Globals.firstMove)
        {
            Globals.playerHealth -= 10;
        }

        if ((Globals.playerHealth <= 0) && !_isPlayerDead)
        {
            _GameOver();
        }
    }

    private void _GameOver()
    {
        _isPlayerDead = true;
        _canPlayerMove = false;

        if (Globals.sessionScore > Globals.highScore)
        {
            Globals.highScore = Globals.sessionScore;
            _PlayOutroAnimationsHighscore();
        }
        else
        {
            _PlayOutroAnimations();
        }

        Statistics.UploadStatistics();
    }

    private void _PlayOutroAnimations()
    {
        _spatialAnimation.Play("camera_up");
        Interface.HideUiAnimations();
    }

    private void _PlayOutroAnimationsHighscore()
    {
        _spatialAnimation.Play("camera_up_long");
        Interface.HideUiAnimationsHighscore();
    }

    private void _EnablePerspectiveMode()
    {
        _faliedCountPM = 0;

        Globals.perspectiveMode = true;
        Interface.PlayPerspectiveAnimation();

        _RestorePlayerHealth();
        Interface.CalculateHealthBar();

        _increaseScoreBy = 2;
    }

    private void _DisablePerspectiveMode()
    {
        Interface.PlayOrthogonalAnimation();

        Globals.perspectiveMode = false;
        _increaseScoreBy = 1;
    }

    private void _OnSpatialAnimationFinished(String animationName)
    {
        if (animationName == "camera_up" || animationName == "camera_up_long")
        {
            GetTree().ReloadCurrentScene();
        }
        else
        {
            _canPlayerMove = true;
        }
    }
}