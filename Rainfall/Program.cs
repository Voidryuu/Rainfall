using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Rainfall
{
    class Program
    {
        private static readonly Random random = new Random();
        private static readonly object ConsoleWriterLock = new object();
        private static List<ConsoleColor> ConsoleColors = Enum.GetValues(typeof(ConsoleColor)).Cast<ConsoleColor>().ToList();
        private static readonly ConsoleColor originalForegroundColor = Console.ForegroundColor;
        private static readonly int originalX = 0;// Console.CursorTop;
        private static readonly int originalY = Console.CursorLeft;
        private static readonly int maxX = Console.WindowWidth - 1 - originalX;
        private static readonly int maxY = Console.WindowHeight - 1 - originalY;

        private static Timer clearWaveStartTextTimer;
        private static Timer rainfallAnimationTimer;
        private static Timer addRainTimer;
        private static Thread handleInputThread;


        private static readonly string instructions = @" ██▀███   ▄▄▄       ██▓ ███▄    █   █████▒▄▄▄       ██▓     ██▓    
▓██ ▒ ██▒▒████▄    ▓██▒ ██ ▀█   █ ▓██   ▒▒████▄    ▓██▒    ▓██▒    
▓██ ░▄█ ▒▒██  ▀█▄  ▒██▒▓██  ▀█ ██▒▒████ ░▒██  ▀█▄  ▒██░    ▒██░    
▒██▀▀█▄  ░██▄▄▄▄██ ░██░▓██▒  ▐▌██▒░▓█▒  ░░██▄▄▄▄██ ▒██░    ▒██░    
░██▓ ▒██▒ ▓█   ▓██▒░██░▒██░   ▓██░░▒█░    ▓█   ▓██▒░██████▒░██████▒
░ ▒▓ ░▒▓░ ▒▒   ▓▒█░░▓  ░ ▒░   ▒ ▒  ▒ ░    ▒▒   ▓▒█░░ ▒░▓  ░░ ▒░▓  ░
  ░▒ ░ ▒░  ▒   ▒▒ ░ ▒ ░░ ░░   ░ ▒░ ░       ▒   ▒▒ ░░ ░ ▒  ░░ ░ ▒  ░
  ░░   ░   ░   ▒    ▒ ░   ░   ░ ░  ░ ░     ░   ▒     ░ ░     ░ ░   
   ░           ░  ░ ░           ░              ░  ░    ░  ░    ░  ░
                                                                   
             It's a letter rain invasion!
        Kill the raindrops by typing their name. 
      Avoid touching the rain using the arrow keys.
  

Controls:                  |     Extra Options: 
  [tab] to change enemy    |       -s to use small characters 
  [esc] to quit the game   |       -t to use text as rain
 
                 Press [space] to start!";

        private static bool gameover = false;
        private static Raindrop player;
        private static List<Raindrop> rain = new List<Raindrop>();
        private static Raindrop raindropInFocus;
        private static int wave = 0;
        private static string waveText = "";
        private static int score = 0;
        private static string scoreText;
        private static int scoreY = originalY + 1;
        private static int health = 3;
        private static string healthText;
        private static int healthY = originalY;
        private static int nrOfRaindrops = 3;
        private static readonly int maxNrOfRaindrops = 20;

        private static readonly int waveStartInterval = 1000;
        private static int rainfallSpeedInterval = 600;
        private static int raindropAdditionInterval = 2000;

        private static bool useLargeChars = true;
        private static bool useTextAsRain = false;

        static void Main(string[] args)
        {
            SetProgramOptions(args);
            SetConsoleProperties();

            InitPlayer();
            InitRain();
            InitAnimationTimers();

            DrawInstructions();
            DrawGround();
            DrawPlayer();
            DrawScore();
            DrawHealth();

            StartNewWave();
            StartInputLoop();

            if (gameover)
            {
                RestoreConsoleProperties();
            }
        }

        private static void SetProgramOptions(string[] args)
        {
            foreach (string arg in args)
            {
                if (arg == "-s") { useLargeChars = false; }
                if (arg == "-t") { useTextAsRain = true; }
            }
        }

        private static void SetConsoleProperties()
        {
            Console.CursorVisible = false;
            ConsoleColors.Remove(Console.BackgroundColor);
            ConsoleColors.Remove(ConsoleColor.Red);

            /* debugging...
            Console.WriteLine($"originalX: {originalX}");
            Console.WriteLine($"originalY: {originalY}");
            Console.WriteLine($"Console.WindowWidth: {Console.WindowWidth}");
            Console.WriteLine($"Console.WindowHeight: {Console.WindowHeight}");*/
        }

        private static void InitPlayer()
        {
            string text = "o";
            LargeText largeText = Characters.GetLargeText(text);
            int playerX = useLargeChars ? maxX / 2 - largeText.Width / 2 : maxX / 2;
            int playerY = useLargeChars ? maxY - largeText.Height : maxY - 1;
            player = new Raindrop(text, largeText, playerX, playerY, ConsoleColors[random.Next(ConsoleColors.Count)]);
        }

        private static void InitRain()
        {
            for (int i = 0; i < maxNrOfRaindrops; i++)
            {
                string text = useTextAsRain ? Characters.GetRandomText() : Characters.GetRandomChar().ToString();
                LargeText largeText = Characters.GetLargeText(text);
                int x = useLargeChars ? random.Next(originalX, maxX - largeText.Width) : random.Next(originalX, maxX - text.Length);
                rain.Add(new Raindrop(text, largeText, x, originalY, ConsoleColors[random.Next(ConsoleColors.Count)]));
            }
        }

        private static void InitAnimationTimers()
        {
            clearWaveStartTextTimer = new Timer(ClearWaveStartText, null, waveStartInterval, Timeout.Infinite); 
            rainfallAnimationTimer = new Timer(RainfallAnimation, null, waveStartInterval, rainfallSpeedInterval);
            addRainTimer = new Timer(AddRain, null, waveStartInterval, raindropAdditionInterval);
        }

        private static void DrawInstructions()
        {
            string[] instructionLines = instructions.Split(Environment.NewLine);
            int width = 0;
            foreach (string line in instructionLines) { if (line.Length > width) width = line.Length; }
            WriteLinesAt(instructionLines, maxX / 2 - width / 2, maxY / 2 - instructionLines.Length / 2, originalForegroundColor);
            ConsoleKeyInfo input;
            do { input = Console.ReadKey(true); } 
            while (input.Key != ConsoleKey.Spacebar);
            ClearInstructions();
        }

        private static void ClearInstructions()
        {
            string[] instructionLines = instructions.Split(Environment.NewLine);
            string[] spaces = new string[instructionLines.Length];
            int width = 0;
            for (int lineIndex = 0; lineIndex < instructionLines.Length; lineIndex++)
            {
                string line = instructionLines[lineIndex];
                if (line.Length > width) width = line.Length;
                for (int charIndex = 0; charIndex < line.Length; charIndex++)
                {
                    spaces[lineIndex] += " ";
                }
            }
            WriteLinesAt(spaces, maxX / 2 - width / 2, maxY / 2 - instructionLines.Length / 2, originalForegroundColor);
        }

        private static void DrawGround()
        {
            string ground = "";
            for (int i = 0; i < maxX; i++) { ground += "-"; }
            WriteAt(ground, originalX, maxY);
        }

        private static void DrawPlayer()
        {
            Draw(player);
        }

        private static void DrawScore()
        {
            scoreText = $"score: {score}";
            WriteAt(scoreText, maxX - scoreText.Length - 1, scoreY);
        }

        private static void DrawHealth()
        {
            healthText = "health: ";
            for (int i = 0; i < health; i++) { healthText += "♥"; }
            WriteAt(healthText, maxX - healthText.Length - 1, healthY);
        }

        private static void StartNewWave()
        {
            // reset rain previous wave
            for (int i = 0; i < nrOfRaindrops; i++)
            {
                Raindrop raindrop = rain[i];
                if (raindrop.State == RaindropState.Visible) { DrawSpaces(raindrop); }
                raindrop.State = RaindropState.Nonexisting;
                raindrop.Text = useTextAsRain ? Characters.GetRandomText() : Characters.GetRandomChar().ToString();
                raindrop.LargeText = Characters.GetLargeText(raindrop.Text);
                raindrop.X = useLargeChars ? random.Next(originalX, maxX - raindrop.LargeText.Width) : random.Next(originalX, maxX - raindrop.Text.Length);
                raindrop.Y = originalY;
                raindrop.Color = ConsoleColors[random.Next(ConsoleColors.Count)];
                raindrop.PressedPart = null;
            }

            // increase difficulty
            if (useTextAsRain)
            {
                int nrOfRaindropsToAdd = wave == 0 ? 0 :
                                         wave < 5 ? 1 : 2;
                nrOfRaindrops = Math.Min(nrOfRaindrops + nrOfRaindropsToAdd, maxNrOfRaindrops);
                rainfallSpeedInterval -= rainfallSpeedInterval > 500 ? 50 : 0;
                raindropAdditionInterval -= raindropAdditionInterval > 1500 ? 200 :
                                            raindropAdditionInterval > 1000 ? 100 : 0;
            } 
            else
            {
                int nrOfRaindropsToAdd = wave == 0 ? 0 :
                                         wave < 5 ? 2 : 3;
                nrOfRaindrops = Math.Min(nrOfRaindrops + nrOfRaindropsToAdd, maxNrOfRaindrops);
                rainfallSpeedInterval -= rainfallSpeedInterval > 200 ? 100 : 0;
                raindropAdditionInterval -= raindropAdditionInterval > 500 ? 500 :
                                            raindropAdditionInterval > 100 ? 100 : 0;
            }

            // init wave rain
            for (int i = 0; i < nrOfRaindrops; i++) { rain[i].State = RaindropState.Invisible; }

            wave++;
            DrawWaveStartText();

            // reset timers
            clearWaveStartTextTimer.Change(waveStartInterval, Timeout.Infinite);
            rainfallAnimationTimer.Change(waveStartInterval, rainfallSpeedInterval);
            addRainTimer.Change(waveStartInterval, raindropAdditionInterval);
        }

        private static void DrawWaveStartText()
        {
            waveText = $"Wave {wave}";
            WriteAt(waveText, maxX / 2 - waveText.Length / 2, maxY / 2);
        }

        private static void ClearWaveStartText(Object state)
        {
            string spaces = "";
            for (int i = 0; i < waveText.Length; i++) { spaces += " "; }
            WriteAt(spaces, maxX/2 - spaces.Length/2 , maxY/2);
        }

        private static void RainfallAnimation(Object state)
        {
            if (!gameover)
            {
                foreach (Raindrop raindrop in rain)
                {
                    if (raindrop.State == RaindropState.Visible)
                    {
                        DrawSpaces(raindrop);
                        raindrop.Y++;
                        Draw(raindrop);
                        if (raindrop.PressedPart != null)
                        {
                            DrawSpaces(raindrop.PressedPart);
                            raindrop.PressedPart.Y++;
                            Draw(raindrop.PressedPart);
                        }
                        int maxYRaindrop = useLargeChars ? maxY - raindrop.LargeText.Height : maxY;
                        if (raindrop.Y == maxYRaindrop)
                        {
                            raindrop.State = RaindropState.Fallen;
                            if (raindrop == raindropInFocus) { raindropInFocus = null; }
                            if (useLargeChars)
                            {
                                DrawSpaces(raindrop);
                            }
                            if (AreThereNoMoreRaindropsFalling())
                            {
                                StartNewWave();
                            }
                        }
                        if (IsRaindropPlayerCollision(raindrop))
                        {
                            DrawSpaces(raindrop);
                            raindrop.State = RaindropState.Fallen;
                            if (raindrop == raindropInFocus) { raindropInFocus = null; }
                            DecreaseHealth();
                            if (health == 0)
                            {
                                gameover = true;
                                WriteGameOver();
                                return;
                            }
                            if (AreThereNoMoreRaindropsFalling())
                            {
                                StartNewWave();
                            }
                        }
                    }
                }
            }
        }

        private static void DecreaseHealth()
        {
            DrawSpaces(healthText, maxX - healthText.Length - 1, healthY);
            health--;
            DrawHealth();
        }

        private static bool IsRaindropPlayerCollision(Raindrop raindrop)
        {
            if (useLargeChars)
            {
                if (Math.Abs(player.X - raindrop.X) < raindrop.LargeText.Width &&
                    Math.Abs(player.Y - raindrop.Y) < raindrop.LargeText.Height)
                {
                    return true;
                }
            } 
            else
            {
                if (player.X == raindrop.X && player.Y == raindrop.Y)
                {
                    return true;
                }
            }
            return false;
        }

        private static void AddRain(Object state)
        {
            if (!gameover)
            {
                foreach (Raindrop raindrop in rain)
                {
                    if (raindrop.State == RaindropState.Invisible)
                    {
                        raindrop.State = RaindropState.Visible;
                        return;
                    }
                }
            }
        }

        private static void WriteAt(string s, int x, int y)
        {
            WriteAt(s, x, y, originalForegroundColor);
        }
        private static void Draw(Raindrop raindrop)
        {
            if (useLargeChars)
            {
                WriteLinesAt(raindrop.LargeText.Text, raindrop.X, raindrop.Y, raindrop.Color);
            }
            else
            {
                WriteAt(raindrop.Text, raindrop.X, raindrop.Y, raindrop.Color);
            }
            DrawScore();
            DrawHealth();
        }
        private static void DrawSpaces(Raindrop raindrop)
        {
            if (useLargeChars)
            {
                WriteLinesAt(Characters.GetLargeCharEmptyLines(raindrop.LargeText), raindrop.X, raindrop.Y, raindrop.Color);
            }
            else
            {
                string spaces = "";
                foreach (char c in raindrop.Text) { spaces += " "; }
                WriteAt(spaces, raindrop.X, raindrop.Y, raindrop.Color);
            }
        }

        private static void DrawSpaces(string s, int x, int y)
        {
            string spaces = "";
            for (int i = 0; i < s.Length; i++) { spaces += " "; }
            WriteAt(spaces, x, y);
        }

        private static void WriteAt(string s, int x, int y, ConsoleColor color)
        {
            try
            {
                lock (ConsoleWriterLock)
                {
                    int cursorX = originalX + x;
                    int cursorY = originalY + y;
                    if (cursorX <= maxX)
                    {
                        Console.SetCursorPosition(cursorX, cursorY);
                        Console.ForegroundColor = color;
                        Console.Write(s);
                    }
                }
            }
            catch (ArgumentOutOfRangeException e)
            {
                //Console.Clear();
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        private static void WriteLinesAt(string[] lines, int x, int y, ConsoleColor color)
        {
            foreach (string line in lines)
            {
                WriteAt(line, x, y, color);
                y++;
            }
        }

        private static void StartInputLoop()
        {
            handleInputThread = new Thread(() => HandleInput());
            handleInputThread.Start();
        }

        static void HandleInput()
        {
            ConsoleKeyInfo input;
            do
            {
                input = Console.ReadKey(true);
                if (!gameover)
                {
                    if (input.Key == ConsoleKey.LeftArrow)
                    {
                        if (player.X > originalX)
                        {
                            DrawSpaces(player);
                            player.X--;
                            DrawPlayer();
                        }
                    }
                    else if (input.Key == ConsoleKey.RightArrow)
                    {
                        if (player.X < maxX)
                        {
                            DrawSpaces(player);
                            player.X++;
                            DrawPlayer();
                        }
                    }
                    else if (input.Key == ConsoleKey.Tab)
                    {
                        raindropInFocus = null;
                    }
                    else
                    {
                        HandleRainTextInput(input);
                    }
                }
            } while (input.Key != ConsoleKey.Escape && !gameover);
            WriteGameOver();
        }

        private static void HandleRainTextInput(ConsoleKeyInfo input)
        {
            char inputChar = input.KeyChar;
            foreach (Raindrop raindrop in rain)
            {
                if (raindrop.State == RaindropState.Visible && 
                    (raindropInFocus == null || raindropInFocus == raindrop) &&
                    inputChar == raindrop.Text[0])
                {
                    if (raindropInFocus == null) { raindropInFocus = raindrop; }
                    char pressedChar = raindrop.Text[0];
                    LargeText pressedCharLargeText = Characters.GetLargeText(pressedChar.ToString());
                    if (raindrop.PressedPart == null)
                    {
                        raindrop.PressedPart = new Raindrop(pressedChar.ToString(), pressedCharLargeText, raindrop.X, raindrop.Y, ConsoleColor.Red);
                    }
                    else
                    {
                        raindrop.PressedPart.Text += pressedChar.ToString();
                        raindrop.PressedPart.LargeText = Characters.GetLargeText(raindrop.PressedPart.Text);
                    }
                    if (raindrop.Text.Length > 1)
                    {
                        string unpressedText = raindrop.Text[1..];
                        raindrop.Text = unpressedText;
                        raindrop.LargeText = Characters.GetLargeText(unpressedText);
                        raindrop.X += useLargeChars ? pressedCharLargeText.Width : 1;
                        Draw(raindrop);
                        Draw(raindrop.PressedPart);
                    }
                    else
                    {
                        raindrop.State = RaindropState.Pressed;
                        DrawSpaces(raindrop.PressedPart);
                        raindropInFocus = null;
                        IncreaseScore();
                        if (AreThereNoMoreRaindropsFalling())
                        {
                            StartNewWave();
                        }
                    }
                    return;
                }
            }
        }

        private static void IncreaseScore()
        {
            DrawSpaces(scoreText, maxX - scoreText.Length - 1, scoreY);
            score++;
            DrawScore();
        }

        private static void WriteGameOver()
        {
            //Console.Clear();
            Console.ForegroundColor = originalForegroundColor;
            WriteAt($"Game Over. Score: {score} in Wave {wave}", originalX, maxY + 1);
        }

        private static bool AreThereNoMoreRaindropsFalling()
        {
            foreach (Raindrop raindrop in rain) 
            {
                if (raindrop.State == RaindropState.Visible || raindrop.State == RaindropState.Invisible)
                {
                    return false;
                }
            }
            return true;
        }

        private static void RestoreConsoleProperties()
        {
            Console.CursorVisible = true;
            Console.ForegroundColor = originalForegroundColor;
        }
    }
}
