using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using esscWPFShell;
using OCRMSupportForce.Models;

namespace OCRMSupportForce.ViewModels
{
    public class LoginDialogViewModel : WorkspaceViewModel
    {
        private ApplicationViewModel _appvm;
        private Window _dialog;
        private ForceWSDLSupport _login;

        private mySqlModel sqlconnection;

        private RelayCommand _closecmd;

        public LoginDialogViewModel(ApplicationViewModel Parent, Window dialog, ForceWSDLSupport Login, mySqlModel _sqlcon)
        {
            sqlconnection = _sqlcon;
            base.DisplayName = "Salesforce Login";
            _appvm = Parent;
            _dialog = dialog;

            // commands
            _closecmd = new RelayCommand(param => this._close());

            // Call the login...
            _login = Login;

            _login.LogIn();
        }


        #region Public Properties

        public string LoginErrorMessage
        {
            get { return _login.GetLoginException; }
        }

        public string AsUserContext
        {
            get { return "Requested Access as user: " + _login.AsUserName; }
        }

        public string SuccessfulLogin
        {
            get
            {
                if (_login.LoginSuccess)
                    return "Connected: True";
                else
                    return "Connected: False";
            }
        }

        public string AsSessionID
        {
            get
            {
                if (_login.LoginSuccess)
                    return "Session ID: "+_login.SessionID;
                else
                    return "Session ID: Connection Failed";
            }
        }

        public string AsServerURL
        {
            get
            {
                if (_login.LoginSuccess)
                    return "Server URL: " + _login.SessionID;
                else
                    return "Server URL: Connection Failed";
            }
        }

        // mySql Connection Properties
        public String SqlConnectionStatus
        {
            get { return sqlconnection.ConnectionStatus; }
        }

        public String SqlErrorMessage
        {
            get { return sqlconnection.ErrorMessage; }
        }

        public String SqlUserName
        {
            get { return "Requested access as user Application"; }
        }

        public String SqlServerPort
        {
            get { return "Running on localhost"; }
        }

        public String SqlServerSchema
        {
            get { return "Schema: ocrm"; }
        }

        #endregion

        #region Public Commands
        public RelayCommand CloseLoginSplashWindow
        {
            get { return _closecmd; }
        }

        #endregion

        #region Private Commands
        private void _close()
        {
            base.CloseCommand.Execute(null);
        }


        #endregion


    }
}
