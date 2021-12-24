using Godot;
using System;

public class Interface : Control
{
    private PlayerVariables Globals;
    private Player Player;

    private Control healthBar;
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

    private float updateHealthBy;

    // Init function
    public override void _Ready()
    {
        healthBar = GetNode<Control>("Main/Health");
        healthAnimation = GetNode<AnimationPlayer>("Main/Health/HealthAnim");
        buttonsAnimLeft = GetNode<AnimationPlayer>("Main/Left/ShowHide");
        buttonsAnimRight = GetNode<AnimationPlayer>("Main/Right/ShowHide");

        textFps = GetNode<Label>("Main/Fps");
        highScoreText = GetNode<Label>("Main/HighScore");
        scoreText = GetNode<Label>("Main/Score");
        textAnim = scoreText.GetNode<AnimationPlayer>("Bump");
        textUI = GetNode<AnimationPlayer>("Main/Fps/ShowHide");

        Globals = GetNode<PlayerVariables>("/root/PlayerVariables");
        Player = GetNode<Player>("/root/Level/Player");

        if (!screenSizeCalculated) GetScreenSize();
    }

    // One time screen size calculation
    private void GetScreenSize()
    {
        float screenSize = GetViewport().Size.x;
	    updateHealthBy = screenSize / Globals.fullHealth;
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
		
        float health = Globals.playerHealth * updateHealthBy;
        Vector2 pos = new Vector2(health, 16);

        healthBar.SetSize(pos, false);
    }

    // Update text and play animation
    public void AddScore()
    {
        scoreText.Text = Globals.sessionScore.ToString();
	    textAnim.Play("TextAnim");
    }

    public void UpdateFps(String fps)
    {
        String fpsText = fps + " fps";
	    textFps.Text = fpsText;
    }

    // Gameover call
    public void HideUiAnimations()
    {
        buttonsAnimRight.Play("Hide");
	    buttonsAnimLeft.Play("Hide");
	    textAnim.Play("Hide");
        textUI.Play("HideUI");
	
	    healthAnimation.Play("HealthUp");
    }

    public void UpdateHighScore(int score)
    {
        highScoreText.Text = "HiScore: " + score;
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