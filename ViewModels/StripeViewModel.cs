using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Windows.Data;

using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using OCRMSupportForce.Dialogs;
using esscWPFShell;
using OCRMSupportForce.Models;
using OCRMSupportForce.Views;


namespace OCRMSupportForce.ViewModels
{
    public class StripeViewModel : WorkspaceViewModel
    {
        #region constructor

        public StripeViewModel(ApplicationViewModel MainWindowViewModel, string _displayname)
        {
            _parent = MainWindowViewModel;
            _websupport = new ForceWebSupport();
            this.DisplayName = _displayname;
            
            _websupport = new ForceWebSupport();
            _model = new StripeModel(_websupport);

            // Prep the Relaycommands
            _loadstripefile = new RelayCommand(param => this._openstripeexcelfile());
            _stripefiletodatagrid = new RelayCommand(param => this._executestripefile());
            _executeselectdonor = new RelayCommand(param => this._insertselecteddonor(_selectedsearch, _selected));
            _executepostbatch = new RelayCommand(param => this._posttobatch());
            _updatedsfcontact = new RelayCommand(param => this._updatecontactrecord());
            _close = new RelayCommand(param => this.CloseWorkspace());



            _controller = new BatchPosting(_websupport.webbinding);

            // Prep the combo box
            _model.OpenBatchList(_websupport);
            if (_model.InError)
            {
                ErrorViewModel _errorvm = new ErrorViewModel(_parent, _model.ErrorMessage);
                _parent.MainWindow.InjectWorkSpace(_errorvm);
            }
            else
            {
                OpenBatches = new CollectionView(_model.SF_Batches);
                SelectedBatch = (StripeBatches)OpenBatches.GetItemAt(0);
            }
        }

        #endregion

        #region Private Properties

        private ForceWebSupport _websupport;
        private ApplicationViewModel _parent;
        private StripeModel _model;
        private BatchPosting _controller;


        private RelayCommand _loadstripefile;
        private RelayCommand _stripefiletodatagrid;
        private RelayCommand _executeselectdonor;
        private RelayCommand _executepostbatch;
        private RelayCommand _updatedsfcontact;
        private RelayCommand _close;


        private OpenFileDialog xfile = new OpenFileDialog();
        private CollectionView _comparsionlist;

        private ComparsionClass _selected;
        private CollectionView _searchlist;

        private SForceWebReference.Contact _selectedsearch;

        #endregion

        #region Public Properties

        public CollectionView OpenBatches { get; set; }
        public StripeBatches SelectedBatch { get; set; }
        public CollectionView ComparsionList
        {
            get { return _comparsionlist; }
            set { _comparsionlist = value; }
        }
        public CollectionView SelectionList
        {
            get { return _searchlist; }
            set { _searchlist = value; }
        }

        public ComparsionClass SelectedComparsion
        {
            get 
            {
                return _selected;
            }
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    OnPropertyChanged("StripeFirstName");
                    OnPropertyChanged("StripeLastName");
                    OnPropertyChanged("StripeEmail");
                    OnPropertyChanged("SFDonorID");
                    OnPropertyChanged("StripeAddr");
                    OnPropertyChanged("StripeCity");
                    OnPropertyChanged("StripeState");
                    OnPropertyChanged("StripeZip");

                    OnPropertyChanged("OrginalSFDonorID");
                    OnPropertyChanged("SFEmail");
                    OnPropertyChanged("SFFName");
                    OnPropertyChanged("SFLName");
                    OnPropertyChanged("SFAddr");
                    OnPropertyChanged("SFCity");
                    OnPropertyChanged("SFState");
                    OnPropertyChanged("SFZip");
                }

                if (!(_selected.SFDonorFound))
                {
                    _model.PopulateSearchList(_selected);
                    SelectionList = new CollectionView(_model.SearchList);
                }
                else
                {
                    SelectionList = null;
                }

