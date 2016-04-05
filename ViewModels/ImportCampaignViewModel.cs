using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Data;

using esscWPFShell;
using OCRMSupportForce.Models;
using OCRMSupportForce.Views;
using System.Windows.Forms;

using Excel = NetOffice.ExcelApi;

namespace OCRMSupportForce.ViewModels
{
    public class ImportCampaignViewModel : WorkspaceViewModel
    {
        #region Private Properties

        private Excel.Application xapp; 


        // parent
        ApplicationViewModel _parent;
        CampaignModel _campaign;
        ForceWSDLSupport _wsdlconnection;

        private RelayCommand _parentquery;
        private RelayCommand _loadspreadsheet;
        private RelayCommand _executeload;
        
        private List<SalesForceService.Campaign> campaignlist;
        private ObservableCollection<SalesForceService.Campaign> _pushlist;

        //properties

        private String _parentname;
        private SalesForceService.Campaign _parentcampaign;

        // Parent Properties to copy
        String _hierarchy;
        String _campaigndescription;
        String _classificationcode;
        String _campaigntype;
        String _campaignrecordtype = "Appeal";
        String _campaignstatus;
        String _startdate = String.Empty;
        String _enddate = String.Empty;
        String _externalid;

        bool? _campaignisactive = true;

        #endregion

        #region Constructor
        public ImportCampaignViewModel(ApplicationViewModel MainWindowViewModel, string _displayname, ForceWSDLSupport wsdlconnection)
        {
            try
            {
                xapp = new Excel.Application();
            }
            catch(Exception e)
            {
                ErrorViewModel _errorvm = new ErrorViewModel(_parent, _wsdlconnection, e.ToString());
                _parent.MainWindow.InjectWorkSpace(_errorvm);
            }

            _parent = MainWindowViewModel;
            this.DisplayName = _displayname;

            _wsdlconnection = wsdlconnection;

            _campaign = new CampaignModel(_wsdlconnection);
 
            // commands
            _parentquery = new RelayCommand(param => this.parentquery());
            _loadspreadsheet = new RelayCommand(param => this.loadspreadsheet());
            _executeload = new RelayCommand(param => this.executeload());
//            _close = new RelayCommand(param => this.CloseWorkspace());
//            _executequery.CanExecute(false);

        }

        #endregion

        #region public properties

        // Find parameters
        public String ParentName
        {
            get { return _parentname; }
            set { _parentname = value; }
        }

        // Parent property override
        public String Hierachy
        {
            get { return _hierarchy; }
            set { _hierarchy = value; }
        }

        public String CampaignDescription
        {
            get { return _campaigndescription; }
            set { _campaigndescription = value;
            OnPropertyChanged("CampaignDescription");
            }
        }

        public String ClassificationCode
        {
            get { return _classificationcode; }
            set { _classificationcode = value; }
        }

        public String CampaignType
        {
            get { return _campaigntype; }
            set { _campaigntype = value; }
        }

        public String CampaignRecordType
        {
            get { return _campaignrecordtype; }
            set { _campaignrecordtype = value; }
        }

        public String CampaignStatus
        {
            get { return _campaignstatus; }
            set { _campaignstatus = value; }
        }

        public String IsActive
        {
            get
            {
                        return "True";
            }

        }

        public String StartDate
        {
            get { return _startdate; }
            set { _startdate = value; }
        }

        public String EndDate
        {
            get { return _enddate; }
            set { _enddate = value; }
        }

        public String ExternalId
        {
            get { return _externalid; }
            set { _externalid = value; }
        }

        public ObservableCollection<SalesForceService.Campaign> DisplayFromSpreadsheet
        {
            get { return _pushlist; }
        }

        #endregion

        #region private methods

