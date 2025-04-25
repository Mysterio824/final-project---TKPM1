using DevTools.UI.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BenchmarkBuilderTool
{
    class BenchmarkBuilderTool : ITool
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private List<BenchmarkSuite> _suites = new List<BenchmarkSuite>();
        private string _unit = "ms";

        public List<BenchmarkSuite> Suites
        {
            get => _suites;
            set
            {
                _suites = value;
                OnPropertyChanged(nameof(Suites));
            }
        }

        public string Unit
        {
            get => _unit;
            set
            {
                _unit = value;
                OnPropertyChanged(nameof(Unit));
            }
        }

        public BenchmarkBuilderTool()
        {
            // Initialize with two default suites
            Suites.Add(new BenchmarkSuite { Name = "Suite 1", Values = new List<double>() });
            Suites.Add(new BenchmarkSuite { Name = "Suite 2", Values = new List<double>() });
        }

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        public void AddSuite()
        {
            Suites.Add(new BenchmarkSuite { Name = $"Suite {Suites.Count + 1}", Values = new List<double>() });
            OnPropertyChanged(nameof(Suites));
        }

        public void RemoveSuite(BenchmarkSuite suite)
        {
            Suites.Remove(suite);
            OnPropertyChanged(nameof(Suites));
        }

        public void ResetSuites()
        {
            Suites.Clear();
            Suites.Add(new BenchmarkSuite { Name = "Suite 1", Values = new List<double>() });
            Suites.Add(new BenchmarkSuite { Name = "Suite 2", Values = new List<double>() });
            OnPropertyChanged(nameof(Suites));
        }

        public List<BenchmarkResult> CalculateResults()
        {
            var results = new List<BenchmarkResult>();

            foreach (var suite in Suites)
            {
                var values = suite.Values;
                if (values.Count == 0) continue;

                double mean = values.Average();
                double variance = values.Count > 1
                    ? values.Sum(v => Math.Pow(v - mean, 2)) / values.Count
                    : 0;

                results.Add(new BenchmarkResult
                {
                    SuiteName = suite.Name,
                    Mean = mean,
                    Variance = variance,
                    Samples = values.Count
                });
            }

            // Sort by mean (ascending)
            results = results.OrderBy(r => r.Mean).ToList();

            // Assign positions
            for (int i = 0; i < results.Count; i++)
            {
                results[i].Position = i + 1;
            }

            return results;
        }

        public string GenerateMarkdownTable()
        {
            var results = CalculateResults();
            if (results.Count == 0) return string.Empty;

            var sb = new StringBuilder();
            sb.AppendLine($"| Position | Suite | Samples | Mean ({Unit}) | Variance |");
            sb.AppendLine("| -------- | ----- | ------- | ----------- | -------- |");

            foreach (var result in results)
            {
                string meanDisplay = result.Mean.ToString("F2");
                if (result.Position > 1)
                {
                    var baseline = results[0];
                    double diff = result.Mean - baseline.Mean;
                    double ratio = result.Mean / (baseline.Mean == 0 ? 1 : baseline.Mean);
                    meanDisplay += $" (+{diff:F2} ; x{ratio:F2})";
                }

                sb.AppendLine($"| {result.Position} | {result.SuiteName} | {result.Samples} | {meanDisplay} | {result.Variance:F2} |");
            }

            return sb.ToString();
        }

        public string GenerateBulletList()
        {
            var results = CalculateResults();
            if (results.Count == 0) return string.Empty;

            var sb = new StringBuilder();

            foreach (var result in results)
            {
                sb.AppendLine($"- {result.SuiteName}");
                sb.AppendLine($"    - Position: {result.Position}");
                sb.AppendLine($"    - Mean: {result.Mean:F2}{GetComparisonText(result, results)}");
                sb.AppendLine($"    - Variance: {result.Variance:F2}");
                sb.AppendLine($"    - Samples: {result.Samples}");
            }

            return sb.ToString();
        }

        private string GetComparisonText(BenchmarkResult result, List<BenchmarkResult> allResults)
        {
            if (result.Position == 1 || allResults.Count <= 1) return "";

            var baseline = allResults[0];
            double diff = result.Mean - baseline.Mean;
            double ratio = result.Mean / (baseline.Mean == 0 ? 1 : baseline.Mean);
            return $" (+{diff:F2} ; x{ratio:F2})";
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public object Execute(object input)
        {
            return input;
        }

        public UserControl GetUI()
        {
            return new BenchmarkBuilderToolUI(this);
        }
    }

    public class BenchmarkSuite
    {
        public string Name { get; set; }
        public List<double> Values { get; set; }
    }

    public class BenchmarkResult
    {
        public int Position { get; set; }
        public string SuiteName { get; set; }
        public int Samples { get; set; }
        public double Mean { get; set; }
        public double Variance { get; set; }
    }
}
