using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace BeatNix
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        TimeSpan timerSpan = new TimeSpan(0, 0, 0, 0, 10);
    }
}
