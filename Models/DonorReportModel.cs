using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Data;
using System.Data;
using esscWPFShell;
using OCRMSupportForce.SalesForceService;

using System.ComponentModel;
using System.Windows.Threading;
using System.ServiceModel;
using System.Xml;
using System.Net;
using System.IO;
using System.Configuration;

namespace OCRMSupportForce.Models
{
    public class DonorReportModel
    {
        #region Creation

        public DonorReportModel(ForceWSDLSupport sfdcService)
        {
            _sfdcService = sfdcService;

            _downloads = new SupportingDownloads(_sfdcService);

            _reportdatatable = new DataTable("Donors");

            _excludegiftsinkind = true;
            _fromdate = DateTime.Now.AddDays(-30);
            _todate = DateTime.Now;

            // Drop Down Prep
            _maxrecordtypes.Add(new MaxRecordType(10, "Max 10 Records"));
            _maxrecordtypes.Add(new MaxRecordType(10, "Max 100 Records"));
            _maxrecordtypes.Add(new MaxRecordType(1000, "Max 1,000 Records"));
            _maxrecordtypes.Add(new MaxRecordType(1500, "Max 1,500 Records"));
            _maxrecordtypes.Add(new MaxRecordType(2000, "Max 2,000 Records"));

            MaxRecords = new CollectionView(_maxrecordtypes);
            SelectedMaxRecordItem = (MaxRecordType) MaxRecords.GetItemAt(4);

            _orderbycolumns.Add(new OrderByColumns(" order by sum(causeview__amount__c) desc", "Sum Of Amount Descending"));
            _orderbycolumns.Add(new OrderByColumns(" order by sum(causeview__amount__c) asc", "Sum Of Amount Ascending"));

            OrderBy = new CollectionView(_orderbycolumns);
            SelectedOrderByItem = (OrderByColumns)OrderBy.GetItemAt(0);


            // Populate Solicitors List
            _solicitors.Add(new Solicitor(String.Empty, String.Empty, String.Empty, "All Solicitors"));
            _downloads.LoadSolicitorsList(_solicitors);

            Solicitors = new CollectionView(_solicitors);

            SelectedSolicitor = (Solicitor)Solicitors.GetItemAt(0);

            _funds.Add(new Funds(String.Empty,String.Empty,String.Empty, "All Funds"));
            _downloads.LoadFundList(_funds);

            OCRMFunds = new CollectionView(_funds);
            SelectedFund = (Funds)OCRMFunds.GetItemAt(0);

            // DataTable Prep
            _reportdatatable = new DataTable("Donors");

            
            _reportdatatable.Columns.Add("Sum_Of_Payments");
            _reportdatatable.Columns["Sum_Of_Payments"].DataType = System.Type.GetType("System.Decimal");

            _reportdatatable.Columns.Add("Freq");
            _reportdatatable.Columns.Add("Min");
            _reportdatatable.Columns["Min"].DataType = System.Type.GetType("System.Decimal");

            _reportdatatable.Columns.Add("Max");
            _reportdatatable.Columns["Max"].DataType = System.Type.GetType("System.Decimal");

            _reportdatatable.Columns.Add("Payment_Date");
            _reportdatatable.Columns["Payment_Date"].DataType = System.Type.GetType("System.DateTime");

            _reportdatatable.Columns.Add("Donor");
            _reportdatatable.Columns.Add("Phone");
            _reportdatatable.Columns.Add("Email");
 
            _reportdatatable.Columns.Add("Solicitor");

            _reportdatatable.Columns.Add("Date_of_First_Gift");
            _reportdatatable.Columns["Date_of_First_Gift"].DataType = System.Type.GetType("System.DateTime");

            _reportdatatable.Columns.Add("Address");
            _reportdatatable.Columns.Add("City");
            _reportdatatable.Columns.Add("State");
            _reportdatatable.Columns.Add("Zip");
            _reportdatatable.Columns.Add("Gift_Type");
            _reportdatatable.Columns.Add("Fund");
        }

        #endregion

        #region Private Properties

