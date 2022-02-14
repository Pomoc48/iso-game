using Godot;
using System;

public enum PlafformDifficulty
{
    Easy,
    Medium,
    Hard,
}

public enum PlaftormType
{
    Long,
    Corner,
    Cross,
    Twoway
}

public enum Direction
{
    RightUp,
    RightDown,
    LeftDown,
    LeftUp
}

public class Globals : Node
{
    public Vector3 playerPosition;

    public SpatialMaterial materialBlue;

    public Texture openTexture;
    public Texture closeTexture;

    public Direction cameraRotation;
    public Direction animationDirection;
    public Direction startingDirection;
    public int[] possibleMoves;

    public PlafformDifficulty platformDifficulty;

    public Color emissionColor;

    public readonly int FULL_HEALTH = 24;
    public readonly int FIVE_SEC_IN_FRAMES = 300;

    public bool firstMove;
    public bool perspectiveMode;

    public float playerHealth;
    public float animationSpeed = 0.25f;

    public int sessionScore;
    public int highScore;
    public int correctMoves;
    public int totalMoves;

    public String[] categoriesArray = {
        "HighScore",
        "NumberOfGames", 
        "CombinedScore",
        "CorrectMoves",
        "TotalMoves"
    };
    
    private Random _random = new();
    private ConfigFile _configFile = new();

    private int cyclesCount;
    private readonly int _FULL_MOVE = 20;
    private readonly float _CHANGE_HUE_BY = 0.0016f;

    public override void _Ready()
    {
        String materialPath = "res://materials/emission";
        materialBlue = (SpatialMaterial)GD.Load(materialPath + ".tres");

        openTexture = (Texture)GD.Load("res://assets/textures/Stats.png");
        closeTexture = (Texture)GD.Load("res://assets/textures/Close.png");

        emissionColor = materialBlue.Emission;
    }

    public void NewGame()
    {
        firstMove = true;
        perspectiveMode = false;
        
        playerHealth = FULL_HEALTH;
        sessionScore = 0;
        cyclesCount = 0;
        correctMoves = 0;
        totalMoves = 0;

        animationSpeed = 0.25f;

        playerPosition = new();
        cameraRotation = Direction.LeftUp;
        platformDifficulty = PlafformDifficulty.Easy;
    }

    public void SaveStats(String[] categories, int[] values)
    {
        for (int n = 0; n < categories.Length; n++)
        {
            _configFile.SetValue("Main", categories[n], values[n]);
        }
        
        _configFile.Save("user://config");
    }

    public int LoadStats(String category)
    {
        if (_configFile.Load("user://config") != Error.Ok)
        {
            int[] values = {0, 0, 0, 0, 0};
            SaveStats(categoriesArray, values);
        }

        int number = (int) _configFile.GetValue("Main", category, 0);
        return number;
    }

    public bool GetRandomBool()
    {
        int chance = _random.Next(100);

        return chance switch
        {
            < 50 => false,
            _ => true
        };
    }

    public int GetRandomNumber(int range)
    {
        return _random.Next(range);
    }

    public int GetRandomNumber(int min, int max)
    {
        return _random.Next(min, max);
    }

    public Vector3 GetFuturePosition()
    {
        Vector3 position = playerPosition;

        position.y = 0;

        switch (animationDirection)
        {
            case Direction.RightDown:
                position.z += _FULL_MOVE;
                break;

            case Direction.LeftDown:
                position.x += -_FULL_MOVE;
                break;

            case Direction.LeftUp:
                position.z += -_FULL_MOVE;
                break;

            case Direction.RightUp:
                position.x += _FULL_MOVE;
                break;
        }
            
        return position;
    }

    public int GetNextCycle(int cycle)
    {
        if ((cyclesCount += 1) > 5)
        {
            cyclesCount = 0;

            if (platformDifficulty < PlafformDifficulty.Hard)
            {
                platformDifficulty++;
            }
        }

        return cycle + _random.Next(5);
    }

    public SpatialMaterial GetEmissionMaterial(float offset)
    {
        SpatialMaterial newHue = new SpatialMaterial();

        newHue.EmissionEnabled = true;
        newHue.Emission = emissionColor;

        Color hueOffset = newHue.Emission;

        hueOffset.h += offset;
        
        if (perspectiveMode)
        {
            hueOffset.h += 0.5f;
        }

        hueOffset.h = _CheckCap(hueOffset.h);

        newHue.Emission = hueOffset;
        return newHue;
    }

    private float _CheckCap(float value)
    {
        if (value > 1)
        {
            value -= 1;
        }

        if (value < 0)
        {
            value += 1;
        }

        return value;
    }

    public void UpdateEmissionMaterial()
    {
        if ((emissionColor.h += _CHANGE_HUE_BY) > 1)
        {
            emissionColor.h = 0;
        }

        emissionColor.v = 1;
    }
}
