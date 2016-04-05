using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

using MySql.Data.MySqlClient;
using OCRMSupportForce.SalesForceService;
using System.ServiceModel;
using OCRMSupportForce.ViewModels;
using OCRMSupportForce.Supporting;

using Excel = NetOffice.ExcelApi;


namespace OCRMSupportForce.Models
{
    public class OnPremiseModel
    {
        #region private properties

        ForceWSDLSupport _wsdl;
        mySqlModel _connection;

        private String _errormessage;
        private bool _inerror;

        private Excel.Worksheet _worksheet;

        #endregion

        public OnPremiseModel(ForceWSDLSupport sfdcService, mySqlModel connection)
        {
            _wsdl = sfdcService;
            _connection = connection;

            _inerror = false;
            _errormessage = String.Empty;
        }

        #region private methods

        private int CountDonorsInScope()
        {
            SalesForceService.QueryResult _myresult;

            QueryOptions options = new QueryOptions();
            options.batchSize = 250;

            try
            {
                EndpointAddress apiAddr = new EndpointAddress(_wsdl.ServerURL);
                SalesForceService.SessionHeader header = new SessionHeader();
                header.sessionId = _wsdl.SessionID;

                SalesForceService.SoapClient queryClient = new SalesForceService.SoapClient("Soap", apiAddr);
                string query = String.Empty;

                query = String.Format(DetailSOQLQueries.DonorCountQ, FromDate, ToDate);
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
                WorkerThread.ReportProgress(-1);
                return 0;
            }
        }

        private void AppendDonorResultToSQL(QueryResult result)
        {
            SalesForceService.sObject[] records = result.records;
            SalesForceService.Contact myrecord;

            for (int i = 0; i < result.records.Count(); i++)
            {
                if (WorkerThread.CancellationPending)
                {
                    WorkerThread.ReportProgress(-1);
                    break;
                }

                    DonorProgress++;
                    if (WorkerThread != null)
                        WorkerThread.ReportProgress(DonorProgress);

                    myrecord = (SalesForceService.Contact)records[i];
                    DownsertDonorRecord(myrecord);
            }
        }

        private void AppendAccountResultToSQL(QueryResult result)
        {
            SalesForceService.sObject[] records = result.records;
            SalesForceService.Account myrecord;

            for (int i = 0; i < result.records.Count(); i++)
            {
                myrecord = (SalesForceService.Account)records[i];
                DownsertAccountRecord(myrecord);
            }
        }

