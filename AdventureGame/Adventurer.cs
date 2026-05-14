namespace AdventureGame;

public class Adventurer {
    private bool hasKey = false;
    private bool hasLamp = false;

    public bool HasKey() => hasKey;
    public void SetKey(bool b) => hasKey = b;
    public bool HasLamp() => hasLamp;
    public void SetLamp(bool b) => hasLamp = b;
}
