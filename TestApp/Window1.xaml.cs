using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Rtf2HtmlMod;

namespace TestApp
{
    /// <summary>
    /// Interaktionslogik für Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public MainViewModel ViewModel
        {
            get
            {
                return this.DataContext as MainViewModel;
            }
            set
            {
                this.DataContext = value;
            }
        }

        public Window1()
        {
            InitializeComponent();
            ViewModel = new MainViewModel();
        }

        private void Btn_test_Click(object sender, RoutedEventArgs e)
        {
            var html = ViewModel.Text;
            MessageBox.Show(html);
        }
    }
}