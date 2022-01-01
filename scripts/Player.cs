using Godot;
using System;

public class Player : Spatial
{
    private PlayerVariables G;
    private Level Level;

    private Interface interfaceMain;
    private Tween playerTween;
    private MeshInstance playerMesh;
    private Camera playerCamera;

    private AnimationPlayer cameraBounce;
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
        G = GetNode<PlayerVariables>("/root/PlayerVariables");

        Level = GetNode<Level>("/root/Level");
        interfaceMain = GetNode<Interface>("/root/Level/Interface");

        playerTween = GetNode<Tween>("Tween");
        playerMesh = this.GetNode<MeshInstance>("Spatial/Mesh");
        playerCamera = this.GetNode<Camera>("Camera");

        cameraAnimation = playerCamera.GetNode<AnimationPlayer>("CameraPan");
        cameraBounce = GetNode<AnimationPlayer>("CameraBounce");

        G.ResetVars();
        G.playerPosition = this.Translation;

        nextCycle = G.GetMaxCycle(maxCycle, 4);

        // Get previous highscore

        G.highScore = G.Load("HighScore");
        interfaceMain.UpdateHighScore(G.highScore, true);
    }

    // Debug for now
    public override void _Process(float delta)
    {
        if (Input.IsActionPressed(keys[0]) &&
                Input.IsActionPressed(keys[1]))
            if (canMove) CheckMove(0);

        if (Input.IsActionPressed(keys[1]) &&
                Input.IsActionPressed(keys[2]))
            if (canMove) CheckMove(1);

        if (Input.IsActionPressed(keys[2]) &&
                Input.IsActionPressed(keys[3]))
            if (canMove) CheckMove(2);

        if (Input.IsActionPressed(keys[3]) &&
                Input.IsActionPressed(keys[0]))
            if (canMove) CheckMove(3);
    }

    // Runs every game tick
    public override void _PhysicsProcess(float delta)
    {
        LooseHealth();

        // No life game_over check
        if ((G.playerHealth <= 0) && !playerDead) GameOver();

        FramesCalculation();
    }

    public void CheckMove(int dir)
    {
        if (!canMove) return;

        // Calculation based on camera rotation
        G.animDirection = G.RetranslateDirection(dir);
        
        if (G.IsMoveLegal())
        {
            CorrectScoreCalculation();
            return;
        }

        RebounceCheck(dir);
    }

    private void LooseHealth()
    {
        if (G.firstMove) return;

        // Take less life when rotating
        if (cameraRotating) G.playerHealth -= lifeLossRate / 4;
        else G.playerHealth -= lifeLossRate / 2;

        interfaceMain.CalculateHealthBar();
    }

    private void FramesCalculation()
    {
        frames++;
        if (frames < 120) return;

        frames = 0;
        Level.CreateDecorations();

        // Debug only
        String fps = Engine.GetFramesPerSecond().ToString();
        interfaceMain.UpdateFps(fps);
    }

    private void CorrectScoreCalculation()
    {
        EnableControls(false, false);

        // Movement animation
        playerTween.InterpolateProperty(this, "translation",
                this.Translation, G.DirectionCalc(), 0.25f,
                Tween.TransitionType.Quad, Tween.EaseType.InOut);
        playerTween.Start();
        
        G.sessionScore++;
        interfaceMain.AddScore();
        
        // Progress the game
        Level.GeneratePlatform();
        GiveHealth(lifeGainRate);
        DifficultyIncrease();
    }

    private void DifficultyIncrease()
    {
        speedupCounter++;
        if (speedupCounter < nextCycle) return;

        lifeLossRate += 0.01f;
        lifeGainRate += 0.25f;
        speedupCounter = 0;

        CheckAndGenerateNewCycle();
        RotateCamera();
    }

    private void CheckAndGenerateNewCycle()
    {
        maxCycle--;

        if (maxCycle < 5)
        {
            maxCycle = 5;
            nextCycle = G.GetMaxCycle(maxCycle, 4);
            return;
        }

        nextCycle = G.GetMaxCycle(maxCycle, 5);
    }

    private void RotateCamera()
    {
        // Get random rotation direction
        bool clockwise = G.RandomBool();

        // Camera rotation section
        if (clockwise) G.camRotIndex++;
        else G.camRotIndex--;

        // camRotIndex = 3 -> DEFAULT
        if (G.camRotIndex > 3) G.camRotIndex = 0;
        if (G.camRotIndex < 0) G.camRotIndex = 3;

        // Disable controls for animation duration
        EnableControls(false, false);
        PlayCorrectAnimation(clockwise);
    }

    private void PlayCorrectAnimation(bool clockwise)
    {
        Vector3 oldRotRad = this.RotationDegrees;
        Vector3 newRot = oldRotRad;

        if (clockwise) newRot.y += 90;
        else newRot.y -= 90;

        playerTween.InterpolateProperty(this, "rotation_degrees",
                oldRotRad, newRot, 0.5f, Tween.TransitionType.Quad,
                Tween.EaseType.InOut);
        playerTween.Start();
    }
	
    // Reenable controls
    private void _on_CameraRotation_animation_finished(String anim_name)
    {
        EnableControls(true, false);
        cameraRotating = false;
    }

    private void _on_Tween_tween_all_completed()
    {
        // Update global position at the end of animation
        G.playerPosition = this.Translation;

        // Small bug fix
        if (!cameraRotating) EnableControls(true, false);
    }

    private void GiveHealth(float ammount)
    {
        // Health cap check
        if ((G.playerHealth + ammount) > G.fullHealth)
        {
            G.playerHealth = G.fullHealth;
            return;
        }

        G.playerHealth += ammount;
    }
        
    private void RebounceCheck(int original_dir)
    {
        EnableControls(false, true);

        // Wrong move penalty
        if (!G.firstMove) G.playerHealth -= 10;
        
        // Instant game over
        if ((G.playerHealth <= 0) && !playerDead)
        {
            GameOver();
            return;
        }

        // Animate player rebounce
        cameraBounce.Play("Bounce" + original_dir.ToString());	
    }

    private void GameOver()
    {
        // Preventing movement after death
        playerDead = true;
        EnableControls(false, true);

        if (G.sessionScore > G.highScore)
        {
            G.Save("HighScore", G.sessionScore);
            interfaceMain.UpdateHighScore(G.sessionScore, false);

            // Give more time for the new highscore animation
            cameraAnimation.Play("CameraUpLong");
            interfaceMain.HideUiAnimations(true);
            return;
        }

        // Outro animations
        cameraAnimation.Play("CameraUp");
        interfaceMain.HideUiAnimations(false);

    }

    private void EnableControls(bool enable, bool red)
    {
        if (enable)
        {
            canMove = true;

            if (playerMesh.GetSurfaceMaterial(0) == G.emissionBlue) return;
            playerMesh.SetSurfaceMaterial(0, G.emissionBlue);
            return;
        }

        canMove = false;
        if (red) playerMesh.SetSurfaceMaterial(0, G.emissionRed);
    }

    private void EnableCameraPerspective(bool perspective)
    {
        if (perspective)
        {
            playerCamera.Projection = Camera.ProjectionEnum.Perspective;
            return;
        }

        playerCamera.Projection = Camera.ProjectionEnum.Orthogonal;
    }
        
    private void _on_CameraPan_animation_finished(String anim_name)
    {
        if (anim_name == "CameraUp" || anim_name == "CameraUpLong")
        {
            GetTree().ReloadCurrentScene();
            return;
        }
            
        EnableControls(true, false);
    }
}