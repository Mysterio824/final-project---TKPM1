using DevTools.UI.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextDiffTool
{
    class TextDiffTool : ITool
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string ComputeDiff(string originalText, string modifiedText)
        {
            if (string.IsNullOrEmpty(originalText) && string.IsNullOrEmpty(modifiedText))
                return "Both texts are empty";

            if (string.IsNullOrEmpty(originalText))
                return "Original text is empty, all content is new";

            if (string.IsNullOrEmpty(modifiedText))
                return "Modified text is empty, all content was removed";

            var diff = new StringBuilder();
            var originalLines = originalText.Split('\n');
            var modifiedLines = modifiedText.Split('\n');

            var matcher = new DiffMatchPatch();
            var diffs = matcher.diff_main(originalText, modifiedText);
            matcher.diff_cleanupSemantic(diffs);

            foreach (var change in diffs)
            {
                switch (change.operation)
                {
                    case Operation.DELETE:
                        diff.Append("- ").AppendLine(change.text);
                        break;
                    case Operation.INSERT:
                        diff.Append("+ ").AppendLine(change.text);
                        break;
                    case Operation.EQUAL:
                        diff.Append("  ").AppendLine(change.text);
                        break;
                }
            }

            return diff.ToString();
        }

        public object Execute(object input)
        {
            return input;
        }

        public UserControl GetUI()
        {
            return new TextDiffToolUI(this);
        }
    }
    public enum Operation { DELETE, INSERT, EQUAL }

    public class Diff
    {
        public Operation operation;
        public string text;

        public Diff(Operation operation, string text)
        {
            this.operation = operation;
            this.text = text;
        }
    }

    public class DiffMatchPatch
    {
        public List<Diff> diff_main(string text1, string text2)
        {
            var diffs = new List<Diff>();

            if (text1 == text2)
            {
                if (!string.IsNullOrEmpty(text1))
                    diffs.Add(new Diff(Operation.EQUAL, text1));
                return diffs;
            }

            int i = 0, j = 0;
            while (i < text1.Length || j < text2.Length)
            {
                // Find characters that are the same
                int equalStart = i;
                while (i < text1.Length && j < text2.Length && text1[i] == text2[j])
                {
                    i++;
                    j++;
                }

                if (equalStart < i)
                    diffs.Add(new Diff(Operation.EQUAL, text1.Substring(equalStart, i - equalStart)));

                // Find characters that were removed
                int deleteStart = i;
                while (i < text1.Length && j < text2.Length && text1[i] != text2[j])
                    i++;

                if (deleteStart < i)
                    diffs.Add(new Diff(Operation.DELETE, text1.Substring(deleteStart, i - deleteStart)));

                // Find characters that were added
                int insertStart = j;
                while (j < text2.Length && (i >= text1.Length || text1[i] != text2[j]))
                    j++;

                if (insertStart < j)
                    diffs.Add(new Diff(Operation.INSERT, text2.Substring(insertStart, j - insertStart)));
            }

            return diffs;
        }

        public void diff_cleanupSemantic(List<Diff> diffs)
        {
            for (int i = diffs.Count - 1; i >= 0; i--)
            {
                if (string.IsNullOrEmpty(diffs[i].text))
                    diffs.RemoveAt(i);
            }
        }
    }
}
