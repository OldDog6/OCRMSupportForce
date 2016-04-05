using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ServiceModel;
using System.Xml;
using System.Net;
using System.IO;
using System.Configuration;
using OCRMSupportForce.SalesForceService;
using OCRMSupportForce.SForceWebReference;

using System.Windows.Data;
using System.Data;
using esscWPFShell;

namespace OCRMSupportForce.Models
{
    #region DBMS Primitives

    public class Solicitor
    {
        public Solicitor(String _id, String _clause, String _orgclause, String _description)
        {
            ID = _id;
            Clause = _clause;
            OrgClause = _orgclause;
            Description = _description;
        }

        public String ID { get;set;}
        public String OrgClause { get; set; }
        public String Clause { get; set; }
        public String Description { get; set; }
    }

    public class Funds
    {
        public Funds(String _id, String _clause, String _orgclause, String _name)
        {
            ID = _id;
            FundName = _name;
            Clause = _clause;
            OrgClause = _orgclause;
        }

        public String ID { get; set; }
        public String FundName { get; set; }
        public String Clause { get; set; }
        public String OrgClause { get; set; }

    }
    
    public class StripeBatches
    {
        public StripeBatches(String sf_id, String id, String name)
        {
            SalesforceID = sf_id;
            ID = id;
            Name = name;
        }

        public String SalesforceID { get; set; }
        public String ID { get; set; }
        public String Name { get; set; }
        public String Display 
        {
            get
            {
                if (SalesforceID != "NA")
                    return ID + ": " + Name;
                else
                    return Name;
            }
        }
    }
    
    #endregion

    public class ForceWSDLSupport
    {
        #region Constructor
        public ForceWSDLSupport(bool attemptlogin)
        {
            if (attemptlogin)
            {
                LogIn();
            }
        }
        #endregion

        #region Private Variables

        private EndpointAddress sfdcEndpoint;
        private SalesForceService.LoginResult sfdcResult;
        private SalesForceService.SessionHeader sfdcHeader;
        private SoapClient sfdcClient;
        private string loginexception = string.Empty;
        private bool _success = false;

        #endregion

        #region Public properties

        public EndpointAddress EndPoint
        {
            get { return sfdcEndpoint; }
            set { sfdcEndpoint = value; }
        }

        public SoapClient Client
        {
            get { return sfdcClient; }
            
        }

        public SalesForceService.LoginResult LoginResult
        {
            get { return sfdcResult; }
            set { sfdcResult = value; }
        }

        public string SessionID
        {
            get { return sfdcResult.sessionId; }
        }

        public string ServerURL
        {
            get { return sfdcResult.serverUrl; }
        }

        public SalesForceService.SessionHeader SessionHeader
        {
            get { return sfdcHeader; }
        }

        public string GetLoginException
        {
            get { return loginexception; }
        }

        public string AsUserName
        {
            get { return Strings.ForceUser; }
        }

        public bool LoginSuccess
        {
            get { return _success; }
        }

        #endregion

        #region Public functions

        public void LogIn()
        {
            SalesForceService.SoapClient loginClient = new SalesForceService.SoapClient();
            // retrieve credentials
            string sfdcPassword = Strings.ForcePassword;
            string sfdcToken = Strings.ForceToken;
            
            string pw = sfdcPassword + sfdcToken;
            try
            {
                sfdcResult = loginClient.login(null, Strings.ForceUser, pw);
                sfdcEndpoint = new EndpointAddress(ServerURL);
                sfdcHeader = new SalesForceService.SessionHeader();
                sfdcHeader.sessionId = SessionID;
                _success = true;
            }
            catch (Exception e)
            {
                loginexception = e.ToString();
                _success = false;
            }
        }    

        /// <summary>
        /// Just Sample, Do not use...
        /// </summary>
        public void CreateLead()
        {
/*            Lead sfdcLead = new Lead();
            string firstName = "Jane";
            string lastName = "Doe";
            string email = "jandoe@salesforce.com";
            string companyName = "Salesforce.com";
            sfdcLead.FirstName = firstName;
            sfdcLead.LastName = lastName;
            sfdcLead.Email = email;
            sfdcLead.Company = companyName;

            SaveResult[] results;
            SalesForceService.LimitInfo[] li;
  
            Client.create(
                sfdcHeader,
                null,null,null,null,null,null,null,null,null,null,null,
                new sObject[] { sfdcLead }, 
                out li,
                out results);

            if (!(results[0].success))
                _success = false;
            else
                _success = true;

 */
        }
        #endregion
    }

