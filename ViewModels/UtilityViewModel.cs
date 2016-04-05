using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Forms;
using System.Data;

using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using OCRMSupportForce.Dialogs;
using esscWPFShell;
using OCRMSupportForce.Models;
using OCRMSupportForce.Views;

namespace OCRMSupportForce.ViewModels
{
    public class UtilityViewModel : WorkspaceViewModel
    {
        public UtilityViewModel(ApplicationViewModel MainWindowViewModel, string _displayname, ForceWSDLSupport forceWSDLSupport)
        {
            _parent = MainWindowViewModel;
            this.DisplayName = _displayname;

            _solicitormod = new RelayCommand(param => this._executeModSolicitors());
            _loadbatchlist = new RelayCommand(param => this._loadbatchstripelist());
            _loadstripefile = new RelayCommand(param => this._openstripeexcelfile());
            _exestripefile = new RelayCommand(param => this._executestripefile());

            _forceconnection = forceWSDLSupport;
            _websupport = new ForceWebSupport();

            // create the batch model instance
            _stripeuploadmodel = new UploadStripeData(_websupport);

            xfile = new OpenFileDialog();
            xfile.DefaultExt = "xls";
        }

        #region Private Properties

        private RelayCommand _solicitormod;
        private RelayCommand _loadbatchlist;
        private RelayCommand _loadstripefile;
        private RelayCommand _exestripefile;

        private ApplicationViewModel _parent;

        private ForceWSDLSupport _forceconnection;
        private ForceWebSupport _websupport;

        private UploadStripeData _stripeuploadmodel;

        private OpenFileDialog xfile;

        #endregion

        #region Private methods

        private void _executeModSolicitors()
        {
            ModifySolicitors m = new ModifySolicitors(_websupport);
            m.LoadContactList("Melissa Stupfel");
        }

        // Stripe methods
        private void _loadbatchstripelist()
        {
            _stripeuploadmodel.LoadBatchList();
            if (_stripeuploadmodel.OpenBatchesList != null)
            {
                this.OnPropertyChanged("DisplayBatches");
            }
        }
        
        private void _openstripeexcelfile()
        {
            if (SelectedRow["id"] != null)
            {
                this.OnPropertyChanged("DisplayBatches");
                String BatchID = SelectedRow["id"].ToString();

                DialogResult result = xfile.ShowDialog();
                if (result == DialogResult.OK)
                {
                    ExcelFileName = xfile.FileName;
                    this.OnPropertyChanged("ExcelFileName");

                }
            }
        }

        private void _executestripefile()
        {
            String selectedID = SelectedRow["BatchId"].ToString();

            _stripeuploadmodel.OpenExcelFile(selectedID);

            if (_websupport.InError)
            {
                ErrorViewModel _errorvm = new ErrorViewModel(_parent, _websupport.ErrorMessage);
                _parent.MainWindow.InjectWorkSpace(_errorvm);


            }
        }

        // end stripe methods 

        #endregion

        #region Public Properties

        public DataTable DisplayBatches
        {
            get { return _stripeuploadmodel.OpenBatchesList;}
        }

        public DataRowView SelectedRow { get; set; }

        public String ExcelFileName
        {
            get { return _stripeuploadmodel.ExcelFileName; }
            set { _stripeuploadmodel.ExcelFileName = value; }
        }

        #endregion

        #region Relay Commands
        public RelayCommand ExecuteModifySolicitors
        {
            get { return _solicitormod; }
        }

        public RelayCommand LoadBatchList
        {
            get { return _loadbatchlist; }
        }

        public RelayCommand OpenExcelFile
        {
            get { return _loadstripefile; }
        }

        public RelayCommand ExecuteStripeFile
        {
            get { return _exestripefile; }
        }

        #endregion
    }
}
