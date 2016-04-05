using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;

using esscWPFShell;
using OCRMSupportForce.Models;

using Excel = NetOffice.ExcelApi;


namespace OCRMSupportForce.ViewModels
{
    public class OnPremiseViewModel : WorkspaceViewModel
    {
        #region Private Properties

        ApplicationViewModel _appvm;
        ForceWSDLSupport _wsdl;
        mySqlModel _connection;
        
        // Parameters and Display Values
        private String _donorxofy;
        private String _paymentxofy;

        private DateTime _startdate;
        private DateTime _enddate;
        private int _maxdonors;
        private int _currentdonor;
        private int _maxpayments;
        private int _currentpayment;

        private int _currentlapsed;

        // Threading
        BackgroundWorker worker;
        BackgroundWorker excelThread;

        // Relay Commands
        private RelayCommand _executequery;
        private RelayCommand _close;
        private RelayCommand _executelaspedreport;
        private RelayCommand _deduplicateworkspace;
        private RelayCommand _downloadaccounts;
        private RelayCommand _execute5Kdonors;

        // excel privates
        private DateTime _excelstart;
        private DateTime _excelend;
        private int _excelprocess;

        // Models
        private OnPremiseModel _model;
        private LapsedDonorsReport _lapseddonors;
        
        #endregion

        #region Creation

        public OnPremiseViewModel(ApplicationViewModel MainWindowViewModel, string _displayname, ForceWSDLSupport forceWSDLSupport, mySqlModel connection)
        {
            this.DisplayName = _displayname;
            _appvm = MainWindowViewModel;
            _wsdl = forceWSDLSupport;
            _connection = connection;

            _donorxofy = String.Empty;
            _paymentxofy = String.Empty;
            _currentdonor = 0;
            _currentpayment = 0;

            MaxDonors = 100;
            MaxPayments = 100;

            _model = new OnPremiseModel(_wsdl, _connection);

            // Set Default Dat Range
            _startdate = DateTime.Now.AddDays(-30);
            _enddate = DateTime.Now;

            _excelstart = _startdate;
            _excelend = _enddate;

            // Set up async thread...
            
            worker = new BackgroundWorker();

            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;

            _model.WorkerThread = worker;

            
            // Setup Excel worker thread
            excelThread = new BackgroundWorker();

            excelThread.DoWork +=excelThread_DoWork;
            excelThread.ProgressChanged += excelThread_ProgressChanged;
            excelThread.RunWorkerCompleted += excelThread_RunWorkerCompleted;
            excelThread.WorkerSupportsCancellation = true;
            excelThread.WorkerReportsProgress = true;
            


            // relay commands
            _executequery = new RelayCommand(param => this.ExecuteQuery());
            _executelaspedreport = new RelayCommand(param => this.RunLaspedDonors());
            _deduplicateworkspace = new RelayCommand(param => this.OpenDeduplicateWorkspace());
            _downloadaccounts = new RelayCommand(param => this.DownloadAccounts());
            _execute5Kdonors = new RelayCommand(param => this.Write5KDonors());

        }
   
        #endregion

