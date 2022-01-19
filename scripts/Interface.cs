using Godot;
using System;

public class Interface : Control
{
    private Globals G;
    private Player P;

    private Texture blueTexture;
    private Texture redTexture;

    private Control healthBar;
    private TextureRect healthBarTR;
    private Label scoreText;
    private Label highScoreText;

    private AnimationPlayer interfaceAnim;

    private bool healthBarShowed;

    private float screenSize;
    private float updateHealthBy;

    // Init function
    public override void _Ready()
    {
        G = GetNode<Globals>("/root/Globals");
        P = GetNode<Player>("/root/Level/Player");

        interfaceAnim = GetNode<AnimationPlayer>("InterfaceAnim");

        healthBar = GetNode<Control>("Main/Health");
        healthBarTR = GetNode<TextureRect>("Main/Health/Bar");

        highScoreText = GetNode<Label>("Main/HighScore");
        scoreText = GetNode<Label>("Main/Score");

        blueTexture = (Texture)GD.Load("res://assets/textures/squareBlue.png");
        redTexture = (Texture)GD.Load("res://assets/textures/squareRed.png");

        GetScreenSize();

        // Get previous highscore
        G.highScore = G.Load("HighScore");
        highScoreText.Text = "HiScore: " + G.highScore;
    }

    // One time screen size calculation
    private void GetScreenSize()
    {
        screenSize = GetViewport().Size.x;
        updateHealthBy = screenSize / G.fullHealth;
    }

    // Calculate healthbar pixels
    public void CalculateHealthBar()
    {
        if (!healthBarShowed)
        {
            interfaceAnim.Play("healthbar_show");
            healthBarShowed = true;
        }
        
        float health = G.playerHealth * updateHealthBy;

        Vector2 pos = new Vector2(health, 16);
        healthBar.SetSize(pos, false);
    }

    public void CalculatePerspectiveBar(float frames)
    {
        // Reverse percentage
        frames /= -600;
        frames += 1;

        float newSize = screenSize * frames;

        Vector2 pos = new Vector2(newSize, 16);
        healthBar.SetSize(pos, false);
    }

    // Update text and play animation
    public void AddScore()
    {
        scoreText.Text = G.sessionScore.ToString();
        interfaceAnim.Play("score_bump");
    }

    public void HideUiAnimations(bool highscore)
    {
        if (highscore)
        {
            interfaceAnim.Play("ui_hide_highscore");
        }
        else
        {
            interfaceAnim.Play("ui_hide");
        }
    }

    public void ColorHealthbarRed(bool red)
    {
        if (red && (healthBarTR.Texture != redTexture))
        {
            healthBarTR.Texture = redTexture;
        }
        else if (healthBarTR.Texture != blueTexture)
        {
            healthBarTR.Texture = blueTexture;
        }
    }

    // Animating transition between projections
    public void PlayBlindAnim(bool perspective)
    {
        if (perspective)
        {
            interfaceAnim.Play("blind_perspective");
        }
        else
        {
            interfaceAnim.Play("blind_orthogonal");
        }
    }

    public void ShowStatistics(bool show)
    {
        if (show)
        {
            interfaceAnim.Play("stats_view_show");
        }
        else
        {
            interfaceAnim.Play("stats_view_hide");
        }
    }

    // Connect UI buttons
    private void _on_Left_button_down()
    {
        P.CheckMove(Direction.LeftUp);
    }

    private void _on_Right_button_down()
    {
        P.CheckMove(Direction.RightDown);
    }

    private void _on_Up_button_down()
    {
        P.CheckMove(Direction.RightUp);
    }

    private void _on_Down_button_down()
    {
        P.CheckMove(Direction.LeftDown);
    }
}