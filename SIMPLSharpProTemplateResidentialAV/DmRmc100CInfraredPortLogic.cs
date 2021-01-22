using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;

namespace SIMPLSharpProTemplateResidentialAV
{
    public class DmRmc100CInfraredPortLogic
    {
        Crestron.SimplSharpPro.IROutputPort irPort;
        public DmRmc100CInfraredPortLogic(Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc100C dmRmc100Device, uint irPortNumber, string irFilePath, bool printCommands)
        {
            if (dmRmc100Device.NumberOfIROutputPorts >= irPortNumber)
            {
                irPort = dmRmc100Device.IROutputPorts[irPortNumber];

                if (Crestron.SimplSharp.CrestronIO.File.Exists(irFilePath))
                {
                    irPort.LoadIRDriver(irFilePath);
                    if (printCommands)
                    {
                        string[] commands = irPort.AvailableIRCmds();
                        foreach (string command in commands)
                        {
                            CrestronConsole.Print("\n\r AvailableIRCmds: " + command);
                        }
                    }
                }
                else
                {
                    ErrorLog.Error("IR file {0} does not exist.", irFilePath);
                }
            }
            else
            {
                ErrorLog.Error("Cannot use IR port {0} on DM-RMC-100-C.", irPortNumber);
            }
        }

        public void startIrCommand(string command)
        {
            if (irPort.IsIRCommandAvailable(command))
            {
                irPort.Press(command);
            }
            else
            {
                ErrorLog.Error("IR command {0} is not available.", command);
            }
        }

        public void stopIrCommand()
        {
            irPort.Release();
        }

        /// <summary>
        /// pulseTime is in ms.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="pulseTime"></param>
        public void pulseIrCommand(string command, ushort pulseTime)
        {
            if (irPort.IsIRCommandAvailable(command))
            {
                irPort.PressAndRelease(command, pulseTime);
            }
            else
            {
                ErrorLog.Error("IR command {0} is not available.", command);
            }
        }
    }
}