    /// <summary>
    /// ForceWebSupport uses the more modern SOAP calls, easier and quicker
    /// </summary>
    public class ForceWebSupport
    {
        #region constructor

        public ForceWebSupport()
        {
            // execute a login...
            webbinding = new SForceWebReference.SforceService();

            try
            {
                loginresult = webbinding.login(Strings.ForceUser, Strings.ForcePassword + Strings.ForceToken);
                webbinding.Url = loginresult.serverUrl;
                webbinding.SessionHeaderValue = new SForceWebReference.SessionHeader();
                webbinding.SessionHeaderValue.sessionId = loginresult.sessionId;

                loadgeneralfund();
                loadstripeappeal();
            }
            catch (Exception e)
            {
                InError = true;
                ErrorMessage = e.ToString();
            }
        }

        #endregion

        #region public properties

        public SForceWebReference.SforceService webbinding { get; set; }
        public SForceWebReference.LoginResult loginresult { get; set; }

        public bool InError { get; set; }
        public String ErrorMessage { get; set; }

        public SForceWebReference.causeview__Fund__c AsGeneralFund { get; set; }

        public SForceWebReference.Campaign AsStripeAppeal { get; set; }

        #endregion

        #region Private Methods

        private void loadstripeappeal()
        {
            SForceWebReference.QueryResult queryResult = null;

            try
            {
                string query = "SELECT Id FROM Campaign WHERE name ='STRIPE MAIL'";
                queryResult = webbinding.query(query);
                InError = false;
            }
            catch (Exception e)
            {
                InError = true;
                ErrorMessage = e.ToString();
            }

            SForceWebReference.sObject[] records = queryResult.records;
            if (records != null)
                if (records.Count() == 1)
                    AsStripeAppeal = (SForceWebReference.Campaign )records[0];
                else
                    AsStripeAppeal = null;
            else
                AsStripeAppeal = null;
        }

        private void loadgeneralfund()
        {
            SForceWebReference.QueryResult queryResult = null;

            try
            {
                string query = "SELECT Id FROM Causeview__Fund__c WHERE name ='F-00002'";
                queryResult = webbinding.query(query);
                InError = false;
            }
            catch (Exception e)
            {
                InError = true;
                ErrorMessage = e.ToString();
            }

            SForceWebReference.sObject[] records = queryResult.records;
            if (records != null)
                if (records.Count() == 1)
                    AsGeneralFund = (SForceWebReference.causeview__Fund__c)records[0];
                else
                    AsGeneralFund = null;
            else
                AsGeneralFund = null;
        }


        #endregion

        #region public methods

        public SForceWebReference.causeview__Gift__c GetGiftByExternalID(String extid)
        {
            SForceWebReference.QueryResult queryResult = null;

            try
            {
                string query = String.Format("SELECT Id FROM Causeview__Gift__c WHERE causeview__External_Trans_ID__c ='{0}'", extid);
                queryResult = webbinding.query(query);
                InError = false;
            }
            catch (Exception e)
            {
                InError = true;
                ErrorMessage = e.ToString();
            }

            SForceWebReference.sObject[] records = queryResult.records;
            if (records != null)
                if (records.Count() == 1)
                    return (SForceWebReference.causeview__Gift__c)records[0];
                else
                    return null;
            else
                return null;
        }

        public SForceWebReference.causeview__Gift_Detail__c GetGiftDetail(String _parentid)
        {
            SForceWebReference.QueryResult queryResult = null;
            try
            {
                string query = String.Format("SELECT Id FROM causeview__Gift_Detail__c WHERE causeview__Gift__c = '{0}'", _parentid);
                queryResult = webbinding.query(query);
                InError = false;
            }
            catch (Exception e)
            {
                InError = true;
                ErrorMessage = e.ToString();
            }

            SForceWebReference.sObject[] records = queryResult.records;
            if (records != null)
                if (records.Count() == 1)
                    return (SForceWebReference.causeview__Gift_Detail__c) records[0];
                else
                    return null;
            else
                return null;
        }

