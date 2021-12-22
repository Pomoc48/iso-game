using Godot;
using System;

public class PlayerVariables : Node
{
    private Random rnd = new Random();
    
    public int fullHealth = 24;
    public int fullMove = 20;

    public float playerHealth = 24.0f;
    public bool firstMove = true;

    public int camRotIndex = 3;
    public int sessionScore = 0;
    public int animDirection;
    public int prevDirection;

    public String prevBlock;
    public Vector3 playerPosition;

    public void ResetVars()
    {
        playerHealth = fullHealth;
        firstMove = true;
        camRotIndex = 3;
        sessionScore = 0;
    }

    public bool RandomBool()
    {
        // Bigger range for better randomness
	    var foo = rnd.Next(100);

        if (foo < 50) return false;
        else return true;
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
            firstMove = false;
            return true;
        }
            
        else
        {
            // Checking for wrong moves
            switch (animDirection)
            {
                case 1:
                    return !CheckMatch("Corner1", "Corner2", "Long0", 3);

                case 2:
                    return !CheckMatch("Corner2", "Corner3", "Long1", 0);

                case 3:
                    return !CheckMatch("Corner0", "Corner3", "Long0", 1);

                default:
                    return !CheckMatch("Corner0", "Corner1", "Long1", 2);
            }
        }
    }

    private bool CheckMatch(String c1, String c2, String l, int dir)
    {
        String[] corners = {c1, c2, l};

        // Backtracking check
        foreach (String i in corners)
        {
            if (prevBlock == i) return true;
        }
            
        if (prevDirection == dir) return true;
        else return false;
    }

    public int RetranslateDirection(int direction)
    {
        // (Clockwise)
        if (camRotIndex != 3)
        {
		    direction -= (camRotIndex + 1);
        }

        // Reverse overflow check
		if (direction < 0) direction += 4;

	    return direction;
    }

	public int DecorationsCalc()
    {
        bool pos = RandomBool();
        
        // Add safe margin around the player
        int temp = rnd.Next(7, 18);
        if (!pos) temp *= -1;

        return temp;
    }

    // Get new cycle lenght for camera rotation
    public int GetMaxCycle(int cycle, int range)
    {
        int temp;

        if (RandomBool()) temp = cycle + rnd.Next(range);
        else temp = cycle - rnd.Next(range);

        return temp;
    }
}