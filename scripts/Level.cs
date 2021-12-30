using Godot;
using System;

public class Level : Spatial
{
    private Random rnd = new Random();
    private PlayerVariables Globals;

    private Spatial platformsSpace;
    private Spatial decorationsSpace;

    private int history = 3;
    private int decoHistory = 8;
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

        blockPos = Globals.DecorationsCalc(blockPos);

        // Always below platforms
        blockPos.y = 2;

        PackedScene block = (PackedScene)ResourceLoader
                .Load("res://assets/Block.tscn");
                
        Spatial blockI = (Spatial)block.Instance();
        blockI.Translation = blockPos;
        
        decorationsSpace.AddChild(blockI);
        totalDeco++;
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

            totalPlatforms--;
            child.GetNode<AnimationPlayer>("Spatial/AnimationPlayer").Play("Down");
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