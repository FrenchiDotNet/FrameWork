using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace FrameWork {

    /**
     * Class:       Zone
     * @author:     Ryan French
     * @version:    1.1
     * Description: ...
     */
    public class Zone {

        //===================// Members //===================//

        public string name = "";
        public ushort id = 0;
        public ushort currentSource = 0;
        public string currentSourceName { get { return Core.Sources.ContainsKey(currentSource) ? Core.Sources[currentSource].name : "Off"; } }
        public ushort volume = 0;
        public bool   muteState = false;
        public bool   powerIsOn { get { return currentSource > 0 ? true : false; } }
        public bool   powerIsOff { get { return currentSource == 0 ? true : false; } }
        public bool   hasAudio { get { return audioSources.Count > 0 ? true : false; } }
        public bool   hasVideo { get { return videoSources.Count > 0 ? true : false; } }
        public bool   hasDisplay { get { return display != null ? true : false; } }
        public bool   hasLift { get { return lift != null ? true : false; } }
        public bool   hasLights { get { return lightingLoads.Count > 0 || lightingPresets.Count > 0 ? true : false; } }
        public bool   hasShades { get { return shades.Count > 0 ? true : false; } }
        public bool   hasHVAC { get { return hvacs.Count > 0 ? true : false; } }

        public List<Source> audioSources;
        public List<Source> videoSources;
        public List<Light>  lightingLoads;
        public List<Light>  lightingPresets;
        public List<Shade>  shades;
        public List<HVAC>   hvacs;

        public Display display;
        public Lift    lift;

        public DelegateString UpdateCurrentSourceName { get; set; }
        public DelegateUshort UpdateCurrentSourceID { get; set; }
        public DelegateUshort UpdateCurrentVolume { get; set; }
        public DelegateUshort UpdateMuteState { get; set; }
        public DelegateUshort UpdateDisplayAvailable { get; set; }

        public DelegateUshort UpdateHWMuteState { get; set; }
        public DelegateUshort UpdateHWVolumeDirect { get; set; }

        public event DelegateString UpdateCurrentSourceNameEvent;
        public event DelegateUshort UpdateCurrentSourceIDEvent;
        public event DelegateUshort UpdateCurrentVolumeEvent;
        public event DelegateUshort UpdateMuteStateEvent;
        public event DelegateUshort UpdateDisplayAvailableEvent;
        public event DelegateUshort UpdateLiftAvailableEvent;

        // ...
        internal delegate void UpdateZoneListSource(Zone zone, ushort sourceID, ushort sourceIcon, string sourceName);
        internal event UpdateZoneListSource UpdateZoneListSourceEvent;

        // S+ delegate for digital hi/lo commands to SIMPL Zone module
        public DelegateUshort2 SendZoneCommand { get; set; }

        // S+ delegate for requesting string-encoded list of Audio Sources
        public delegate SimplSharpString DelegateStringRequest();
        public DelegateStringRequest AudioSourcesRequest { get; set; }

        // S+ delegate for requesting string-encoded list of Video Sources
        //public delegate SimplSharpString DelegateVideoSourcesRequest();
        public DelegateStringRequest VideoSourcesRequest { get; set; }

        // S+ delegate for requesting string-encoded list of Lights
        //public delegate SimplSharpString DelegateLightsRequest();
        public DelegateStringRequest LightsLoadRequest { get; set; }
        public DelegateStringRequest LightsPresetRequest { get; set; }

        // S+ delegate for requesting string-encoded list of Shades
        public DelegateStringRequest ShadesRequest { get; set; }

        // S+ delegate for requesting string-encoded list of HVAC Zones
        public DelegateStringRequest HVACRequest { get; set; }

        //===================// Constructor //===================//

        public Zone() {

            this.name = "Unconfigured Zone";

            audioSources    = new List<Source>();
            videoSources    = new List<Source>();
            lightingLoads   = new List<Light>();
            lightingPresets = new List<Light>();
            shades          = new List<Shade>();
            hvacs           = new List<HVAC>();

        }

        //===================// Methods //===================//

        /**
         * Method:      CreateZone
         * Access:      public
         * @param:      ushort _sid
         * Description: Called by S+ to set Zone variables
         */
        public void CreateZone (ushort _id, string _name) {

            this.id   = _id;
            this.name = _name;

        }

        /**
         * Method:      SetCurrentSource
         * Access:      public
         * @param:      ushort _sid
         * Description: Accepts a SourceID argument, saves the ID as currentSource, then invokes events to subscribed interfaces
         */
        public void SetCurrentSource(ushort _sid) {

            // If Zone has a current source, unregister from that source first
            if (this.currentSource != 0)
                Core.Sources[this.currentSource].UnregisterZone (this);


            // Clean this shit up, create a single event or something
            this.currentSource = _sid;
            if (UpdateCurrentSourceName != null)
                UpdateCurrentSourceName((SimplSharpString)currentSourceName);
            if(UpdateCurrentSourceID != null)
                UpdateCurrentSourceID(this.currentSource);
            if (UpdateCurrentSourceNameEvent != null)
                UpdateCurrentSourceNameEvent((SimplSharpString)currentSourceName);
            if (UpdateCurrentSourceIDEvent != null)
                UpdateCurrentSourceIDEvent(this.currentSource);

            // Update any Zone Lists subscribed to this zone
            if (UpdateZoneListSourceEvent != null)
                UpdateZoneListSourceEvent(this, this.currentSource, Core.Sources[this.currentSource].icon, Core.Sources[this.currentSource].name);

            // Register Zone with Source
            if (_sid != 0)
                Core.Sources[_sid].RegisterZone (this);

        }

        /**
         * Method:      SetMuteFb
         * Access:      public
         * @param:      ushort _state
         * Description: Accepts a ushort value of 0 or 1, stores a boolean representation to muteState, and invokes events to subscribed interfaces.
         */
        public void SetMuteFb(ushort _state) {
            this.muteState = (_state == 0) ? false : true;
            // Clean this shit up, create a single event or something
            if (UpdateMuteState != null)
                UpdateMuteState(_state);
            if (UpdateMuteStateEvent != null)
                UpdateMuteStateEvent(_state);
        }

        /**
         * Method:      SetVolumeFb
         * Access:      public
         * @param:      ushort _level
         * Description: Accepts a ushort argument ranging from 0 to 65535 representing a percentage of 0 to 100. Stores the value as volume, then
         *              invokes events to subscribed interfaces.
         */
        public void SetVolumeFb(ushort _level) {
            this.volume = _level;
            // Clean this shit up, create a single event or something
            if (UpdateCurrentVolume != null)
                UpdateCurrentVolume(_level);
            if(UpdateCurrentVolumeEvent != null)
                UpdateCurrentVolumeEvent(_level);
        }

        /**
         * Method:      SetVolumeDirect
         * Access:      public
         * @param:      ushort _level
         * Description: Accepts a ushort argument ranging from 0 to 65535 representing a percentage of 0 to 100. Forwards the level to a S+
         *              deleagte to request a new volume level from this Zone's hardware audio device.
         */
        public void SetVolumeDirect(ushort _level) {
            UpdateHWVolumeDirect(_level);
        }

        /**
         * Method:      SetMuteState
         * Access:      public
         * @param:      ushort _state
         * Description: ...
         */
        public void SetMuteState(ushort _state) {
            //CrestronConsole.PrintLine(String.Format("SetMuteState [{0}] on zone [{1}]", _state, this.id));
            UpdateHWMuteState(_state);
        }

        /**
         * Method:      ToggleMuteState
         * Access:      public
         * Description: ...
         */
        public void ToggleMuteState() {
            //CrestronConsole.PrintLine(String.Format("ToggleMuteState on zone [{0}]", this.id));
            SetMuteState((ushort)(this.muteState ? 0 : 1));
        }

        /**
         * Method:      VolumeUpDownUshort
         * Access:      public
         * @param:      ushort _direction, ushort _state
         * Description: Sends Press/Release states for Volume Up/Down commands to S+
         */
        public void VolumeUpDownUshort(ushort _direction, ushort _state) {
            // Trigger Volume Up/Down delegate to S+ with state _state
            if (_direction == 0) { // Down
                if (_state == 1) // Release opposite state
                    SendZoneCommand(11, 0);

                SendZoneCommand(12, _state);
            } else if (_direction == 1) { // Up
                if (_state == 1) // Release opposite state
                    SendZoneCommand(12, 0);

                SendZoneCommand(11, _state);
            }
        }

        /**
         * Method:      VolumeUp
         * Access:      public
         * @param:      bool _state
         * Description: Convenience function for press/release (_state) of Volume Up cmd
         */
        public void VolumeUp(bool _state) {
            SendZoneCommand(11, (ushort)(_state ? 1 : 0));
        }

        /**
         * Method:      VolumeDown
         * Access:      public
         * @param:      bool _state
         * Description: Convenience function for press/release (_state) of Volume Down cmd
         */
        public void VolumeDown(bool _state) {
            SendZoneCommand(12, (ushort)(_state ? 1 : 0));
        }

        /**
         * Method:      ParseAudioSources
         * Access:      public
         * Description: Triggered by Core.initializeSystem to request string-encoded list of SourceID's
         *              from S+ module. Looks up SourceID's in Core.Sources and adds to local list.
         */
        public void ParseAudioSources() {
            string lTmp = AudioSourcesRequest().ToString();

            if (lTmp == "")
                return;

            string[] list = lTmp.Replace(" ", "").Split(',');
            ushort sid;
            for (int i = 0; i < list.Length; i++) {
                sid = ushort.Parse(list[i]);
                if(Core.Sources.ContainsKey(sid))
                    audioSources.Add(Core.Sources[sid]);
            }
        }

        /**
         * Method:      ParseVideoSources
         * Access:      public
         * Description: Triggered by Core.initializeSystem to request string-encoded list of SourceID's
         *              from S+ module. Looks up SourceID's in Core.Sources and adds to local list.
         */
        public void ParseVideoSources() {
            string lTmp = VideoSourcesRequest().ToString();

            if (lTmp == "")
                return;

            string[] list = lTmp.Replace(" ", "").Split(',');
            ushort sid;
            for (int i = 0; i < list.Length; i++) {
                sid = ushort.Parse(list[i]);
                if (Core.Sources.ContainsKey(sid))
                    videoSources.Add(Core.Sources[sid]);
            }
        }

        /**
         * Method:      ParseLights
         * Access:      public
         * Description: Triggered by Core.initializeSystem to request string-encoded list of LightID's
         *              from S+ module. Looks up LightID's in Core.Lights and adds to local list.
         */
        public void ParseLights() {
            // Loads
            string lTmp = LightsLoadRequest().ToString();

            if (lTmp == "")
                return;

            string[] list = lTmp.Replace(" ", "").Split(',');
            ushort lid;
            for (int i = 0; i < list.Length; i++) {
                lid = ushort.Parse(list[i]);
                if (Core.Lights.ContainsKey(lid))
                    lightingLoads.Add(Core.Lights[lid]);
            }

            // Presets
            string pTmp = LightsPresetRequest().ToString();

            if (pTmp == "")
                return;

            string[] plist = pTmp.Replace(" ", "").Split(',');
            for (int i = 0; i < plist.Length; i++) {
                lid = ushort.Parse(plist[i]);
                if (Core.Lights.ContainsKey(lid))
                    lightingPresets.Add(Core.Lights[lid]);
            }
        }

        /**
         * Method:      ParseShades
         * Access:      public
         * Description: Triggered by Core.initializeSystem to request string-encoded list of ShadeID's
         *              from S+ module. Looks up ShadeID's in Core.Shades and adds to local list.
         */
        public void ParseShades() {

            string lTmp = ShadesRequest().ToString();

            if (lTmp == "")
                return;

            string[] list = lTmp.Replace(" ", "").Split(',');
            ushort sid;
            for (int i = 0; i < list.Length; i++) {
                sid = ushort.Parse(list[i]);
                if (Core.Shades.ContainsKey(sid))
                    shades.Add(Core.Shades[sid]);
            }

        }

        /**
         * Method:      ParseHVAC
         * Access:      public
         * Description: Triggered by Core.initializeSystem to request string-encoded list of HVACID's
         *              from S+ module. Looks up HVACID's in Core.HVACs and adds to local list.
         */
        public void ParseHVAC() {

            string lTmp = HVACRequest().ToString();

            if (lTmp == "")
                return;

            string[] list = lTmp.Replace(" ", "").Split(',');
            ushort hid;
            for (int i = 0; i < list.Length; i++) {
                hid = ushort.Parse(list[i]);
                if (Core.HVACs.ContainsKey(hid))
                    hvacs.Add(Core.HVACs[hid]);
            }

        }

        /**
         * Method:      associateDisplay
         * Access:      public
         * @param:      Display _dsp
         * Description: ...
         */
        public void associateDisplay(Display _dsp) {
            this.display = _dsp;

            // Subscribe Display to UpdateCurrentSourceIDEvent
            this.UpdateCurrentSourceIDEvent += new DelegateUshort(_dsp.ZoneSourceUpdateHandler);

            // Clean this shit up, create a single event or something
            if (UpdateDisplayAvailable != null)
                UpdateDisplayAvailable(1);
            if (UpdateDisplayAvailableEvent != null)
                UpdateDisplayAvailableEvent(1);
        }

        /**
         * Method:      associateLift
         * Access:      public
         * @param:      Lift _lift
         * Description: ...
         */
        public void associateLift(Lift _lift) {
            this.lift = _lift;

            // Subscribe Lift to UpdateCurrentSourceIDEvent
            this.UpdateCurrentSourceIDEvent += new DelegateUshort(_lift.ZoneSourceUpdateHandler);

            if (UpdateLiftAvailableEvent != null)
                UpdateLiftAvailableEvent(1);
        }

        /**
         * Method:      refreshDisplayStatus
         * Access:      public
         * Description: ...
         */
        public void refreshDisplayStatus() {
            // {{-- TODO: Clean this shit up, create a single event or something --}}
            if (UpdateCurrentSourceName != null) 
                UpdateCurrentSourceName((SimplSharpString)this.currentSourceName);
            if (UpdateCurrentSourceNameEvent != null)
                UpdateCurrentSourceNameEvent((SimplSharpString)this.currentSourceName);
            
            if (UpdateCurrentSourceID != null) 
                UpdateCurrentSourceID(this.currentSource);
            if (UpdateCurrentSourceIDEvent != null)
                UpdateCurrentSourceIDEvent(this.currentSource);
            
            if (UpdateCurrentVolume != null) 
                UpdateCurrentVolume(this.volume);
            if (UpdateCurrentVolumeEvent != null)
                UpdateCurrentVolumeEvent(this.volume);
            
            if (UpdateMuteState != null) 
                UpdateMuteState((ushort)(this.muteState ? 1 : 0));
            if (UpdateMuteStateEvent != null)
                UpdateMuteStateEvent((ushort)(this.muteState ? 1 : 0));
            
            if (UpdateDisplayAvailable != null) 
                UpdateDisplayAvailable((ushort)(this.hasDisplay ? 1 : 0));
            if (UpdateDisplayAvailableEvent != null)
                UpdateDisplayAvailableEvent((ushort)(this.hasDisplay ? 1 : 0));
            
        }

        /**
         * Method:      hasSource
         * Access:      public
         * @return:     bool
         * @param:      Source src
         * Description: ...
         */
        public bool hasSource(Source src) {
            if (audioSources.Contains(src) || videoSources.Contains(src))
                return true;
            else
                return false;
        }

        /**
         * Method:      hasSourceUshort
         * Access:      public
         * @return:     bool
         * @param:      ushort _sourceID
         * Description: ...
         */
        public bool hasSourceUshort(ushort _sourceID) {
            return this.hasSource(Core.Sources[_sourceID]);
        }

    }
  
}