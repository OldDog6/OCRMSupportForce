using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using MySql.Data.MySqlClient;
using esscWPFShell;
using OCRMSupportForce.SalesForceService;

using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Threading;
using System.ServiceModel;
using System.Xml;
using System.Net;
using System.IO;
using System.Configuration;

namespace OCRMSupportForce.Models
{
    public class DonorDetailModel
    {
        #region Creation

        public DonorDetailModel(ForceWSDLSupport sfdcService, mySqlModel connection)
        {
            _sfdcService = sfdcService;
            _connection = connection;

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

            _solicitors.Add(new Solicitor(String.Empty, String.Empty, String.Empty, "All"));
            _downloads.LoadSolicitorsList(_solicitors);

            Solicitors = new CollectionView(_solicitors);

            SelectedSolicitor = (Solicitor)Solicitors.GetItemAt(0);

            // DataTable Prep
            _reportdatatable = new DataTable("Donors");

            _reportdatatable.Columns.Add("Donor");
            _reportdatatable.Columns.Add("Donation_Amount");
            _reportdatatable.Columns["Donation_Amount"].DataType = System.Type.GetType("System.Double");

            _reportdatatable.Columns.Add("Freq");
            _reportdatatable.Columns.Add("Min");
            _reportdatatable.Columns["Min"].DataType = System.Type.GetType("System.Double");

            _reportdatatable.Columns.Add("Max");
            _reportdatatable.Columns["Max"].DataType = System.Type.GetType("System.Double");

            _reportdatatable.Columns.Add("Payment_Date");
            _reportdatatable.Columns["Payment_Date"].DataType = System.Type.GetType("System.DateTime");
            _reportdatatable.Columns.Add("Solicitor");
            _reportdatatable.Columns.Add("Address");
            _reportdatatable.Columns.Add("City");
            _reportdatatable.Columns.Add("State");
            _reportdatatable.Columns.Add("Zip");
            _reportdatatable.Columns.Add("Email");
            _reportdatatable.Columns.Add("Phone");
            _reportdatatable.Columns.Add("Gift_Type");
            _reportdatatable.Columns.Add("Fund");
        }

        #endregion
        
        #region Private Properties

        List<MaxRecordType> _maxrecordtypes = new List<MaxRecordType>();
        List<OrderByColumns> _orderbycolumns = new List<OrderByColumns>();
        List<Solicitor> _solicitors = new List<Solicitor>();

        private mySqlModel _connection;

        private DataTable _reportdatatable;
        private DataTable _importdatatable;

        private BackgroundWorker _worker;

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

        private String _errormessage = String.Empty;
        private bool _inerror = false;

        private int _totaldonors = 0;
        private int _processeddonors = 0;
        private int _totalpayments = 0;
        private int _processedpayments = 0;
        private int _totalcampaigns = 0;
        private int _processedcampaigns = 0;

        private bool _ondonors = true;

        private int _processingstate = 0;

        #endregion

        #region Public Properties

        //Drop downs
        public CollectionView MaxRecords { get; set; }
        public CollectionView OrderBy { get; set; }
        public CollectionView Solicitors { get; set; }

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

        public BackgroundWorker WorkerThread
        {
            get { return _worker; }
            set { _worker = value; }
        }

        public int TotalDonors
        {
            get { return _totaldonors; }
        }
 
        public int TotalPayments
        {
            get { return _totalpayments; }
        }

        public int TotalCampaigns
        {
            get { return _totalcampaigns; }
        }

        public bool ProcessingDonors
        {
            get { return _ondonors; }
        }

        public int ProcessingState
        {
            get { return _processingstate; }
            set { _processingstate = value; }
        }

        #endregion

        #region Public Methods

        public void ExecuteQuery()
        {
            // Reset Progress
            _processeddonors = 0;
            _processedpayments = 0;
            _processedcampaigns = 0;
            _ondonors = true;
            _processingstate = 0;

            QueryRecentDonors();
            _ondonors = false;

            _processingstate = 1;
            QueryRecentPayments();

//            _processingstate = 2;
//            QueryCampaigns();
        }

