using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeatNixFileDialog {
    class Item : TreeViewItemModel {
        public Item(FileLoader fileParent, Boolean hasChild)
            : base(fileParent, hasChild) {
        }

        public String Name { set; get; }
        public String DirLocation { set; get; }
    }
}
