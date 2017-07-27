using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace FrameWork {

    public abstract class AudioDevice {

        public AudioDevice() {
        }

        public abstract void Initialize();

    }

}