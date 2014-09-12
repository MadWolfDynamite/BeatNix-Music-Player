using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BeatNix {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private MusicPlayer soundPlayer = new MusicPlayer();
        private int timerMode = 0;

        System.Windows.Threading.DispatcherTimer trackTimer = new System.Windows.Threading.DispatcherTimer();
        private int timerInterval = 10;

        private Boolean isCurrentlySeeking = false;
        private Boolean playlistMode = false;

        public MainWindow() {
            InitializeComponent();

            trackTimer.Tick += new EventHandler(updateTrackTime);
            trackTimer.Interval = new TimeSpan(0, 0, 0, 0, timerInterval);
        }

        // Event Handlers for loading tracks into player
        private void Load_CanExecute(Object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }
        private void Load_Executed(Object sender, ExecutedRoutedEventArgs e) {
            BeatNixFileDialog.TrackDialog userTrack = new BeatNixFileDialog.TrackDialog();
            userTrack.ShowDialog();

            if (userTrack.DialogResult.Value) {
                    if (soundPlayer.GetType() == typeof(MusicPlayer)) { // Initial Startup
                        playlistMode = userTrack.PlaylistMode;

                        if (userTrack.PlaylistMode || userTrack.FolderMode)
                            soundPlayer = new Playlist();
                        else
                            soundPlayer = new SingleTrack();

                        if (userTrack.FolderMode)
                            soundPlayer.LoadFolder(userTrack.SelectedFolder, userTrack.FileType);
                        else
                            soundPlayer.LoadTrack(userTrack.SelectedFile);
                    }

                    else if (soundPlayer.GetType() == typeof(SingleTrack)) { // Single Track Mode
                        if (playlistMode) {
                            soundPlayer = new SingleTrack();
                            playlistMode = false;
                        }

                        soundPlayer.LoadTrack(userTrack.SelectedFile);
                    }

                    else { // Playlist Mode
                    }

                    if (cb_AutoPlay.IsChecked.Value && !soundPlayer.IsPlaying) {
                        Play_Executed(sender, e as ExecutedRoutedEventArgs);
                    }

                    ls_Tracks.ItemsSource = soundPlayer.Tracklist();
            } // end of if(userTrack.DialogResult)
        } // end of Load_Executed

        // Event Handlers for playing (and pausing) preloaded tracks 
        private void Play_CanExecute(Object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = soundPlayer.FileLoaded;
        }
        private void Play_Executed(Object sender, ExecutedRoutedEventArgs e) {
            if (!soundPlayer.InProgress) {
                updateCurrentlyPlaying(true);
                ls_Tracks.SelectedIndex = 0;

                sl_Seek.IsEnabled = true;
            }

            if (!soundPlayer.IsPlaying) {
                soundPlayer.Play(cb_Repeat.IsChecked.Value);
                bn_PlayPause.Content = "Pause";

                if (soundPlayer.TrackDuration != sl_Seek.Maximum) 
                    sl_Seek.Maximum = soundPlayer.TrackDuration;

                tb_PlayerStatus.Text = "Playing";
                trackTimer.Start();
            }
            else {
                soundPlayer.Pause();
                bn_PlayPause.Content = "Play";

                tb_PlayerStatus.Text = "Paused";
            }
        }

        // Event Handlers for stopping playback
        private void Stop_CanExecute(Object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = soundPlayer.InProgress;
        }
        private void Stop_Executed(Object sender, ExecutedRoutedEventArgs e) {
            soundPlayer.Stop();
            bn_PlayPause.Content = "Play";

            ls_Tracks.SelectedIndex = -1;
            updateCurrentlyPlaying(false);

            sl_Seek.IsEnabled = false;
            sl_Seek.Value = 0;

            tb_PlayerStatus.Text = "BeatNix Music Player";
            trackTimer.Stop();

            ls_Tracks.ItemsSource = soundPlayer.Tracklist();
        }

        // Event Handers for previous track
        private void Prev_CanExecute(Object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = soundPlayer.InProgress || soundPlayer.GetType().Equals(typeof(MusicPlayer));
        }
        private void Prev_Executed(Object sender, ExecutedRoutedEventArgs e) {
            soundPlayer.PrevTrack(cb_Repeat.IsChecked.Value);

            if (soundPlayer.TrackDuration != sl_Seek.Maximum)
                sl_Seek.Maximum = soundPlayer.TrackDuration;

            ls_Tracks.ItemsSource = soundPlayer.Tracklist();
        }

        // Event Handlers for skipping to next track
        private void Next_CanExecute(Object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = ls_Tracks.Items.Count > 1;
        }
        private void Next_Executed(Object sender, ExecutedRoutedEventArgs e) {
            soundPlayer.SkipTrack(cb_Repeat.IsChecked.Value);

            if (soundPlayer.TrackDuration != sl_Seek.Maximum)
                sl_Seek.Maximum = soundPlayer.TrackDuration;

            ls_Tracks.ItemsSource = soundPlayer.Tracklist();
            updateCurrentlyPlaying(true);
        }

        // Event Handers for switching the timer format
        private void Time_CanExecute(Object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }
        private void Time_Executed(Object sender, ExecutedRoutedEventArgs e) {
            timerMode++;
            timerMode %= 3;
        }

        // Switches the timer format (alternative handler for clicking textblock)
        private void updateTimerMode(object sender, MouseButtonEventArgs e) {
            timerMode++;
            timerMode %= 3;
        }

        // Updates the details of currently playing track
        private void updateCurrentlyPlaying(Boolean trackState) {
            if (trackState) { // Playback has started
                String[] trackInfo = soundPlayer.TrackDetails();
                albumArtImage.Source = soundPlayer.AlbumArt();

                tb_Title.Text = trackInfo[0]; // Track Title
                tb_Artist.Text = trackInfo[1]; // Track Artist
                tb_Album.Text = trackInfo[2]; // Track Album

                tb_Year.Text = trackInfo[3]; // Track Year
                tb_Bitrate.Text = String.Format("{0}kb/s", trackInfo[4]); // Track Bitrate
                tb_Format.Text = trackInfo[5]; // Track Filetype (.mp3 /.ogg / etc.)

                tb_Location.Text = trackInfo[6]; // Track Location (what is used by the class itself)

                lb_Title.IsEnabled = true;

                lb_Bitrate.IsEnabled = true;
                lb_Format.IsEnabled = true;

                lb_Location.IsEnabled = true;
                
                // Checks for empty values (Title, Bitrate, Filetype and Location will always have a value)
                if (albumArtImage.Source == null) {
                    Uri defaultImage = new Uri(Environment.CurrentDirectory + "/Images/Placeholder.png");
                    albumArtImage.Source = new BitmapImage(defaultImage);
                }

                if (trackInfo[1] != null)
                    lb_Artist.IsEnabled = true;
                else
                    lb_Artist.IsEnabled = false;

                if (trackInfo[2] != null)
                    lb_Album.IsEnabled = true;
                else
                    lb_Album.IsEnabled = false;

                if (Convert.ToInt32(trackInfo[3]) != 0) {
                    lb_Year.IsEnabled = true;
                    tb_Year.Visibility = Visibility.Visible;
                }
                else {
                    lb_Year.IsEnabled = false;
                    tb_Year.Visibility = Visibility.Hidden;
                }

                detailsEnabled.IsChecked = true;
            }
            else { // Playback has ended
                // Resets everthing to default (labels disabled / textblocks hidden)
                Uri defaultImage = new Uri(Environment.CurrentDirectory + "/Images/Placeholder.png");
                albumArtImage.Source = new BitmapImage(defaultImage);

                lb_Title.IsEnabled = false;
                lb_Artist.IsEnabled = false;
                lb_Album.IsEnabled = false;

                lb_Year.IsEnabled = false;
                lb_Bitrate.IsEnabled = false;
                lb_Format.IsEnabled = false;

                lb_Location.IsEnabled = false;

                detailsEnabled.IsChecked = false;
            } // end of else statement
        } // end of updateCurrentlyPlaying

        // Updates time elasped (or remaning) on track
        private void updateTrackTime(Object sender, EventArgs e) {
            if (!isCurrentlySeeking) {
                tb_TrackTime.Text = soundPlayer.TrackPosition(timerMode);
                sl_Seek.Value = soundPlayer.TrackPositionRaw;

                if ((soundPlayer.HasEnded() && !isCurrentlySeeking) && cb_Repeat.IsChecked.Value == false) {
                    if (cb_AutoPlay.IsChecked.Value && ls_Tracks.Items.Count > 1) //Auto Plays next preloaded track
                        Next_Executed(sender, e as ExecutedRoutedEventArgs);
                    else
                        Stop_Executed(sender, e as ExecutedRoutedEventArgs);
                }
            }
            else {
                int seekSecond = Convert.ToInt32(sl_Seek.Value / 1000);
                int seekMinute = seekSecond / 60;
                seekSecond %= 60;

                double seekHour;
                if (seekMinute >= 60) {
                    seekHour = seekMinute / 60;
                    seekMinute %= 60;
                }
                else
                    seekHour = 0;

                String result = "";
                if (seekHour > 0)
                    result = seekHour.ToString() + ":";

                result += seekMinute.ToString("00") + ":" + seekSecond.ToString("00");
                double percentage = (sl_Seek.Value / soundPlayer.TrackDuration) * 100;

                tb_TrackTime.Text = String.Format("Seek {0} / {1} ({2}%)", result, soundPlayer.TrackDurationFormatted(), Math.Round(percentage));
            } 
        }

        // Enables the user to seek through track, then seeks to position upon release
        private void updateSeekingStatus(object sender, MouseButtonEventArgs e) {
            isCurrentlySeeking = true;
        }
        private void seekTrack(object sender, MouseButtonEventArgs e) {
            soundPlayer.SeekTrack(sl_Seek.Value, cb_Repeat.IsChecked.Value);
            tb_TrackTime.Text = soundPlayer.TrackPosition(timerMode);

            isCurrentlySeeking = false;
        }

        // Changes the state of the repeat function
        private void updateRepeatStatus(object sender, RoutedEventArgs e) {
            if (soundPlayer.IsPlaying)
                soundPlayer.Play(cb_Repeat.IsChecked.Value);
        }

    }
}
