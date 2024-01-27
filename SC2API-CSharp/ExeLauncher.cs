using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SC2API.CSharp
{
    public class ExeLauncher
    {
        private readonly GameConfig _gameConfig;

        public ExeLauncher(GameConfig gameConfig)
        {
            _gameConfig = gameConfig;
        }

        public void StartSC2Instance(int port)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo(_gameConfig.StarcraftExe);
            processStartInfo.Arguments = string.Format("-listen {0} -port {1} -displayMode 0 -dataVersion {2}", _gameConfig.Address, port, _gameConfig.DataVersion);
            processStartInfo.WorkingDirectory = Path.Combine(_gameConfig.StarcraftDir, "Support64");
            Process.Start(processStartInfo);
        }
    }
}
