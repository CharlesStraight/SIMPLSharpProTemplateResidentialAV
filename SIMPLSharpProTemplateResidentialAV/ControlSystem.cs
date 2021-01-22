using System;
using System.Collections.Generic;
using Crestron.SimplSharp;                          	// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       	// For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.CrestronThread;        	// For Threading
using Crestron.SimplSharpPro.Diagnostics;		    	// For System Monitor Access
using Crestron.SimplSharpPro.DeviceSupport;         	// For Generic Device Support
using Crestron.SimplSharpPro.UI;                        // For UI Devices.
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;


namespace SIMPLSharpProTemplate
{
    public class ControlSystem : CrestronControlSystem
    {
        private Tsw750 userInterface;
        private Tsw750 userInterface2;
        private UserInterface uiLogic;

        private DMPS3AVControl avControl;
        private DmTx201CLogic dmTx1;

        private DmRmc100CInfraredPortLogic appleTvIr;
        private DmRmc100CInfraredPortLogic blurayIr;

        private DmRmc100CSerialPortLogic display1Serial;

        private EISCLogic program2Eisc;
        private DMPS3AudioLevelControlLogic audioLevelControl;
        
        public ControlSystem()
            : base()
        {
            try
            {
                Thread.MaxNumberOfUserThreads = 20;

                //Subscribe to the controller events (System, Program, and Ethernet)
                CrestronEnvironment.SystemEventHandler += new SystemEventHandler(ControlSystem_ControllerSystemEventHandler);
                CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(ControlSystem_ControllerProgramEventHandler);
                CrestronEnvironment.EthernetEventHandler += new EthernetEventHandler(ControlSystem_ControllerEthernetEventHandler);
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in the constructor: {0}", e.Message);
            }
        }

        



