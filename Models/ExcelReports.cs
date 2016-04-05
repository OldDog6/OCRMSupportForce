using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.ComponentModel;
using OCRMSupportForce.Supporting;

using Excel = NetOffice.ExcelApi;

namespace OCRMSupportForce.Models
{
    public class LapsedDonorsReport
    {
        #region private Properties

        mySqlModel _connection;

        BackgroundWorker _thread;

        // parameters
        DateTime _fromdate;
        DateTime _todate;

        // results
        bool _inerror;
        String _errormessage;

        // Excel commons
        Excel.Workbook    _workbook;
        Excel.Worksheet   _worksheet;
        Excel.Application _application;

        // Report Title Fields
        String _username;
        int _recordcount;

        // Report Columns
        String _causeview_ID;
        String _donorid;
        String _addressee;
        String _salutation;
        String _casualsalutation;
        String _lastname;
        String _internalsolicitor;
        String _address;
        String _city;
        String _state;
        String _zip;
        String _phone;
        String _telemarketingnotes;
        String _cellphone;
       
        Decimal _lastgiftamount;
        Decimal _largestGift;
        DateTime _lastgiftdate;
        int _lifetimefreq;
        Decimal _lifetimegiving;
        Decimal _liftimeaverage;

        int _currentrow;

        #endregion

        #region creation

        public LapsedDonorsReport(mySqlModel con, String username)
        {
            _connection = con;
            _username = username;
        }

        #endregion

        #region private methods

        #region Excel Handling

        /// <summary>
        /// Launch Excel workbook, write header and goto body of query
        /// </summary>
        private void CreateSpreadsheet()
        {
            try
            {
                _application = new Excel.Application();
                _workbook = _application.Workbooks.Add();

                _application.Visible = false;
                _application.DisplayAlerts = true;

                var style = _application.ActiveWorkbook.Styles.Add("HeaderStyle");
                style.Font.Name = "Verdana";
                style.Font.Size = 10;
                style.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Black);
                style.Font.Bold = true;

                _worksheet = (Excel.Worksheet) _application.ActiveSheet;

                // Add Header
                WriteHeader();

                GetLapsedDonors();

            }
            catch (Exception e)
            {
                _inerror = true;
                _errormessage = e.ToString();
                _thread.ReportProgress(-1);
                _thread.CancelAsync();
            }
            finally
            {
                _application.Visible = true;
            }
        }

        private void WriteHeader()
        {
            GetRecordCount();

            Excel.Range header;
            Excel.Range titles;

            header = _worksheet.Range(_worksheet.Cells[1, 6], _worksheet.Cells[1, 6]);
            header.Style = "Title";

            header.Value = "OCRM Lapsed Donor Report";

            titles = _worksheet.Range(_worksheet.Cells[6, 1],
                            _worksheet.Cells[6, 20]);
            titles.Style = "Heading 2";

            _worksheet.Range("A6").Value = "Causeview ID";
            _worksheet.Range("B6").Value = "DonorID";
            _worksheet.Range("C6").Value = "Addressee";
            _worksheet.Range("D6").Value = "Salutation";
            _worksheet.Range("E6").Value = "Casual Salutation";
            _worksheet.Range("F6").Value = "Last Name";
            _worksheet.Range("G6").Value = "Internal Solicitor";
            _worksheet.Range("H6").Value = "Address";
            _worksheet.Range("I6").Value = "City";
            _worksheet.Range("J6").Value = "State";
            _worksheet.Range("K6").Value = "Zip";
            _worksheet.Range("L6").Value = "Phone";
            _worksheet.Range("M6").Value = "Cell Phone";
            _worksheet.Range("N6").Value = "Last Gift Amount";
            _worksheet.Range("O6").Value = "Last Gift Date";
            _worksheet.Range("P6").Value = "Largest Lifetime Gift";
            _worksheet.Range("Q6").Value = "Lifetime Gift Count";
            _worksheet.Range("R6").Value = "Lifetime Sum of Gifts";
            _worksheet.Range("S6").Value = "Average Gift";
            _worksheet.Range("T6").Value = "Telemarketing Notes";

            _worksheet.Name = "mySql Export";

            Excel.Range after = _worksheet.Range("A2");
            after.Style = "Normal";
            after.Value = String.Format("After: {0:M/d/yyyy}", _fromdate);

            Excel.Range before = _worksheet.Range("A3");
            before.Style = "Normal";
            before.Value = String.Format("Before: {0:M/d/yyyy}", _todate);

            Excel.Range cnt = _worksheet.Range("A4");
            cnt.Style = "Normal";
            cnt.Value = String.Format("Records: {0}", _recordcount);

            Excel.Range rdate = _worksheet.Range("N2");
            rdate.Style = "Normal";
            rdate.Value = String.Format("Run Date: {0:M/d/yyyy}", DateTime.Now);

            Excel.Range rtime = _worksheet.Range("N3");
            rtime.Style = "Normal";
            rtime.Value = String.Format("Run Date: {0:t}", DateTime.Now);

            Excel.Range bywho = _worksheet.Range("N4");
            bywho.Style = "Normal";
            bywho.Value = String.Format("By: {0}", _username);

            // Set Widths
            _worksheet.Range("A1").ColumnWidth = 12;
            _worksheet.Range("B1").ColumnWidth = 21;
            _worksheet.Range("C1").ColumnWidth = 20;
            _worksheet.Range("D1").ColumnWidth = 22;
            _worksheet.Range("E1").ColumnWidth = 20;
            _worksheet.Range("F1").ColumnWidth = 20;
            _worksheet.Range("G1").ColumnWidth = 20;
            _worksheet.Range("H1").ColumnWidth = 25;
            _worksheet.Range("I1").ColumnWidth = 16;
            _worksheet.Range("J1").ColumnWidth = 15;
            _worksheet.Range("K1").ColumnWidth = 13;
            _worksheet.Range("L1").ColumnWidth = 15;
            _worksheet.Range("M1").ColumnWidth = 12;
            _worksheet.Range("N1").ColumnWidth = 20;
            _worksheet.Range("O1").ColumnWidth = 15;
            _worksheet.Range("P1").ColumnWidth = 15;
            _worksheet.Range("Q1").ColumnWidth = 15;
            _worksheet.Range("R1").ColumnWidth = 15;
            _worksheet.Range("S1").ColumnWidth = 15;
            _worksheet.Range("T1").ColumnWidth = 64;
        }