        List<MaxRecordType> _maxrecordtypes = new List<MaxRecordType>();
        List<OrderByColumns> _orderbycolumns = new List<OrderByColumns>();
        List<Solicitor> _solicitors = new List<Solicitor>();
        List<Funds> _funds = new List<Funds>();

        private DataTable _reportdatatable;
        private DataTable _importdatatable;

        // WSDL Properties
        private ForceWSDLSupport _sfdcService;
        private SalesForceService.QueryResult _myresult;
        private SupportingDownloads _downloads;
       
        // filters
        private string _namewildcard = string.Empty;
        private String _cityfilter = String.Empty;
        private String _zipfilter = String.Empty;

        private bool _excludegiftsinkind = false;

        private DateTime? _fromdate = null;
        private DateTime? _todate = null;

        private int _groupoptions = 0;

        private decimal _minamount = 25.00m;
        private decimal _maxamount = 90000000.00m;

        private bool _indivselection = true;
        private bool _orgselection = false;
        private bool _churchselection = false;

        private String _errormessage = String.Empty;
        private bool _inerror = false;

        #endregion

        #region Private Methods
        private void AppendResultToReportDataTable(QueryResult result)
        {
            SalesForceService.sObject[] records = result.records;

            for (int i = 0; i < result.records.Count(); i++)
            {
                SalesForceService.AggregateResult c = (SalesForceService.AggregateResult)result.records[i];

                DataRow row = _reportdatatable.NewRow();

                // New limit logic...
                decimal sumofpayments = Convert.ToDecimal(c.Any[0].InnerXml);
                row["Sum_Of_Payments"] = String.Format("{0:#,##}", Convert.ToDecimal(c.Any[0].InnerXml));
                row["Freq"] = String.Format("{0:0}", Convert.ToInt32(c.Any[4].InnerXml));
                row["Min"] = String.Format("{0:#,##}", Convert.ToDecimal(c.Any[3].InnerXml));
                row["Max"] = String.Format("{0:#,##}", Convert.ToDecimal(c.Any[2].InnerXml));
                row["Payment_Date"] = String.Format("{0:MM/dd/yyyy}", Convert.ToDateTime(c.Any[1].InnerXml));

                row["Donor"] = c.Any[5].InnerXml;
                row["Address"] = c.Any[6].InnerXml;
                row["City"] = c.Any[7].InnerXml;
                row["State"] = c.Any[9].InnerXml;
                row["Zip"] = c.Any[8].InnerXml;

                if (_indivselection)
                {
                    row["Email"] = c.Any[10].InnerXml;
                    row["Phone"] = c.Any[11].InnerXml;
                    row["Solicitor"] = c.Any[12].InnerXml;

                    if (c.Any[13].InnerXml != String.Empty)
                        row["Date_of_First_Gift"] = Convert.ToDateTime(c.Any[13].InnerXml);

                    row["Gift_Type"] = c.Any[14].InnerXml;
                    row["Fund"] = c.Any[15].InnerText;
                }
                else
                {
                    row["Solicitor"] = c.Any[11].InnerXml;
                    if (c.Any[12].InnerXml != String.Empty)
                        row["Date_of_First_Gift"] = Convert.ToDateTime(c.Any[12].InnerXml);

                    row["Phone"] = c.Any[10].InnerXml;
                    row["Gift_Type"] = c.Any[13].InnerXml;
                    row["Fund"] = c.Any[14].InnerText;
                }

                _reportdatatable.Rows.Add(row);
            }
        }

