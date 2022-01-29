using Godot;
using System;

public class Decorations : Spatial
{
    private Globals Globals;

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

        if (Globals.firstMove)
        {
            blockPosition = Globals.playerPosition;
        }
        else
        {
            blockPosition = Globals.GetFuturePosition();
        }

        blockPosition = _CalculatePosition(blockPosition);

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
        SpatialMaterial newHue = Globals.GetEmissionMaterial(0.05f);

        MeshInstance meshInstance = blockInstance.GetNode<MeshInstance>("MeshInstance");
        CPUParticles cpuParticles = blockInstance.GetNode<CPUParticles>("CPUParticles");

        cpuParticles.Mesh.SurfaceSetMaterial(0, newHue);
        meshInstance.SetSurfaceMaterial(0, newHue);

        return blockInstance;
    }

    private Vector3 _CalculatePosition(Vector3 centerPosition)
    {
        int rangeX;
        int rangeZ;

        int randomShort = Globals.GetRandomNumber(16, 24);
        int randomWide = Globals.GetRandomNumber(24);

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