        private void WriteBody()
        {
            try
            {
                int rw = _currentrow + 7;
                String srw = rw.ToString();

                _worksheet.Range("A" + srw).Value = _causeview_ID;

                Excel.Range rng = _worksheet.Range("B" + srw);
                _worksheet.Hyperlinks.Add(rng, "https://na32.salesforce.com/" + _donorid, "", "", _donorid);

                _worksheet.Range("C" + srw).Value = _addressee;
                _worksheet.Range("D" + srw).Value = _salutation;
                _worksheet.Range("E" + srw).Value = _casualsalutation;
                _worksheet.Range("F" + srw).Value = _lastname;
                _worksheet.Range("G" + srw).Value = _internalsolicitor;
                _worksheet.Range("H" + srw).Value = _address;
                _worksheet.Range("I" + srw).Value = _city;
                _worksheet.Range("J" + srw).Value = _state;
                _worksheet.Range("K" + srw).Value = _zip;
                _worksheet.Range("L" + srw).Value = _phone;
                _worksheet.Range("M" + srw).Value = _cellphone;
                _worksheet.Range("N" + srw).Value = _lastgiftamount;
                _worksheet.Range("O" + srw).Value = _lastgiftdate;
                _worksheet.Range("P" + srw).Value = _largestGift;
                _worksheet.Range("Q" + srw).Value = _lifetimefreq;
                _worksheet.Range("R" + srw).Value = _lifetimegiving;
                _worksheet.Range("S" + srw).Value = _liftimeaverage;
                _worksheet.Range("T" + srw).Value = _telemarketingnotes;

                _thread.ReportProgress(_currentrow);

                _currentrow++;
            }
            catch (Exception e)
            {
                _inerror = true;
                _errormessage = e.ToString();
                _thread.ReportProgress(-1);
                _thread.CancelAsync();
            }
        }

        #endregion

        #region mySql Calls

        private void GetRecordCount()
        {
            String query = LclQueries.LapsedDonorCountQ;

            MySqlCommand cmd = new MySqlCommand(query, _connection.MyConnection);
            cmd.Parameters.AddWithValue("@BEGINDATEPARAM", _fromdate);
            cmd.Parameters.AddWithValue("@ENDDATEPARAM", _todate);

            try
            {
                if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                    cmd.Connection.Open();
                _recordcount = Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception e)
            {
                _inerror = true;
                _errormessage = e.ToString();
                _recordcount = 0;
            }
            finally
            {
                cmd.Connection.Close();
            }
        }

        private void GetLapsedDonors()
        {
            // Max out net read
            MySqlCommand tocmd = new MySqlCommand("set net_write_timeout=99999; set net_read_timeout=99999", _connection.MyConnection);
            try
            {
                if (tocmd.Connection.State == System.Data.ConnectionState.Closed)
                    tocmd.Connection.Open();

                tocmd.ExecuteNonQuery();
            }
            catch(Exception e)
            {
                _inerror = true;
                _errormessage = e.ToString();
                _thread.ReportProgress(-1);
                _thread.CancelAsync();
            }
            finally
            {
                tocmd.Connection.Close();
            }

            _currentrow = 0;

            String query = LclQueries.SelectLapsedDonors;
            MySqlCommand cmd = new MySqlCommand(query, _connection.MyConnection);
            cmd.CommandTimeout = 2000;
            cmd.Parameters.AddWithValue("@BEGINDATEPARAM", _fromdate);
            cmd.Parameters.AddWithValue("@ENDDATEPARAM", _todate);

            MySqlDataReader reader;

            try
            {
                if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                    cmd.Connection.Open();

                reader = cmd.ExecuteReader();
      
                while (reader.Read())
                {
                    PrepForWriting(reader);
                }
                reader.Close();
            }
            catch (Exception e)
            {
                _inerror = true;
                _errormessage = e.ToString();
                _thread.ReportProgress(-1);
                _thread.CancelAsync();
            }
            finally
            {
                cmd.Connection.Close();
            }
        }

