using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Linq;

namespace TerminalMusic
{
    class Program
    {
        public static bool ExeRunning = true;
        public static Thread AudioSystem;
        public static byte Flag = 0;


        public static float Volume = 1f;
        public static bool Playing = false;
        public static TimeSpan PlayingPos = new TimeSpan();
        public static TimeSpan PlayingLen = new TimeSpan();
        public static int SongSelection = 0;
        public static List<Entry> Songs = new List<Entry>();
        public static TimeSpan PartLen = new TimeSpan();

        public static uint RenderPart = 0;

        public static int WinX = Console.WindowWidth;
        public static int WinY = Console.WindowHeight;

        public static int Selected = 0;
        public static int SelectedYSize;

        public static int Offset = 0;
        public static Stopwatch Stopwatch;

        static void Main(string[] args)
        {
            Stopwatch = new Stopwatch();
            Load();
            AudioSystem = new Thread(PlayAudio);
            AudioSystem.Start();
            Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight);
            Flag = 1;
            while (ExeRunning)
            {
                if (WinX != Console.WindowWidth || WinY != Console.WindowHeight)
                {
                    try
                    {
                        Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight);
                        Flag = 1;
                        WinX = Console.WindowWidth;
                        WinY = Console.WindowHeight;
                    }
                    catch
                    {
                        Flag = 1;
                    }
                }

                Console.CursorVisible = false;
                if (Flag == 1)
                    Draw();
                if (Console.KeyAvailable & Flag == 0)
                    KeyDown(Console.ReadKey(true).Key);
            }
        }
        public static void Load()
        {
            Songs.Clear();
            DirectoryInfo folderpath = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
            FileInfo[] Files0 = folderpath.GetFiles("*.mp3");
            foreach (FileInfo file in Files0)
            {
                Songs.Add(new Entry(file.Name, file.FullName));
            }
            FileInfo[] Files1 = folderpath.GetFiles("*.wav");
            foreach (FileInfo file in Files1)
            {
                Songs.Add(new Entry(file.Name, file.FullName));
            }
        }

        public static void Draw()
        {
            Stopwatch.Restart();
            char[,] Buffer = new char[Console.WindowWidth, Console.WindowHeight];
            SelectedYSize = Console.WindowHeight - 6;

            for (int y = 0; y < Console.WindowHeight; y++)
            {
                for (int x = 0; x < Console.WindowWidth; x++)
                {
                    Buffer[x, y] = ' ';
                }
            }


            // Border
            for (int y = 0; y < Console.WindowHeight - 1; y++)
            {
                Buffer[0, y] = '║';
                Buffer[Console.WindowWidth - 1, y] = '║';
            }
            for (int i = 0; i < Console.WindowWidth; i++)
            {
                Buffer[i, 1] = '═';
            }
            Buffer[0, 1] = '╔';
            Buffer[Console.WindowWidth - 1, 1] = '╗';

            for (int i = 0; i < Console.WindowWidth; i++)
            {
                Buffer[i, Console.WindowHeight - 1] = '═';
            }

            Buffer[0, Console.WindowHeight - 1] = '╚';
            Buffer[Console.WindowWidth - 1, Console.WindowHeight - 1] = '╝';

            for (int i = 0; i < Console.WindowWidth; i++)
            {
                Buffer[i, 4] = '═';
            }
            Buffer[0, 4] = '╠';
            Buffer[Console.WindowWidth - 1, 4] = '╣';
            // Text

            string playnowtext = "Current Song: " + Songs[SongSelection].Name;
            for (int i = 0; i < playnowtext.Length; i++)
            {
                Buffer[i + 1, 2] = playnowtext[i];
            }
            // Song Selection
            if (SelectedYSize > Songs.Count)
            {
                SelectedYSize -= (SelectedYSize - Songs.Count);
                System.Diagnostics.Debug.WriteLine(SelectedYSize);
            }
            for (int i = 0; i < SelectedYSize; i++)
            {
                string SongBuffer = "";
                if (i == Selected - Offset)
                {
                    if (i + Offset < Songs.Count)
                    {
                        SongBuffer = "->" + Songs[i + Offset].Name;
                    }
                }
                else
                {
                    if (i + Offset < Songs.Count)
                    {
                        SongBuffer = Songs[i + Offset].Name;
                    }
                }
                for (int b = 0; b < SongBuffer.Length; b++)
                {
                    Buffer[b + 1, i + 5] = SongBuffer[b];

                }
            }
            Console.SetCursorPosition(0, 0);
            string DisplayBuffer = "";
            for (int i = 0; i < Console.WindowHeight; i++)
            {
                DisplayBuffer = DisplayBuffer + new string(CustomArray<char>.GetColumn(Buffer, i));
            }
            Console.Write(DisplayBuffer);
            Flag = 0;
            Console.Title = "TerminalMusic";

            /*catch
            {
                Console.Title = "Window is to Small to Draw";
            }*/
            Debug.WriteLine("Time: " + Stopwatch.ElapsedMilliseconds);
        }

        public static void KeyDown(ConsoleKey key)
        {
            if (key == ConsoleKey.Spacebar)
            {
                if (Playing == true)
                {
                    Playing = false;
                }
                else
                {
                    Playing = true;
                }
            }
            else if (key == ConsoleKey.DownArrow)
            {
                if (Selected < Songs.Count)
                    if (Selected == SelectedYSize - 1)
                    {
                        Offset += 1;
                        Selected++;
                    }
                    else
                        Selected++;
                Flag = 1;
            }
            else if (key == ConsoleKey.UpArrow)
            {
                if (Selected > 0 + Offset)
                    Selected -= 1;
                else if (Offset > 0)
                {
                    Offset -= 1;
                    Selected -= 1;
                }

                Flag = 1;
            }
            else if (key == ConsoleKey.Enter)
            {
                SongSelection = Selected;
                Playing = true;
                Flag = 1;
            }
            else if (key == ConsoleKey.P)
            {
                if (Volume < 1f)
                {
                    Volume += 0.05f;
                }
            }
            else if (key == ConsoleKey.M)
            {
                if (Volume > 0f)
                {
                    Volume -= 0.05f;
                }
            }
        }


        public static void PlayAudio()
        {
            int oldID = -1;
            while (ExeRunning)
            {
                using (var audioFile = new AudioFileReader(Songs[SongSelection].Path))
                using (var outputDevice = new WaveOutEvent())
                {
                    outputDevice.Init(audioFile);
                    PlayingLen = audioFile.TotalTime;
                    PartLen = PlayingLen / 10;
                    while (SongSelection == oldID)
                    {
                        if (Playing == true)
                        {
                            outputDevice.Play();
                        }
                        else
                        {
                            outputDevice.Stop();
                        }
                        outputDevice.Volume = Volume;
                        if (RenderPart < PlayingLen / PartLen)
                        {
                            RenderPart = Convert.ToUInt32(PlayingLen / PartLen);
                            //Flag = 1;
                        }
                    }
                    oldID = SongSelection;
                    audioFile.Dispose();
                    outputDevice.Dispose();
                }
            }
        }
    }

    public static class CustomArray<T>
    {
        public static T[] GetColumn(T[,] matrix, int columnNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(0))
                    .Select(x => matrix[x, columnNumber])
                    .ToArray();
        }

        public static T[] GetRow(T[,] matrix, int rowNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(1))
                    .Select(x => matrix[rowNumber, x])
                    .ToArray();
        }
    }

    public class Entry
    {
        public string Name;
        public string Path;
        public Entry(string _name, string _path)
        {
            Name = _name;
            Path = _path;
        }
    }
}
