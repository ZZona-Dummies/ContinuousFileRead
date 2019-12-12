using System;
using System.IO;
using System.Text.RegularExpressions;

namespace ContinousFileRead
{
    internal class Program
    {
        private static string ConnectionLog =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam", "logs",
                "connection_log.txt");

        // private static FileSystemWatcherUtil watcherUtil;

        private static void Main(string[] args)
        {
            //FileSystemWatcherUtil.Create(ConnectionLog, LastLines_Callback);

            Tail tail = new Tail(ConnectionLog);
            //{
            //    LevelRegex = prms.LevelRegex,
            //    LineFilter = prms.Filter
            //};

            tail.Changed += tail_Changed;
            tail.Run();

            Console.ReadKey();
        }

        private static void tail_Changed(object sender, Tail.TailEventArgs e)
        {
            Console.WriteLine(e.Line);
        }

        private static void LastLines_Callback(string[] obj)
        {
            Console.WriteLine(string.Join(Environment.NewLine, obj));
        }

        //public static void Follow(string path)
        //{
        //    long previousSize = 0;

        //    // Note the FileShare.ReadWrite, allowing others to modify the file
        //    using (FileStream fileStream = File.Open(path, FileMode.Open,
        //        FileAccess.Read, FileShare.ReadWrite))
        //    {
        //        //fileStream.Seek(0, SeekOrigin.End);
        //        //using (StreamReader streamReader = new StreamReader(fileStream))
        //        //{
        //        //    for (; ; )
        //        //    {
        //        //        // Substitute a different timespan if required.
        //        //        Thread.Sleep(TimeSpan.FromSeconds(0.5));

        //        //        // Write the output to the screen or do something different.
        //        //        // If you want newlines, search the return value of "ReadToEnd"
        //        //        // for Environment.NewLine.
        //        //        Console.Out.Write(streamReader.ReadToEnd());
        //        //    }
        //        //}

        //        fileStream.Seek(previousSize, SeekOrigin.Begin);
        //        //read, display, ...

        //        using (var reader = new StreamReader(fileStream))
        //        {
        //        }

        //        previousSize = fileStream.Length;
        //    }
        //}
    }

    public sealed class FileSystemWatcherUtil : IDisposable
    {
        public FileSystemWatcher Watcher { get; private set; }
        public AddedContentReader Reader { get; private set; }

        public static FileSystemWatcherUtil Instance { get; private set; }

        private FileSystemWatcherUtil()
        {
        }

        public static void Create(string filePath, Action<string[]> lastLinesCallback)
        {
            if (lastLinesCallback == null)
                throw new ArgumentNullException(nameof(lastLinesCallback));

            CreateInstance(filePath, null);

            void ChangedCallback(object sender, FileSystemEventArgs e)
            {
                // TODO: If we don't call CreateInstance, Instance is null?
                var lines = Instance.Reader.GetLastLines();
                lastLinesCallback(lines);
            }

            Create(filePath, ChangedCallback);
        }

        public static void Create(string filePath, Action<object, FileSystemEventArgs> changedCallback)
        {
            if (changedCallback == null)
                throw new ArgumentNullException(nameof(changedCallback));

            string directory = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileName(filePath);

            var watcher = new FileSystemWatcher(directory ?? throw new InvalidOperationException())
            {
                Filter = fileName,
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
            };
            watcher.Changed += (sender, e) => changedCallback(sender, e);

            if (Instance == null)
                CreateInstance(filePath, watcher);
            else
                Instance.Watcher = watcher;
        }

        private static void CreateInstance(string filePath, FileSystemWatcher watcher)
        {
            Instance = new FileSystemWatcherUtil
            {
                Watcher = watcher,
                Reader = new AddedContentReader(filePath)
            };
        }

        public void Dispose()
        {
            Watcher.Dispose();
            Reader.Dispose();
        }
    }

    public sealed class AddedContentReader : IDisposable
    {
        private readonly FileStream _fileStream;
        private readonly StreamReader _reader;

        //Start position is from where to start reading first time. consequent read are managed by the Stream reader
        public AddedContentReader(string fileName, long startPosition = 0)
        {
            //Open the file as FileStream
            _fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            _reader = new StreamReader(_fileStream);
            //Set the starting position
            _fileStream.Position = startPosition;
        }

        //Get the current offset. You can save this when the application exits and on next reload
        //set startPosition to value returned by this method to start reading from that location
        public long CurrentOffset => _fileStream.Position;

        //Returns the lines added after this function was last called
        public string GetAddedLines()
        {
            return _reader.ReadToEnd();
        }

        public string[] GetLastLines()
            => Regex.Split(GetAddedLines(), "\r\n|\r|\n");

        public void Dispose()
        {
            _reader.Close();
            _reader.Dispose();

            _fileStream.Close();
            _fileStream.Dispose();
        }
    }
}