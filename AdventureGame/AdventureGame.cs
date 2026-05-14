using System;
using System.IO;
using System.Text.Json;

namespace AdventureGame;

public class Coord { public int row { get; set; } = 0; public int col { get; set; } = 0; }
public class DungeonData {
    public Coord adventurerStart { get; set; } = new();
    public Coord exit { get; set; } = new();
    public Coord grueStart { get; set; } = new();
    public Coord lamp { get; set; } = new();
    public Coord key { get; set; } = new();
    public Coord chest { get; set; } = new();
}

public class AdventureGame {
    private Adventurer adventurer = new();
    private Room[,] dungeon = new Room[8, 8];
    private int aRow, aCol, gRow, gCol, eRow, eCol;
    private bool isChestOpen, isAdventureAlive, hasReachedExit, hasQuit;
    private string gameLog = "";

    public void Start() {
        do {
            Init();
            while (isAdventureAlive && !hasReachedExit && !hasQuit) {
                Console.Clear();
                DrawMap();
                Console.WriteLine($"\nLOG: {gameLog}");
                Console.WriteLine($"INV: {(adventurer.HasKey() ? "[Llave] " : "")}{(adventurer.HasLamp() ? "[Lámpara]" : "")}");
                Console.Write("\n(WASD) Mover, (K) Tomar Llave, (L) Tomar Lámpara, (O) Abrir, (Q) Salir: ");
                
                string input = Console.ReadLine()?.ToUpper() ?? "";
                if (input == "Q") { hasQuit = true; break; }
                
                ProcessInput(input);
                UpdateGrue(); 
                
                if (isChestOpen && aRow == eRow && aCol == eCol) hasReachedExit = true;
                if (aRow == gRow && aCol == gCol) { isAdventureAlive = false; gameLog = "¡El Grue te atrapó en la oscuridad!"; }
            }
            FinalizeGame();
        } while (RetryMenu());
    }

    private void Init() {
        adventurer = new Adventurer();
        for (int r = 0; r < 8; r++) {
            for (int c = 0; c < 8; c++) {
                dungeon[r, c] = new Room();
                bool isPath = (r == 0 || r == 7 || c == 0 || c == 7 || r == 4 || c == 4);
                dungeon[r, c].SetDescription(isPath ? "Pasillo" : "Pared");
            }
        }
        LoadDungeonConfig("dungeon.json");
        isChestOpen = false; isAdventureAlive = true; hasReachedExit = false;
        gameLog = "Está muy oscuro... Busca la Lámpara [L]";
    }

    private void LoadDungeonConfig(string path) {
        if (File.Exists(path)) {
            var data = JsonSerializer.Deserialize<DungeonData>(File.ReadAllText(path));
            if (data != null) {
                aRow = data.adventurerStart.row; aCol = data.adventurerStart.col;
                gRow = data.grueStart.row; gCol = data.grueStart.col;
                eRow = data.exit.row; eCol = data.exit.col;
                dungeon[data.key.row, data.key.col].SetKey(true);
                dungeon[data.key.row, data.key.col].SetDescription("Pasillo");
                dungeon[data.lamp.row, data.lamp.col].SetLamp(true);
                dungeon[data.lamp.row, data.lamp.col].SetDescription("Pasillo");
                dungeon[data.chest.row, data.chest.col].SetChest(true);
                dungeon[data.chest.row, data.chest.col].SetDescription("Pasillo");
            }
        }
    }

    private void DrawMap() {
        Console.WriteLine("╔════════════════════════╗");
        for (int r = 0; r < 8; r++) {
            Console.Write("║ ");
            for (int c = 0; c < 8; c++) {
                if (r == aRow && c == aCol) Console.Write(" P ");
                else if (isChestOpen && r == gRow && c == gCol) Console.Write(" G ");
                else if (!adventurer.HasLamp()) {
                    if (dungeon[r, c].HasLamp()) Console.Write(" L ");
                    else Console.Write("   "); // Niebla de guerra
                }
                else {
                    if (r == eRow && c == eCol) Console.Write(" E ");
                    else if (dungeon[r, c].HasKey()) Console.Write(" K ");
                    else if (dungeon[r, c].HasChest() && !isChestOpen) Console.Write(" C ");
                    else if (dungeon[r, c].GetDescription() == "Pared") Console.Write("###");
                    else Console.Write(" . ");
                }
            }
            Console.WriteLine(" ║");
        }
        Console.WriteLine("╚════════════════════════╝");
    }

    private void ProcessInput(string input) {
        int nR = aRow, nC = aCol;
        if (input == "W") nR--; else if (input == "S") nR++; else if (input == "A") nC--; else if (input == "D") nC++;

        if (input == "K" && dungeon[aRow, aCol].HasKey()) {
            adventurer.SetKey(true); dungeon[aRow, aCol].SetKey(false);
            gameLog = "¡Tienes la LLAVE!"; return;
        }
        if (input == "L" && dungeon[aRow, aCol].HasLamp()) {
            adventurer.SetLamp(true); dungeon[aRow, aCol].SetLamp(false);
            gameLog = "¡Luz activada! Ahora busca el cofre."; return;
        }
        if (input == "O" && dungeon[aRow, aCol].HasChest()) {
            if (adventurer.HasKey()) { 
                isChestOpen = true; gameLog = "¡ABIERTO! ¡CORRE A LA SALIDA [E]!"; 
            } else { 
                gameLog = "¡NECESITAS LA LLAVE [K]!"; 
            }
            return;
        }

        if (nR >= 0 && nR < 8 && nC >= 0 && nC < 8 && dungeon[nR, nC].GetDescription() == "Pasillo") {
            aRow = nR; aCol = nC;
            if (!adventurer.HasLamp() && (aRow > 0 && aRow < 7 && aCol > 0 && aCol < 7)) {
                isAdventureAlive = false; gameLog = "Te perdiste en las sombras...";
            }
        }
    }

    private void UpdateGrue() {
        if (!isChestOpen) return;
        int dR = aRow - gRow, dC = aCol - gCol;
        if (Math.Abs(dR) >= Math.Abs(dC)) { if (!TryM(dR, 0)) TryM(0, dC); }
        else { if (!TryM(0, dC)) TryM(dR, 0); }
    }

    private bool TryM(int dR, int dC) {
        int nR = gRow + (dR == 0 ? 0 : dR > 0 ? 1 : -1);
        int nC = gCol + (dC == 0 ? 0 : dC > 0 ? 1 : -1);
        if (nR >= 0 && nR < 8 && nC >= 0 && nC < 8 && dungeon[nR, nC].GetDescription() == "Pasillo") {
            gRow = nR; gCol = nC; return true;
        }
        return false;
    }

    private void FinalizeGame() {
        Console.Clear(); DrawMap();
        if (hasReachedExit) Console.WriteLine("\n¡VICTORIA! Escapaste.");
        else Console.WriteLine($"\nPERDISTE: {gameLog}");
    }

    private bool RetryMenu() {
        if (hasQuit) return false;
        Console.Write("\n¿Jugar de nuevo? (S/N): ");
        return Console.ReadLine()?.ToUpper() == "S";
    }
}
