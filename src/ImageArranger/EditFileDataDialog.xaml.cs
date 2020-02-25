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

        private string mFilePath;
        /// <summary>
        /// The absolute path to the file whose data we're viewing.
        /// Setting this property causes all UI to update appropriately.
        /// </summary>
        public string FilePath
        {
            get { return mFilePath; }
            set
            {
                // Set mFilePath
                mFilePath = value;

                // Update tbFilename's text
                tbFilename.Text = System.IO.Path.GetFileName(value);

                // Populate timestamps
                timestamps = new ObservableCollection<FileTimestampModel>(SqliteDataAccess.GetAllTimestampsForFile(mFilePath));
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
            btnUpdatePath.Visibility = (tboxPath.Text.Equals(mFilePath)) ? Visibility.Hidden : Visibility.Visible;
        }

        private void btnUpdatePath_Click(object sender, RoutedEventArgs e)
        {
            string newFilePath = tboxPath.Text;
            string newParentDirPath = System.IO.Path.GetDirectoryName(newFilePath);

            // Set properties of our cached list of timestamps to reflect the values we want to update in the database
            foreach (FileTimestampModel timestamp in timestamps)
            {
                timestamp.FileAbsolutePath = newFilePath;
                timestamp.ParentDirAbsolutePath = newParentDirPath;
            }

            // Update database
            SqliteDataAccess.UpdateFileTimestamps(timestamps.ToList());

            // Update FilePath property
            this.FilePath = newFilePath;
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