        private string ChurchBuildWhereClause()
        {
            string whereclause = string.Empty;

            whereclause = "where causeview__payment__r.causeview__Donation__r.causeview__Organization__c <> null ";
            whereclause = whereclause + " and causeview__payment__r.causeview__Donation__r.causeview__Organization__r.Personal_Code__C ='Church/Ministry' ";

            if (FromDate != null)
            {
                whereclause = whereclause + String.Format("and causeview__payment__r.causeview__Date__c > {0:yyyy-MM-dd} ", FromDate);
            }

            if (ToDate != null)
            {
                whereclause = whereclause + String.Format("and causeview__payment__r.causeview__Date__c < {0:yyyy-MM-dd} ", ToDate);
            }

            if (ExcludeGiftsInKind)
            {
                whereclause = whereclause + " and causeview__payment__r.causeview__Donation__r.causeview__Gift_Type__c <> 'Gift in Kind' ";
                whereclause = whereclause + " and causeview__Fund__r.causeview__Fund_Name__c <> 'Inkind Food' ";
                whereclause = whereclause + " and causeview__Fund__r.causeview__Fund_Name__c <> 'Laurel House Gift In Kind\\'s' ";
                whereclause = whereclause + " and causeview__Fund__r.causeview__Fund_Name__c <> 'Inkind Clothing' ";
                whereclause = whereclause + " and causeview__Fund__r.causeview__Fund_Name__c <> 'Inkind Misc' ";
                whereclause = whereclause + " and causeview__Fund__r.causeview__Fund_Name__c <> 'CNRM Gift In Kinds' ";
            }

            // Always have a min
            whereclause = whereclause + String.Format("and causeview__payment__r.causeview__amount__c >= {0:0.00} ", 1);

            // Restriction by Solicitor
            whereclause = whereclause + SelectedSolicitor.OrgClause;
            whereclause = whereclause + SelectedFund.OrgClause;

            // Restriction by City & Zipcode
            if (_cityfilter != String.Empty)
            {
                whereclause = whereclause + String.Format(" and causeview__payment__r.causeview__Donation__r.causeview__Organization__r.BillingCity Like '%{0}%'", _cityfilter);
            }

            if (_zipfilter != String.Empty)
            {
                whereclause = whereclause + String.Format(" and causeview__payment__r.causeview__Donation__r.causeview__Organization__r.BillingPostalCode Like '%{0}%'", _zipfilter);
            }

            if (_namewildcard != String.Empty)
            {
                whereclause = whereclause + String.Format(" and causeview__payment__r.causeview__Donation__r.causeview__Organization__r.name Like '%{0}%'", _namewildcard);
            }

            return whereclause;
        }

        private string OrgBuildWhereClause()
        {
            string whereclause = string.Empty;

            whereclause = "where causeview__payment__r.causeview__Donation__r.causeview__Organization__c <> null ";

            if (FromDate != null)
            {
                whereclause = whereclause + String.Format("and causeview__payment__r.causeview__Date__c > {0:yyyy-MM-dd} ", FromDate);
            }

            if (ToDate != null)
            {
                whereclause = whereclause + String.Format("and causeview__payment__r.causeview__Date__c < {0:yyyy-MM-dd} ", ToDate);
            }

            if (ExcludeGiftsInKind)
            {
                whereclause = whereclause + " and causeview__payment__r.causeview__Donation__r.causeview__Gift_Type__c <> 'Gift in Kind' ";
                whereclause = whereclause + " and causeview__Fund__r.causeview__Fund_Name__c <> 'Inkind Food' ";
                whereclause = whereclause + " and causeview__Fund__r.causeview__Fund_Name__c <> 'Laurel House Gift In Kind\\'s' ";
                whereclause = whereclause + " and causeview__Fund__r.causeview__Fund_Name__c <> 'Inkind Clothing' ";
                whereclause = whereclause + " and causeview__Fund__r.causeview__Fund_Name__c <> 'Inkind Misc' ";
                whereclause = whereclause + " and causeview__Fund__r.causeview__Fund_Name__c <> 'CNRM Gift In Kinds' "; 
            }

                // Always have a min
            whereclause = whereclause + String.Format("and causeview__payment__r.causeview__amount__c >= {0:0.00} ", 1);

            // Restriction by Solicitor
            whereclause = whereclause + SelectedSolicitor.OrgClause;
            whereclause = whereclause + SelectedFund.OrgClause;

            // Restriction by City & Zipcode
            if (_cityfilter != String.Empty)
            {
                whereclause = whereclause + String.Format(" and causeview__payment__r.causeview__Donation__r.causeview__Organization__r.BillingCity Like '%{0}%'", _cityfilter);
            }

            if (_zipfilter != String.Empty)
            {
                whereclause = whereclause + String.Format(" and causeview__payment__r.causeview__Donation__r.causeview__Organization__r.BillingPostalCode Like '%{0}%'", _zipfilter);
            }

            if (_namewildcard != String.Empty)
            {
                whereclause = whereclause + String.Format(" and causeview__payment__r.causeview__Donation__r.causeview__Organization__r.name Like '%{0}%'", _namewildcard);
            }
            
            return whereclause;
        }

