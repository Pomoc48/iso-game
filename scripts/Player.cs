using Godot;
using System;

public class Player : Spatial
{
    private PlayerVariables Globals;
    private Level Level;

    private Interface interfaceMain;
    private Tween playerTween;

    private AnimationPlayer cameraRotation;
    private AnimationPlayer cameraAnimation;

    private bool canMove = false;
    private bool playerDead = false;
    private bool cameraRotating = false;

    private float lifeLossRate = 0.04f;
    private float lifeGainRate = 2.0f;

    private int speedupCounter = 0;

    String[] keys = {"ui_up", "ui_right", "ui_down", "ui_left"};

    // Init function
    public override void _Ready()
    {
        Globals = GetNode<PlayerVariables>("/root/PlayerVariables");

        Level = GetNode<Level>("/root/Level");
        interfaceMain = GetNode<Interface>("/root/Level/Interface");

        playerTween = GetNode<Tween>("Tween");

        cameraAnimation = GetNode<AnimationPlayer>("Camera/CameraPan");
        cameraRotation = GetNode<AnimationPlayer>("CameraRotation");

        Globals.ResetVars();
        Globals.playerPosition = this.Translation;
    }

    // Debug only holding multiple buttons cause bugs
    public override void _Process(float delta)
    {
        if (canMove)
        {
            for (int x = 0; x <= 3; x++)
            {
                // Keyboard input collection
                if (Input.IsActionPressed(keys[x]))
                {
                    CheckMove(x);
                }
            }
        }
    }

    // Runs every game tick
    public override void _PhysicsProcess(float delta)
    {
        // Loose hp after game started
        if (!Globals.firstMove && !cameraRotating)
        {
            Globals.playerHealth -= lifeLossRate;
            interfaceMain.CalculateHealthBar();
        }

        // No life game_over check
        if ((Globals.playerHealth <= 0) && !playerDead)
        {
            GameOver();
        }

        // Debug only
        interfaceMain.UpdateFps(Engine.GetFramesPerSecond().ToString());
    }

    public void TouchControls(int dir)
    {
        // Prevent double inputs
        if (canMove) CheckMove(dir);
    }

    private void CorrectScoreCalculation()
    {
        // Movement animation
        playerTween.InterpolateProperty(this, "translation",
                this.Translation, Globals.DirectionCalc(), 0.25f,
                Tween.TransitionType.Quad, Tween.EaseType.InOut);
        playerTween.Start();
        
        Globals.sessionScore += 1;
        interfaceMain.AddScore();
        
        speedupCounter += 1;
        
        // Progress the game
        Level.GeneratePlatform();
        GiveHealth(lifeGainRate);
        
        Level.CreateDecorations();
        
        // Slowly increase difficulty
        if (speedupCounter >= 10)
        {
            lifeLossRate += 0.01f;
            lifeGainRate += 0.25f;
            speedupCounter = 0;

            RotateCamera();
        }  
    }

    private void RotateCamera()
    {
        // Get random rotation direction
        bool clockwise = Globals.RandomBool();
        
        // Camera rotation section
        if (clockwise) Globals.camRotIndex += 1;
        else Globals.camRotIndex -= 1;

        // camera_rotation_index = 3 -> DEFAULT
        if (Globals.camRotIndex > 3) Globals.camRotIndex = 0;
        if (Globals.camRotIndex < 0) Globals.camRotIndex= 3;

        // Disable controls for animation duration
        canMove = false;
        cameraRotating = true;

        // Play correct camera animation
        if (clockwise)
        {
            cameraRotation.Play("RotationCW" +
                    Globals.camRotIndex.ToString());
        }

        else
        {
            String[] ccwArray = {"2", "1", "0", "3"};
            cameraRotation.Play("RotationCCW" +
                    ccwArray[Globals.camRotIndex]);
        }
    }
	
    // Reenable controls
    private void _on_CameraRotation_animation_finished(String _anim_name)
    {
        canMove = true;
        cameraRotating = false;
    }

    private void _on_Tween_tween_all_completed()
    {
        // Update global position at the end of animation
        Globals.playerPosition = this.Translation;

        // Small bug fix
        if (cameraRotating) return;
        canMove = true;
    }

    private void GiveHealth(float ammount)
    {
        if ((Globals.playerHealth + ammount) > Globals.fullHealth)
        {
            // Health cap check
            Globals.playerHealth = Globals.fullHealth;
        }

        else Globals.playerHealth += ammount;
    }
                    
    private void CheckMove(int dir)
    {
        // Calculation based on camera rotation
        Globals.animDirection = Globals.RetranslateDirection(dir);
        canMove = false;
        
        if (Globals.IsMoveLegal()) CorrectScoreCalculation();
        else RebounceCheck(dir);
    }
        
    private void RebounceCheck(int original_dir)
    {
        // Wrong move penalty
        Globals.playerHealth -= 8;
        
        // Instant game over
        if ((Globals.playerHealth <= 0) && !playerDead) GameOver();
        // Animate player rebounce
        else cameraRotation.Play("Bounce" + original_dir.ToString());	
    }

    private void GameOver()
    {
        // Preventing movement after death
        playerDead = true;
        canMove = false;

        // print("Final Score:", Globals.session_score)

        // Wait for outro anim and restart
        cameraAnimation.Play("CameraUp");
        interfaceMain.HideUiAnimations();
    }
        
    private void _on_CameraPan_animation_finished(String anim_name)
    {
        if (anim_name == "CameraUp") GetTree().ReloadCurrentScene();
        else canMove = true;
    }
}