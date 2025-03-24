using MyTool.ToolContracts;

namespace MyTool
{
    public class ReverseStringTool : ITool
    {
        public string Name => "String Reverser";
        public string Description => "Reverses the input string.";

        public ToolType Type => ToolType.String;

        public string Execute(string input)
        {
            char[] charArray = input.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }
}