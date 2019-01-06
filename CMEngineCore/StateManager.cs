using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;

namespace CMEngineCore
{
    public class StateManager
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void Archive()
        {
            string savdDir = ConfigurationManager.AppSettings["SaveFolder"];
            DirectoryInfo dir = new DirectoryInfo(savdDir);
            if (!dir.Exists) Directory.CreateDirectory(savdDir);

            if (!Directory.Exists(string.Format("{0}\\{1}", savdDir, "archive")))
                Directory.CreateDirectory(string.Format("{0}\\{1}", savdDir, "archive"));

            foreach(FileInfo file in dir.GetFiles())
            {
                string newName = string.Format("{0}\\{1}\\{2}", savdDir, "archive", Path.GetFileNameWithoutExtension(file.Name) + "_" + DateTime.Now.Ticks + ".dat");
                File.Copy(file.FullName, newName);
            }
        }

    }
}
