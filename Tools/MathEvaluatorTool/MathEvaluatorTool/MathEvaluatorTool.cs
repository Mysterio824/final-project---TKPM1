using DevTools.UI.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathEvaluatorTool
{
    class MathEvaluatorTool : ITool
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Method to evaluate mathematical expressions
        public string EvaluateMathExpression(string expression)
        {
            try
            {
                // Using DataTable to evaluate mathematical expressions
                DataTable dt = new DataTable();
                // Replace common math functions with their .NET equivalents
                expression = PreprocessExpression(expression);

                var result = dt.Compute(expression, "");
                return result.ToString();
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        // Preprocess expression to handle math functions
        private string PreprocessExpression(string expression)
        {
            // Replace common mathematical functions with their .NET equivalents
            expression = expression.Replace("sqrt", "Sqrt");
            expression = expression.Replace("cos", "Cos");
            expression = expression.Replace("sin", "Sin");
            expression = expression.Replace("tan", "Tan");
            expression = expression.Replace("abs", "Abs");
            expression = expression.Replace("exp", "Exp");
            expression = expression.Replace("log", "Log");
            expression = expression.Replace("log10", "Log10");
            expression = expression.Replace("pow", "Pow");

            // Handle Math functions by evaluating them separately
            // This is a simplistic approach and wouldn't handle nested functions properly
            foreach (var func in new[] { "Sqrt", "Cos", "Sin", "Tan", "Abs", "Exp", "Log", "Log10" })
            {
                int startIndex = expression.IndexOf(func + "(");
                while (startIndex != -1)
                {
                    int openBracket = startIndex + func.Length;
                    int closeBracket = FindClosingBracket(expression, openBracket);

                    if (closeBracket != -1)
                    {
                        string argument = expression.Substring(openBracket + 1, closeBracket - openBracket - 1);

                        // Evaluate the argument first (could be a sub-expression)
                        DataTable dt = new DataTable();
                        var argResult = dt.Compute(argument, "");

                        // Apply the math function
                        double result = 0;
                        double argValue = Convert.ToDouble(argResult);

                        switch (func)
                        {
                            case "Sqrt": result = Math.Sqrt(argValue); break;
                            case "Cos": result = Math.Cos(argValue); break;
                            case "Sin": result = Math.Sin(argValue); break;
                            case "Tan": result = Math.Tan(argValue); break;
                            case "Abs": result = Math.Abs(argValue); break;
                            case "Exp": result = Math.Exp(argValue); break;
                            case "Log": result = Math.Log(argValue); break;
                            case "Log10": result = Math.Log10(argValue); break;
                        }

                        // Replace the function call with its result
                        expression = expression.Remove(startIndex, closeBracket - startIndex + 1)
                                              .Insert(startIndex, result.ToString());
                    }

                    startIndex = expression.IndexOf(func + "(", startIndex + 1);
                }
            }

            return expression;
        }

        private int FindClosingBracket(string text, int openBracketIndex)
        {
            int depth = 0;
            for (int i = openBracketIndex; i < text.Length; i++)
            {
                if (text[i] == '(')
                    depth++;
                else if (text[i] == ')')
                {
                    depth--;
                    if (depth == 0)
                        return i;
                }
            }
            return -1;
        }

        public object Execute(object input)
        {
            return input;
        }

        public UserControl GetUI()
        {
            return new MathEvaluatorToolUI(this);
        }
    }
}
