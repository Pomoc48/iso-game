using Godot;
using System;

public class Level : Spatial
{
    private Random rnd = new Random();
    private PlayerVariables Globals;

    private Spatial platformsSpace;
    private Spatial decorationsSpace;

    private int history = 4;
    private int decoHistory = 12;
    private int totalDeco = 0;
    private int totalPlatforms = 1;

    // Array of possible moves
    String[,] pMoves = {
        {"Long0", "Corner0", "Corner1"},
        {"Long1", "Corner1", "Corner2"},
        {"Long0", "Corner2", "Corner3"},
        {"Long1", "Corner3", "Corner0"}
    };

    // Init function
    public override void _Ready()
    {
        platformsSpace = GetNode<Spatial>("Platforms");
	    decorationsSpace = GetNode<Spatial>("Decorations");

        Globals = GetNode<PlayerVariables>("/root/PlayerVariables");

	    CreateDecorations();
        RotateStartingPlatform();
    }

    // Create floating cubes decorations
    public void CreateDecorations()
    {
        Vector3 blockPos = new Vector3();

        // Idle animation position fix
        if (Globals.firstMove) blockPos = Globals.playerPosition;
        // Future move pos
        else blockPos = Globals.DirectionCalc();

        // Random offset
        blockPos.x += Globals.DecorationsCalc();
        blockPos.z += Globals.DecorationsCalc();

        // Always below platforms
        int tempY = rnd.Next(10);
        blockPos.y = -8;
        blockPos.y += tempY;

        PackedScene block = (PackedScene)ResourceLoader
                .Load("res://assets/Block.tscn");
                
        Spatial blockI = (Spatial)block.Instance();
        blockI.Translation = blockPos;
        
        decorationsSpace.AddChild(blockI);
        totalDeco++;

        // Remove old blocks and disable particles
        if (totalDeco >= decoHistory)
        {
            int decoIndex = totalDeco - decoHistory;
            Spatial blockDeco = decorationsSpace.GetChild<Spatial>(decoIndex);

            blockDeco.GetNode<AnimationPlayer>("AnimationPlayer").Play("Hide");
            //blockDeco.GetNode<CPUParticles>("CPUParticles").Emitting = false;

            // yield(get_tree().create_timer(0.25), "timeout")
            // # blockDeco.set_visible(false)
            // TODO work to be done
            
            blockDeco.QueueFree();
            totalDeco--;
        }
    }

    public void GeneratePlatform()
    {
        int randomNumber = rnd.Next(3);

        // Save for latter backtracking check
        Globals.prevBlock = pMoves[Globals.animDirection, randomNumber];

        PackedScene platform;

        platform = (PackedScene)ResourceLoader.Load("res://assets/platforms/" +
                    Globals.prevBlock + ".tscn");

        // Load and place new platforms
        Spatial platformI = (Spatial)platform.Instance();

        platformI.Translation = Globals.DirectionCalc();
        Vector3 temp = platformI.Translation;
        temp.y = -16; 
        platformI.Translation = temp;

        platformsSpace.AddChild(platformI);
        
        totalPlatforms++;
        platformI.GetNode<AnimationPlayer>("Spatial/AnimationPlayer").Play("Up");
        
        Globals.prevDirection = Globals.animDirection;
        
        // Animate remove old platforms
        if (totalPlatforms >= history)
        {
            int childIndex = totalPlatforms - history;
            Spatial child = platformsSpace.GetChild<Spatial>(childIndex);

            child.GetNode<AnimationPlayer>("Spatial/AnimationPlayer").Play("Down");

            // yield(get_tree().create_timer(0.2), "timeout")

            child.QueueFree();
            totalPlatforms--;
        }
    }

    private void RotateStartingPlatform()
    {
        float rotation = Globals.GenerateStartingPos();

        // Convert degrees to radians
        rotation *= -1.5707963268f;
        platformsSpace.GetChild<Spatial>(0).RotateY(rotation);
    }
}