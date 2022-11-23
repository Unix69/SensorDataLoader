using System;
using System.Collections.ObjectModel;
using System.Windows;
using static SensorDataLoader100.TcSensorDataLoaderModel;


namespace SensorDataLoader100
{



    public partial class TcSensorDataLoaderView : Window
    {
        

        TcSensorDataLoaderControl cmSensorDataLoaderControl;
        ObservableCollection<EnvironmentMeasureRealtime> cmEnvironmentRealtimeObservableMeasureData = null;
        ObservableCollection<KinematicMeasureRealtime> cmKinematicRealtimeObservableMeasureData = null;
        ObservableCollection<EnvironmentMeasureStatistics> cmEnvironmentStatisticsObservableMeasureData = null;
        ObservableCollection<KinematicMeasureStatistics> cmKinematicStatisticsObservableMeasureData = null;

        private enum DataGridMode { Realtime, Statistics };
        private enum DataGridSource { Environment, Kinematic, ElectricalConsumption };

        private DataGridMode cmDatagridMode;
        private DataGridSource cmDatagridSource;

        public void fAttachToProcess(string pProgramName, string pAttachtTitle, string pAttachFilePath, string pAttachKey) {
            if (System.IO.File.Exists(pAttachtTitle))
            {
                string rContent = System.IO.File.ReadAllText(pAttachtTitle);
                if (rContent.Contains(pAttachKey))
                    System.Windows.MessageBox.Show(pAttachFilePath, pProgramName);

            }

        }



        public TcSensorDataLoaderView()
        {

           

            InitializeComponent();

            fAttachToProcess("SensorDataLoader100", "Attach", @"C:\SPEA\Attach.ini", "SensorDataLoader100.exe=1");

            this.MainPanelWindow.Visibility = Visibility.Hidden;

            string[] aArgs = Environment.GetCommandLineArgs();
            if (aArgs.Length > 1 && aArgs[1] == "CONSOLE")
            {
                ConsoleManager.Show();
            }

            this.cmEnvironmentRealtimeObservableMeasureData = new ObservableCollection<EnvironmentMeasureRealtime>();
            this.cmKinematicRealtimeObservableMeasureData = new ObservableCollection<KinematicMeasureRealtime>();
            this.cmEnvironmentStatisticsObservableMeasureData = new ObservableCollection<EnvironmentMeasureStatistics>();
            this.cmKinematicStatisticsObservableMeasureData = new ObservableCollection<KinematicMeasureStatistics>();
            this.SensorMeasureDataGrid.ItemsSource = cmEnvironmentRealtimeObservableMeasureData;


            this.cmDatagridMode = DataGridMode.Realtime;

            this.cmSensorDataLoaderControl = new TcSensorDataLoaderControl();
            this.cmSensorDataLoaderControl.cpEnvironmentRealtimeObservableMeasureData = cmEnvironmentRealtimeObservableMeasureData;
            this.cmSensorDataLoaderControl.cpKinematicRealtimeObservableMeasureData = cmKinematicRealtimeObservableMeasureData;
            this.cmSensorDataLoaderControl.cpEnvironmentStatisticsObservableMeasureData = cmEnvironmentStatisticsObservableMeasureData;
            this.cmSensorDataLoaderControl.cpKinematicStatisticsObservableMeasureData = cmKinematicStatisticsObservableMeasureData;

            this.MainPanelWindow.Visibility = Visibility.Visible;

        }



        public void fSetEnvironmentRealtimeMeasureDataAsDataGridItemsSource()
        {
            if (SensorMeasureDataGrid != null)
            {
                this.SensorMeasureDataGrid.ItemsSource = cmEnvironmentRealtimeObservableMeasureData;
            }
        }

        public void fSetKinematicRealtimeMeasureDataAsDataGridItemsSource()
        {
            if (SensorMeasureDataGrid != null)
            {
                this.SensorMeasureDataGrid.ItemsSource = cmKinematicRealtimeObservableMeasureData;
            }
        }

        public void fSetNewEnvironmentRealtimeMeasureDataCollectionAsDataGridItemsSource(ObservableCollection<EnvironmentMeasureRealtime> pEnvironmentObservableSensorMeasureData)
        {
            if (SensorMeasureDataGrid != null)
            {
                this.cmEnvironmentRealtimeObservableMeasureData = pEnvironmentObservableSensorMeasureData;
                this.SensorMeasureDataGrid.ItemsSource = cmEnvironmentRealtimeObservableMeasureData;
            }
        }


        public void fSetNewKinematicRealtimeMeasureDataCollectionAsDataGridItemsSource(ObservableCollection<KinematicMeasureRealtime> pKinematicObservableSensorMeasureData)
        {
            if (SensorMeasureDataGrid != null)
            {
                this.cmKinematicRealtimeObservableMeasureData = pKinematicObservableSensorMeasureData;
                this.SensorMeasureDataGrid.ItemsSource = cmKinematicRealtimeObservableMeasureData;
            }
        }


        public void fSetEnvironmentStatisticsMeasureDataAsDataGridItemsSource()
        {
            if (SensorMeasureDataGrid != null)
            {
                this.SensorMeasureDataGrid.ItemsSource = cmEnvironmentStatisticsObservableMeasureData;
            }
        }

        public void fSetKinematicStatisticsMeasureDataAsDataGridItemsSource()
        {
            if (SensorMeasureDataGrid != null)
            {
                this.SensorMeasureDataGrid.ItemsSource = cmKinematicStatisticsObservableMeasureData;
            }
        }

