using DevTools.UI.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASCIIArtGeneratorTool
{
    class ASCIIArtGeneratorTool : ITool
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly Dictionary<string, Dictionary<char, string[]>> _fonts = new Dictionary<string, Dictionary<char, string[]>>
    {
        {"1Row", new Dictionary<char, string[]>
            {
                {'A', new[] {"@"}}, {'B', new[] {"B"}}, {'C', new[] {"C"}}, {'D', new[] {"D"}}, {'E', new[] {"E"}},
                {'F', new[] {"F"}}, {'G', new[] {"G"}}, {'H', new[] {"H"}}, {'I', new[] {"I"}}, {'J', new[] {"J"}},
                {'K', new[] {"K"}}, {'L', new[] {"L"}}, {'M', new[] {"M"}}, {'N', new[] {"N"}}, {'O', new[] {"O"}},
                {'P', new[] {"P"}}, {'Q', new[] {"Q"}}, {'R', new[] {"R"}}, {'S', new[] {"S"}}, {'T', new[] {"T"}},
                {'U', new[] {"U"}}, {'V', new[] {"V"}}, {'W', new[] {"W"}}, {'X', new[] {"X"}}, {'Y', new[] {"Y"}},
                {'Z', new[] {"Z"}}, {'0', new[] {"0"}}, {'1', new[] {"1"}}, {'2', new[] {"2"}}, {'3', new[] {"3"}},
                {'4', new[] {"4"}}, {'5', new[] {"5"}}, {'6', new[] {"6"}}, {'7', new[] {"7"}}, {'8', new[] {"8"}},
                {'9', new[] {"9"}}, {' ', new[] {" "}}
            }
        },
        {"3-D", new Dictionary<char, string[]>
            {
                {'A', new[] {" __ ", "/__\\", "\\__/"} },
                {'B', new[] {" __ ", "|__]", "|__]"} },
                {'C', new[] {" __", "/  ", "\\__"} },
                {'D', new[] {" __ ", "|  \\", "|__/"} },
                {'E', new[] {" ___", "|__ ", "|___"} },
                {'F', new[] {" ___", "|__ ", "|   "} },
                {'G', new[] {" __", "/  ", "\\__/"} },
                {'H', new[] {"    ", "|__|", "|  |"} },
                {'I', new[] {"  ", "| ", "| "} },
                {'J', new[] {"    ", "   |", "\\__|"} },
                {'K', new[] {"    ", "|_/ ", "| \\ "} },
                {'L', new[] {"    ", "|   ", "|___"} },
                {'M', new[] {"    ", "|\\/|", "|  |"} },
                {'N', new[] {"    ", "|\\ |", "| \\|"} },
                {'O', new[] {" __ ", "/  \\", "\\__/"} },
                {'P', new[] {" __ ", "|__]", "|   "} },
                {'Q', new[] {" __ ", "/  \\", "\\_\\/"}},
                {'R', new[] {" __ ", "|__]", "|  \\"} },
                {'S', new[] {" __", "/__", "\\__"} },
                {'T', new[] {"___", " | ", " | "} },
                {'U', new[] {"    ", "|  |", "\\__/"} },
                {'V', new[] {"    ", "\\  /", " \\/ "} },
                {'W', new[] {"    ", "|  |", "|/\\|"} },
                {'X', new[] {"    ", "\\_/", "/ \\"} },
                {'Y', new[] {"    ", "\\_/ ", " |  "} },
                {'Z', new[] {"___", " / ", "/__"} },
                {'0', new[] {" __ ", "/  \\", "\\__/"} },
                {'1', new[] {"  ", "/| ", " | "} },
                {'2', new[] {" _ ", "/ \\", "\\_/"} },
                {'3', new[] {"__ ", " _]", "__]"} },
                {'4', new[] {"   ", "|_|", "  |"} },
                {'5', new[] {" __", "|_ ", " _]"} },
                {'6', new[] {" _ ", "|_ ", "|_]"} },
                {'7', new[] {"__", " /", "/ "} },
                {'8', new[] {" _ ", "(_)", "(_)"} },
                {'9', new[] {" _ ", "(_\\", " / "} },
                {' ', new[] {"  ", "  ", "  "} }
            }
        },
        {"3x5", new Dictionary<char, string[]>
            {
                {'A', new[] {" _ ", "/ \\", "|_|", "| |", "   "} },
                {'B', new[] {" _ ", "| \\", "|_/", "| \\", "|_/"} },
                {'C', new[] {" __", "/  ", "|  ", "\\__", "   "} },
                {'D', new[] {" _ ", "| \\", "| |", "| /", "|_\\"} },
                {'E', new[] {" __", "|_ ", "|__", "|  ", "|__"} },
                {'F', new[] {" __", "|_ ", "|__", "|  ", "|  "} },
                {'G', new[] {" __", "/  ", "| _", "\\__|", "   "} },
                {'H', new[] {"   ", "| |", "|_|", "| |", "   "} },
                {'I', new[] {" _ ", " | ", " | ", " | ", " _ "} },
                {'J', new[] {"  _", "  |", "  |", "\\_|", "   "} },
                {'K', new[] {"   ", "|/ ", "|\\ ", "| \\", "   "} },
                {'L', new[] {"   ", "|  ", "|  ", "|__", "   "} },
                {'M', new[] {"   ", "|V|", "| |", "| |", "   "} },
                {'N', new[] {"   ", "|\\|", "| |", "| |", "   "} },
                {'O', new[] {" _ ", "/ \\", "| |", "\\_/", "   "} },
                {'P', new[] {" _ ", "| \\", "|_/", "|  ", "|  "} },
                {'Q', new[] {" _ ", "/ \\", "| |", "\\_X", "   "} },
                {'R', new[] {" _ ", "| \\", "|_/", "| \\", "|  "} },
                {'S', new[] {" __", "/  ", "\\_\\", "  /", "\\__"} },
                {'T', new[] {"___", " | ", " | ", " | ", "   "} },
                {'U', new[] {"   ", "| |", "| |", "\\_/", "   "} },
                {'V', new[] {"   ", "\\  /", " \\/ ", "    ", "    "} },
                {'W', new[] {"   ", "| |", "| |", "|V|", "   "} },
                {'X', new[] {"   ", "\\ /", " X ", "/ \\", "   "} },
                {'Y', new[] {"   ", "\\ /", " V ", " | ", "   "} },
                {'Z', new[] {"___", "  /", " / ", "/  ", "___"} },
                {'0', new[] {" _ ", "/ \\", "| |", "\\_/", "   "} },
                {'1', new[] {"   ", " /|", "  |", "  |", "  _"} },
                {'2', new[] {" _ ", "  \\", " / ", "/  ", "/__ "} },
                {'3', new[] {" _ ", "  \\", " < ", "  /", " _ "} },
                {'4', new[] {"   ", "/ |", "__|", "  |", "   "} },
                {'5', new[] {" __", "|_ ", " _>", "  /", " _ "} },
                {'6', new[] {" __", "/  ", "|_ ", "|_/", "   "} },
                {'7', new[] {"___", "  /", " / ", "/  ", "/   "} },
                {'8', new[] {" _ ", "/ \\", "\\_/", "/ \\", "\\_/"} },
                {'9', new[] {" _ ", "/ \\", "\\_/", "  /", " _ "} },
                {' ', new[] {"   ", "   ", "   ", "   ", "   "} }
            }
        },
        {"5 Line Oblique", new Dictionary<char, string[]>
            {
                {'A', new[] {"    ___    ", "   /   \\   ", "  /  _  \\  ", " /  / \\  \\ ", "/__/   \\__\\"} },
                {'B', new[] {" _______  ", "|  __   \\ ", "| |__| _/ ", "|  __  \\  ", "|_| |_|\\_\\"} },
                {'C', new[] {"  _______ ", " /  ___  \\", "|  /   \\  |", "| |     | |", " \\_____/_/ "} },
                {'D', new[] {" ______  ", "|  ___ \\ ", "| |   \\ \\", "| |___/ /", "|______/ "} },
                {'E', new[] {" ________ ", "|  ____  |", "| |____| |", "|  ____  |", "|_|    |_|"} },
                {'F', new[] {" ________ ", "|  ____  |", "| |____| |", "|  ____  |", "|_|      |"} },
                {'G', new[] {"  _______ ", " /  ___  \\", "|  /   \\  |", "| |  O  | |", " \\_____/_/ "} },
                {'H', new[] {" __    __ ", "|  |  |  |", "|  |__|  |", "|   __   |", "|__|  |__|"} },
                {'I', new[] {" ______ ", "|_    _|", "  |  |  ", "  |  |  ", " _|__|_ "} },
                {'J', new[] {"     ____ ", "    |    |", "    |    |", " ___|    |", "|______/ "} },
                {'K', new[] {" __   __ ", "|  | /  |", "|  |/   |", "|   /\\  |", "|__/  \\_|"} },
                {'L', new[] {" __      ", "|  |     ", "|  |     ", "|  |___  ", "|______|"} },
                {'M', new[] {" _     _ ", "| \\   / |", "|  \\_/  |", "| |\\_/| |", "|_|   |_|"} },
                {'N', new[] {" __    __ ", "|  \\  |  |", "|   \\ |  |", "|  |\\ \\  |", "|__| \\____|"} },
                {'O', new[] {"  _____  ", " / ___ \\ ", "| |   | |", "| |___| |", " \\_____/ "} },
                {'P', new[] {" ______  ", "|  __  \\ ", "| |__| / ", "|  ____/ ", "|_|      "} },
                {'Q', new[] {"  _____  ", " / ___ \\ ", "| |   | |", "| |__/| |", " \\_____\\_\\"} },
                {'R', new[] {" ______  ", "|  __  \\ ", "| |__| / ", "|  _  _\\ ", "|_| \\_\\_\\"} },
                {'S', new[] {" _______ ", "|  _____|", "|_____  |", " _____| |", "|_______|"} },
                {'T', new[] {" _______ ", "|__   __|", "   | |   ", "   | |   ", "   |_|   "} },
                {'U', new[] {" __    __ ", "|  |  |  |", "|  |  |  |", "|  |__|  |", " \\______/ "} },
                {'V', new[] {"__      __", "\\ \\    / /", " \\ \\  / / ", "  \\ \\/ /  ", "   \\__/   "} },
                {'W', new[] {"__        __", "\\ \\      / /", " \\ \\ /\\ / / ", "  \\ V  V /  ", "   \\_/\\_/   "} },
                {'X', new[] {"__    __", "\\ \\  / /", " \\ \\/ / ", " / /\\ \\ ", "/_/  \\_\\"} },
                {'Y', new[] {"__    __", "\\ \\  / /", " \\ \\/ / ", "  \\  /  ", "   \\/   "} },
                {'Z', new[] {" _______ ", "|___   /|", "   /  / |", "  /  /__|", " /______|"} },
                {'0', new[] {"  _____  ", " / ___ \\ ", "| / _ \\ |", "| \\___/ |", " \\_____/ "} },
                {'1', new[] {"  __   ", " /  |  ", "/_/ |  ", "   _|  ", "  |___ "} },
                {'2', new[] {" _____  ", "/__ __\\ ", "  / /   ", " / /__  ", "/______|"} },
                {'3', new[] {" ______ ", "|___  / ", "   / /  ", "  / /__ ", " /_____|"} },
                {'4', new[] {"   ___   ", "  / _ \\  ", " / / \\ \\ ", "/_/___\\_\\", "    |_|  "} },
                {'5', new[] {" _______ ", "|  ____|_", "| |____  ", " \\__   | ", " ______| "} },
                {'6', new[] {"  _____  ", " / ____| ", "| |____  ", "| |   | |", " \\|___| |"} },
                {'7', new[] {" _______ ", "|___   / ", "   /  /  ", "  /  /   ", " /__/    "} },
                {'8', new[] {"  _____  ", " / ___ \\ ", "( (___) )", " > ___ < ", "/_/   \\_\\"} },
                {'9', new[] {"  _____  ", " / ___ \\ ", "| |___| |", " \\___  | ", "     |_| "} },
                {' ', new[] {"  ", "  ", "  ", "  ", "  "} }
            }
        }
    };

        public string GenerateASCIIArt(string input, string fontName, int width)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            if (!_fonts.ContainsKey(fontName))
                return "Selected font not found.";

            var font = _fonts[fontName];
            var result = new StringBuilder();

            int charsPerLine = CalculateCharsPerLine(width, fontName);

            for (int i = 0; i < input.Length; i += charsPerLine)
            {
                string chunk = i + charsPerLine <= input.Length
                    ? input.Substring(i, charsPerLine)
                    : input.Substring(i);

                int lineHeight = GetLineHeight(fontName);
                var lines = new string[lineHeight];

                for (int j = 0; j < lineHeight; j++)
                {
                    lines[j] = string.Empty;
                }

                foreach (char c in chunk.ToUpper())
                {
                    char ch = font.ContainsKey(c) ? c : ' ';

                    for (int j = 0; j < lineHeight; j++)
                    {
                        lines[j] += font[ch][j];
                    }
                }

                for (int j = 0; j < lineHeight; j++)
                {
                    result.AppendLine(lines[j]);
                }

                if (i + charsPerLine < input.Length)
                {
                    result.AppendLine();
                }
            }

            return result.ToString();
        }

        private int CalculateCharsPerLine(int width, string fontName)
        {
            if (!_fonts.ContainsKey(fontName))
                return 10; // Default value

            var font = _fonts[fontName];
            int charWidth = GetCharacterWidth(fontName);

            return Math.Max(1, width / charWidth);
        }

        private int GetLineHeight(string fontName)
        {
            if (!_fonts.ContainsKey(fontName))
                return 1;

            var font = _fonts[fontName];
            return font.ContainsKey('A') ? font['A'].Length : 1;
        }

        private int GetCharacterWidth(string fontName)
        {
            if (!_fonts.ContainsKey(fontName))
                return 1;

            var font = _fonts[fontName];
            if (font.ContainsKey('A') && font['A'].Length > 0)
                return font['A'][0].Length;
            else
                return 1;
        }

        public object Execute(object input)
        {
            return input;
        }

        public UserControl GetUI()
        {
            return new ASCIIArtGeneratorToolUI(this);
        }
    }
}
