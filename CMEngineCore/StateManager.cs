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

        public static void Save()
        {
            string saveDir = ConfigurationManager.AppSettings["SaveFolder"];
            DirectoryInfo dir = new DirectoryInfo(saveDir);

            if (!dir.Exists) Directory.CreateDirectory(saveDir);

            try
            {

                if (dir.Exists)
                {
                    foreach (FileInfo file in dir.GetFiles())
                        file.Delete();
                }

                TradeManager.Instance.Save(string.Format("{0}\\{1}", saveDir, TradeManager.DataFile));
                ParentOrderManager.Instance.Save(string.Format("{0}\\{1}", saveDir, ParentOrderManager.DataFile));
                Log.Info("Status saved");
            }
            catch (Exception ex)
            {
                Log.Error("Fialed to save the state. Error: " + ex.Message);
                Log.Error(ex.StackTrace);
            }
        }

        public static void Resume()
        {
            string saveDir = ConfigurationManager.AppSettings["SaveFolder"];
            var tradeMgr = TradeManager.

        }

    }
}
