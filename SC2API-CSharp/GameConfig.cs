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

        public bool Realtime { get; set; } = true;

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
    }
}
