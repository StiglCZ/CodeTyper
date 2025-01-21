enum Language {
    C,
    Cs,
    Cpp,
    Java,
}

class Test {
    public float baseTime;
    public float timeLeft;
    public Language lang = Language.Java;
    
    public uint mistakes = 0U;
    public uint characters = 0U;
}

class Program {
    private static Queue<char> keys = new Queue<char>();
    private static void CaptureKeys() {
        while(true) {
            ConsoleKeyInfo key = Console.ReadKey(true);
            if(key.Key == ConsoleKey.Backspace) keys.Enqueue((char)0);
            else if(key.Key == ConsoleKey.Tab)  keys.Enqueue(' ');
            else keys.Enqueue(key.KeyChar);
        }
    }

    static void RefreshTime(float time) {
        var pos = Console.GetCursorPosition();
        Console.SetCursorPosition(0, 0);
        Console.Write(new string(' ', 0));
        Console.SetCursorPosition(0, 0);
        Console.Write($"Time: {time}");
        Console.SetCursorPosition(pos.Left, pos.Top);
    }
    static void RefreshWords(uint words) {
        var pos = Console.GetCursorPosition();
        Console.SetCursorPosition(20, 0);
        Console.Write(new string(' ', 10));
        Console.SetCursorPosition(20, 0);
        Console.Write($"Words: {words}");
        Console.SetCursorPosition(pos.Left, pos.Top);
    }

    static void WriteNewLine(string[] lines, int index) {
        Console.SetCursorPosition(0, 1);
        Console.WriteLine(new string(' ', 100));
        Console.WriteLine(new string(' ', 100));
        Console.WriteLine(new string(' ', 100));
        Console.SetCursorPosition(0, 1);
        
        if(index > 0) Console.WriteLine(lines[index-1]);
        else Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(lines[index]);
        Console.WriteLine(lines[index+1]);
        Console.ResetColor();
        Console.SetCursorPosition(0, 2);
    }

    private static string[] PullCode(Language lang) {
        string? code = new CodePuller().Pull(lang);
        
        if(code == null) {
            Console.WriteLine("Code pull failed!");
            Environment.Exit(1);
        }
        
        code = code
            .Replace("\r", "")
            .Replace("\n\n", "\n")
            .Replace("\t", " ")
            .Trim();
        string[] lines = code.Split('\n');
        return lines;
    }
    
    static void ExecuteTest(Test test) {
        test.timeLeft = test.baseTime;
        const int period = 20;

        string[] lines;
        int line = 0;
        int pos = 0;
        do{
            lines = PullCode(test.lang);
        } while(lines.Length < 20);

        Console.WriteLine("Pressing tab will work as a space would");
        Console.WriteLine("Pressing any key will start the test...");
        Console.ReadKey(true);
        
        Task.Factory.StartNew(CaptureKeys);
        Console.Clear();
        
        WriteNewLine(lines, line);
        
        while(true) {
            while(keys.TryDequeue(out char current)) {
                if (current == 0x0D || pos >= lines[line].Length) {
                    WriteNewLine(lines, ++line);
                    pos = 0;
                    continue;
                }
                
                if (current == 0x20) {
                    while (pos < lines[line].Length && (lines[line][pos] == 0x20)) {
                        pos++;
                    }
                    if(lines[line][pos+1] == 0x20) pos++;
                    Console.CursorLeft = pos;
                    continue;
                }

                if(current == 0) {
                    continue;
                }

                if (current != lines[line][pos])
                    test.mistakes++;
                Console.Write(current);
                test.characters++;
                pos++;
                
                Console.CursorTop = 2;
                Console.CursorLeft = pos;
            }

            
            Thread.Sleep(period);
            test.timeLeft -= 0.001f * period;
            if(test.timeLeft < 0)
                break;
            RefreshTime(test.timeLeft);
            RefreshWords(test.characters);
        }

        Console.Clear();
        Console.WriteLine($"Total tokens: {test.characters}");
        Console.WriteLine($"Total mistakes: {test.mistakes}");
    }
    public static void Main(string[] args) {
        Test test = new();
        
        do { Console.Write("Time(s): "); }
        while (!float.TryParse(Console.ReadLine(), out test.baseTime));

        string[] langs = Enum.GetNames<Language>();
        do {
            Console.WriteLine("Avilable Languages: ");
            foreach (string lang in langs)
                Console.WriteLine(lang);
            
        } while (!Enum.TryParse<Language>(Console.ReadLine(), true, out test.lang));
        ExecuteTest(test);
    }
}
