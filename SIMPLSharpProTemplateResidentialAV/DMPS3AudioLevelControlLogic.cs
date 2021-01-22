using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;

namespace SIMPLSharpProTemplateResidentialAV
{
    public class DMPS3AudioLevelControlLogic
    {
        public Crestron.SimplSharpPro.DM.Cards.Card.Dmps3HdmiAudioOutput.Dmps3AudioOutputStream analogAudioOutput;
        public Crestron.SimplSharpPro.DM.Cards.Card.Dmps3HdmiAudioOutput.Dmps3DmHdmiOutputStream hdmiAudioOutput;
        public DMPS3AudioLevelControlLogic(Crestron.SimplSharpPro.DM.Cards.Card.Dmps3HdmiAudioOutput audioOutput)
        {
            analogAudioOutput = audioOutput.AudioOutputStream as Crestron.SimplSharpPro.DM.Cards.Card.Dmps3HdmiAudioOutput.Dmps3AudioOutputStream;
            hdmiAudioOutput = audioOutput.DmHdmiOutputStream as Crestron.SimplSharpPro.DM.Cards.Card.Dmps3HdmiAudioOutput.Dmps3DmHdmiOutputStream;
        }
        public delegate void AnalogAudioOutputMasterVolumeChangeEventHandler(ushort level, bool muteOnFeedback);
        public event AnalogAudioOutputMasterVolumeChangeEventHandler analogAudioOutputMasterVolumeChangeEvent;

        public delegate void HdmiAudioOutputMasterVolumeChangeEventHandler(ushort level, bool muteOnFeedback);
        public event HdmiAudioOutputMasterVolumeChangeEventHandler hdmiAudioOutputMasterVolumeChangeEvent;

        //Analog Output
        public void setAnalogOutputLevel(short level)
        {
            analogAudioOutput.MasterVolume.ShortValue = level;
        }

        public void rampAnalogOutputLevel(short level, uint rampTime)
        {
            analogAudioOutput.MasterVolume.CreateSignedRamp(level, rampTime);
        }

        public void rampAnalogOutputLevelUp(uint rampTime)
        {
            analogAudioOutput.MasterVolume.CreateSignedRamp(analogAudioOutput.OutputMixer.MaxVolumeFeedback.ShortValue, rampTime);
        }

        public void rampAnalogOutputLevelDown(uint rampTime)
        {
            analogAudioOutput.MasterVolume.CreateSignedRamp(analogAudioOutput.OutputMixer.MinVolumeFeedback.ShortValue, rampTime);
        }

        public void stopAnalogOutputLevelRamping()
        {
            analogAudioOutput.MasterVolume.StopRamp();
        }

        public void analogOutputMuteOn()
        {
            analogAudioOutput.MasterMuteOn();
        }

        public void analogOutputMuteOff()
        {
            analogAudioOutput.MasterMuteOff();
        }

        public void analogOutputMuteToggle()
        {
            analogAudioOutput.MasterMuteToggle();
        }

        //HDMI Output
        public void setHdmiOutputLevel(short level)
        {
            hdmiAudioOutput.MasterVolume.ShortValue = level;
        }

        public void rampHdmiOutputLevel(short level, uint rampTime)
        {
            hdmiAudioOutput.MasterVolume.CreateSignedRamp(level, rampTime);
        }

        public void rampHdmiOutputLevelUp(uint rampTime)
        {
            hdmiAudioOutput.MasterVolume.CreateSignedRamp(hdmiAudioOutput.OutputMixer.MaxVolumeFeedback.ShortValue, rampTime);
        }

        public void rampHdmiOutputLevelDown(uint rampTime)
        {
            hdmiAudioOutput.MasterVolume.CreateSignedRamp(hdmiAudioOutput.OutputMixer.MinVolumeFeedback.ShortValue, rampTime);
        }

        public void stopHdmiOutputLevelRamping()
        {
            hdmiAudioOutput.MasterVolume.StopRamp();
        }

        public void hdmiOutputMuteOn()
        {
            hdmiAudioOutput.MasterMuteOn();
        }

        public void hdmiOutputMuteOff()
        {
            hdmiAudioOutput.MasterMuteOff();
        }

        public void hdmiOutputMuteToggle()
        {
            hdmiAudioOutput.MasterMuteToggle();
        }

        public void DMOutputChange(Switch device, DMOutputEventArgs args)
        {
            if (args.EventId == DMOutputEventIds.MasterVolumeFeedBackEventId || args.EventId == DMOutputEventIds.MasterMuteOnFeedBackEventId)
            {
                if(args.Stream == analogAudioOutput)
                {
                    analogAudioOutputMasterVolumeChangeEvent(analogAudioOutput.MasterVolumeFeedBack.UShortValue, analogAudioOutput.MasterMuteOnFeedBack.BoolValue);
                }
                else if (args.Stream == hdmiAudioOutput)
                {
                    hdmiAudioOutputMasterVolumeChangeEvent(hdmiAudioOutput.MasterVolumeFeedBack.UShortValue, hdmiAudioOutput.MasterMuteOnFeedBack.BoolValue);
                }
            }
        }
    }
}