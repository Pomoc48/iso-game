using Godot;
using System;

public class Interface : Control
{
    private Globals G;
    private Player Player;

    private Texture whiteTexture;
    private Texture redTexture;

    private Control healthBar;
    private TextureRect healthBarTR;
    private Label scoreText;
    private Label textFps;
    private Label highScoreText;

    private AnimationPlayer healthAnimation;
    private AnimationPlayer buttonsAnimLeft;
    private AnimationPlayer buttonsAnimRight;
    private AnimationPlayer textAnim;
    private AnimationPlayer textUI;

    private bool screenSizeCalculated;
    private bool healthBarShow = true;

    private float screenSize;
    private float updateHealthBy;

    // Init function
    public override void _Ready()
    {
        healthBar = GetNode<Control>("Main/Health");
        healthBarTR = GetNode<TextureRect>("Main/Health/Bar");
        healthAnimation = GetNode<AnimationPlayer>("Main/Health/HealthAnim");
        buttonsAnimLeft = GetNode<AnimationPlayer>("Main/Left/ShowHide");
        buttonsAnimRight = GetNode<AnimationPlayer>("Main/Right/ShowHide");

        textFps = GetNode<Label>("Main/Fps");
        highScoreText = GetNode<Label>("Main/HighScore");
        scoreText = GetNode<Label>("Main/Score");
        textAnim = scoreText.GetNode<AnimationPlayer>("Bump");
        textUI = GetNode<AnimationPlayer>("Main/Fps/ShowHide");

        G = GetNode<Globals>("/root/Globals");
        Player = GetNode<Player>("/root/Level/Player");

        whiteTexture = (Texture)GD.Load("res://assets/textures/square.png");
        redTexture = (Texture)GD.Load("res://assets/textures/squareRed.png");

        if (!screenSizeCalculated) GetScreenSize();
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
            healthAnimation.Play("HealthDown");
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
	    textAnim.Play("TextAnim");
    }

    public void UpdateFps(String fps)
    {
        String fpsText = fps + " fps";
	    textFps.Text = fpsText;
    }

    // Gameover call
    public void HideUiAnimations(bool highscore)
    {
        buttonsAnimRight.Play("Hide");
	    buttonsAnimLeft.Play("Hide");

        // Omit only when new highscore
	    if (!highscore) textAnim.Play("Hide");

        textUI.Play("HideUI");
	    healthAnimation.Play("HealthUp");
    }

    public void UpdateHighScore(int score, bool start)
    {
        highScoreText.Text = "HiScore: " + score;
        if (start) return;

        // Ignore on start
        textAnim.Play("Highscore");
    }

    public void ColorHealthbarRed(bool red)
    {
        if (red)
        {
            if (healthBarTR.Texture == redTexture) return;
            healthBarTR.Texture = redTexture;


            return;
        }
        
        if (healthBarTR.Texture == whiteTexture) return;
        healthBarTR.Texture = whiteTexture;
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