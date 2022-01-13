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
    public bool perspectiveMode = false;

    public int camRotIndex = 3;
    public int sessionScore = 0;
    public int animDirection;
    public int highScore;

    public int correctMoves;
    public int totalMoves;

    public String prevBlock;
    public Vector3 playerPosition;

    public SpatialMaterial emissionRed;
    public SpatialMaterial emissionBlue;

    public int[] pMoves;

    public String[] categoriesP = {
        "HighScore",
        "NumberOfGames", 
        "CombinedScore",
        "CorrectMoves",
        "TotalMoves"
    };

    private int startDir;
    private int cyclesCount;
    private int platformDifficulty;

    public override void _Ready()
    {
        String matPath = "res://materials/emission";
        emissionRed = (SpatialMaterial)GD.Load(matPath + "2.tres");
        emissionBlue = (SpatialMaterial)GD.Load(matPath + ".tres");
    }

    public void ResetVars()
    {
        playerHealth = fullHealth;
        firstMove = true;
        perspectiveMode = false;
        camRotIndex = 3;
        sessionScore = 0;
        platformDifficulty = 0;
        cyclesCount = 0;
        correctMoves = 0;
        totalMoves = 0;
    }

    public void Save(String[] categories, int[] values)
    {
        for (int n = 0; n < categories.Length; n++)
        {
            CF.SetValue("Main", categories[n], values[n]);
        }
        
        CF.Save("user://config");
    }

    public int Load(String category)
    {
        if (CF.Load("user://config") != Error.Ok)
        {
            int[] values = {0, 0, 0, 0, 0};
            Save(categoriesP, values);
        }

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

                correctMoves++;
                totalMoves++;

                return true;
            }

            return false;
        }

        totalMoves++;

        foreach (int n in pMoves)
        {
            if (animDirection == n)
            {
                correctMoves++;
                return true;
            }
        }

        return false;
    }

    public int GetPlatformType()
    {
        var foo = rnd.Next(100);

        // cs / tw /  l /  c
        switch (platformDifficulty)
        {
            case 2:
                // 2  / 20 / 18 / 60
                if (foo < 2) return 3;
                if (foo >= 2 && foo < 22) return 4;
                if (foo >= 22 && foo < 40) return 1;
                return 2;

            case 1:
                // 5  / 30 / 25 / 40
                if (foo < 5) return 3;
                if (foo >= 5 && foo < 35) return 4;
                if (foo >= 35 && foo < 60) return 1;
                return 2;

            default:
                // 10 / 30 / 35 / 25
                if (foo < 10) return 3;
                if (foo >= 10 && foo < 40) return 4;
                if (foo >= 40 && foo < 75) return 1;
                return 2;
        }
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
        cyclesCount++;

        // Increase platform difficulty every ~100 points 
        if (cyclesCount > 5)
        {
            cyclesCount = 0;
            if (platformDifficulty < 2) platformDifficulty++;
        }

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

        if (foo < 30)
        {
            if (foo < 5) return 3;
            return 2;
        }
        return 1;
    }
}