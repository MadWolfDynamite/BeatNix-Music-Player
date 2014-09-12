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
using System.Windows.Shapes;
using System.IO;
using IWshRuntimeLibrary;

namespace BeatNixFileDialog {
    /// <summary>
    /// Interaction logic for TrackDialog.xaml
    /// </summary>
    public partial class TrackDialog : Window {
        public String SelectedFile { set; get; }
        public String SelectedFolder { set; get; }
        FileFormat selectedFileType;
        public Boolean PlaylistMode { set; get; }
        public Boolean FolderMode { set; get; }

        public TrackDialog() {
            InitializeComponent();

            RootFolderLoader fileList = new RootFolderLoader();
            base.DataContext = fileList;

            FileFormat fileSet = new FileFormat("Music");
            fileTypeComboBox.ItemsSource = fileSet.FileList;
            fileTypeComboBox.SelectedIndex = 0;

            fileComboBox.ItemsSource = recentFiles();

            PlaylistMode = false;
        }

        public FileFormat FileType {
            get { return selectedFileType; }
        }

        private List<IWshShortcut> recentFiles() {
            List<IWshShortcut> result = new List<IWshShortcut>();
            DirectoryInfo dirInfo = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Recent));
            WshShell shell = new WshShell();

            foreach (var file in dirInfo.GetFiles()) {
                if (selectedFileType.ExtensionLower.Equals(".*")) {
                    if (file.Name.Contains(".lnk")) {
                        IWshShortcut link = shell.CreateShortcut(file.FullName) as IWshShortcut;
                        result.Add(link);
                    }
                }
                else
                    if (file.Name.Contains(".lnk")) {
                        IWshShortcut link = shell.CreateShortcut(file.FullName) as IWshShortcut;
                        if (link.TargetPath.Contains(selectedFileType.ExtensionLower) || link.TargetPath.Contains(selectedFileType.ExtensionUpper)) 
                            result.Add(link);
                    }
            }

            return result;
        }

        private void confirmSelection(object sender, RoutedEventArgs e) {
            if (PlaylistCheckBox.IsChecked == true || (FolderCheckBox.IsEnabled && FolderCheckBox.IsChecked == true))
                PlaylistMode = true;

            if (fileTabControl.SelectedIndex != 1 || (FolderCheckBox.IsEnabled && FolderCheckBox.IsChecked == true))
                FolderMode = true;

            DialogResult = true;
        }

        private void populateList(String location) {
            fileListBox.Items.Clear();
            var dirInfo = new DirectoryInfo(location);

            if (selectedFileType.ExtensionLower.Equals(".*"))
                foreach (var track in dirInfo.GetFiles())
                    populateList(track);
            else
                foreach (var track in dirInfo.GetFiles()) {
                    if (track.Name.Contains(selectedFileType.ExtensionLower) || track.Name.Contains(selectedFileType.ExtensionUpper))
                        fileListBox.Items.Add(track);
                }

        }
        private void populateList(FileInfo trackInfo) {
                foreach (FileFormat fileType in fileTypeComboBox.ItemsSource) {
                    if (trackInfo.Name.Contains(fileType.ExtensionLower) || trackInfo.Name.Contains(fileType.ExtensionUpper))
                        fileListBox.Items.Add(trackInfo);
            } //end of foreach loop (All Files)
        }

        private void selectFolder(object sender, RoutedPropertyChangedEventArgs<object> e) {
            try {
                var result = folderTreeView.SelectedItem as FileLoader;
                SelectedFolder = result.FolderLocation;
            }
            catch (Exception) {
                var result = folderTreeView.SelectedItem as DirectoryItem;
                SelectedFolder = result.DirLocation;
            }

            if (System.IO.Directory.Exists(SelectedFolder))
                populateList(SelectedFolder);
        }

        private void selectFile(object sender, SelectionChangedEventArgs e) {
            try {
                var result = fileListBox.SelectedItem as FileInfo;
                SelectedFile = result.FullName;

                fileComboBox.Text = result.Name;
            }
            catch (Exception) {
            }
        }
        private void selectRecentFile(object sender, SelectionChangedEventArgs e) {
            try {
                var result = fileComboBox.SelectedItem as IWshShortcut;
                SelectedFile = result.TargetPath;
            }
            catch (Exception) {
            }

            fileComboBox.Text = SelectedFile;
        }

        private void update_FileType(object sender, SelectionChangedEventArgs e) {
            var result = (FileFormat)fileTypeComboBox.SelectedItem;
            selectedFileType = result;

            fileComboBox.ItemsSource = recentFiles();

            if (SelectedFolder != null)
                populateList(SelectedFolder);

            if (SelectedFile != null)
                fileComboBox.Text = SelectedFile;
        }

        private void togglePreloadOption(object sender, SelectionChangedEventArgs e) {
            FolderCheckBox.IsEnabled = fileTabControl.SelectedIndex == 1;
        }
    }
}
