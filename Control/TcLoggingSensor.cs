using Spea.Archimede.ArchimedeFormatterLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Spea.Archimede.ArchimedeFormatterLibrary.Sensor;

namespace SensorDataLoader100.Control
{
    class TcLoggingSensor
    {

        internal Sensor cpSensor;
        internal List<PhysicalProperty> cpLoggableProperties;
        internal class CurrentProperty{
            public PhysicalProperty cpProperty;
            public UInt64 rpPropertyStartAcquireTime;
        }
        internal CurrentProperty cpCurrent;

        public TcLoggingSensor(Sensor pSensor, List<PhysicalProperty> pPhysicalProperties) {
            this.cpSensor = pSensor;
            this.cpLoggableProperties = new List<PhysicalProperty>(pPhysicalProperties);
            this.cpCurrent = new CurrentProperty();
            cpCurrent.cpProperty = cpLoggableProperties[0];
        }

        public TcLoggingSensor(Sensor pSensor)
        {
            this.cpSensor = pSensor;
            this.cpLoggableProperties = new List<PhysicalProperty>();
            this.cpCurrent = new CurrentProperty();
        }

        public TcLoggingSensor()
        {
            this.cpSensor = new Sensor();
            this.cpLoggableProperties = new List<PhysicalProperty>();
            this.cpCurrent = new CurrentProperty();
        }

        public void fSwitchLoggingProperty() {
            this.cpCurrent.cpProperty = this.cpLoggableProperties[(this.cpLoggableProperties.IndexOf(this.cpCurrent.cpProperty) + 1) % this.cpLoggableProperties.Count];
            this.cpCurrent.rpPropertyStartAcquireTime = 0;
        }

        public void fStartLoggingCurrentProperty() {
            this.cpCurrent.rpPropertyStartAcquireTime = (UInt64)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public bool fIsExpired() {
            return ((ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() >= this.cpCurrent.cpProperty.Log.TimeIntervalAcquire + this.cpCurrent.rpPropertyStartAcquireTime);
        }

    }
}