        /// <summary>
        /// InitializeSystem - this method gets called after the constructor 
        /// has finished. 
        /// 
        /// Use InitializeSystem to:
        /// * Start threads
        /// * Configure ports, such as serial and verisports
        /// * Start and initialize socket connections
        /// Send initial device configurations
        /// 
        /// Please be aware that InitializeSystem needs to exit quickly also; 
        /// if it doesn't exit in time, the SIMPL#Pro program will exit.
        /// </summary>
        public override void InitializeSystem()
        {
            try
            {
                if(SupportsSwitcherOutputs)
                {
                    avControl = new DMPS3AVControl(SwitcherInputs, SwitcherOutputs);
                    DMOutputChange += new Crestron.SimplSharpPro.DM.DMOutputEventHandler(avControl.ControlSystem_DMOutputChange);
                    avControl.dmps3SwitchEvent += new DMPS3AVControl.DMPS3SwitchEventHandler(avControl_dmps3SwitchEvent);
                    avControl.dmps3ClearSwitchEvent += new DMPS3AVControl.DMPS3ClearSwitchEventHandler(avControl_dmps3ClearSwitchEvent);

                    DmTx201C dmTx1Device = new DmTx201C(SwitcherInputs[(uint)eDmps34K150CInputs.Dm1] as DMInput);
                    dmTx1 = new DmTx201CLogic(dmTx1Device);
                    dmTx1.DmTx201AudioSourceChangeEvent += new DmTx201CLogic.DmTx201AudioSourceChangeEventHandler(dmTx1_DmTx201AudioSourceChangeEvent);
                    dmTx1.DmTx201VideoSourceChangeEvent += new DmTx201CLogic.DmTx201VideoSourceChangeEventHandler(dmTx1_DmTx201VideoSourceChangeEvent);

                    DmRmc100C dmRx1Device = new DmRmc100C(SwitcherOutputs[(uint)eDmps34K150COutputs.DmHdmiAnalogOutput] as DMOutput);
                    appleTvIr = new DmRmc100CInfraredPortLogic(dmRx1Device, 1, @"\NVRAM\Apple AppleTV.ir", true);
                    blurayIr = new DmRmc100CInfraredPortLogic(dmRx1Device, 2, @"\NVRAM\Bluray.ir", true);
                    display1Serial = new DmRmc100CSerialPortLogic(dmRx1Device, 1, ComPort.eComBaudRates.ComspecBaudRate9600, ComPort.eComDataBits.ComspecDataBits8, ComPort.eComHardwareHandshakeType.ComspecHardwareHandshakeNone, ComPort.eComParityType.ComspecParityNone, ComPort.eComStopBits.ComspecStopBits1, "\n");
                    display1Serial.SerialDataReceivedEvent += new DmRmc100CSerialPortLogic.SerialDataReceivedEventHandler(display1Serial_SerialDataReceivedEvent);

                    audioLevelControl = new DMPS3AudioLevelControlLogic(SwitcherOutputs[(uint)eDmps34K150COutputs.DmHdmiAnalogOutput] as Crestron.SimplSharpPro.DM.Cards.Card.Dmps3HdmiAudioOutput);
                    DMOutputChange += new DMOutputEventHandler(audioLevelControl.DMOutputChange);
                    audioLevelControl.analogAudioOutputMasterVolumeChangeEvent += new DMPS3AudioLevelControlLogic.AnalogAudioOutputMasterVolumeChangeEventHandler(audioLevelControl_analogAudioOutputMasterVolumeChangeEvent);
                    audioLevelControl.hdmiAudioOutputMasterVolumeChangeEvent += new DMPS3AudioLevelControlLogic.HdmiAudioOutputMasterVolumeChangeEventHandler(audioLevelControl_hdmiAudioOutputMasterVolumeChangeEvent);
                }

                uiLogic = new UserInterface();
                uiLogic.digitalChangeEvent += new UserInterface.DigitalChangeEventHandler(uiLogic_digitalChangeEvent);
                uiLogic.AnalogChangeEvent += new UserInterface.AnalogChangeEventHandler(uiLogic_AnalogChangeEvent);
                uiLogic.SerialChangeEvent += new UserInterface.SerialChangeEventHandler(uiLogic_SerialChangeEvent);
                uiLogic.SmartObjectChangeEvent += new UserInterface.SmartObjectChangeEventHandler(uiLogic_SmartObjectChangeEvent);

                userInterface = new Tsw750(0x03, this);
                userInterface.SigChange += new SigEventHandler(uiLogic.userInterface_SigChange);
                userInterface.LoadSmartObjects(@"\NVRAM\UI.sgd");
                foreach(KeyValuePair<uint, SmartObject> kvp in userInterface.SmartObjects)
                {
                    kvp.Value.SigChange += new SmartObjectSigChangeEventHandler(uiLogic.SmartObject_SigChange);
                }
                userInterface.Register();

                userInterface2 = new Tsw750(0x04, this);
                userInterface2.SigChange += new SigEventHandler(uiLogic.userInterface_SigChange);
                userInterface2.LoadSmartObjects(userInterface);
                foreach (KeyValuePair<uint, SmartObject> kvp in userInterface2.SmartObjects)
                {
                    kvp.Value.SigChange += new SmartObjectSigChangeEventHandler(uiLogic.SmartObject_SigChange);
                }
                userInterface2.Register();

                program2Eisc = new EISCLogic(05, "127.0.0.2", this);
                program2Eisc.digitalChangeEvent += new EISCLogic.DigitalChangeEventHandler(program2Eisc_digitalChangeEvent);
                program2Eisc.analogChangeEvent += new EISCLogic.AnalogChangeEventHandler(program2Eisc_analogChangeEvent);
                program2Eisc.serialChangeEvent += new EISCLogic.SerialChangeEventHandler(program2Eisc_serialChangeEvent);
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.Message);
            }
        }

        //dmTx1 Source Change Events
        void dmTx1_DmTx201AudioSourceChangeEvent(DmTx200Base.eSourceSelection audioSource)
        {
            uiLogic.interlock((uint)audioSource + 201, 201, 203, userInterface);
            uiLogic.interlock((uint)audioSource + 201, 201, 203, userInterface2);
        }

        void dmTx1_DmTx201VideoSourceChangeEvent(DmTx200Base.eSourceSelection VideoSource)
        {
            uiLogic.interlock((uint)VideoSource + 206, 206, 208, userInterface);
            uiLogic.interlock((uint)VideoSource + 206, 206, 208, userInterface2);
        }

        void avControl_dmps3SwitchEvent(Crestron.SimplSharpPro.DM.DMOutput output, Crestron.SimplSharpPro.DM.DMInput input)
        {
            if(output.Number == 1)
            {
                userInterface.StringInput[1].StringValue = input.NameFeedback.StringValue;
                userInterface2.StringInput[1].StringValue = input.NameFeedback.StringValue;
            }
        }

