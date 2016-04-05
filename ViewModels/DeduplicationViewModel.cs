using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using OCRMSupportForce.Dialogs;
using esscWPFShell;
using OCRMSupportForce.Models;
using OCRMSupportForce.Views;

namespace OCRMSupportForce.ViewModels
{
    public class DeduplicationViewModel : WorkspaceViewModel
    {
        public DeduplicationViewModel(ApplicationViewModel MainWindowViewModel, string _displayname, mySqlModel connection)
        {
            this.DisplayName = _displayname;

        }

    }
}
