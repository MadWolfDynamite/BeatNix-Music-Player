using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace BeatNix {
    class MusicPlayer : IDisposable {
        //Initialise Varibales (and command for opening music files)
        protected StringBuilder buffer;

        private int second, totalSecond;
        private int minute, totalMinute;
        private int hour, totalHour; //Only for files longer than 60mins
        private double totalRaw; //for displaying rememaining time properly

        //Import the API for Music File Playback
        protected const String baseCommand = @"open ""{0}"" type mpegvideo alias MediaFile";
        [System.Runtime.InteropServices.DllImport("winmm.dll")]
        protected static extern long mciSendString(string lpstrCommand,
            StringBuilder lpstrReturnString, int uReturnLength, IntPtr hwndCallback);

        //Various flags to alter the behavior of the player
        protected Boolean isPlaying = false;
        protected Boolean fileActive = false;
        protected Boolean inProgress = false;

        //Constructor with no parametres
        public MusicPlayer() {
            buffer = new StringBuilder(128);
        }

        //Returns the boolean values of each flag
        public Boolean IsPlaying {
            get { return isPlaying; }
        }
        public Boolean FileLoaded {
            get { return fileActive; }
        }
        public Boolean InProgress  {
            get { return inProgress; }
        }

        //Calculates the length of the loaded track (also checks for tracks over 60mins)
        protected void trackLength(String fileLocation) {
            try {
                TagLib.File track = TagLib.File.Create(fileLocation);

                foreach (TagLib.ICodec codec in track.Properties.Codecs) {
                    TagLib.IAudioCodec acodec = codec as TagLib.IAudioCodec;
                    TagLib.IVideoCodec vcodec = codec as TagLib.IVideoCodec;

                    if (acodec != null && (acodec.MediaTypes & TagLib.MediaTypes.Audio) != TagLib.MediaTypes.None) {
                        totalHour = acodec.Duration.Hours;
                        totalMinute = acodec.Duration.Minutes;
                        totalSecond = acodec.Duration.Seconds;

                        totalRaw = acodec.Duration.TotalMilliseconds;
                    }
                }
            }
            catch (TagLib.UnsupportedFormatException) {
                totalSecond = 0;
                totalRaw = 0;
            }

            //Alternative for if TagLib Returns nothing
            if (totalRaw == 0 || totalSecond == 0) {
                try {
                    String playerCommand = "Status MediaFile length";
                    mciSendString(playerCommand, buffer, 256, IntPtr.Zero);

                    totalRaw = Convert.ToInt32(buffer.ToString());
                    totalSecond = Convert.ToInt32(buffer.ToString());

                    totalSecond /= 1000;
                    totalMinute = totalSecond / 60;
                    totalSecond %= 60;

                    if (totalMinute >= 60)  {
                        totalHour = totalMinute / 60;
                        totalMinute %= 60;
                    }
                    else
                        totalHour = 0;
                }
                catch (FormatException) {
                    totalSecond = 0;
                    totalRaw = 0;
                }
            }
        }

        //Returns the length of the song (formatted as h:mm:ss)
        public String TrackDurationFormatted() {
            String result = "";

            if (totalHour > 0)
                result = totalHour + ":";

            result += totalMinute.ToString("00") + ":" + totalSecond.ToString("00");
            return result;
        }

        //Returns the length of the song (in milliseconds)
        public double TrackDuration {
            get { return totalRaw; }
        }
        //Returns the current position of the track (in milliseconds)
        public int TrackPositionRaw {
            get {
                String playerCommand = "Status MediaFile position";
                mciSendString(playerCommand, buffer, 128, IntPtr.Zero);

                try {
                    return Convert.ToInt32(buffer.ToString());
                }
                catch (Exception) {
                    return 0;
                }
            }
        }

        /*Caculates the current position of the track, and returns the value as a string 
        *(format depends on value received)*/
        public String TrackPosition(int format) {
            String result = "";
            String playerCommand = "Status MediaFile position";
            mciSendString(playerCommand, buffer, 128, IntPtr.Zero);

            try {
                second = Convert.ToInt32(buffer.ToString());
            }
            catch (FormatException) {
                return "N/A";
            }

            second /= 1000;
            minute = second / 60;
            second %= 60;

            if (totalHour > 0) {
                hour = minute / 60;
                minute %= 60;
            }

            switch (format) {
                case 0: //h:mm:ss (elasped)
                    if (totalHour > 0)
                        result = hour + ":";

                    result += minute.ToString("00") + ":" + second.ToString("00");
                    break;
                case 1: //h:mm:ss / h:mm:ss (elasped / total)
                    if (totalHour > 0)
                        result = String.Format("{0}:{1}:{2} / {3}:{4}:{5}", hour, minute.ToString("00"), second.ToString("00"),
                            totalHour, totalMinute.ToString("00"), totalSecond.ToString("00"));
                    else
                        result = String.Format("{0}:{1} / {2}:{3}", minute.ToString("00"), second.ToString("00"),
                            totalMinute.ToString("00"), totalSecond.ToString("00"));
                    break;
                case 2: //h:mm:ss (remaining)
                    int remSecond = Convert.ToInt32(totalRaw) - Convert.ToInt32(buffer.ToString());

                    remSecond /= 1000;
                    int remMinute = remSecond / 60;
                    remSecond %= 60;

                    int remHour = remMinute / 60;
                    if (totalHour > 0) {
                        remMinute %= 60;
                        result = remHour + ":";
                    }

                    result += remMinute.ToString("00") + ":" + remSecond.ToString("00");
                    break;
            }

            return result;
        }

        //Checks if the track is past the cutoff point for restarting same track
        public Boolean pastCutoffTime() {
            String playerCommand = "Status MediaFile position";
            mciSendString(playerCommand, buffer, 128, IntPtr.Zero);

            int position;
            try {
                position = Convert.ToInt32(buffer.ToString());
            }
            catch (FormatException) {
                return false;
            }

            return position / 1000 >= 3;
        }

        //Checks if the end of the track has been reached
        public Boolean HasEnded() {
            return (hour == totalHour) && (minute == totalMinute) && (second == totalSecond);
        }

        //Unloads the track when the user closes application
        public void Dispose() {
            String playerCommand;

            if (isPlaying) {
                playerCommand = "Stop MediaFile";
                mciSendString(playerCommand, null, 0, IntPtr.Zero);
            }

            playerCommand = "close MediaFile";
            mciSendString(playerCommand, null, 0, IntPtr.Zero);
        }

        /*-------------------------------------
         *          Virtual Functions
         *-------------------------------------*/

        //Loading Tracks into Player API
        public virtual void LoadTrack(String fileLocation) { //Do Nothing
        }

        //Loading folders of tracks into Player API
        public virtual void LoadFolder(String fileLocation, BeatNixFileDialog.FileFormat fileType) { //Do Nothing
        }

        //Returns the ID3 Tags of the loaded track (useful for stopped tracks)
        public virtual String[] TrackDetails() {
            String[] result = { "You've done something wrong :P", "", "", "", "" };
            return result;
        }

        //Returns a bitmap image representing the album art (if applicable)
        public virtual BitmapImage AlbumArt() {
            return null;
        }

        //Returns a LinkedList of all tracks (more suited to playlists)
        public virtual LinkedList<String> Tracklist() {
            return null;
        }

        //Checks for loaded track, then plays it (Also repeats if necessary)
        public virtual Boolean Play(Boolean repeatMode) {
            return false;
        }

        //Pauses the current track
        public virtual void Pause() { //Do Nothing
        }

        //Stops current track (also loads new track if necessary)
        public virtual void Stop() { //Do Nothing
        }

        //Skips to previously preloaded track (or restarts current track, more suited to playlists)
        public virtual void PrevTrack(Boolean repeatMode) { //Do Nothing
        }

        //Skips to next preloaded track (more suited to playlists)
        public virtual void SkipTrack(Boolean repeatMode) { //Do Nothing
        }

        //Seeks to specific location on track
        public virtual void SeekTrack(double location, Boolean repeatMode) { //Do Nothing
        }
    }
    /*/////////////////////////////////////////////////////
    *               End of MusicPlayer Class
    ////////////////////////////////////////////////////*/
}
