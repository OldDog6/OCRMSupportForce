using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using esscWPFShell;
using OCRMSupportForce.SalesForceService;
using OCRMSupportForce.SForceWebReference;
using System.Windows.Data;
using System.Data;

using System.ComponentModel;
using System.Windows.Threading;
using System.ServiceModel;
using System.Xml;
using System.Net;
using System.IO;
using System.Configuration;

using FileHelpers;
using OCRMSupportForce.Supporting;

namespace OCRMSupportForce.Models
{
    public class StripeModel
    {
        public StripeModel(ForceWebSupport connection)
        {
            _connection = connection;
            _batchposting = new BatchPosting(_connection.webbinding);
            _comparsionlist = new List<ComparsionClass>();
            _sfsearch = new ForceSearches(_connection);
        }

        #region private properties

        private ForceWebSupport _connection;

        private List<SForceWebReference.causeview__Gift_Batch__c> _batchlist { get; set; }
        private List<StripeBatches> _openbatcheslist { get; set; }
        private List<ComparsionClass> _comparsionlist;
        private List<SForceWebReference.Contact> _searchlist;

        private ForceSearches _sfsearch;

        private bool _inerror = false;
        private String _errormessage = String.Empty;

        private USStates _states = new USStates();

        private BatchPosting _batchposting;

        #endregion

        #region private methods

        private void _appendresultstolist(SForceWebReference.QueryResult myresult)
        {
            SForceWebReference.sObject[] records = myresult.records;

            for (int i = 0; i < myresult.records.Count(); i++)
            {
                _batchlist.Add((SForceWebReference.causeview__Gift_Batch__c)records[i]);
            }
        }

        private bool isvalidfile(StripeRecord result)
        {
            if ((result.Id != "ID#") || (result.CardAddr != "Card Address Line1"))
                return false;
            else
                return true;
        }

        #endregion

        #region Public Properties

        public bool InError { get { return _inerror; } }
        public String ErrorMessage { get { return _errormessage; } }

        public List<StripeBatches> SF_Batches 
        { 
            get { return _openbatcheslist; }
            set { _openbatcheslist = value; }
        }
        
        public List<ComparsionClass> ComparsionList
        {
            get { return _comparsionlist; }
            set { _comparsionlist = value; }
        }

        public List<SForceWebReference.Contact> SearchList
        {
            get { return _searchlist; }
            set { _searchlist = value; }
        }

        #endregion

        #region public methods

        public void OpenBatchList(ForceWebSupport _websupport)
        {
            SF_Batches = new List<StripeBatches>();
            SForceWebReference.QueryResult queryResult = null;

            if (_batchlist == null)
                _batchlist = new List<SForceWebReference.causeview__Gift_Batch__c>();
            else
                _batchlist.Clear();

            try
            {
                string query = String.Format(BatchSOQLQueries.SelectOpenBatches);

                queryResult = _websupport.webbinding.query(query);
                _appendresultstolist(queryResult);
                _inerror = false;
            }
            catch (Exception e)
            {
                _inerror = true;
                _errormessage = e.ToString();
            }

            SF_Batches.Add(new StripeBatches("NA", "Please select...", String.Empty));

            foreach (SForceWebReference.causeview__Gift_Batch__c item in _batchlist)
            {
                SF_Batches.Add(new StripeBatches(item.Id, item.Name, item.causeview__Name__c));
            }
        }

        public void ProcessStripeFile(String fname)
        {
            if (_comparsionlist.Count > 0)
                _comparsionlist.Clear();
           
            var engine = new FileHelperEngine<StripeRecord>();

            _inerror = false;
            _errormessage = String.Empty;

            try
            {
                var result = engine.ReadFile(fname);

                if (isvalidfile(result[0]))
                {
                    // Populate comparsion class
                    foreach (StripeRecord item in result)
                    {
                        // Skip First Line
                        if (item.Id != "ID#")
                        {
                            // normalize states
                            item.CardState = _states.StateAsAbbr(item.CardState);
                            _comparsionlist.Add(new ComparsionClass(_sfsearch.lookupbyemailorblank(item.CardEmail), item));
                        }
                    }
                }
                else
                {
                    _inerror = true;
                    _errormessage = "The Stripe Batch File is not on the standard format, are you sure you are loading the correct file?";
                }
            }
            catch(Exception e)
            {
                _inerror = true;
                _errormessage = e.ToString();
            }
        }

