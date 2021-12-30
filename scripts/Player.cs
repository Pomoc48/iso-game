using Godot;
using System;

public class Player : Spatial
{
    private PlayerVariables Globals;
    private Level Level;

    private Interface interfaceMain;
    private Tween playerTween;
    private MeshInstance playerMesh;
    private Camera playerCamera;

    private AnimationPlayer cameraRotation;
    private AnimationPlayer cameraAnimation;

    private bool canMove = false;
    private bool playerDead = false;
    private bool cameraRotating = false;

    private float lifeLossRate = 0.04f;
    private float lifeGainRate = 2.0f;

    private int speedupCounter = 0;
    private int nextCycle = 0;
    private int maxCycle = 20;
    private int frames = 0;

    String[] keys = {"ui_up", "ui_right", "ui_down", "ui_left"};

    // Init function
    public override void _Ready()
    {
        Globals = GetNode<PlayerVariables>("/root/PlayerVariables");

        Level = GetNode<Level>("/root/Level");
        interfaceMain = GetNode<Interface>("/root/Level/Interface");

        playerTween = GetNode<Tween>("Tween");
        playerMesh = this.GetNode<MeshInstance>("Spatial/Spatial2");
        playerCamera = this.GetNode<Camera>("Camera");

        cameraAnimation = playerCamera.GetNode<AnimationPlayer>("CameraPan");
        cameraRotation = GetNode<AnimationPlayer>("CameraRotation");

        Globals.ResetVars();
        Globals.playerPosition = this.Translation;

        nextCycle = Globals.GetMaxCycle(maxCycle, 4);

        // Get previous highscore

        Globals.highScore = Globals.Load("HighScore");
        interfaceMain.UpdateHighScore(Globals.highScore);
    }

    // Debug for now
    public override void _Process(float delta)
    {

        if (Input.IsActionPressed(keys[0]) &&
                Input.IsActionPressed(keys[1]))
        {
            if (canMove) CheckMove(0);
        }

        if (Input.IsActionPressed(keys[1]) &&
                Input.IsActionPressed(keys[2]))
        {
            if (canMove) CheckMove(1);
        }

        if (Input.IsActionPressed(keys[2]) &&
                Input.IsActionPressed(keys[3]))
        {
            if (canMove) CheckMove(2);
        }

        if (Input.IsActionPressed(keys[3]) &&
                Input.IsActionPressed(keys[0]))
        {
            if (canMove) CheckMove(3);
        }
    }

    // Runs every game tick
    public override void _PhysicsProcess(float delta)
    {
        // Loose hp after game started
        if (!Globals.firstMove)
        {
            // Take less life when rotating
            if (cameraRotating) Globals.playerHealth -= lifeLossRate / 4;
            else Globals.playerHealth -= lifeLossRate / 2;

            interfaceMain.CalculateHealthBar();
        }

        // No life game_over check
        if ((Globals.playerHealth <= 0) && !playerDead)
        {
            GameOver();
        }

        frames++;

        if (frames >= 120)
        {
            frames = 0;
            Level.CreateDecorations();

            // Debug only
            interfaceMain.UpdateFps(Engine.GetFramesPerSecond().ToString());
        }
    }

    public void CheckMove(int dir)
    {
        if (!canMove) return;

        // Calculation based on camera rotation
        Globals.animDirection = Globals.RetranslateDirection(dir);
        
        if (Globals.IsMoveLegal()) CorrectScoreCalculation();
        else RebounceCheck(dir);

    }

    private void CorrectScoreCalculation()
    {
        EnableControls(false, false);

        // Movement animation
        playerTween.InterpolateProperty(this, "translation",
                this.Translation, Globals.DirectionCalc(), 0.25f,
                Tween.TransitionType.Quad, Tween.EaseType.InOut);
        playerTween.Start();
        
        Globals.sessionScore++;
        interfaceMain.AddScore();
        
        speedupCounter++;
        
        // Progress the game
        Level.GeneratePlatform();
        GiveHealth(lifeGainRate);
        
        // Slowly increase difficulty
        if (speedupCounter >= nextCycle)
        {
            lifeLossRate += 0.01f;
            lifeGainRate += 0.25f;
            speedupCounter = 0;

            maxCycle--;

            if (maxCycle < 5)
            {
                maxCycle = 5;
                nextCycle = Globals.GetMaxCycle(maxCycle, 4);
            }

            else nextCycle = Globals.GetMaxCycle(maxCycle, 5);

            RotateCamera();
        }  
    }

    private void RotateCamera()
    {
        // Get random rotation direction
        bool clockwise = Globals.RandomBool();
        
        // Camera rotation section
        if (clockwise) Globals.camRotIndex++;
        else Globals.camRotIndex--;

        // camera_rotation_index = 3 -> DEFAULT
        if (Globals.camRotIndex > 3) Globals.camRotIndex = 0;
        if (Globals.camRotIndex < 0) Globals.camRotIndex= 3;

        // Disable controls for animation duration
        EnableControls(false, false);
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
        EnableControls(true, false);
        cameraRotating = false;
    }

    private void _on_Tween_tween_all_completed()
    {
        // Update global position at the end of animation
        Globals.playerPosition = this.Translation;

        // Small bug fix
        if (cameraRotating) return;
        EnableControls(true, false);
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
        
    private void RebounceCheck(int original_dir)
    {
        EnableControls(false, true);

        // Wrong move penalty
        if (!Globals.firstMove) Globals.playerHealth -= 10;
        
        // Instant game over
        if ((Globals.playerHealth <= 0) && !playerDead) GameOver();
        // Animate player rebounce
        else cameraRotation.Play("Bounce" + original_dir.ToString());	
    }

    private void GameOver()
    {
        // Preventing movement after death
        playerDead = true;
        EnableControls(false, true);

        if (Globals.sessionScore > Globals.highScore)
        {
            Globals.Save("HighScore", Globals.sessionScore);
            interfaceMain.UpdateHighScore(Globals.sessionScore);
        }

        // Outro animations
        cameraAnimation.Play("CameraUp");
        interfaceMain.HideUiAnimations();
    }

    private void EnableControls(bool enable, bool red)
    {
        if (enable)
        {
            canMove = true;

            if (playerMesh.GetSurfaceMaterial(0) == Globals.emissionBlue) return;
            playerMesh.SetSurfaceMaterial(0, Globals.emissionBlue);
            return;
        }

        canMove = false;
        if (red) playerMesh.SetSurfaceMaterial(0, Globals.emissionRed);
    }

    private void EnableCameraPerspective(bool perspective)
    {
        if (perspective) playerCamera.Projection = Camera.ProjectionEnum.Perspective;
        else playerCamera.Projection = Camera.ProjectionEnum.Orthogonal;
    }
        
    private void _on_CameraPan_animation_finished(String anim_name)
    {
        if (anim_name == "CameraUp") GetTree().ReloadCurrentScene();
        else EnableControls(true, false);
    }
}