        #region Async
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            _model.ExecuteDonorQuery();
            _model.ExecutePaymentQuery();
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == -1)
            {
                ErrorViewModel _errorvm = new ErrorViewModel(_appvm, _wsdl, _model.ErrorMessage);
                _appvm.MainWindow.InjectWorkSpace(_errorvm);
            }
            else
            {
                if (_model.OnDonors)
                {
                    _currentdonor = e.ProgressPercentage;
                    OnPropertyChanged("ProcessDonor");
                    OnPropertyChanged("DonorXofY");
                }
                else
                {
                    _currentpayment = e.ProgressPercentage;
                    OnPropertyChanged("ProcessPayment");
                    OnPropertyChanged("PaymentXofY");
                }
            }
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ErrorViewModel _errorvm = new ErrorViewModel(_appvm, _wsdl, _model.ErrorMessage);
                _appvm.MainWindow.InjectWorkSpace(_errorvm);
            }
        }

        private void excelThread_DoWork(object sender, DoWorkEventArgs e)
        {
            _lapseddonors.LoadSpreadsheet();
        }

        private void excelThread_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (_lapseddonors.OnRecord == -1)
            {
                ErrorViewModel _errorvm = new ErrorViewModel(_appvm, _wsdl, _lapseddonors.ErrorMessage);
                _appvm.MainWindow.InjectWorkSpace(_errorvm);
            }
            else
            {
                MaxLapsed = _lapseddonors.MaxRecords;
                ProcessLapsed = e.ProgressPercentage;
                OnPropertyChanged("LapsedXofY");
                OnPropertyChanged("ProcessLapsed");
            }

        }

        private void excelThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            int x = ProcessLapsed;

            if (ProcessLapsed == -1)
            {
                ErrorViewModel _errorvm = new ErrorViewModel(_appvm, _wsdl, _model.ErrorMessage);
                _appvm.MainWindow.InjectWorkSpace(_errorvm);
            }
        }

        #endregion

        #region public properties

        public String DonorXofY
        {
            get {
                if (ProcessDonor > 0)
                    return String.Format("processing donor record {0} of {1}", ProcessDonor, MaxDonors);
                else
                    return String.Empty;
            }
        }

        public String PaymentXofY
        {
            get { if (ProcessPayment > 0)
                    return String.Format("processing payment record {0} of {1}", ProcessPayment, MaxPayments);
                  else
                    return String.Empty;
            }
        }

        public DateTime StartDate
        {
            get { return _startdate; }
            set { _startdate = value; }
        }

        public DateTime EndDate
        {
            get { return _enddate; }
            set { _enddate = value; }
        }

        public int MaxDonors { get; set; }

        public int ProcessDonor
        {
            get { return _currentdonor; }
            set { _currentdonor = value; }
        }

        public int MaxPayments { get; set; }

        public int ProcessPayment
        {
            get { return _currentpayment; }
            set { _currentpayment = value; }
        }

        // Excel Properties

        public int MaxLapsed { get; set; }

        public int ProcessLapsed
        {
            get { return _excelprocess; }
            set { _excelprocess = value; }
        }

        public DateTime ExcelStart
        {
            get { return _excelstart; }
            set { _excelstart = value; }
        }

        public DateTime ExcelEnd
        {
            get { return _excelend; }
            set { _excelend = value; }
        }

        public String LapsedXofY
        {
            get
            {
                if (ProcessLapsed > 0)
                    return String.Format("processing lapsed donor record {0} of {1}", ProcessLapsed, MaxLapsed);
                else
                    return String.Empty;
            }


        }

        #endregion

        #region private methods



        #endregion

        #region Public Methods

        public void ExecuteQuery()
        {
            // Show Donor Count/Progress
            _model.FromDate = StartDate;
            _model.ToDate = EndDate;
            
            _model.ExecuteDonorCount();
            _model.ExecutePaymentCount();

            _currentdonor = 0;
            MaxDonors = _model.CountOfDonors;

            OnPropertyChanged("MaxDonors");
            OnPropertyChanged("ProcessDonor");
            OnPropertyChanged("DonorXofY");

            _currentpayment = 0;
            MaxPayments = _model.CountOfPayments;
            OnPropertyChanged("MaxPayments");
            OnPropertyChanged("ProcessPayment");
            OnPropertyChanged("PaymentXofY");

            if (_model.InError)
            {
                ErrorViewModel _errorvm = new ErrorViewModel(_appvm, _wsdl, _model.ErrorMessage);
                _appvm.MainWindow.InjectWorkSpace(_errorvm);
            }

            worker.RunWorkerAsync();
        }

        public void RunLaspedDonors()
        {
            _lapseddonors = new LapsedDonorsReport(_connection, _appvm.FriendlyUserName);
            _lapseddonors.ExcelThread = excelThread;

            _lapseddonors.StartDate = _excelstart;
            _lapseddonors.EndDate = _excelend;

            _excelprocess = 0;
            MaxLapsed = 0;

            excelThread.RunWorkerAsync();
        }

        public void OpenDeduplicateWorkspace()
        {
            DeduplicationViewModel vm = new DeduplicationViewModel(_appvm, "DeDuplicate", _connection);
            _appvm.MainWindow.InjectWorkSpace(vm);
        }

        public void DownloadAccounts()
        {
            _model.DownloadAllAccounts();
            if (_model.InError)
            {
                ErrorViewModel _errorvm = new ErrorViewModel(_appvm, _wsdl, _model.ErrorMessage);
                _appvm.MainWindow.InjectWorkSpace(_errorvm);
            }
        }

        // 5 K Donors
        public void Write5KDonors()
        {
            // _model.ExecuteFiveKQuery();
            _model.ExecuteTaskQuery();
        }



        #endregion

        #region Relay Commands

        public RelayCommand ExecuteDonorQuery
        {
            get { return _executequery; }
        }

        public RelayCommand ExecuteLapsedDonors
        {
            get { return _executelaspedreport; }
        }

        public RelayCommand ExecuteOpenDeduplicate
        {
            get { return _deduplicateworkspace; }
        }

        public RelayCommand ExecuteDownloadAccounts
        {
            get { return _downloadaccounts; }
        }

        public RelayCommand Execute5KDonorQuery
        {
            get { return _execute5Kdonors; }
        }

        #endregion

    }
}
