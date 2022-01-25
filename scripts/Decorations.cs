using Godot;
using System;

public class Decorations : Spatial
{
    private Globals Globals;
    private Random _random = new();

    public override void _Ready()
    {
        Globals = GetNode<Globals>("/root/Globals");
    }

    public void Create()
    {
        Vector3 blockPosition = _GetPosition();
        _Load(blockPosition);
    }

    private Vector3 _GetPosition()
    {
        Vector3 blockPosition = new();

        if (Globals.firstMove) // Idle animation position fix
        {
            blockPosition = Globals.playerPosition;
        }
        else
        {
            blockPosition = Globals.GetFuturePosition();
        }

        // Add random offset
        blockPosition = CalculatePosition(blockPosition);

        return blockPosition;
    }

    private void _Load(Vector3 blockPosition)
    {
        String blockPath = "res://scenes/Block.tscn";
        PackedScene block = (PackedScene)ResourceLoader.Load(blockPath);
                
        Spatial blockInstance = (Spatial)block.Instance();
        blockInstance.Translation = blockPosition;

        blockInstance = _Recolor(blockInstance);
        this.AddChild(blockInstance);
    }

    private Spatial _Recolor(Spatial blockInstance)
    {
        SpatialMaterial newHue = new();

        newHue.EmissionEnabled = true;
        newHue.Emission = Globals.emissionColor;

        MeshInstance meshInstance = blockInstance.GetNode<MeshInstance>("MeshInstance");
        CPUParticles cpuParticles = blockInstance.GetNode<CPUParticles>("CPUParticles");

        cpuParticles.Mesh.SurfaceSetMaterial(0, newHue);
        meshInstance.SetSurfaceMaterial(0, newHue);

        return blockInstance;
    }

    private Vector3 CalculatePosition(Vector3 centerPosition)
    {
        int rangeX;
        int rangeZ;

        int randomShort = _random.Next(16, 24);
        int randomWide = _random.Next(24);

        if (Globals.GetRandomBool())
        {
            rangeX = randomShort;
            rangeZ = randomWide;
        }
        else
        {
            rangeX = randomWide;
            rangeZ = randomShort;
        }

        if (Globals.GetRandomBool())
        {
            centerPosition.x += rangeX;
        }
        else
        {
            centerPosition.x -= rangeX;
        }

        if (Globals.GetRandomBool())
        {
            centerPosition.z += rangeZ;
        }
        else
        {
            centerPosition.z -= rangeZ;
        }

        centerPosition.y = 2 + Globals.platformHeight;

        return centerPosition;
    }
}
