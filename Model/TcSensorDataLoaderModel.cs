using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace SensorDataLoader100
{
    public class TcSensorDataLoaderModel
    {

        public class EnvironmentMeasureRealtime : INotifyPropertyChanged
        {
            private float temperature;
            private float pressure;
            private float humidity;
            private string position;

            public EnvironmentMeasureRealtime(string pPosition, float pTemperature, float pHumidity, float pPressure) {
                this.position = pPosition;
                this.temperature = pTemperature;
                this.humidity = pHumidity;
                this.pressure = pPressure;
            }



            public string Position
            {
                get { return (position); }
                set
                {
                    position = value;
                    this.OnEnvironmentSensorMeasurePropertyChanged(nameof(Position));
                }
            }
            public string Temperature
            {
                get { return (temperature.ToString() + " C"); }
                set
                {
                    temperature = float.Parse(value);
                    this.OnEnvironmentSensorMeasurePropertyChanged(nameof(Temperature));
                }
            }
            public string Pressure
            {
                get { return (pressure.ToString() + " mBar"); }
                set
                {
                    pressure = float.Parse(value);
                    this.OnEnvironmentSensorMeasurePropertyChanged(nameof(Pressure));
                }
            }
            public string Humidity
            {
                get { return (humidity.ToString() + " %"); }
                set
                {
                    this.humidity = float.Parse(value);
                    this.OnEnvironmentSensorMeasurePropertyChanged(nameof(Humidity));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnEnvironmentSensorMeasurePropertyChanged([CallerMemberName] string pPropertyName = null)
            {
                var rmHandler = PropertyChanged;
                rmHandler?.Invoke(this, new PropertyChangedEventArgs(pPropertyName));
            }


        }
        public class KinematicMeasureRealtime : INotifyPropertyChanged
        {
            private float inclination;
            private string position;


            public KinematicMeasureRealtime(string pPosition, float pInclination)
            {
                this.position = pPosition;
                this.inclination = pInclination;
            }



            public string Position
            {
                get { return (position); }
                set
                {
                    this.position = value;
                    this.OnKinematicSensorMeasurePropertyChanged(nameof(Position));
                }
            }
            public string Inclination
            {
                get { return (inclination.ToString() + " degree"); }
                set
                {
                    this.inclination = float.Parse(value);
                    this.OnKinematicSensorMeasurePropertyChanged(nameof(Inclination));
                }
            }
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnKinematicSensorMeasurePropertyChanged([CallerMemberName] string pPropertyName = null)
            {
                var rmHandler = PropertyChanged;
                rmHandler?.Invoke(this, new PropertyChangedEventArgs(pPropertyName));
            }
        }






        public class EnvironmentMeasureStatistics : INotifyPropertyChanged
        {
            private string position;
            private MeasureStatistic temperature;
            private MeasureStatistic humidity;
            private MeasureStatistic pressure;

            public EnvironmentMeasureStatistics(string pPosition)
            {
                this.position = pPosition;
                this.temperature = new MeasureStatistic("Temperature", "C", 0, 0, 0);
                this.humidity = new MeasureStatistic("Humidity", "%", 0, 0, 0);
                this.pressure = new MeasureStatistic("Pressure", "mBar", 0, 0, 0);
            }

            public string Position
            {
                get { return (this.position); }
                set
                {
                    this.position = value;
                    this.OnStatisticsPropertyChanged(nameof(this.position));
                }
            }


            public MeasureStatistic Temperature
            {
                get { return (this.temperature); }
                set
                {
                    this.temperature = value;
                    this.OnStatisticsPropertyChanged(nameof(Temperature));
                }
            }

            public MeasureStatistic Humidity
            {
                get { return (this.humidity); }
                set
                {
                    this.humidity = value;
                    this.OnStatisticsPropertyChanged(nameof(Humidity));
                }
            }


            public MeasureStatistic Pressure
            {
                get { return (this.pressure); }
                set
                {
                    this.pressure = (MeasureStatistic)value;
                    this.OnStatisticsPropertyChanged(nameof(Pressure));
                }
            }

           

            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnStatisticsPropertyChanged([CallerMemberName] string pPropertyName = null)
            {
                var rmHandler = PropertyChanged;
                rmHandler?.Invoke(this, new PropertyChangedEventArgs(pPropertyName));
            }

        }

        public class KinematicMeasureStatistics : INotifyPropertyChanged
        {
            private MeasureStatistic inclination;
            private string position;

            public KinematicMeasureStatistics(string pPosition)
            {
                this.inclination = new MeasureStatistic("Inclination", "degree", 0, 0, 0);
                this.position = pPosition;
            }

            public string Position
            {
                get { return (this.position); }
                set
                {
                    this.position = value;
                    this.OnStatisticsPropertyChanged(nameof(Position));
                }
            }

            public MeasureStatistic Inclination
            {
                get { return (this.inclination); }
                set
                {
                    this.inclination = value;
                    this.OnStatisticsPropertyChanged(nameof(Inclination));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnStatisticsPropertyChanged([CallerMemberName] string pPropertyName = null)
            {
                var rmHandler = PropertyChanged;
                rmHandler?.Invoke(this, new PropertyChangedEventArgs(pPropertyName));
            }
        }

        public class MeasureStatistic : INotifyPropertyChanged
        {
            private string measuretype;
            private string measureunit;
            private float max;
            private float min;
            private float avg;
            private ulong count = 0;
            private float sum = 0;

            public MeasureStatistic(string pMeasureType, string pMeasureUnit, float pMax, float pMin, float pAvg)
            {
                this.measuretype = pMeasureType;
                this.measureunit = pMeasureType;
                this.count = 0;
                this.sum = 0;
                this.max = pMax;
                this.avg = pMin;
                this.min = pAvg;
               
            }


            public void fUpdateMeasureStatistic(float pValue) {
                this.sum += pValue;
                this.count++;
                this.max = (this.max < pValue ? pValue : this.max);
                this.Max = this.max.ToString() + measureunit;
                this.min = (this.min < pValue ? pValue : this.min);
                this.Min = this.min.ToString() + measureunit;
                this.avg = (float) this.sum / this.count;
                this.Avg = this.avg.ToString() + measureunit;
            }

            public string Max
            {
                get { return (max.ToString() + measureunit); }
                set
                {
                    max = float.Parse(value);
                    this.OnStatisticPropertyChanged(nameof(Max));
                }
            }
            public string Min
            {
                get { return (min.ToString() + measureunit); }
                set
                {
                    min = float.Parse(value);
                    this.OnStatisticPropertyChanged(nameof(Min));
                }
            }
            public string Avg
            {
                get { return (avg.ToString() + measureunit); }
                set
                {
                    avg = float.Parse(value);
                    this.OnStatisticPropertyChanged(nameof(Avg));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnStatisticPropertyChanged([CallerMemberName] string pPropertyName = null)
            {
                var rmHandler = PropertyChanged;
                rmHandler?.Invoke(this, new PropertyChangedEventArgs(pPropertyName));
            }

        }






        public class EnvironmentTempMeasureStatistics : INotifyPropertyChanged
        {
            private string temperatureunit; 
            private float temperaturemax;
            private float temperaturemin;
            private float temperatureavg;
            private ulong temperaturecount = 0;
            private float temperaturesum = 0;

            private string humidityunit;
            private float humiditymax;
            private float humiditymin;
            private float humidityavg;
            private ulong humiditycount = 0;
            private float humiditysum = 0;

            private string pressureunit;
            private float pressuremax;
            private float pressuremin;
            private float pressureavg;
            private ulong pressurecount = 0;
            private float pressuresum = 0;

            public EnvironmentTempMeasureStatistics()
            {
                this.temperatureavg = 0;
                this.temperaturemax = 0;
                this.temperaturemin = 1000;
                this.temperaturecount = 0;
                this.temperaturesum = 0;

                this.humidityavg = 0;
                this.humiditymax = 0;
                this.humiditymin = 1000;
                this.humiditycount = 0;
                this.humiditysum = 0;

                this.pressureavg = 0;
                this.pressuremax = 0;
                this.pressuremin = 1000;
                this.pressurecount = 0;
                this.pressuresum = 0;

            }


            public void fUpdateTemperatureStatistic(float pValue)
            {
                this.temperaturesum += pValue;
                this.temperaturecount++;
                this.temperaturemax = (this.temperaturemax < pValue ? pValue : this.temperaturemax);
                this.TemperatureMax = this.temperaturemax.ToString() + temperatureunit;
                this.temperaturemin = (this.temperaturemin < pValue ? pValue : this.temperaturemin);
                this.TemperatureMin = this.temperaturemin.ToString() + temperatureunit;
                this.temperatureavg = (float)this.temperaturesum / this.temperaturecount;
                this.TemperatureAvg = this.temperatureavg.ToString() + temperatureunit;
            }

            public string TemperatureMax
            {
                get { return (temperaturemax.ToString() + temperatureunit); }
                set
                {
                    temperaturemax = float.Parse(value);
                    this.OnStatisticPropertyChanged(nameof(TemperatureMax));
                }
            }
            public string TemperatureMin
            {
                get { return (temperaturemin.ToString() + temperatureunit); }
                set
                {
                    temperaturemin = float.Parse(value);
                    this.OnStatisticPropertyChanged(nameof(TemperatureMin));
                }
            }
            public string TemperatureAvg
            {
                get { return (temperatureavg.ToString() + temperatureunit); }
                set
                {
                    temperatureavg = float.Parse(value);
                    this.OnStatisticPropertyChanged(nameof(TemperatureAvg));
                }
            }

            public void fUpdateHumidityStatistic(float pValue)
            {
                this.temperaturesum += pValue;
                this.humiditycount++;
                this.humiditymax = (this.humiditymax < pValue ? pValue : this.humiditymax);
                this.HumidityMax = this.humiditymax.ToString() + humidityunit;
                this.humiditymin = (this.humiditymin < pValue ? pValue : this.humiditymin);
                this.HumidityMin = this.humiditymin.ToString() + humidityunit;
                this.humidityavg = (float)this.humiditysum / this.humiditycount;
                this.HumidityAvg = this.humidityavg.ToString() + humidityunit;
            }

            public string HumidityMax
            {
                get { return (humiditymax.ToString() + humidityunit); }
                set
                {
                    humiditymax = float.Parse(value);
                    this.OnStatisticPropertyChanged(nameof(HumidityMax));
                }
            }
            public string HumidityMin
            {
                get { return (humiditymin.ToString() + humidityunit); }
                set
                {
                    humiditymin = float.Parse(value);
                    this.OnStatisticPropertyChanged(nameof(HumidityMin));
                }
            }
            public string HumidityAvg
            {
                get { return (humidityavg.ToString() + humidityunit); }
                set
                {
                    humidityavg = float.Parse(value);
                    this.OnStatisticPropertyChanged(nameof(HumidityAvg));
                }
            }


            public void fUpdatePressureStatistic(float pValue)
            {
                this.temperaturesum += pValue;
                this.pressurecount++;
                this.pressuremax = (this.pressuremax < pValue ? pValue : this.pressuremax);
                this.PressureMax = this.pressuremax.ToString() + pressureunit;
                this.pressuremin = (this.pressuremin < pValue ? pValue : this.pressuremin);
                this.PressureMin = this.pressuremin.ToString() + pressureunit;
                this.pressureavg = (float)this.pressuresum / this.pressurecount;
                this.PressureAvg = this.pressureavg.ToString() + pressureunit;
            }

            public string PressureMax
            {
                get { return (pressuremax.ToString() + pressureunit); }
                set
                {
                    pressuremax = float.Parse(value);
                    this.OnStatisticPropertyChanged(nameof(PressureMax));
                }
            }
            public string PressureMin
            {
                get { return (pressuremin.ToString() + pressureunit); }
                set
                {
                    pressuremin = float.Parse(value);
                    this.OnStatisticPropertyChanged(nameof(PressureMin));
                }
            }
            public string PressureAvg
            {
                get { return (pressureavg.ToString() + pressureunit); }
                set
                {
                    pressureavg = float.Parse(value);
                    this.OnStatisticPropertyChanged(nameof(PressureAvg));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnStatisticPropertyChanged([CallerMemberName] string pPropertyName = null)
            {
                var rmHandler = PropertyChanged;
                rmHandler?.Invoke(this, new PropertyChangedEventArgs(pPropertyName));
            }

        }
    }
}
