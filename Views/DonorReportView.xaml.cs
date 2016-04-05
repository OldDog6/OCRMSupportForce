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
using System.ComponentModel;
using System.Windows.Threading;
using OCRMSupportForce.Models;

namespace OCRMSupportForce.Views
{
    /// <summary>
    /// Interaction logic for DonorReportView.xaml
    /// </summary>
    public partial class DonorReportView : UserControl
    {
        public DonorReportView()
        {
            InitializeComponent();
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            ResultGrid.SelectAll();
            
        }

        private void ResultGrid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ResultGrid.InvalidateVisual();
        }

        private void ResultGrid_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            ResultGrid.InvalidateVisual();
        }
    }
}
