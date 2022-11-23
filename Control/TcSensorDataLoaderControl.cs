using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;
using System.Threading;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Text;
using static SensorDataLoader100.TcSM100Manager;
using MongoDB.Bson;
using System.Collections.ObjectModel;
using static SensorDataLoader100.TcSensorDataLoaderModel;
using static Spea.Archimede.ArchimedeFormatterLibrary.Sensor;
using Spea.Archimede.ArchimedeFormatterLibrary;
using SensorDataLoader100.Control;
using SensorDataLoader100.Util;
using static SensorDataLoader100.TcSM100Manager.TcSM100;
using static SensorDataLoader100.TcSM100Manager.TcSM100Commands;

namespace SensorDataLoader100
{
    public static class TcSM100Manager {

        public static class TcSM100Commands{
            public static List<TcCommand> cpGeneralCommands;
            public static Dictionary<string, List<TcCommand>> cpCommands;
            public static List<TcCommand> cpLogCommands;

            static TcSM100Commands() {
                cpGeneralCommands = new List<TcCommand>();
                cpGeneralCommands.Add(new TcCommand("AcquireStop", (int)SubCommand.scmdAcquireStop));
                cpGeneralCommands.Add(new TcCommand("AcquireStart", (int)SubCommand.scmdAcquireStart));
                cpGeneralCommands.Add(new TcCommand("WriteId", (int)SubCommand.scmdWriteId));
                cpGeneralCommands.Add(new TcCommand("ReadId", (int)SubCommand.scmdReadId));


                cpLogCommands = new List<TcCommand>();
                cpLogCommands.Add(new TcCommand("Setup", (int)Command.cmdLogSetup));
                cpLogCommands.Add(new TcCommand("Read", (int)Command.cmdLogRead));
                cpLogCommands.Add(new TcCommand("Enable", (int)SubCommand.scmdLogEnable));
                cpLogCommands.Add(new TcCommand("Disable", (int)SubCommand.scmdLogDisable));
                cpLogCommands.Add(new TcCommand("ReadNSamples", (int)SubCommand.scmdLogSampleNum));
                cpLogCommands.Add(new TcCommand("ReadSampleN", (int)SubCommand.scmdLogReadSampleNum));
              

                Command cmdAcquireCommand = Command.cmdTempAquireStartStop;
                Command cmdReadCommand = Command.cmdTempRead;
                Command cmdSetupCommand = Command.cmdTempMeterSetup;
                Command cmdTimeIntervalAcquireCommand = Command.cmdTempTimeIntervalAcquire;
                int rUniqueCommandsOffset = 10;

                cpCommands = new Dictionary<string, List<TcCommand>>();
                foreach (PhysicalMeasure cPhysicalMeasure in cmPhysicalMeasures)
                {
                    string rMeasureType = cPhysicalMeasure.MeasureType;
                    List<TcCommand> cCommands = new List<TcCommand>();
                    cCommands.Add(new TcCommand("Acquire", (int)cmdAcquireCommand));
                    cCommands.Add(new TcCommand("Read", (int)cmdReadCommand));
                    cCommands.Add(new TcCommand("Setup", (int)cmdSetupCommand));
                    cCommands.Add(new TcCommand("TimeIntervalAcquire", (int)cmdTimeIntervalAcquireCommand));
                    cmdAcquireCommand += rUniqueCommandsOffset;
                    cmdReadCommand += rUniqueCommandsOffset;
                    cmdSetupCommand += rUniqueCommandsOffset;
                    cmdTimeIntervalAcquireCommand += rUniqueCommandsOffset;
                    cpCommands.Add(rMeasureType, cCommands);
                }
            }

            public class TcCommand {

                public const string kGeneralAcquireStop = "AcquireStop";
                public const string kGeneralAcquireStart = "AcquireStart";
                public const string kGeneralWriteId = "WriteId";
                public const string kGeneralReadId = "ReadId";

                public const string kLogSetup = "Setup";
                public const string kLogRead = "Read";
                public const string kLogEnable = "Enable";
                public const string kLogDisable = "Disable";
                public const string kReadNSamples = "ReadNSamples";
                public const string kReadSampleN = "ReadSampleN";

                public const string kAcquire = "Acquire";
                public const string kRead = "Read";
                public const string kSetup = "Setup";
                public const string kTimeAcquire = "TimeIntervalAcquire";


                public string Text;
                public int Value;
                public TcCommand(string pText, int pValue) {
                    this.Text = pText;
                    this.Value = pValue;
                }
            }


            public static TcCommand fGetCommand(string pMeasureType, string pCommandDescription) {
                return (cpCommands[pMeasureType].Find((cCommand)=> { return (cCommand.Text == pCommandDescription); }));
            }

            public static TcCommand fGetLogCommand(string pCommandDescription)
            {
                return (cpLogCommands.Find((cCommand) => { return (cCommand.Text == pCommandDescription); }));
            }

            public static TcCommand fGetGeneralCommand(string pCommandDescription)
            {
                return (cpGeneralCommands.Find((cCommand) => { return (cCommand.Text == pCommandDescription); }));
            }

            public static string fGetMeasureTypeByCommand(int pCommand)
            {

                string rResolvedMeasureType = "";
                cpCommands.First(
                    (KeyValuePair<string, List<TcCommand>> cPair) =>
                    {
                        if (cPair.Value.Find((cCommand)=> { return (cCommand.Value == pCommand); }) != null) {
                            rResolvedMeasureType = cPair.Key;
                            return (true);
                        }
                        return (false);
                    }
                    
                );

                return (rResolvedMeasureType);
            }



        }



        public static class TcSM100
        {
            public static List<PhysicalMeasure> cmPhysicalMeasures;


            static TcSM100() {
                cmPhysicalMeasures = new List<PhysicalMeasure>();
                fSetupComposition();
            }

            public class Temperature
            {
                public static uint ODRScale = 1000000;
                public static uint NDimensions = 1;
                public static uint LogNumber = 0;
                public const string MeasureType = "Temperature";
                public static string MeasureUnit = "C";
                public static string[] Dimensions = { MeasureType };
                public static UInt64[] OutputDataRate;

