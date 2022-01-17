using Godot;
using System;

public class Level : Spatial
{
    private Globals G;

    private Spatial platformsSpace;
    private Spatial decorationsSpace;

    private int history = 4;
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

    int[,,] twowayMoves = {
        {{1, 3}, {0, 1}, {0, 3}},
        {{0, 2}, {2, 1}, {0, 1}},
        {{1, 3}, {3, 2}, {1, 2}},
        {{2, 0}, {0, 3}, {3, 2}},
    };

    // Init function
    public override void _Ready()
    {
        G = GetNode<Globals>("/root/Globals");

        platformsSpace = GetNode<Spatial>("Platforms");
	    decorationsSpace = GetNode<Spatial>("Decorations");
        
        // Rotate starting platform
        float rotation = G.GenerateStartingPlatformPos();

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
        String blockPath;

        if (G.perspectiveMode) blockPath = "res://scenes/BlockM.tscn";
        else blockPath = "res://scenes/Block.tscn";

        PackedScene block = (PackedScene)ResourceLoader.Load(blockPath);
                
        Spatial blockI = (Spatial)block.Instance();
        blockI.Translation = blockPos;
        
        decorationsSpace.AddChild(blockI);
        totalDeco++;
    }

    public void GeneratePlatform()
    {
        int platformType = G.GetPlatformType();

        switch (platformType)
        {
            case 1:
                PlatformLong();
                break;

            case 2:
                platformCorner();
                break;

            case 3:
                PlatformCross();
                break;
            
            case 4:
                PlatformTwoWay();
                break;
        }

        totalPlatforms++;

        if (totalPlatforms < history) return;
        RemoveOldPlatforms();
    }

    private void PlatformLong()
    {
        Spatial platformBlockI;
        platformBlockI = PlacePlatform("Long");

        // Rotate by 90 when animDirection uneven
        if (G.animDirection % 2 != 0)
        {
            Vector3 rotationL = new Vector3(0, 90, 0);
            platformBlockI.RotationDegrees =  rotationL;
        }

        G.pMoves = new int[1]{G.animDirection};
    }

    private void platformCorner()
    {
        Spatial platformBlockI;
        platformBlockI = PlacePlatform("Corner");

        bool doReverse = G.RandomBool();
        int reverse = 0;

        // Change direction of the corner to the other side
        if (doReverse) reverse++;
        float yRot = (G.animDirection + reverse) * -90;

        Vector3 rotationV = new Vector3(0, yRot, 0);
        platformBlockI.RotationDegrees =  rotationV;

        // Check direction change
        int foo = doReverse ? 0 : 1;
        G.pMoves = new int[1]{cornerMoves[G.animDirection, foo]};
    }

    private void PlatformCross()
    {
        Spatial platformBlockI;
        platformBlockI = PlacePlatform("Cross");

        // Only opposite direction is removed from the array
        G.pMoves = new int[3];
        for (int i = 0; i < 3; i++)
        {
            G.pMoves[i] = crossMoves[G.animDirection, i];
        }
    }

    private void PlatformTwoWay()
    {
        Spatial platformBlockI;
        platformBlockI = PlacePlatform("TwoWay");

        // Get new random orientation of the platform
        Random rnd = new Random();
        int side = rnd.Next(3);

        float yRotT = G.animDirection * -90;

        if (side == 1) yRotT += 90;
        if (side == 2) yRotT += -90;

        Vector3 rotationT = new Vector3(0, yRotT, 0);
        platformBlockI.RotationDegrees =  rotationT;

        // Get valid moves based on new rotation
        G.pMoves = new int[2];
        for (int i = 0; i < 2; i++)
        {
            G.pMoves[i] = twowayMoves[G.animDirection, side, i];
        }
    }

    private Spatial PlacePlatform(String type)
    {
        PackedScene platformBlock;
        Spatial platformBlockI;
        String platformPath;

        if (G.perspectiveMode)
            platformPath = "res://scenes/platformsM/"+type+"M.tscn";
        else platformPath = "res://scenes/platforms/"+type+".tscn";

        platformBlock = (PackedScene)ResourceLoader.Load(platformPath);

        platformBlockI = (Spatial)platformBlock.Instance();

        // Get platform future pos
        platformBlockI.Translation = G.DirectionCalc();

        // Starting animation fix
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