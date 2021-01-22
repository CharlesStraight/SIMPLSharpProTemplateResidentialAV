using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;

namespace SIMPLSharpProTemplateResidentialAV
{
    public class DmTx201CLogic
    {
        public Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201C dmTx201c;
        public DmTx201CLogic(Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201C dmTx201cDevice)
        {
            dmTx201c = dmTx201cDevice;
            dmTx201c.BaseEvent += new Crestron.SimplSharpPro.BaseEventHandler(dmTx201c_BaseEvent);
        }

        public delegate void DmTx201AudioSourceChangeEventHandler(DmTx200Base.eSourceSelection audioSource);
        public event DmTx201AudioSourceChangeEventHandler DmTx201AudioSourceChangeEvent;

        public delegate void DmTx201VideoSourceChangeEventHandler(DmTx200Base.eSourceSelection VideoSource);
        public event DmTx201VideoSourceChangeEventHandler DmTx201VideoSourceChangeEvent;

        void dmTx201c_BaseEvent(Crestron.SimplSharpPro.GenericBase device, Crestron.SimplSharpPro.BaseEventArgs args)
        {
            switch (args.EventId)
            {
                case DmTx200Base.AudioSourceFeedbackEventId:
                    {
                        DmTx201AudioSourceChangeEvent(dmTx201c.AudioSourceFeedback);
                        break;
                    }
                case DmTx200Base.VideoSourceFeedbackEventId:
                    {
                        DmTx201VideoSourceChangeEvent(dmTx201c.VideoSourceFeedback);
                        break;
                    }
            }
        }
        
        /// <summary>
        /// Sets the audio input.
        /// Values are Auto = 0 Digital = 1 Analog = 2 Disable = 3.
        /// </summary>
        public void setAudioInput(DmTx200Base.eSourceSelection input)
        {
            dmTx201c.AudioSource = input;
        }
        
        /// <summary>
        /// Sets the video input.
        /// Values are Auto = 0 Digital = 1 Analog = 2 Disable = 3.
        /// </summary>
        public void setVideoInput(DmTx200Base.eSourceSelection input)
        {
            dmTx201c.VideoSource = input;
        }

        /// <summary>
        /// Sets the video and video input.
        /// Values are Auto = 0 Digital = 1 Analog = 2 Disable = 3.
        /// </summary>
        public void setAVInput(DmTx200Base.eSourceSelection input)
        {
            dmTx201c.AudioSource = input;
            dmTx201c.VideoSource = input;
        }
    }
}