        #endregion

        /// <summary>
        /// Fill fields _lastgiftdate and lastgiftamount
        /// </summary>
        /// <param name="donorId"></param>
        public void LastDonationData(String donorId)
        {
            // use a seperate connection since the parent is still in use...
            mySqlModel subconnection = new mySqlModel();

            MySqlDataReader reader;

            String query = LclQueries.LapsedGivingQ;
            MySqlCommand cmd = new MySqlCommand(query, subconnection.MyConnection);

            cmd.Parameters.AddWithValue("@DONORIDPARAM", donorId);

            try
            {
                if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                    cmd.Connection.Open();

                reader = cmd.ExecuteReader();

                // should only have one row, if none found set to 0
                if (reader.Read())
                {
                    _largestGift = reader.GetDecimal(0);
                    _lastgiftamount = reader.GetDecimal(1);
                    _lifetimefreq = reader.GetInt32(2);
                    _lifetimegiving = reader.GetDecimal(3);
                    if (_lifetimefreq > 0)
                        _liftimeaverage = _lifetimegiving / _lifetimefreq;
                    else
                        _liftimeaverage = 0;

                    _internalsolicitor = StringHandling.SafeGetString(reader, 4);
                }
                else
                {
                    _largestGift = 0;
                    _lastgiftamount = 0;
                    _lifetimefreq = 0;
                    _lifetimegiving = 0;
                    _liftimeaverage = 0;
                    _internalsolicitor = String.Empty;
                }
            }
            catch (Exception e)
            {
                _inerror = true;
                _errormessage = e.ToString();
                _thread.ReportProgress(-1);
                _thread.CancelAsync();
            }
            finally
            {
                cmd.Connection.Close();
            }
        }


        private void PrepForWriting(MySqlDataReader reader)
        {
            String sal = String.Empty;

            try
            {
                sal = StringHandling.SafeGetString(reader, 2);
                _donorid = StringHandling.SafeGetString(reader, 0);

                _causeview_ID = StringHandling.SafeGetString(reader, 13);

                if (sal != String.Empty)
                    _addressee = sal + ' ' + StringHandling.SafeGetString(reader, 1);  // Sal + Name  Mr & Mrs John Smith
                else
                    _addressee = StringHandling.SafeGetString(reader, 1);  // Sal + Name  Mr & Mrs John Smith

                _lastname = StringHandling.SafeGetString(reader, 4);

                if (sal != String.Empty)
                    _salutation = sal + ' ' + _lastname;  // Sal + LastName // Mr and Mrs Smith
                else
                    _salutation = _lastname;  // Sal + LastName // Mr and Mrs Smith

                _casualsalutation = StringHandling.SafeGetString(reader, 3);  // Just First Name

                _address = StringHandling.SafeGetString(reader, 5);
                _city = StringHandling.SafeGetString(reader, 6);
                _state = StringHandling.SafeGetString(reader, 7);
                _zip = StringHandling.SafeGetString(reader, 8);
                _phone = StringHandling.SafeGetString(reader, 9);
                _lastgiftdate = reader.GetDateTime(10);
                _cellphone = StringHandling.SafeGetString(reader, 12);
                _telemarketingnotes = StringHandling.SafeGetString(reader, 11);
                

                LastDonationData(_donorid);
                WriteBody();
            }
            catch (Exception e)
            {
                _inerror = true;
                _errormessage = e.ToString();
                _thread.ReportProgress(-1);
                _thread.CancelAsync();
            }
        }

        #endregion

        #region public properties

        public BackgroundWorker ExcelThread
        {
            get { return _thread; }
            set { _thread = value; }
        }

        public DateTime StartDate
        {
            get { return _fromdate; }
            set { _fromdate = value; }
        }

        public DateTime EndDate
        {
            get { return _todate; }
            set { _todate = value; }
        }

        public int OnRecord
        {
            get { return _currentrow; }
            set { _currentrow = value; }
        }

        public int MaxRecords
        {
            get { return _recordcount; }
            set { _recordcount = value; }
        }

        public String ErrorMessage
        {
            get { return _errormessage; }
            set { _errormessage = value; }
        }

        public bool InError
        {
            get { return _inerror; }
            set { _inerror = value; }
        }

        #endregion

        #region public methods

        public void LoadSpreadsheet()
        {
            _currentrow = 0;
            CreateSpreadsheet();
        }

        #endregion

    }
}