        void avControl_dmps3ClearSwitchEvent(Crestron.SimplSharpPro.DM.DMOutput output)
        {
            if (output.Number == 1)
            {
                userInterface.StringInput[1].StringValue = "No Input Selected.";
                userInterface2.StringInput[1].StringValue = "No Input Selected.";
            }
        }

        //display1 Com Port Events
        void display1Serial_SerialDataReceivedEvent(string data)
        {
            if (data.Contains("PowerIsOn"))
            {
                userInterface.BooleanInput[301].BoolValue = true;
                userInterface2.BooleanInput[301].BoolValue = true;
            }
            else if (data.Contains("PowerIsOff"))
            {
                userInterface.BooleanInput[301].BoolValue = false;
                userInterface2.BooleanInput[301].BoolValue = false;
            }
        }

        void uiLogic_SmartObjectChangeEvent(uint deviceId, SmartObjectEventArgs args)
        {
            switch (args.SmartObjectArgs.ID)
            {
                case 1: //dpad AppleTV
                    {
                        if (args.Sig.BoolValue)
                        {
                            //Example of something you can do
                            //Can program you smart graphics to use the same name as the command
                            //appleTvIr.startIrCommand(args.Sig.Name);

                            switch (args.Sig.Name)
                            {
                                case "Up":
                                    {
                                        appleTvIr.startIrCommand("UP");
                                        break;
                                    }
                                case "Down":
                                    {
                                        //pulse down
                                        appleTvIr.startIrCommand("DOWN");
                                        break;

                                    }
                                case "Left":
                                    {
                                        appleTvIr.startIrCommand("LEFT");
                                        break;
                                    }
                                case "Right":
                                    {
                                        appleTvIr.startIrCommand("RIGHT");
                                        break;
                                    }
                                case "Center":
                                    {
                                        appleTvIr.startIrCommand("SELECT");
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            appleTvIr.stopIrCommand();
                        }
                        break;
                    }
             
            }
        }

        void uiLogic_SerialChangeEvent(uint deviceId, SigEventArgs args)
        {
            if(args.Sig.Number == 401)
            {
                program2Eisc.setStringInput(args.Sig.Number - 400, args.Sig.StringValue);
            }
        }

        void uiLogic_AnalogChangeEvent(uint deviceId, SigEventArgs args)
        {
            if (args.Sig.Number == 401)
            {
                program2Eisc.setAnalogInput(args.Sig.Number - 400, args.Sig.UShortValue);
            }
        }

        void uiLogic_digitalChangeEvent(uint deviceId, SigEventArgs args)
        {
            //Handle Press & Release
            //Program2 EISC
            if(args.Sig.Number >= 401 && args.Sig.Number <= 410)
            {
                program2Eisc.setDigitalInput(args.Sig.Number - 400, args.Sig.BoolValue);
            }

            //Handle Touch Panel Presses
            if (args.Sig.BoolValue)
            {
                //AV Switching
                if (args.Sig.Number >= 101 && args.Sig.Number <= 110)
                {
                    uint inputNumber = args.Sig.Number - 100;
                    avControl.setSwitch(inputNumber, 1);
                }
                else if(args.Sig.Number == 100)
                {
                    avControl.clearSwitch(1);
                }
                //DM-TX #1 Switching
                else if (args.Sig.Number >= 201 && args.Sig.Number <= 203)
                {
                    dmTx1.setAudioInput((DmTx200Base.eSourceSelection) args.Sig.Number - 201);
                }
                else if (args.Sig.Number >= 206 && args.Sig.Number <= 208)
                {
                    dmTx1.setVideoInput((DmTx200Base.eSourceSelection)args.Sig.Number - 206);
                }
                //Display Power
                else if(args.Sig.Number == 301)
                {
                    if (userInterface.BooleanInput[301].BoolValue || userInterface2.BooleanInput[301].BoolValue)
                    {
                        display1Serial.send("PowerOff\n");
                    }
                    else
                    {
                        display1Serial.send("PowerOn\n");
                    }
                }
            }
        }

        //Program2 EISC Change Events
        void program2Eisc_serialChangeEvent(uint id, SigEventArgs args)
        {
            userInterface.StringInput[args.Sig.Number + 400].StringValue = args.Sig.StringValue;
            userInterface2.StringInput[args.Sig.Number + 400].StringValue = args.Sig.StringValue;
        }

        void program2Eisc_analogChangeEvent(uint id, SigEventArgs args)
        {
            userInterface.UShortInput[args.Sig.Number + 400].UShortValue= args.Sig.UShortValue;
            userInterface2.UShortInput[args.Sig.Number + 400].UShortValue = args.Sig.UShortValue;
        }

        void program2Eisc_digitalChangeEvent(uint id, SigEventArgs args)
        {
            userInterface.BooleanInput[args.Sig.Number + 400].BoolValue = args.Sig.BoolValue;
            userInterface2.BooleanInput[args.Sig.Number + 400].BoolValue = args.Sig.BoolValue;
        }

        //DMPS3 Audio Level Events
        void audioLevelControl_analogAudioOutputMasterVolumeChangeEvent(ushort level, bool muteOnFeedback)
        {
            userInterface.UShortInput[501].ShortValue = (short)level;
            userInterface.BooleanInput[501].BoolValue = muteOnFeedback;
            userInterface.BooleanInput[502].BoolValue = !muteOnFeedback;

            userInterface2.UShortInput[501].ShortValue = (short)level;
            userInterface2.BooleanInput[501].BoolValue = muteOnFeedback;
            userInterface2.BooleanInput[502].BoolValue = !muteOnFeedback;
        }

        void audioLevelControl_hdmiAudioOutputMasterVolumeChangeEvent(ushort level, bool muteOnFeedback)
        {
            userInterface.UShortInput[506].ShortValue = (short)level;
            userInterface.BooleanInput[506].BoolValue = muteOnFeedback;
            userInterface.BooleanInput[507].BoolValue = !muteOnFeedback;

            userInterface2.UShortInput[506].ShortValue = (short)level;
            userInterface2.BooleanInput[506].BoolValue = muteOnFeedback;
            userInterface2.BooleanInput[507].BoolValue = !muteOnFeedback;
        }

        /// <summary>
        /// Event Handler for Ethernet events: Link Up and Link Down. 
        /// Use these events to close / re-open sockets, etc. 
        /// </summary>
        /// <param name="ethernetEventArgs">This parameter holds the values 
        /// such as whether it's a Link Up or Link Down event. It will also indicate 
        /// wich Ethernet adapter this event belongs to.
        /// </param>
        void ControlSystem_ControllerEthernetEventHandler(EthernetEventArgs ethernetEventArgs)
        {
            switch (ethernetEventArgs.EthernetEventType)
            {//Determine the event type Link Up or Link Down
                case (eEthernetEventType.LinkDown):
                    //Next need to determine which adapter the event is for. 
                    //LAN is the adapter is the port connected to external networks.
                    if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter)
                    {
                        //
                    }
                    break;
                case (eEthernetEventType.LinkUp):
                    if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter)
                    {

                    }
                    break;
            }
        }

