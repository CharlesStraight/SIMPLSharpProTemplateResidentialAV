using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;

namespace SIMPLSharpProTemplateResidentialAV
{
    public class DMPS3AVControl
    {
        public Crestron.SimplSharpPro.CrestronCollection<ICardInputOutputType> switcherInputs;
        public Crestron.SimplSharpPro.CrestronCollection<ICardInputOutputType> switcherOutputs;

        public DMPS3AVControl(Crestron.SimplSharpPro.CrestronCollection<ICardInputOutputType> dmps3switcherInputs, Crestron.SimplSharpPro.CrestronCollection<ICardInputOutputType> dmps3switcherOutputs)
        {
            switcherInputs = dmps3switcherInputs;
            switcherOutputs = dmps3switcherOutputs;
        }

        public delegate void DMPS3SwitchEventHandler(DMOutput output, DMInput input);
        public event DMPS3SwitchEventHandler dmps3SwitchEvent;

        public delegate void DMPS3ClearSwitchEventHandler(DMOutput output);
        public event DMPS3ClearSwitchEventHandler dmps3ClearSwitchEvent;

        public void setSwitch(uint inputNumber, uint outputNumber)
        {
            DMOutput output = switcherOutputs[outputNumber] as DMOutput; //Typecasting to a DMOutput
            output.VideoOut = switcherInputs[inputNumber] as DMInput; //Typecasting to a DMInput
        }

        public void clearSwitch(uint outputNumber)
        {
            DMOutput output = switcherOutputs[outputNumber] as DMOutput; //Typecasting to a DMOutput
            output.VideoOut = null;
        }
        
        public void ControlSystem_DMOutputChange(Crestron.SimplSharpPro.DM.Switch device, Crestron.SimplSharpPro.DM.DMOutputEventArgs args)
        {
            switch (args.EventId)
            {
                case DMOutputEventIds.VideoOutEventId:
                    {
                        DMOutput output = switcherOutputs[args.Number] as DMOutput;
                        if (output.VideoOutFeedback != null)
                        {
                            DMInput input = output.VideoOutFeedback;
                            dmps3SwitchEvent(output, input);
                        }
                        else
                        {
                            dmps3ClearSwitchEvent(output);
                        }

                        break;
                    }
                   
            }
        }
    }
}