        private void DownsertAccountRecord(SalesForceService.Account record)
        {

            if (!(AccountIDExists(record.Id)))
            {
                MySqlCommand cmd = new MySqlCommand("INSERT INTO ACCOUNTS VALUES (@IDPARAM,@ORGIDPARAM,@NAMEPARAM,@PHONEPARAM,@ADDRESSPARAM,@CITYPARAM,@STATEPARAM,@ZIPPARAM,@SOLITITORPARAM)", _connection.MyConnection);

                cmd.Parameters.AddWithValue("@IDPARAM", record.Id);
                cmd.Parameters.AddWithValue("@ORGIDPARAM", record.causeview__Organization_ID__c);
                cmd.Parameters.AddWithValue("@NAMEPARAM", record.Name);
                cmd.Parameters.AddWithValue("@PHONEPARAM", record.Phone);
                cmd.Parameters.AddWithValue("@ADDRESSPARAM", record.BillingStreet);
                cmd.Parameters.AddWithValue("@CITYPARAM", record.BillingCity);
                cmd.Parameters.AddWithValue("@STATEPARAM", record.BillingState);
                cmd.Parameters.AddWithValue("@ZIPPARAM", record.BillingPostalCode);
                cmd.Parameters.AddWithValue("@SOLICITORPARAM", record.Internal_Solicitor__c);

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

        private void DownsertDonorRecord(SalesForceService.Contact record)
        {
            String query;

            if (DonorIDExists(record.Id))
                query = LclQueries.UpdateDonorRecord;
            else
                query = LclQueries.InsertDonorRecord;

            MySqlCommand cmd = new MySqlCommand(query, _connection.MyConnection);

            cmd.Parameters.AddWithValue("@IDPARAM", record.Id);
            cmd.Parameters.AddWithValue("@NAMEPARAM", record.Name);
            cmd.Parameters.AddWithValue("@PHONEPARAM", record.Phone);
            cmd.Parameters.AddWithValue("@EMAILPARAM", record.Email);
            cmd.Parameters.AddWithValue("@ADDRPARAM", record.MailingStreet);
            cmd.Parameters.AddWithValue("@CITYPARAM", record.MailingCity);
            cmd.Parameters.AddWithValue("@STATEPARAM", record.MailingState);
            cmd.Parameters.AddWithValue("@ZIPPARAM", record.MailingPostalCode);
            cmd.Parameters.AddWithValue("@HASDONATEDPARAM", record.causeview__Donor__c);
            cmd.Parameters.AddWithValue("@ISMAJORGIFTSPARAM", record.causeview__Major_Gift_Donor__c);
            cmd.Parameters.AddWithValue("@DESRIPTIONPARAM", record.Description);
            cmd.Parameters.AddWithValue("@FIRSTNAMEPARAM", record.FirstName);
            cmd.Parameters.AddWithValue("@EMAILOPTOUTPARAM", record.HasOptedOutOfEmail);
            cmd.Parameters.AddWithValue("@LASTNAMEPARAM", record.LastName);
            cmd.Parameters.AddWithValue("@MIDDLENAMEPARAM", record.Middle_Name__c);
            cmd.Parameters.AddWithValue("@MOBILEPHONEPARAM", record.MobilePhone);
            cmd.Parameters.AddWithValue("@SALUTATIONPARAM", record.Salutation);
            cmd.Parameters.AddWithValue("@TITLEPARAM", record.Title);
            cmd.Parameters.AddWithValue("@WHYSUPPORTPARAM", StringHandling.Truncate(record.Why_They_Support_OCRM__c, 255));
            cmd.Parameters.AddWithValue("@ANONYMOUSPARAM", record.causeview__Anonymous__c);
            cmd.Parameters.AddWithValue("@DECEASEDPARAM", record.causeview__Deceased__c);
            cmd.Parameters.AddWithValue("@COMMPREFERENCEPARAM", record.Communication_Preference__c);
            cmd.Parameters.AddWithValue("@DELETEDPARAM", record.IsDeleted);
            cmd.Parameters.AddWithValue("@TELEMARKETINGNOTESPARAM", record.Telemarketing__c);
            cmd.Parameters.AddWithValue("@LASTDONATIONDATEPARAM", record.causeview__Date_of_Last_Gift__c);
            cmd.Parameters.AddWithValue("@CONSTITUENTIDPARAM", record.causeview__Constituent_ID__c);
            cmd.Parameters.AddWithValue("@ACCOUNTIDPARAM", record.AccountId);
            cmd.Parameters.AddWithValue("@MATCHKEYPARAM", AsMatchkey(record));

            try
            {
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                _inerror = true;
                _errormessage = e.ToString();
                WorkerThread.ReportProgress(-1);
                WorkerThread.CancelAsync();
            }
            finally
            {
                cmd.Connection.Close();
            }
        }

        private bool AccountIDExists(String Id)
        {
            bool rval = false;
            int count;
            String query = "SELECT ID FROM ACCOUNTS WHERE ID = " + String.Format(" '{0}'", Id);

            MySqlCommand cmd = new MySqlCommand(query, _connection.MyConnection);

            try
            {
                if (cmd.Connection.State == System.Data.ConnectionState.Closed)
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

        private bool DonorIDExists(String Id)
        {
            bool rval = false;
            int count;
            String query = LclQueries.DonorExistsQ + String.Format(" '{0}'", Id);

            MySqlCommand cmd = new MySqlCommand(query, _connection.MyConnection);

            try
            {
                if (cmd.Connection.State == System.Data.ConnectionState.Closed) 
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
                WorkerThread.CancelAsync();
                
            }
            finally
            {
                cmd.Connection.Close();
            }
            return rval;
        }

        private String AsMatchkey(SalesForceService.Contact record)
        {
            // return ZipPlus toUpper of lastname, a space and then the address up to first whitespace as Matchkey
            // Without record or address return an empty string
            String rval = String.Empty;

            if (record == null)
                return rval;

            if (record.MailingAddress == null) 
                return rval;

            if (record.MailingAddress.street == null)
                return rval;

            if (record.MailingPostalCode == null)
                return rval;

            if (record.MailingPostalCode.Length < 5)
                return rval;

            // If LName Hypened, just use first LName
            if (record.LastName.IndexOf('-') > 0)
                rval = rval + record.MailingPostalCode.Substring(0,5) + record.LastName.Substring(0,record.LastName.IndexOf('-')).ToUpper()  +' ';
            else
                rval = rval + record.MailingPostalCode.Substring(0, 5) + record.LastName.ToUpper().Replace("'","") + ' ';

            // really, get rid of control chars
            rval = rval.Replace("'", "");

            //If no space in address?
            if (record.MailingAddress.street.IndexOf(' ') > 0)
                rval = rval + record.MailingAddress.street.Substring(0, record.MailingAddress.street.IndexOf(' '));

            return StringHandling.Truncate(rval,64);
        }

        private int CountPaymentsInScope()
        {
            SalesForceService.QueryResult _myresult;

            QueryOptions options = new QueryOptions();
            options.batchSize = 250;

            try
            {
                EndpointAddress apiAddr = new EndpointAddress(_wsdl.ServerURL);
                SalesForceService.SessionHeader header = new SessionHeader();
                header.sessionId = _wsdl.SessionID;

                SalesForceService.SoapClient queryClient = new SalesForceService.SoapClient("Soap", apiAddr);
                string query = String.Empty;

                query = String.Format(DetailSOQLQueries.PaymentCountQ, FromDate, ToDate);
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
                WorkerThread.ReportProgress(-1);
                return 0;
            }
        }

        private void AppendPaymentResultToSQL(QueryResult result)
        {
            SalesForceService.sObject[] records = result.records;
            SalesForceService.causeview__Gift_Detail__c  myrecord;

            for (int i = 0; i < result.records.Count(); i++)
            {
                if (WorkerThread.CancellationPending)
                {
                    WorkerThread.ReportProgress(-1);
                    break;
                }

                PaymentProgress++;
                if (WorkerThread != null)
                    WorkerThread.ReportProgress(PaymentProgress);

                myrecord = (SalesForceService.causeview__Gift_Detail__c)records[i];

                if (myrecord != null)
                        DownsertPaymentRecord(myrecord);
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
                WorkerThread.CancelAsync();
                WorkerThread.ReportProgress(-1);
            }
            finally
            {
                cmd.Connection.Close();
            }
            return rval;
        }

        private bool MatchKeyExists(SalesForceService.Contact donorrecord, int calyear)
        {
            bool rval = false;
            int count;
            String query = String.Format(LclQueries.MatchkeyExistsQ, AsMatchkey(donorrecord), calyear);

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
                WorkerThread.CancelAsync();
                WorkerThread.ReportProgress(-1);
            }
            finally
            {
                cmd.Connection.Close();
            }
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

            ID = myrecord.causeview__Payment__c;
            Decimal Amount = Convert.ToDecimal(myrecord.causeview__Payment__r.causeview__Amount__c);
            DateTime RecDate = Convert.ToDateTime(myrecord.causeview__Payment__r.causeview__Date__c);

            FundName = StringHandling.Truncate(myrecord.causeview__Fund__c,45);

            DonorID = GetPaymentDonorID(myrecord);

            GiftType = StringHandling.Truncate(myrecord.causeview__Payment__r.causeview__Donation__r.causeview__Gift_Type__c, 45);

            if (myrecord.causeview__Payment__r.causeview__Donation__r.causeview__Constituent__r == null)
                Solicitor = String.Empty;
            else
                Solicitor = StringHandling.Truncate(myrecord.causeview__Payment__r.causeview__Donation__r.causeview__Constituent__r.Internal_Solicitor__c,45);

            if (myrecord.causeview__Campaign__r == null)
                Campaign = String.Empty;
            else
                Campaign = StringHandling.Truncate(myrecord.causeview__Campaign__r.Id,45);

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
                WorkerThread.CancelAsync();
                WorkerThread.ReportProgress(-1);
            }
            finally
            {
                cmd.Connection.Close();
            }

            DownsertSummaryRecord(myrecord);
        }

        private  SalesForceService.Contact GetDonorRecord(String DonorID)
        {
            SalesForceService.Contact rval = null;
            if (DonorID != String.Empty)
            {
                SalesForceService.QueryResult _myresult;

                QueryOptions options = new QueryOptions();
                options.batchSize = 250;

                try
                {
                    EndpointAddress apiAddr = new EndpointAddress(_wsdl.ServerURL);
                    SalesForceService.SessionHeader header = new SessionHeader();
                    header.sessionId = _wsdl.SessionID;

                    SalesForceService.SoapClient queryClient = new SalesForceService.SoapClient("Soap", apiAddr);

                    String query = DetailSOQLQueries.GetDonorByID + "where Id = '" + DonorID + "'";

                    queryClient.query(header,
                                      options,
                                      null,
                                      null,
                                      query,
                                      out _myresult);
                    
                    if (_myresult != null)
                        if (_myresult.records != null)
                            if (_myresult.records.Count() > 0)
                                rval = (SalesForceService.Contact)_myresult.records[0];
                }
                catch (Exception e)
                {
                    _inerror = true;
                    _errormessage = e.ToString();
                    WorkerThread.ReportProgress(-1);
                }
            }
            return rval;
        }

        private void DownsertSummaryRecord(SalesForceService.causeview__Gift_Detail__c payrecord)
        {
            SalesForceService.Contact donorrecord = GetDonorRecord(GetPaymentDonorID(payrecord));

            if (donorrecord != null)
            {
                String query;
                Decimal totaldonations = 0m;


                if (MatchKeyExists(donorrecord, 2015))
                {
                    query = LclQueries.UpdateMatchKeyRecord;
                    totaldonations = GetPaymentsSummary(AsMatchkey(donorrecord), 2015);
                }
                else
                {
                    query = LclQueries.InsertMatchKeyRecord;
                    totaldonations = 0m;
                }

                totaldonations = totaldonations + Convert.ToDecimal(payrecord.causeview__Payment__r.causeview__Amount__c);

                MySqlCommand cmd = new MySqlCommand(query, _connection.MyConnection);

                cmd.Parameters.AddWithValue("@MATCHKEYPARAM", AsMatchkey(donorrecord));
                cmd.Parameters.AddWithValue("@CALYEARPARAM", 2015);
                cmd.Parameters.AddWithValue("@TOTALDONATIONSPARAM", totaldonations);

                try
                {
                    cmd.Connection.Open();
                    if (AsMatchkey(donorrecord) != String.Empty)
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    _inerror = true;
                    _errormessage = e.ToString();
                    WorkerThread.CancelAsync();
                    WorkerThread.ReportProgress(-1);
                }
                finally
                {
                    cmd.Connection.Close();
                }
            }
        }

        private Decimal GetPaymentsSummary(String matchkey, int calyear)
        {
            String query = String.Format("Select totaldonations from paymentsummary where matchkey = '{0}' and calyear = {1}",matchkey,calyear);
            Decimal rval = 0m;
            MySqlCommand cmd = new MySqlCommand(query, _connection.MyConnection);

            try
            {
                cmd.Connection.Open();
                rval = Convert.ToDecimal(cmd.ExecuteScalar());
            }
            catch (Exception e)
            {
                _inerror = true;
                _errormessage = e.ToString();
                WorkerThread.CancelAsync();
                WorkerThread.ReportProgress(-1);
            }
            finally
            {
                cmd.Connection.Close();
            }

            return rval;
        }

        private String GetPaymentDonorID(SalesForceService.causeview__Gift_Detail__c myrecord)
        {
            String rval = String.Empty;

            if (myrecord.causeview__Payment__r.causeview__Donation__r != null)
            {

                if (myrecord.causeview__Payment__r.causeview__Donation__r.causeview__Constituent__c != null)
                    rval = myrecord.causeview__Payment__r.causeview__Donation__r.causeview__Constituent__c;
                else
                if (myrecord.causeview__Payment__r.causeview__Donation__r.causeview__Organization__c != null)
                    rval = myrecord.causeview__Payment__r.causeview__Donation__r.causeview__Organization__c;
            }
            return rval;
        }

        // 5 K Q Items
        private void OpenExcelWorksheet()
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

                _worksheet = (Excel.Worksheet)application.ActiveSheet;

            }
            catch (Exception e)
            {
                _errormessage = e.ToString();
                _inerror = true;
            }
        }
 
