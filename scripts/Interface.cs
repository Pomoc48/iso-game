using Godot;
using System;

public class Interface : Control
{
    private Globals Globals;
    private Player Player;

    private Control _healthBar;
    private TextureRect _healthBarTR;
    private Label _scoreLabel;
    private Label _highScoreLabel;

    private AnimationPlayer _interfaceAnimation;

    private bool _healthBarShowed;

    private float _screenSize;
    private float _updateHealthBy;

    private const int FIVE_SEC_IN_FRAMES = 600;

    // Init function
    public override void _Ready()
    {
        Globals = GetNode<Globals>("/root/Globals");
        Player = GetNode<Player>("/root/Level/Player");

        _interfaceAnimation = GetNode<AnimationPlayer>("InterfaceAnim");

        _healthBar = GetNode<Control>("Main/Health");
        _healthBarTR = GetNode<TextureRect>("Main/Health/Bar");

        _highScoreLabel = GetNode<Label>("Main/HighScore");
        _scoreLabel = GetNode<Label>("Main/Score");

        GetScreenSize();

        // Get previous highscore
        Globals.highScore = Globals.LoadStats("HighScore");
        _highScoreLabel.Text = "HiScore: " + Globals.highScore;
    }

    // One time screen size calculation
    private void GetScreenSize()
    {
        _screenSize = GetViewport().Size.x;
        _updateHealthBy = _screenSize / Globals.FULL_HEALTH;
    }

    // Calculate healthbar pixels
    public void CalculateHealthBar()
    {
        if (!_healthBarShowed)
        {
            _interfaceAnimation.Play("healthbar_show");
            _healthBarShowed = true;
        }
        
        float health = Globals.playerHealth * _updateHealthBy;

        Vector2 pos = new Vector2(health, 16);
        _healthBar.SetSize(pos, false);
    }

    public void CalculatePerspectiveBar(float frames)
    {
        // Game physics runs 120f/s, 5s is 600f
        frames /= -FIVE_SEC_IN_FRAMES;
        // Reverse percentage
        frames += 1;

        float newSize = _screenSize * frames;

        Vector2 pos = new Vector2(newSize, 16);
        _healthBar.SetSize(pos, false);
    }

    // Update text and play animation
    public void AddScore()
    {
        _scoreLabel.Text = Globals.sessionScore.ToString();
        // Fix PlayBlindAnim() freezing
        if (!_interfaceAnimation.IsPlaying())
        {
            _interfaceAnimation.Play("score_bump");
        }
    }

    public void HideUiAnimations(bool highscore)
    {
        if (highscore)
        {
            _interfaceAnimation.Play("ui_hide_highscore");
        }
        else
        {
            _interfaceAnimation.Play("ui_hide");
        }
    }

    public void ColorHealthbarRed(bool red)
    {
        if (red && (_healthBarTR.Texture !=  Globals.redTexture))
        {
            _healthBarTR.Texture =  Globals.redTexture;
        }
        else if (_healthBarTR.Texture !=  Globals.blueTexture)
        {
            _healthBarTR.Texture =  Globals.blueTexture;
        }
    }

    // Animating transition between projections
    public void PlayBlindAnim(bool perspective)
    {
        if (perspective)
        {
            _interfaceAnimation.Play("blind_perspective");
        }
        else
        {
            _interfaceAnimation.Play("blind_orthogonal");
        }
    }

    public void ShowStatistics(bool show)
    {
        if (show)
        {
            _interfaceAnimation.Play("stats_view_show");
        }
        else
        {
            _interfaceAnimation.Play("stats_view_hide");
        }
    }

    // Connect UI buttons
    private void _OnLeftButtonDown()
    {
        Player.CheckMove(Direction.LeftUp);
    }

    private void _OnRightButtonDown()
    {
        Player.CheckMove(Direction.RightDown);
    }

    private void _OnUpButtonDown()
    {
        Player.CheckMove(Direction.RightUp);
    }

    private void _OnDownButtonDown()
    {
        Player.CheckMove(Direction.LeftDown);
    }
}