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
using System.Threading;
using System.ComponentModel;
using OCRMSupportForce.Models;
using OCRMSupportForce.ViewModels;

namespace OCRMSupportForce.Dialogs
{
    /// <summary>
    /// Interaction logic for dlgLongWait.xaml
    /// </summary>
    public partial class dlgLongWait : Window
    {
        #region Private Properties

        BackgroundWorker workers;
        DonorDetailModel _model;
        DonorDetailViewModel _viewmodel;

        #endregion

        public dlgLongWait(DonorDetailModel model, DonorDetailViewModel viewmodel)
        {
            InitializeComponent();

            _model = model;
            _model.ProcessingState = 0;
            _viewmodel = viewmodel;

            workers = new BackgroundWorker();

            workers.DoWork +=workers_DoWork;
            workers.ProgressChanged +=workers_ProgressChanged;
            workers.RunWorkerCompleted +=workers_RunWorkerCompleted;
            workers.WorkerSupportsCancellation = true;
            workers.WorkerReportsProgress = true;

            _model.WorkerThread = workers;

            DonorProgress.Value = 0;
            PaymentProgress.Value = 0;
        }

        private void workers_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _viewmodel.QueryFinished();
            this.Close();
        }

        private void workers_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (_model.ProcessingState == 0)
            {
                DonorProgress.Value = e.ProgressPercentage;
                DonorLabelName.Content = DonorXofY;
            }

            if (_model.ProcessingState == 1)
            { 
                DonorLabelName.Content = "Finished";
                DonorProgress.Maximum = 100;
                DonorProgress.Value = 100;

                PaymentProgress.Value = e.ProgressPercentage;
                PaymentLabelName.Content = PaymentXofY;
            }

            if (_model.ProcessingState == 2)
            {
                PaymentLabelName.Content = "Finished";
                PaymentProgress.Value = 100;
            }
        }

        private void workers_DoWork(object sender, DoWorkEventArgs e)
        {
            _model.ExecuteQuery();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            // Set Donors Max
            _model.LoadTotalDonors();
            _model.LoadTotalPayments();

            DonorProgress.Maximum = _model.TotalDonors;
            PaymentProgress.Maximum = _model.TotalPayments;

            workers.RunWorkerAsync();
        }

        public String DonorXofY
        {
            get { return String.Format("processing donor record {0} of {1}", DonorProgress.Value, _model.TotalDonors); }
        }

        public String PaymentXofY
        {
            get { return String.Format("processing payment record {0} of {1}", PaymentProgress.Value, _model.TotalPayments); }
        }

    }
}
