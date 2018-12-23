using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMEngineCore
{
    public class CSVManager
    {
        public static CSVManager Instance = new CSVManager();

        private CSVManager() { }

        public void GenerateCSVFile (List<string> inputs, string filename)
        {
            System.IO.StreamWriter tw = null;
            try
            {
                tw = new System.IO.StreamWriter(filename, false);

                foreach(string line in inputs)
                {
                    tw.WriteLine(line);
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (tw != null)
                    tw.Close();
            }
        }

        public bool LaunchCSVFile(string filename)
        {
            if (File.Exists(filename))
            {
                Task.Factory.StartNew(() => Process.Start(filename));
                return true;
            }else
            {
                //log file not exist
                return false;
            }
        }
    }
}
