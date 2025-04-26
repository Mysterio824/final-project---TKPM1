using DevTools.UI.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SQLPrettifierTool
{
    class SQLPrettifierTool : ITool
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Method to prettify SQL
        public string PrettifySql(string input, string dialect, string keywordCase, string indentStyle)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Format SQL based on selected options
            string formattedSql = FormatSql(input);

            // Apply keyword case
            formattedSql = ApplyKeywordCase(formattedSql, keywordCase);

            // Apply indentation style
            formattedSql = ApplyIndentStyle(formattedSql, indentStyle);

            // Apply dialect-specific formatting
            formattedSql = ApplyDialectFormatting(formattedSql, dialect);

            return formattedSql;
        }

        private string FormatSql(string input)
        {
            // Basic SQL formatting - add new lines and proper spacing
            string result = input;

            // Add newlines after common SQL clauses
            result = Regex.Replace(result, @"\b(SELECT|FROM|WHERE|GROUP BY|ORDER BY|HAVING|JOIN|LEFT JOIN|RIGHT JOIN|INNER JOIN|OUTER JOIN|UNION|INSERT INTO|UPDATE|DELETE FROM)\b", Environment.NewLine + "$1", RegexOptions.IgnoreCase);

            // Add space after commas
            result = Regex.Replace(result, @",", ", ");

            // Remove extra whitespace
            result = Regex.Replace(result, @"\s+", " ");

            // Clean up spaces around parentheses
            result = Regex.Replace(result, @"\s*\(\s*", " (");
            result = Regex.Replace(result, @"\s*\)\s*", ") ");

            // Add indentation to clauses
            result = Regex.Replace(result, @"\n(FROM|WHERE|GROUP BY|ORDER BY|HAVING)", "\n  $1", RegexOptions.IgnoreCase);

            return result;
        }

        private string ApplyKeywordCase(string sql, string keywordCase)
        {
            // List of SQL keywords to transform
            string[] keywords = new[] { "SELECT", "FROM", "WHERE", "GROUP BY", "HAVING", "ORDER BY",
                                   "JOIN", "LEFT", "RIGHT", "INNER", "OUTER", "UNION", "INSERT",
                                   "UPDATE", "DELETE", "CREATE", "DROP", "ALTER", "TABLE", "VIEW",
                                   "INDEX", "CONSTRAINT", "PRIMARY", "FOREIGN", "KEY", "REFERENCES", "AS" };

            switch (keywordCase)
            {
                case "Uppercase":
                    foreach (var keyword in keywords)
                    {
                        sql = Regex.Replace(sql, $@"\b{keyword}\b", keyword.ToUpper(), RegexOptions.IgnoreCase);
                    }
                    break;
                case "lowercase":
                    foreach (var keyword in keywords)
                    {
                        sql = Regex.Replace(sql, $@"\b{keyword}\b", keyword.ToLower(), RegexOptions.IgnoreCase);
                    }
                    break;
                case "preserve":
                    // Do nothing - keep original case
                    break;
            }

            return sql;
        }

        private string ApplyIndentStyle(string sql, string indentStyle)
        {
            string[] lines = sql.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            StringBuilder result = new StringBuilder();

            switch (indentStyle)
            {
                case "Standard":
                    // Standard indentation already applied in FormatSql
                    return sql;

                case "tabular left":
                    // Left-align all clauses
                    foreach (var line in lines)
                    {
                        result.AppendLine(line.TrimStart());
                    }
                    break;

                case "tabular right":
                    // Right indent increasing for nested statements
                    int indentLevel = 0;
                    foreach (var line in lines)
                    {
                        string trimmedLine = line.TrimStart();

                        // Decrease indent for closing elements
                        if (trimmedLine.StartsWith(")") || trimmedLine.StartsWith("END"))
                        {
                            indentLevel = Math.Max(0, indentLevel - 1);
                        }

                        result.AppendLine(new string(' ', indentLevel * 4) + trimmedLine);

                        // Increase indent for opening elements
                        if (trimmedLine.EndsWith("(") || trimmedLine.Contains("BEGIN"))
                        {
                            indentLevel++;
                        }
                    }
                    break;
            }

            return result.ToString().TrimEnd();
        }

        private string ApplyDialectFormatting(string sql, string dialect)
        {
            // Apply dialect-specific formatting rules
            switch (dialect)
            {
                case "PostgreSQL":
                    // PostgreSQL specific formatting
                    sql = sql.Replace("JOIN", "JOIN"); // Placeholder for real PostgreSQL specific rules
                    break;

                case "MySQL":
                    // MySQL specific formatting
                    sql = sql.Replace("LIMIT", "LIMIT"); // Placeholder for real MySQL specific rules
                    break;

                case "SQL Server Transact-SQL":
                    // SQL Server specific formatting
                    sql = sql.Replace("TOP", "TOP"); // Placeholder for real SQL Server specific rules
                    break;

                // Additional dialect specific rules can be added here

                default:
                    // Standard SQL formatting - no changes
                    break;
            }

            return sql;
        }

        public object Execute(object input)
        {
            return input;
        }

        public UserControl GetUI()
        {
            return new SQLPrettifierToolUI(this);
        }
    }
}
