using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace BeatNixFileDialog {
    class FileLoader : TreeViewItemModel {
        private String dirInfo;

        public FileLoader(String path) : base(null,true) {
            dirInfo = path;
        }

        public String FolderName {
            get {
                var folderInfo = new DirectoryInfo(dirInfo);
                return folderInfo.Name;
            }
        }

        public String FolderLocation {
            get { return dirInfo; }
        }

        protected override void LoadChildren() {
            var subFolders = new DirectoryInfo(dirInfo);

            try {
                foreach (var directory in subFolders.GetDirectories()) {
                    var child = new DirectoryItem(directory.FullName, this) {
                        Name = directory.Name,
                        DirLocation = directory.FullName
                    };

                    Children.Add(child);
                }
            }
            catch (Exception) { } //Do Nothing
        }
    }
}