                public static void fSetupOuputDataRate() { 
                    int i = 0;
                    int rScale = 1000000;
                    OutputDataRate = new UInt64[3];
                    OutputDataRate[i++] = (ulong)rScale;
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)7));
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)12.5));
                }
                


            }

            public class Humidity
            {
                public static uint NDimensions = 1;
                public static uint LogNumber = 1;
                public const string MeasureType = "Humidity";
                public static string MeasureUnit = "%";
                public static string[] Dimensions = { MeasureType };
                public static UInt64[] OutputDataRate;

                public static void fSetupOuputDataRate()
                {
                    int i = 0;
                    int rScale = 1000000;
                    OutputDataRate = new UInt64[3];
                    OutputDataRate[i++] = (ulong)rScale;
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)7));
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)12.5));

                }

            }

            public class Pressure
            {
                public static uint NDimensions = 1;
                public static uint LogNumber = 5;
                public const string  MeasureType = "Pressure";
                public static string MeasureUnit = "mBar";
                public static string[] Dimensions = { MeasureType };
                public static UInt64[] OutputDataRate;

            public static void fSetupOuputDataRate()
            {
                    int i = 0;
                    int rScale = 1000000;
                    OutputDataRate = new UInt64[5];
                    OutputDataRate[i++] = (ulong)rScale;
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)10));
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)20));
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)50));
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)75));

                }
            }

            public class Audio
            {
                public static uint NDimensions = 1;
                public static uint LogNumber = 6;
                public const string MeasureType = "Audio";
                public static string MeasureUnit = "dB";
                public static string[] Dimensions = { MeasureType };
                public static UInt64[] OutputDataRate;

                public static void fSetupOuputDataRate()
                {
                    int i = 0;
                    int rScale = 1000000;
                    OutputDataRate = new UInt64[3];
                    OutputDataRate[i++] = (ulong)rScale;
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)7));
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)12.5));

                }
            }

            public class Light
            {
                public static uint NDimensions = 1;
                public static uint LogNumber = 7;
                public const string MeasureType = "Light";
                public static string MeasureUnit = "lm";
                public static string[] Dimensions = { MeasureType };
                public static UInt64[] OutputDataRate;

                public static void fSetupOuputDataRate()
                {
                    int i = 0;
                    int rScale = 1000000;
                    OutputDataRate = new UInt64[3];
                    OutputDataRate[i++] = (ulong)rScale;
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)7));
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)12.5));

                }
            }


            public class Acceleration
            {
                public static uint NDimensions = 3;
                public static uint LogNumber = 2;
                public const string  MeasureType = "Acceleration";
                public static string MeasureUnit = "mg";
                public static string[] Dimensions = { MeasureType + "_x", MeasureType + "_y", MeasureType + "_z" };
                public static UInt64[] OutputDataRate;

                public static void fSetupOuputDataRate()
                {
                    int i = 0;
                    int rScale = 1000000;
                    OutputDataRate = new UInt64[10];
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)12.5));
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)26));
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)52));
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)104));
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)216));
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)432));
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)833));
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)1666));
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)3332));
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)6664));
                }
            }

            public class MagneticField
            {
                public static uint NDimensions = 3;
                public static uint LogNumber = 4;
                public const string MeasureType = "MagneticField";
                public static string MeasureUnit = "mG";
                public static string[] Dimensions = { MeasureType + "_x", MeasureType + "_y", MeasureType + "_z" };
                public static UInt64[] OutputDataRate;

                public static void fSetupOuputDataRate()
                {
                    int i = 0;
                    int rScale = 1000000;
                    OutputDataRate = new UInt64[4];
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)10));
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)20));
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)50));
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)100));

                }
            }

            public class Gyroscope
            {
                public static uint NDimensions = 3;
                public static uint LogNumber = 6;
                public const string MeasureType = "Gyroscope";
                public static string MeasureUnit = "mdps";
                public static string[] Dimensions = { MeasureType + "_x", MeasureType + "_y", MeasureType + "_z" };
                public static UInt64[] OutputDataRate;

                public static void fSetupOuputDataRate()
                {
                    int i = 0;
                    int rScale = 1000000;
                    OutputDataRate = new UInt64[10];
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)12.5));
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)26));
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)52));
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)104));
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)216));
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)432));
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)833));
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)1666));
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)3332));
                    OutputDataRate[i++] = (ulong)((double)rScale * (double)((double)1 / (double)6664));

                }
            }

            public class PhysicalMeasure
            {
                public uint NDimensions = 0;
                public uint LogNumber = 0;
                public string MeasureType = "";
                public string MeasureUnit = "";
                public string[] Dimensions = { };
                public UInt64[] OutputDataRate;
                public PhysicalMeasure(uint pNDimensions, uint pLogNumber, string pMeasureType, string pMeasureUnit, string[] pDimensions, UInt64 [] pOutputDataRate)
                {
                    this.NDimensions = pNDimensions;
                    this.LogNumber = pLogNumber;
                    this.MeasureType = pMeasureType;
                    this.MeasureUnit = pMeasureUnit;
                    this.Dimensions = pDimensions;
                    this.OutputDataRate = pOutputDataRate;
                }
            }

            public static PhysicalMeasure fGetPhysicalMeasureByMeasureType(string pMeasureType) {
                return(cmPhysicalMeasures.Find(
                        (cPhysicalMeasure) => {
                            return(cPhysicalMeasure.MeasureType == pMeasureType);
                        }
                    )
                );
            }

            static void fSetupComposition()
            {


                Temperature.fSetupOuputDataRate();
                Humidity.fSetupOuputDataRate();
                Pressure.fSetupOuputDataRate();
                Audio.fSetupOuputDataRate();
                Light.fSetupOuputDataRate();
                Acceleration.fSetupOuputDataRate();
                MagneticField.fSetupOuputDataRate();
                Gyroscope.fSetupOuputDataRate();

                cmPhysicalMeasures.Add(new PhysicalMeasure(Temperature.NDimensions, Temperature.LogNumber,Temperature.MeasureType, Temperature.MeasureUnit, Temperature.Dimensions, Temperature.OutputDataRate));
                cmPhysicalMeasures.Add(new PhysicalMeasure(Humidity.NDimensions, Humidity.LogNumber,  Humidity.MeasureType, Humidity.MeasureUnit, Humidity.Dimensions, Humidity.OutputDataRate));
                cmPhysicalMeasures.Add(new PhysicalMeasure(Pressure.NDimensions, Pressure.LogNumber, Pressure.MeasureType, Pressure.MeasureUnit, Pressure.Dimensions, Pressure.OutputDataRate));
                cmPhysicalMeasures.Add(new PhysicalMeasure(Audio.NDimensions, Audio.LogNumber, Audio.MeasureType, Audio.MeasureUnit, Audio.Dimensions, Audio.OutputDataRate));
                cmPhysicalMeasures.Add(new PhysicalMeasure(Light.NDimensions, Light.LogNumber, Light.MeasureType, Light.MeasureUnit, Light.Dimensions, Light.OutputDataRate));
                cmPhysicalMeasures.Add(new PhysicalMeasure(Acceleration.NDimensions, Acceleration.LogNumber, Acceleration.MeasureType, Acceleration.MeasureUnit, Acceleration.Dimensions, Acceleration.OutputDataRate));
                cmPhysicalMeasures.Add(new PhysicalMeasure(MagneticField.NDimensions, MagneticField.LogNumber, MagneticField.MeasureType, MagneticField.MeasureUnit, MagneticField.Dimensions, MagneticField.OutputDataRate));
                cmPhysicalMeasures.Add(new PhysicalMeasure(Gyroscope.NDimensions,Gyroscope.LogNumber,  Gyroscope.MeasureType, Gyroscope.MeasureUnit, Gyroscope.Dimensions, Gyroscope.OutputDataRate));
                return;




            }
        }

        #region Commands, Subcommands and Sensor Setup Parameters
        public enum Command
        {
            cmdReadWriteId = 0x02,
            cmdTempMeterSetup = 0x10,
            cmdTempAquireStartStop = 0x11,
            cmdTempTimeIntervalAcquire = 0x12,
            cmdTempRead = 0x13,
            cmdHumMeterSetup = 0x20,
            cmdHumAcquireStartStop = 0x21,
            cmdHumTimeIntervalAcquire = 0x22,
            cmdHumRead = 0x23,
            cmdPressMeterSetup = 0x30,
            cmdPressAquireStartStop = 0x31,
            cmdPressTimeIntervalAcquire = 0x32,
            cmdPressRead = 0x33,
            cmdPressMeterFilterSelect = 0x35,
            cmdLuxMeterSetup = 0x40,
            cmdLuxAquireStartStop = 0x41,
            cmdLuxTimeIntervalAcquire = 0x42,
            cmdLuxRead = 0x43,
            cmdMagMeterSetup = 0x50,
            cmdMagAquireStartStop = 0x51,
            cmdMagTimeIntervalAcquire = 0x52,
            cmdMagRead = 0x53,
            cmdAccMeterSetup = 0x60,
            cmdAccAquireStartStop = 0x61,
            cmdAccTimeIntervalAcquire = 0x62,
            cmdAccRead = 0x63,
            cmdGyroMeterSetup = 0x70,
            cmdGyroAquireStartStop = 0x71,
            cmdGyroTimeIntervalAcquire = 0x72,
            cmdGyroRead = 0x73,
            cmdAudioMeterSetup = 0x80,
            cmdAudioAquireStartStop = 0x81,
            cmdAudioTimeIntervalAcquire = 0x82,
            cmdAudioRead = 0x83,
            cmdLogSetup = 0x90,
            cmdLogReadSampleNumber = 0x91,
            cmdLogRead = 0x91,
        }
        public enum SubCommand
        {
            scmdWriteId = 0x00,
            scmdReadId = 0x01,
            scmdSampleRead = 0x00,
            scmdAcquireStart = 0x01,
            scmdAcquireStop = 0x00,
            scmdAlarmEnable = 0x01,
            scmdAlarmDisable = 0x00,
            scmdLPFEnable = 0x00,
            scmdLPFDisable = 0x01,
            scmdLogEnable = 0x01,
            scmdLogDisable = 0x00,
            scmdLogSampleNum = 0x00,
            scmdLogReadSampleNum = 0x01
        }
        public enum Error
        {
            Ack = 0x00,
            Fail = 0xff
        }
        public enum OutputDataRate
        {
            odrHumTemp1 = 0x00,
            odrHumTemp7 = 0x01,
            odrHumTemp125 = 0x02,
            odrPress1 = 0x00,
            odrPress10 = 0x01,
            odrPress20 = 0x02,
            odrPress50 = 0x03,
            odrPress75 = 0x04,
            odrMag10 = 0x00,
            odrMag20 = 0x01,
            odrMag50 = 0x02,
            odrMag100 = 0x03,
            odrAccGyro16 = 0x00,
            odrAccGyro125 = 0x01,
            odrAccGyro26 = 0x02,
            odrAccGyro52 = 0x03,
            odrAccGyro104 = 0x04,
            odrAccGyro208 = 0x05,
            odrAccGyro416 = 0x06,
            odrAccGyro833 = 0x07,
            odrAccGyro166 = 0x08,
            odrAccGyro333 = 0x09,
            odrAccGyro666 = 0x0A,
            odrAudio8000 = 0x00,
            odrAudio16000 = 0x01,
            odrAudio24000 = 0x02,
            odrAudio48000 = 0x03
        }
        public enum AverageNumber
        {
            avTempHum2 = 0x00,
            avTempHum4 = 0x01,
            avTempHum8 = 0x02,
            avTempHum16 = 0x03,
            avTempHum32 = 0x04,
            avTempHum64 = 0x05,
            avTempHum128 = 0x06,
            avTempHum256 = 0x07,
        }
        public enum Bandwith
        {
            band2 = 0x00,
            band9 = 0x02,
            band20 = 0x03
        }
        public enum Again
        {
            agAudio1 = 0x00,
            agAudio8 = 0x01,
            agAudio16 = 0x02,
            agAudio120 = 0x03,
            agAudio1600 = 0x04
        }
        public enum LPF
        {
            enable = 0x00,
            disable = 0x01
        }
        public enum Scale
        {
            scAcc2 = 0x00,
            scAcc4 = 0x02,
            scAcc8 = 0x03,
            scAcc16 = 0x01,
            scAcc125 = 0x00,
            scGyro250 = 0x01,
            scGyro500 = 0x02,
            scGyro1000 = 0x03,
            scGyro2000 = 0x04
        }
        public enum LogSensor
        {
            Temperature = 0x00,
            Humidity = 0x01,
            Acceleration = 0x02,
            Gyroscope = 0x03,
            MagneticField = 0x04,
            Pressure = 0x05,
            Audio = 0x06,
            Light = 0x07
        }
        public enum LogEnable
        {
            disable = 0x00,
            enable = 0x01
        }
        #endregion

        #region Costants
        public static string kSensorModel = "SM100";
        public static int kCanBufferInLength = 128;
        public static int kSensorReadReadFail = 0;
        #endregion

        #region Utils
        private static void fConsoleWrite(string msg) {
            if (msg != "")
            {
                msg = DateTime.Now.ToLongTimeString() + msg;

            }
            Console.WriteLine(msg);
            return;
        }
        #endregion

        #region Write Read Sensor Id
        public static int fSensorReadId(Command pReadCommand, SubCommand pSubcommand, out char[] pId)
        {

            byte[] aBufferOut = new byte[2];
            byte[] aBufferIn = new byte[1024];
            int rReturnStatus = 0;
            Command rCommand;
            Error rSetupStatus = Error.Fail;

            aBufferOut[0] = (byte)pReadCommand;
            aBufferOut[1] = (byte)pSubcommand;

            bool success = true;
            if (!success)
            {
                pId = new char[0];
                rReturnStatus = -1;
                fConsoleWrite("[fSensorSetup] Write and Read fails with error " + " on CanId ");
                return (rReturnStatus);
            }
            pId = new char[0];

            return (0);

            if (aBufferIn.Length != (sizeof(byte) * 4))
            {
                rReturnStatus = -1;
                pId = new char[0];
                fConsoleWrite("[fSensorSetup] Can message to small on CanId ");
                return (rReturnStatus);
            }

            rCommand = (Command)aBufferIn[0];
            rSetupStatus = (Error)aBufferIn[1];

            if (rCommand != pReadCommand)
            {
                pId = new char[0];
                rReturnStatus = -1;
                return (rReturnStatus);
            }

            if (rSetupStatus == Error.Fail)
            {
                pId = new char[0];
                rReturnStatus = -1;
                return (rReturnStatus);
            }

            pId = new char[2];
            pId[0] = BitConverter.ToChar(aBufferIn, 2);
            pId[1] = BitConverter.ToChar(aBufferIn, 3);
            rReturnStatus = 0;
            return (rReturnStatus);
        }
        public static int fSensorWriteId(Command pWriteCommand, SubCommand pSubcommand, char[] pId)
        {

            byte[] aBufferOut = new byte[4];
            byte[] aBufferIn = new byte[1024];
            int rReturnStatus = 0;
            Command rCommand;
            Error rSetupStatus = Error.Fail;


            if (pId.Length != 2)
            {
                rReturnStatus = -1;
                fConsoleWrite("[fSensorWriteId] Write and Read fails with error " + " on CanId ");
                return (rReturnStatus);
            }

            aBufferOut[0] = (byte)pWriteCommand;
            aBufferOut[1] = (byte)pSubcommand;
            aBufferOut[2] = (byte)pId[0];
            aBufferOut[3] = (byte)pId[1];


            bool success = true;
            if (!success)
            {
                rReturnStatus = -1;
                fConsoleWrite("[fSensorWriteId] Write and Read fails with error " + " on CanId ");
                return (rReturnStatus);
            }

            if (aBufferIn.Length != (sizeof(byte) * 4))
            {
                rReturnStatus = -1;
                fConsoleWrite("[fSensorWriteId] Can message to small on CanId ");
                return (rReturnStatus);
            }

            rCommand = (Command)aBufferIn[0];
            rSetupStatus = (Error)aBufferIn[1];

            if (rCommand != pWriteCommand)
            {
                rReturnStatus = -1;
                return (rReturnStatus);
            }

            if (rSetupStatus == Error.Fail)
            {
                rReturnStatus = -1;
                return (rReturnStatus);
            }
            else
            {
                rReturnStatus = 0;
                return (rReturnStatus);
            }
        }
        #endregion

        #region Sensor Setup Functions
        //Setup Humidity/Temperature Meter
        public static int fSensorSetup(int pCanId, Command pSetupCommand, OutputDataRate pOutputDataRate, AverageNumber pAverageNum)
        {

            byte[] aBufferOut = new byte[3];
            byte[] aBufferIn = new byte[128];
            int rBufferInLength = 0;
            int rReturnStatus = 0;
            Command rCommand;
            Error rSetupStatus = Error.Fail;

            aBufferOut[0] = (byte)pSetupCommand;
            aBufferOut[1] = (byte)pOutputDataRate;
            aBufferOut[2] = (byte)pAverageNum;


            int rErrorCode = TcPCXINTComunicationWrapper.fPcxintCanTxRxCommand(pCanId, aBufferOut.Length, aBufferOut, aBufferIn.Length, ref aBufferIn, ref rBufferInLength, 1000);
            if (rErrorCode < 0)
            {
                rReturnStatus = -1;
                fConsoleWrite("[fSensorSetup] Write and Read fails with error " + rErrorCode + " on CanId " + pCanId);
                return (rReturnStatus);
            }

            if (rBufferInLength < (sizeof(byte) * 2))
            {
                rReturnStatus = -1;
                fConsoleWrite("[fSensorSetup] Can message to small on CanId " + pCanId);
                return (rReturnStatus);
            }

            rCommand = (Command)aBufferIn[0];
            rSetupStatus = (Error)aBufferIn[1];

            if (rCommand != pSetupCommand)
            {
                rReturnStatus = -1;
                return (rReturnStatus);
            }

            if (rSetupStatus == Error.Fail)
            {
                rReturnStatus = -1;
                return (rReturnStatus);
            }
            else
            {
                rReturnStatus = 0;
                return (rReturnStatus);
            }


        }


        //Setup Pressure Meter
        public static int fSensorSetup(int pCanId, Command pSetupCommand, OutputDataRate pOutputDataRate, Bandwith pBandwith)
        {

            byte[] aBufferOut = new byte[3];
            byte[] aBufferIn = new byte[64];
            int rBufferInLength = 0;
            int rReturnStatus = 0;
            Command rCommand;
            Error rSetupStatus = Error.Fail;

            aBufferOut[0] = (byte)pSetupCommand;
            aBufferOut[1] = (byte)pOutputDataRate;
            aBufferOut[2] = (byte)pBandwith;


            int rErrorCode = TcPCXINTComunicationWrapper.fPcxintCanTxRxCommand(pCanId, aBufferOut.Length, aBufferOut, aBufferIn.Length, ref aBufferIn, ref rBufferInLength, 1000);
            if (rErrorCode < 0)
            {
                rReturnStatus = -1;
                fConsoleWrite("[fSensorSetup] Write and Read fails with error " + rErrorCode + " on CanId " + pCanId);
                return (rReturnStatus);
            }


            if (rBufferInLength < (sizeof(byte) * 2))
            {
                rReturnStatus = -1;
                fConsoleWrite("[fSensorSetup] Can message to small on CanId " + pCanId);
                return (rReturnStatus);
            }


            rCommand = (Command)aBufferIn[0];
            rSetupStatus = (Error)aBufferIn[1];

            if (rCommand != pSetupCommand)
            {
                rReturnStatus = -1;
                return (rReturnStatus);
            }

            if (rSetupStatus == Error.Fail)
            {
                rReturnStatus = -1;
                return (rReturnStatus);
            }
            else
            {
                rReturnStatus = 0;
                return (rReturnStatus);
            }


        }

        /*
        //Setup Lux Meter
        public int fMeterSetup(ComunicInstance pCommunication, Command pSetupCommand, OutputDataRate pOutputDataRate, int pAGain)
        {
            return (0);
        }
        */


        //Setup Magneto Meter
        public static int fSensorSetup(int pCanId, Command pSetupCommand, OutputDataRate pOutputDataRate, LPF pLPFEnable)
        {

            byte[] aBufferOut = new byte[3];
            byte[] aBufferIn = new byte[1024];
            int rReturnStatus = 0;
            int rBufferInLength = 0;
            Command rCommand;
            Error rSetupStatus = Error.Fail;

            aBufferOut[0] = (byte)pSetupCommand;
            aBufferOut[1] = (byte)pOutputDataRate;
            aBufferOut[2] = (byte)pLPFEnable;


            int rErrorCode = TcPCXINTComunicationWrapper.fPcxintCanTxRxCommand(pCanId, aBufferOut.Length, aBufferOut, aBufferIn.Length, ref aBufferIn, ref rBufferInLength, 1000);
            if (rErrorCode < 0)
            {
                rReturnStatus = -1;
                fConsoleWrite("[fSensorSetup] Write and Read fails with error " + rErrorCode + " on CanId " + pCanId);
                return (rReturnStatus);
            }

            if (rBufferInLength < (sizeof(byte) * 2))
            {
                rReturnStatus = -1;
                fConsoleWrite("[fSensorSetup] Can message to small on CanId " + pCanId);
                return (rReturnStatus);
            }


            rCommand = (Command)aBufferIn[0];
            rSetupStatus = (Error)aBufferIn[1];

            if (rCommand != pSetupCommand)
            {
                rReturnStatus = -1;
                return (rReturnStatus);
            }

            if (rSetupStatus == Error.Fail)
            {
                rReturnStatus = -1;
                return (rReturnStatus);
            }
            else
            {
                rReturnStatus = 0;
                return (rReturnStatus);
            }


        }


        //Setup Acceleration/Gyro Meter
        public static int fSensorSetup(int pCanId, Command pSetupCommand, OutputDataRate pOutputDataRate, Scale pFullScale)
        {

            byte[] aBufferOut = new byte[3];
            byte[] aBufferIn = new byte[64];
            int rBufferInLength = 0;
            int rReturnStatus = 0;
            Command rCommand;
            Error rSetupStatus = Error.Fail;

            aBufferOut[0] = (byte)pSetupCommand;
            aBufferOut[1] = (byte)pOutputDataRate;
            aBufferOut[2] = (byte)pFullScale;


            int rErrorCode = TcPCXINTComunicationWrapper.fPcxintCanTxRxCommand(pCanId, aBufferOut.Length, aBufferOut, aBufferIn.Length, ref aBufferIn, ref rBufferInLength, 1000);
            if (rErrorCode < 0)
            {
                rReturnStatus = -1;
                fConsoleWrite("[fSensorSetup] Write and Read fails with error " + rErrorCode + " on CanId " + pCanId);
                return (rReturnStatus);
            }


            if (rBufferInLength < (sizeof(byte) * 2))
            {
                rReturnStatus = -1;
                fConsoleWrite("[fSensorSetup] Can message to small on CanId " + pCanId);
                return (rReturnStatus);
            }

            rCommand = (Command)aBufferIn[0];
            rSetupStatus = (Error)aBufferIn[1];

            if (rCommand != pSetupCommand)
            {
                rReturnStatus = -1;
                return (rReturnStatus);
            }

            if (rSetupStatus == Error.Fail)
            {
                rReturnStatus = -1;
                return (rReturnStatus);
            }
            else
            {
                rReturnStatus = 0;
                return (rReturnStatus);
            }


        }

        /*
        //Setup Audio Meter
        public static int fSensorSetup(int pCanId, Command pSetupCommand, OutputDataRate pOutputDataRate, int pVolume)
        {
           
        }
        */

        //Setup Logger
        public static int fLogSetup(int pCanId, Command pSetupCommand, LogEnable pLogStatus, LogSensor pSelectedSensor)
        {

            byte[] aBufferOut = new byte[3];
            byte[] aBufferIn = new byte[64];
            int rBufferInLength = 0;
            int rReturnStatus = 0;
            Command rCommand;
            Error rSetupStatus = Error.Fail;

            aBufferOut[0] = (byte)pSetupCommand;
            aBufferOut[1] = (byte)pLogStatus;
            aBufferOut[2] = (byte)pSelectedSensor;


            int rErrorCode = TcPCXINTComunicationWrapper.fPcxintCanTxRxCommand(pCanId, aBufferOut.Length, aBufferOut, aBufferIn.Length, ref aBufferIn, ref rBufferInLength, 1000);
            if (rErrorCode < 0)
            {
                rReturnStatus = -1;
                fConsoleWrite("[fSensorSetup] Write and Read fails with error " + rErrorCode + " on CanId " + pCanId);
                return (rReturnStatus);
            }

            if (rBufferInLength < (sizeof(byte) * 2))
            {
                rReturnStatus = -1;
                fConsoleWrite("[fSensorSetup] Can message to small on CanId " + pCanId);
                return (rReturnStatus);
            }

            rCommand = (Command)aBufferIn[0];
            rSetupStatus = (Error)aBufferIn[1];

            if (rCommand != pSetupCommand)
            {
                rReturnStatus = -1;
                return (rReturnStatus);
            }

            if (rSetupStatus == Error.Fail)
            {
                rReturnStatus = -1;
                return (rReturnStatus);
            }
            else
            {
                rReturnStatus = 0;
                return (rReturnStatus);
            }

        }
        #endregion

        #region Sensor Read Functions

        //Read Pressure/Temperature/Humidity Meter
        public static int fSensorRead(int pInstance, int pCanId, Command pReadCommand, out float pValue)
        {

            int rReturnStatus = 0;
            Command rCommand;
            Error rReadStatus = Error.Fail;
            byte[] aBufferOut = new byte[1];
            byte[] aBufferIn = new byte[1024];

            aBufferOut[0] = (byte)pReadCommand;

            int rResult = 0;
            if (rResult < 0)
            {
                pValue = 0;
                rReturnStatus = -1;
                fConsoleWrite("[fLogNSamplesRead] Write and Read fails with error " +  " on CanId " + pCanId);
                return (rReturnStatus);
            }

            int rBufferInLength = 0;
            rResult = 0;
            if (rResult < 0)
            {
                pValue = 0;
                rReturnStatus = -1;
                fConsoleWrite("[fLogNSamplesRead] Write and Read fails with error " + " on CanId " + pCanId);
                return (rReturnStatus);
            }

            if (rBufferInLength != (sizeof(byte) * 2 + sizeof(float)))
            {
                pValue = 0;
                rReturnStatus = -1;
                fConsoleWrite("[fLogNSamplesRead] Can message to small on CanId " + pCanId);
                return (rReturnStatus);
            }


            rCommand = (Command)aBufferIn[0];
            rReadStatus = (Error)aBufferIn[1];


            if (rCommand != pReadCommand)
            {
                pValue = 0;
                rReturnStatus = -1;
                return (rReturnStatus);
            }

            if (rReadStatus == Error.Fail)
            {
                pValue = 0;
                rReturnStatus = -1;
                return (rReturnStatus);
            }

            pValue = BitConverter.ToSingle(aBufferIn, 2);
            rReturnStatus = 0;
            return (rReturnStatus);
        }

        //Read Acc/Gyro Meter
        public static int fSensorRead(int pInstance, int pCanId, Command pReadCommand, out float pXValue, out float pYValue, out float pZValue)
        {

            int rReturnStatus = 0;
            Command rCommand;
            Error rReadStatus = Error.Fail;
            byte[] aBufferOut = new byte[1];
            byte[] aBufferIn = new byte[1024];

            aBufferOut[0] = (byte)pReadCommand;

            int rResult =0;
            if (rResult < 0)
            {
                pXValue = 0;
                pYValue = 0;
                pZValue = 0;
                rReturnStatus = -1;
                fConsoleWrite("[fLogNSamplesRead] Write and Read fails with error " + " on CanId " + pCanId);
                return (rReturnStatus);
            }

            int rBufferInLength = 0;
            rResult = 0;
            if (rResult < 0)
            {
                pXValue = 0;
                pYValue = 0;
                pZValue = 0;
                rReturnStatus = -1;
                fConsoleWrite("[fLogNSamplesRead] Write and Read fails with error " + " on CanId " + pCanId);
                return (rReturnStatus);
            }

            if (rBufferInLength < (sizeof(byte) * 2 + sizeof(float) * 3))
            {
                pXValue = 0;
                pYValue = 0;
                pZValue = 0;
                rReturnStatus = -1;
                fConsoleWrite("[fLogNSamplesRead] Can message to small on CanId " + pCanId);
                return (rReturnStatus);
            }


            rCommand = (Command)aBufferIn[0];
            rReadStatus = (Error)aBufferIn[1];

            if (rCommand != pReadCommand)
            {
                pXValue = 0;
                pYValue = 0;
                pZValue = 0;
                rReturnStatus = -1;
                return (rReturnStatus);
            }

            if (rReadStatus == Error.Fail)
            {
                pXValue = BitConverter.ToSingle(aBufferIn, 2);
                pYValue = BitConverter.ToSingle(aBufferIn, 6);
                pZValue = BitConverter.ToSingle(aBufferIn, 10);
                rReturnStatus = -1;
                return (rReturnStatus);
            }

            pXValue = BitConverter.ToSingle(aBufferIn, 2);
            pYValue = BitConverter.ToSingle(aBufferIn, 6);
            pZValue = BitConverter.ToSingle(aBufferIn, 10);
            rReturnStatus = 0;
            return (rReturnStatus);

        }

        public static int fLogNSamplesRead(int pCanId, Command pReadCommand, SubCommand pSubCommand, out int pSamplesNumber)
        {

            int rReturnStatus = 0;
            Command rCommand;
            Error rReadStatus = Error.Fail;
            byte[] aBufferOut = new byte[2];
            byte[] aBufferIn = new byte[1024];


            aBufferOut[0] = (byte)pReadCommand;
            aBufferOut[1] = (byte)pSubCommand;


            int rBufferInLength = 0;
            int rResult = TcPCXINTComunicationWrapper.fPcxintCanTxRxCommand(pCanId, aBufferOut.Length, aBufferOut, aBufferIn.Length, ref aBufferIn, ref rBufferInLength, 2000);
            if (rResult < 0)
            {
                pSamplesNumber = 0;
                rReturnStatus = -1;
                fConsoleWrite("[fLogNSamplesRead] Write and Read fails with error " + rResult + " on CanId " + pCanId);
                return (rReturnStatus);
            }


            if (rBufferInLength < (sizeof(byte) * 2 + sizeof(UInt32)))
            {
                pSamplesNumber = 0;
                rReturnStatus = -1;
                fConsoleWrite("[fSensorSetup] Can message to small on CanId " + pCanId);
                return (rReturnStatus);
            }

            rCommand = (Command)aBufferIn[0];
            rReadStatus = (Error)aBufferIn[1];

            if (rCommand != pReadCommand)
            {
                pSamplesNumber = 0;
                rReturnStatus = -1;
                return (rReturnStatus);
            }

            if (rReadStatus == Error.Fail)
            {
                pSamplesNumber = 0;
                rReturnStatus = -1;
                return (rReturnStatus);
            }
            else
            {
                pSamplesNumber = BitConverter.ToInt32(aBufferIn, 2);
                rReturnStatus = 0;
                return (rReturnStatus);
            }
        }

        public static int fLogRead(int pCanId, Command pReadCommand, int pSampleNumber, out float[] pLoggedData)
        {

            int rReturnStatus = 0;
            Command rCommand;
            Error rReadStatus = Error.Fail;

            byte[] aNumberOfSample = BitConverter.GetBytes(pSampleNumber);
            byte[] aBufferOut = new byte[5];
            byte[] aBufferIn = new byte[1024];

            aBufferOut[0] = (byte)pReadCommand;
            aBufferOut[1] = aNumberOfSample[0];
            aBufferOut[2] = aNumberOfSample[1];
            aBufferOut[3] = aNumberOfSample[2];
            aBufferOut[4] = aNumberOfSample[3];


            int rBufferInLength = 0;
            int rResult = TcPCXINTComunicationWrapper.fPcxintCanTxRxCommand(pCanId, aBufferOut.Length, aBufferOut, aBufferIn.Length, ref aBufferIn, ref rBufferInLength, 2000);
            if (rResult < 0)
            {
                pLoggedData = new float[0];
                rReturnStatus = -1;
                fConsoleWrite("[fLogRead] Write and Read fails with error " + rResult + " on CanId " + pCanId);
                return (rReturnStatus);
            }
            

            if (rBufferInLength < 2 * sizeof(byte) + sizeof(UInt16) + sizeof(float))
            {
               
                pLoggedData = new float[0];
                rReturnStatus = -1;
                fConsoleWrite("[fLogRead] Can message to small on CanId " + pCanId);
                return (rReturnStatus);
            }


            rCommand = (Command)aBufferIn[0];
            rReadStatus = (Error)aBufferIn[1];

            if (rCommand != pReadCommand)
            {
                pLoggedData = new float[0];
                rReturnStatus = -1;
                return (rReturnStatus);
            }

            if (rReadStatus == Error.Fail)
            {
                pLoggedData = new float[0];
                rReturnStatus = -1;
                return (rReturnStatus);
            }

            int rSampleByteLength = BitConverter.ToInt16(aBufferIn, 2);
            int rNumberOfSamples = (int)rSampleByteLength / sizeof(float);

            if (rBufferInLength != 2 * sizeof(byte) + sizeof(UInt16) + rNumberOfSamples * sizeof(float))
            {
                pLoggedData = new float[0];
                rReturnStatus = -1;
                fConsoleWrite("[fLogRead] Can message to small on CanId " + pCanId);
                return (rReturnStatus);
            }

            pLoggedData = new float[rNumberOfSamples];
            for (int i = 0; i < rNumberOfSamples; i++)
            {
                pLoggedData[i] = BitConverter.ToSingle(aBufferIn, 2 * sizeof(byte) + sizeof(UInt16) + i * sizeof(float));
            }

            rReturnStatus = 0;

            return (rReturnStatus);
        }

        public static int fLogRead(int pCanId, Command pReadCommand, int pSampleNumber, out float[] pLoggedData, out UInt64 pLatencyMilliseconds)
        {

            int rReturnStatus = 0;
            Command rCommand;
            Error rReadStatus = Error.Fail;

            byte[] aNumberOfSample = BitConverter.GetBytes(pSampleNumber);
            byte[] aBufferOut = new byte[5];
            byte[] aBufferIn = new byte[1024];

            aBufferOut[0] = (byte)pReadCommand;
            aBufferOut[1] = aNumberOfSample[0];
            aBufferOut[2] = aNumberOfSample[1];
            aBufferOut[3] = aNumberOfSample[2];
            aBufferOut[4] = aNumberOfSample[3];

           
            int rBufferInLength = 0;
            DateTime cStart = DateTime.UtcNow;
            int rResult = TcPCXINTComunicationWrapper.fPcxintCanTxRxCommand(pCanId, aBufferOut.Length, aBufferOut, aBufferIn.Length, ref aBufferIn, ref rBufferInLength, 2000);
            DateTime cEnd = DateTime.UtcNow;
            if (rResult < 0)
            {
                pLatencyMilliseconds = (UInt64)(cStart - cEnd).Milliseconds;
                pLoggedData = new float[0];
                rReturnStatus = -1;
                fConsoleWrite("[fLogRead] Write and Read fails with error " + rResult + " on CanId " + pCanId);
                return (rReturnStatus);
            }
           
            
            pLatencyMilliseconds = (UInt64)(cEnd-cStart).Milliseconds;


            if (rBufferInLength < 2 * sizeof(byte) + sizeof(UInt16) + sizeof(float))
            {
                cEnd = DateTime.UtcNow;
                pLoggedData = new float[0];
                rReturnStatus = -1;
                fConsoleWrite("[fLogRead] Can message to small on CanId " + pCanId);
                return (rReturnStatus);
            }


            rCommand = (Command)aBufferIn[0];
            rReadStatus = (Error)aBufferIn[1];

            if (rCommand != pReadCommand)
            {
                pLoggedData = new float[0];
                rReturnStatus = -1;
                return (rReturnStatus);
            }

            if (rReadStatus == Error.Fail)
            {
                pLoggedData = new float[0];
                rReturnStatus = -1;
                return (rReturnStatus);
            }

            int rSampleByteLength = BitConverter.ToInt16(aBufferIn, 2);
            int rNumberOfSamples = (int)rSampleByteLength / sizeof(float);

            if (rBufferInLength != 2 * sizeof(byte) + sizeof(UInt16) + rNumberOfSamples * sizeof(float))
            {
                pLoggedData = new float[0];
                rReturnStatus = -1;
                fConsoleWrite("[fLogRead] Can message to small on CanId " + pCanId);
                return (rReturnStatus);
            }

            pLoggedData = new float[rNumberOfSamples];
            for (int i = 0; i < rNumberOfSamples; i++)
            {
                pLoggedData[i] = BitConverter.ToSingle(aBufferIn, 2 * sizeof(byte) + sizeof(UInt16) + i * sizeof(float));
            }

            rReturnStatus = 0;

            return (rReturnStatus);
        }
        #endregion

        #region Sensor Acquire Start/Stop Functions
        public static int fSensorAcquire(int pCanId, Command pAcquireCommand, SubCommand pStartStop)
        {

            int rReturnStatus = 0;
            Command rCommand;
            Error rAcquireStatus = Error.Fail;
            byte[] aBufferOut = new byte[2];
            bool rReceivedAcquireStartOk = false;

            try
            {

                aBufferOut[0] = (byte)pAcquireCommand;
                aBufferOut[1] = (byte)pStartStop;


                int rErrorCode = TcPCXINTComunicationWrapper.fPcxintCanTxCommand(pCanId, aBufferOut.Length, aBufferOut);
                if (rErrorCode < 0)
                {
                    rReturnStatus = -1;
                    fConsoleWrite("[fSensorAcquire] Send fails with error " + rErrorCode + " on CanId " + pCanId);
                    return (rReturnStatus);
                }

                fConsoleWrite("[fSensorAcquire] Sent command " + pAcquireCommand + " on CanId " + pCanId);

                while (!rReceivedAcquireStartOk)
                {
                    byte[] aBufferIn = new byte[100];
                    int rBufferInLength = 0;

                    rErrorCode = TcPCXINTComunicationWrapper.fPcxintCanRxCommand(pCanId, aBufferIn.Length, ref aBufferIn, ref rBufferInLength, 1000);
                    if (rErrorCode < 0)
                    {
                        //rReturnStatus = -1;
                        fConsoleWrite("[fSensorAcquire] Receive fails with error " + rErrorCode + " on CanId " + pCanId);
                        //return (rReturnStatus);
                    }

                    if (rErrorCode == 0 && rBufferInLength == 2 * sizeof(byte))
                    {
                        rCommand = (Command)aBufferIn[0];
                        rAcquireStatus = (Error)aBufferIn[1];

                        fConsoleWrite("[fSensorAcquire] Received command " + rCommand + " with status " + rAcquireStatus + " on CanId " + pCanId);

                        if (rCommand == pAcquireCommand)
                        {

                            rReceivedAcquireStartOk = true;
                            if (rAcquireStatus == Error.Ack)
                            {
                                rReturnStatus = 0;
                                return (rReturnStatus);
                            }
                            else if (rAcquireStatus == Error.Fail)
                            {
                                rReturnStatus = -1;
                                return (rReturnStatus);
                            }
                        }
                    }
                    else if (rErrorCode == 0 && (rBufferInLength == (2 * sizeof(byte) + 3 * sizeof(float)) || rBufferInLength == (2 * sizeof(byte) + sizeof(float))))
                    {
                        fConsoleWrite("[fSensorAcquire] Received command " + ((Command)aBufferIn[0]) + " with status " + ((Error)aBufferIn[1]) + " on CanId " + pCanId);
                    }
                }
                rReturnStatus = 0;
                return (rReturnStatus);
            }
            catch (Exception e)
            {
                return (-1);
            }

        }
        #endregion

        #region Sensor Time Interval Acquire
        public static int fSensorAcquire(int pCanId, Command pAcquireCommand, UInt64 pMilliseconds)
        {

            int rReturnStatus = 0;
            Command rCommand;
            Error rAcquireStatus = Error.Fail;
            bool rReceivedAcquireStartOk = false;

            try
            {
                byte[] aMilliBytes = BitConverter.GetBytes(pMilliseconds);
                byte[] aBufferOut = new byte[5];
                aBufferOut[0] = (byte)pAcquireCommand;
                aBufferOut[1] = aMilliBytes[0];
                aBufferOut[2] = aMilliBytes[1];
                aBufferOut[3] = aMilliBytes[2];
                aBufferOut[4] = aMilliBytes[3];


                int rErrorCode = TcPCXINTComunicationWrapper.fPcxintCanTxCommand(pCanId, aBufferOut.Length, aBufferOut);
                if (rErrorCode < 0)
                {
                    rReturnStatus = -1;
                    fConsoleWrite("[fSensorAcquire] Send fails with error " + rErrorCode + " on CanId " + pCanId);
                    return (rReturnStatus);
                }

                fConsoleWrite("[fSensorAcquire] Sent command " + pAcquireCommand + " on CanId " + pCanId);

                while (!rReceivedAcquireStartOk)
                {
                    byte[] aBufferIn = new byte[100];
                    int rBufferInLength = 0;

                    rErrorCode = TcPCXINTComunicationWrapper.fPcxintCanRxCommand(pCanId, aBufferIn.Length, ref aBufferIn, ref rBufferInLength, 1000);
                    if (rErrorCode < 0)
                    {
                        //rReturnStatus = -1;
                        fConsoleWrite("[fSensorAcquire] Receive fails with error " + rErrorCode + " on CanId " + pCanId);
                        //return (rReturnStatus);
                    }

                    if (rErrorCode == 0 && rBufferInLength == 2 * sizeof(byte))
                    {
                        rCommand = (Command)aBufferIn[0];
                        rAcquireStatus = (Error)aBufferIn[1];

                        fConsoleWrite("[fSensorAcquire] Received command " + rCommand + " with status " + rAcquireStatus + " on CanId " + pCanId);

                        if (rCommand == pAcquireCommand)
                        {

                            rReceivedAcquireStartOk = true;
                            if (rAcquireStatus == Error.Ack)
                            {
                                rReturnStatus = 0;
                                return (rReturnStatus);
                            }
                            else if (rAcquireStatus == Error.Fail)
                            {
                                rReturnStatus = -1;
                                return (rReturnStatus);
                            }
                        }
                    }
                    else if (rErrorCode == 0 && (rBufferInLength == (2 * sizeof(byte) + 3 * sizeof(float)) || rBufferInLength == (2 * sizeof(byte) + sizeof(float))))
                    {
                        fConsoleWrite("[fSensorAcquire] Received command " + ((Command)aBufferIn[0]) + " with status " + ((Error)aBufferIn[1]) + " on CanId " + pCanId);
                    }
                }
                rReturnStatus = 0;
                return (rReturnStatus);
            }
            catch (Exception e)
            {
                return (-1);
            }
        }
           
        #endregion
    }

    public static class TcArchimedeLibraryExtensions
    {


        [DllImport("ArchimedeInterfaceLibrary.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int fSendCDCOLLMeasureData(StringBuilder pSN, StringBuilder pTaskName, StringBuilder pTaskNumber, StringBuilder pTestTaskNumber, StringBuilder pMeasureType, StringBuilder pMeasureUnit, StringBuilder pResult, double pValue, double pHighLimit, double pLowLimit);

        [DllImport("ArchimedeInterfaceLibrary.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int fSendFormattedMeasureData(StringBuilder pMessage, UInt16 pLength);


        [DllImport("ArchimedeInterfaceLibrary.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int fSendEventData(StringBuilder pTestCell, UInt64 pCycleID, StringBuilder pDate, StringBuilder pTime, UInt64 pEventID, StringBuilder pDescription, StringBuilder pData);


        [DllImport("ArchimedeInterfaceLibrary.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int fSendFormattedEventData(StringBuilder pMessage, UInt16 pLength);

        [DllImport("ArchimedeInterfaceLibrary.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int fInitializeClientWithConfigurationFile(StringBuilder pAgentID, StringBuilder pArchimedeInterfaceLibraryConfigurationFile);

        [DllImport("ArchimedeInterfaceLibrary.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int fSendFormattedEquipmentData(StringBuilder pMessage, UInt16 pLength);

        [DllImport("ArchimedeInterfaceLibrary.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int fRelease();

        internal static bool rIsArchimedeInitialized = false;


        public static int fArchimedeSendEquipmentData(string pMessage, UInt16 pLength)
        {
            return (fSendFormattedEquipmentData(new StringBuilder(pMessage), pLength));

        }

        public static int fArchimedeInitializeClientWithConfigurationFile(string pAgentID, string pArchimedeInterfaceLibraryConfigurationFile)
        {
            return (fInitializeClientWithConfigurationFile(new StringBuilder(pAgentID), new StringBuilder(pArchimedeInterfaceLibraryConfigurationFile)));
        }

        public static int fArchimedeReleaseConnection()
        {
            return (fRelease());
        }

        public static int fArchimedeSendEventData(string pTestCell, UInt64 pCycleID, string pDate, string pTime, UInt64 pEventID, string pDescription, string pData)
        {
            return (fSendEventData(new StringBuilder(pTestCell), pCycleID, new StringBuilder(pDate), new StringBuilder(pTime), pEventID, new StringBuilder(pDescription), new StringBuilder(pData)));
        }



        public static int fArchimedeSendEventData(string pMessage, UInt16 pLength)
        {
            return (fSendFormattedEventData(new StringBuilder(pMessage), pLength));
        }

        public static int fArchimedeSendMeasureData(string pSN, string pTaskName, string pTaskNumber, string pTestTaskNumber, string pMeasureType, string pMeasureUnit, string pResult, double pValue, double pHighLimit, double pLowLimit)
        {
            return (fSendCDCOLLMeasureData(new StringBuilder(pSN), new StringBuilder(pTaskName), new StringBuilder(pTaskNumber), new StringBuilder(pTestTaskNumber), new StringBuilder(pMeasureType), new StringBuilder(pMeasureUnit), new StringBuilder(pResult), pValue, pHighLimit, pLowLimit));
        }

        public static int fArchimedeSendMeasureData(string pMessage, UInt16 pLength)
        {
            return (fSendFormattedMeasureData(new StringBuilder(pMessage), pLength));
        }

    }

    public static class TcPCXINTComunicationWrapper
    {

        #region Functions Imports

        [DllImport("PcxintCommunic.dll", EntryPoint = "fmPcxintCanInit", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int _fmPcxintCanInit();

        [DllImport("PcxintCommunic.dll", EntryPoint = "fmPcxintCanOpen", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int _fmPcxintCanOpen(string pBoardType, int pBoardNo, int pPortNo, int pSpeed, int pTimeoutRead);

        [DllImport("PcxintCommunic.dll", EntryPoint = "fmPcxintCanClose", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int _fmPcxintCanClose();

        [DllImport("PcxintCommunic.dll", EntryPoint = "fmPcxintCanCheckSumSet", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int _fmPcxintCanCheckSumSet(int pCheckSumBytes);

        [DllImport("PcxintCommunic.dll", EntryPoint = "fmPcxintCanIsPortConnected", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern bool _fmPcxintCanIsPortConnected();

        [DllImport("PcxintCommunic.dll", EntryPoint = "fmPcxintCanLock", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int _fmPcxintCanLock();

        [DllImport("PcxintCommunic.dll", EntryPoint = "fmPcxintCanUnlock", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int _fmPcxintCanUnlock();

        [DllImport("PcxintCommunic.dll", EntryPoint = "fmPcxintCanSync", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int _fmPcxintCanSync();

        [DllImport("PcxintCommunic.dll", EntryPoint = "fmPcxintCanEmptyRxQueue", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int _fmPcxintCanEmptyRxQueue(int pDeviceId);

        [DllImport("PcxintCommunic.dll", EntryPoint = "fmPcxintCanTxCommand", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int _fmPcxintCanTxCommand(int pDeviceId, int pTxDataLen, byte[] pTxData);

        [DllImport("PcxintCommunic.dll", EntryPoint = "fmPcxintCanRxCommand", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int _fmPcxintCanRxCommand(int pDeviceId, int pRxMaxDataLen, IntPtr pRxData, IntPtr pBytesReceived, int pRxTimeout);

        [DllImport("PcxintCommunic.dll", EntryPoint = "fmPcxintCanRxIfAnyCommand", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int _fmPcxintCanRxIfAnyCommand(int pDeviceId, int pRxMaxDataLen, IntPtr pRxData, IntPtr pBytesReceived);

        #endregion

        #region Functions Wrappers

        public static int fPcxintCanInit()
        {
            var rErrorCode = _fmPcxintCanInit();
            Fix();
            return rErrorCode;
        }

        public static int fPcxintCanOpen(string pBoardType, int pBoardNo, int pPortNo, int pSpeed, int pTimeoutRead)
        {
            int rErrorCode = _fmPcxintCanOpen(pBoardType, pBoardNo, pPortNo, pSpeed, pTimeoutRead);
            Fix();
            return rErrorCode;
        }

        public static int fPcxintCanClose()
        {
            var rErrorCode = _fmPcxintCanClose();
            Fix();
            return rErrorCode;
        }

        public static int fPcxintCanCheckSumSet(int pCheckSumBytes)
        {
            var rErrorCode = _fmPcxintCanCheckSumSet(pCheckSumBytes);
            Fix();
            return rErrorCode;
        }

        public static bool fPcxintCanIsPortConnected()
        {

            var rSuccess = _fmPcxintCanIsPortConnected();
            Fix();
            return rSuccess;
        }

        public static int fPcxintCanLock()
        {
            var rErrorCode = _fmPcxintCanLock();
            Fix();
            return rErrorCode;
        }

        public static int fPcxintCanUnlock()
        {
            var rErrorCode = _fmPcxintCanUnlock();
            Fix();
            return rErrorCode;
        }

        public static int fPcxintCanSync()
        {
            var rErrorCode = _fmPcxintCanSync();
            Fix();
            return rErrorCode;
        }

        public static int fPcxintCanEmptyRxQueue(int pDeviceId)
        {
            var rErrorCode = _fmPcxintCanEmptyRxQueue(pDeviceId);
            Fix();
            return rErrorCode;
        }

        public static int fPcxintCanTxCommand(int pDeviceId, int pTxDataLen, byte[] pTxData)
        {
            //IntPtr rTxDataPtr = Marshal.AllocHGlobal(pTxData.Length);
            //Marshal.StructureToPtr(pTxData, rTxDataPtr, true);

            var rErrorCode = _fmPcxintCanTxCommand(pDeviceId, pTxDataLen, pTxData);

            //Marshal.FreeHGlobal(rTxDataPtr);
            Fix();

            return rErrorCode;
        }

        public static int fPcxintCanRxCommand(int pDeviceId, int pRxMaxDataLen, ref byte[] pRxData, ref int pBytesReceived, int pRxTimeout)
        {
            int rErrorCode = 0;
            try
            {
                IntPtr rRxDataPtr = Marshal.AllocHGlobal(pRxMaxDataLen);
                IntPtr rBytesReceivedPtr = Marshal.AllocHGlobal(Marshal.SizeOf(pBytesReceived));

                rErrorCode = _fmPcxintCanRxCommand(pDeviceId, pRxMaxDataLen, rRxDataPtr, rBytesReceivedPtr, pRxTimeout);

                pBytesReceived = (int)Marshal.PtrToStructure(rBytesReceivedPtr, typeof(int));
                if (pBytesReceived > pRxMaxDataLen)
                    pBytesReceived = pRxMaxDataLen;

                Marshal.Copy(rRxDataPtr, pRxData, 0, pBytesReceived);

                Marshal.FreeHGlobal(rBytesReceivedPtr);
                Marshal.FreeHGlobal(rRxDataPtr);
                Fix();
            } catch(Exception e) {

                Console.WriteLine(e.ToString());
                rErrorCode = -1;
            }

            return rErrorCode;
        }

        public static int fPcxintCanTxRxCommand(int pDeviceId, int pTxDataLen, byte[] pTxData, int pRxMaxDataLen, ref byte[] pRxData, ref int pBytesReceived, int pRxTimeout)
        {
            var rErrorCode = fPcxintCanLock();
            if (rErrorCode == 0)
            {
                rErrorCode = fPcxintCanTxCommand(pDeviceId, pTxDataLen, pTxData);
                if (rErrorCode == 0)
                    rErrorCode = fPcxintCanRxCommand(pDeviceId, pRxMaxDataLen, ref pRxData, ref pBytesReceived, pRxTimeout);

                fPcxintCanUnlock();
            }

            return rErrorCode;
        }

        public static int fPcxintCanRxIfAnyCommand(int pDeviceId, int pRxMaxDataLen, ref byte[] pRxData, ref int pBytesReceived)
        {
            IntPtr rRxDataPtr = Marshal.AllocHGlobal(pRxMaxDataLen);
            IntPtr rBytesReceivedPtr = Marshal.AllocHGlobal(Marshal.SizeOf(pBytesReceived));

            var rErrorCode = _fmPcxintCanRxIfAnyCommand(pDeviceId, pRxMaxDataLen, rRxDataPtr, rBytesReceivedPtr);

            pBytesReceived = (int)Marshal.PtrToStructure(rBytesReceivedPtr, typeof(int));
            if (pBytesReceived > pRxMaxDataLen)
                pBytesReceived = pRxMaxDataLen;

            Marshal.Copy(rRxDataPtr, pRxData, 0, pBytesReceived);

            Marshal.FreeHGlobal(rBytesReceivedPtr);
            Marshal.FreeHGlobal(rRxDataPtr);
            Fix();

            return rErrorCode;
        }

        #endregion

        #region FpuNativeMethods

        public const string MsvcrtDll = "msvcrt.dll";

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Fix()
        {
            const int _MCW_EM = 0x0008001f;
            const int _EM_INVALID = 0x00000010;
            _controlfp(_MCW_EM, _EM_INVALID);
        }
        #endregion

        #region PlatformInvoke

        [DllImport(MsvcrtDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int _controlfp(int inNew, int inMask);

        #endregion
    }


    class TcSensorDataLoaderControl : INotifyPropertyChanged
    {

        public class PCXINTSettings
        {
            public int rpBoardNumber;
            public int rpPortNumber;
            public int rpSpeed;
            public int rpTimeout;

            public const int rpBoardDefaultNumber = 1;
            public const int rpPortDefaultNumber = 1;
            public const int rpDefaultSpeed = 1;
            public const int rpDefaultTimeout = 1000000;


            public PCXINTSettings(int pBoardNumber, int pPortNumber, int pSpeed, int pTimeout)
            {
                this.rpBoardNumber = pBoardNumber;
                this.rpPortNumber = pPortNumber;
                this.rpSpeed = pSpeed;
                this.rpTimeout = pTimeout;
            }
        }

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string pSection, string pKey, string pDefaultValue, StringBuilder pValue, int pSize, string pFilePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileInt(string pSection, string pKey, int pDefaultValue, string pFilePath);


        public enum LoaderMode { Monitoring, Logging };




        private TcLogBook cmLogBook;

        
        private const int kLogNoSamples = -1;
        private const int kLogReadNSamplesFail = -3;
        private const int kLogNSamplesInvalid = -2;
        private const int kLogReadSamplesFail = -4;

        private PCXINTSettings cmPCXINTSettings;
        
        private static bool rmLogEnable;
        private const bool rmLogDefaultEnable = true;

        private static bool rmConsoleEnable;
        private const bool rmConsoleDefaultEnable = true;

        private static string rmMachineHardwareConfigurationDefaultIniFilePath = AppDomain.CurrentDomain.BaseDirectory;
        private static string rmMachineHardwareConfigurationIniFilePath = rmMachineHardwareConfigurationDefaultIniFilePath;
        
        private const string rmMachineHardwareConfigurationDefaultFileName = "machine_hwconfig.json";
        private static string rmMachineHardwareConfigurationFileName = rmMachineHardwareConfigurationDefaultFileName;

        private static bool rmArchimedeDefaultEnable = true;
        private static bool rmArchimedeEnable = rmArchimedeDefaultEnable;
       

        private const string rmSensorDataLoaderDefaultIniFileName = "SensorDataLoader.ini";
        private static string rmSensorDataLoaderIniFileName = rmSensorDataLoaderDefaultIniFileName;
        

        private static string rmSensorDataLoaderDefaultIniFilePath = AppDomain.CurrentDomain.BaseDirectory;
        private static string rmSensorDataLoaderIniFilePath = rmSensorDataLoaderDefaultIniFilePath;



        private const string rmArchimedeDefaultIniFileName = "ArchimedeDriverConf.ini";
        private static string rmArchimedeIniFileName = rmArchimedeDefaultIniFileName;


        private static string rmArchimedeDefaultIniFilePath = AppDomain.CurrentDomain.BaseDirectory;
        private static string rmArchimedeIniFilePath = rmArchimedeDefaultIniFilePath;





        private bool rmRun = false;
        private bool rmInitSuccess;

        



        //Ini Sections
        string kLoaderSection = "LOADER";
        string kLogSection = "LOADER";
        string kPCXINTSection = "PCXINT";

        //Ini Keys
        private const string kLogEanbledKey = "LOGENABLED";
        private const string kConsoleEanbledKey = "CONSOLEENABLED";
        private const string kMachineFilePathKey = "MACHINEFILEPATH";
        private const string kMachineFileNameKey = "MACHINEFILENAME";
        public const string kBoardNumberKey = "BOARDNUMBER";
        public const string kPortNumberKey = "PORTNUMBER";
        public const string kSpeedKey = "SPEED";
        public const string kTimeoutKey = "CANTIMEOUT";
        public const string kLoaderModeKey = "LOADERMODE";
        public const string kArchimedeEnabledKey = "ARCHIMEDEENABLED";
        public const string kReferenceTypeKey = "REFERENCETYPE";
        public const string kEquipmentSNKey = "EQUIPMENTSN";




        private LoaderMode cmLoaderMode;
        public event PropertyChangedEventHandler PropertyChanged;
        private Dictionary<int, Sensor> cmSensorComunicationMap = null;
        private Dictionary<int, Sensor> cmEnabledSensorComunicationMap = null;
        private string rmEquipmentSerialNumber;
        private const string rmEquipmentDefaultSerialNumber = "FDS014AA";

        private string rmReferenceType;
        private const string rmReferenceDefaultType = "Sensor";

        private Dictionary<Sensor, Dictionary<PhysicalProperty, double>> cmSensorCorrectionOffsetMap = null;
        private Plant cmThisPlant = null;
        private Equipment cmThisEquipment = null;
        

        private ObservableCollection<EnvironmentMeasureRealtime> cmEnvironmentRealtimeObservableMeasureData = null;
        private ObservableCollection<KinematicMeasureRealtime> cmKinematicObservableSensorMeasureData = null;
        private ObservableCollection<EnvironmentMeasureStatistics> cmEnvironmentStatisticsObservableMeasureData = null;
        private ObservableCollection<KinematicMeasureStatistics> cmKinematicStatisticsObservableMeasureData = null;


        public ObservableCollection<EnvironmentMeasureRealtime> cpEnvironmentRealtimeObservableMeasureData
        {
            get => this.cmEnvironmentRealtimeObservableMeasureData;
            set {
                this.cmEnvironmentRealtimeObservableMeasureData = value;
            }
        }

        public ObservableCollection<KinematicMeasureRealtime> cpKinematicRealtimeObservableMeasureData
        {
            get => this.cmKinematicObservableSensorMeasureData;
            set {
                this.cmKinematicObservableSensorMeasureData = value;
            }
        }

        public ObservableCollection<EnvironmentMeasureStatistics> cpEnvironmentStatisticsObservableMeasureData
        {
            get => cmEnvironmentStatisticsObservableMeasureData;
            set
            {
                this.cmEnvironmentStatisticsObservableMeasureData = value;
            }
        }

        public ObservableCollection<KinematicMeasureStatistics> cpKinematicStatisticsObservableMeasureData
        {
            get => this.cmKinematicStatisticsObservableMeasureData;
            set
            {
                this.cmKinematicStatisticsObservableMeasureData = value;
            }
        }

        public LoaderMode cpLoaderMode
        {
            get { return(this.cmLoaderMode); }
            set
            {
                this.cmLoaderMode = value;
                rOnPropertyChanged(nameof(cpLoaderMode));
            }
        }

        public bool rpInitSuccess
        {
            get => rmInitSuccess;
            set
            {
                rmInitSuccess = value;
                rOnPropertyChanged(nameof(rpInitSuccess));
            }
        }
        public bool rpRun
        {
            get => rmRun;
            set
            {
                rmRun = value;
                rOnPropertyChanged(nameof(rpRun));
            }
        }


        protected virtual void rOnPropertyChanged([CallerMemberName] string pPropertyName = null)
        {
            var rmHandler = PropertyChanged;
            rmHandler?.Invoke(this, new PropertyChangedEventArgs(pPropertyName));
        }

        public TcSensorDataLoaderControl()
        {

            try {
                string rSensorDataLoaderIniFile = rmSensorDataLoaderIniFilePath + rmSensorDataLoaderIniFileName;
                fInitializeSensorDataLoaderStatus(rSensorDataLoaderIniFile);
                fConsoleWriteLine("[TcSensorDataLoaderControl] Configuration file %s readed");
            }
            catch (Exception e)
            {
                fConsoleWriteLine("[TcSensorDataLoaderControl] Exception " + e.GetType().FullName + " thrown with message " + e.Message);
                fConsoleWriteLine("[TcSensorDataLoaderControl] Fail to deserialize json plant object");
            }

            this.cmLogBook = new TcLogBook(rmLogEnable);
            
        }


        public void fInitializeSensorDataLoaderStatus(string pLoaderInitFilePath) {

            try
            {
                //PCXINT Section
                int rBoardNumber = ((rBoardNumber = GetPrivateProfileInt(kPCXINTSection, kBoardNumberKey, 1, pLoaderInitFilePath)) > 0 ? rBoardNumber : PCXINTSettings.rpBoardDefaultNumber);
                int rPortNumber = ((rPortNumber = GetPrivateProfileInt(kPCXINTSection, kPortNumberKey, 1, pLoaderInitFilePath)) > 0 ? rPortNumber : PCXINTSettings.rpPortDefaultNumber);
                int rSpeed = ((rSpeed = GetPrivateProfileInt(kPCXINTSection, kSpeedKey, 1, pLoaderInitFilePath)) > 0 ? rSpeed : PCXINTSettings.rpDefaultSpeed);
                int rTimeout = ((rTimeout = GetPrivateProfileInt(kPCXINTSection, kTimeoutKey, 1, pLoaderInitFilePath)) > 0 ? rTimeout : PCXINTSettings.rpDefaultTimeout);
                this.cmPCXINTSettings = new PCXINTSettings(rBoardNumber, rPortNumber, rSpeed, rTimeout);


                //LOG Section
                bool rLogEnable = (GetPrivateProfileInt(kLogSection, kLogEanbledKey, 1, pLoaderInitFilePath) == 1 ? true : rmLogDefaultEnable);
                rmLogEnable = rLogEnable;

                bool rConsoleEnable = (GetPrivateProfileInt(kLogSection, kConsoleEanbledKey, 1, pLoaderInitFilePath) == 1 ? true : rmConsoleDefaultEnable);
                rmConsoleEnable = rConsoleEnable;



                //LOADER Section
                StringBuilder cMachineHardwareConfigurationFilePath = new StringBuilder(rmMachineHardwareConfigurationDefaultIniFilePath);
                string rMachineHardwareConfigurationFilePath = (GetPrivateProfileString(kLoaderSection, kMachineFilePathKey, rmMachineHardwareConfigurationDefaultIniFilePath, cMachineHardwareConfigurationFilePath, cMachineHardwareConfigurationFilePath.MaxCapacity, pLoaderInitFilePath) > 0 ? cMachineHardwareConfigurationFilePath.ToString() : rmMachineHardwareConfigurationDefaultIniFilePath);
                rmMachineHardwareConfigurationIniFilePath = rMachineHardwareConfigurationFilePath;

                StringBuilder cMachineHardwareConfigurationFileName = new StringBuilder(rmMachineHardwareConfigurationDefaultFileName);
                string rMachineHardwareConfigurationFileName = (GetPrivateProfileString(kLoaderSection, kMachineFileNameKey, rmMachineHardwareConfigurationDefaultFileName, cMachineHardwareConfigurationFileName, cMachineHardwareConfigurationFileName.MaxCapacity, pLoaderInitFilePath) > 0 ? cMachineHardwareConfigurationFileName.ToString() : rmMachineHardwareConfigurationDefaultFileName);
                rmMachineHardwareConfigurationFileName = rMachineHardwareConfigurationFileName;


                bool rArchimedeEnable = (GetPrivateProfileInt(kLoaderSection, kArchimedeEnabledKey, 1, pLoaderInitFilePath) == 1 ? true : rmArchimedeDefaultEnable);
                rmArchimedeEnable = rArchimedeEnable;

                int rLoaderMode = ((rLoaderMode = GetPrivateProfileInt(kLoaderSection, kLoaderModeKey, 1, pLoaderInitFilePath)) > 0 ? rLoaderMode : default);
                this.cmLoaderMode = (LoaderMode)rLoaderMode;

                StringBuilder cReferenceType = new StringBuilder(rmReferenceDefaultType);
                string rReferenceType = (GetPrivateProfileString(kLoaderSection, kReferenceTypeKey, rmReferenceDefaultType, cReferenceType, cReferenceType.MaxCapacity, pLoaderInitFilePath) > 0 ? cReferenceType.ToString() : rmReferenceDefaultType);
                this.rmReferenceType = rReferenceType;

                StringBuilder cEquipmentSN = new StringBuilder(rmEquipmentDefaultSerialNumber);
                string rEquipmentSN = (GetPrivateProfileString(kLoaderSection, kEquipmentSNKey, rmEquipmentDefaultSerialNumber, cEquipmentSN, cEquipmentSN.MaxCapacity, pLoaderInitFilePath) > 0 ? cReferenceType.ToString() : rmEquipmentDefaultSerialNumber);
                this.rmEquipmentSerialNumber = rEquipmentSN;




                fConsoleWriteLine("[fInitializeSensorDataLoaderStatus] Sensor Data Loader INI File readed");
                return;
            }
            catch (Exception e)
            {
                fConsoleWriteLine("[fInitializeSensorDataLoaderStatus] Exception " + e.GetType().FullName + " thrown with message " + e.Message);
                fConsoleWriteLine("[fInitializeSensorDataLoaderStatus] Sensor Data Loader INI File not readed");
                return;
            }
        }

       
        private int fAcquireMachineHardwareConfiguration(string pMachineHardwareConfigurationFile) {

            string rMachineHardwareConfigurationFileContent;
            
            Plant cThisPlant;
            Equipment cThisEquipment;


            if (cmSensorComunicationMap != null) {
                fConsoleWriteLine("[fAcquireMachineHardwareConfiguration] Machine hardware configuration already initialized");
                return (-2);
            }

            if (pMachineHardwareConfigurationFile.Trim() == ""){
                fConsoleWriteLine("[fAcquireMachineHardwareConfiguration] Invalid machine hardware configuration file path " + pMachineHardwareConfigurationFile);
                return (-1);
            }
            

            try {
                rMachineHardwareConfigurationFileContent = File.ReadAllText(pMachineHardwareConfigurationFile);
            } catch (Exception e) {
                fConsoleWriteLine("[fAcquireMachineHardwareConfiguration] Exception " + e.GetType().FullName + " thrown with message " + e.Message);
                fConsoleWriteLine("[fAcquireMachineHardwareConfiguration] Fail to read machine hardware configuration file");
                return (-1);
            }

            try {
                cThisPlant = JsonConvert.DeserializeObject<Plant>(rMachineHardwareConfigurationFileContent);
            } catch (Exception e) {
                fConsoleWriteLine("[fAcquireMachineHardwareConfiguration] Exception " + e.GetType().FullName + " thrown with message " + e.Message);
                fConsoleWriteLine("[fAcquireMachineHardwareConfiguration] Fail to deserialize json plant object");
                return (-1);
            }


            try {
                cThisEquipment = cThisPlant.Equipments.Find((cEquipment) => { return (cEquipment.SerialNumber.Equals(rmEquipmentSerialNumber)); });
                if (cThisEquipment == null){
                    fConsoleWriteLine("[fAcquireMachineHardwareConfiguration] Invalid Machine Hardware Configuration File " + pMachineHardwareConfigurationFile);
                    return (-1);
                }
            } catch (Exception e) {
                fConsoleWriteLine("[fAcquireMachineHardwareConfiguration] Exception " + e.GetType().FullName + " thrown with message " + e.Message);
                fConsoleWriteLine("[fAcquireMachineHardwareConfiguration] Fail to find this equipment into the given plant");
                return (-1);
            }

            try {
                cmSensorComunicationMap = new Dictionary<int, Sensor>();
                cThisEquipment.Sensors
                    .FindAll((cSensor) => { return (cSensor.Type.Equals(TcSM100Manager.kSensorModel)); })
                    .ForEach((cSensor) => {
                        if (int.TryParse(cSensor.CanAddress.Trim('#'), out int rCanId)) {
                            cmSensorComunicationMap.Add(rCanId, cSensor);
                        }
                    });
                cThisEquipment.Racks
                    .ForEach((cRack) => {
                        cRack._Units
                            .ForEach((cUnit) => {
                                cUnit.Sensors
                                    .FindAll((cSensor) => { return (cSensor.Type.Equals(TcSM100Manager.kSensorModel)); })
                                    .ForEach((cSensor) => {
                                        if (int.TryParse(cSensor.CanAddress.Trim('#'), out int rCanId)) {
                                            cmSensorComunicationMap.Add(rCanId, cSensor);
                                        }
                                    });
                            });
                        cRack.Sensors
                            .FindAll((cSensor) => { return (cSensor.Type.Equals(TcSM100Manager.kSensorModel)); })
                            .ForEach((cSensor) => {
                                if (int.TryParse(cSensor.CanAddress.Trim('#'), out int rCanId)) {
                                    cmSensorComunicationMap.Add(rCanId, cSensor);
                                }
                            });
                    });
            }
            catch (Exception e) {
                fConsoleWriteLine("[fAcquireMachineHardwareConfiguration] Exception " + e.GetType().FullName + " thrown with message " + e.Message);
                fConsoleWriteLine("[fAcquireMachineHardwareConfiguration] Fail to find parse equipment structure");
                return (-1);
            }
            
            //Store Equipment structure
            cmThisEquipment = cThisEquipment;
            cmThisPlant = cThisPlant;
            return (0);
        }


        private int fInitializeSensorCorrectionOffsetMap()
        {

            try
            {

                Dictionary<string, Dictionary<string, double>> cGivenSensorOffsetMap = new Dictionary<string, Dictionary<string, double>>();

                Dictionary<string, double> cTemporanySensorOffsetMapA = new Dictionary<string, double>();
                cTemporanySensorOffsetMapA.Add(Temperature.MeasureType, 0); //-3.93
                cTemporanySensorOffsetMapA.Add(Humidity.MeasureType, 0); //5
                cTemporanySensorOffsetMapA.Add(Pressure.MeasureType, 0);
                cTemporanySensorOffsetMapA.Add(Acceleration.MeasureType, 0);
                cTemporanySensorOffsetMapA.Add(MagneticField.MeasureType, 0);
                cTemporanySensorOffsetMapA.Add(Gyroscope.MeasureType, 0);
                cGivenSensorOffsetMap.Add("Rear Mobile Clamp", cTemporanySensorOffsetMapA);

                Dictionary<string, double> cTemporanySensorOffsetMapB = new Dictionary<string, double>();
                cTemporanySensorOffsetMapB[Temperature.MeasureType] = 0; //-3.43
                cTemporanySensorOffsetMapB[Humidity.MeasureType] = 0 ; //2.27
                cTemporanySensorOffsetMapB[Pressure.MeasureType] = 0;
                cTemporanySensorOffsetMapB[Acceleration.MeasureType] = 0;
                cTemporanySensorOffsetMapB[MagneticField.MeasureType] = 0;
                cTemporanySensorOffsetMapB[Gyroscope.MeasureType] = 0;
                cGivenSensorOffsetMap.Add("Front Fixed Clamp", cTemporanySensorOffsetMapB);

                Dictionary<string, double> cTemporanySensorOffsetMapC = new Dictionary<string, double>();
                cTemporanySensorOffsetMapC[Temperature.MeasureType] = 0; // -3.6
                cTemporanySensorOffsetMapC[Humidity.MeasureType] = 0;// 4.5
                cTemporanySensorOffsetMapC[Pressure.MeasureType] = 0;
                cTemporanySensorOffsetMapC[Acceleration.MeasureType] = 0;
                cTemporanySensorOffsetMapC[MagneticField.MeasureType] = 0;
                cTemporanySensorOffsetMapC[Gyroscope.MeasureType] = 0;
                cGivenSensorOffsetMap.Add("Chassis", cTemporanySensorOffsetMapC);

                Dictionary<string, double> cTemporanySensorOffsetMapD = new Dictionary<string, double>();
                cTemporanySensorOffsetMapD[Temperature.MeasureType] = 0;// -2.6
                cTemporanySensorOffsetMapD[Humidity.MeasureType] = 0; //6.7
                cTemporanySensorOffsetMapD[Pressure.MeasureType] = 0;
                cTemporanySensorOffsetMapD[Acceleration.MeasureType] = 0;
                cTemporanySensorOffsetMapD[MagneticField.MeasureType] = 0;
                cTemporanySensorOffsetMapD[Gyroscope.MeasureType] = 0;
                cGivenSensorOffsetMap.Add("External Environment", cTemporanySensorOffsetMapD);

                Dictionary<string, double> cTemporanySensorOffsetMapE = new Dictionary<string, double>();
                cTemporanySensorOffsetMapE[Temperature.MeasureType] = 0;// -1.6;
                cTemporanySensorOffsetMapE[Humidity.MeasureType] = 0; //3.4;
                cTemporanySensorOffsetMapE[Pressure.MeasureType] = 0;
                cTemporanySensorOffsetMapE[Acceleration.MeasureType] = 0;
                cTemporanySensorOffsetMapE[MagneticField.MeasureType] = 0;
                cTemporanySensorOffsetMapE[Gyroscope.MeasureType] = 0;
                cGivenSensorOffsetMap.Add("Internal Electrical Top Box", cTemporanySensorOffsetMapE);

                Dictionary<string, double> cTemporanySensorOffsetMapF = new Dictionary<string, double>();
                cTemporanySensorOffsetMapF[Temperature.MeasureType] = 0;
                cTemporanySensorOffsetMapF[Humidity.MeasureType] = 0;
                cTemporanySensorOffsetMapF[Pressure.MeasureType] = 0;
                cTemporanySensorOffsetMapF[Acceleration.MeasureType] = 0;
                cTemporanySensorOffsetMapF[MagneticField.MeasureType] = 0;
                cTemporanySensorOffsetMapF[Gyroscope.MeasureType] = 0;
                cGivenSensorOffsetMap.Add("Axis 4", cTemporanySensorOffsetMapF);



                Dictionary<string, double> cTemporanySensorOffsetMapG = new Dictionary<string, double>();
                cTemporanySensorOffsetMapG[Temperature.MeasureType] = 0;
                cTemporanySensorOffsetMapG[Humidity.MeasureType] = 0;
                cTemporanySensorOffsetMapG[Pressure.MeasureType] = 0;
                cTemporanySensorOffsetMapG[Acceleration.MeasureType] = 0;
                cTemporanySensorOffsetMapG[MagneticField.MeasureType] = 0;
                cTemporanySensorOffsetMapG[Gyroscope.MeasureType] = 0;
                cGivenSensorOffsetMap.Add("Axis 3", cTemporanySensorOffsetMapG);

                Dictionary<string, double> cTemporanySensorOffsetMapH = new Dictionary<string, double>();
                cTemporanySensorOffsetMapH[Temperature.MeasureType] = 0;
                cTemporanySensorOffsetMapH[Humidity.MeasureType] = 0;
                cTemporanySensorOffsetMapH[Pressure.MeasureType] = 0;
                cTemporanySensorOffsetMapH[Acceleration.MeasureType] = 0;
                cTemporanySensorOffsetMapH[MagneticField.MeasureType] = 0;
                cTemporanySensorOffsetMapH[Gyroscope.MeasureType] = 0;
                cGivenSensorOffsetMap.Add("Axis 2", cTemporanySensorOffsetMapH);

               

                Dictionary<string, double> cTemporanySensorOffsetMapI = new Dictionary<string, double>();
                cTemporanySensorOffsetMapI[Temperature.MeasureType] = 0;
                cTemporanySensorOffsetMapI[Humidity.MeasureType] = 0;
                cTemporanySensorOffsetMapI[Pressure.MeasureType] = 0;
                cTemporanySensorOffsetMapI[Acceleration.MeasureType] = 0;
                cTemporanySensorOffsetMapI[MagneticField.MeasureType] = 0;
                cTemporanySensorOffsetMapI[Gyroscope.MeasureType] = 0;
                cGivenSensorOffsetMap.Add("Axis 1", cTemporanySensorOffsetMapI);

                Dictionary<string, double> cTemporanySensorOffsetMapL = new Dictionary<string, double>();
                cTemporanySensorOffsetMapL[Temperature.MeasureType] = 0;
                cTemporanySensorOffsetMapL[Humidity.MeasureType] = 0;
                cTemporanySensorOffsetMapL[Pressure.MeasureType] = 0;
                cTemporanySensorOffsetMapL[Acceleration.MeasureType] = 0;
                cTemporanySensorOffsetMapL[MagneticField.MeasureType] = 0;
                cTemporanySensorOffsetMapL[Gyroscope.MeasureType] = 0;
                cGivenSensorOffsetMap.Add("Axis 5", cTemporanySensorOffsetMapL);

                Dictionary<string, double> cTemporanySensorOffsetMapM = new Dictionary<string, double>();
                cTemporanySensorOffsetMapM[Temperature.MeasureType] = 0;
                cTemporanySensorOffsetMapM[Humidity.MeasureType] = 0;
                cTemporanySensorOffsetMapM[Pressure.MeasureType] = 0;
                cTemporanySensorOffsetMapM[Acceleration.MeasureType] = 0;
                cTemporanySensorOffsetMapM[MagneticField.MeasureType] = 0;
                cTemporanySensorOffsetMapM[Gyroscope.MeasureType] = 0;
                cGivenSensorOffsetMap.Add("Axis 8", cTemporanySensorOffsetMapM);






                this.cmSensorCorrectionOffsetMap= new Dictionary<Sensor, Dictionary<PhysicalProperty, double>>();
                foreach (Sensor cSensor in cmEnabledSensorComunicationMap.Values) {
                    Dictionary<PhysicalProperty, double> cOffsetMap = new Dictionary<PhysicalProperty, double>();
                    foreach (PhysicalProperty cProperty in cSensor.PhysicalProperties)
                    {
                        if (cProperty.Enabled == 1) {
                            cOffsetMap.Add(cProperty, cGivenSensorOffsetMap[cSensor.Position][cProperty.MeasureType]);
                        }
                    }
                    this.cmSensorCorrectionOffsetMap.Add(cSensor, cOffsetMap);
                }

                fConsoleWriteLine("[fInitializeSensorCorrectionOffsetMap] Initialized Command Map");

            }
            catch (Exception e)
            {
                fConsoleWriteLine("[fInitializeSensorCorrectionOffsetMap] Exception " + e.GetType().FullName + " thrown with message " + e.Message);
                fConsoleWriteLine("[fInitializeSensorCorrectionOffsetMap] Fail to find parse equipment structure");
                return (-1);
            }


            return (0);
        }

        private int fSelectEnabledSensor() {
            try {
                
                if (cmEnabledSensorComunicationMap != null)
                {
                    fConsoleWriteLine("[fSelectEnabledSensor] Select enable sensor already done");
                    return (-2);
                }

                cmEnabledSensorComunicationMap = new Dictionary<int, Sensor>();

                foreach (KeyValuePair<int, Sensor> rSensorPair in cmSensorComunicationMap) {
                    if (rSensorPair.Value.Enabled == 1) {
                        cmEnabledSensorComunicationMap.Add(rSensorPair.Key, rSensorPair.Value);
                    }
                }
            }
            catch (Exception e) {
                fConsoleWriteLine("[fSelectEnabledSensor] Exception " + e.GetType().FullName + " thrown with message " + e.Message);
                fConsoleWriteLine("[fSelectEnabledSensor] Fail to find parse equipment structure");
                return (-1);
            }


            return (0);
        }


        private int fArchimedeSendMachineHardwareConfiguration()
        {
            try
            {
                string rConfigurationMessage = cmThisPlant.ToJson().ToString();
                UInt16 rConfigurationMessageLength = (UInt16)rConfigurationMessage.Length;
                int rErrorCode = TcArchimedeLibraryExtensions.fArchimedeSendEquipmentData(rConfigurationMessage, rConfigurationMessageLength);
                if (rErrorCode < 0)
                {
                    fConsoleWriteLine("[fArchimedeSendMachineHardwareConfiguration] Fail to send machine hardware configuration");
                    return (-1);
                }
                else { 
                    fConsoleWriteLine("[fArchimedeSendMachineHardwareConfiguration] Machine hardware configuration sent"); }

                return (0);

            }
            catch (Exception e)
            {
                fConsoleWriteLine("[fArchimedeDataLoaderStartRemoteConnection] Exception " + e.GetType().FullName + " thrown with message " + e.Message);
                fConsoleWriteLine("[fArchimedeDataLoaderStartRemoteConnection] Fail to connect to Archimede");
                return (-1);
            }
        }



        private int fArchimedeStartRemoteConnection(string pSystemName, string pArchimedeLibraryConfigurationFilePath)
        {
            try
            {
                int rErrorCode =  TcArchimedeLibraryExtensions.fArchimedeInitializeClientWithConfigurationFile(pSystemName, pArchimedeLibraryConfigurationFilePath);
                if (rErrorCode < 0)
                {
                    TcArchimedeLibraryExtensions.rIsArchimedeInitialized = false;
                    fConsoleWriteLine("[fArchimedeDataLoaderStartRemoteConnection] Fail to connect to Archimede");
                    return (-1);
                }
                else { fConsoleWriteLine("[fArchimedeDataLoaderStartRemoteConnection] Connected to Archimede"); TcArchimedeLibraryExtensions.rIsArchimedeInitialized = true; }

                Thread.Sleep(2000);
                return (0);

            }
            catch (Exception e)
            {
                fConsoleWriteLine("[fArchimedeDataLoaderStartRemoteConnection] Exception " + e.GetType().FullName + " thrown with message " + e.Message);
                fConsoleWriteLine("[fArchimedeDataLoaderStartRemoteConnection] Fail to connect to Archimede");
                return (-1);
            }
        }


        private int fArchimedeReleaseRemoteConnection()
        {
            try
            {

                int rErrorCode = TcArchimedeLibraryExtensions.fArchimedeReleaseConnection();
                if (rErrorCode < 0)
                {
                    fConsoleWriteLine("[fArchimedeReleaseRemoteConnection] Fail to disconnect from Archimede");
                    return (-1);
                }
                else { fConsoleWriteLine("[fArchimedeReleaseRemoteConnection] Disconnected from Archimede"); }

                return (0);

            }
            catch (Exception e)
            {
                fConsoleWriteLine("[fArchimedeReleaseRemoteConnection] Exception " + e.GetType().FullName + " thrown with message " + e.Message);
                fConsoleWriteLine("[fArchimedeReleaseRemoteConnection] Fail to disconnect from Archimede");
                return (-1);
            }
        }


        public void fEndComunication() {

            //Force exit from control loop
            this.rpRun = false;

            //Reset Sensors
            int rErrorCode = fResetAllSensors();
            if (rErrorCode != 0) {
                fConsoleWriteLine("[fEndComunication] Reset sensors fails with error " + rErrorCode);
            } else { 
                fConsoleWriteLine("[fEndComunication] Reset sensors done"); 
            }

            

            //Close can comunication
            rErrorCode = TcPCXINTComunicationWrapper.fPcxintCanClose();
            if (rErrorCode != 0){
                fConsoleWriteLine("[fEndComunication] Close can comunication fails with error " + rErrorCode);
            } else {
                fConsoleWriteLine("[fEndComunication] Close can comunication done");
            }

            //End Archimede remote connection
            rErrorCode = fArchimedeReleaseRemoteConnection();
            if (rErrorCode < 0){
                fConsoleWriteLine("[fEndComunication] Close archimede connection fails with error " + rErrorCode);
            } else {
                fConsoleWriteLine("[fEndComunication] Close archimede connection done");
            }
        }


        public int fInitializeCanComunication() {
            //Initialize PCXINT Can comunication
            int rErrorCode = TcPCXINTComunicationWrapper.fPcxintCanInit();
            if (rErrorCode < 0)
            {
                fConsoleWriteLine("[fInitializeCanComunication] Can Init fails with error " + rErrorCode);
                return(rErrorCode);
            }



            //Open PCXINT Can comunication
            rErrorCode = TcPCXINTComunicationWrapper.fPcxintCanOpen("PCXINT", cmPCXINTSettings.rpBoardNumber, cmPCXINTSettings.rpPortNumber, cmPCXINTSettings.rpSpeed, cmPCXINTSettings.rpTimeout);
            if (rErrorCode != 0)
            {
                fConsoleWriteLine("[fInitializeCanComunication] Can Open fails with error " + rErrorCode);
                return (rErrorCode);
            }

                
            //Clear PCXINT Can queue for each Device in order to cancel residual Can messages
            foreach (int rCanId in cmEnabledSensorComunicationMap.Keys)
            {
                TcPCXINTComunicationWrapper.fPcxintCanEmptyRxQueue(rCanId);
            }


            //Chech if Can port is connected
            bool rCanConnected = TcPCXINTComunicationWrapper.fPcxintCanIsPortConnected();
            if (!rCanConnected)
            {
                fConsoleWriteLine("[fInitializeCanComunication] Can port is not connected");
                rErrorCode = -1;
                return (rErrorCode);
            }

            return (rErrorCode);
        }


        public void fInitializeSensorDataGrid(){
            foreach (KeyValuePair<int, Sensor> cSensorPair in cmEnabledSensorComunicationMap)
            {
                Sensor cSensor = cSensorPair.Value;
                if (cmEnvironmentRealtimeObservableMeasureData != null || cmEnvironmentRealtimeObservableMeasureData.Count == 0) {
                    cmEnvironmentRealtimeObservableMeasureData.Add(new EnvironmentMeasureRealtime(cSensor.Position, 0, 0, 0));
                }
                if (cmKinematicObservableSensorMeasureData != null || cmKinematicObservableSensorMeasureData.Count == 0) {
                    cmKinematicObservableSensorMeasureData.Add(new KinematicMeasureRealtime(cSensor.Position, 0));
                }
                if (cmEnvironmentStatisticsObservableMeasureData != null || cmEnvironmentStatisticsObservableMeasureData.Count == 0)
                {
                    cmEnvironmentStatisticsObservableMeasureData.Add(new EnvironmentMeasureStatistics(cSensor.Position));
                }

                if (cmKinematicStatisticsObservableMeasureData != null || cmKinematicStatisticsObservableMeasureData.Count == 0)
                {
                    cmKinematicStatisticsObservableMeasureData.Add(new KinematicMeasureStatistics(cSensor.Position));
                }


            }



        }

        public int fInit()
        {
            try
            {

                //Read MachineHW Configuration json file
                string rMachineHardwareConfigurationFile = rmMachineHardwareConfigurationIniFilePath + rmMachineHardwareConfigurationFileName;
                int rErrorCode = fAcquireMachineHardwareConfiguration(rMachineHardwareConfigurationFile);
                if (rErrorCode == -1) {
                    fConsoleWriteLine("[fInit] Fail to read machine_hwconfig.json file with error " + rErrorCode);
                    return(rErrorCode);
                }

                //Select enabled sensors from MachineHW Configuration json structure
                rErrorCode = fSelectEnabledSensor();
                if (rErrorCode == -1)
                {
                    fConsoleWriteLine("[fInit] Fail to select enabled sensors with error " + rErrorCode);
                    return (rErrorCode);
                }


                //Initialize sensor offset map in order to adjust measure values 
                rErrorCode = fInitializeSensorCorrectionOffsetMap();
                if (rErrorCode == -1)
                {
                    fConsoleWriteLine("[fInit] Fail to initialize sensor offset map with error " + rErrorCode);
                    return (rErrorCode);
                }


                fInitializeSensorDataGrid();

                rErrorCode = fInitializeCanComunication();
                if (rErrorCode != 0)
                {
                    fConsoleWriteLine("[fInit] Can initialization fails with error " + rErrorCode);
                    return (rErrorCode);
                }

                fConsoleWriteLine("[fInit] Resetting Sensors...");
                //Reset all sensors by sending acquire stop command
                rErrorCode = fResetAllSensors();
                if (rErrorCode != 0)
                {
                    fConsoleWriteLine("[fInit] Reset active sensors fails with error " + rErrorCode);
                    return (rErrorCode);
                }

                //Setup each sensor with default configuration parameters
                rErrorCode = fSensorSetupAll();
                if (rErrorCode != 0) {
                    fConsoleWriteLine("[fInit] Setup fails with error " + rErrorCode);
                    return (rErrorCode);
                }

                //Init Archimede remote connection
                rErrorCode = fArchimedeStartRemoteConnection("SensorDataLoader", rmArchimedeIniFilePath + rmArchimedeIniFileName);
                if (rErrorCode != 0)
                {

                    fConsoleWriteLine("[fInit] Archimede remote connection fails with error " + rErrorCode);
                    return (rErrorCode);
                }

                //Send Hardware configuration to Archimede
                rErrorCode = fArchimedeSendMachineHardwareConfiguration();
                if (rErrorCode != 0)
                {
                    fConsoleWriteLine("[fInitComunication] Machine hardware configuration send fails with error " + rErrorCode);
                    return (rErrorCode);
                }


                fConsoleWriteLine("[fInit] Initialization successfull");
                this.rpInitSuccess = true;
                this.rpRun = true;
                rErrorCode = 0;
                return(rErrorCode);
            }
            catch (Exception e)
            {
                fConsoleWriteLine("[fInit] Exception " + e.GetType().FullName + " thrown with message " + e.Message);
                this.rpInitSuccess = false;
                this.rpRun = false;
                return(-1);
            }
        }

        

        private void fConsoleWriteLine(string pMessage)
        {
            if (pMessage != ""){
                pMessage = "[" + DateTime.Now.ToLongTimeString() + "]" + pMessage;
            }

            if (cmLogBook != null){
                cmLogBook.fDebug(pMessage);
            }

            if (rmConsoleEnable) {
                Console.WriteLine(pMessage);
            }   
        }
        

        private int fSensorAcquireAll(SubCommand pSubcommand)
        {
            foreach (KeyValuePair<int, Sensor> cSensorPair in cmEnabledSensorComunicationMap)
            {
                foreach (PhysicalProperty cPhysicalProperty in cSensorPair.Value.PhysicalProperties) {

                    if (cPhysicalProperty.Enabled == 1) {
                        Command rCommand = (Command) fGetCommand(cPhysicalProperty.MeasureType, TcCommand.kAcquire).Value;
                        if (fSensorAcquire(cSensorPair.Key, rCommand, pSubcommand) < 0)
                        {
                            fConsoleWriteLine("[fSensorAcquireAll] Acquire command " + rCommand + " fails on CanId " + cSensorPair.Key);
                        }
                        Thread.Sleep(30);
                    }
                }
            }
            return (0);
        }

        private int fResetAllSensors()
        {

            int rErrorCode = 0;
            int rReturnStatus = 0;


           
            List<Command> cAcquireCommands = new List<Command>();
            cAcquireCommands.Add((Command)fGetCommand(Temperature.MeasureType, TcCommand.kAcquire).Value);
            cAcquireCommands.Add((Command)fGetCommand(Humidity.MeasureType, TcCommand.kAcquire).Value);
            cAcquireCommands.Add((Command)fGetCommand(Pressure.MeasureType, TcCommand.kAcquire).Value);
            cAcquireCommands.Add((Command)fGetCommand(Acceleration.MeasureType, TcCommand.kAcquire).Value);
            cAcquireCommands.Add((Command)fGetCommand(Gyroscope.MeasureType, TcCommand.kAcquire).Value);
            cAcquireCommands.Add((Command)fGetCommand(MagneticField.MeasureType, TcCommand.kAcquire).Value);

            TcPCXINTComunicationWrapper.fPcxintCanSync();
            
            //foreach (KeyValuePair<int, Sensor> cSensorPair in cmEnabledSensorComunicationMap) {
            foreach (KeyValuePair<int, Sensor> cSensorPair in cmSensorComunicationMap) {
                int rCanId = cSensorPair.Key;
                Sensor cSensor = cSensorPair.Value;
                int rNumOfStops = 0;
                foreach (Command rCommand in cAcquireCommands)
                {
                    byte[] aBufferOut = new byte[2];
                    aBufferOut[0] = (byte)rCommand;
                    aBufferOut[1] = (byte)fGetGeneralCommand(TcCommand.kGeneralAcquireStop).Value;

                    rErrorCode = TcPCXINTComunicationWrapper.fPcxintCanTxCommand(rCanId, aBufferOut.Length, aBufferOut);
                    if (rErrorCode < 0)
                    {
                        rReturnStatus = -1;
                        fConsoleWriteLine("[fResetAllSensors] Send Acquire Start Stop command (cmd=" + rCommand + ", subcmd=" + SubCommand.scmdAcquireStop + ") fails with error " + rErrorCode + " on CanId " + rCanId);
                        return (rReturnStatus);
                    } else {
                        rNumOfStops++;
                        fConsoleWriteLine("[fResetAllSensors] Send Acquire Start Stop command (cmd=" + rCommand + ", subcmd=" + SubCommand.scmdAcquireStop + ") on CanId " + rCanId);

                    }

                    Thread.Sleep(10);
                }





                while (rNumOfStops > 0)
                {
                    byte[] aBufferIn = new byte[128];
                    int rBufferInLength = 0;

                    rErrorCode = TcPCXINTComunicationWrapper.fPcxintCanRxCommand(rCanId, aBufferIn.Length, ref aBufferIn, ref rBufferInLength, 2000);
                    if (rErrorCode < 0)
                    {
                        fConsoleWriteLine("[fResetAllSensors] Read fails with error " + rErrorCode + " on CanId " + rCanId);
                    }
                    else if (rErrorCode == 0)
                    {
                        Command rCommand = (Command)aBufferIn[0];
                        Error rAcquireStatus = (Error)aBufferIn[1];

                        if (cAcquireCommands.Contains<Command>(rCommand))
                        {
                            if (rAcquireStatus == Error.Ack)
                            {
                                rNumOfStops--;
                                fConsoleWriteLine("[fResetAllSensors] Received Acquire Start Stop Ack command (cmd=" + rCommand + ", status=" + rAcquireStatus + ") on CanId " + rCanId);
                            }
                            else if (rAcquireStatus == Error.Fail)
                            {
                                fConsoleWriteLine("[fResetAllSensors] Received Acquire Start Stop Fail command (cmd=" + rCommand + ", status=" + rAcquireStatus + ") on CanId " + rCanId);
                                rReturnStatus = -1;
                                return (rReturnStatus);
                            }
                        }
                    }
                }
            }

            rReturnStatus = 0;
            return (rReturnStatus);
        }


        private void fthSendSensorMeasure(Measure pMeasure, ref int pResult) {

            fConsoleWriteLine("[fthSendSensorMeasure] Send " + pMeasure.MeasureType + " measure value " + pMeasure.Value + pMeasure.MeasureUnit + " from " + pMeasure.ReferenceType + " device with ID " + pMeasure.ReferenceID);
            try { 
            
                string rMeasureMessage = pMeasure.fGetJsonSerializedString();
                UInt16 rMessageLength = (UInt16) rMeasureMessage.Length;
                if (TcArchimedeLibraryExtensions.rIsArchimedeInitialized)
                {
                    int rErrorCode = TcArchimedeLibraryExtensions.fArchimedeSendMeasureData(rMeasureMessage, rMessageLength);
                    if (rErrorCode < 0)
                    {
                        fConsoleWriteLine("[fthSendSensorMeasure] Fails to send measure to Archimede");
                        pResult = -1;
                        return;
                    }
                    else
                    {
                        pResult = 0;
                        fConsoleWriteLine("[fthSendSensorMeasure] Measure sent correctly");
                    }
                }
            }
            catch (Exception e)
            {
                fConsoleWriteLine("[fthSendSensorMeasure] Exception " + e.GetType().FullName + " thrown with message " + e.Message);
                return;
            }

        }

        private void fUpdateDataInDataGridSource(Sensor pSensor, Measure pMeasure) {
            //Update Environmental ListView
            if (pMeasure.MeasureType == Temperature.MeasureType || pMeasure.MeasureType == Humidity.MeasureType || pMeasure.MeasureType == Pressure.MeasureType)
            {
                EnvironmentMeasureRealtime cEnvironmentDataToUpdate = cmEnvironmentRealtimeObservableMeasureData
                    .Single((pSensorData) => { return (pSensorData.Position == pSensor.Position); });

                if (pMeasure.MeasureType == Temperature.MeasureType) {
                    cEnvironmentDataToUpdate.Temperature = pMeasure.Value.ToString();
                }
                else if (pMeasure.MeasureType == Humidity.MeasureType)
                {
                    cEnvironmentDataToUpdate.Humidity = pMeasure.Value.ToString();
                }
                else if (pMeasure.MeasureType == Pressure.MeasureType)
                {
                    cEnvironmentDataToUpdate.Pressure = pMeasure.Value.ToString();
                }
            } 
            else if (pMeasure.MeasureType == Acceleration.Dimensions[Acceleration.NDimensions-1]) {
                KinematicMeasureRealtime cKinematicDataToUpdate = cmKinematicObservableSensorMeasureData
                .Single((pSensorData) => { return (pSensorData.Position == pSensor.Position); });

                cKinematicDataToUpdate.Inclination = ((180/Math.PI) * Math.Acos((double)pMeasure.Value)).ToString();

            }
        }

        private int fthDispatchMeasure(Sensor pSensor, Command pCommand, float[] pValues) {
            try
            {
                string rResolvedMeasureType = fGetMeasureTypeByCommand((int)pCommand);

                
                PhysicalProperty rPhysicalProperty = pSensor.PhysicalProperties.Find((PhysicalProperty) => { return (PhysicalProperty.MeasureType == rResolvedMeasureType); });
                int rNumOfMeasures = pValues.Length;
                DateTime cDateTimeNow = DateTime.UtcNow;


                if (rNumOfMeasures != 1 && rNumOfMeasures != 3)
                {
                    fConsoleWriteLine("[fthDispatchCommand] Number of physical dimensions are wrong, " + rNumOfMeasures + " values for measure");
                    return (-1);
                }

                for (int i = 0; i < rNumOfMeasures; i++)
                {
                    Measure cMeasure = new Measure();
                    PhysicalMeasure cPhysicalMeasure = fGetPhysicalMeasureByMeasureType(rPhysicalProperty.MeasureType);
                    cMeasure.MeasureType = cPhysicalMeasure.Dimensions[i];
                    cMeasure.MeasureUnit = rPhysicalProperty.MeasureUnit;
                    cMeasure.Value = pValues[i] + (this.cmSensorCorrectionOffsetMap[pSensor].ContainsKey(rPhysicalProperty) ? this.cmSensorCorrectionOffsetMap[pSensor][rPhysicalProperty] : 0);
                    cMeasure.ReferenceID = pSensor.ReferenceID;
                    cMeasure.ReferenceType = rmReferenceType;
                    cMeasure.fSetTimeStamp(cDateTimeNow);

                    
                    fUpdateDataInDataGridSource(pSensor, cMeasure);

                    int rErrorCode = fSendSensorMeasure(cMeasure);
                    if (rErrorCode < 0)
                    {
                        fConsoleWriteLine("[fthDispatchCommand] Send sensor measure fails to start");
                        return (-1);
                    }
                }


                return (0);
            }
            catch (Exception e)
            {
                fConsoleWriteLine("[fthDispatchCommand] Exception " + e.GetType().FullName + " thrown with message " + e.Message);
                return(-1);
            }

        }
        
        private int fSendSensorMeasure(Measure pMeasure) {

            int rResult = 0;

            Thread thSender = new Thread(() => fthSendSensorMeasure(pMeasure, ref rResult));
            
            try
            {
                thSender.Priority = ThreadPriority.Highest;
                thSender.IsBackground = true;
                thSender.Name = "Sensor " + pMeasure.ReferenceID.ToString() + " Measure Sender";
                thSender.Start();
                thSender.Join(10000);

                if (rResult == 0) {
                    fConsoleWriteLine("[fSendSensorMeasure] Sent " + pMeasure.MeasureType + " measure value " + pMeasure.Value + pMeasure.MeasureUnit + " from " + pMeasure.ReferenceType + " device with ID " + pMeasure.ReferenceID);
                    return (rResult);
                } else {
                    fConsoleWriteLine("[fSendSensorMeasure] Not Sent " + pMeasure.MeasureType + " measure value " + pMeasure.Value + pMeasure.MeasureUnit + " from " + pMeasure.ReferenceType + " device with ID " + pMeasure.ReferenceID);
                    return (rResult);
                }
            }
            catch (Exception e)
            {
                fConsoleWriteLine("[fSendSensorMeasure] Exception " + e.GetType().FullName + " thrown with message " + e.Message);
                thSender.Abort();
                return (rResult);
            }
        }
        
        private int fDispatchMeasure(Sensor pSensor, float[] pValues, Command pCommand)
        {

            Thread thCommandDispatcher = new Thread(() => fthDispatchMeasure(pSensor, pCommand, pValues));

            try
            {
                thCommandDispatcher.Priority = ThreadPriority.Highest;
                thCommandDispatcher.IsBackground = true;
                thCommandDispatcher.Name = "Command Dispatcher";
                thCommandDispatcher.Start();
                return (0);
            }
            catch (Exception e)
            {
                fConsoleWriteLine("[fDispatchCommand] Exception " + e.GetType().FullName + " thrown with message " + e.Message);
                thCommandDispatcher.Abort();
                return (-1);
            }
        }
        
        private void fthMonitoring()
        {

            int rErrorCode = 0;
            fConsoleWriteLine("[fthMonitoring] Monitoring start");

            try
            {
                if (fSensorAcquireAll(SubCommand.scmdAcquireStart) < 0)
                {
                    fConsoleWriteLine("[fthMonitoring] Fail to acquire all enabled sensors");
                    return;
                }


                while (this.rpRun)
                {
                    foreach (KeyValuePair<int, Sensor> cSensorPair in cmEnabledSensorComunicationMap)
                    {
                        int rCanId = cSensorPair.Key;
                        Sensor cSensor = cSensorPair.Value;

                        float[] rValues;
                        Command rCommand;

                        if ((rErrorCode = fAsyncRead(rCanId, out rCommand, out rValues)) < 0){
                            fConsoleWriteLine("[fthMonitoring] Fail to read async data with error " + rErrorCode + "on CanId " + rCanId);
                        } else {
                            if ((rErrorCode = fDispatchMeasure(cSensor, rValues, rCommand)) < 0){
                                fConsoleWriteLine("[fthMonitoring] Fail to dispatch command " + rCommand + " with error " + rErrorCode + " on CanId " + rCanId);
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception e) {
                fConsoleWriteLine("[fthMonitoring] Exception " + e.GetType().FullName + " thrown with message " + e.Message);
                fEndComunication();
            }

            fConsoleWriteLine("[fthMonitoring] Exit from Monitoring Mode ");
            return;
        }
        
        public void fStartMonitoring()
        {
            fConsoleWriteLine("[fStartMonitoring] Start Monitoring Mode");
            Thread thMonitoring = new Thread(() => fthMonitoring());
            thMonitoring.Priority = ThreadPriority.Highest;
            thMonitoring.IsBackground = true;
            thMonitoring.Name = "Start Monitor";
            thMonitoring.Start();
        }


       
        private int fAsyncRead(int pCanId, out Command pCommand, out float[] pValues)
        {

            byte[] aBufferIn = new byte[1024];
            int rReturnStatus = 0;
            Command rCommand;
            Error rReadStatus = Error.Fail;
            int rMeasuresToRead = 0;
            bool rCommandReceived = false;
            int rBufferInLength = 0;
            int rErrorCode = 0;
            try
            {
                while (!rCommandReceived)
                {
                    rErrorCode = TcPCXINTComunicationWrapper.fPcxintCanRxCommand(pCanId, aBufferIn.Length, ref aBufferIn, ref rBufferInLength, 1000);
                    if (rErrorCode != 0)
                    {
                        fConsoleWriteLine("[fAsyncRead] Read no command on CanId " + pCanId);
                        rReturnStatus = -1;
                        pCommand = 0;
                        pValues = new float[0];
                        return (rReturnStatus);
                    }
                    else if (rBufferInLength != (2 * sizeof(byte) + sizeof(float)) && rBufferInLength != (2 * sizeof(byte) + 3 * sizeof(float)))
                    {
                        fConsoleWriteLine("[fAsyncRead] Read corrupted command of length " + rBufferInLength + " on CanId " + pCanId);
                    }
                    else
                    {
                        fConsoleWriteLine("[fAsyncRead] Read command on CanId " + pCanId);
                        rCommandReceived = true;
                    }
                }

                rCommand = (Command)aBufferIn[0];
                pCommand = rCommand;
                rReadStatus = (Error)aBufferIn[1];

                if (rReadStatus == Error.Fail)
                {
                    pValues = new float[0];
                    rReturnStatus = -1;
                    return (rReturnStatus);
                }

                switch (rCommand)
                {
                    case Command.cmdTempRead:
                        rMeasuresToRead = 1;
                        break;
                    case Command.cmdHumRead:
                        rMeasuresToRead = 1;
                        break;
                    case Command.cmdPressRead:
                        rMeasuresToRead = 1;
                        break;
                    case Command.cmdAccRead:
                        rMeasuresToRead = 3;
                        break;
                    case Command.cmdGyroRead:
                        rMeasuresToRead = 3;
                        break;
                    case Command.cmdMagRead:
                        rMeasuresToRead = 3;
                        break;
                }


                if (rBufferInLength != ((sizeof(byte) * 2) + (sizeof(float) * rMeasuresToRead)))
                {
                    pValues = new float[0];
                    rReturnStatus = -1;
                    fConsoleWriteLine("[fAsyncRead] Can message to small on CanId " + pCanId);
                    return (rReturnStatus);
                }


                pValues = new float[rMeasuresToRead];
                int i = 0;
                for (i = 0; i < rMeasuresToRead; i++)
                {
                    pValues[i] = BitConverter.ToSingle(aBufferIn, 2 * sizeof(byte) + i * sizeof(float));
                }

                rReturnStatus = 0;
                return (rReturnStatus);
            }
            catch (Exception e)
            {
                pValues = new float[0];
                pCommand = 0;
                fConsoleWriteLine("[fAsyncRead] Exception " + e.GetType().FullName + " thrown with message " + e.Message);
                rReturnStatus = -1;
                return (rReturnStatus);
            }

        }
        

        private int fSensorSetupAll()
        {
            foreach (KeyValuePair<int, Sensor> cSensorPair in cmEnabledSensorComunicationMap)
            {
                int rCanId = cSensorPair.Key;
                Sensor cSensor = cSensorPair.Value;
                List<PhysicalProperty> cPhysicalProperties = cSensorPair.Value.PhysicalProperties;
                foreach (PhysicalProperty cPhysicalProperty in cPhysicalProperties)
                {
                   
                    TcCommand cCommand = fGetCommand(cPhysicalProperty.MeasureType, TcCommand.kSetup);

                    Command cSetupCommand = (Command) cCommand.Value;
                    if (cPhysicalProperty.Enabled == 1)
                    {
                        int rErrorCode = fSetupPhysicalProperty(cPhysicalProperty, rCanId);
                        if (rErrorCode < 0) {
                            fConsoleWriteLine("[fSensorSetupAll] Setup command " + cSetupCommand + " with odr " + cPhysicalProperty.OutputDataRate + " fails on CanId " + rCanId);
                        }
                        else {
                            fConsoleWriteLine("[fSensorSetupAll] Setup command " + cSetupCommand + " with odr " + cPhysicalProperty.OutputDataRate + " success on CanId " + rCanId);
                        }
                        Thread.Sleep(30);
                    }
                }
            }
            return (0);
                
        }

        private int fSetupPhysicalProperty(PhysicalProperty pPhysicalProperty, int pCanId)
        {
            int rErrorCode;
            if (pPhysicalProperty == null || pCanId < 0) {
                rErrorCode = -1;
                return (rErrorCode);
            }


           
            TcCommand cCommand = fGetCommand(pPhysicalProperty.MeasureType, TcCommand.kSetup);
            Command cSetupCommand = (Command)cCommand.Value;

            switch (pPhysicalProperty.MeasureType) {
                case Temperature.MeasureType:
                    rErrorCode = fSensorSetup(pCanId, cSetupCommand, (OutputDataRate)pPhysicalProperty.OutputDataRate, (AverageNumber)pPhysicalProperty.AverageNumber);
                    break;
                case Humidity.MeasureType:
                    rErrorCode = fSensorSetup(pCanId, cSetupCommand, (OutputDataRate)pPhysicalProperty.OutputDataRate, (AverageNumber)pPhysicalProperty.AverageNumber);
                    break;
                case Pressure.MeasureType:
                    rErrorCode = fSensorSetup(pCanId, cSetupCommand, (OutputDataRate)pPhysicalProperty.OutputDataRate, (Bandwith)pPhysicalProperty.Bandwidth);
                    break;
                case Gyroscope.MeasureType:
                    rErrorCode = fSensorSetup(pCanId, cSetupCommand, (OutputDataRate)pPhysicalProperty.OutputDataRate, (Scale)pPhysicalProperty.Scale);
                    break;
                case MagneticField.MeasureType:
                    rErrorCode = fSensorSetup(pCanId, cSetupCommand, (OutputDataRate)pPhysicalProperty.OutputDataRate, (LPF)pPhysicalProperty.LowPassFilter);
                    break;
                case Acceleration.MeasureType:
                    rErrorCode = fSensorSetup(pCanId, cSetupCommand, (OutputDataRate)pPhysicalProperty.OutputDataRate, (Scale)pPhysicalProperty.Scale);
                    break;
                case Light.MeasureType:
                    rErrorCode = fSensorSetup(pCanId, cSetupCommand, (OutputDataRate)pPhysicalProperty.OutputDataRate, (AverageNumber)pPhysicalProperty.AverageNumber);
                    break;
                case Audio.MeasureType:
                    rErrorCode = fSensorSetup(pCanId, cSetupCommand, (OutputDataRate)pPhysicalProperty.OutputDataRate, (AverageNumber)pPhysicalProperty.AverageNumber);
                    break;
                default:
                    rErrorCode = -1;
                    break;
            }
            return (rErrorCode);
        }

        //private int fthSendSensorMeasureBlock(Sensor pSensor, PhysicalProperty pPhysicalProperty, float[][] pValues)
        //{
        //    try
        //    {
        //        int rNumOfDimension = pValues[0].Length;
        //        DateTime cDateTimeNow = DateTime.UtcNow;
        //        string[] aAxesLable = { "_x", "_y", "_z" };

        //        if (rNumOfDimension != 1 && rNumOfDimension != 3)
        //        {
        //            fConsoleWriteLine("[fthSendSensorMeasureBlock] Number of physical dimensions are wrong, " + rNumOfDimension + " values for measure");
        //            return (-1);
        //        }

        //        for (int i = 0; i < pValues.Length; i++)
        //        {
        //            for (int j = 0; j < pValues[i].Length; j++)
        //            {
        //                Measure rMeasure = new Measure();
        //                rMeasure.MeasureType = pPhysicalProperty.MeasureType + (rNumOfDimension == 3 ? aAxesLable[i] : "");
        //                rMeasure.MeasureUnit = pPhysicalProperty.MeasureUnit;
        //                rMeasure.Value = pValues[i][j];
        //                rMeasure.ReferenceID = pSensor.ReferenceID;
        //                rMeasure.ReferenceType = rmReferenceType;
        //                rMeasure.fSetTimeStamp(cDateTimeNow);

        //                int rErrorCode = fSendSensorMeasure(rMeasure);
        //                if (rErrorCode < 0)
        //                {
        //                    fConsoleWriteLine("[fthSendSensorMeasureBlock] Send sensor measure fails to start");
        //                    return (-1);
        //                }
        //            }
        //        }
        //        return (0);
        //    }
        //    catch (Exception e)
        //    {
        //        fConsoleWriteLine("[fthSendSensorMeasureBlock] Exception " + e.GetType().FullName + " thrown with message " + e.Message);
        //        return (-1);
        //    }

        //}



        //private int fSendSensorMeasureBlock(Sensor pSensor, PhysicalProperty pPhysicalProperty, float[][] pValues)
        //{

        //    Thread thSensorMeasureBlockSender = new Thread(() => fthSendSensorMeasureBlock(pSensor, pPhysicalProperty, pValues));

        //    try
        //    {
        //        thSensorMeasureBlockSender.Priority = ThreadPriority.Highest;
        //        thSensorMeasureBlockSender.IsBackground = true;
        //        thSensorMeasureBlockSender.Name = "Sensor Measure Block Sender";
        //        thSensorMeasureBlockSender.Start();
        //        return (0);
        //    }
        //    catch (Exception e)
        //    {
        //        fConsoleWriteLine("[fSendSensorMeasureBlock] Exception " + e.GetType().FullName + " thrown with message " + e.Message);
        //        thSensorMeasureBlockSender.Abort();
        //        return (-1);
        //    }
        //}




        private int fDispatchMeasureBlock(UInt64 pStartAcquireTime, Sensor pSensor, PhysicalProperty pPhysicalProperty, float[][] pValues)
        {

           

            Thread thCommandDispatcher = new Thread(() => fthDispatchMeasureBlock(pStartAcquireTime, pSensor, pPhysicalProperty, pValues));

            try
            {
                thCommandDispatcher.Priority = ThreadPriority.Highest;
                thCommandDispatcher.IsBackground = true;
                thCommandDispatcher.Name = "Command Dispatcher";
                thCommandDispatcher.Start();
                return (0);
            }
            catch (Exception e)
            {
                fConsoleWriteLine("[fDispatchMeasureBlock] Exception " + e.GetType().FullName + " thrown with message " + e.Message);
                thCommandDispatcher.Abort();
                return (-1);
            }
        }

        private int fthDispatchMeasureBlock(UInt64 pStartAcquireTime,  Sensor pSensor, PhysicalProperty pPhysicalProperty , float[][] pValues)
        {
            try
            {

                if (pValues == null || pValues.Length == 0 || pValues[0] == null) {
                    return (-1);
                }

                int rNumOfDimensions = pValues[0].Length;
                if (rNumOfDimensions != 1 && rNumOfDimensions != 3)
                {
                    fConsoleWriteLine("[fthDispatchMeasureBlock] Number of physical dimensions are wrong, " + rNumOfDimensions + " values for measure");
                    return (-1);
                }

                DateTimeOffset cDateTimeNow = DateTimeOffset.FromUnixTimeMilliseconds((long)pStartAcquireTime);
               
               
                for (int i = 0; i < pValues.Length; i++)
                {
                    for (int j = 0; j < pValues[i].Length; j++)
                    {
                        Measure cMeasure = new Measure();
                        try {
                            PhysicalMeasure cPhysicalMeasure = fGetPhysicalMeasureByMeasureType(pPhysicalProperty.MeasureType);
                            cMeasure.MeasureType = cPhysicalMeasure.Dimensions[j];
                            cMeasure.MeasureUnit = pPhysicalProperty.MeasureUnit;
                            cMeasure.Value = pValues[i][j] + (this.cmSensorCorrectionOffsetMap[pSensor].ContainsKey(pPhysicalProperty) ? this.cmSensorCorrectionOffsetMap[pSensor][pPhysicalProperty] : 0);
                            cMeasure.ReferenceID = pSensor.ReferenceID;
                            cMeasure.ReferenceType = rmReferenceType;
                            ulong rOffset = (ulong)i * cPhysicalMeasure.OutputDataRate[(int)pPhysicalProperty.OutputDataRate];
                            cMeasure.TimestampMicroEpoch = (ulong?)((pStartAcquireTime * 1000) + rOffset);
                            cMeasure.fSetTimeStamp(DateTimeOffset.FromUnixTimeMilliseconds((long)(((pStartAcquireTime * 1000) + rOffset) / 1000)).UtcDateTime);
                        }
                        catch (Exception e)
                        {
                            fConsoleWriteLine("[fthDispatchMeasureBlock] Exception " + e.GetType().FullName + " thrown with message " + e.Message);
                            return (-1);
                        }

                        fUpdateDataInDataGridSource(pSensor, cMeasure);

                        int rErrorCode = fSendSensorMeasure(cMeasure);
                        if (rErrorCode < 0)
                        {
                            fConsoleWriteLine("[fthDispatchMeasureBlock] Send sensor measure fails to start");
                            return (-1);
                        }
                    }
                }

                return (0);
            }
            catch (Exception e)
            {
                fConsoleWriteLine("[fthDispatchMeasureBlock] Exception " + e.GetType().FullName + " thrown with message " + e.Message);
                return (-1);
            }

        }

        public void fStartLogging()
        {
            Thread thLogging = new Thread(fthLogging);
            thLogging.Priority = ThreadPriority.Highest;
            thLogging.IsBackground = true;
            thLogging.Name = "Start Logger";
            thLogging.Start();
        }
       
        private void fthLogging()
        {
            fConsoleWriteLine("[fthLogging] Logging start");

            int rErrorCode = 0;
            Dictionary<Sensor, Dictionary<PhysicalProperty, int>> cSensorsPhysicalPropertiesTimeAcquireInterval = new Dictionary<Sensor, Dictionary<PhysicalProperty, int>>();
            Dictionary<Sensor, int> cSensorsCurrentPhysicalPropertyLogMap = new Dictionary<Sensor, int>();
            Dictionary<Sensor, int> cSensorsTimeAcquireIntervalLogMap = new Dictionary<Sensor, int>();
            Command cLogSetupCommand = (Command)fGetLogCommand(TcCommand.kLogSetup).Value;
            LogEnable cLogEnable = (LogEnable)fGetLogCommand(TcCommand.kLogEnable).Value;
            LogEnable cLogDisable = (LogEnable)fGetLogCommand(TcCommand.kLogDisable).Value;





            List<TcLoggingSensor> cLoggingSensors = new List<TcLoggingSensor>();
            foreach (Sensor cSensor in cmEnabledSensorComunicationMap.Values.ToList<Sensor>())
            {
                List<PhysicalProperty> cSensorSupportedProperties = cSensor.PhysicalProperties.FindAll((cPhysicalProperty) => { return (cPhysicalProperty.Enabled == 1); });
                cLoggingSensors.Add(new TcLoggingSensor(cSensor, cSensorSupportedProperties));
            }



            


            foreach (TcLoggingSensor cLoggingSensor in cLoggingSensors) {
                LogSensor cSensorLogNumber = (LogSensor) fGetPhysicalMeasureByMeasureType(cLoggingSensor.cpCurrent.cpProperty.MeasureType).LogNumber;
                if (!int.TryParse(cLoggingSensor.cpSensor.CanAddress, out int rCanId)) {
                    return;
                }

                if ((rErrorCode = fLogSetup(rCanId, cLogSetupCommand, cLogEnable, cSensorLogNumber)) < 0)
                {
                    fConsoleWriteLine("[fthLogging] Log Setup fails with error " + rErrorCode + " on CanId " + rCanId);
                    return;
                }
            }


            foreach (TcLoggingSensor cLoggingSensor in cLoggingSensors)
            {
                LogSensor cSensorLogNumber = (LogSensor)fGetPhysicalMeasureByMeasureType(cLoggingSensor.cpCurrent.cpProperty.MeasureType).LogNumber;
                if (!int.TryParse(cLoggingSensor.cpSensor.CanAddress, out int rCanId))
                {
                    return;
                }

                Command cTimeAcquireCommand = (Command) fGetCommand(cLoggingSensor.cpCurrent.cpProperty.MeasureType, TcCommand.kTimeAcquire).Value;
                if ((rErrorCode = fSensorAcquire(rCanId, cTimeAcquireCommand, cLoggingSensor.cpCurrent.cpProperty.Log.TimeIntervalAcquire)) < 0)
                {
                    fConsoleWriteLine("[fthLogging] Log Setup fails with error " + rErrorCode + " on CanId " + rCanId);
                    return;
                }
                cLoggingSensor.fStartLoggingCurrentProperty();
            }

            try
            {
                while (this.rpRun)
                {


                     List<TcLoggingSensor> cLoggingSensorsToBeRestarted = cLoggingSensors.FindAll(
                        (cLoggingSensor) => { 
                            return (cLoggingSensor.fIsExpired()); 
                        }
                    );


                    foreach (TcLoggingSensor cLoggingSensor in cLoggingSensorsToBeRestarted)
                    {

                        float[][] cDataLogMap;
                        if (!int.TryParse(cLoggingSensor.cpSensor.CanAddress, out int rCanId))
                        {
                            return;
                        }

                        rErrorCode = fLogReadAllSamples(rCanId, out cDataLogMap);
                        if (rErrorCode == kLogReadSamplesFail || rErrorCode == kLogReadNSamplesFail || rErrorCode == kLogNSamplesInvalid)
                        {
                            fConsoleWriteLine("[fthLogging] Log Read fails with error " + rErrorCode + " on CanId " + rCanId);
                        }
                        else if (rErrorCode == kLogNoSamples)
                        {
                            fConsoleWriteLine("[fthLogging] No samples received on CanId " + rCanId);
                        }
                        else
                        {
                            //Send the entier measure block related to a physical measure
                            if ((rErrorCode = fDispatchMeasureBlock(cLoggingSensor.cpCurrent.rpPropertyStartAcquireTime, cLoggingSensor.cpSensor, cLoggingSensor.cpCurrent.cpProperty, cDataLogMap)) < 0)
                            {
                                fConsoleWriteLine("[fthLogging] Send Sensor Measure Block fails with error " + rErrorCode + " on CanId " + rCanId);
                                return;
                            }

                        }


                        cLoggingSensor.fSwitchLoggingProperty();
                        LogSensor cSensorLogNumber = (LogSensor)fGetPhysicalMeasureByMeasureType(cLoggingSensor.cpCurrent.cpProperty.MeasureType).LogNumber;
                        if ((rErrorCode = fLogSetup(rCanId, cLogSetupCommand, cLogEnable, cSensorLogNumber)) < 0)
                        {
                            fConsoleWriteLine("[fthLogging] Log Setup fails with error " + rErrorCode + " on CanId " + rCanId);
                            return;
                        }

                        Command cTimeAcquireCommand = (Command)fGetCommand(cLoggingSensor.cpCurrent.cpProperty.MeasureType, TcCommand.kTimeAcquire).Value;
                        if ((rErrorCode = fSensorAcquire(rCanId, cTimeAcquireCommand, cLoggingSensor.cpCurrent.cpProperty.Log.TimeIntervalAcquire)) < 0)
                        {
                            fConsoleWriteLine("[fthLogging] Log Setup fails with error " + rErrorCode + " on CanId " + rCanId);
                            return;
                        }
                        cLoggingSensor.fStartLoggingCurrentProperty();






                    }   
                }




                foreach (TcLoggingSensor cLoggingSensor in cLoggingSensors)
                {

                    LogSensor cSensorLogNumber = (LogSensor)fGetPhysicalMeasureByMeasureType(cLoggingSensor.cpCurrent.cpProperty.MeasureType).LogNumber;
                    if (!int.TryParse(cLoggingSensor.cpSensor.CanAddress, out int rCanId))
                        {
                            return;
                        }

                        if ((rErrorCode = fLogSetup(rCanId, cLogSetupCommand, cLogDisable, cSensorLogNumber)) < 0)
                        {
                            fConsoleWriteLine("[fthLogging] Log Setup fails with error " + rErrorCode + " on CanId " + rCanId);
                            //return;
                        }
                  
                }



                fConsoleWriteLine("[fthLogging] Exit from Logging Mode");
                return;
            }
            catch (Exception e)
            {
                fConsoleWriteLine("[fthLogging] Exception " + e.GetType().FullName + " thrown with message " + e.Message);
                return;
            }

            return;
        }

        /*
        //Function that log in infinite loop
        private void fthLoggingSensor()
        {
            LogSensor[] cLogSensors = new LogSensor[6];
            cLogSensors[0] = LogSensor.Temp;
            cLogSensors[1] = LogSensor.Hum;
            cLogSensors[2] = LogSensor.Press;
            cLogSensors[3] = LogSensor.Acc;
            cLogSensors[4] = LogSensor.Gyro;
            cLogSensors[4] = LogSensor.Magn;


            while (rmRun)
            {
                foreach (LogSensor cSensor in cLogSensors)
                {
                    if (fLogSensor(0, 0, cSensor, 10) < 0)
                    {
                        fConsoleWriteLine("[fthLoggingSensor] Log Sensor fails on CanId");
                        return;
                    }
                }
            }
            return;
        }*/


        //Read All Available Logs
        private int fLogReadAllSamples(int pCanId, out float[][] pLoggedData)
        {
            int rSamplesNumber = 0;
            int i = 0;



            Command cLogReadCommand = (Command)fGetLogCommand(TcCommand.kLogRead).Value;
            SubCommand cLogReadNSamplesCommand = (SubCommand)fGetGeneralCommand(TcCommand.kReadNSamples).Value;

            if (TcSM100Manager.fLogNSamplesRead(pCanId, cLogReadCommand, cLogReadNSamplesCommand, out rSamplesNumber) < 0)
            {
                pLoggedData = null;
                fConsoleWriteLine("[fLogReadAll] Log Sensor fails on CanId " + pCanId);
                return (kLogReadNSamplesFail);
            }

            if (rSamplesNumber == 0)
            {
                pLoggedData = null;
                fConsoleWriteLine("[fLogReadAll] No samples number on CanId " + pCanId);
                return (kLogNoSamples);
            }
            else if (rSamplesNumber < 0)
            {
                pLoggedData = null;
                fConsoleWriteLine("[fLogReadAll] No samples number on CanId " + pCanId);
                return (kLogNSamplesInvalid);
            }

            float[][] rLoggedData = new float[rSamplesNumber][];
            pLoggedData = new float[rSamplesNumber][];


            DateTime cStartTime = DateTime.UtcNow;
            DateTime cCumTime = DateTime.UtcNow;
            for (i = 0; i < rSamplesNumber; i++)
            {
                
                if (TcSM100Manager.fLogRead(pCanId, (Command)fGetLogCommand(TcCommand.kLogRead).Value, i + 1, out rLoggedData[i], out UInt64 cLatencyMilliseconds) < 0)
                {
                    fConsoleWriteLine("[fLogReadN] Log read n. sample " + (i + 1) + " with command " + Command.cmdLogRead + " fails on CanId " + pCanId);
                    return (kLogReadSamplesFail);
                }
                fConsoleWriteLine("[fLogReadNSamples] acquisition block latency time " + cLatencyMilliseconds + "ms");
                //cCumTime.AddMilliseconds(cLatencyMilliseconds);
                pLoggedData[i] = new float[rLoggedData[i].Length];

                rLoggedData[i].CopyTo(pLoggedData[i], 0);
            }
            fConsoleWriteLine("[fLogReadNSamples] Cumulative acquisition block latency time " + (cCumTime-cStartTime));
            return (0);
        }
    }
}
