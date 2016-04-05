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
    public class DonorReportViewModel : WorkspaceViewModel
    {
        #region Constructor

        public DonorReportViewModel(ApplicationViewModel MainWindowViewModel, string _displayname, ForceWSDLSupport forceWSDLSupport)
        {
            _parent = MainWindowViewModel;

            this.DisplayName = _displayname;
            _donorreportmodel = new DonorReportModel(forceWSDLSupport);
            _wsdlSupport = forceWSDLSupport;
           

            // commands
            _executequery = new RelayCommand(param => this.ExecuteQuery());
            _close = new RelayCommand(param => this.CloseWorkspace());
            _pushtoexcel = new RelayCommand(param => this.CopyToExcel());
            _executequery.CanExecute(false);

        }

        #endregion

        #region Private Properties

        // parent
        ApplicationViewModel _parent;
        ForceWSDLSupport _wsdlSupport;


        // Model and commands
        private DonorReportModel _donorreportmodel;
        private RelayCommand _executequery;
        private RelayCommand _pushtoexcel;
        private RelayCommand _close;


        // Drop Downs
        List<OrderByColumns> _orderbycolumns = new List<OrderByColumns>();

        // Excel
        Excel.Style headerstyle;

        #endregion

        #region Public Properties

        // drop downs
        public CollectionView MaxRecords
        {
            get { return _donorreportmodel.MaxRecords; }
            set { _donorreportmodel.MaxRecords = value; }
        }

        public CollectionView OrderByColumns
        {
            get { return _donorreportmodel.OrderBy; }
            set { _donorreportmodel.OrderBy = value; }
        }

        public CollectionView Solicitors
        {
            get { return _donorreportmodel.Solicitors; }
            set { _donorreportmodel.Solicitors = value; }
        }

        public CollectionView OCRMFunds
        {
            get { return _donorreportmodel.OCRMFunds; }
            set { _donorreportmodel.OCRMFunds = value; }
        }

        public bool IndivSelection
        {
            get { return _donorreportmodel.IndivSelection; }
            set { _donorreportmodel.IndivSelection = value; }
        }

        public bool OrgSelection
        {
            get { return _donorreportmodel.OrgSelection; }
            set { _donorreportmodel.OrgSelection = value; }
        }



        public MaxRecordType SelectedMaxRecordItem
        {
            get { return _donorreportmodel.SelectedMaxRecordItem; }
            set { _donorreportmodel.SelectedMaxRecordItem = value; }
        }

        public OrderByColumns SelectedOrderByItem
        {
            get { return _donorreportmodel.SelectedOrderByItem; }
            set { _donorreportmodel.SelectedOrderByItem = value; }
        }

        public Solicitor SelectedSolicitor
        {
            get { return _donorreportmodel.SelectedSolicitor; }
            set { _donorreportmodel.SelectedSolicitor = value; }
        }

        public Funds SelectedFund
        {
            get { return _donorreportmodel.SelectedFund; }
            set { _donorreportmodel.SelectedFund = value; }
        }

        public string RecordCountDescription
        {
            get { return "records: "+ _donorreportmodel.DonorResultSetCount.ToString(); }
        }

        public String DonorWildcard
        {
            get { return _donorreportmodel.NameWildcard; }
            set { _donorreportmodel.NameWildcard = value; }
        }

        public String CityFilter
        {
            get { return _donorreportmodel.CityFilter; }
            set { _donorreportmodel.CityFilter = value; }
        }

        public String ZipFilter
        {
            get { return _donorreportmodel.ZipFilter; }
            set { _donorreportmodel.ZipFilter = value; }
        }

        public DateTime? FromDate
        {
            get { return _donorreportmodel.FromDate; }
            set { _donorreportmodel.FromDate = value; }
        }

        public DateTime? ToDate
        {
            get { return _donorreportmodel.ToDate; }
            set { _donorreportmodel.ToDate = value; }
        }

        public bool ExcludeGIK
        {
            get { return _donorreportmodel.ExcludeGiftsInKind; }
            set { _donorreportmodel.ExcludeGiftsInKind = value; }
        }

        public string Minimum
        {
            get { return String.Format("{0:0,0}", _donorreportmodel.MinAmount); }
            set { _donorreportmodel.MinAmount = Convert.ToDecimal(value); }
        }

        public string Maximum
        {
            get { return String.Format("{0:0,0}", _donorreportmodel.MaxAmount); }
            set { _donorreportmodel.MaxAmount = Convert.ToDecimal(value); }
        }

        public int GroupOption
        {
            get { return _donorreportmodel.GroupOptions; }
            set { _donorreportmodel.GroupOptions = value; }
        }

        public DataTable ResultSet
        {
            get { return _donorreportmodel.ReportDataTable; }
        }

        #endregion

        #region Public Methods

        public void ExecuteQuery()
        {
            dlgWaitMessage waitDlg = new dlgWaitMessage(_donorreportmodel, this);
            _parent.ShowDialog(waitDlg);
        }

        public void FireQuery(DonorReportModel mymodel)
        {
            mymodel.ExecuteQuery();
            OnPropertyChanged("RecordCountDescription");
            OnPropertyChanged("ResultSet");
            if (mymodel.InError)
            {
                ErrorViewModel _errorvm = new ErrorViewModel(_parent, _wsdlSupport, _donorreportmodel.ErrorMessage);
                _parent.MainWindow.InjectWorkSpace(_errorvm);
            }
        }

        public void CopyToExcel()
        {
            try
            {
                var application = new Excel.Application();
                application.Workbooks.Add();

                application.Visible = true;
                application.DisplayAlerts = false;

                var style = application.ActiveWorkbook.Styles.Add("HeaderStyle");
                style.Font.Name = "Verdana";
                style.Font.Size = 10;
                style.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Black);
                style.Font.Bold = true;

                var sheet = (Excel.Worksheet)application.ActiveSheet;

                var tempHeadingArray = new object[_donorreportmodel.ReportDataTable.Columns.Count];
                for (var i = 0; i < _donorreportmodel.ReportDataTable.Columns.Count; i++)
                {
                    tempHeadingArray[i] = _donorreportmodel.ReportDataTable.Columns[i].ColumnName;
                }

                AddHeader(sheet, _donorreportmodel.ReportDataTable, tempHeadingArray);
                AddDataRows(sheet, _donorreportmodel.ReportDataTable, tempHeadingArray);
                AddFooter(sheet, _donorreportmodel.ReportDataTable);
            }
            catch(Exception e)
            {
                ErrorViewModel _errorvm = new ErrorViewModel(_parent, _wsdlSupport, e.ToString());
                _parent.MainWindow.InjectWorkSpace(_errorvm);
            }
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

        public RelayCommand PushToExcel
        {
            get { return _pushtoexcel; }
        }

        #endregion

        #region private Methods

        private void CloseWorkspace()
        {
            base.CloseCommand.Execute(null);
        }

        private void SelectAll()
        {
 
        }

        private void AddHeader(Excel.Worksheet sheet,  DataTable dataset, object[] tempArray)
        {
            Excel.Range header;
            Excel.Range titles;

            header = sheet.Range(sheet.Cells[1, 6], sheet.Cells[1, 6]);
            header.Style = "Title";

            if (IndivSelection)
                header.Value = "OCRM Donor Report - Individuals";
            else
                header.Value = "OCRM Donor Report - Organizations";
            
            titles = sheet.Range(sheet.Cells[6, 1],
                            sheet.Cells[6, (dataset.Columns.Count)]);
            titles.Style = "Heading 2";
            
            sheet.Name = "OCRM Export";

            titles.Value = tempArray;
            AddHeaderParameters(sheet);
        }

        private void AddHeaderParameters(Excel.Worksheet sheet)
        {
            Excel.Range after = sheet.Range("A2");
            after.Style = "Normal";
            after.Value = String.Format("After: {0:M/d/yyyy}", FromDate);

            Excel.Range before = sheet.Range("A3");
            before.Style = "Normal";
            before.Value = String.Format("Before: {0:M/d/yyyy}", ToDate);

            Excel.Range cnt = sheet.Range("A4");
            cnt.Style = "Normal";
            cnt.Value = String.Format("Records: {0}", _donorreportmodel.DonorResultSetCount);

            Excel.Range rdate = sheet.Range("N2");
            rdate.Style = "Normal";
            rdate.Value = String.Format("Run Date: {0:M/d/yyyy}", DateTime.Now);

            Excel.Range rtime = sheet.Range("N3");
            rtime.Style = "Normal";
            rtime.Value = String.Format("Run Date: {0:t}", DateTime.Now);

            Excel.Range bywho = sheet.Range("N4");
            bywho.Style = "Normal";
            bywho.Value = String.Format("By: {0}", _parent.FriendlyUserName);
        }

        private void AddFooter(Excel.Worksheet sheet, DataTable dt)
        {
            Excel.Range totalcol;
            // set default col widths
            sheet.Range("A1").ColumnWidth = 20;
            sheet.Range("B1").ColumnWidth = 8;
            sheet.Range("C1").ColumnWidth = 10;
            sheet.Range("D1").ColumnWidth = 10;
            sheet.Range("E1").ColumnWidth = 16;
            sheet.Range("F1").ColumnWidth = 47;
            sheet.Range("G1").ColumnWidth = 14;
            sheet.Range("H1").ColumnWidth = 28;
            sheet.Range("I1").ColumnWidth = 15;
            sheet.Range("J1").ColumnWidth = 31;
            sheet.Range("K1").ColumnWidth = 27;
            sheet.Range("L1").ColumnWidth = 13;
            sheet.Range("M1").ColumnWidth = 10;
            sheet.Range("N1").ColumnWidth = 18;
            sheet.Range("O1").ColumnWidth = 13;

            // Total Payments Format
            totalcol = sheet.Range(sheet.Cells[7, 1], sheet.Cells[dt.Rows.Count+7,1]);
            totalcol.NumberFormat = "#,##0.00";

            // Min
            totalcol = sheet.Range(sheet.Cells[7, 3], sheet.Cells[dt.Rows.Count + 7, 3]);
            totalcol.NumberFormat = "#,##0.00";

            // Max
            totalcol = sheet.Range(sheet.Cells[7, 4], sheet.Cells[dt.Rows.Count + 7, 4]);
            totalcol.NumberFormat = "#,##0.00";
        }

        private static void AddDataRows(Excel.Worksheet sheet, DataTable dataset, object[] tempArray)
        {
            for(int i = 0; i < dataset.Rows.Count; i++)
            {
                for (int j = 0; j < dataset.Columns.Count; j++)
                {
                    sheet.Cells[(i + 7), (j + 1)].Value = dataset.Rows[i][j];
                }
            }
        }


        #endregion

    }
}
