using Godot;
using System;

public class Level : Spatial
{
    private Globals Globals;
    private Node Statistics;
    private Node PlatformSpace;
    private Node Interface;
    private Node Decorations;
    private Player Player;

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
        Statistics = GetNode("/root/Level/Interface/Main/StatsButton");
        PlatformSpace = GetNode("/root/Level/Platforms");
        Interface = GetNode("/root/Level/Interface");
        Decorations = GetNode("/root/Level/Decorations");
        Player = GetNode<Player>("/root/Level/Player");

        Globals.NewGame();
        
        Player.UpdateColor();
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

        if (!Globals.perspectiveMode && !Globals.firstMove)
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
        Globals.animationDirection = _TranslateDirection(direction);

        if (_IsMoveValid())
        {
            _CorrectMove();
        }
        else
        {
            _WrongMove(direction);
        }
    }

    public void TogglePlayerControls(bool enable)
    {
        _canPlayerMove = enable;
    }

    public bool _IsMoveValid()
    {
        Globals.totalMoves++;

        foreach (int direction in Globals.possibleMoves)
        {
            if ((int)Globals.animationDirection == direction)
            {
                Globals.correctMoves++;

                if (Globals.firstMove)
                {
                    Globals.firstMove = false;
                }

                return true;
            }
        }

        return false;
    }

    private Direction _TranslateDirection(Direction direction)
    {
        if (Globals.cameraRotation != Direction.LeftUp)
        {
            direction -= (Globals.cameraRotation + 1);
        }

        if (direction < 0)
        {
            direction += 4;
        }

        return direction;
    }

    private void _LooseHealthOnTick()
    {
        Globals.playerHealth -= _lifeLossRate;
        Interface.Call("calculate_healthbar");
    }

    private void _CalculateFrames()
    {
        _frameCount++;
        if (_frameCount >= 60)
        {
            _frameCount = 0;
            Decorations.Call("create");
        }

        if (Globals.perspectiveMode)
        {
            _CalculatePerspectiveFrames();
        }
    }

    private void _CalculatePerspectiveFrames()
    {
        _frameCountPM++;

        Interface.Call("calculate_perspective_bar", _frameCountPM);

        if (_frameCountPM == Globals.FIVE_SEC_IN_FRAMES)
        {
            _frameCountPM = 0;
            _DisablePerspectiveMode();
        }
    }

    private void _CorrectMove()
    {
        Player.AnimateMovement();
        
        Globals.sessionScore += _increaseScoreBy;
        Interface.Call("update_score");
        
        _RollPerspectiveMode();

        Globals.UpdateEmissionMaterial();
        PlatformSpace.Call("generate");

        Player.UpdateColor();
        Interface.Call("update_healthbar_color");

        _GivePlayerHealth();
        _DifficultyIncrease();
    }

    private void _RollPerspectiveMode()
    {
        if (!Globals.perspectiveMode)
        {
            int chance = Globals.GetRandomNumber(100);
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
        Player.RotateCameraBy(_GetRandomRotationAmmount());
    }

    private void _GenerateNewDifficultyCycle()
    {
        if ((_maxDifficultyCycle -= 1) < 5)
        {
            _maxDifficultyCycle = 5;
        }

        _difficultyCycle = Globals.GetNextCycle(_maxDifficultyCycle);
    }

    public int _GetRandomRotationAmmount()
    {
        var randomChance = Globals.GetRandomNumber(100);

        return randomChance switch
        {
            < 5 => 3,
            >= 5 and < 30 => 2,
            _ => 1
        };
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
        Interface.Call("play_interface_animation", "ui_hide");
    }

    private void _PlayOutroAnimationsHighscore()
    {
        Player.PlaySpatialAnimation("camera_up_long");
        Interface.Call("play_interface_animation", "ui_hide_highscore");
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

        Statistics.Call("_upload");
    }

    private void _EnablePerspectiveMode()
    {
        _faliedCountPM = 0;

        Globals.perspectiveMode = true;
        Interface.Call("play_interface_animation", "blind_perspective");

        _RestorePlayerHealth();
        Interface.Call("calculate_healthbar");

        _increaseScoreBy = 2;
    }

    private void _DisablePerspectiveMode()
    {
        Interface.Call("play_interface_animation", "blind_orthogonal");

        Globals.perspectiveMode = false;
        _increaseScoreBy = 1;
    }

    private void _OnLeftButtonDown()
    {
        CheckMove(Direction.LeftUp);
    }

    private void _OnRightButtonDown()
    {
        CheckMove(Direction.RightDown);
    }

    private void _OnUpButtonDown()
    {
        CheckMove(Direction.RightUp);
    }

    private void _OnDownButtonDown()
    {
        CheckMove(Direction.LeftDown);
    }
}
