using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace OCRMSupportForce.Models
{
    public class mySqlModel
    {
        #region private properties        

        private MySqlConnection connection;
        private String myConnectionString = "server=localhost;uid=Application;pwd=1800cc~4cyl;database=ocrm;Allow User Variables=True";
            
        private String _connectionstatus;
        private String _errormessage;

        private bool _isconnected = false;
        private bool _inerror = false;

        #endregion

        #region Create
        public mySqlModel()
        {
            try
            {
                connection = new MySqlConnection(myConnectionString);
            }
            catch (Exception e)
            {
                _isconnected = false;
                _connectionstatus = "Connected: False";
                _errormessage = e.ToString();
            }
        }

        #endregion

        #region Public Methods

        #endregion

        #region Public properties

        public bool IsConnected
        {
            get { return _isconnected; }
        }

        public bool InError
        {
            get { return _inerror; }
        }

        public String ErrorMessage
        {
            get { return _errormessage; }
        }

        public String ConnectionStatus
        {
            get { return _connectionstatus; }
        }

        public MySqlConnection MyConnection
        {
            get { return connection; }
        }

        #endregion
    }
}
