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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DocumentDB_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void SubmitNewRequest(object sender, EventArgs e)
        {
            try
            {
                var newwindow = new SubmitNewForm();
                newwindow.dpRequestDate.Text = DateTime.Now.Date.ToString();
                newwindow.dpActionDate.Text = DateTime.Now.AddDays(2).ToString();
                this.Close();
                newwindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void ActionRequest(object sender, RoutedEventArgs e)
        {
            //Create a Form to allow user to Search on Parameters
            try
            {
                var newwindow = new RequestSearch();
                this.Close();
                newwindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
