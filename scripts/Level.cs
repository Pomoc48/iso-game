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

    int[,] cornerMoves = {
        {3, 1},
        {0, 2},
        {1, 3},
        {2, 0},
    };

    int[,] crossMoves = {
        {3, 0, 1},
        {1, 0, 2},
        {3, 1, 2},
        {3, 0, 2},
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

                if (G.animDirection % 2 != 0)
                {
                    Vector3 rotationL = new Vector3(0, 90, 0);
                    platformBlockI.RotationDegrees =  rotationL;
                }

                G.pMoves = new int[1]{G.animDirection};
                break;

            case 2:

                platformBlockI = PlacePlatform("Corner");
                bool doReverse = G.RandomBool();

                if (doReverse) reverse++;
                float yRot = (G.animDirection + reverse) * -90;

                Vector3 rotationV = new Vector3(0, yRot, 0);
                platformBlockI.RotationDegrees =  rotationV;

                if (doReverse)
                {
                    G.pMoves = new int[1]{cornerMoves[G.animDirection, 0]};
                    break;
                }

                G.pMoves = new int[1]{cornerMoves[G.animDirection, 1]};
                break;

            case 3:

                platformBlockI = PlacePlatform("Cross");

                G.pMoves = new int[3];
                for (int i = 0; i < 3; i++)
                {
                    G.pMoves[i] = crossMoves[G.animDirection, i];
                }

                break;
        }

        totalPlatforms++;

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