                OnPropertyChanged("SelectionList");
            }
        }

        public SForceWebReference.Contact SelectedSearch
        {
            get { return _selectedsearch; }
            set { _selectedsearch = value; }
        }

        // Items to use to display comparsion Class
        public String StripeFirstName
        {
            get { return SelectedComparsion.FirstName; }
            set { SelectedComparsion.FirstName = value; }
        }
        public String StripeLastName
        {
            get { return SelectedComparsion.LastName; }
            set { SelectedComparsion.LastName = value; }
        }
        public String StripeEmail
        {
            get { return SelectedComparsion.StripeDonorRecord.CardEmail; }
            set { SelectedComparsion.StripeDonorRecord.CardEmail = value; }
        }
        public String SFDonorID
        {
            get 
            {
                if (SelectedComparsion.SFDonorRecord.Id != null)
                    return SelectedComparsion.SFDonorRecord.Id;
                else
                    return "New Donor";
            }

            set { SelectedComparsion.SFDonorRecord.Id = value; }

        }
        public String StripeAddr
        {
            get { return SelectedComparsion.StripeDonorRecord.CardAddr; }
            set { SelectedComparsion.StripeDonorRecord.CardAddr = value; }
        }
        public String StripeCity
        {
            get { return SelectedComparsion.StripeDonorRecord.CardCity; }
            set { SelectedComparsion.StripeDonorRecord.CardCity = value; }
        }
        public String StripeState
        {
            get { return SelectedComparsion.StripeDonorRecord.CardState; }
            set { SelectedComparsion.StripeDonorRecord.CardState = value; }
        }
        public String StripeZip
        {
            get { return SelectedComparsion.StripeDonorRecord.CardZip; }
            set { SelectedComparsion.StripeDonorRecord.CardZip = value; }
        }

        public String OrginalSFDonorID
        {
            get
            {
                if (SelectedComparsion.SFDonorFound)
                    return SelectedComparsion.SFDonorRecord.Id;
                else
                    return String.Empty;
            }
            set { SelectedComparsion.SFDonorRecord.Id = value; }
        }
        public String SFEmail
        {
            get
            {
                if (SelectedComparsion.SFDonorFound)
                    return SelectedComparsion.SFDonorRecord.Email;
                else
                    return String.Empty;
            }
            set { SelectedComparsion.SFDonorRecord.Email = value; }
        }
        public String SFFName
        {
            get
            {
                if (SelectedComparsion.SFDonorFound)
                    return SelectedComparsion.SFDonorRecord.FirstName;
                else
                    return String.Empty;
            }
            set { SelectedComparsion.SFDonorRecord.FirstName = value; }
        }
        public String SFLName
        {
            get
            {
                if (SelectedComparsion.SFDonorFound)
                    return SelectedComparsion.SFDonorRecord.LastName;
                else
                    return String.Empty;
            }
            set { SelectedComparsion.SFDonorRecord.LastName = value; }
        }
        public String SFAddr
        {
            get
            {
                if (SelectedComparsion.SFDonorFound)
                    return SelectedComparsion.SFDonorRecord.MailingStreet;
                else
                    return String.Empty;
            }
            set { SelectedComparsion.SFDonorRecord.MailingStreet = value; }
        }
        public String SFCity
        {
            get
            {
                if (SelectedComparsion.SFDonorFound)
                    return SelectedComparsion.SFDonorRecord.MailingCity;
                else
                    return String.Empty;
            }
            set { SelectedComparsion.SFDonorRecord.MailingCity = value; }
        }
        public String SFState
        {
            get
            {
                if (SelectedComparsion.SFDonorFound)
                    return SelectedComparsion.SFDonorRecord.MailingState;
                else
                    return String.Empty;
            }
            set { SelectedComparsion.SFDonorRecord.MailingState = value; }
        }
        public String SFZip
        {
            get
            {
                if (SelectedComparsion.SFDonorFound)
                    return SelectedComparsion.SFDonorRecord.MailingPostalCode;
                else
                    return String.Empty;
            }
            set { SelectedComparsion.SFDonorRecord.MailingPostalCode = value; }
        }

        public String ExcelFileName { get; set; }
            
        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        private void _openstripeexcelfile()
        {
            DialogResult result = xfile.ShowDialog();
            if (result == DialogResult.OK)
            {
                ExcelFileName = xfile.FileName;
                this.OnPropertyChanged("ExcelFileName");
            }
        }

        private void _executestripefile()
        {
            _model.ProcessStripeFile(xfile.FileName);

            if (_model.InError)
            {
                ErrorViewModel _errorvm = new ErrorViewModel(_parent, _model.ErrorMessage);
                _parent.MainWindow.InjectWorkSpace(_errorvm);
            }
            else
            {
                _comparsionlist = new CollectionView(_model.ComparsionList);
                OnPropertyChanged("ComparsionList");
            }
        }

        private void _posttobatch()
        {
            // Check if all donors found...
            String SelectedBatchID = SelectedBatch.SalesforceID;

            // Send the model's comparsion list with the batch ID for processing
            _controller.PostToBatch(SelectedBatchID, _model.ComparsionList, xfile.FileName);

            if (_controller.InError)
            {
                ErrorViewModel _errorvm = new ErrorViewModel(_parent, _controller.ErrorMessage);
                _parent.MainWindow.InjectWorkSpace(_errorvm);
            }

            System.Windows.Forms.MessageBox.Show("Finished...");
        }

        private void _updatecontactrecord()
        {
            if (SelectedComparsion != null)
            {
                if (SelectedComparsion.SFDonorRecord != null)
                {
                    if (SelectedComparsion.StripeDonorRecord != null)
                    {
                        // Copy data from Stripe Record to SF Record
                        SelectedComparsion.SFDonorRecord.FirstName = SelectedComparsion.FirstName;
                        SelectedComparsion.SFDonorRecord.LastName = SelectedComparsion.LastName;
                        SelectedComparsion.SFDonorRecord.Email = SelectedComparsion.StripeDonorRecord.CardEmail;

                        SelectedComparsion.SFDonorRecord.MailingStreet = SelectedComparsion.StripeDonorRecord.CardAddr;
                        SelectedComparsion.SFDonorRecord.MailingCity = SelectedComparsion.StripeDonorRecord.CardCity;
                        SelectedComparsion.SFDonorRecord.MailingState = SelectedComparsion.StripeDonorRecord.CardState;
                        SelectedComparsion.SFDonorRecord.MailingPostalCode = SelectedComparsion.StripeDonorRecord.CardZip;

                        OnPropertyChanged("SFEmail");
                        OnPropertyChanged("SFFName");
                        OnPropertyChanged("SFLName");
                        OnPropertyChanged("SFAddr");
                        OnPropertyChanged("SFCity");
                        OnPropertyChanged("SFState");
                        OnPropertyChanged("SFZip");

                        SelectedComparsion.UpdateSFRecord();
                        ComparsionList.Refresh();

                        _model.UpsertContactRecord(SelectedComparsion);
                    }
                }
            }
        }

        private void _insertselecteddonor(SForceWebReference.Contact source, ComparsionClass target)
        {
            target.InsertSFRecord(source);
            // ComparsionList = new CollectionView(_model.ComparsionList);
            ComparsionList.Refresh();

            // Update SF Donor Record & ID
            OnPropertyChanged("SFDonorID");
            OnPropertyChanged("OrginalSFDonorID");
            OnPropertyChanged("SFEmail");
            OnPropertyChanged("SFFName");
            OnPropertyChanged("SFLName");
            OnPropertyChanged("SFAddr");
            OnPropertyChanged("SFCity");
            OnPropertyChanged("SFState");
            OnPropertyChanged("SFZip");

            // Clear SelectionList
            _model.ClearSearchList();
            SelectionList.Refresh();

            OnPropertyChanged("SelectedComparsion");
            OnPropertyChanged("ComparsionList");
        }

        private void CloseWorkspace()
        {
            base.CloseCommand.Execute(null);
        }

        #endregion

        #region Relay Commands

        public RelayCommand OpenExcelFile
        {
            get { return _loadstripefile; }
        }

        public RelayCommand ExecuteStripeFile
        {
            get { return _stripefiletodatagrid; }
        }

        public RelayCommand ExecuteSelectDonor
        {
            get { return _executeselectdonor; }
        }

        public RelayCommand ExecuteToBatch
        {
            get { return _executepostbatch; }
        }

        public RelayCommand UpdateSFContact
        {
            get { return _updatedsfcontact; }
        }

        public RelayCommand Close
        {
            get { return _close; }
        }


        #endregion

    }
}
