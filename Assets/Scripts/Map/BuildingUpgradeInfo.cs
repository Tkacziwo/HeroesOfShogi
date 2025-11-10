public class BuildingUpgradeInfo
{
    public string Name;

    public uint level;

    public uint amount;
    
    public RequiredResources requiredResources = new();
}

public class RequiredResources
{
    public uint wood;

    public uint stone;

    public uint gold;

    public uint goldResin;
}