        private int FiveKDetail(String donorID, int row)
        {
            // get a new connection and command...
            mySqlModel _lclconnection = new mySqlModel();

            MySqlCommand cmd = new MySqlCommand(BatchSOQLQueries.FiveKDetailQ, _lclconnection.MyConnection);
            MySqlDataReader reader;

            cmd.Parameters.AddWithValue("@DonorIDParam", donorID);

            try
            {
                _lclconnection.MyConnection.Open();
                reader = cmd.ExecuteReader();

                while(reader.Read())
                {
                    row++;
                    _worksheet.Cells[row,5].Value = reader[1].ToString();
                    _worksheet.Cells[row,6].Value = reader[2].ToString();
                    _worksheet.Cells[row,7].Value = reader[3].ToString();
                }
            }
            catch(Exception e)
            {

            }
            finally
            {
                _lclconnection.MyConnection.Close();
            }

            return row;
        }

        // Task List

        private void InsertTask(QueryResult myresult)
        {
            // My SQl Insert...
            MySqlCommand cmd = new MySqlCommand("INSERT INTO TASKS VALUES (@IDPARAM,@DESCPARAM,@CREATEDPARAM,@SOLICITORPARAM,@DONORPARAM,@SUBJECTPARAM)", _connection.MyConnection);

            SalesForceService.sObject[] records = myresult.records;
            SalesForceService.Task myrecord;

            for (int i = 0; i < records.Count(); i++)
            {
                myrecord = (SalesForceService.Task)records[i];

                // Set Parameters
                cmd.Parameters.AddWithValue("@IDPARAM", myrecord.Id);
                cmd.Parameters.AddWithValue("@DESCPARAM", String.Empty);
                cmd.Parameters.AddWithValue("@CREATEDPARAM", (DateTime?) myrecord.CreatedDate);
                cmd.Parameters.AddWithValue("@SOLICITORPARAM", myrecord.OwnerId);
                cmd.Parameters.AddWithValue("@DONORPARAM", myrecord.WhoId);
                cmd.Parameters.AddWithValue("@SUBJECTPARAM", myrecord.Subject);

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

                    cmd.Parameters.Clear();

                }
            }
        }

