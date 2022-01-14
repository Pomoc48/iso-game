using Godot;
using System;

public class Statistics : TextureButton
{
    private Globals G;
    private Interface I;

    private Label statsText;

    private Texture openTexture;
    private Texture closeTexture;
    
    private bool statsOpened = false;

    private int numberOfGamesF;
    private int combinedScoreF;
    private int correctMovesF;
    private int totalMovesF;

    public override void _Ready()
    {
        G = GetNode<Globals>("/root/Globals");
        I = GetNode<Interface>("/root/Level/Interface");

        String path2 = "/root/Level/Interface/Main/Stats/Stats";
        statsText = GetNode<Label>(path2);

        openTexture = (Texture)GD.Load("res://assets/textures/Stats.png");
        closeTexture = (Texture)GD.Load("res://assets/textures/Close.png");

        LoadStatistics();
    }

    private void LoadStatistics()
    {
        numberOfGamesF = G.Load(G.categoriesP[1]);
        combinedScoreF = G.Load(G.categoriesP[2]);
        correctMovesF = G.Load(G.categoriesP[3]);
        totalMovesF = G.Load(G.categoriesP[4]);

        float percentageF = 0f;

        if (totalMovesF != 0)
        {
            percentageF = correctMovesF / (float) totalMovesF;
            percentageF *= 100; 
        }

        String percentageS = percentageF.ToString("F");

        String games = "Number of games: " + numberOfGamesF + "\n";
        String score = "Combined score: " + combinedScoreF + "\n";
        String percentage = "Correct move percentage: " + percentageS + "%\n";
        String moves = "Total moves: " + totalMovesF + "\n";

        statsText.Text = games + score + percentage + moves;
    }

    public void UploadStatistics()
    {
        int games = numberOfGamesF + 1;
        int score = combinedScoreF + G.sessionScore;
        int cMoves = correctMovesF + G.correctMoves;
        int moves = totalMovesF + G.totalMoves;

        int[] values = {G.highScore, games, score, cMoves, moves};

        G.Save(G.categoriesP, values);
    }

    private void _on_StatsButton_button_down()
    {
        if (statsOpened)
        {
            I.ShowStatistics(false);
            this.TextureNormal = openTexture;

            statsOpened = false;
            return;
        }

        I.ShowStatistics(true);
        this.TextureNormal = closeTexture;

        statsOpened = true;
    }
}