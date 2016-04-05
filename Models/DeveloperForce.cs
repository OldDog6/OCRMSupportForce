using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Salesforce.Common;
using Salesforce.Force;
using System.Threading.Tasks;
using System.Dynamic;
using OCRMSupportForce.Properties;

namespace OCRMSupportForce.Models
{
    public class DeveloperForce
    {
        #region private properties

        private bool _loggedin = false;
        private static readonly string _consumerkey = "3MVG98SW_UPr.JFgnoYJbdPJ0.oPX8kGbi3vOq883K4l.ZWRupdG_W16Yw.piVdFIR1dj2aqTWjD3B7j784mn";
        private static readonly string _consumersecret = "3085158263696175826";
        private static readonly string _username = "olddog6@gmail.com";
        private static readonly string _password = "Consult135";
        private static readonly string _token = "sixi79wylyA6FFGovYdY5BBbW";

        private Task<AuthenticationClient> _mylogin;

        #endregion

        #region Create

        public DeveloperForce()
        {
            try
            {
                _mylogin = Authorize();
                _mylogin.Wait();
                Auth = _mylogin.Result;

                Client = new ForceClient(Auth.InstanceUrl, Auth.AccessToken, Auth.ApiVersion);
            }
            catch(Exception e)
            {
                _loggedin = false;
                ErrorMessage = e.Message;
            }
        }

        #endregion

        #region Public Properties

        public AuthenticationClient Auth { get; set; }
        public ForceClient Client { get; set; }

        public String ErrorMessage { get; set; }

        public Campaign ParentCampaign { get; set; }

        #endregion

        #region public Methods

        public async Task GetParentCampaign(String ParentName)
        {
            String qry = String.Format("SELECT ID, Name FROM Campaign where Name = {0}",ParentName);
            var campaigns = new List<Campaign>();
            var results = await Client.QueryAsync<Campaign>(qry);

            campaigns.AddRange(results.Records);

            if (results.TotalSize == 1)
                ParentCampaign = campaigns[0];
            else
                ParentCampaign = null;
   
        }

        #endregion

        #region Private methods
        private static async Task<AuthenticationClient> Authorize()
        {
            AuthenticationClient auth = new AuthenticationClient();
            auth.UsernamePasswordAsync(_consumerkey, _consumersecret, _username, _password + _token);
            return auth;
        }

        #endregion

        private void SpinningWheel()
        {


        }

    }


 
}