        #endregion

        #region Public properties

        public BackgroundWorker WorkerThread { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        // All donors in scope
        public int CountOfDonors { get; set; }

        // Current record
        public int DonorProgress { get; set; }

        public bool OnDonors { get; set; }

        public int CountOfPayments { get; set; }

        public int PaymentProgress { get; set; }

        public bool InError
        {
            get { return _inerror; }
        }

        public String ErrorMessage
        {
            get { return _errormessage; }
        }

        #endregion

        #region Public methods

        public void DownloadAllAccounts()
        {
            bool done = false;

            QueryOptions options = new QueryOptions();
            SalesForceService.QueryResult _myresult;

            options.batchSize = 250;

            try
            {
                EndpointAddress apiAddr = new EndpointAddress(_wsdl.ServerURL);
                SalesForceService.SessionHeader header = new SessionHeader();
                header.sessionId = _wsdl.SessionID;

                SalesForceService.SoapClient queryClient = new SalesForceService.SoapClient("Soap", apiAddr);
                string query = String.Empty;

                query = String.Format("SELECT Id, causeview__Organization_ID__c,Name, BillingStreet, BillingCity, BillingPostalCode, BillingState, Primary_Contact__c, Phone, Internal_Solicitor__c FROM Account");
                queryClient.query(header,
                                     options,
                                     null,
                                     null,
                                     query,
                                     out _myresult);

                while (!done)
                {
                    AppendAccountResultToSQL(_myresult);

                    if ((_myresult.done) || (_inerror))
                    {
                        done = true;
                    }
                    else
                    {
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

        public void ExecuteDonorCount()
        {
            DonorProgress = 0;
            CountOfDonors = CountDonorsInScope();
        }

        public void ExecuteDonorQuery()
        {
            OnDonors = true;

            DonorProgress = 0;
            QueryOptions options = new QueryOptions();
            SalesForceService.QueryResult _myresult;

            options.batchSize = 250;

            try
            {
                EndpointAddress apiAddr = new EndpointAddress(_wsdl.ServerURL);
                SalesForceService.SessionHeader header = new SessionHeader();
                header.sessionId = _wsdl.SessionID;

                SalesForceService.SoapClient queryClient = new SalesForceService.SoapClient("Soap", apiAddr);
                string query = String.Empty;

                query = String.Format(DetailSOQLQueries.PopulateDonorTable, FromDate, ToDate);
                 
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

                    if ((_myresult.done) ||(_inerror))
                    {
                        done = true;
                    }
                    else
                    {
                        queryClient.queryMore(header, options, _myresult.queryLocator, out _myresult);
                    }
                }
                 _inerror = false;
            }
            catch (Exception e)
            {
                _inerror = true;
                _errormessage = e.ToString();
                WorkerThread.ReportProgress(-1);
                WorkerThread.CancelAsync();
            }
            OnDonors = false;
        }

        public void ExecutePaymentCount()
        {
            PaymentProgress = 0;
            CountOfPayments = CountPaymentsInScope();
        }

        public void ExecutePaymentQuery()
        {
            PaymentProgress = 0;
            WorkerThread.ReportProgress(0);

            QueryOptions options = new QueryOptions();
            SalesForceService.QueryResult _myresult;

            options.batchSize = 250;

            try
            {
                EndpointAddress apiAddr = new EndpointAddress(_wsdl.ServerURL);
                SalesForceService.SessionHeader header = new SessionHeader();
                header.sessionId = _wsdl.SessionID;

                SalesForceService.SoapClient queryClient = new SalesForceService.SoapClient("Soap", apiAddr);
                string query = String.Empty;

                query = String.Format(DetailSOQLQueries.PopulatePaymentTable, FromDate, ToDate);

                queryClient.query(header,
                                     options,
                                     null,
                                     null,
                                     query,
                                     out _myresult);

                bool done = false;


                while (!done)
                {

                    if (_myresult != null)
                        AppendPaymentResultToSQL(_myresult);

                    if ((_myresult.done) || (_inerror))
                    {
                        done = true;
                    }
                    else
                    {
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

        public void ExecuteFiveKQuery()
        {
            OpenExcelWorksheet();

            MySqlCommand cmd = new MySqlCommand(BatchSOQLQueries.FiveKQ, _connection.MyConnection);
            MySqlDataReader reader;

            int row = 3;


            try
            {
                cmd.Connection.Open();
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    if (reader[1].ToString() != "Anonymous")
                    {
                        // process to Excel
                        _worksheet.Cells[row, 1].Value = reader[1].ToString();
                        _worksheet.Cells[row, 2].Value = reader[2].ToString();
                        _worksheet.Cells[row, 3].Value = reader[3].ToString();
                        _worksheet.Cells[row, 4].Value = reader[4].ToString();
                        _worksheet.Cells[row, 5].Value = reader[5].ToString();
                        _worksheet.Cells[row, 6].Value = reader[6].ToString();
                        _worksheet.Cells[row, 7].Value = reader[7].ToString();

                        row = FiveKDetail(reader[0].ToString(), row);

                        _worksheet.Cells[row, 8].Value = reader[8].ToString();

                        row++;
                    }
                }
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

        public void ExecuteTaskQuery()
        {
            QueryOptions options = new QueryOptions();
            SalesForceService.QueryResult _myresult;

            options.batchSize = 250;

            try
            {
                EndpointAddress apiAddr = new EndpointAddress(_wsdl.ServerURL);
                SalesForceService.SessionHeader header = new SessionHeader();
                header.sessionId = _wsdl.SessionID;

                SalesForceService.SoapClient queryClient = new SalesForceService.SoapClient("Soap", apiAddr);
                string query = String.Empty;

                query = String.Format(BatchSOQLQueries.Task2015Q);

                queryClient.query(header,
                                     options,
                                     null,
                                     null,
                                     query,
                                     out _myresult);

                bool done = false;

                while (!done)
                {

                    if (_myresult != null)
                        InsertTask(_myresult);

                    if ((_myresult.done) || (_inerror))
                    {
                        done = true;
                    }
                    else
                    {
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
    }


}
