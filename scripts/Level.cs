using Godot;
using System;

public class Level : Spatial
{
    private Random rnd = new Random();
    private Globals G;

    private Spatial platformsSpace;
    private Spatial decorationsSpace;

    private int history = 3;
    private int decoHistory = 8;
    private int totalDeco = 0;
    private int totalPlatforms = 1;

    private float degreeInRad = 1.5707963268f;

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
        G = GetNode<Globals>("/root/Globals");
        
        // Rotate starting platform
        float rotation = G.GenerateStartingPos();

        rotation *= -degreeInRad;
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
        Spatial platformBlockI;

        int platformType = G.GetPlatformType();
        int reverse = 0;

        switch (platformType)
        {
            case 1:

                platformBlockI = PlacePlatform("Long");

                if (G.animDirection % 2 == 0)
                {
                    platformBlockI.RotateY(90 * degreeInRad);
                }
                break;

            case 2:

                platformBlockI = PlacePlatform("Corner");

                if (G.RandomBool()) reverse++;
                float yRot = (G.animDirection + reverse) * -90;

                Vector3 rotationV = new Vector3(0, yRot, 0);
                platformBlockI.RotationDegrees =  rotationV;
                break;

            case 3:

                platformBlockI = PlacePlatform("Cross");
                break;
        }

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

    private Spatial PlacePlatform(String type)
    {
        PackedScene platformBlock;
        Spatial platformBlockI;

        platformBlock = (PackedScene)ResourceLoader
                .Load("res://assets/platforms/" + type + ".tscn");

        platformBlockI = (Spatial)platformBlock.Instance();

        platformBlockI.Translation = G.DirectionCalc();
        Vector3 temp = platformBlockI.Translation;
        temp.y = -16;
        platformBlockI.Translation = temp;

        platformsSpace.AddChild(platformBlockI);

        return platformBlockI;
    }

    private void RemoveOldPlatforms()
    {
        int childIndex = totalPlatforms - history;
        Spatial child = platformsSpace.GetChild<Spatial>(childIndex);

        totalPlatforms--;
        child.GetNode<AnimationPlayer>("Spatial/AnimationPlayer").Play("Down");
    }
}