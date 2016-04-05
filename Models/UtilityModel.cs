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
using OCRMSupportForce.SForceWebReference;

using System.ComponentModel;
using System.Windows.Threading;
using System.ServiceModel;
using System.Xml;
using System.Net;
using System.IO;
using System.Configuration;

using FileHelpers;


namespace OCRMSupportForce.Models
{
    /// <summary>
    /// Causeview Version, Move Contact Internal Solicitors from X to Y
    /// </summary>
    public class ModifySolicitors
    {

        /// <summary>
        /// Constructor
        /// </summary>
        public ModifySolicitors(ForceWebSupport websupport)
        {
            _websupport = websupport;
        }

        #region Private Properties

        private ForceWebSupport _websupport;

        private String _errormessage = String.Empty;
        private bool _inerror = false;
        private bool done;

        private SalesForceService.QueryResult _myresult;

        private String _fromsolicitorName { get; set; }
        private String _tosolicitorname { get; set; }

        private List<OCRMSupportForce.SForceWebReference.Contact> _solicitorlist;

        #endregion

        #region private methods

        private void AppendResultsToList(SForceWebReference.QueryResult results)
        {
            SForceWebReference.sObject[] records = results.records;

            for (int i = 0; i < results.records.Count(); i++)
            {
                _solicitorlist.Add((SForceWebReference.Contact)records[i]);
            }
        }

