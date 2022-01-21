using Godot;
using System;

public class Player : Spatial
{
    private Globals Globals;
    private Statistics Statistics;
    private Level Level;
    private Interface Interface;

    private Random _random = new Random();

    private MeshInstance _playerMesh;
    private Camera _playerCamera;
    private Tween _playerTween;

    private AnimationPlayer _spatialAnimation;

    private bool _canPlayerMove = false;
    private bool _isPlayerDead = false;
    private bool _isCameraRotating = false;

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
        _playerMesh = this.GetNode<MeshInstance>("Spatial/Mesh");
        _playerCamera = this.GetNode<Camera>("Camera");

        _spatialAnimation = GetNode<AnimationPlayer>("SpatialAnim");

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
        // No life game_over check
        if ((Globals.playerHealth <= 0) && !_isPlayerDead)
        {
            _GameOver();
        }

        _CalculateFrames();

        if (!Globals.perspectiveMode && (_frameCountPM % 2) == 0)
        {
            _LooseHealth();
        }
    }

    public void CheckMove(Direction direction)
    {
        if (!_canPlayerMove)
        {
            return;
        }

        // Calculation based on camera rotation
        Globals.animationDirection = Globals.RetranslateDirection(direction);

        if (Globals.IsMoveLegal())
        {
            _CorrectScoreCalculation();
            return;
        }

        _RebounceCheck(direction);
    }

    private void _LooseHealth()
    {
        if (Globals.firstMove)
        {
            return;
        }

        // Take less life when rotating
        if (_isCameraRotating)
        {
            Globals.playerHealth -= _lifeLossRate / 4;
        }
        else
        {
            Globals.playerHealth -= _lifeLossRate / 2;
        }

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
        if (!Globals.perspectiveMode)
        {
            return;
        }

        _frameCountPM++;
        if ((_frameCountPM % 2) != 0)
        {
            return;
        }

        Interface.CalculatePerspectiveBar(_frameCountPM);

        if (_frameCountPM == 600)
        {
            _frameCountPM = 0;
            _EnablePerspectiveMode(false);
        }
    }

    private void _CorrectScoreCalculation()
    {
        _EnableControls(false, false);

        Vector3 oldPos = this.Translation;
        Vector3 newPos = Globals.DirectionCalc();

        // Increaase platform height
        Globals.platformHeight += Globals.INCREASE_HEIGHT_BY;
        newPos.y += Globals.INCREASE_HEIGHT_BY;

        // Movement animation
        _PlayTweenAnim("translation", oldPos, newPos, _animationSpeed);
        
        Globals.sessionScore += _increaseScoreBy;
        Interface.AddScore();
        
        // Don't active it twice by a small chance
        if (!Globals.perspectiveMode)
        {
            _RollPerspectiveMode();
        }

        // Progress the game
        Level.GeneratePlatform();
        GiveHealth(_lifeGainRate);
        _DifficultyIncrease();
    }

    private void _RollPerspectiveMode()
    {
        int chance = _random.Next(100);

        // 1% chance to activate special mode after 20 moves
        if (chance < 1 && _faliedCountPM >= 20)
        {
            _EnablePerspectiveMode(true);
            _faliedCountPM = 0;
        }
        else
        {
            _faliedCountPM++;

            // Quadruple the chances after unlucky 100 moves
            if (chance < 4 && _faliedCountPM >= 100)
            {
                _EnablePerspectiveMode(true);
                _faliedCountPM = 0;
            }
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

        _CheckAndGenerateNewCycle();
        _RotateCameraBy(Globals.GetRandomRotationAmmount());
    }

    // For camera rotation and diff increase
    private void _CheckAndGenerateNewCycle()
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
        _EnableControls(false, false);
        _PlayCorrectAnimation(rotateClockwise, rotations);
    }

    private void _PlayCorrectAnimation(bool clockwise, int rotations)
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

        if (!_isCameraRotating) // Small bug fix
        {
            _EnableControls(true, false);
        }
    }

    private void GiveHealth(float ammount)
    {
        // Health cap check
        if ((Globals.playerHealth + ammount) > Globals.FULL_HEALTH)
        {
            Globals.playerHealth = Globals.FULL_HEALTH;
        }
        else
        {
            Globals.playerHealth += ammount;
        }
    }

    private void _RebounceCheck(Direction originalDirection)
    {
        // No penalties during perspective mode
        if (!Globals.perspectiveMode)
        {
            _EnableControls(false, true);

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
        else
        {
            _EnableControls(false, false);
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
        _EnableControls(false, true);

        if (Globals.sessionScore > Globals.highScore)
        {
            Globals.highScore = Globals.sessionScore;

            // Give more time for the new highscore animation
            _spatialAnimation.Play("camera_up_long");
            Interface.HideUiAnimations(true);
        }

        else
        {
            // Outro animations
            _spatialAnimation.Play("camera_up");
            Interface.HideUiAnimations(false);
        }

        Statistics.UploadStatistics();
    }

    private void _EnableControls(bool enable, bool red)
    {
        if (enable)
        {
            _canPlayerMove = true;

            if (!Globals.perspectiveMode)
            {
                _ChangePlayerColor(false);
            }
        }
        else
        {
            _canPlayerMove = false;

            if (red)
            {
                _ChangePlayerColor(true);
            }
        }
    }

    private void _EnablePerspectiveMode(bool perspective)
    {
        Globals.perspectiveMode = perspective;
        Interface.PlayBlindAnim(perspective);

        if (perspective)
        {
            // Replenish full health
            GiveHealth(Globals.FULL_HEALTH);
            Interface.CalculateHealthBar();

            _ChangePlayerColor(true);
            Interface.ColorHealthbarRed(true);

            // Double score when in perspective mode
            _increaseScoreBy = 2;
        }
        else
        {
            _increaseScoreBy = 1;

            _ChangePlayerColor(false);
            Interface.ColorHealthbarRed(false);
        }
    }

    private void _ChangePlayerColor(bool red)
    {
        if (red && (_playerMesh.GetSurfaceMaterial(0) == Globals.materialRed))
        {
            _playerMesh.SetSurfaceMaterial(0, Globals.materialRed);
        }
        else if (_playerMesh.GetSurfaceMaterial(0) == Globals.materialBlue)
        {
            _playerMesh.SetSurfaceMaterial(0, Globals.materialBlue);
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
            _EnableControls(true, false);
            _isCameraRotating = false;
        }
    }
}