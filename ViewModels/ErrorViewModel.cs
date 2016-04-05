using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OCRMSupportForce.Models;
using esscWPFShell;


namespace OCRMSupportForce.ViewModels
{
    public class ErrorViewModel : WorkspaceViewModel
    {
        private ApplicationViewModel _appvm;
        private ForceWSDLSupport _login;

        private String _message;
        private RelayCommand _closecmd;


        public ErrorViewModel(ApplicationViewModel Parent, ForceWSDLSupport Login, String ErrorMessage)
        {
            base.DisplayName = "Salesforce Support Error Message";
            _appvm = Parent;
            _login = Login;
            
            // commands
            _closecmd = new RelayCommand(param => this._close());

            _message = ErrorMessage;
        }

        public ErrorViewModel(ApplicationViewModel Parent, String ErrorMessage)
        {
            base.DisplayName = "Salesforce Support Error Message";
            _appvm = Parent;
            _closecmd = new RelayCommand(param => this._close());

            _message = ErrorMessage;
        }

        #region Public Properties

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
                    return "Session ID: " + _login.SessionID;
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

        public String ErrorMessage
        {
            get { return _message; }
        }

        #endregion

        #region Public Commands
        public RelayCommand CloseErrorWindow
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
