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
    /// Interaction logic for EditFileDataDialog.xaml
    /// </summary>
    public partial class EditFileDataDialog : Window
    {
        // Instance variables

        public ObservableCollection<FileTimestampModel> timestamps = new ObservableCollection<FileTimestampModel>();


        // Properties

        private FileStatisticsModel mFile;
        public FileStatisticsModel File
        {
            get { return mFile; }
            set
            {
                // Set mFile
                mFile = value;

                // Populate timestamps
                timestamps = new ObservableCollection<FileTimestampModel>(SqliteDataAccess.GetAllTimestampsForFile(mFile.AbsolutePath));
                lbTimestamps.ItemsSource = timestamps;
            }
        }


        // Constructor

        public EditFileDataDialog()
        {
            InitializeComponent();

            this.DataContext = this;
        }


        // Event handlers

        private void tboxPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnUpdatePath.Visibility = (tboxPath.Text.Equals(mFile.AbsolutePath)) ? Visibility.Hidden : Visibility.Visible;
        }

        private void btnUpdatePath_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("TODO update path");
        }

        private void lbTimestamps_CmDeleteClick(object sender, RoutedEventArgs e)
        {
            if (lbTimestamps.SelectedItems == null || lbTimestamps.SelectedItems.Count != 1) return;

            FileTimestampModel selectedTimestamp = (FileTimestampModel) lbTimestamps.SelectedItems[0];

            // TODO delete the selected timestamp from the database
            MessageBox.Show("Delete timestamp " + selectedTimestamp.Id + " for file:\n" + selectedTimestamp.FileAbsolutePath);
        }

        public void btnDeleteTimestamp_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Delete timestamp, sender:\n" + sender);
        }

        public void btnClose_Click(object sender, RoutedEventArgs e)
        {
            // TODO can change to true if needed
            this.DialogResult = false;
        }

        
    }
}
