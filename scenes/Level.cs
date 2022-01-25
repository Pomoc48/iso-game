using Godot;
using System;

public class Level : Spatial
{
    private Globals Globals;
    private Statistics Statistics;
    private Platforms Platforms;
    private Interface Interface;
    private Decorations Decorations;
    private Player Player;

    private Random _random = new();

    private bool _canPlayerMove = false;
    private bool _isPlayerDead = false;

    private int _frameCount = 0;
    private int _frameCountPM = 0;
    private int _faliedCountPM = 0;

    private float _lifeLossRate = 0.04f;
    private float _lifeGainRate = 2.0f;

    private int _speedupCounter = 0;
    private int _difficultyCycle = 0;
    private int _maxDifficultyCycle = 20;
    private int _increaseScoreBy = 1;

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
        Player = GetNode<Player>("/root/Level/Player");

        Globals.NewGame();
        Globals.playerPosition = Player.Translation;

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

    public void TogglePlayerMove(bool enable)
    {
        _canPlayerMove = enable;
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
        Player.AnimatePlayerMovement();
        
        Globals.sessionScore += _increaseScoreBy;
        Interface.UpdateScore();
        
        _RollPerspectiveMode();

        Platforms.Generate();

        Player.UpdatePlayerColor();
        Interface.UpdateHealthbarColor();

        _GivePlayerHealth();
        _DifficultyIncrease();
    }

    private void _RollPerspectiveMode()
    {
        if (!Globals.perspectiveMode)
        {
            int chance = _random.Next(100);
            _CheckPerspectiveModeChances(chance);
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
        if (Globals.animationSpeed > 0.15f)
        {
            Globals.animationSpeed -= 0.005f;
        }

        _GenerateNewDifficultyCycle();
        Player.RotateCameraBy(Globals.GetRandomRotationAmmount());
    }

    private void _GenerateNewDifficultyCycle()
    {
        if ((_maxDifficultyCycle -= 1) < 5)
        {
            _maxDifficultyCycle = 5;
        }

        _difficultyCycle = Globals.GetNextCycle(_maxDifficultyCycle);
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
            String animationName = "bounce" + ((int)direction).ToString();
            Player.PlaySpatialAnimation(animationName);
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

    private void _PlayOutroAnimations()
    {
        Player.PlaySpatialAnimation("camera_up");
        Interface.HideUiAnimations();
    }

    private void _PlayOutroAnimationsHighscore()
    {
        Player.PlaySpatialAnimation("camera_up_long");
        Interface.HideUiAnimationsHighscore();
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
}
