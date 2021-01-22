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
    public class DmRmc100CSerialPortLogic
    {
        public Crestron.SimplSharpPro.ComPort comPort;
        public delegate void SerialDataReceivedEventHandler(string data);
        public event SerialDataReceivedEventHandler SerialDataReceivedEvent;
        public string delimiter;
        private StringBuilder receiveBuffer;

        public DmRmc100CSerialPortLogic(Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc100C dmRmc100CDevice, 
            uint comPortNumber,
            Crestron.SimplSharpPro.ComPort.eComBaudRates baudRate,
            Crestron.SimplSharpPro.ComPort.eComDataBits dataBits,
            Crestron.SimplSharpPro.ComPort.eComHardwareHandshakeType hwHandshake,
            Crestron.SimplSharpPro.ComPort.eComParityType parity,
            Crestron.SimplSharpPro.ComPort.eComStopBits stopBits,
            string delimiter)
        {
            if (dmRmc100CDevice.NumberOfIROutputPorts >= comPortNumber)
            {
                comPort = dmRmc100CDevice.ComPorts[comPortNumber];
                Crestron.SimplSharpPro.ComPort.ComPortSpec spec = new Crestron.SimplSharpPro.ComPort.ComPortSpec();
                spec.BaudRate = baudRate;
                spec.DataBits = dataBits;
                spec.HardwareHandShake = hwHandshake;
                spec.Parity = parity;
                spec.StopBits = stopBits;
                comPort.SetComPortSpec(spec);
                this.delimiter = delimiter;
                receiveBuffer = new StringBuilder();
                comPort.SerialDataReceived += new Crestron.SimplSharpPro.ComPortDataReceivedEvent(SerialDataReceived);
            }
        }

        public void send(string sendString)
        {
            comPort.Send(sendString);
        }

        public void SerialDataReceived(Crestron.SimplSharpPro.ComPort ReceivingComPort, Crestron.SimplSharpPro.ComPortSerialDataEventArgs args)
        {
            receiveBuffer.Append(comPort.RcvdString);
            if (receiveBuffer.ToString().Contains(delimiter))
            {
                SerialDataReceivedEvent(receiveBuffer.ToString());
                receiveBuffer.Remove(0, receiveBuffer.Length);
            }
        }
    }
}