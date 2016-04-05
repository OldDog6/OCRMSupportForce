using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Data;

using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using OCRMSupportForce.Dialogs;
using esscWPFShell;
using OCRMSupportForce.Models;
using OCRMSupportForce.Views;
using OCRMSupportForce.SalesForceService;

using Excel = NetOffice.ExcelApi;

namespace OCRMSupportForce.ViewModels
{
    public class DonorDetailViewModel : WorkspaceViewModel
    {
        #region private properties

        // parent
        ApplicationViewModel _parent;
        ForceWSDLSupport _wsdlSupport;
        mySqlModel _sqlconnection;

        // Model and commands
        private DonorDetailModel _donordetailmodel;
        private RelayCommand _executequery;
        private RelayCommand _close;

        // Drop Downs
        List<OrderByColumns> _orderbycolumns = new List<OrderByColumns>();

        #endregion

        #region Creation

        public DonorDetailViewModel(ApplicationViewModel MainWindowViewModel, string _displayname, ForceWSDLSupport forceWSDLSupport, mySqlModel connection)
        {
            this.DisplayName = _displayname;
            _parent = MainWindowViewModel;
            _wsdlSupport = forceWSDLSupport;
            _sqlconnection = connection;

            _donordetailmodel = new DonorDetailModel(_wsdlSupport, _sqlconnection);
            // commands
            _executequery = new RelayCommand(param => this.ExecuteQuery());
            _close = new RelayCommand(param => this.CloseWorkspace());
            // _executequery.CanExecute(false);
        }

        #endregion

        #region Relay Commands
        public RelayCommand ExecuteDonorQuery
        {
            get { return _executequery; }
        }

        public RelayCommand Close
        {
            get { return _close; }
        }

        #endregion

        #region Public Properties

        // drop downs
        public CollectionView MaxRecords
        {
            get { return _donordetailmodel.MaxRecords; }
            set { _donordetailmodel.MaxRecords = value; }
        }

        public CollectionView OrderByColumns
        {
            get { return _donordetailmodel.OrderBy; }
            set { _donordetailmodel.OrderBy = value; }
        }

        public CollectionView Solicitors
        {
            get { return _donordetailmodel.Solicitors; }
            set { _donordetailmodel.Solicitors = value; }
        }

        public bool IndivSelection
        {
            get { return _donordetailmodel.IndivSelection; }
            set { _donordetailmodel.IndivSelection = value; }
        }

        public bool OrgSelection
        {
            get { return _donordetailmodel.OrgSelection; }
            set { _donordetailmodel.OrgSelection = value; }
        }

        public MaxRecordType SelectedMaxRecordItem
        {
            get { return _donordetailmodel.SelectedMaxRecordItem; }
            set { _donordetailmodel.SelectedMaxRecordItem = value; }
        }

        public OrderByColumns SelectedOrderByItem
        {
            get { return _donordetailmodel.SelectedOrderByItem; }
            set { _donordetailmodel.SelectedOrderByItem = value; }
        }

        public Solicitor SelectedSolicitor
        {
            get { return _donordetailmodel.SelectedSolicitor; }
            set { _donordetailmodel.SelectedSolicitor = value; }
        }

        public string RecordCountDescription
        {
            get { return "records: " + _donordetailmodel.DonorResultSetCount.ToString(); }
        }

        public String DonorWildcard
        {
            get { return _donordetailmodel.NameWildcard; }
            set { _donordetailmodel.NameWildcard = value; }
        }

        public String CityFilter
        {
            get { return _donordetailmodel.CityFilter; }
            set { _donordetailmodel.CityFilter = value; }
        }

        public String ZipFilter
        {
            get { return _donordetailmodel.ZipFilter; }
            set { _donordetailmodel.ZipFilter = value; }
        }

        public DateTime? FromDate
        {
            get { return _donordetailmodel.FromDate; }
            set { _donordetailmodel.FromDate = value; }
        }

        public DateTime? ToDate
        {
            get { return _donordetailmodel.ToDate; }
            set { _donordetailmodel.ToDate = value; }
        }

        public bool ExcludeGIK
        {
            get { return _donordetailmodel.ExcludeGiftsInKind; }
            set { _donordetailmodel.ExcludeGiftsInKind = value; }
        }

        public string Minimum
        {
            get { return String.Format("{0:0,0}", _donordetailmodel.MinAmount); }
            set { _donordetailmodel.MinAmount = Convert.ToDecimal(value); }
        }

        public string Maximum
        {
            get { return String.Format("{0:0,0}", _donordetailmodel.MaxAmount); }
            set { _donordetailmodel.MaxAmount = Convert.ToDecimal(value); }
        }

        public int GroupOption
        {
            get { return _donordetailmodel.GroupOptions; }
            set { _donordetailmodel.GroupOptions = value; }
        }

        public DataTable ResultSet
        {
            get { return _donordetailmodel.ReportDataTable; }
        }

        #endregion

        #region Public Methods

        public void ExecuteQuery()
        {
            dlgLongWait waitDlg = new dlgLongWait(_donordetailmodel, this);
            _parent.ShowDialog(waitDlg);
        }

        public void QueryFinished()
        {
            if (_donordetailmodel.InError)
            {
              ErrorViewModel _errorvm = new ErrorViewModel(_parent, _wsdlSupport, _donordetailmodel.ErrorMessage);
             _parent.MainWindow.InjectWorkSpace(_errorvm);
            }
        }

        public void CloseWorkspace()
        {

        }

        #endregion

        #region Private Methods


        #endregion

    }
}
