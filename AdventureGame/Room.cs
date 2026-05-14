namespace AdventureGame;

public class Room {
    public bool hasKey = false;
    public bool hasChest = false;
    public bool hasLamp = false;
    public string description = "Pared";

    public bool HasKey() => hasKey;
    public void SetKey(bool b) => hasKey = b;
    public bool HasChest() => hasChest;
    public void SetChest(bool b) => hasChest = b;
    public bool HasLamp() => hasLamp;
    public void SetLamp(bool b) => hasLamp = b;
    public string GetDescription() => description;
    public void SetDescription(string d) => description = d;
}
