using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace FrameWork {

    /**
     * Class:       Source
     * @author:     Ryan French
     * @version:    1.1
     * Description: ...
     */
    public class Source {

        //===================// Members //===================//

        public string name;
        public ushort id;
        public ushort type;
        public ushort icon;
        public bool isAudioSource;
        public bool isVideoSource;

        public bool inUse;
        public List<Zone> zonesUsing;

        public ushort setIsAudioSource { set { isAudioSource = (value == 1) ? true : false; } }
        public ushort setIsVideoSource { set { isVideoSource = (value == 1) ? true : false; } }

        public DelegateUshort SendEvent { get; set; }

        //===================// Constructor //===================//

        public Source () {

            zonesUsing = new List<Zone> ();

        }

        public Source (string _name, ushort _id, ushort _type, ushort _icon) {

            this.name = _name;
            this.id   = _id;
            this.type = _type;
            this.icon = _icon;

        }

        public void RegisterZone (Zone _zn) {

            if (!zonesUsing.Contains (_zn)) {

                zonesUsing.Add (_zn);
                if (zonesUsing.Count == 1) {

                    inUse = true;
                    SendEvent ((ushort)SourceCommands.PowerOn);

                }

            }

        }

        public void UnregisterZone (Zone _zn) {

            if (zonesUsing.Contains (_zn)) {

                zonesUsing.Remove (_zn);
                if (zonesUsing.Count == 0) {

                    inUse = false;
                    SendEvent ((ushort)SourceCommands.PowerOff);

                }

            }

        }

        //===================// Methods //===================//

    }

    public enum SourceCommands {
        PowerOff = 1,
        PowerOn
    }
}