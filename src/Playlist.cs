using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.IO;

namespace BeatNix {
    class Playlist : MusicPlayer {
        //Initialise Variables
        private int currentTrack = 0, nextTrack = 0; //Playlist Position
        LinkedList<String> trackList = new LinkedList<String>();
        private int playlistSize = 0;

        //Constructor with no parametres
        public Playlist() {
            buffer = new StringBuilder(128);
        }

        //Checks for file compatability
        private void validateTrack(String track) {
            BeatNixFileDialog.FileFormat compatableFiles = new BeatNixFileDialog.FileFormat("Music");

            foreach (var fileType in compatableFiles.FileList) {
                if ((track.Contains(fileType.ExtensionLower) || track.Contains(fileType.ExtensionUpper)) 
                    && !trackList.Contains(track))
                    trackList.AddLast(track);
            }

        }

        //Formats tracks into playlist into [ARTIST - TITLE]
        private String trackDetails(String fileLocation) {
            String result;

            String songTitle;
            String[] seperators = { "\\", ".mp3", ".ogg", ".wma", ".wav", ".flac", ".ape", "m4a", "cda" }; //For Songs with no Title Tag

            TagLib.File track = TagLib.File.Create(fileLocation);
            if (track.Tag.Title == null) {
                String[] defaultTitle = fileLocation.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
                songTitle = defaultTitle[defaultTitle.Length - 1];

                result = songTitle;
            }
            else {
                songTitle = track.Tag.Title;
                result = String.Format("{0} - {1}", track.Tag.FirstPerformer, songTitle);
            }

            return result;
        }

        /*------------------------------------------------------
         *          Overidden Functions From MusicPlayer
         *------------------------------------------------------*/

        //Load Tracks into Player API
        public override void LoadTrack(String fileLocation) {
            String playerCommand;

            if (fileActive && !inProgress) { //Unloads previous track
                playerCommand = "close MediaFile";
                mciSendString(playerCommand, null, 0, IntPtr.Zero);
            }

            if (!trackList.Contains(fileLocation)) {
                trackList.AddLast(fileLocation);
                playlistSize++;
            }

            playerCommand = String.Format(baseCommand, trackList.ElementAt(nextTrack));
            mciSendString(playerCommand, null, 0, IntPtr.Zero);
            fileActive = true;
        }

        //Preloads a folder of tracks
        public override void LoadFolder(String folderLocation, BeatNixFileDialog.FileFormat fileType) {
            String playerCommand;

            if (fileActive && !inProgress) { //Unloads previous track
                playerCommand = "close MediaFile";
                mciSendString(playerCommand, null, 0, IntPtr.Zero);
            }

            DirectoryInfo dirInfo = new DirectoryInfo(folderLocation);

            foreach (var track in dirInfo.GetFiles()) {
                if (fileType.ExtensionLower.Equals(".*"))
                   validateTrack(track.FullName);
                else if ((track.Name.Contains(fileType.ExtensionLower) || track.Name.Contains(fileType.ExtensionUpper)) 
                    && !trackList.Contains(track.FullName))
                    trackList.AddLast(track.FullName);
            }

            playlistSize = trackList.Count;

            playerCommand = String.Format(baseCommand, trackList.ElementAt(nextTrack));
            mciSendString(playerCommand, null, 0, IntPtr.Zero);
            fileActive = true;
        }

