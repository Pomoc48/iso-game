using Godot;
using System;

public class Interface : Control
{
    private Globals G;
    private Player Player;

    private Texture blueTexture;
    private Texture redTexture;

    private Control healthBar;
    private TextureRect healthBarTR;
    private Label scoreText;
    private Label highScoreText;

    private AnimationPlayer interfaceAnim;

    private bool screenSizeCalculated;
    private bool healthBarShow = true;

    private float screenSize;
    private float updateHealthBy;

    // Init function
    public override void _Ready()
    {
        interfaceAnim = GetNode<AnimationPlayer>("InterfaceAnim");

        healthBar = GetNode<Control>("Main/Health");
        healthBarTR = GetNode<TextureRect>("Main/Health/Bar");

        highScoreText = GetNode<Label>("Main/HighScore");
        scoreText = GetNode<Label>("Main/Score");

        G = GetNode<Globals>("/root/Globals");
        Player = GetNode<Player>("/root/Level/Player");

        blueTexture = (Texture)GD.Load("res://assets/textures/squareBlue.png");
        redTexture = (Texture)GD.Load("res://assets/textures/squareRed.png");

        if (!screenSizeCalculated) GetScreenSize();

        // Get previous highscore
        G.highScore = G.Load("HighScore");
        highScoreText.Text = "HiScore: " + G.highScore;
    }

    // One time screen size calculation
    private void GetScreenSize()
    {
        screenSize = GetViewport().Size.x;
	    updateHealthBy = screenSize / G.fullHealth;
	    screenSizeCalculated = true;
    }

    // Calculate healthbar pixels
    public void CalculateHealthBar()
    {
        if (healthBarShow)
        {
            interfaceAnim.Play("healthbar_show");
		    healthBarShow = false;
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

    // Gameover call
    public void HideUiAnimations(bool highscore)
    {
        // Omit only when new highscore
	    if (highscore) interfaceAnim.Play("ui_hide_highscore");
        else interfaceAnim.Play("ui_hide");
    }

    public void ColorHealthbarRed(bool red)
    {
        if (red)
        {
            if (healthBarTR.Texture == redTexture) return;
            healthBarTR.Texture = redTexture;

            return;
        }
        
        if (healthBarTR.Texture == blueTexture) return;
        healthBarTR.Texture = blueTexture;
    }

    // Animating transition between projections
    public void PlayBlindAnim(bool perspective)
    {
        GD.Print(perspective);
        if (perspective)
        {
            interfaceAnim.Play("blind_perspective");
            return;
        }
        interfaceAnim.Play("blind_orthogonal");
    }

    public void ShowStatistics(bool show)
    {
        if (show) interfaceAnim.Play("stats_view_show");
        else interfaceAnim.Play("stats_view_hide");
    }

    // Connect UI buttons
    private void _on_Left_button_down()
    {
        Player.CheckMove(3);
    }

    private void _on_Right_button_down()
    {
        Player.CheckMove(1);
    }

    private void _on_Up_button_down()
    {
        Player.CheckMove(0);
    }

    private void _on_Down_button_down()
    {
        Player.CheckMove(2);
    }
}