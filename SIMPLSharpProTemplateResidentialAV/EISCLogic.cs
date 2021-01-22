using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.EthernetCommunication;


namespace SIMPLSharpProTemplate
{
    public class EISCLogic
    {
        public EthernetIntersystemCommunications eisc;
        public EISCLogic(uint ipId, string ipAddress, ControlSystem controlSystem)
        {
            eisc = new EthernetIntersystemCommunications(ipId, ipAddress, controlSystem);
            eisc.SigChange += new Crestron.SimplSharpPro.DeviceSupport.SigEventHandler(SigChange);
            if (eisc.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
            {
                ErrorLog.Error("Error registering EISC with IPID: {0} and IP address {1} Reason: {2}", ipId, ipAddress, eisc.RegistrationFailureReason);
            }
        }

        public delegate void DigitalChangeEventHandler(uint id, SigEventArgs args);
        public event DigitalChangeEventHandler digitalChangeEvent;

        public delegate void AnalogChangeEventHandler(uint id, SigEventArgs args);
        public event AnalogChangeEventHandler analogChangeEvent;

        public delegate void SerialChangeEventHandler(uint id, SigEventArgs args);
        public event SerialChangeEventHandler serialChangeEvent;        

        public void setDigitalInput(uint input, bool value)
        {
            eisc.BooleanInput[input].BoolValue = value;
        }

        public void setAnalogInput(uint input, ushort value)
        {
            eisc.UShortInput[input].UShortValue = value;
        }

        public void setStringInput(uint input, string value)
        {
            eisc.StringInput[input].StringValue = value;
        }

        void SigChange(Crestron.SimplSharpPro.DeviceSupport.BasicTriList currentDevice, SigEventArgs args)
        {
            switch(args.Sig.Type)
            {
                case eSigType.Bool:
                    {
                        digitalChangeEvent(currentDevice.ID, args);
                        break;
                    }
                case eSigType.UShort:
                    {
                        analogChangeEvent(currentDevice.ID, args);
                        break;
                    }
                case eSigType.String:
                    {
                        serialChangeEvent(currentDevice.ID, args);
                        break;
                    }
            }
        }
    }
}