        //Returns the ID3 Tags of the loaded track (useful for stopped tracks)
        public override String[] TrackDetails() {
            String trackTitle;
            String[] seperators = { "\\", ".mp3", ".ogg", ".wma", ".wav", ".flac", ".ape", "m4a", "cda" }; //For Songs with no Title Tag

            TagLib.File track = TagLib.File.Create(trackList.ElementAt(nextTrack));
            if (track.Tag.Title == null) {
                String[] defaultTitle = trackList.ElementAt(nextTrack).Split(seperators, StringSplitOptions.RemoveEmptyEntries);
                trackTitle = defaultTitle[defaultTitle.Length - 1];
            }
            else
                trackTitle = track.Tag.Title;

            //Used to retrieve the bitrate and file type of loaded track
            String trackAudioBitrate = null, trackAudioType = null, trackVideoType = null;

            foreach (TagLib.ICodec codec in track.Properties.Codecs) {
                TagLib.IAudioCodec acodec = codec as TagLib.IAudioCodec;
                TagLib.IVideoCodec vcodec = codec as TagLib.IVideoCodec;

                if (acodec != null && (acodec.MediaTypes & TagLib.MediaTypes.Audio) != TagLib.MediaTypes.None) {
                    trackAudioBitrate = acodec.AudioBitrate.ToString();
                    trackAudioType = acodec.Description;
                }

                if (vcodec != null && (vcodec.MediaTypes & TagLib.MediaTypes.Video) != TagLib.MediaTypes.None)
                    trackVideoType = vcodec.Description;
            }

            String[] result = { trackTitle, track.Tag.FirstPerformer, track.Tag.Album, track.Tag.Year.ToString(),
                                 trackAudioBitrate, trackAudioType, trackList.ElementAt(nextTrack) };
            return result;
        }

        //Returns a bitmap image representing the album art (if applicable)
        public override BitmapImage AlbumArt() {
            TagLib.File track = TagLib.File.Create(trackList.ElementAt(nextTrack));
            try {
                TagLib.IPicture trackArt = track.Tag.Pictures[0];
                BitmapImage albumArt = new BitmapImage();

                MemoryStream ms = new MemoryStream(trackArt.Data.Data);
                ms.Seek(0, SeekOrigin.Begin);

                albumArt.BeginInit();
                albumArt.StreamSource = ms;
                albumArt.EndInit();

                return albumArt;
            }
            catch (IndexOutOfRangeException) {
                return null;
            }
        }

        //Returns a LinkedList of all tracks
        public override LinkedList<string> Tracklist() {
            LinkedList<String> result = new LinkedList<String>();

            foreach (String track in trackList)
                result.AddLast(trackDetails(track));

            return result;
        }

        //Checks for loaded track, then plays it (Also repeats if necessary)
        public override Boolean Play(Boolean repeatMode) {
            if (!fileActive)
                return false;

            if (!inProgress) {
                trackLength(trackList.ElementAt(nextTrack));

                currentTrack = nextTrack;
                nextTrack++;

                if (nextTrack == playlistSize)
                    nextTrack = 0;
            }

            String playerCommand = "Play MediaFile";
            if (repeatMode) //Adds repeat Functionality
                playerCommand += " REPEAT";

            mciSendString(playerCommand, null, 0, IntPtr.Zero);

            isPlaying = true;
            inProgress = true;

            return true;
        }

        //Pauses the current track in it's current position
        public override void Pause() {
            String playerCommand = "Stop MediaFile"; //Actually pauses the file...
            mciSendString(playerCommand, null, 0, IntPtr.Zero);

            isPlaying = false;
        }

        //Stops Current Track
        public override void Stop() {
            String playerCommand = "Stop MediaFile";
            mciSendString(playerCommand, null, 0, IntPtr.Zero);

            isPlaying = false;
            inProgress = false;

            nextTrack = currentTrack;
            LoadTrack(trackList.ElementAt(nextTrack));
        }

        //Jumps to previous track (or restarts current track)
        public override void PrevTrack(bool repeatMode) {
            if (pastCutoffTime() && isPlaying) {
                Stop();

                isPlaying = true;
            }
            else if (isPlaying) {
                Stop();
                nextTrack--;

                if (nextTrack < 0)
                    nextTrack = 0;

                isPlaying = true;
            }
            else {
                Stop();
                nextTrack--;

                if (nextTrack < 0)
                    nextTrack = 0;
            }

            LoadTrack(trackList.ElementAt(nextTrack));
            if (isPlaying)
                Play(repeatMode);
        }

        /*/////////////////////////////////////////////////////
         *               End of Playlist Class
         */////////////////////////////////////////////////////*/
    }
}
