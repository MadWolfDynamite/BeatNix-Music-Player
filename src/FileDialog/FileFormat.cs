using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeatNixFileDialog {
    public class FileFormat {
        public List<FileFormat> FileList { get; set; }

        String extensionName;

        String fileTypeUpper;
        String fileTypeLower;

        public FileFormat (String mediaType) {
            FileList = new List<FileFormat>();

            switch (mediaType) {
                case "Music":
                    String[] compatibleFiles = {
                        "All Supported Files|.*|.*",
                        "MPEG Audio (.mp3)|.mp3|.MP3",
                        "Ogg Vorbis (.ogg)|.ogg|.OGG",
                        "Windows Media Audio (.wma)|.wma|.WMA",
                        "Waveform Audio (.wav)|.wav|.WAV",
                        "Free Lossless Audio Codec (.flac)|.flac|.FLAC",
                        "Monkeys Audio (.ape)|.ape|.APE",
                        "MPEG-4 Audio (.m4a)|.m4a|.M4A",
                        "Audio CD (.cda)|.cda|.CDA"};

                    foreach (String fileType in compatibleFiles) {
                        String[] result = fileType.Split('|');

                        FileList.Add(new FileFormat(result[0],result[1],result[2]));
                    }
                    break;

                case "Video":
                    String[] compatibleFiles02 = {
                        "All Supported Files|.*|.*",
                        "Audio Video Interleave (.avi)|.avi|.AVI",
                        "MPEG Video (.mpeg)|.mpeg|.MPEG",
                        "Windows Media Video (.wmv)|.wmv|.WMV",
                        "MPEG Video (.mpg)|.mpg|.MPG" };

                    foreach (String fileType in compatibleFiles02) {
                        String[] result = fileType.Split('|');

                        FileList.Add(new FileFormat(result[0],result[2],result[1]));
                    }
                    break;
            }
        }
        private FileFormat(String exName, String extenU, String extenL) {
            extensionName = exName;

            fileTypeUpper = extenU;
            fileTypeLower = extenL;
        }

        public String Name { 
            get { return extensionName; } 
        }
        public String ExtensionUpper {
            get { return fileTypeUpper; }
        }
        public String ExtensionLower {
            get { return fileTypeLower; }
        }
    }
}
