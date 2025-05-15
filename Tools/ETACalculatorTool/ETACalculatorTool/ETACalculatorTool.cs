using DevTools.UI.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETACalculatorTool
{
    class ETACalculatorTool : ITool
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Method to calculate ETA
        public ETAResult CalculateETA(int totalElements, int consumedElements, int timeSpan, string timeUnit)
        {
            // Calculate how many elements per minute
            double elementsPerMinute = CalculateElementsPerMinute(consumedElements, timeSpan, timeUnit);

            // Calculate remaining elements
            int remainingElements = totalElements - consumedElements;

            // Calculate total minutes needed for all elements
            double totalMinutesForAll = totalElements / elementsPerMinute;

            // Calculate remaining minutes
            double remainingMinutes = remainingElements / elementsPerMinute;

            // Format the duration string
            string formattedDuration = FormatDuration(remainingMinutes);

            // Calculate end time
            DateTime endTime = DateTime.Now.AddMinutes(remainingMinutes);
            string formattedEndTime = FormatEndTime(endTime);

            return new ETAResult
            {
                TotalDuration = formattedDuration,
                EndTime = formattedEndTime,
                EndDateTime = endTime
            };
        }

        private double CalculateElementsPerMinute(int consumedElements, int timeSpan, string timeUnit)
        {
            // Convert everything to minutes
            double timeSpanInMinutes = ConvertToMinutes(timeSpan, timeUnit);

            // Calculate elements per minute
            return consumedElements / timeSpanInMinutes;
        }

        private double ConvertToMinutes(int timeSpan, string timeUnit)
        {
            switch (timeUnit)
            {
                case "milliseconds":
                    return timeSpan / 60000.0;
                case "seconds":
                    return timeSpan / 60.0;
                case "minutes":
                    return timeSpan;
                case "hours":
                    return timeSpan * 60.0;
                case "days":
                    return timeSpan * 24 * 60.0;
                default:
                    return timeSpan; // Default to minutes
            }
        }

        private string FormatDuration(double minutes)
        {
            TimeSpan timeSpan = TimeSpan.FromMinutes(minutes);

            if (timeSpan.TotalDays >= 1)
            {
                return $"{(int)timeSpan.TotalDays} days {timeSpan.Hours} hours {timeSpan.Minutes} minutes";
            }
            else if (timeSpan.TotalHours >= 1)
            {
                return $"{timeSpan.Hours} hours {timeSpan.Minutes} minutes";
            }
            else
            {
                return $"{timeSpan.Minutes} minutes {timeSpan.Seconds} seconds";
            }
        }

        private string FormatEndTime(DateTime endTime)
        {
            DateTime now = DateTime.Now;

            if (endTime.Date == now.Date)
            {
                return $"today at {endTime:HH:mm}";
            }
            else if (endTime.Date == now.Date.AddDays(1))
            {
                return $"tomorrow at {endTime:HH:mm}";
            }
            else
            {
                return $"on {endTime:yyyy-MM-dd} at {endTime:HH:mm}";
            }
        }

        public object Execute(object input)
        {
            return input;
        }

        public UserControl GetUI()
        {
            return new ETACalculatorToolUI(this);
        }
    }

    public class ETAResult
    {
        public string TotalDuration { get; set; }
        public string EndTime { get; set; }
        public DateTime EndDateTime { get; set; }
    }
}
