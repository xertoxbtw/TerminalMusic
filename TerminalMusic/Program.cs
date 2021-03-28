using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace TerminalMusic
{
    class Program
    {
        public static bool ExeRunning = true;
        public static Thread AudioSystem;
        public static byte Flag = 0;

        public static string Path = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

        public static float Volume = 1f;
        public static bool Playing = false;
        public static TimeSpan PlayingPos = new TimeSpan();
        public static TimeSpan PlayingLen = new TimeSpan();
        public static int SongSelection = 0;
        public static List<Entry> Songs = new List<Entry>();
        public static long PartLen = 0;

        public static int WinX = Console.WindowWidth;
        public static int WinY = Console.WindowHeight;

        public static int Selected = 0;
        public static int SelectedYSize;

        public static int Offset = 0;
        public static int TopUISize = 8;
        public static bool NoMusic = true;
        static void Main(string[] args)
        {
            Load();
            AudioSystem = new Thread(PlayAudio);
            AudioSystem.Start();
            Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight);
            Flag = 1;
            while (ExeRunning)
            {
                //System.Diagnostics.Debug.WriteLine(AudioSystem.ThreadState);
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
                if (Console.KeyAvailable)
                    KeyDown(Console.ReadKey(true).Key);
            }
        }
        public static void Load()
        {
            Songs.Clear();
            DirectoryInfo folderpath = new DirectoryInfo(Path);
            FileInfo[] Files = folderpath.GetFiles("*.mp3");
            foreach (FileInfo file in Files)
            {
                Songs.Add(new Entry(file.Name.Replace(file.Extension, ""), file.FullName));
            }
            Files = folderpath.GetFiles("*.wav");
            foreach (FileInfo file in Files)
            {
                Songs.Add(new Entry(file.Name.Replace(file.Extension, ""), file.FullName));
            }
            Files = null;
        }

        public static void Draw()
        {
            try
            {
                // Create Buffer Array and Fill
                char[,] Buffer = new char[Console.WindowWidth, Console.WindowHeight];
                SelectedYSize = Console.WindowHeight - TopUISize - 1;

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
                    Buffer[i, TopUISize - 1] = '═';
                }
                Buffer[0, TopUISize - 1] = '╠';
                Buffer[Console.WindowWidth - 1, TopUISize - 1] = '╣';
                // Ui TopBar Text

                string topTextUI = "Current Song: " + Songs[SongSelection].Name;
                for (int i = 0; i < topTextUI.Length; i++)
                {
                    Buffer[i + 1, 2] = topTextUI[i];
                }


                topTextUI = "Volume: " + Math.Round(Volume * 100) + "%";
                for (int i = 0; i < topTextUI.Length; i++)
                {
                    Buffer[i + 1, 3] = topTextUI[i];
                }

                /*Playing Status
                if (NoMusic == false)
                {
                    topTextUI = PlayingPos+"/"+PlayingLen;
                    for (int i = 0; i < topTextUI.Length; i++)
                    {
                        Buffer[i + 1, 4] = topTextUI[i];
                    }
                }*/


                // Song Selection
                if (SelectedYSize > Songs.Count)
                {
                    SelectedYSize -= (SelectedYSize - Songs.Count);
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
                        Buffer[b + 1, i + TopUISize] = SongBuffer[b];
                    }
                }

                // Draw To Screen
                Console.SetCursorPosition(0, 0);
                string DisplayBuffer = "";
                for (int i = 0; i < Console.WindowHeight; i++)
                {
                    DisplayBuffer = DisplayBuffer + new string(CustomArray<char>.GetColumn(Buffer, i));
                }
                Console.Write(DisplayBuffer);
                Flag = 0;
                Console.Title = "TerminalMusic";
            }
            catch
            {
                Console.Title = "Window is to Small to Draw";
            }

        }

        public static void KeyDown(ConsoleKey key)
        {
            // Switch Audio Playing
            if (key == ConsoleKey.Spacebar)
            {
                if (NoMusic)
                {
                    NoMusic = true;
                    Playing = true;
                }
                else
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
            }
            // Scroll Down
            else if (key == ConsoleKey.DownArrow)
            {
                if (Selected < Songs.Count - 1)
                    if (Selected == SelectedYSize - 1)
                    {
                        Offset += 1;
                        Selected++;
                    }
                    else
                        Selected++;
                Flag = 1;
            }
            // Scroll Up
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
            // Select Song
            else if (key == ConsoleKey.Enter)
            {
                if (NoMusic)
                {
                    NoMusic = false;
                }
                SongSelection = Selected;
                Playing = true;
                Flag = 1;
            }
            // Volume Plus
            else if (key == ConsoleKey.P)
            {
                if (Volume < 1f)
                {
                    Volume += 0.05f;
                }
                Flag = 1;
            }
            // Volume Minus
            else if (key == ConsoleKey.M)
            {
                if (Volume > 0f)
                {
                    Volume -= 0.05f;
                }
                Flag = 1;
            }
            else if (key == ConsoleKey.F5)
            {
                Load();
                Flag = 1;
            }
        }

        public static void PlayAudio()
        {
            int oldID = -1;
            while (ExeRunning)
            {
                while (NoMusic) { }
                using (var audioFile = new AudioFileReader(Songs[SongSelection].Path))
                using (var outputDevice = new WaveOutEvent())
                {
                    outputDevice.Init(audioFile);
                    PlayingLen = audioFile.TotalTime;
                    PlayingPos = new TimeSpan(0);
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
                        PlayingPos = new TimeSpan(outputDevice.GetPosition());

                        outputDevice.Volume = Volume;
                    }
                    oldID = SongSelection;
                    audioFile.Dispose();
                    outputDevice.Dispose();
                }
            }
        }
    }
}