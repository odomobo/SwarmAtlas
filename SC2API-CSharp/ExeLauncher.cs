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
        // injected
        public required GameConfig GameConfig { protected get; init; }

        public void StartSC2Instance(int port, bool fullscreen = false)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo(GameConfig.StarcraftExe);
            processStartInfo.Arguments = string.Format("-listen {0} -port {1} -displayMode {2} -dataVersion {3}",
                GameConfig.Address, 
                port,
                fullscreen ? 1 : 0,
                GameConfig.DataVersion
            );
            processStartInfo.WorkingDirectory = Path.Combine(GameConfig.StarcraftDir, "Support64");
            Process.Start(processStartInfo);
        }
    }
}
