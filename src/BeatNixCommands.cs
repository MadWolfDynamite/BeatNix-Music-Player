using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace BeatNix {

    // Custom Commands for WPF implementation
    public static class BeatNixCommands {
        // Loads Tracks into respective class
        public static readonly RoutedUICommand LoadTrack = new RoutedUICommand(
                "Load Music",
                "LoadTrack",
                typeof(BeatNixCommands),
                new InputGestureCollection() {
                    new KeyGesture(Key.O, ModifierKeys.Control)
                }
            );

        // Plays (or Pauses) the preloaded track
        public static readonly RoutedUICommand PlayTrack = new RoutedUICommand(
                "Play / Pause",
                "PlayTrack",
                typeof(BeatNixCommands),
                new InputGestureCollection() {
                    new KeyGesture(Key.P, ModifierKeys.Control),
                    new KeyGesture(Key.MediaPlayPause)
                }
            );

        // Stops the preloaded track
        public static readonly RoutedUICommand StopTrack = new RoutedUICommand(
                "Stop",
                "StopTrack",
                typeof(BeatNixCommands),
                new InputGestureCollection() {
                    new KeyGesture(Key.S, ModifierKeys.Control),
                    new KeyGesture(Key.MediaStop)
                }
            );

        // Skips to previously preloaded track (or restarts current track, more suited to playlists)
        public static readonly RoutedUICommand PrevTrack = new RoutedUICommand(
                "Prev",
                "PrevTrack",
                typeof(BeatNixCommands),
                new InputGestureCollection() {
                    new KeyGesture(Key.N, ModifierKeys.Control),
                    new KeyGesture(Key.MediaPreviousTrack)
                }
            );

        // Skips to next preloaded track (more suited to playlists)
        public static readonly RoutedUICommand NextTrack = new RoutedUICommand(
                "Next",
                "NextTrack",
                typeof(BeatNixCommands),
                new InputGestureCollection() {
                    new KeyGesture(Key.M, ModifierKeys.Control),
                    new KeyGesture(Key.MediaNextTrack)
                }
            );

        // Changes the format of timer
        public static readonly RoutedUICommand TimerToggle = new RoutedUICommand(
                "Timer Mode",
                "TimerToggle",
                typeof(BeatNixCommands),
                new InputGestureCollection() {
                    new KeyGesture(Key.T, ModifierKeys.Control)
                }
            );

        // Allows for additional means of closing program
        public static readonly RoutedUICommand Exit = new RoutedUICommand (
                "Exit",
                "Exit",
                typeof(BeatNixCommands),
                new InputGestureCollection() {
                    new KeyGesture(Key.F4, ModifierKeys.Alt)
                }
            );

        /*/////////////////////////////////////////////////////
         *                 End of Player Commands
         * ////////////////////////////////////////////////////*/
    }
}
