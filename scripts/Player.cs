using Godot;
using System;

public class Player : Spatial
{
    private Globals Globals;
    private Statistics Statistics;
    private Level Level;
    private Interface Interface;

    private Random _random = new Random();

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
    private int _nextCycle = 0;
    private int _maxCycle = 20;
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

    // Init function
    public override void _Ready()
    {
        Globals = GetNode<Globals>("/root/Globals");
        Statistics = GetNode<Statistics>("/root/Level/Interface/Main/StatsButton");
        Level = GetNode<Level>("/root/Level");
        Interface = GetNode<Interface>("/root/Level/Interface");

        _playerTween = GetNode<Tween>("Tween");
        _playerSpatial = this.GetNode<Spatial>("Spatial");
        _playerMesh = _playerSpatial.GetNode<MeshInstance>("Mesh");

        _spatialAnimation = GetNode<AnimationPlayer>("SpatialAnim");

        _bounceParticles = _playerSpatial.GetNode<CPUParticles>("Bounce");
        _gameOverParticles = _playerSpatial.GetNode<CPUParticles>("GameOver");

        Globals.NewGame();
        Globals.playerPosition = this.Translation;

        _nextCycle = Globals.GetNextCycle(_maxCycle, 4);
    }

    // Debug for now
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

    // Runs every game tick
    public override void _PhysicsProcess(float delta)
    {
        // No life game over check
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

        if (Globals.IsMoveLegal())
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
            Level.CreateDecoration();
        }

        // Disable PM after 5s
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

        // Progress the game
        Level.GeneratePlatform();

        _UpdatePlayerColor();
        Interface.UpdateHealthbarColor();

        _GiveHealth(_lifeGainRate);
        _DifficultyIncrease();
    }

    private void _AnimatePlayerMovement()
    {
        Vector3 oldPosition = this.Translation;
        Vector3 newPosition = Globals.DirectionCalc();

        // Increase platform height
        Globals.platformHeight += Globals.INCREASE_HEIGHT_BY;
        newPosition.y += Globals.INCREASE_HEIGHT_BY;

        // Movement animation
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
        // Don't active it twice by a small chance
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

        if (_speedupCounter < _nextCycle)
        {
            return;
        }

        _lifeLossRate += 0.01f;
        _lifeGainRate += 0.25f;

        // Cap player speed at 0.15f
        if (_animationSpeed > 0.15f)
        {
            _animationSpeed -= 0.005f;
        }

        _speedupCounter = 0;

        _GenerateNewCycle();
        _RotateCameraBy(Globals.GetRandomRotationAmmount());
    }

    // For camera rotation and diff increase
    private void _GenerateNewCycle()
    {
        _maxCycle--;

        if (_maxCycle < 5)
        {
            _maxCycle = 5;
            _nextCycle = Globals.GetNextCycle(_maxCycle, 4);
        }
        else
        {
            _nextCycle = Globals.GetNextCycle(_maxCycle, 5);
        }
    }

    private void _RotateCameraBy(int rotations)
    {
        // Get random rotation direction
        bool rotateClockwise = Globals.RandomBool();

        // Camera rotation
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

        // Disable controls for animation duration
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

    // For player movement and camera rotations
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

    private void _GiveHealth(float ammount)
    {
        // Health cap check
        if ((Globals.playerHealth += ammount) > Globals.FULL_HEALTH)
        {
            Globals.playerHealth = Globals.FULL_HEALTH;
        }
    }

    private void _WrongMove(Direction originalDirection)
    {
        // No penalties during perspective mode
        if (!Globals.perspectiveMode)
        {
            // Wrong move penalty
            if (!Globals.firstMove)
            {
                Globals.playerHealth -= 10;
            }

            // Instant game over
            if ((Globals.playerHealth <= 0) && !_isPlayerDead)
            {
                _GameOver();
            }
        }

        if (!_isPlayerDead)
        {
            // Animate player rebounce
            int originalDirectionInt = (int)originalDirection;
            _spatialAnimation.Play("bounce" + originalDirectionInt.ToString());
        }
    }

    private void _GameOver()
    {
        // Preventing movement after death
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

        // Replenish full health
        _GiveHealth(Globals.FULL_HEALTH);
        Interface.CalculateHealthBar();

        // Double score when in perspective mode
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