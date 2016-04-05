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
    public class CampaignModel
    {
        #region private properties

        ForceWSDLSupport _connection;
        bool _queryinerror;
        String _errormessage;
        
        #endregion


        #region Constructor

        public CampaignModel(ForceWSDLSupport forceWSDLSupport)
        {
            _connection = forceWSDLSupport;
            _queryinerror = false;
        }

        #endregion

        #region Public Properties

        public bool InError
        {
            get { return _queryinerror; }
            set { _queryinerror = value; }
        }

        public String ErrorMessage
        {
            get { return _errormessage; }
            set { _errormessage = value; }
        }

        #endregion

        #region public methods

        public SalesForceService.Campaign GetParentCampaign(String parentname)
        {
            QueryOptions options = new QueryOptions();
            options.batchSize = 250;
            SalesForceService.QueryResult _myresult = new QueryResult();

            String query = String.Format("select Id, Description, Name, StartDate, EndDate, Type, Status, Classification_Code__c from Campaign where Name = '{0}'", parentname);

            try
            {
                EndpointAddress apiAddr = new EndpointAddress(_connection.ServerURL);
                SalesForceService.SessionHeader header = new SessionHeader();
                header.sessionId = _connection.SessionID;

                SalesForceService.SoapClient queryClient = new SalesForceService.SoapClient("Soap", apiAddr);
 
                queryClient.query(header,
                                     options,
                                     null,
                                     null,
                                     query,
                                     out _myresult);
            }
            catch(Exception e)
            {
                _queryinerror = true;
                _errormessage = e.ToString();
            }

            // record not found
            if (_myresult.records == null)
            {
                _queryinerror = true;
                _errormessage = "No results found";
                return null;
            }
            else
                return (SalesForceService.Campaign)_myresult.records[0];
        }

        public List<SalesForceService.Campaign> GetParentCampaignList(SalesForceService.Campaign parent)
        {
            List<SalesForceService.Campaign> rval = new List<SalesForceService.Campaign>();
            rval.Add(parent);

            QueryOptions options = new QueryOptions();
            options.batchSize = 250;
            SalesForceService.QueryResult _myresult = new QueryResult();

            String query = String.Format("select Id, Name, StartDate, EndDate, Type, Status, ParentId, IsActive, ExternalId__c, RecordTypeID from Campaign where ParentId = '70138000001DCi5AAG'");
            try
            {
                EndpointAddress apiAddr = new EndpointAddress(_connection.ServerURL);
                SalesForceService.SessionHeader header = new SessionHeader();
                header.sessionId = _connection.SessionID;

                SalesForceService.SoapClient queryClient = new SalesForceService.SoapClient("Soap", apiAddr);

                queryClient.query(header,
                                     options,
                                     null,
                                     null,
                                     query,
                                     out _myresult);
            }
            catch (Exception e)
            {
                _queryinerror = true;
                _errormessage = e.ToString();
            }

            if (_myresult.records != null)
            {
                foreach (SalesForceService.Campaign r in _myresult.records)
                {
                    rval.Add(r);
                }
            }

            return rval;
        }

        public void InsertChildCampaign(SalesForceService.Campaign parent, SalesForceService.Campaign child)
        {
            if (!(ChildExists(parent,child)))
            {
                SaveResult[] results = new SaveResult[1];
                SalesForceService.LimitInfo[] li = new LimitInfo[1];
                PackageVersion[] pv = new PackageVersion[1];

                child.ParentId = parent.Id;
                EndpointAddress apiAddr = new EndpointAddress(_connection.ServerURL);

                using (SalesForceService.SoapClient createClient = new SalesForceService.SoapClient("Soap", apiAddr))
                {
                    try
                    {
                        createClient.create(_connection.SessionHeader, null, null, null, null, null, null, null, null, null,
                            pv,
                            null,
                            new SalesForceService.sObject[] { child },
                            out li,
                            out results);

                        if (!(results[0].success))
                        {
                            _queryinerror = true;
                            _errormessage = results[0].errors[0].message;
                        }
                    }
                    catch (Exception e)
                    {
                        _queryinerror = true;
                        _errormessage = e.ToString();
                    }
                }
            }
        }

        public bool ChildExists(SalesForceService.Campaign parent, SalesForceService.Campaign child)
        {
            bool rval = false;
            QueryOptions options = new QueryOptions();
            options.batchSize = 250;
            SalesForceService.QueryResult _myresult = new QueryResult();

            String query = String.Format("select Id, Name from Campaign where Name = '{0}' and ParentId = '{1}'", child.Name, parent.Id);

            try
            {
                EndpointAddress apiAddr = new EndpointAddress(_connection.ServerURL);
                SalesForceService.SessionHeader header = new SessionHeader();
                header.sessionId = _connection.SessionID;

                SalesForceService.SoapClient queryClient = new SalesForceService.SoapClient("Soap", apiAddr);

                queryClient.query(header,
                                     options,
                                     null,
                                     null,
                                     query,
                                     out _myresult);
            }
            catch (Exception e)
            {
                _queryinerror = true;
                _errormessage = e.ToString();
            }

            // record not found
            if (_myresult.records != null)
            {
                rval = true;
            }
            return rval;
        }

        #endregion
    }
}
