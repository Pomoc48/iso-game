using Godot;
using System;

public class Globals : Node
{
    private Random rnd = new Random();
    private ConfigFile CF = new ConfigFile();
    
    public int fullHealth = 24;
    public int fullMove = 20;

    public float playerHealth = 24.0f;
    public bool firstMove = true;

    public int camRotIndex = 3;
    public int sessionScore = 0;
    public int animDirection;
    public int highScore;

    public String prevBlock;
    public Vector3 playerPosition;

    public SpatialMaterial emissionRed;
    public SpatialMaterial emissionBlue;
    public SpatialMaterial emissionWhite;

    public int[] pMoves;

    private int startDir;

    public override void _Ready()
    {
        emissionRed = (SpatialMaterial)GD.Load("res://materials/emission2.tres");
        emissionBlue = (SpatialMaterial)GD.Load("res://materials/emission.tres");
        // emissionWhite = (SpatialMaterial)GD.Load("res://materials/emission3.tres");
    }

    public void ResetVars()
    {
        playerHealth = fullHealth;
        firstMove = true;
        camRotIndex = 3;
        sessionScore = 0;
    }

    public void Save(String category, int value)
    {
        CF.SetValue("Main", category, value);
        CF.Save("user://config");
    }

    public int Load(String category)
    {
        if (CF.Load("user://config") != Error.Ok) Save("HighScore", 0);
        int number = (int) CF.GetValue("Main", category, 0);
        return number;
    }

    public bool RandomBool()
    {
        // Bigger range for better randomness
	    var foo = rnd.Next(100);

        if (foo < 50) return false;
        return true;
    }

    // Translate directions to vectors
    public Vector3 DirectionCalc()
    {
        Vector3 vect = playerPosition;

        switch (animDirection)
        {
            case 1:
                vect.z += fullMove;
                break;

            case 2:
                vect.x += -fullMove;
                break;

            case 3:
                vect.z += -fullMove;
                break;

            default:
                vect.x += fullMove;
                break;
        }
            
        return vect;
    }

    public bool IsMoveLegal()
    {
        if (firstMove)
        {
            if (animDirection == startDir)
            {
                firstMove = false;
                return true;
            }

            return false;
        }

        foreach (int n in pMoves)
        {
            if (animDirection == n)
            {
                return true;
            }
        }

        return false;
    }

    public int GetPlatformType()
    {
        var foo = rnd.Next(100);

        if (foo < 10) return 3;
        if (foo >= 10 && foo < 40) return 4;
        if (foo >= 40 && foo < 75) return 1;
        return 2;

        // diff 1
        // cs / tw / ll / cc
        // 10 / 30 / 35 / 25

        // diff 2
        // cs / tw / ll / cc
        // 5  / 30 / 25 / 40

        // diff 3
        // cs / tw / ll / cc
        // 2  / 20 / 18 / 60
    }

    public int RetranslateDirection(int direction)
    {
        // (Clockwise)
        if (camRotIndex != 3) direction -= (camRotIndex + 1);

        // Reverse overflow check
		if (direction < 0) direction += 4;

	    return direction;
    }

	public Vector3 DecorationsCalc(Vector3 centerPos)
    {
        int rangeS;
        int rangeS2;

        if (RandomBool())
        {
            rangeS = rnd.Next(16, 24);
            rangeS2 = rnd.Next(24);
        }
        else
        {
            rangeS2 = rnd.Next(16, 24);
            rangeS = rnd.Next(24);
        }

        if (RandomBool()) centerPos.x += rangeS;
        else centerPos.x -= rangeS;

        if (RandomBool()) centerPos.z += rangeS2;
        else centerPos.z -= rangeS2;

        centerPos.y = 2;
        return centerPos;
    }

    // Get new cycle lenght for camera rotation
    public int GetMaxCycle(int cycle, int range)
    {
        int temp;

        if (RandomBool()) temp = cycle + rnd.Next(range);
        else temp = cycle - rnd.Next(range);

        return temp;
    }

    public int GenerateStartingPos()
    {
        startDir = rnd.Next(4);
        return startDir;
    }

    public int DetermineRotationAmmount()
    {
        var foo = rnd.Next(100);

        if (foo < 25)
        {
            if (foo < 12) return 3;
            return 2;
        }
        return 1;
    }
}