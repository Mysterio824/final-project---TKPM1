using DevTools.UI.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chronometer
{
    class Chronometer : ITool
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private DispatcherTimer timer;
        private Stopwatch stopwatch;
        private string currentTime = "00:00.000";

        public Chronometer()
        {
            stopwatch = new Stopwatch();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += Timer_Tick;
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

        private void Timer_Tick(object sender, object e)
        {
            UpdateTime();
        }

        public void Start()
        {
            if (!stopwatch.IsRunning)
            {
                stopwatch.Start();
                timer.Start();
            }
        }

        public void Reset()
        {
            stopwatch.Reset();
            UpdateTime();
        }

        private void UpdateTime()
        {
            TimeSpan ts = stopwatch.Elapsed;
            CurrentTime = string.Format("{0:00}:{1:00}.{2:000}",
                ts.Minutes, ts.Seconds, ts.Milliseconds);
        }

        public string CurrentTime
        {
            get { return currentTime; }
            private set
            {
                if (currentTime != value)
                {
                    currentTime = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentTime)));
                }
            }
        }

        public object Execute(object input)
        {
            return input;
        }

        public UserControl GetUI()
        {
            return new ChronometerUI(this);
        }
    }
}