        private string IndivBuildWhereClause()
        {
            string whereclause = string.Empty;

            whereclause = "where causeview__payment__r.causeview__Donation__r.causeview__Constituent__c <> null ";

            if (FromDate != null)
            {
                whereclause = whereclause + String.Format("and causeview__payment__r.causeview__Date__c > {0:yyyy-MM-dd} ", FromDate); 
            }

            if (ToDate != null)
            {
                whereclause = whereclause + String.Format("and causeview__payment__r.causeview__Date__c < {0:yyyy-MM-dd} ", ToDate); 
            }

            if (ExcludeGiftsInKind)
            {
                whereclause = whereclause + " and causeview__payment__r.causeview__Donation__r.causeview__Gift_Type__c <> 'Gift in Kind' ";
                whereclause = whereclause + " and causeview__Fund__r.causeview__Fund_Name__c <> 'Inkind Food' ";
                whereclause = whereclause + " and causeview__Fund__r.causeview__Fund_Name__c <> 'Laurel House Gift In Kind\\'s' ";
                whereclause = whereclause + " and causeview__Fund__r.causeview__Fund_Name__c <> 'Inkind Clothing' ";
                whereclause = whereclause + " and causeview__Fund__r.causeview__Fund_Name__c <> 'Inkind Misc' ";
                whereclause = whereclause + " and causeview__Fund__r.causeview__Fund_Name__c <> 'CNRM Gift In Kinds' "; 
            }

            // Always have a min
            whereclause = whereclause + String.Format("and causeview__payment__r.causeview__amount__c >= {0:0.00}  ", 1);

            // Restriction by Solicitor
            whereclause = whereclause + SelectedSolicitor.Clause;

            whereclause = whereclause + SelectedFund.Clause;

            // Restriction by City & Zipcode
            if (_cityfilter != String.Empty)
            {
                whereclause = whereclause + String.Format(" and causeview__payment__r.causeview__Donation__r.causeview__Constituent__r.mailingCity Like '%{0}%'", _cityfilter);
            }

            if (_zipfilter != String.Empty)
            {
                whereclause = whereclause + String.Format(" and causeview__payment__r.causeview__Donation__r.causeview__Constituent__r.mailingPostalCode Like '%{0}%'", _zipfilter);
            }

            if (_namewildcard != String.Empty)
            {
                whereclause = whereclause + String.Format(" and causeview__payment__r.causeview__Donation__r.causeview__Constituent__r.causeview__primary_Addressee__c Like '%{0}%'", _namewildcard);
            }

            return whereclause;
        }

        private string LimitClause()
        {
            return String.Format(" Limit {0:0}", SelectedMaxRecordItem.MaxRecords);
        }

        private string BuildHavingClause()
        {
            return String.Format(" having (sum(causeview__payment__r.causeview__Amount__c) > {0:0} and sum(causeview__payment__r.causeview__Amount__c) < {1:0}) ", _minamount, _maxamount);
        }

        #endregion

        #region Public Methods