        public void LoadTotalDonors()
        {
            _totaldonors = DonorsInScope();
        }

        public void LoadTotalPayments()
        {
            _totalpayments = PaymentsInScope();
        }

        #endregion

        #region private methods

        private int DonorsInScope()
        {
            QueryOptions options = new QueryOptions();
            options.batchSize = 250;
            try
            {
                EndpointAddress apiAddr = new EndpointAddress(_sfdcService.ServerURL);
                SalesForceService.SessionHeader header = new SessionHeader();
                header.sessionId = _sfdcService.SessionID;

                SalesForceService.SoapClient queryClient = new SalesForceService.SoapClient("Soap", apiAddr);
                string query = String.Empty;

                query = String.Format(Strings.DonorCountQ, _fromdate, _todate);
                queryClient.query(header,
                                     options,
                                     null,
                                     null,
                                     query,
                                     out _myresult);

                SalesForceService.AggregateResult c = (SalesForceService.AggregateResult)_myresult.records[0];

                return (Convert.ToInt32(c.Any[0].InnerXml));
            }
            catch(Exception e)
            {
                _inerror = true;
                _errormessage = e.ToString();
                return 0;
            }
        }

        private int PaymentsInScope()
        {
            QueryOptions options = new QueryOptions();
            options.batchSize = 250;
            try
            {
                EndpointAddress apiAddr = new EndpointAddress(_sfdcService.ServerURL);
                SalesForceService.SessionHeader header = new SessionHeader();
                header.sessionId = _sfdcService.SessionID;

                SalesForceService.SoapClient queryClient = new SalesForceService.SoapClient("Soap", apiAddr);
                string query = String.Empty;

                query = String.Format(Strings.PaymentCountQ, _fromdate, _todate);
                queryClient.query(header,
                                     options,
                                     null,
                                     null,
                                     query,
                                     out _myresult);

                SalesForceService.AggregateResult c = (SalesForceService.AggregateResult)_myresult.records[0];

                return (Convert.ToInt32(c.Any[0].InnerXml));
            }
            catch (Exception e)
            {
                _inerror = true;
                _errormessage = e.ToString();
                return 0;
            }
        }

