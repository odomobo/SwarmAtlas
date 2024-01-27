using SC2APIProtocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SC2API.CSharp
{
    public record GameVersion(string Version, string DataVersion, string Path);

    public class GameConfig
    {
        public GameConfig() {
            findSc2InstallPath();
        }

        public readonly string Address = "127.0.0.1";

        public string StarcraftDir;
        public string StarcraftExe;
        public string DataVersion => _gameVersion.DataVersion;

        private GameVersion _gameVersion = new GameVersion("4.10.0", "B89B5D6FA7CBF6452E721311BFBC6CB2", "Base75689");

        private void findSc2InstallPath()
        {
            string myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string executeInfo = Path.Combine(myDocuments, "Starcraft II", "ExecuteInfo.txt");
            if (!File.Exists(executeInfo))
                throw new Exception("Unable to find ExecuteInfo.txt at " + executeInfo);

            string[] lines = File.ReadAllLines(executeInfo);
            foreach (string line in lines)
            {
                string argument = line.Substring(line.IndexOf('=') + 1).Trim();
                if (line.Trim().StartsWith("executable"))
                {
                    StarcraftDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(argument)));
                    StarcraftExe = Path.Combine(StarcraftDir, "Versions", _gameVersion.Path, "SC2_x64.exe");
                }
            }
        }

        public void ParseClArgs(string[] args)
        {
            for (int i = 0; i < args.Count(); i += 2)
            {
                if (args[i] == "-g" || args[i] == "--GamePort")
                    GamePort = int.Parse(args[i + 1]);
                else if (args[i] == "-o" || args[i] == "--StartPort")
                    StartPort = int.Parse(args[i + 1]);
                else if (args[i] == "-l" || args[i] == "--LadderServer")
                    LadderServer = args[i + 1];
                else if (args[i] == "--OpponentId")
                    OpponentID = args[i + 1];
                else if (args[i] == "-c" || args[i] == "--ComputerOpponent")
                {
                    if (ComputerRace == Race.NoRace)
                        ComputerRace = Race.Random;
                    if (ComputerDifficulty == Difficulty.Unset)
                        ComputerDifficulty = Difficulty.VeryHard;
                    i--;
                }
                else if (args[i] == "-a" || args[i] == "--ComputerRace")
                {
                    if (args[i + 1] == "Protoss")
                        ComputerRace = Race.Protoss;
                    else if (args[i + 1] == "Terran")
                        ComputerRace = Race.Terran;
                    else if (args[i + 1] == "Zerg")
                        ComputerRace = Race.Zerg;
                    else if (args[i + 1] == "Random")
                        ComputerRace = Race.Random;
                }
                else if (args[i] == "-d" || args[i] == "--ComputerDifficulty")
                {
                    if (args[i + 1] == "VeryEasy")
                    {
                        ComputerDifficulty = Difficulty.VeryEasy;
                    }
                    if (args[i + 1] == "Easy")
                    {
                        ComputerDifficulty = Difficulty.Easy;
                    }
                    if (args[i + 1] == "Medium")
                    {
                        ComputerDifficulty = Difficulty.Medium;
                    }
                    if (args[i + 1] == "MediumHard")
                    {
                        ComputerDifficulty = Difficulty.MediumHard;
                    }
                    if (args[i + 1] == "Hard")
                    {
                        ComputerDifficulty = Difficulty.Hard;
                    }
                    if (args[i + 1] == "Harder")
                    {
                        ComputerDifficulty = Difficulty.Harder;
                    }
                    if (args[i + 1] == "VeryHard")
                    {
                        ComputerDifficulty = Difficulty.VeryHard;
                    }
                    if (args[i + 1] == "CheatVision")
                    {
                        ComputerDifficulty = Difficulty.CheatVision;
                    }
                    if (args[i + 1] == "CheatMoney")
                    {
                        ComputerDifficulty = Difficulty.CheatMoney;
                    }
                    if (args[i + 1] == "CheatInsane")
                    {
                        ComputerDifficulty = Difficulty.CheatInsane;
                    }

                    ComputerDifficulty = Difficulty.Easy;
                }
            }
        }

        public int GamePort { get; set; } = 5678;
        public int StartPort { get; set; }
        public string LadderServer { get; set; }
        public Race ComputerRace { get; set; } = Race.NoRace;
        public Difficulty ComputerDifficulty { get; set; } = Difficulty.Unset;
        public string OpponentID { get; set; }
        public bool Realtime { get; set; } = true;
    }
}