        public void fSetNewEnvironmentStatisticsMeasureDataCollectionAsDataGridItemsSource(ObservableCollection<EnvironmentMeasureStatistics> pEnvironmentStatisticObservableMeasureData)
        {
            if (SensorMeasureDataGrid != null)
            {
                this.cmEnvironmentStatisticsObservableMeasureData = pEnvironmentStatisticObservableMeasureData;
                this.SensorMeasureDataGrid.ItemsSource = cmEnvironmentStatisticsObservableMeasureData;
            }
        }


        public void fSetNewKinematicStatisticsMeasureDataCollectionAsDataGridItemsSource(ObservableCollection<KinematicMeasureStatistics> pKinematicStatisticsObservableMeasureData)
        {
            if (SensorMeasureDataGrid != null)
            {
                this.cmKinematicStatisticsObservableMeasureData = pKinematicStatisticsObservableMeasureData;
                this.SensorMeasureDataGrid.ItemsSource = cmKinematicStatisticsObservableMeasureData;
            }
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.StopRadioButton.IsEnabled = true;
            this.StartRadioButton.IsEnabled = false;

            if (this.cmSensorDataLoaderControl.fInit() == 0)
            {

                if (this.cmSensorDataLoaderControl.cpLoaderMode == TcSensorDataLoaderControl.LoaderMode.Logging)
                {
                    this.cmSensorDataLoaderControl.fStartLogging();
                }
                else if (this.cmSensorDataLoaderControl.cpLoaderMode == TcSensorDataLoaderControl.LoaderMode.Monitoring)
                {
                    this.cmSensorDataLoaderControl.fStartMonitoring();
                }
            }
            else
            {
                MessageBox.Show("Sensor Initialization Error","SensorDataLoader.100", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Environments_Click(object sender, RoutedEventArgs e)
        {
            if (this.cmDatagridMode == DataGridMode.Realtime)
            {
                fSetEnvironmentRealtimeMeasureDataAsDataGridItemsSource();
            }
            else if (this.cmDatagridMode == DataGridMode.Statistics)
            {
                fSetEnvironmentStatisticsMeasureDataAsDataGridItemsSource();
            }
        }

        private void Kinematics_Click(object sender, RoutedEventArgs e)
        {
            if (this.cmDatagridMode == DataGridMode.Realtime)
            {
                fSetKinematicRealtimeMeasureDataAsDataGridItemsSource();
            }
            else if (this.cmDatagridMode == DataGridMode.Statistics)
            {
                fSetKinematicStatisticsMeasureDataAsDataGridItemsSource();
            }
        }

        private void ElectricalConsumption_Click(object sender, RoutedEventArgs e)
        {
            //Powermeter start
            if (this.cmDatagridMode == DataGridMode.Realtime)
            {
                //fSetElectricalConsumptionRealtimeMeasureDataAsDataGridItemsSource();
            }
            else if (this.cmDatagridMode == DataGridMode.Statistics)
            {
                //fSetElectricalConsumptionStatisticsMeasureDataAsDataGridItemsSource();
            }
        }

        private void Realtime_Click(object sender, RoutedEventArgs e)
        {

            if (this.cmDatagridMode != DataGridMode.Realtime)
            {
                this.cmDatagridMode = DataGridMode.Realtime;
                if (this.cmDatagridSource == DataGridSource.Environment)
                {
                    fSetEnvironmentRealtimeMeasureDataAsDataGridItemsSource();
                }
                else if (this.cmDatagridSource == DataGridSource.Kinematic)
                {
                    fSetKinematicRealtimeMeasureDataAsDataGridItemsSource();
                }
                else if (this.cmDatagridSource == DataGridSource.ElectricalConsumption)
                {
                    //fSetElectricalConsumptionRealtimeMeasureDataAsDataGridItemsSource();
                }
            }
        }

        private void Statistics_Click(object sender, RoutedEventArgs e)
        {
            if (this.cmDatagridMode != DataGridMode.Statistics)
            {
                this.cmDatagridMode = DataGridMode.Statistics;
                if (this.cmDatagridSource == DataGridSource.Environment)
                {
                    fSetEnvironmentStatisticsMeasureDataAsDataGridItemsSource();
                }
                else if (this.cmDatagridSource == DataGridSource.Kinematic)
                {
                    fSetKinematicStatisticsMeasureDataAsDataGridItemsSource();
                }
                else if (this.cmDatagridSource == DataGridSource.ElectricalConsumption)
                {
                    //fSetElectricalConsumptionStatisticsMeasureDataAsDataGridItemsSource();
                }
            }
        }



        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (StartRadioButton.IsEnabled) {
                this.StopRadioButton.IsEnabled = true;
                this.StartRadioButton.IsEnabled = false;
                this.cmSensorDataLoaderControl.fInit();
                if (this.cmSensorDataLoaderControl.cpLoaderMode == TcSensorDataLoaderControl.LoaderMode.Logging) {
                    this.cmSensorDataLoaderControl.fStartLogging();
                } else if (this.cmSensorDataLoaderControl.cpLoaderMode == TcSensorDataLoaderControl.LoaderMode.Monitoring) { 
                    this.cmSensorDataLoaderControl.fStartMonitoring();
                }
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            //Close control comunication
            this.cmSensorDataLoaderControl.fEndComunication();
            this.StartRadioButton.IsEnabled = true;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            //Hide and Close Window
            this.MainPanelWindow.Hide();
            this.MainPanelWindow.IsEnabled = false;
            this.MainPanelWindow.Close();

            //Close control comunication
            this.cmSensorDataLoaderControl.fEndComunication();

            //Exit from windows application
            System.Windows.Application.Current.Shutdown();

                        
        }



       




    }
}
