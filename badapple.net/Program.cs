using System.Diagnostics;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;
using System.Drawing;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Threading;

namespace badapple.net
{
    class Program
    {
        static void Log(string msg, bool critical = false, bool warning = false)
        {
            var color = ConsoleColor.Cyan;

            if (critical)
            {
                color = ConsoleColor.Red;
            } else if (warning)
            {
                color = ConsoleColor.Yellow;
            }

            Console.ForegroundColor = color;
            Console.WriteLine($"Log: {msg}");
            Console.ResetColor();
        }

        static void ClrScr()
        {
            Console.Clear();
        }

        static void PrepareDisplay()
        {
            ClrScr();
            Console.BackgroundColor = ConsoleColor.White;
            ClrScr();
            Console.ForegroundColor = ConsoleColor.Black;
        }

        public static async Task Main(string[] args)
        {
            string ytdl = "yt-dlp.exe";
            string ffmpeg = "ffmpeg.exe";
            string badapplePath = "【東方】Bad Apple!! ＰＶ【影絵】 [FtutLA63Cp8].mp4";
            string replacementPath = "standard.mp4";

            Log("Checking for YTDL and FFMPeg");

            bool ytdlExists = File.Exists(ytdl);
            bool ffmpegExists = File.Exists(ffmpeg);
            bool badappleExists = File.Exists(replacementPath);

            if (!ytdlExists && !ffmpegExists)
            {
                Log("Warning: Could not find ffmpeg or ytdl within this programs local path. Installing...", warning: true);

                Log("Downloading YTDl...");
                await Utils.DownloadYtDlp();

                Log("Downloading FFmPeg...");
                await Utils.DownloadFFmpeg();
            }

            if (!badappleExists)
            {
                Log("Downloading Bad Apple MP4...");
                await DownloadVideoAsync();

                File.Move(badapplePath, replacementPath);
            } else
            {
                Log("Skipping download as Bad Apple already exists in the programs local path...");
            }

            Log("Attempting to decompile frames... (This may take a while!)");

            string frameStore = "frames/";

            badapplePath = replacementPath;

            Directory.CreateDirectory(frameStore);
            Log("Out directory created.");
            
            DecompileFrames(badapplePath);

            Log("Preparing display...");
            DisplayFrames(frameStore);
        }

        public static async Task DownloadVideoAsync()
        {
            string url = "https://www.youtube.com/watch?v=FtutLA63Cp8";
            
            var ytdl = new YoutubeDL();
            var video = await ytdl.RunVideoDownload(url, recodeFormat: VideoRecodeFormat.Mp4);

            bool dlResult = video.Success;
            int failDelay = 1 * 1000;

            if (!dlResult)
            {
                Log("Failure downloading the Bad Apple MP4. Exiting...", true);
                await Task.Delay(failDelay);
                return;
            }

            Log($"Video downloaded. Save location: {video.Data}");
            
        }

        public static void DecompileFrames(string path)
        {
            string args = $"-i {path} -vf fps=1 frames/out%d.png";
            Process ffmpegProc = new Process();
            ffmpegProc.StartInfo = new ProcessStartInfo("ffmpeg", args);
            try
            {
                ffmpegProc.Start();
                ffmpegProc.WaitForExit();
                ClrScr();
            } catch(Exception e)
            {
                Log($"Something went wrong!\nMessage: {e.Message}", critical: true);
            }
            return;
        }

        public static void DisplayFrames(string framesPath)
        {
            PrepareDisplay();

            string[] frameFiles = Directory.GetFiles(framesPath);
            int frameCount = 0;

            for (int i = 0; i < frameFiles.Length; i++)
            {

                StringBuilder sb = new StringBuilder();
                string frameFile = frameFiles[i];

                if (frameFile.Contains(".png"))
                {

                    int displayWidth = Console.LargestWindowWidth;
                    int displayHeight = Console.LargestWindowHeight;

                    Bitmap original = (Bitmap)Image.FromFile(frameFile);
                    Bitmap image = new Bitmap(original, new Size(displayWidth, displayHeight));

                    int width = image.Width;
                    int height = image.Height;

                    Console.SetWindowSize(displayWidth, displayHeight);
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            Color pixel = image.GetPixel(x, y);
                            
                            int r, g, b;

                            r = pixel.R;
                            g = pixel.G;
                            b = pixel.B;

                            double grayPixel = ToGrayScale(r, g, b);
                            string caret = PixelToAscii(grayPixel);

                            sb.Append(caret);
                        }

                        sb.Append("\n");
                    }
                    Console.WriteLine(sb);
                    sb.Clear();
                }

                Thread.Sleep(1000);
                ClrScr();
            }

            Log($"Frame count: {frameCount}");

            ClrScr();
            PrepareDisplay();
            
        }

        public static double ToGrayScale(int r, int g, int b)
        {
            double grayR = 0.299 * r;
            double grayG = 0.587 * g;
            double grayB = 0.114 * b;

            double grayScale = (grayR + grayG + grayB) / 3;
            return grayScale;
        }

        public static string PixelToAscii(double grayness)
        {
            StringBuilder sb = new StringBuilder();

            if (grayness <= 10)
            {
                sb.Append("∙");
            }
            else if (grayness <= 20)
            {
                sb.Append("*");
            }
            else if (grayness <= 30)
            {
                sb.Append("∙");
            }
            else if (grayness <= 40)
            {
                sb.Append("$");
            }
            else if (grayness <= 50)
            {
                sb.Append("%");
            }
            else if (grayness <= 60)
            {
                sb.Append("^");
            }
            else if (grayness <= 70)
            {
                sb.Append("\"");
            }
            else if (grayness <= 80)
            {
                sb.Append("@");
            }
            else if (grayness <= 100)
            {
                sb.Append(">");
            }
            else if (grayness <= 120)
            {
                sb.Append(".");
            }
            else if (grayness <= 130)
            {
                sb.Append("+");
            }
            else if (grayness <= 140)
            {
                sb.Append("-");
            }
            else if (grayness <= 150)
            {
                sb.Append("_");
            }
            else
            {
                sb.Append("!");
            }
            return sb.ToString();
        }
    }
}