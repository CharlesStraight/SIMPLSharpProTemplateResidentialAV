using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.Lighting;

namespace SIMPLSharpProTemplateResidentialAV
{
    public class Clx2Dimu8Logic
    {
        private Clx2DimU8 clx2DimU8Device;
        private ClxDimmerBase clxDimBase;
        private uint loadNumber;
        public Clx2Dimu8Logic(uint cresnetId, ControlSystem controlSystem)
        {
            ClxLightingLoad help;
            help = new ClxLightingLoad();
            clx2DimU8Device = new Clx2DimU8(cresnetId, controlSystem);
            clx2DimU8Device.Register();
        }
    }
}