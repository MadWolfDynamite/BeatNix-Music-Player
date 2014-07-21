using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;

namespace BeatNixFileDialog {
    class RootFolderLoader {
        public List<FileLoader> Items { set; get; }

        public RootFolderLoader() {
            var driveInfo = DriveInfo.GetDrives();

            Items = new List<FileLoader>();

            foreach (var directory in driveInfo)
                Items.Add(new FileLoader(directory.RootDirectory.ToString()));
        }

    }
}
