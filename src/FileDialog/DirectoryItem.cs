using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BeatNixFileDialog {
    class DirectoryItem : Item {
        public List<Item> Items { set; get; }

        public DirectoryItem(String dirInfo, FileLoader parent) 
            : base (parent, true) {
            Items = new List<Item>();
        }

        protected override void LoadChildren() {
            var subFolders = new DirectoryInfo(DirLocation);

            foreach (var directory in subFolders.GetDirectories()) {
                var child = new DirectoryItem(directory.FullName, new FileLoader(DirLocation)) {
                    Name = directory.Name,
                    DirLocation = directory.FullName
                };

                Children.Add(child);
            }
        }
    }
}
