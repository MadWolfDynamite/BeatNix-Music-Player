using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.IO;

namespace BeatNix {
    class SingleTrack : MusicPlayer {
        //Initialise Varibales
        private String currentTrack = "";
        private String preloadedTrack;

        //Constructor with no parameters
        public SingleTrack() {
            buffer = new StringBuilder(128);
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
                currentTrack = "";
            }

            playerCommand = String.Format(baseCommand, fileLocation);
            mciSendString(playerCommand, null, 0, IntPtr.Zero);

            preloadedTrack = fileLocation;
            fileActive = true;
        }

        //Returns the ID3 Tags of the loaded track (useful for stopped tracks)
        public override String[] TrackDetails() {
            String trackTitle;
            String[] seperators = { "\\", ".mp3", ".ogg", ".wma", ".wav", ".flac", ".ape", "m4a", "cda" }; //For Songs with no Title Tag

            TagLib.File track = TagLib.File.Create(preloadedTrack);
            if (track.Tag.Title == null) {
                String[] defaultTitle = preloadedTrack.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
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
                                 trackAudioBitrate, trackAudioType, preloadedTrack };
            return result;
        }

        //Returns a bitmap image representing the album art (if applicable)
        public override BitmapImage AlbumArt() {
            TagLib.File track = TagLib.File.Create(preloadedTrack);
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

        //Returns a LinkedList of preloaded track (also the current track, if necessary)
        public override LinkedList<String> Tracklist() {
            LinkedList<String> result = new LinkedList<String>();

            if (!currentTrack.Equals("") && !currentTrack.Equals(preloadedTrack)) {
                String songTitle;
                String[] seperators = { "\\", ".mp3", ".ogg", ".wma", ".wav", ".flac", ".ape", "m4a", "cda" }; //For Songs with no Title Tag

                TagLib.File track = TagLib.File.Create(currentTrack);
                if (track.Tag.Title == null) {
                    String[] defaultTitle = currentTrack.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
                    songTitle = defaultTitle[defaultTitle.Length - 1];

                    result.AddLast(songTitle);
                }
                else {
                    songTitle = track.Tag.Title;
                    result.AddLast(String.Format("{0} - {1}", track.Tag.FirstPerformer, songTitle));
                }
            }

            String[] preloadDetails = TrackDetails();

            if (preloadDetails[1] != null) {
                String preloadInfo = String.Format("{0} - {1}", preloadDetails[1], preloadDetails[0]);

                if (!result.Contains(preloadInfo))
                    result.AddLast(preloadInfo);
            }
            else {
                if (!result.Contains(preloadDetails[0]))
                    result.AddLast(preloadDetails[0]);
            }

            return result;
        }

        //Checks for loaded track, then plays it (Also repeats if necessary)
        public override Boolean Play(Boolean repeatMode) {
            if (!fileActive)
                return false;

            if (!inProgress) {
                trackLength(preloadedTrack);
                currentTrack = preloadedTrack;
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

        //Stops current track (also loads new track if necessary)
        public override void Stop() {
            String playerCommand = "Stop MediaFile";
            mciSendString(playerCommand, null, 0, IntPtr.Zero);

            isPlaying = false;
            inProgress = false;

            if (preloadedTrack.Equals(currentTrack))
                LoadTrack(currentTrack);
            else
                LoadTrack(preloadedTrack);
        }

        //Restarts current track
        public override void PrevTrack(Boolean repeatMode) {
            String tempTrack = "";

            String playerCommand = "Stop MediaFile";
            mciSendString(playerCommand, null, 0, IntPtr.Zero);
            inProgress = false;

            if (!preloadedTrack.Equals(currentTrack))
                tempTrack = preloadedTrack;

            LoadTrack(currentTrack);

            if (isPlaying)
                Play(repeatMode);

            if (!tempTrack.Equals(""))
                LoadTrack(tempTrack);
        }

        //Skips to preloaded track (if different)
        public override void SkipTrack(Boolean repeatMode) {
            String playerCommand;

            if (isPlaying) {
                playerCommand = "Stop MediaFile";
                mciSendString(playerCommand, null, 0, IntPtr.Zero);
            }

            inProgress = false;
            LoadTrack(preloadedTrack);

            if (isPlaying)
                Play(repeatMode);
        } //end of SkipTrack

        //Seeks to specific location on track
        public override void SeekTrack(double location, Boolean repeatMode) {
            String playerCommand = String.Format("Seek MediaFile to {0}", Math.Round(location));
            mciSendString(playerCommand, null, 0, IntPtr.Zero);

            if (isPlaying)
                Play(repeatMode);
        }
    }
    /*/////////////////////////////////////////////////////
     *               End of SingleTrack Class
     */////////////////////////////////////////////////////*/
}
