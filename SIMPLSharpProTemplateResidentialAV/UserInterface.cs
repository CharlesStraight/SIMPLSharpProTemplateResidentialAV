using System;
using Crestron.SimplSharp;                          	// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       	// For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.DeviceSupport;         	// For Generic Device Support
using Crestron.SimplSharpPro.UI;                        // For UI Devices.

namespace SIMPLSharpProTemplateResidentialAV
{
    public class UserInterface
    {
        public delegate void DigitalChangeEventHandler(uint deviceId, SigEventArgs args);
        public event DigitalChangeEventHandler digitalChangeEvent;

        public delegate void AnalogChangeEventHandler(uint deviceId, SigEventArgs args);
        public event AnalogChangeEventHandler AnalogChangeEvent;

        public delegate void SerialChangeEventHandler(uint deviceId, SigEventArgs args);
        public event SerialChangeEventHandler SerialChangeEvent;

        public delegate void SmartObjectChangeEventHandler(uint deviceId, SmartObjectEventArgs args);
        public event SmartObjectChangeEventHandler SmartObjectChangeEvent;

        public void interlock(uint setIndex, ushort startIndex, ushort endIndex, BasicTriList userInterface)
        {
            resetInterlock(startIndex, endIndex, userInterface);
            userInterface.BooleanInput[setIndex].BoolValue = true;
        }

        public void resetInterlock(ushort startIndex, ushort endIndex, BasicTriList userInterface)
        {
            for (ushort i = startIndex; i <= endIndex; i++)
            {
                userInterface.BooleanInput[i].BoolValue = false;
            }
        }

        public void resetInterlockArray(ushort[] indexes, BasicTriList userInterface)
        {
            foreach (ushort index in indexes)
            {
                userInterface.BooleanInput[index].BoolValue = false;
            }
        }


        public void userInterface_SigChange(BasicTriList currentDevice, SigEventArgs args)
        {
            switch (args.Sig.Type)
            {
                case eSigType.Bool: //Digital
                    {
                        if (args.Sig.BoolValue)
                        {
                            if (args.Sig.Number > 100 && args.Sig.Number < 111)
                            {
                                interlock(args.Sig.Number, 101, 110, currentDevice);
                            }
                            else if (args.Sig.Number == 100)
                            {
                                resetInterlock(101, 110, currentDevice);
                            }
                            else if (args.Sig.Number > 150 && args.Sig.Number < 161)
                            {
                                interlock(args.Sig.Number, 151, 160, currentDevice);
                            }
                            else if (args.Sig.Number == 150)
                            {
                                resetInterlock(151, 160, currentDevice);
                                resetInterlockArray(new ushort[] { 1, 4, 6, 8 }, currentDevice);
                            }
                        }
                        digitalChangeEvent(currentDevice.ID, args);
                        break;
                    }
                case eSigType.String:   //Strings
                    {
                        SerialChangeEvent(currentDevice.ID, args);
                        break;
                    }
                case eSigType.UShort:   //Analogs
                    {
                        AnalogChangeEvent(currentDevice.ID, args);
                        break;
                    }
            }
        }

        public void SmartObject_SigChange(GenericBase currentDevice, SmartObjectEventArgs args)
        {
            SmartObjectChangeEvent(currentDevice.ID, args);
        }
    }
}