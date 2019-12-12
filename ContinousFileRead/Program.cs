using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace ContinousFileRead
{
    internal class Program
    {
        private static string ConnectionLog =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam", "logs",
                "connection_log.txt");

        private static string RegexOk => @"RecvMsgClientLogOnResponse\(\) : \[U:\d:\d+\] 'OK'";
        private static string RegexId => @"\[U:\d:\d+\]";

        // private static FileSystemWatcherUtil watcherUtil;

        private static void Main(string[] args)
        {
            Tail tail = new Tail(ConnectionLog)
            {
                //LevelRegex = prms.LevelRegex,
                LineFilter = RegexOk
            };

            tail.Changed += tail_Changed;
            tail.Run();

            Console.ReadKey();
        }

        private static void tail_Changed(object sender, Tail.TailEventArgs e)
        {
            Console.WriteLine(e.Line);
        }
    }
}