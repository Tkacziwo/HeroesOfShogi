public class CityBuilding
{
    public CityBuilding()
    {
    }
    public CityBuilding(CityBuilding other)
    {
        this.name = other.name;
        this.level = other.level;
        this.maxLevel = other.maxLevel;
        this.producedResource = other.producedResource;
        this.producedAmount = other.producedAmount;
        this.producedUnits = other.producedUnits;
    }

    public CityBuilding(string name)
    {
        this.name = name;
    }

    public string name;

    public uint level = 0;

    public uint maxLevel;

    public WorldResource? producedResource;

    public uint producedAmount;

    public string[] producedUnits;
}