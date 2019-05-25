using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityUtilities.ExecutionOrder.ExecutionOrderControl;

namespace UnityUtilities
{
    [AutoInit]
    [ExecutionOrder(ExecutionOrderValue.Early)]
    public class LogFile : MonoBehaviour
    {
        public enum FileNames
        {
            server,
            serverObjectLog,
            client,
            clientObjectLog
        }

        Dictionary<FileNames, StreamWriter> streamWriters = new Dictionary<FileNames, StreamWriter>();

        static LogFile instance;

        private void Awake()
        {
            instance = this;
        }

        public static void WriteLineToLog(FileNames file, string line)
        {
            instance?._WriteLineToLog(file, line);
        }

        private void _WriteLineToLog(FileNames file, string line)
        {
            EnsureStream(file);

            StreamWriter sw = streamWriters[file];
            lock(sw)
            {
                sw.WriteLine("[" + DateTime.Now.ToString() + "]");
                sw.WriteLine("\t" + line);
            }
        }

        public static void WriteLinesToLog(FileNames file, IEnumerable<string> lines)
        {
            instance?._WriteLinesToLog(file, lines);
        }

        private void _WriteLinesToLog(FileNames file, IEnumerable<string> lines)
        {
            EnsureStream(file);

            StreamWriter sw = streamWriters[file];

            lock(sw)
            {
                sw.WriteLine("[" + DateTime.Now.ToString() + "]");
                foreach(var line in lines)
                {
                    sw.WriteLine("\t" + line);
                }
            }
        }

        private void EnsureStream(FileNames file)
        {
            if(!streamWriters.ContainsKey(file))
            {
                StreamWriter sw = File.AppendText(file.ToString() + ".log");

                lock(sw)
                {
                    streamWriters[file] = sw;

                    sw.WriteLine("");
                    sw.WriteLine("");
                    sw.WriteLine("");
                    sw.WriteLine("");
                    sw.WriteLine("");
                    sw.WriteLine("[" + DateTime.Now.ToString() + "]");
                    sw.WriteLine("----------------------------------------------------------" + "    Session Started    " + "----------------------------------------------------------");
                    sw.WriteLine("");
                }
            }
        }

        private void OnDestroy()
        {
            foreach(var writer in streamWriters)
            {
                writer.Value.Close();
            }
        }
    }
}
