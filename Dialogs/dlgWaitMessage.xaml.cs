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
using System.Windows.Threading;
using System.ComponentModel;
using OCRMSupportForce.Models;
using OCRMSupportForce.ViewModels;


namespace OCRMSupportForce.Dialogs
{
    /// <summary>
    /// Interaction logic for dlgWaitMessage.xaml
    /// </summary>
    public partial class dlgWaitMessage : Window
    {
        #region Private Properties

        private DonorReportModel _model;
        private DonorReportViewModel _viewmodel;
        private int _donorrecord = 0;
        private int _totaldonors = 0;
        private int _payrecord = 0;
        private int _totalpayments = 0;

        #endregion

        public dlgWaitMessage(DonorReportModel model, DonorReportViewModel viewmodel)
        {
            InitializeComponent();
            _model = model;
            _viewmodel = viewmodel;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            _viewmodel.FireQuery(_model);
            this.Close();
        }

        #region Public Properties

        public String DonorXofY
        {
            get { return String.Format("Loading donor {0} of {1}", _donorrecord, _totaldonors); }
        }

        public String PaymentXofY
        {
            get { return String.Format("Loading donor {0} of {1}", _payrecord, _totalpayments); }
        }

        #endregion
    }
}