        public void PostGift(SForceWebReference.causeview__Gift__c mygift, SForceWebReference.causeview__Gift_Detail__c mygiftdetail, String Fname)
        {
            SForceWebReference.causeview__Gift_Detail__c _lookup;

            try
            {
                SForceWebReference.UpsertResult[] results = webbinding.upsert("causeview__External_Trans_ID__c", new SForceWebReference.sObject[] { mygift });

                foreach (SForceWebReference.UpsertResult result in results)
                {
                    if (!result.success)
                    {
                        InError = true;
                        ErrorMessage = result.errors[0].message;
                    }
                    else
                    {

                        mygiftdetail.causeview__Gift__c = results[0].id;
                        mygiftdetail.causeview__Amount__c = mygift.causeview__Amount__c;
                        mygiftdetail.causeview__Amount__cSpecified = true;

                        mygiftdetail.causeview__Description__c = String.Format("From File: {0}",Fname);
                        mygiftdetail.causeview__New_Campaign__c = AsStripeAppeal.Id; 

                        mygiftdetail.causeview__Fund__c = AsGeneralFund.Id;

                        _lookup = GetGiftDetail(results[0].id);

                        if (_lookup == null)
                        {
                            PostGiftDetail(mygiftdetail);
                        }
                        else
                        {
                            mygiftdetail.Id = _lookup.Id;
                            UpdateGiftDetail(mygiftdetail);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                InError = true;
                ErrorMessage = e.ToString();
            }
        }

        public void PostGiftDetail(SForceWebReference.causeview__Gift_Detail__c mygiftdetail)
        {
            try
            {
                SForceWebReference.SaveResult[] results = webbinding.create(new SForceWebReference.sObject[] { mygiftdetail });

                foreach (SForceWebReference.SaveResult result in results)
                {
                    if (!result.success)
                    {
                        InError = true;
                        ErrorMessage = result.errors[0].message;
                    }
                }
            }
            catch (Exception e)
            {
                InError = true;
                ErrorMessage = e.ToString();
            }
        }

        public void UpdateGiftDetail(SForceWebReference.causeview__Gift_Detail__c mygiftdetail)
        {
            try
            {
                SForceWebReference.SaveResult[] results = webbinding.update(new SForceWebReference.sObject[] { mygiftdetail });

                foreach (SForceWebReference.SaveResult result in results)
                {
                    if (!result.success)
                    {
                        InError = true;
                        ErrorMessage = result.errors[0].message;
                    }
                }
            }
            catch (Exception e)
            {
                InError = true;
                ErrorMessage = e.ToString();
            }
        }

        public SForceWebReference.causeview__Appeal__c GetAppeal()
        {
            SForceWebReference.causeview__Appeal__c rval = null;

            return rval;
        }

        #endregion
    }

    #region Supporting Downloads

    public class SupportingDownloads
    {
        public SupportingDownloads(ForceWSDLSupport log)
        {
            _log = log;
        }

        #region private properties

        private ForceWSDLSupport _log;

        #endregion

        #region public Methods


        /// <summary>
        ///  use the Force.com to populate the Internal Solicitors list
        /// </summary>
        /// <returns></returns>
        public void LoadSolicitorsList(List <Solicitor> mylist)
        {
            SalesForceService.QueryOptions options = new SalesForceService.QueryOptions();
            options.batchSize = 250;
            SalesForceService.QueryResult _myresult;
            bool done = false;

            if (_log.LoginSuccess)
            {
                // Aggreate Queries cannot use QueryMore approach
                try
                {
                    EndpointAddress apiAddr = new EndpointAddress(_log.ServerURL);
                    SalesForceService.SessionHeader header = new SalesForceService.SessionHeader();
                    header.sessionId = _log.SessionID;

                    SalesForceService.SoapClient queryClient = new SalesForceService.SoapClient("Soap", apiAddr);
                    string query = "select Id, name from user order by Name";

                    queryClient.query(header,
                                         options,
                                         null,
                                         null,
                                         query,
                                         out _myresult);

                    while (!done)
                    {
                        SalesForceService.sObject[] records = _myresult.records;

                        for (int i = 0; i < _myresult.records.Count(); i++)
                        {
                            SalesForceService.User c = (SalesForceService.User) _myresult.records[i];

                            String newclause = String.Format(" and causeview__payment__r.causeview__Donation__r.causeview__Constituent__r.Internal_Solicitor__c = '{0}'", c.Name);
                            String orgClause = String.Format(" and causeview__payment__r.causeview__Donation__r.causeview__Organization__r.Internal_Solicitor__c = '{0}'", c.Name);

                            mylist.Add(new Solicitor(c.AccountId, newclause, orgClause,c.Name));
                        }

                        // AppendResultToReportDataTable(_myresult);
                        queryClient.queryMore(header, options, _myresult.queryLocator, out _myresult);
                        if (_myresult.done)
                            done = true;
                    }
                }
                catch (Exception e)
                {

                }
            }
        }
   
        public void LoadFundList(List <Funds> mylist)
        {
            SalesForceService.QueryOptions options = new SalesForceService.QueryOptions();
            options.batchSize = 250;
            SalesForceService.QueryResult _myresult;
            bool done = false;

            if (_log.LoginSuccess)
            {
                // Aggreate Queries cannot use QueryMore approach
                try
                {
                    EndpointAddress apiAddr = new EndpointAddress(_log.ServerURL);
                    SalesForceService.SessionHeader header = new SalesForceService.SessionHeader();
                    header.sessionId = _log.SessionID;

                    SalesForceService.SoapClient queryClient = new SalesForceService.SoapClient("Soap", apiAddr);
                    string query = "SELECT ID, causeview__Fund_Name__c FROM causeview__Fund__c";

                    queryClient.query(header,
                                         options,
                                         null,
                                         null,
                                         query,
                                         out _myresult);

                    while (!done)
                    {
                        SalesForceService.sObject[] records = _myresult.records;

                        for (int i = 0; i < _myresult.records.Count(); i++)
                        {
                            SalesForceService.causeview__Fund__c c = (SalesForceService.causeview__Fund__c)_myresult.records[i];

                            String newclause = String.Format(" and causeview__Fund__r.Id = '{0}'", c.Id);
                            String orgClause = String.Format(" and causeview__Fund__r.Id = '{0}'", c.Id);

                            mylist.Add(new Funds(c.Id,newclause,orgClause,c.causeview__Fund_Name__c));
                        }

                        queryClient.queryMore(header, options, _myresult.queryLocator, out _myresult);
                        if (_myresult.done)
                            done = true;
                    }
                }
                catch (Exception e)
                {

                }
            }

        }


        #endregion
    }

    public class ForceSearches
    {
        public ForceSearches(ForceWebSupport connection)
        {
            _connection = connection;
        }

        #region private properties

        ForceWebSupport _connection;
        bool _inerror = false;
        String _errormessage = String.Empty;

        #endregion

        #region public methods

        public SForceWebReference.Contact lookupbyemail(String emailaddr)
        {
            SForceWebReference.Contact donor = null;

            SForceWebReference.QueryResult queryResult = null;

            try
            {
                string query = String.Format("SELECT Id,FirstName,LastName, Email, MailingStreet, MailingCity, MailingState, MailingPostalCode  FROM Contact WHERE Email = '{0}'", emailaddr);
                queryResult = _connection.webbinding.query(query);
                _inerror = false;
            }
            catch (Exception e)
            {
                _inerror = true;
                _errormessage = e.ToString();
            }

            SForceWebReference.sObject[] records = queryResult.records;
            if (records != null)
                if (records.Count() == 1)
                    donor = (SForceWebReference.Contact)records[0];

            return donor;
        }

        public SForceWebReference.Contact lookupbyemailorblank(String emailaddr)
        {
            SForceWebReference.Contact donor = lookupbyemail(emailaddr);

            if (donor == null)
            {
                donor = new SForceWebReference.Contact();
                donor.FirstName = "Not";
                donor.LastName = "Found";
            }

            return donor;
        }

        public List<SForceWebReference.Contact> GetSearchList(String name, String StreetAddr)
        {
            // return a list of all donors with last name or same street address case insensitive
            List<SForceWebReference.Contact> rval = new List<SForceWebReference.Contact>();
            string query = String.Empty;

//            SForceWebReference.Contact x = new SForceWebReference.Contact();
//            x.MailingPostalCode

            SForceWebReference.QueryResult queryResult = null;
            try
            {
                query = String.Format("SELECT Id, FirstName, LastName, Email, MailingStreet, MailingCity, MailingState, MailingPostalCode FROM Contact WHERE LastName like '%{0}%' or MailingStreet like '%{1}%' order by FirstName, LastName, MailingCity", name, StreetAddr);  
                queryResult = _connection.webbinding.query(query);
                _inerror = false;
            }
            catch (Exception e)
            {
                _inerror = true;
                _errormessage = e.ToString();
            }

            SForceWebReference.sObject[] records = queryResult.records;
            if (records != null)
            {
                foreach(SForceWebReference.Contact item in records)
                {
                    rval.Add(item);
                }
            }
            return rval;
        }

        #endregion
    }

    public class BatchPosting
    {
        public BatchPosting(SForceWebReference.SforceService connection)
        {
            _connection = connection;
            _inerror = false;
            _errormessage = String.Empty;

            loadstripeappeal();
            loadgeneralfund();
        }

        #region private properties

        SForceWebReference.SforceService _connection;
        bool _inerror = false;
        String _errormessage = String.Empty;

        #endregion

        #region Private Methods

        private SForceWebReference.causeview__Gift_Batch__c GetBatchFromID(String ID)
        {
            SForceWebReference.causeview__Gift_Batch__c rval = null;
            SForceWebReference.QueryResult queryResult = null;

            try
            {
                string query = String.Format("SELECT Id FROM causeview__Gift_Batch__c WHERE Id = '{0}'", ID);
                queryResult = _connection.query(query);
                _inerror = false;
            }
            catch (Exception e)
            {
                _inerror = true;
                _errormessage = e.ToString();
            }

            SForceWebReference.sObject[] records = queryResult.records;
            if (records.Count() == 1)
                rval = (SForceWebReference.causeview__Gift_Batch__c)records[0];

            return rval;
        }

        private SForceWebReference.causeview__Gift__c GetGiftByExternalID(String extid)
        {
            SForceWebReference.QueryResult queryResult = null;

            try
            {
                string query = String.Format("SELECT Id FROM Causeview__Gift__c WHERE causeview__External_Trans_ID__c ='{0}'", extid);
                queryResult = _connection.query(query);
                _inerror = false;
            }
            catch (Exception e)
            {
                _inerror = true;
                _errormessage = e.ToString();
            }

            SForceWebReference.sObject[] records = queryResult.records;
            if (records != null)
                if (records.Count() == 1)
                    return (SForceWebReference.causeview__Gift__c)records[0];
                else
                    return null;
            else
                return null;
        }

        private SForceWebReference.causeview__Gift_Detail__c GetGiftDetail(String _parentid)
        {
            SForceWebReference.QueryResult queryResult = null;
            try
            {
                string query = String.Format("SELECT Id FROM causeview__Gift_Detail__c WHERE causeview__Gift__c = '{0}'", _parentid);
                queryResult = _connection.query(query);
                InError = false;
            }
            catch (Exception e)
            {
                InError = true;
                ErrorMessage = e.ToString();
            }

            SForceWebReference.sObject[] records = queryResult.records;
            if (records != null)
                if (records.Count() == 1)
                    return (SForceWebReference.causeview__Gift_Detail__c) records[0];
                else
                    return null;
            else
                return null;
        }

        private void PostGift(SForceWebReference.causeview__Gift__c mygift)
        {
            try
            {
                SForceWebReference.UpsertResult[] results = _connection.upsert("causeview__External_Trans_ID__c", new SForceWebReference.sObject[] { mygift });

                foreach (SForceWebReference.UpsertResult result in results)
                {
                    if (!result.success)
                    {
                        _inerror = true;
                        _errormessage = result.errors[0].message;
                    }
                    else
                    {
                        /*
                        mygiftdetail.causeview__Gift__c = results[0].id;
                        mygiftdetail.causeview__Amount__c = mygift.causeview__Amount__c;
                        mygiftdetail.causeview__Amount__cSpecified = true;

                        mygiftdetail.causeview__Description__c = String.Format("From File: {0}", Fname);
                        mygiftdetail.causeview__New_Campaign__c = AsStripeAppeal.Id;

                        mygiftdetail.causeview__Fund__c = AsGeneralFund.Id;

                        _lookup = GetGiftDetail(results[0].id);

                        if (_lookup == null)
                        {
                            PostGiftDetail(mygiftdetail);
                        }
                        else
                        {
                            mygiftdetail.Id = _lookup.Id;
                            UpdateGiftDetail(mygiftdetail);
                        }
                         */
                    }
                }
            }
            catch (Exception e)
            {
                _inerror = true;
                _errormessage = e.ToString();
            }

        }

        private void PostGiftDetail(SForceWebReference.causeview__Gift_Detail__c mygiftdetail)
        {
            try
            {
                SForceWebReference.SaveResult[] results = _connection.create(new SForceWebReference.sObject[] { mygiftdetail });

                foreach (SForceWebReference.SaveResult result in results)
                {
                    if (!result.success)
                    {
                        InError = true;
                        ErrorMessage = result.errors[0].message;
                    }
                }
            }
            catch (Exception e)
            {
                InError = true;
                ErrorMessage = e.ToString();
            }
        }

        private void UpdateGiftDetail(SForceWebReference.causeview__Gift_Detail__c mygiftdetail)
        {
            try
            {
                SForceWebReference.SaveResult[] results = _connection.update(new SForceWebReference.sObject[] { mygiftdetail });

                foreach (SForceWebReference.SaveResult result in results)
                {
                    if (!result.success)
                    {
                        InError = true;
                        ErrorMessage = result.errors[0].message;
                    }
                }
            }
            catch (Exception e)
            {
                InError = true;
                ErrorMessage = e.ToString();
            }
        }

        private void loadstripeappeal()
        {
            SForceWebReference.QueryResult queryResult = null;

            try
            {
                string query = "SELECT Id FROM Campaign WHERE name ='STRIPE MAIL'";
                queryResult = _connection.query(query);
                InError = false;
            }
            catch (Exception e)
            {
                InError = true;
                ErrorMessage = e.ToString();
            }

            SForceWebReference.sObject[] records = queryResult.records;
            if (records != null)
                if (records.Count() == 1)
                    AsStripeAppeal = (SForceWebReference.Campaign)records[0];
                else
                    AsStripeAppeal = null;
            else
                AsStripeAppeal = null;
        }

        private void loadgeneralfund()
        {
            SForceWebReference.QueryResult queryResult = null;

            try
            {
                string query = "SELECT Id FROM Causeview__Fund__c WHERE name ='F-00002'";
                queryResult = _connection.query(query);
                InError = false;
            }
            catch (Exception e)
            {
                InError = true;
                ErrorMessage = e.ToString();
            }

            SForceWebReference.sObject[] records = queryResult.records;
            if (records != null)
                if (records.Count() == 1)
                    AsGeneralFund = (SForceWebReference.causeview__Fund__c)records[0];
                else
                    AsGeneralFund = null;
            else
                AsGeneralFund = null;
        }

        private void editcontact(SForceWebReference.Contact record)
        {
            try
            {
                SForceWebReference.SaveResult[] results = _connection.update(new SForceWebReference.sObject[] { record });

                foreach (SForceWebReference.SaveResult result in results)
                {
                    if (!result.success)
                    {
                        InError = true;
                        ErrorMessage = result.errors[0].message;
                    }
                }
            }
            catch (Exception e)
            {
                InError = true;
                ErrorMessage = e.ToString();
            }
        }

        private void insertcontact(ComparsionClass data)
        {
            SForceWebReference.Contact record = new SForceWebReference.Contact();

            record.FirstName = data.FirstName;
            record.LastName = data.LastName;
            record.Email = data.StripeDonorRecord.CardEmail;
            record.MailingStreet = data.StripeDonorRecord.CardAddr;
            record.MailingState = data.StripeDonorRecord.CardState;
            record.MailingCity = data.StripeDonorRecord.CardCity;
            record.MailingPostalCode = data.StripeDonorRecord.CardZip;

            try
            {
                SForceWebReference.SaveResult[] results = _connection.create(new SForceWebReference.sObject[] { record });

                foreach (SForceWebReference.SaveResult result in results)
                {
                    if (!result.success)
                    {
                        InError = true;
                        ErrorMessage = result.errors[0].message;
                    }
                }
            }
            catch (Exception e)
            {
                InError = true;
                ErrorMessage = e.ToString();
            }
        }

        #endregion

        #region Public Properties

        public bool InError
        { 
            get {return _inerror; }
            set {_inerror = value; }
        }

        public String ErrorMessage
        {
            get { return _errormessage; }
            set { _errormessage = value; }
        }

        public SForceWebReference.causeview__Fund__c AsGeneralFund { get; set; }

        public SForceWebReference.Campaign AsStripeAppeal { get; set; }

        #endregion

        #region Public Methods

        public void PostToBatch(String BatchID, List<ComparsionClass> Data, String Fname)
        {
            // Get the batch to post to...
            SForceWebReference.causeview__Gift_Batch__c _batch = GetBatchFromID(BatchID);

            bool detailexists = false;

            // Walk thru the ComparsionClass and post each item to the batch
            foreach(ComparsionClass item in Data)
            {
                // New or Edit?
                SForceWebReference.causeview__Gift__c record = GetGiftByExternalID(item.StripeDonorRecord.Id);

                if (record == null)
                {
                    record = new SForceWebReference.causeview__Gift__c();
                }

                record.causeview__External_Trans_ID__c = item.StripeDonorRecord.Id;
                record.causeview__Constituent__c = item.SFDonorRecord.Id;

                record.causeview__Channel__c = "Web";
                record.causeview__Gift_Type__c = "One Time Gift";
                record.causeview__Receipt_Type__c = "Single Receipt";
                record.causeview__Expected_Amount__c = Convert.ToDouble(item.StripeDonorRecord.Amount);
                record.causeview__Amount__c = Convert.ToDouble(item.StripeDonorRecord.Amount);
                record.causeview__Total_Gift_Amount__c = Convert.ToDouble(item.StripeDonorRecord.Amount);
                record.causeview__Gift_Date__c = Convert.ToDateTime(item.StripeDonorRecord.Created);
                record.causeview__Status__c = "Entered";
                record.causeview__Expected_Amount__cSpecified = true;

                record.causeview__GiftBatch__c = _batch.Id;
                record.causeview__Batch_Status__c = "Pending";

                PostGift(record);

                if (_inerror)
                    break;

                // now find the gift_detail record

                SForceWebReference.causeview__Gift_Detail__c recorddetail = GetGiftDetail(record.Id);

                if (recorddetail == null)
                {
                    detailexists = false;
                    recorddetail = new SForceWebReference.causeview__Gift_Detail__c();
                }
                else
                    detailexists = true;

                recorddetail.causeview__Gift__c = record.Id;
                recorddetail.causeview__Amount__c = Convert.ToDouble(item.StripeDonorRecord.Amount);
                recorddetail.causeview__Amount__cSpecified = true;

                recorddetail.causeview__Description__c = String.Format("From File: {0}", Fname);
                recorddetail.causeview__New_Campaign__c = AsStripeAppeal.Id;
                recorddetail.causeview__Fund__c = AsGeneralFund.Id;

                // else update... ugh...
                if (detailexists)
                {

                }
                else
                {
                    PostGiftDetail(recorddetail);
                }

                if (_inerror)
                    break;
            }
        }

        public void UpdateContact(SForceWebReference.Contact data)
        {
            editcontact(data);
        }

        public void InsertContact(ComparsionClass data)
        {
            insertcontact(data);
        }


        #endregion

    }

    #endregion

}

