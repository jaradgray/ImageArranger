using System;
using System.Collections.Generic;
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
    /// Interaction logic for DialogBox.xaml
    /// </summary>
    public partial class DialogBox : Window
    {
        // Properties

        /// <summary>
        /// The text that will be displayed as the prompt.
        /// </summary>
        public string Prompt { get; set; } = "Delete all data for the selected item?";

        // Constructor

        public DialogBox()
        {
            InitializeComponent();

            this.DataContext = this;
        }


        // Event handlers

        public void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        public void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