        /// <summary>
        /// Download donors with a donation within the date range
        /// If exists, then update, else insert, no worries about clauses other than date range...
        /// </summary>
        private void QueryRecentDonors()
        {
            QueryOptions options = new QueryOptions();
            options.batchSize = 250;

            try
            {
                EndpointAddress apiAddr = new EndpointAddress(_sfdcService.ServerURL);
                SalesForceService.SessionHeader header = new SessionHeader();
                header.sessionId = _sfdcService.SessionID;

                SalesForceService.SoapClient queryClient = new SalesForceService.SoapClient("Soap", apiAddr);
                string query = String.Empty;

                if (_indivselection)
                {
                    query = Strings.RecentDonorSelect + " where causeview__Date_of_Last_Gift__c >= " + String.Format("{0:yyyy-MM-dd}", _fromdate) + " and causeview__Date_of_Last_Gift__c <= " + String.Format("{0:yyyy-MM-dd}", _todate); 
                }
                else
                {
//                    query = Strings.OrgQSelectClause + " " + OrgBuildWhereClause() + " " + Strings.OrgQGroupClause + SelectedOrderByItem.Clause + " " + LimitClause();
                }

                queryClient.query(header,
                                     options,
                                     null,
                                     null,
                                     query,
                                     out _myresult);

                bool done = false;

                while (!done)
                {
                    AppendDonorResultToSQL(_myresult);

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

        private void AppendDonorResultToSQL(QueryResult result)
        {
            SalesForceService.sObject[] records = result.records;
            SalesForceService.Contact myrecord;

            for (int i = 0; i < result.records.Count(); i++)
            {
                _processeddonors++;
                if (_worker != null)
                    _worker.ReportProgress(_processeddonors);

                myrecord = (SalesForceService.Contact) records[i];
                DownsertDonorRecord(myrecord.Id, myrecord.causeview__Primary_Addressee__c, myrecord.Phone, myrecord.Email, myrecord.MailingStreet, myrecord.MailingCity, myrecord.MailingState, myrecord.MailingPostalCode);
            }
        }

        private void DownsertDonorRecord(String ID, String Name, String Phone, String Email, String StreetAddr, String City, String State, String Zip)
        {
            String query;

            if (DonorIDExists(ID))
                query = LclQueries.UpdateDonorRecord;
            else
                query = LclQueries.InsertDonorRecord;

            MySqlCommand cmd = new MySqlCommand(query, _connection.MyConnection);

            cmd.Parameters.AddWithValue("@IDPARAM", ID);
            cmd.Parameters.AddWithValue("@NAMEPARAM", Name);
            cmd.Parameters.AddWithValue("@PHONEPARAM", Phone);
            cmd.Parameters.AddWithValue("@EMAILPARAM", Email);
            cmd.Parameters.AddWithValue("@ADDRPARAM", StreetAddr);
            cmd.Parameters.AddWithValue("@CITYPARAM", City);
            cmd.Parameters.AddWithValue("@STATEPARAM", State);
            cmd.Parameters.AddWithValue("@ZIPPARAM", Zip);

            try
            {
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                _inerror = true;
                _errormessage = e.ToString();
            }
            finally
            {
                cmd.Connection.Close();
            }
        }

        private bool DonorIDExists(String Id)
        {
            bool rval = false;
            int count;
            String query = LclQueries.DonorExistsQ + String.Format(" '{0}'", Id);

            MySqlCommand cmd = new MySqlCommand(query, _connection.MyConnection);

            try
            {
                cmd.Connection.Open();
                count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count == 1)
                    rval = true;
                else
                    rval = false;
            }
            catch (Exception e)
            {
                _inerror = true;
                _errormessage = e.ToString();
            }
            finally
            {
                cmd.Connection.Close();
            }
             return rval;
        }

        private void QueryRecentPayments()
        {
            QueryOptions options = new QueryOptions();
            options.batchSize = 250;
 
            try
            {
                EndpointAddress apiAddr = new EndpointAddress(_sfdcService.ServerURL);
                SalesForceService.SessionHeader header = new SessionHeader();
                header.sessionId = _sfdcService.SessionID;

                SalesForceService.SoapClient queryClient = new SalesForceService.SoapClient("Soap", apiAddr);
                string query = String.Empty;

                if (_indivselection)
                {
                    query = String.Format(Strings.RecentPaymentSelect,_fromdate, _todate);
                }
                else
                {
                    //                    query = Strings.OrgQSelectClause + " " + OrgBuildWhereClause() + " " + Strings.OrgQGroupClause + SelectedOrderByItem.Clause + " " + LimitClause();
                }

                queryClient.query(header,
                                     options,
                                     null,
                                     null,
                                     query,
                                     out _myresult);

                bool done = false;

                while (!done)
                {
                    AppendPaymentResultToSQL(_myresult);

                    if (_myresult.done)
                    {
                        done = true;
                    }
                    else
                    {
                        queryClient.queryMore(header, options, _myresult.queryLocator, out _myresult);
                    }
                }
            }
            catch (Exception e)
            {
                _inerror = true;
                _errormessage = e.ToString();
            }
        }

        private void AppendPaymentResultToSQL(QueryResult result)
        {
            SalesForceService.sObject[] records = result.records;
            SalesForceService.causeview__Gift_Detail__c myrecord = new causeview__Gift_Detail__c();

            for (int i = 0; i < result.records.Count(); i++)
            {
                _processedpayments++;
                if (_worker != null)
                    _worker.ReportProgress(_processedpayments);

                myrecord = (SalesForceService.causeview__Gift_Detail__c) records[i];
                if (myrecord != null)
                    DownsertPaymentRecord(myrecord);
            }
        }

        /// <summary>
        /// Check to see if required fields are null
        /// </summary>
        /// <param name="myrecord"></param>
        /// <returns></returns>
        private bool BadDownsert(SalesForceService.causeview__Gift_Detail__c myrecord)
        {
            bool rval = false;

            if (myrecord == null)
                rval = true;

            if (myrecord.causeview__Payment__c == null)
                rval = true;

            // need to reqrite for organizations
            if (myrecord.causeview__Payment__r.causeview__Donation__r == null)
                rval = true;

            return rval;
        }

        private void DownsertPaymentRecord(SalesForceService.causeview__Gift_Detail__c myrecord)
        {
            String query;

            String ID;
            String FundName;
            String DonorID;
            String GiftType;
            String Solicitor;
            String Campaign;

            if (!(BadDownsert(myrecord)))
            {

                ID = myrecord.causeview__Payment__c;
                Decimal Amount = Convert.ToDecimal(myrecord.causeview__Payment__r.causeview__Amount__c);
                DateTime RecDate = Convert.ToDateTime(myrecord.causeview__Payment__r.causeview__Date__c);

                if (myrecord.causeview__Fund__c == null)
                    FundName = String.Empty;
                else
                    FundName = (String)myrecord.causeview__Fund__c;

                DonorID = myrecord.causeview__Payment__r.causeview__Donation__r.causeview__Constituent__c;

                if (myrecord.causeview__Payment__r.causeview__Donation__r.causeview__Gift_Type__c == null)
                    GiftType = String.Empty;
                else
                    GiftType = myrecord.causeview__Payment__r.causeview__Donation__r.causeview__Gift_Type__c;

                if (myrecord.causeview__Payment__r.causeview__Donation__r.causeview__Constituent__r.Internal_Solicitor__c != null)
                    Solicitor = myrecord.causeview__Payment__r.causeview__Donation__r.causeview__Constituent__r.Internal_Solicitor__c;
                else
                    Solicitor = String.Empty;

                if (myrecord.causeview__Campaign__r != null)
                    Campaign = myrecord.causeview__Campaign__r.Id;
                else
                    Campaign = String.Empty;

                if (PaymentIDExists(ID))
                    query = LclQueries.UpdatePaymentRecord;
                else
                    query = LclQueries.InsertPaymentRecord;

                MySqlCommand cmd = new MySqlCommand(query, _connection.MyConnection);
                cmd.Parameters.AddWithValue("@IDPARAM", ID);
                cmd.Parameters.AddWithValue("@AMOUNTPARAM", Amount);
                cmd.Parameters.AddWithValue("@RECDATEPARAM", RecDate);
                cmd.Parameters.AddWithValue("@FUNDIDPARAM", FundName);
                cmd.Parameters.AddWithValue("@DONORIDPARAM", DonorID);
                cmd.Parameters.AddWithValue("@GIFTTYPEPARAM", GiftType);
                cmd.Parameters.AddWithValue("@SOLICITORPARAM", Solicitor);
                cmd.Parameters.AddWithValue("@CAMPAIGNPARAM", Campaign);
                try
                {
                    cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    _inerror = true;
                    _errormessage = e.ToString();
                }
                finally
                {
                    cmd.Connection.Close();
                }
            }
        }

        private bool PaymentIDExists(String Id)
        {
            bool rval = false;
            int count;
            String query = LclQueries.PaymentExistsQ + String.Format(" '{0}'", Id);

            MySqlCommand cmd = new MySqlCommand(query, _connection.MyConnection);

            try
            {
                cmd.Connection.Open();
                count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count == 1)
                    rval = true;
                else
                    rval = false;
            }
            catch (Exception e)
            {
                _inerror = true;
                _errormessage = e.ToString();
            }
            finally
            {
                cmd.Connection.Close();
            }
            return rval;
        }
        
        #endregion

    }
}
