using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;

namespace ImageArranger
{
    /// <summary>
    /// Interaction logic for StatisticsWindow.xaml
    /// </summary>
    public partial class StatisticsWindow : Window
    {
        // TODO this does not belong in the code-behind
        /// <summary>
        /// The different modes for ordering displayed file and folder statistics items
        /// </summary>
        public enum SortMode
        {
            /// <summary>
            /// Order by views, descending
            /// </summary>
            Frequent,

            /// <summary>
            /// Order by timestamp last viewed, descending
            /// </summary>
            Recent
        }

        // Instance variables

        public ObservableCollection<FileStatisticsModel> fileStatisticsCollection = new ObservableCollection<FileStatisticsModel>();


        // Constructor

        public StatisticsWindow()
        {
            InitializeComponent();

            LoadFileStatistics(SortMode.Frequent);
        }


        // Event handlers

        private void FrequentButton_Click(object sender, RoutedEventArgs e)
        {
            LoadFileStatistics(SortMode.Frequent);
        }

        private void RecentButton_Click(object sender, RoutedEventArgs e)
        {
            LoadFileStatistics(SortMode.Recent);
        }

        private void FilesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                // Cast the selected item to a FileStatisticsModel object
                FileStatisticsModel selected = (FileStatisticsModel) e.AddedItems[0];
                // Get the selected item's AbsolutePath attribute
                string path = selected.AbsolutePath;

                // If the selected item's path leads to an image...
                string[] validImageExtensions = { ".BMP", ".GIF", ".JPEG", ".JPG", ".PNG" };
                if (validImageExtensions.Contains(System.IO.Path.GetExtension(path).ToUpperInvariant()))
                {
                    // Set preview Image's source to the selected item's AbsolutePath
                    previewImage.Source = new BitmapImage(new Uri(path));
                }
            }
            else
            {
                previewImage.Source = null;
            }
        }


        // Private methods

        private void LoadFileStatistics(SortMode sortMode)
        {
            // Build a collection of FileStatisticsModels, with one element for each unique file represented in the database
            fileStatisticsCollection = new ObservableCollection<FileStatisticsModel>();
            List<string> uniqueFilePaths = SqliteDataAccess.GetUniqueFilePaths();
            foreach (string filePath in uniqueFilePaths)
            {
                List<FileTimestampModel> allTimestamps = SqliteDataAccess.GetAllTimestampsForFile(filePath);
                fileStatisticsCollection.Add(new FileStatisticsModel(filePath, allTimestamps));
            }

            // Re-order the collection based on sortMode parameter
            switch (sortMode)
            {
                case SortMode.Frequent:
                    // Order by NumViews property
                    fileStatisticsCollection = new ObservableCollection<FileStatisticsModel>(fileStatisticsCollection.OrderByDescending(fsm => fsm.NumViews));
                    break;

                case SortMode.Recent:
                    // Order by last opened Ticks property
                    fileStatisticsCollection = new ObservableCollection<FileStatisticsModel>(fileStatisticsCollection.OrderByDescending(fsm => fsm.TimestampLastOpened.Ticks));
                    break;
            }

            // Set filesListView's ItemsSource property
            filesListView.ItemsSource = fileStatisticsCollection;
        }

        
    }
}
