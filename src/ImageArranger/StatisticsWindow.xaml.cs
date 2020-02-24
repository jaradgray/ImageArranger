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

namespace ImageArranger
{
    /// <summary>
    /// Interaction logic for StatisticsWindow.xaml
    /// </summary>
    public partial class StatisticsWindow : Window
    {
        // Instance variables

        public ObservableCollection<FileStatisticsModel> fileStatisticsCollection = new ObservableCollection<FileStatisticsModel>();


        // Constructor

        public StatisticsWindow()
        {
            InitializeComponent();

            LoadFileStatistics();
        }


        // Private methods

        private void LoadFileStatistics()
        {
            // TODO generate the list of FileStatisticsModels that we'll display
            //  Right now it's just one element for each unique file represented in the database
            List<string> uniqueFilePaths = SqliteDataAccess.GetUniqueFilePaths();
            foreach (string filePath in uniqueFilePaths)
            {
                List<FileTimestampModel> allTimestamps = SqliteDataAccess.GetAllTimestampsForFile(filePath);
                fileStatisticsCollection.Add(new FileStatisticsModel(filePath, allTimestamps));
            }

            filesListView.ItemsSource = fileStatisticsCollection;
        }
    }
}
