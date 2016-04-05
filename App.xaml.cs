using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using esscWPFShell;
using OCRMSupportForce.Dialogs;
using OCRMSupportForce.ViewModels;
using OCRMSupportForce.Models;
using System.Deployment.Application;

namespace OCRMSupportForce
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ApplicationViewModel _appViewModel;
        private ForceWSDLSupport _forceWSDLSupport;
        private mySqlModel _mysqlconnection;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            _mysqlconnection = new mySqlModel();

            _appViewModel = new ApplicationViewModel();

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                _appViewModel.CurrentDataSource = "Production";
                _appViewModel.CurrentVersion = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString(4);
                _appViewModel.DeployMode = "Network Deployment";
            }
            else
            {
                _appViewModel.CurrentDataSource = "Production";
                _appViewModel.CurrentVersion = "N/A";
                _appViewModel.DeployMode = "Development-No deployment";
            }


            if (_appViewModel != null)
            {
                MainWindowViewModel mainWinViewModel = new MainWindowViewModel(_appViewModel);
                _appViewModel.MainWindow = mainWinViewModel;

                // Creation and binding of the Mainwindows bits...
                ApplicationHeaderViewModel appHeadervm = new ApplicationHeaderViewModel(ApplicationHeader.Create("OCRM Salesforce Support", "WSDL API framework 34.0"));
                mainWinViewModel.AppHeader = appHeadervm;
                StatusBarViewModel statusBarvm = new StatusBarViewModel(_appViewModel);
                mainWinViewModel.AppStatusBar = statusBarvm;

                InjectCommands();

                // Create the mainwindow itself
                MainWindow mainWin = new MainWindow();
                mainWin.DataContext = mainWinViewModel;
                mainWin.Show();

                // Create the login model
                _forceWSDLSupport = new ForceWSDLSupport(false);

                LoginDialogViewModel vm = new LoginDialogViewModel(_appViewModel, mainWin, _forceWSDLSupport, _mysqlconnection);
                _appViewModel.MainWindow.InjectWorkSpace(vm);

            }
        }

        private void InjectCommands()
        {
            // ez security, no commands to do anything is not authentic

            // two roles, can say hello and can only exit
            _appViewModel.MainWindow.Model.InjectCommandViewModel(new CommandViewModel("Exit", new RelayCommand(param => this.ExitCommand())));
            _appViewModel.MainWindow.Model.InjectCommandViewModel(new CommandViewModel("Stripe", new RelayCommand(param => this.OpenStripeWorkspace())));
            _appViewModel.MainWindow.Model.InjectCommandViewModel(new CommandViewModel("Campaigns", new RelayCommand(param => this.OpenImportCampaignWorkspace())));
            _appViewModel.MainWindow.Model.InjectCommandViewModel(new CommandViewModel("On Premise", new RelayCommand(param => this.OpenOnPremiseWorkspace())));
            // _appViewModel.MainWindow.Model.InjectCommandViewModel(new CommandViewModel("Donor Details", new RelayCommand(param => this.OpenDetailWorkspace())));
            _appViewModel.MainWindow.Model.InjectCommandViewModel(new CommandViewModel("Donor Report", new RelayCommand(param => this.OpenDonorWorkspace())));
           
        }

        #region Application Level Commands
        
        private void ExitCommand()
        {
           this.Shutdown();
        }

        private void OpenStripeWorkspace()
        {
            // To create a new workspace and call, must have a valid MainWindowViewModel 
            if (_appViewModel.MainWindow != null)
            {
                StripeViewModel vm = new StripeViewModel(_appViewModel, "Stripe Batch");
                _appViewModel.MainWindow.InjectWorkSpace(vm);
            }
        }

        private void OpenDonorWorkspace()
        {
            // To create a new workspace and call, must have a valid MainWindowViewModel 
            if (_appViewModel.MainWindow != null)
            {
                DonorReportViewModel vm = new DonorReportViewModel(_appViewModel, "Donor Report", _forceWSDLSupport);
                _appViewModel.MainWindow.InjectWorkSpace(vm);
            }
        }

        private void OpenOnPremiseWorkspace()
        {
            if (_appViewModel.MainWindow != null)
            {
                OnPremiseViewModel vm = new OnPremiseViewModel(_appViewModel, "Donor Detail", _forceWSDLSupport, _mysqlconnection);
                _appViewModel.MainWindow.InjectWorkSpace(vm);
            }

        }

        private void OpenDetailWorkspace()
        {
            if (_appViewModel.MainWindow != null)
            {
                DonorDetailViewModel vm = new DonorDetailViewModel(_appViewModel, "Donor Detail", _forceWSDLSupport, _mysqlconnection);
                _appViewModel.MainWindow.InjectWorkSpace(vm);
            }
        }

        private void OpenImportCampaignWorkspace()
        {
            if (_appViewModel.MainWindow != null)
            {
                ImportCampaignViewModel vm = new ImportCampaignViewModel(_appViewModel, "Import Campaigns", _forceWSDLSupport);
                _appViewModel.MainWindow.InjectWorkSpace(vm);
            }
        }

 
        #endregion

        #region Dialog Handling

        public void OpenWaitMessage(DonorReportModel model, DonorReportViewModel viewmodel)
        {
            dlgWaitMessage newwin = new dlgWaitMessage(model, viewmodel);
            newwin.ShowDialog();
        }
 
        public void OpenLongWaitMessage(DonorDetailModel model, DonorDetailViewModel viewmodel)
        {
            dlgLongWait newwin = new dlgLongWait(model, viewmodel);
            newwin.ShowDialog();
        }

        #endregion


    }
}