        private void parentquery()
        {
            _parentcampaign = _campaign.GetParentCampaign(_parentname);

            if (_campaign.InError)
            {
                ErrorViewModel _errorvm = new ErrorViewModel(_parent, _wsdlconnection, _campaign.ErrorMessage);
                _parent.MainWindow.InjectWorkSpace(_errorvm);
            }           
            else
            { 
                campaignlist = _campaign.GetParentCampaignList(_parentcampaign);
                _hierarchy = _parentcampaign.Id;
                _classificationcode = _parentcampaign.Classification_Code__c;
                _campaigndescription = _parentcampaign.Description;
                _campaigntype = _parentcampaign.Type;

                _campaignstatus = _parentcampaign.Status;
                _campaignisactive = _parentcampaign.IsActive;
                _startdate = String.Format("{0:MM/dd/yy}", _parentcampaign.StartDate);
                _enddate = String.Format("{0:MM/dd/yy}", _parentcampaign.EndDate);
                _externalid = _parentcampaign.ExternalId__c;

                OnPropertyChanged("Hierachy");
                OnPropertyChanged("CampaignDescription");
                OnPropertyChanged("ClassificationCode");
                OnPropertyChanged("CampaignType");
                OnPropertyChanged("CampaignStatus");
                OnPropertyChanged("IsActive");
                OnPropertyChanged("StartDate");
                OnPropertyChanged("EndDate");
                OnPropertyChanged("ExternalId");
            }
        }

        private void loadspreadsheet()
        {
            _pushlist = new ObservableCollection<SalesForceService.Campaign>();

            OpenFileDialog dlg = new OpenFileDialog();

            DialogResult result = dlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                readspreadsheet(dlg.FileName);
            }
            OnPropertyChanged("DisplayFromSpreadsheet");
        }

        private void executeload()
        {
            updatedescriptions();

            foreach (SalesForceService.Campaign c in _pushlist)
            {
                if (!(_campaign.InError))
                    _campaign.InsertChildCampaign(_parentcampaign,c);
            }

            if (!(_campaign.InError))
            {
                _pushlist.Clear();
                OnPropertyChanged("DisplayFromSpreadsheet");
            }
            else
            {
                ErrorViewModel _errorvm = new ErrorViewModel(_parent, _wsdlconnection, _campaign.ErrorMessage);
                _parent.MainWindow.InjectWorkSpace(_errorvm);
            }
        }

        private void readspreadsheet(string filename)
        {
            Excel.Workbook workbook = xapp.Workbooks.Open(filename);
            Excel.Sheets excelSheets = workbook.Worksheets;

            Excel.Worksheet WorkSheet = (Excel.Worksheet)workbook.ActiveSheet;
            Excel.Range excelCell = (Excel.Range) WorkSheet.get_Range("A1", "A1");

            int i = 1;
            String value = excelCell.Value.ToString();

            while ((value != String.Empty) && (i < 50))
            {
                insertpushitem(excelCell.Value.ToString());
                i++;
                excelCell = (Excel.Range) WorkSheet.get_Range("A"+i.ToString(), "A"+i.ToString());

                if (excelCell.Value == null)
                    break;

                value = excelCell.Value.ToString();
            }
            xapp.Workbooks.Close();
        }

        private void insertpushitem(String newname)
        {
            SalesForceService.Campaign newCampaign = new SalesForceService.Campaign();

            newCampaign.Name = newname;
            newCampaign.Classification_Code__c = _classificationcode;
            newCampaign.Description = _campaigndescription;
            newCampaign.IsActive = (bool?) true;
            newCampaign.StartDate = (DateTime?)_parentcampaign.StartDate;
            newCampaign.ExternalId__c = _externalid;
            newCampaign.Type = _campaigntype;
            newCampaign.Status = _campaignstatus;
            newCampaign.RecordTypeId = "012500000001P2tAAE";

            newCampaign.IsActiveSpecified = true;
            newCampaign.StartDateSpecified = true;

            _pushlist.Add(newCampaign);
        }

        private void updatedescriptions()
        {
            foreach(SalesForceService.Campaign c in _pushlist)
            {
                c.Description = _campaigndescription;
            }
        }

        #endregion

        #region Relay Commands

        public RelayCommand FindParentQuery
        {
            get { return _parentquery; }
        }

        public RelayCommand LoadSpreadsheet
        {
            get { return _loadspreadsheet; }
        }

        public RelayCommand Execute
        {
            get { return _executeload; }
        }

        #endregion
    }
}