        /// <summary>
        /// Event Handler for Programmatic events: Stop, Pause, Resume.
        /// Use this event to clean up when a program is stopping, pausing, and resuming.
        /// This event only applies to this SIMPL#Pro program, it doesn't receive events
        /// for other programs stopping
        /// </summary>
        /// <param name="programStatusEventType"></param>
        void ControlSystem_ControllerProgramEventHandler(eProgramStatusEventType programStatusEventType)
        {
            switch (programStatusEventType)
            {
                case (eProgramStatusEventType.Paused):
                    //The program has been paused.  Pause all user threads/timers as needed.
                    break;
                case (eProgramStatusEventType.Resumed):
                    //The program has been resumed. Resume all the user threads/timers as needed.
                    break;
                case (eProgramStatusEventType.Stopping):
                    //The program has been stopped.
                    //Close all threads. 
                    //Shutdown all Client/Servers in the system.
                    //General cleanup.
                    //Unsubscribe to all System Monitor events
                    break;
            }

        }

        /// <summary>
        /// Event Handler for system events, Disk Inserted/Ejected, and Reboot
        /// Use this event to clean up when someone types in reboot, or when your SD /USB
        /// removable media is ejected / re-inserted.
        /// </summary>
        /// <param name="systemEventType"></param>
        void ControlSystem_ControllerSystemEventHandler(eSystemEventType systemEventType)
        {
            switch (systemEventType)
            {
                case (eSystemEventType.DiskInserted):
                    //Removable media was detected on the system
                    break;
                case (eSystemEventType.DiskRemoved):
                    //Removable media was detached from the system
                    break;
                case (eSystemEventType.Rebooting):
                    //The system is rebooting. 
                    //Very limited time to preform clean up and save any settings to disk.
                    break;
            }

        }
    }
}