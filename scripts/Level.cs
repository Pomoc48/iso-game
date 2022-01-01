using Godot;
using System;

public class Level : Spatial
{
    private Random rnd = new Random();
    private PlayerVariables G;

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
        G = GetNode<PlayerVariables>("/root/PlayerVariables");
        
        // Rotate starting platform
        float rotation = G.GenerateStartingPos();

        rotation *= -G.degreeInRad;
        platformsSpace.GetChild<Spatial>(0).RotateY(rotation);
    }

    // Create floating cubes decorations
    public void CreateDecorations()
    {
        Vector3 blockPos = new Vector3();

        // Idle animation position fix
        if (G.firstMove) blockPos = G.playerPosition;
        // Future move pos
        else blockPos = G.DirectionCalc();

        // Random offset
        blockPos = G.DecorationsCalc(blockPos);

        LoadDecoration(blockPos);
    }

    private void LoadDecoration(Vector3 blockPos)
    {
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
        G.prevBlock = pMoves[G.animDirection, randomNumber];

        PackedScene platform = (PackedScene)ResourceLoader
                .Load("res://assets/platforms/" + G.prevBlock + ".tscn");

        // Load and place new platforms
        Spatial platformI = (Spatial)platform.Instance();

        platformI.Translation = G.DirectionCalc();
        Vector3 temp = platformI.Translation;
        temp.y = -16;
        platformI.Translation = temp;

        platformsSpace.AddChild(platformI);

        totalPlatforms++;
        platformI.GetNode<AnimationPlayer>("Spatial/AnimationPlayer").Play("Up");

        G.prevDirection = G.animDirection;

        // Animate remove old platforms
        if (totalPlatforms < history) return;

        RemoveOldPlatforms();
    }

    private void RemoveOldPlatforms()
    {
        int childIndex = totalPlatforms - history;
        Spatial child = platformsSpace.GetChild<Spatial>(childIndex);

        totalPlatforms--;
        child.GetNode<AnimationPlayer>("Spatial/AnimationPlayer").Play("Down");
    }
}