        public void ExecuteQuery()
        {
            QueryOptions options = new QueryOptions();
            options.batchSize = 250;

            _reportdatatable.Rows.Clear();

            // Aggreate Queries cannot use QueryMore approach
            try
            {
                EndpointAddress apiAddr = new EndpointAddress(_sfdcService.ServerURL);
                SalesForceService.SessionHeader header = new SessionHeader();
                header.sessionId = _sfdcService.SessionID;

                SalesForceService.SoapClient queryClient = new SalesForceService.SoapClient("Soap", apiAddr);
                string query = String.Empty;

                if (_indivselection)
                {
                    query = Strings.INQSelectClause+" "+ IndivBuildWhereClause() + " " + Strings.INQGroupClause + " " + BuildHavingClause() +  SelectedOrderByItem.Clause + LimitClause();
                }
                else if (_orgselection)
                {
                    query = Strings.OrgQSelectClause + " " + OrgBuildWhereClause() + " " + Strings.OrgQGroupClause + BuildHavingClause() + SelectedOrderByItem.Clause + " " + LimitClause();
                }
                else
                {
                    query = Strings.OrgQSelectClause + " " + ChurchBuildWhereClause() + "  " + Strings.OrgQGroupClause + BuildHavingClause() + SelectedOrderByItem.Clause + " " + LimitClause();
                }

                queryClient.query(header,
                                     options,
                                     null,
                                     null,
                                     query,
                                     out _myresult);

                bool done = false;

                if (_myresult.records == null)
                    done = true;

                while (!done)
                {
                    AppendResultToReportDataTable(_myresult);

                    if ((_reportdatatable.Rows.Count > SelectedMaxRecordItem.MaxRecords) || (_myresult.done))
                    {
                        done = true;
                    }
                    else
                    {
                        string rval = String.Empty;
                            
                        queryClient.queryMore(header, options, _myresult.queryLocator, out _myresult);
                    }
                }
                _inerror = false;
            }
            catch (Exception e)
            {
                _inerror = true;
                _errormessage = e.ToString();
            }
        }

        #endregion

        #region Public Properties
       
        //Drop downs
        public CollectionView MaxRecords { get; set; }
        public CollectionView OrderBy { get; set; }
        public CollectionView Solicitors { get; set; }
        public CollectionView OCRMFunds { get; set; }


        public int DonorResultSetCount
        {
            get
            {
                if (_reportdatatable == null)
                    return 0;
                else
                    return _reportdatatable.Rows.Count; 
            }
        }

        public MaxRecordType SelectedMaxRecordItem { get; set; }

        public OrderByColumns SelectedOrderByItem { get; set; }

        public Solicitor SelectedSolicitor { get; set; }

        public Funds SelectedFund { get; set; }

        public DataTable ReportDataTable
        {
            get { return _reportdatatable; }
            set { _reportdatatable = value; }
        }

        public DataTable ImportDataTable
        {
            get { return _importdatatable; }
            set { _importdatatable = value; }
        }

        // filters
        public bool IndivSelection
        {
            get { return _indivselection; }
            set { _indivselection = value; }
        }

        public bool OrgSelection
        {
            get { return _orgselection; }
            set { _orgselection = value; }
        }

        public bool ChurchSelection
        {
            get { return _churchselection; }
            set { _churchselection = value; }
        }

        public string NameWildcard
        {
            get { return _namewildcard; }
            set { _namewildcard = value; }
        }

        public String CityFilter
        {
            get { return _cityfilter; }
            set { _cityfilter = value; }
        }

        public String ZipFilter
        {
            get { return _zipfilter; }
            set { _zipfilter = value; }
        }

        public bool ExcludeGiftsInKind
        {
            get { return _excludegiftsinkind; }
            set { _excludegiftsinkind = value; }
        }

        public string DisplayFromDate
        {
            get
            {
                if (_fromdate == null)
                    return "Any";
                else
                    return String.Format("mm/dd/yy", _fromdate);
            }
        }

        public string DisplayToDate
        {
            get
            {
                if (_todate == null)
                    return "Any";
                else
                    return String.Format("mm/dd/yy", _todate);
            }
        }

        public DateTime? FromDate
        {
            get { return _fromdate; }
            set { _fromdate = value; }
        }

        public DateTime? ToDate
        {
            get { return _todate; }
            set { _todate = value; }
        }
        
        public decimal MinAmount
        {
            get { return _minamount; }
            set { _minamount = value; }
        }

        public decimal MaxAmount
        {
            get { return _maxamount; }
            set { _maxamount = value; }
        }

        public int GroupOptions
        {
            get { return _groupoptions; }
            set { _groupoptions = value; }
        }

        // date filter control elements
        public bool FromDateChosen
        {
            get { return _fromdate != null; }
        }

        public bool ToDateChosen
        {
            get { return _todate != null; }
        }

        public String ErrorMessage
        {
            get { return _errormessage; }
        }

        public bool InError
        {
            get { return _inerror; }
        }

        #endregion
    }
}