        public void PopulateSearchList(ComparsionClass searchterms)
        {
            _searchlist = _sfsearch.GetSearchList(searchterms.LastName, searchterms.StripeDonorRecord.CardAddr);
        }
        
        public void ClearSearchList()
        {
            _searchlist.Clear();
        }

        public void UpsertContactRecord(ComparsionClass data)
        {
            if (data.SFDonorFound)
            {
                // Update
                _batchposting.UpdateContact(data.SFDonorRecord);
            }
            else
            {
                // Insert
                _batchposting.InsertContact(data);
            }
        }

        #endregion
    }

    [DelimitedRecord(",")]
    public class StripeRecord
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
        [FieldOptional]
        public String Junk2;

    }

    public class ComparsionClass
    {
        public ComparsionClass(SForceWebReference.Contact sfdonor, StripeRecord stripedonor)
        {
            SFDonorRecord = sfdonor;
            StripeDonorRecord = stripedonor;
            ProcessName(stripedonor.CardName);
            ProcessAddress();

            if (SFDonorFound)
            {
                TestNameMatch();
                TestAddressMatch();
            }
        }

        private USStates _states = new USStates();

        public SForceWebReference.Contact SFDonorRecord;
        public StripeRecord StripeDonorRecord;

        public String FirstName { get; set; }
        public String LastName { get; set; }
        public String DisplayAddress { get; set; }

        public bool SFDonorFound { get { return ((SFDonorRecord.FirstName != "Not")&&(SFDonorRecord.LastName != "Found")); } }
        public bool NameMatches {get; private set;}
        public bool AddrMatches { get; private set; }

        public String RecordState
        {
            get 
            {
                if (!SFDonorFound)
                    return "Not Found";
                else if (NameMatches && AddrMatches)
                    return "Clean";
                else
                    return "Dirty";
            }
        }
        
        #region public methods
        public void ProcessName(String name)
        {
            SplitName n = new SplitName(name, " ");
            FirstName = StringHandling.FirstCapThenlower(n.FirstName);
            LastName = StringHandling.FirstCapThenlower(n.LastName);
        }

        public void ProcessAddress()
        {
            StripeDonorRecord.CardAddr = StringHandling.SanitizeAddressField(StripeDonorRecord.CardAddr);
            StripeDonorRecord.CardCity = StringHandling.SanitizeAddressField(StripeDonorRecord.CardCity);

            DisplayAddress = StripeDonorRecord.CardAddr + ' ' + StripeDonorRecord.CardCity + ' ' + StripeDonorRecord.CardState + ' ' + StripeDonorRecord.CardZip;
        }

        public void TestNameMatch()
        {
            if ((FirstName == SFDonorRecord.FirstName) && (LastName == SFDonorRecord.LastName))
                NameMatches = true;
            else
                NameMatches = false;
        }

        public void TestAddressMatch()
        {
            AddrMatches = true;

            if (StripeDonorRecord.CardAddr.TrimEnd() != SFDonorRecord.MailingStreet.TrimEnd())
                AddrMatches = false;

            if (StripeDonorRecord.CardCity.TrimEnd() != SFDonorRecord.MailingCity.TrimEnd())
                AddrMatches = false;

            if (StripeDonorRecord.CardState.TrimEnd() != SFDonorRecord.MailingState.TrimEnd())
                AddrMatches = false;

            if (StripeDonorRecord.CardZip.TrimEnd() != SFDonorRecord.MailingPostalCode.TrimEnd())
                AddrMatches = false;
        }

        public void InsertSFRecord(SForceWebReference.Contact sfdonor)
        {
            SFDonorRecord = sfdonor;
            if (SFDonorFound)
            {
                TestNameMatch();
                TestAddressMatch();
            }
        }

        public void UpdateSFRecord()
        {
            if (SFDonorFound)
            {
                TestNameMatch();
                TestAddressMatch();
            }
        }

        #endregion
    }
}