        private void ChangeInternalSolicitor()
        {
            ForceWebSupport webreference = new ForceWebSupport();
            SForceWebReference.PicklistEntry[] picklistvalues = null;

            // get all potential picklist values...
            SForceWebReference.DescribeSObjectResult _result = _websupport.webbinding.describeSObject("Contact");

            SForceWebReference.Field[] fields = _result.fields;
            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].name == "Internal_Solicitor__c")
                {
                    SForceWebReference.Field _solicitorpicklist = fields[i];
                    if (_solicitorpicklist.type == (SForceWebReference.fieldType.picklist))
                    {
                        picklistvalues = _solicitorpicklist.picklistValues;
                    }
                    break;
                }
            }
            
            for (int i = 0;i < _solicitorlist.Count; i++ )
            {
                if (picklistvalues != null)
                {
                    _solicitorlist[i].Internal_Solicitor__c = picklistvalues[41].value;
                    
                    SForceWebReference.SaveResult[] _results = _websupport.webbinding.update(new SForceWebReference.sObject[] { _solicitorlist[i] });
                    for (int j = 0; j < _results.Length; j++)
                    {
                        if (_results[j].success)
                        {
                            int z = 0;
                        }
                        else
                        {
                            int x = 0;
                        }
                    }
                }
            }
        }

        #endregion

        #region Public properties

        public String SelectedSolicitor
        {
            get { return _fromsolicitorName; }
            set { _fromsolicitorName = value; }
        }

        public String ReceivingSolicitor
        {
            get { return _tosolicitorname; }
            set { _tosolicitorname = value; }
        }

        public List<OCRMSupportForce.SForceWebReference.Contact> SolicitorList
        {
            get { return _solicitorlist;}
        }

        public void LoadContactList(String FromSolicitorName)
        {
            _fromsolicitorName = FromSolicitorName;
            SForceWebReference.QueryResult queryResult = null;

            if (_solicitorlist == null)
                _solicitorlist = new List<OCRMSupportForce.SForceWebReference.Contact>();

            if (_solicitorlist.Count > 0)
                _solicitorlist.Clear();


            try
            {
                string query = String.Format(LclQueries.LoadSolicitorContactsQ, _fromsolicitorName);

                queryResult = _websupport.webbinding.query(query);
                AppendResultsToList(queryResult);
                _inerror = false;
            }
            catch (Exception e)
            {
                _inerror = true;
                _errormessage = e.ToString();
            }

            int c = _solicitorlist.Count;
            ChangeInternalSolicitor();
        }

        #endregion

    }


    /// <summary>
    /// Select a Batch, then load a Gift__c list and save 
    /// </summary>
    public class UploadStripeData
    {
        #region Constructor
        public UploadStripeData(ForceWebSupport websupport)
        {
            _websupport = websupport;
            // Populate the collection view
            _openbatcheslist = new DataTable();

            DataColumn BatchID = new DataColumn();
            BatchID.ColumnName = "BatchID";
            _openbatcheslist.Columns.Add(BatchID);

            DataColumn id = new DataColumn();
            id.ColumnName = "id";
            _openbatcheslist.Columns.Add(id);

            DataColumn name = new DataColumn();
            name.ColumnName = "name";
            _openbatcheslist.Columns.Add(name);

            _filename = "None Selected";
            

           // _engine = new FileHelperEngine<Record> (Encoding.UTF8);
        }

        #endregion

        #region private properties

        private ForceWebSupport _websupport;
        private bool _inerror = false;
        private String _errormessage = String.Empty;

        private DataTable _openbatcheslist;
        private String _filename;

        #endregion

        #region private methods

        private void AppendResultsToList(SForceWebReference.QueryResult myresult)
        {
            SForceWebReference.sObject[] records = myresult.records;

            for (int i = 0; i < myresult.records.Count(); i++)
            {
                BatchList.Add((SForceWebReference.causeview__Gift_Batch__c)records[i]);
            }
        }

        private bool isvalidfile(StripeRecord result)
        {
            if ((result.Id != "ID#") || (result.CardAddr != "Card Address Line1"))
                return false;
            else
                return true;
        }

        private String findcontactId(StripeRecord item)
        {
            SForceWebReference.Contact mycontact = lookupbyemail(item.CardEmail);

            if (mycontact != null)
                return mycontact.Id;
            else
                return String.Empty;
        }

        private SForceWebReference.Contact lookupbyemail(String emailaddr)
        {
            SForceWebReference.Contact donor = null;

            SForceWebReference.QueryResult queryResult = null;

            try
            {
                string query = String.Format("SELECT Id FROM Contact WHERE Email = '{0}'", emailaddr);
                queryResult = _websupport.webbinding.query(query);
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

        private SForceWebReference.causeview__Gift_Batch__c asbatch(String ID)
        {
            SForceWebReference.causeview__Gift_Batch__c rval = null;
            SForceWebReference.QueryResult queryResult = null;

            try
            {
                string query = String.Format("SELECT Id FROM causeview__Gift_Batch__c WHERE Id = '{0}'", ID);
                queryResult = _websupport.webbinding.query(query);
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

        private SForceWebReference.causeview__Fund__c asfund()
        {
            SForceWebReference.causeview__Fund__c rval = null;
            SForceWebReference.QueryResult queryResult = null;

            try
            {
                string query = String.Format("SELECT Id FROM causeview__Gift_Batch__c WHERE Name = 'F-00002'");
                queryResult = _websupport.webbinding.query(query);
                _inerror = false;
            }
            catch (Exception e)
            {
                _inerror = true;
                _errormessage = e.ToString();
            }

            SForceWebReference.sObject[] records = queryResult.records;
            if (records.Count() == 1)
                rval = (SForceWebReference.causeview__Fund__c)records[0];

            return rval;

        }

        #endregion

        #region public properties

        SForceWebReference.causeview__Gift_Batch__c Batch { get; set; }

        public List<SForceWebReference.causeview__Gift_Batch__c> BatchList { get; set; }
        public List<SForceWebReference.causeview__Gift__c> Donations { get; set; }

        public DataTable OpenBatchesList { get { return _openbatcheslist; } }

        public String ExcelFileName
        {
            get { return _filename; }
            set { _filename = value; }
        }

        #endregion

        #region public methods

        public void LoadBatchList()
        {
            SForceWebReference.QueryResult queryResult = null;
            if (BatchList == null)
                BatchList = new List<SForceWebReference.causeview__Gift_Batch__c>();
            else
                BatchList.Clear();
            
            try
            {
                string query = String.Format(BatchSOQLQueries.SelectOpenBatches);

                queryResult = _websupport.webbinding.query(query);
                AppendResultsToList(queryResult);
                _inerror = false;
            }
            catch (Exception e)
            {
                _inerror = true;
                _errormessage = e.ToString();
            }

            _openbatcheslist.Rows.Clear();

            foreach(SForceWebReference.causeview__Gift_Batch__c item in BatchList)
            {
                DataRow _row = _openbatcheslist.NewRow();

                _row["BatchID"] = item.Id;
                _row["id"] = item.Name;
                _row["name"] = item.causeview__Name__c;

                _openbatcheslist.Rows.Add(_row);
            }
        }

        public void OpenExcelFile(String selectedbatch)
        {
            var engine = new FileHelperEngine<StripeRecord>();
            var result = engine.ReadFile(_filename);

            SForceWebReference.causeview__Gift__c mygift;
            SForceWebReference.causeview__Gift_Batch__c mybatch = asbatch(selectedbatch);
            SForceWebReference.causeview__Gift_Detail__c mygiftdetail;

          //  SForceWebReference.causeview__Fund__c myfund = asfund();

            if (isvalidfile(result[0]))
            {
                foreach (StripeRecord item in result)
                {
                    // skip the validated header
                    if (item.Amount != "Amount")
                    {
                        // Build causeview gift item and post to causeview...
                        mygift = new SForceWebReference.causeview__Gift__c();
                        mygiftdetail = new SForceWebReference.causeview__Gift_Detail__c();


                        mygift.causeview__External_Trans_ID__c = item.Id;
                        mygift.causeview__Channel__c = "Web";
                        mygift.causeview__Gift_Type__c = "One Time Gift";
                        mygift.causeview__Receipt_Type__c = "Single Receipt";
                        mygift.causeview__Expected_Amount__c = Convert.ToDouble(item.Amount);
                        mygift.causeview__Amount__c = Convert.ToDouble(item.Amount);
                        mygift.causeview__Gift_Date__c = Convert.ToDateTime(item.Created);
                        mygift.causeview__Status__c = "Entered";
                        mygift.causeview__Expected_Amount__cSpecified = true;

                        mygift.causeview__GiftBatch__c = mybatch.Id;
                        mygift.causeview__Batch_Status__c = "Pending";
                        mygift.causeview__Total_Gift_Amount__c = Convert.ToDouble(item.Amount); 

                        mygift.causeview__Constituent__c = findcontactId(item);

                        mygiftdetail.causeview__Amount__c = Convert.ToDouble(item.Amount);
                       
                        _websupport.PostGift(mygift,mygiftdetail, _filename);
                        
                    }
                }
            }
        }

        #endregion

    }

    [DelimitedRecord(",")]
    public class StripeRecordOld
    {
        public String Created;
        public String Id;
        public String Amount;
        public String Status;
        public String CardName;
        public String CardAddr;
        public String CardCity;
        public String CardState;
        public String CardZip;
        public String CardEmail;
        public String Campaign;
        public String Honoree;
        [FieldOptional]
        public String Junk;
    }

}
