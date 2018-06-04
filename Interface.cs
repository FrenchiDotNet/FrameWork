using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;

namespace FrameWork {

    /**
     * Class:       Interface
     * @author:     Ryan French
     * @version:    1.3a
     * Description: ...
     */
    public class Interface {

        //===================// Members //===================//

        public string version = "1.3a";

        public string name;
        public ushort id;
        public ushort defaultZoneID;

        internal List<Zone> zoneList;
        internal List<Zone> shareZones;
        internal List<shareListEvents> shareList;
        internal List<Source> audioSourceList;
        internal List<Source> videoSourceList;
        internal List<sysMenuItem> sysMenuList;
        internal List<SecurityKeypad> securityKeypadList;

        internal SecurityKeypad currentSecurityKeypad;

        internal bool _invertSourceLists;
        internal bool _enableLightingControl;
        internal bool _enableShadeControl;
        internal bool _enableHVACControl;
        internal bool _enableSecurityControl;
        internal bool _enablePoolControl;
        internal bool _enableVoiceControlSetup;
        internal bool _enableStartupEvents;

        public ushort sourceListDisplaySize;
        public ushort sourceListMaxSize;
        public ushort lightingLoadsDisplaySize;
        public ushort lightingPresetsDisplaySize;
        public ushort shadeListDisplaySize;
        public ushort hvacListDisplaySize;

        public Zone currentZone;
        public Source currentSource;

        public bool hasSecurityKeypad { get { return securityKeypadList.Count > 0 ? true : false; } }

        public DelegateUshort2 UpdateDigitalOutput { get; set; }
        public DelegateUshort2 UpdateAnalogOutput { get; set; }
        public DelegateUshortString UpdateStringOutput { get; set; }

        // S+ delegate for sending details of Zone List items
        public delegate void DelegateZoneListUpdate(ushort position, ushort sourceIcon, SimplSharpString zoneName, SimplSharpString sourceName);
        public DelegateZoneListUpdate ZoneListUpdate { get; set; }

        // S+ delegate for sending the lights feedback of Zone List items
        public DelegateUshort2 ZoneListLightsUpdate { get; set; }

        // S+ delegate for sending details of Source List items
        public delegate void DelegateSourceListUpdate(ushort position, ushort listType, ushort sourceID, SimplSharpString sourceName);
        public DelegateSourceListUpdate SourceListUpdate { get; set; }

        // S+ delegate for sending initial details of Share List items
        public delegate void DelegateSendShareListItem (ushort position, SimplSharpString zoneName, ushort shareActive, ushort muteState, ushort volume);
        public DelegateSendShareListItem TriggerSendShareListItem { get; set; }

        // S+ delegate for sending mute state changes of Share List items
        public delegate void DelegateSendShareMuteState(ushort position, ushort state);
        public DelegateSendShareMuteState TriggerSendShareMuteState { get; set; }

        // S+ delegate for sending volume state changes of Share List items
        public delegate void DelegateSendShareVolume(ushort position, ushort volume);
        public DelegateSendShareVolume TriggerSendShareVolume { get; set; }

        // S+ delegate for sending share state changes of Share List items
        public delegate void DelegateSendShareState(ushort position, ushort state);
        public DelegateSendShareState TriggerSendShareState { get; set; }

        // S+ delegate for sending label & visibility of Display Control List items
        public delegate void DelegateSendDisplayControlListItem(ushort position, SimplSharpString label, ushort enable);
        public DelegateSendDisplayControlListItem TriggerSendDisplayControlListItem { get; set; }

        // S+ delegate for requesting list of available zones
        public delegate SimplSharpString DelegateAvailableZoneRequest();
        public DelegateAvailableZoneRequest AvailableZoneRequest { get; set; }

        // S+ delegate for requesting list of Share zones
        public delegate SimplSharpString DelegateShareListRequest();
        public DelegateShareListRequest ShareListRequest { get; set; }

        // S+ delegate for sending details of SysMenu items
        public delegate void DelegateSysMenuListUpdate(ushort position, SimplSharpString label, ushort icon);
        public DelegateSysMenuListUpdate SysMenuListUpdate { get; set; }

        // S+ delegate for sending Lift Control button feedback
        public DelegateUshort2 TriggerSendLiftFb { get; set; }

        public delegate void DelegateLightingLoadListUpdate(ushort position, SimplSharpString label, ushort type);
        public DelegateLightingLoadListUpdate LightingLoadListUpdate   { get; set; }
        public DelegateLightingLoadListUpdate LightingPresetListUpdate { get; set; }

        public delegate void DelegateLightingLoadFbUpdate(ushort position, ushort action, ushort state);
        public DelegateUshort3 LightingLoadFbUpdate   { get; set; }
        public DelegateUshort3 LightingPresetFbUpdate { get; set; }

        // S+ delegate for sending Shade list item
        public delegate void DelegateShadeListUpdate(ushort position, SimplSharpString label, ushort type);
        public DelegateShadeListUpdate ShadeListUpdate { get; set; }

        // S+ delegate for sending Shade Feedback
        public DelegateUshort3 ShadeFbUpdate { get; set; } 

        // S+ delegate for sending HVAC list item details
        public delegate void DelegateHVACListUpdate (ushort position, SimplSharpString label, ushort type, ushort fanOrPower, ushort hasFanSpeed);
        public DelegateHVACListUpdate HVACListUpdate { get; set; }

        // S+ delegate for sending HVAC feedback
        public DelegateUshort3 HVACFbUpdate { get; set; }

        // S+ delegate for requesting Security Keypad list
        public DelegateStringRequest SecurityKeypadListRequest { get; set; }

        // S+ delegates for sending Security Keypad feedback
        public DelegateUshort2 SecurityKeypadFbUpdate { get; set; }
        public DelegateUshortString SecurityKeypadTextFbUpdate { get; set; }

        // S+ delegates for sending Pool feedback
        public DelegateUshortUshortString PoolFbUpdate { get; set; }

        // S+ delegates for sending Voice Control Setup feedback
        public DelegateUshortString VoiceControlEvent { get; set; }
        public DelegateString VoiceControlIDEvent { get; set; }


        //===================// Constructor //===================//

        public Interface() {

            name                  = "";
            id                    = 0;
            defaultZoneID         = 0;

            zoneList           = new List<Zone>();
            shareZones         = new List<Zone>();
            shareList          = new List<shareListEvents>();
            audioSourceList    = new List<Source>();
            videoSourceList    = new List<Source>();
            sysMenuList        = new List<sysMenuItem>();
            securityKeypadList = new List<SecurityKeypad>();

        }

        //===================// Methods //===================//

        /**
         * Method: Initialize
         * Access: public
         * @return: void
         * Description: Method run after all modules have reported in to Core
         */
        public void Initialize() {

            try {

                BuildZoneList();
                BuildShareZones();
                setCurrentZone(this.defaultZoneID);
                ParseSecurityKeypads();

                PoolConnect();

            }
            catch (Exception e) {

                Core.ConsoleMessage(String.Format("[ERROR] Error during Interface {0} initialization: '{1}'", this.id, e));

            }

            Core.ConsoleMessage(String.Format("[STARTUP] Finished referencing assets on Interface {0} [{1}]", this.id, this.name));
            Core.RegCompleteCallback(2);

        }

        /**
         * Method: setCurrentZone
         * Access: public
         * @param: ushort _zid
         * Description: ...
         */
        public void setCurrentZone(ushort _zid) {
            
            if (Core.Zones.ContainsKey(_zid)) {

                // Unsubscribe from current zone events
                if (this.currentZone != null) {

                    this.currentZone.UpdateCurrentSourceNameEvent -= this.ZoneCurrentSourceNameHandler;
                    this.currentZone.UpdateCurrentSourceIDEvent   -= this.ZoneCurrentSourceIDHandler;
                    this.currentZone.UpdateCurrentVolumeEvent     -= this.ZoneCurrentVolumeHandler;
                    this.currentZone.UpdateMuteStateEvent         -= this.ZoneMuteStateHandler;
                    this.currentZone.UpdateDisplayAvailableEvent  -= this.ZoneUpdateDisplayAvailableHandler;

                    if (this.currentZone.hasLift)
                        this.currentZone.lift.LiftCommandFb -= this.LiftCommandFbHandler;

                    // Unsubscribe from current Lights list
                    if (this.currentZone.hasLights && _enableLightingControl) {

                        if (this.currentZone.lightingLoads.Count > 0) {
                            for (int j = 0; j < currentZone.lightingLoads.Count; j++) {
                                currentZone.lightingLoads[j].unsubscribeFromEvents(this);
                            }
                        }

                        if (this.currentZone.lightingPresets.Count > 0) {
                            for (int j = 0; j < currentZone.lightingPresets.Count; j++) {
                                currentZone.lightingPresets[j].unsubscribeFromEvents(this);
                            }
                        }

                    }

                    // Unsubscribe from current Shades list
                    if (this.currentZone.hasShades && _enableShadeControl) {
                        for (int j = 0; j < currentZone.shades.Count; j++) {
                            currentZone.shades[j].unsubscribeFromEvents(this);
                        }
                    }

                    // Unsubscribe from current HVAC list
                    if (this.currentZone.hasHVAC && _enableHVACControl) {
                        for (int j = 0; j < currentZone.hvacs.Count; j++) {
                            currentZone.hvacs[j].unsubscribeFromEvents(this);
                        }
                    }

                    // Remove self from Zone's Interface List
                    this.currentZone.unsubscribeInterface(this);

                }

                // Set new zone reference
                currentZone = Core.Zones[_zid];

                // Update display joins
                TriggerUpdateAnalogOutput((ushort)AnalogJoins.CurrentZone, _zid);
                TriggerUpdateAnalogOutput((ushort)AnalogJoins.CurrentSource, currentZone.currentSource);
                TriggerUpdateStringOutput((ushort)SerialJoins.CurrentZone, currentZone.name);
                TriggerUpdateStringOutput((ushort)SerialJoins.CurrentSource, currentZone.currentSourceName);
                TriggerUpdateDigitalOutput((ushort)DigitalJoins.Mute_Fb, (ushort)(currentZone.muteState ? 1 : 0));
                TriggerUpdateDigitalOutput((ushort)DigitalJoins.DirectVolumeAvailable, (ushort)(currentZone.hasDirectVolume ? 1 : 0));

                // Subscribe to new zone events
                this.currentZone.UpdateCurrentSourceNameEvent += new DelegateString(this.ZoneCurrentSourceNameHandler);
                this.currentZone.UpdateCurrentSourceIDEvent   += new DelegateUshort(this.ZoneCurrentSourceIDHandler);
                this.currentZone.UpdateCurrentVolumeEvent     += new DelegateUshort(this.ZoneCurrentVolumeHandler);
                this.currentZone.UpdateMuteStateEvent         += new DelegateUshort(this.ZoneMuteStateHandler);
                this.currentZone.UpdateDisplayAvailableEvent  += new DelegateUshort(this.ZoneUpdateDisplayAvailableHandler);

                // Subscribe to new lights list
                if (currentZone.hasLights && _enableLightingControl) {

                    // Loads
                    if (currentZone.lightingLoads.Count > 0) {
                        for (int j = 0; j < currentZone.lightingLoads.Count; j++) {
                            currentZone.lightingLoads[j].subscribeToEvents(this);
                        }
                    }

                    // Presets
                    if (currentZone.lightingPresets.Count > 0) {
                        for (int j = 0; j < currentZone.lightingPresets.Count; j++) {
                            currentZone.lightingPresets[j].subscribeToEvents(this);
                        }
                    }

                }

                // Subscribe to new Shades list
                if (currentZone.hasShades && _enableShadeControl) {
                    for (int j = 0; j < currentZone.shades.Count; j++) {
                        currentZone.shades[j].subscribeToEvents(this);
                    }
                }

                // Subscribe to new HVAC list
                if (currentZone.hasHVAC && _enableHVACControl) {
                    for (int j = 0; j < currentZone.hvacs.Count; j++) {
                        currentZone.hvacs[j].subscribeToEvents(this);
                    }
                }

                // Subscribe to Lift
                if (this.currentZone.hasLift)
                    this.currentZone.lift.LiftCommandFb += new DelegateUshort2(this.LiftCommandFbHandler);

                // Update Listen & Watch lists with Zone's sources
                BuildSourceLists();
                UpdateSourceListFb();

                // Update SysMenu List
                UpdateSysMenuList();

                // Update Share List
                UpdateShareList();

                // Update Lift Feedback
                RefreshLiftFb();

                // Build Lights List
                BuildLightsList();

                // Build Shades List
                BuildShadesList ();

                // Build HVAC List
                BuildHVACList ();

                // Add self to Zone's Interface List
                this.currentZone.subscribeInterface(this);

            }

        }

        /**
         * Method: PoolConnect
         * Access: private
         * @return: void
         * Description: ...
         */
        private void PoolConnect() {

            if (_enablePoolControl && Core.pool != null) {

                int count = 0;

                Core.pool.UpdateEvent += this.PoolFbHandler;
                UpdateDigitalOutput((ushort)DigitalJoins.PoolControlAvailable, 1);

                for (int i = 0; i < 12; i++ ) {
                    this.PoolFbUpdate((ushort)PoolCommand.Aux_Label_Fb, (ushort)(i + 1), Core.pool.auxLabels[i]);
                    if (Core.pool.auxLabels[i] != "")
                        count++;
                }

                if (count > 0)
                    this.PoolFbUpdate((ushort)PoolCommand.Aux_Count_Fb, (ushort)count, "");

                this.PoolFbUpdate((ushort)PoolCommand.System_Type, (ushort)Core.pool.systemType, "");
                this.PoolFbUpdate((ushort)PoolCommand.ModeSelect_Pool_Fb, (ushort)(Core.pool.currentMode == PoolMode.Pool ? 1 : 0), "");
                this.PoolFbUpdate((ushort)PoolCommand.ModeSelect_Spa_Fb, (ushort)(Core.pool.currentMode == PoolMode.Spa ? 1 : 0), "");
                this.PoolFbUpdate((ushort)PoolCommand.Ambient_Temp_Fb, (ushort)Core.pool.currentAmbientTemp, "");
                this.PoolFbUpdate((ushort)PoolCommand.Pool_Temp_Fb, (ushort)Core.pool.currentPoolTemp, "");
                this.PoolFbUpdate((ushort)PoolCommand.Pool_Setpoint_Fb, (ushort)Core.pool.currentPoolSetpoint, "");
                this.PoolFbUpdate((ushort)PoolCommand.Spa_Temp_Fb, (ushort)Core.pool.currentSpaTemp, "");
                this.PoolFbUpdate((ushort)PoolCommand.Spa_Setpoint_Fb, (ushort)Core.pool.currentSpaSetpoint, "");

            }

        }

        /**
         * Method: ZoneListEvent
         * Access: public
         * @param: ushort _index
         * @param: ushort _state
         * @return: void
         * Description: Triggered from S+ when a Zone List Button is pressed
         */
        public void ZoneListEvent(ushort _index, ushort _state) {

            if (_state == 1) { // Press

                // Get room associated with list button
                setCurrentZone(zoneList[_index - 1].id);

            }

        }

        /**
         * Method: SourceListEvent
         * Access: public
         * @param: ushort _index
         * @param: ushort _sourceType
         * @param: ushort _state
         * @return: void
         * Description: Triggered from S+ when a Source List Button is pressed/released
         */
        public void SourceListEvent(ushort _index, ushort _sourceType, ushort _state) {

            ushort src;

            if (_sourceType == 0) { // Listen Source

                if(_state == 1) { // Press

                    if(audioSourceList[(int)_index] != null)
                        src = audioSourceList[(int)_index].id;
                    else
                        src = 0;

                    currentZone.SetCurrentSource(src);

                }

            } else if (_sourceType == 1) { // Watch Source

                if (_state == 1) { // Press

                    if (videoSourceList[(int)_index] != null)
                        src = videoSourceList[(int)_index].id;
                    else
                        src = 0;

                    currentZone.SetCurrentSource(src);

                }

            }
        }

        /**
         * Method: SysMenuEvent
         * Access: public
         * @param: ushort _index
         * @return: void
         * Description: Accept incoming SysMenu button press and lookup action in list
         */
        public void SysMenuEvent(ushort _index) {

            switch (sysMenuList[_index].action) {

                case "EnterSetup": {
                    UpdateDigitalOutput((ushort)DigitalJoins.EnterSetup, 1);
                }
                break;
                case "VoiceCtrlPopup": {
                    UpdateVoiceControlFeedback();
                    UpdateDigitalOutput((ushort)DigitalJoins.Popup_Request_Voice_Control, 1);
                }
                break;
                case "DispCmdPopup": {
                    UpdateDisplayControlList();
                    UpdateDigitalOutput((ushort)DigitalJoins.Popup_Request_Display_Commands, 1);
                }
                break;
                case "LiftCmdPopup": {
                    UpdateDigitalOutput((ushort)DigitalJoins.Popup_Request_Lift_Commands, 1);
                }
                break;
                case "AVRCmdPopup": {
                    UpdateDigitalOutput((ushort)DigitalJoins.Popup_Request_AVR_Commands, 1);
                }
                break;

            }

        }

        /**
         * Method: ShareListEvent
         * Access: public
         * @param: ushort _index
         * @return: void
         * Description: Accept incoming Share List button press and send current Source to target Zone
         */
        public void ShareListEvent(ushort _index) {

            // Get zone @ position _index in shareList
            Zone znTmp = this.shareList[_index].znRef;

            // Set currentSource or turn zone off
            if (znTmp.currentSource != this.currentZone.currentSource)
                znTmp.SetCurrentSource(this.currentZone.currentSource);
            else
                znTmp.SetCurrentSource(0);

        }

        /**
         * Method: ShareListVolumeEvent
         * Access: public
         * @param: ushort _index
         * @param: ushort _direction
         * @param: ushort _state
         * @return: void
         * Description: Accept incoming Share List Volume Up/Down button press and send direction (up/down) and
         *              state (press/release) to target Zone
         */
        public void ShareListVolumeEvent(ushort _index, ushort _direction, ushort _state) {

            this.shareList[_index].znRef.VolumeUpDownUshort(_direction, _state);

        }

        /**
         * Method: ShareListMuteEvent
         * Access: public
         * @param: ushort _index
         * @param: ushort _state
         * @return: void
         * Description: Accept incoming Share List Mute button press and send ToggleMuteState to Zone
         */
        public void ShareListMuteEvent(ushort _index) {

            this.shareList[_index].znRef.ToggleMuteState();

        }

        /**
         * Method:      DisplayControlEvent
         * Access:      public
         * @param:      ushort _join
         * @param:      ushort _state
         * Description: Forwards TVControl button events to currentZone's attached Display object
         */
        public void DisplayControlEvent(ushort _join, ushort _state) {

            if (this.currentZone.hasDisplay)
                this.currentZone.display.DisplayCommand(_join, _state);

        }

        /**
         * Method:      LiftControlEvent
         * Access:      public
         * @param:      ushort _join
         * @param:      ushort _state
         * Description: Forwards Lift Control button events from S+ to currentZone's attached Lift object
         */
        public void LiftControlEvent(ushort _join) {

            if(!this.currentZone.hasLift)
                return;

            switch(_join) {
                case 1: { // Raise
                    this.currentZone.lift.Raise();
                }
                break;
                case 2: { // Stop
                    this.currentZone.lift.Stop();
                }
                break;
                case 3: { // Lower
                    this.currentZone.lift.Lower();
                }
                break;
                case 4: { // Hold
                    this.currentZone.lift.ToggleHold();
                }
                break;
            }

        }

        /**
         * Method: PowerOffCurrentZone
         * Access: public
         * @return: void
         * Description: ...
         */
        public void PowerOffCurrentZone() {

            currentZone.SetCurrentSource(0);

        }

        /**
         * Method: PowerOffAll
         * Access: public
         * @return: void
         * Description: Power of all zones in zoneList
         */
        public void PowerOffAll() {

            for (int i = 0; i < zoneList.Count; i++) {
                if (zoneList[i].currentSource != 0)
                    zoneList[i].SetCurrentSource(0);
            }

        }

        /**
         * Method: BuildSourceLists
         * Access: internal
         * @return: void
         * Description: ...
         */
        internal void BuildSourceLists() {

            int asize = currentZone.audioSources.Count;
            int vsize = currentZone.videoSources.Count;

            // Initialize lists
            audioSourceList = new List<Source>((int)sourceListMaxSize);
            videoSourceList = new List<Source>((int)sourceListMaxSize);

            for (int i = 0; i < (int)sourceListMaxSize; i++) {
                audioSourceList.Add(null);
                videoSourceList.Add(null);
            }

            // Copy source lists from current zone and assign to display lists
            if (!_invertSourceLists) {

                for (int i = 0; i < currentZone.audioSources.Count; i++) {
                    audioSourceList[i] = currentZone.audioSources[i];
                }

                for (int i = 0; i < currentZone.videoSources.Count; i++) {
                    videoSourceList[i] = currentZone.videoSources[i];
                }

            } else {

                // Audio Sources
                if (asize == 0) {

                } else if (asize <= sourceListDisplaySize) {

                    for (int i = 0; i < sourceListDisplaySize; i++) {
                        if (i < asize)
                            audioSourceList[sourceListDisplaySize - 1 - i] = currentZone.audioSources[i];
                        else
                            audioSourceList[sourceListDisplaySize - 1 - i] = null;
                    }

                } else {

                    audioSourceList = currentZone.audioSources;
                    audioSourceList.Reverse();

                }

                // Video Sources

                if (vsize == 0) {

                } else if (vsize <= sourceListDisplaySize) {
                    for (int i = 0; i < sourceListDisplaySize; i++) {
                        if (i < vsize)
                            videoSourceList[sourceListDisplaySize - 1 - i] = currentZone.videoSources[i];
                        else
                            videoSourceList[sourceListDisplaySize - 1 - i] = null;
                    }
                } else {

                    for (int i = 0; i < vsize; i++) {
                        videoSourceList[vsize - 1 - i] = currentZone.videoSources[i];
                    }

                }

            }

            // Iterate through lists and send display data to S+
            // Audio Sources
            for (int i = 0; i < sourceListMaxSize; i++) {

                if (asize == 0 || audioSourceList[i] == null) {
                    if (SourceListUpdate != null) {
                        SourceListUpdate((ushort)i, 1, 169, "");
                    }
                } else {
                    if (SourceListUpdate != null) {
                        SourceListUpdate((ushort)i, 1, audioSourceList[i].icon, audioSourceList[i].name);
                    }
                }

                // Video Sources
                if (vsize == 0 || videoSourceList[i] == null) {
                    if (SourceListUpdate != null) {
                        SourceListUpdate((ushort)i, 2, 169, "");
                    }
                } else {
                    if (SourceListUpdate != null) {
                        SourceListUpdate((ushort)i, 2, videoSourceList[i].icon, videoSourceList[i].name);
                    }
                }
            }

            // Send size of lists to S+
            TriggerUpdateAnalogOutput((ushort)AnalogJoins.SourceList_Listen_NumberOfItems, (ushort)asize);
            TriggerUpdateAnalogOutput((ushort)AnalogJoins.SourceList_Watch_NumberOfItems, (ushort)vsize);

        }

        /**
         * Method: BuildZoneList
         * Access: internal
         * @return: void
         * Description: ...
         */
        internal void BuildZoneList() {

            // Request zone list from S+
            string zlist = "";
            ushort zid;

            if (AvailableZoneRequest != null)
                zlist = AvailableZoneRequest().ToString();

            // Check if zlist is blank
            if (zlist == "" && defaultZoneID > 0) {
                this.zoneList.Add(Core.Zones[defaultZoneID]);
            } else {

                // Parse list into zoneList
                if (zlist.ToLower() == "all") { // Add All Zones
                    int count = Core.ZoneList.Count;
                    for (int i = 0; i < count; i++) {
                        // Add zone to zoneList
                        this.zoneList.Add(Core.ZoneList[i]);
                    }
                } else { // Add Specified Zones

                    // Error check list
                    Regex r = new Regex("[^0-9,]$");
                    if (r.IsMatch(zlist)) {
                        Core.ConsoleMessage(String.Format("[ERROR] Invalid characters in Zone List on Interface {0} [{1}].", this.id, this.name));
                    } else {
                        foreach (string zn in zlist.Replace(" ", "").Split(',')) {
                            zid = ushort.Parse(zn);
                            if (Core.Zones.ContainsKey(zid))
                                this.zoneList.Add(Core.Zones[zid]);
                        }
                    }

                }

            }

            int size = this.zoneList.Count;

            for (int i = 0; i < size; i++ ) {
                // Subscribe to zone source updates
                zoneList[i].UpdateZoneListSourceEvent += new Zone.UpdateZoneListSource(ZoneListCurrentSourceUpdateHandler);

                // Send zone details to S+ Zone List
                if (ZoneListUpdate != null)
                    ZoneListUpdate((ushort)i, Core.Sources[zoneList[i].currentSource].icon, zoneList[i].name, zoneList[i].currentSourceName);

                // Subscribe to zone lighting feedback updates
                zoneList[i].UpdateLightsStateEvent += new Zone.UpdateLightsState(this.ZoneUpdateLightsStateEvent);
            }

            TriggerUpdateAnalogOutput((ushort)AnalogJoins.ZoneList_NumberOfItems, (ushort)size);

        }

        /**
         * Method: BuildLightsList
         * Access: internal
         * @return: void
         * Description: Parse string of comma-separated Lights into a List<Light>
         */
        internal void BuildLightsList() {

            if (!_enableLightingControl)
                return;

            // Send display data to S+
            // Loads
            if (LightingLoadListUpdate != null) {

                for (ushort i = 0; i < lightingLoadsDisplaySize; i++) {

                    if (i < currentZone.lightingLoads.Count) {
                        LightingLoadListUpdate((ushort)(i + 1), currentZone.lightingLoads[(int)i].name, (ushort)currentZone.lightingLoads[(int)i].controlType);

                        // Get Current Fb States
                        LightingLoadFbHandler(currentZone.lightingLoads[(int)i].id, LightCommand.On, currentZone.lightingLoads[(int)i].isOn == true ? (ushort)1 : (ushort)0);
                        LightingLoadFbHandler(currentZone.lightingLoads[(int)i].id, LightCommand.Level, currentZone.lightingLoads[(int)i].level);

                    } else {

                        LightingLoadListUpdate((ushort)(i + 1), "", 0);

                    }

                }

            } else
                CrestronConsole.PrintLine("Error in Interface[{0}].BuildLightList: LightingLoadListUpdate delegate is null", this.id);

            // Presets
            if (LightingPresetListUpdate != null) {

                for (ushort i = 0; i < lightingPresetsDisplaySize; i++) {

                    if (i < currentZone.lightingPresets.Count) {

                        LightingPresetListUpdate((ushort)(i + 1), currentZone.lightingPresets[(int)i].name, 1);

                        // Get Current Fb State
                        LightingLoadFbHandler(currentZone.lightingPresets[(int)i].id, LightCommand.On, currentZone.lightingPresets[(int)i].isOn == true ? (ushort)1 : (ushort)0);

                    } else {

                        LightingPresetListUpdate((ushort)(i + 1), "", 0);

                    }

                }

            } else
                CrestronConsole.PrintLine("Error in Interface[{0}].BuildLightList: LightingPresetListUpdate delegate is null", this.id);

            // Send NumberOfItems for Lighting Lists
            TriggerUpdateAnalogOutput((ushort)AnalogJoins.LightingList_NumberOfLoads, (ushort)currentZone.lightingLoads.Count);
            TriggerUpdateAnalogOutput((ushort)AnalogJoins.LightingList_NumberOfPresets, (ushort)currentZone.lightingPresets.Count);

            // Show or Hide UI Lighting Button
            TriggerUpdateDigitalOutput((ushort)DigitalJoins.LightingListAvailable, (ushort)(currentZone.hasLights ? 1 : 0));

        }

        /**
         * Method: BuildShadesList
         * Access: internal
         * @return: void
         * Description: Parse string of comma-separated Shades into a List<Shades>
         */
        internal void BuildShadesList() {

            if (!_enableShadeControl)
                return;

            // Send display data to S+
            if (ShadeListUpdate != null) {

                for (ushort i = 0; i < shadeListDisplaySize; i++) {

                    if (i < currentZone.shades.Count) {

                        ShadeListUpdate((ushort)(i + 1), currentZone.shades[(int)i].name, (ushort)currentZone.shades[(int)i].controlType);

                        ShadeFbHandler(currentZone.shades[(int)i].id, ShadeCommand.Up_Fb, currentZone.shades[(int)i].Up_Fb == true ? (ushort)1 : (ushort)0);
                        ShadeFbHandler(currentZone.shades[(int)i].id, ShadeCommand.Down_Fb, currentZone.shades[(int)i].Down_Fb == true ? (ushort)1 : (ushort)0);
                        ShadeFbHandler(currentZone.shades[(int)i].id, ShadeCommand.Stop_Fb, currentZone.shades[(int)i].Stop_Fb == true ? (ushort)1 : (ushort)0);
                        ShadeFbHandler(currentZone.shades[(int)i].id, ShadeCommand.Moving_Fb, currentZone.shades[(int)i].Moving_Fb == true ? (ushort)1 : (ushort)0);

                    } else {

                        ShadeListUpdate((ushort)(i + 1), "", 0);

                    }

                }

            } else
                CrestronConsole.PrintLine("Error in Interface[{0}].BuildShadesList: ShadeListUpdate delegate is null", this.id);

            // Send NumberOfItems for Shade List
            TriggerUpdateAnalogOutput((ushort)AnalogJoins.ShadeList_NumberOfItems, (ushort)currentZone.shades.Count);

        }

        /**
         * Method: BuildHVACList
         * Access: internal
         * @return: void
         * Description: Parse string of comma-separated HVAC Zones into a List<HVAC>
         */
        internal void BuildHVACList() {

            if (!_enableHVACControl)
                return;

            HVAC unit;

            // Send display data to S+
            if (HVACListUpdate != null) {

                for (ushort i = 0; i < hvacListDisplaySize; i++) {

                    if (i < currentZone.hvacs.Count) {

                        unit = currentZone.hvacs[(int)i];

                        // Send List details
                        HVACListUpdate((ushort)(i + 1), unit.name, (ushort)unit.controlType, (ushort)unit.fanOrPower, (ushort)(unit.hasFanSpeedControl ? 1 : 0));

                        // Send Feedback State
                        HVACFbHandler(unit.id, HVACCommand.SystemMode_Off_Fb, (ushort)(unit.currentSystemMode == "Off" ? 1: 0));
                        HVACFbHandler(unit.id, HVACCommand.SystemMode_Cool_Fb, (ushort)(unit.currentSystemMode == "Cool" ? 1 : 0));
                        HVACFbHandler(unit.id, HVACCommand.SystemMode_Heat_Fb, (ushort)(unit.currentSystemMode == "Heat" ? 1 : 0));
                        HVACFbHandler(unit.id, HVACCommand.SystemMode_Auto_Fb, (ushort)(unit.currentSystemMode == "Auto" ? 1 : 0));
                        
                        // Fan Mode or Power State
                        if (unit.fanOrPower == HVACControlType.FanPwr_Fan) {
                            HVACFbHandler(unit.id, HVACCommand.FanMode_Auto_Fb, (ushort)(unit.currentFanMode == "Auto" ? 1 : 0));
                            HVACFbHandler(unit.id, HVACCommand.FanMode_On_Fb, (ushort)(unit.currentFanMode == "On" ? 1 : 0));
                        } else if (unit.fanOrPower == HVACControlType.FanPwr_Pwr) {
                            HVACFbHandler(unit.id, HVACCommand.Power_On_Fb, (ushort)(unit.powerIsOn ? 1 : 0));
                            HVACFbHandler(unit.id, HVACCommand.Power_Off_Fb, (ushort)(unit.powerIsOn ? 0 : 1));
                        }

                        if (unit.hasFanSpeedControl) {
                            HVACFbHandler(unit.id, HVACCommand.FanSpeed_Low_Fb, (ushort)(unit.currentFanSpeed == "Low" ? 1 : 0));
                            HVACFbHandler(unit.id, HVACCommand.FanSpeed_Medium_Fb, (ushort)(unit.currentFanSpeed == "Medium" ? 1 : 0));
                            HVACFbHandler(unit.id, HVACCommand.FanSpeed_High_Fb, (ushort)(unit.currentFanSpeed == "High" ? 1 : 0));
                            HVACFbHandler(unit.id, HVACCommand.FanSpeed_Max_Fb, (ushort)(unit.currentFanSpeed == "Max" ? 1 : 0));
                        }

                        HVACFbHandler(unit.id, HVACCommand.CoolCall_Fb, (ushort)(unit.coolCallActive ? 1 : 0));
                        HVACFbHandler(unit.id, HVACCommand.HeatCall_Fb, (ushort)(unit.heatCallActive ? 1 : 0));
                        HVACFbHandler(unit.id, HVACCommand.FanCall_Fb, (ushort)(unit.fanCallActive ? 1 : 0));

                        HVACFbHandler(unit.id, HVACCommand.TempAmbient, unit.currentAmbientTemp);
                        HVACFbHandler(unit.id, HVACCommand.TempSetpoint, unit.currentSetpointTemp);
                        

                    } else {

                        HVACListUpdate((ushort)(i + 1), "", 0, 0, 0);

                    }

                }

            }

            // Send NumberOfItems for HVAC List
            TriggerUpdateAnalogOutput((ushort)AnalogJoins.HVACList_NumberOfItems, (ushort)currentZone.hvacs.Count);

            // Show/Hide HVAC icon
            TriggerUpdateDigitalOutput((ushort)DigitalJoins.HVACListAvailable, (ushort)(currentZone.hasHVAC ? 1 : 0));

        }

        /**
         * Method: ParseSecurityKeypads
         * Access: internal
         * @return: void
         * Description: Parse string of comma-separated SecurityKeypads into a List<SecurityKeypad>
         */
        internal void ParseSecurityKeypads () {

            if (!_enableSecurityControl)
                return;

            // Request list from S+
            string lTmp = SecurityKeypadListRequest().ToString();

            if (lTmp == "")
                return;

            // Error check list
            Regex r = new Regex("[^0-9,]$");
            if (r.IsMatch(lTmp)) {
                Core.ConsoleMessage(String.Format("[ERROR] Invalid characters in Security Keypad List on Interface {0} [{1}].", this.id, this.name));
            } else {
                string[] list = lTmp.Replace(" ", "").Split(',');
                ushort kpid;
                for (int i = 0; i < list.Length; i++) {
                    kpid = ushort.Parse(list[i]);
                    if (Core.SecurityKeypads.ContainsKey(kpid))
                        securityKeypadList.Add(Core.SecurityKeypads[kpid]);
                }

                // If keypads are available, enable interface button and set first keypad
                if (hasSecurityKeypad) {
                    SetCurrentSecurityKeypad(securityKeypadList[0].id);
                    UpdateDigitalOutput((ushort)DigitalJoins.SecurityKeypadAvailable, 1);
                }
            }

        }

        /**
         * Method: SetCurrentSecurityKeypad
         * Access: public
         * @return: void
         * Description: Clear association with current keypad, set new subscriptions, update feedback states
         */
        public void SetCurrentSecurityKeypad (ushort _id) {

            if (!_enableSecurityControl || !Core.SecurityKeypads.ContainsKey(_id)) {
                return;
            }

            // Unsubscribe from current Security Keypad
            if (currentSecurityKeypad != null) {
                currentSecurityKeypad.UpdateEvent     -= this.SecurityKeypadFbHandler;
                currentSecurityKeypad.TextUpdateEvent -= this.SecurityKeypadTextFbHandler;
            }

            // Set new Security Keypad
            currentSecurityKeypad = Core.SecurityKeypads[_id];

            // Subscribe to new Security Keypad
            currentSecurityKeypad.UpdateEvent     += this.SecurityKeypadFbHandler;
            currentSecurityKeypad.TextUpdateEvent += this.SecurityKeypadTextFbHandler;

            // Get current feedback
            SecurityKeypadFbUpdate((ushort)SecurityCommand.Disarm_Fb, (ushort)(currentSecurityKeypad.currentArmState == SecurityArmState.Disarmed ? 1 : 0));
            SecurityKeypadFbUpdate((ushort)SecurityCommand.ArmAway_Fb, (ushort)(currentSecurityKeypad.currentArmState == SecurityArmState.ArmAway ? 1 : 0));
            SecurityKeypadFbUpdate((ushort)SecurityCommand.ArmHome_Fb, (ushort)(currentSecurityKeypad.currentArmState == SecurityArmState.ArmHome ? 1 : 0));
            SecurityKeypadFbUpdate((ushort)SecurityCommand.ArmNight_Fb, (ushort)(currentSecurityKeypad.currentArmState == SecurityArmState.ArmNight ? 1 : 0));

            SecurityKeypadFbUpdate((ushort)SecurityCommand.Custom_01_Fb, (ushort)(currentSecurityKeypad.customButtonFeedback[0] ? 1 : 0));
            SecurityKeypadFbUpdate((ushort)SecurityCommand.Custom_02_Fb, (ushort)(currentSecurityKeypad.customButtonFeedback[1] ? 1 : 0));
            SecurityKeypadFbUpdate((ushort)SecurityCommand.Custom_03_Fb, (ushort)(currentSecurityKeypad.customButtonFeedback[2] ? 1 : 0));
            SecurityKeypadFbUpdate((ushort)SecurityCommand.Custom_04_Fb, (ushort)(currentSecurityKeypad.customButtonFeedback[3] ? 1 : 0));
            SecurityKeypadFbUpdate((ushort)SecurityCommand.Custom_05_Fb, (ushort)(currentSecurityKeypad.customButtonFeedback[4] ? 1 : 0));
            SecurityKeypadFbUpdate((ushort)SecurityCommand.Custom_06_Fb, (ushort)(currentSecurityKeypad.customButtonFeedback[5] ? 1 : 0));

            SecurityKeypadTextFbUpdate((ushort)SecurityCommand.Text_KeypadStars, currentSecurityKeypad.currentKeypadStars);
            SecurityKeypadTextFbUpdate((ushort)SecurityCommand.Text_StatusFeedback_Line1, currentSecurityKeypad.currentStatusText_Line1);
            SecurityKeypadTextFbUpdate((ushort)SecurityCommand.Text_StatusFeedback_Line2, currentSecurityKeypad.currentStatusText_Line2);

            SecurityKeypadTextFbUpdate((ushort)SecurityCommand.Function_States, currentSecurityKeypad.getEncodedFunctionStates());
            SecurityKeypadTextFbUpdate((ushort)SecurityCommand.Function_01_Label, currentSecurityKeypad.functionButtonLabels[0]);
            SecurityKeypadTextFbUpdate((ushort)SecurityCommand.Function_02_Label, currentSecurityKeypad.functionButtonLabels[1]);
            SecurityKeypadTextFbUpdate((ushort)SecurityCommand.Function_03_Label, currentSecurityKeypad.functionButtonLabels[2]);
            SecurityKeypadTextFbUpdate((ushort)SecurityCommand.Function_04_Label, currentSecurityKeypad.functionButtonLabels[3]);

            SecurityKeypadFbUpdate((ushort)SecurityCommand.Number_Of_Custom_Buttons, (ushort)currentSecurityKeypad.customButtonCount);
            if (currentSecurityKeypad.customButtonLabels.Count > 0)
                SecurityKeypadTextFbUpdate((ushort)SecurityCommand.Custom_01_Label, currentSecurityKeypad.customButtonLabels[0]);
            else
                SecurityKeypadTextFbUpdate((ushort)SecurityCommand.Custom_01_Label, "");
            if (currentSecurityKeypad.customButtonLabels.Count > 1)
                SecurityKeypadTextFbUpdate((ushort)SecurityCommand.Custom_02_Label, currentSecurityKeypad.customButtonLabels[1]);
            else
                SecurityKeypadTextFbUpdate((ushort)SecurityCommand.Custom_02_Label, "");
            if (currentSecurityKeypad.customButtonLabels.Count > 2)
                SecurityKeypadTextFbUpdate((ushort)SecurityCommand.Custom_03_Label, currentSecurityKeypad.customButtonLabels[2]);
            else
                SecurityKeypadTextFbUpdate((ushort)SecurityCommand.Custom_03_Label, "");
            if (currentSecurityKeypad.customButtonLabels.Count > 3)
                SecurityKeypadTextFbUpdate((ushort)SecurityCommand.Custom_04_Label, currentSecurityKeypad.customButtonLabels[3]);
            else
                SecurityKeypadTextFbUpdate((ushort)SecurityCommand.Custom_04_Label, "");
            if (currentSecurityKeypad.customButtonLabels.Count > 4)
                SecurityKeypadTextFbUpdate((ushort)SecurityCommand.Custom_05_Label, currentSecurityKeypad.customButtonLabels[4]);
            else
                SecurityKeypadTextFbUpdate((ushort)SecurityCommand.Custom_05_Label, "");
            if (currentSecurityKeypad.customButtonLabels.Count > 5)
                SecurityKeypadTextFbUpdate((ushort)SecurityCommand.Custom_06_Label, currentSecurityKeypad.customButtonLabels[5]);
            else
                SecurityKeypadTextFbUpdate((ushort)SecurityCommand.Custom_06_Label, "");

        }

        /**
         * Method: BuildShareZones
         * Access: internal
         * @return: void
         * Description: Parse string of comma-separated zones into a List<Zone>
         */
        internal void BuildShareZones() {

            // Request list from S+
            string sList = "";
            ushort sid;

            if (ShareListRequest != null)
                sList = ShareListRequest().ToString();

            // Return if no Share zones defined
            if (sList == null || sList == "")
                return;

            // Parse into List
            if (sList.ToLower() == "all") { // Add All Zones
                int count = Core.ZoneList.Count;
                for (int i = 0; i < count; i++) {
                    // Add zone to zoneList
                    this.shareZones.Add(Core.ZoneList[i]);
                }
            } else { // Add Specified Zones

                // Error check list
                Regex r = new Regex("[^0-9,]$");
                if (r.IsMatch(sList)) {
                    Core.ConsoleMessage(String.Format("[ERROR] Invalid characters in Share Zones List on Interface {0} [{1}].", this.id, this.name));
                } else {
                    foreach (string zn in sList.Replace(" ", "").Split(',')) {
                        sid = ushort.Parse(zn);
                        if (Core.Zones.ContainsKey(sid))
                            this.shareZones.Add(Core.Zones[sid]);
                    }
                }

            }
        }

        /**
         * Method: UpdateShareList
         * Access: internal
         * @return: void
         * Description: Compare current source to source lists in all Share zones, then output a list of
         *              zones that have that source available.
         */
        internal void UpdateShareList() {
            ushort count = 0;

            // Unregister all current shareList items
            for (int i = 0; i < shareList.Count; i++) {
                shareList[i].Unregister();
            }

            // Reset list
            this.shareList = new List<shareListEvents>();

            // If current zone is Off, just disable share list
            if (currentZone.currentSource != 0) {
                // Cache local reference to currentSource
                Source src = Core.Sources[currentZone.currentSource];

                // Check all share-enabled zones and create list of valid share targets
                for (int i = 0; i < shareZones.Count; i++) {
                    // Skip zone if it is the current zone
                    if (shareZones[i] != currentZone) {
                        // Only check zone if it can actually play the source
                        if ((src.isVideoSource && shareZones[i].hasVideo) || (src.isAudioSource && shareZones[i].hasAudio)) {
                            if (shareZones[i].hasSource(src)) {
                                shareList.Add(new shareListEvents());
                                shareList[count].index = count;
                                shareList[count].intRef = this;
                                shareList[count].znRef = shareZones[i];
                                shareList[count].Register();
                                count++;
                            }
                        }
                    }
                }

                // Parse valid target list details to S+ for display
                for (int i = 0; i < shareList.Count; i++) {
                    if (TriggerSendShareListItem != null)
                        TriggerSendShareListItem((ushort)i, shareList[i].znRef.name, (ushort)(shareList[i].znRef.currentSource == currentZone.currentSource ? 1 : 0), (ushort)(shareList[i].znRef.muteState ? 1 : 0), shareList[i].znRef.volume);
                }

                // Update availability of share icon & list size
                if (count > 0)
                    TriggerUpdateDigitalOutput((ushort)DigitalJoins.ShareListAvailable, 1);
                else
                    TriggerUpdateDigitalOutput((ushort)DigitalJoins.ShareListAvailable, 0);

                TriggerUpdateAnalogOutput((ushort)AnalogJoins.ShareList_NumberOfItems, (ushort)count);
            } else {
                TriggerUpdateDigitalOutput((ushort)DigitalJoins.ShareListAvailable, 0);
            }
        }

        /**
         * Method: UpdateDisplayControlList
         * Access: internal
         * @return: void
         * Description: ...
         */
        internal void UpdateDisplayControlList() {
            for (int i = 0; i < 10; i++) {
                if (i < currentZone.display.buttonCount) {
                    if (TriggerSendDisplayControlListItem != null)
                        TriggerSendDisplayControlListItem((ushort)(i+1), currentZone.display.buttonLabels[i], 1);
                } else {
                    if (TriggerSendDisplayControlListItem != null)
                        TriggerSendDisplayControlListItem((ushort)(i+1), "", 0);
                }
            }
        }


        /**
         * Method: UpdateVoiceControlFeedback
         * Access: internal
         * @return: void
         * Description: Gets current stored values of Voice Control registration variables
         */
        internal void UpdateVoiceControlFeedback() {
            if (VoiceControlEvent == null || VoiceControlIDEvent == null)
                return;

            VoiceControlEvent((ushort)(Core.VoiceControlConnected ? 1 : 0), "connected");
            VoiceControlEvent((ushort)(Core.VoiceControlRegistered ? 1 : 0), "registered");
            VoiceControlEvent((ushort)(Core.VoiceControlReady ? 1 : 0), "ready");
            VoiceControlIDEvent(Core.VoiceControlRegistrationID);

        }

        /**
         * Method: UpdateSourceListFb
         * Access: internal
         * @return: void
         * Description: Create feedback string for Source Lists and send to S+
         */
        internal void UpdateSourceListFb() {
            string aList = "";
            string vList = "";

            for (int i = 0; i < sourceListMaxSize; i++ ) {
                if (audioSourceList[i] != null) {
                    if (currentZone.currentSource == audioSourceList[i].id)
                        aList += "1";
                    else
                        aList += "0";
                } else
                    aList += "0";

                if (videoSourceList[i] != null) {
                    if (currentZone.currentSource == videoSourceList[i].id)
                        vList += "1";
                    else
                        vList += "0";
                } else
                    vList += "0";
            }

            TriggerUpdateStringOutput((ushort)SerialJoins.ListenListFb, aList);
            TriggerUpdateStringOutput((ushort)SerialJoins.WatchListFb, vList);
        }

        /**
         * Method: UpdateSysMenuList
         * Access: internal
         * @return: void
         * Description: Recheck availability of SysMenu items and update display list
         */
        internal void UpdateSysMenuList() {
            sysMenuList = new List<sysMenuItem>();

            // Check for Voice Control Setup permission
            if (_enableVoiceControlSetup && Core.VoiceControlEnabled) {
                sysMenuList.Add(new sysMenuItem("Voice Control", 62, "VoiceCtrlPopup"));
            }

            // Check for TV Control button
            if (currentZone.hasDisplay) {
                sysMenuList.Add(new sysMenuItem("TV Control", 143, "DispCmdPopup"));
            }

            // Check for Lift Button
            if (currentZone.hasLift) {
                sysMenuList.Add(new sysMenuItem("Lift Control", 103, "LiftCmdPopup"));
            }
            
            // Check for AVR Button
            // ...

            // Send list to Interface
            for (int i = 0; i < sysMenuList.Count; i++ ) {
                SysMenuListUpdate((ushort)i, sysMenuList[i].label, sysMenuList[i].icon);
            }

            // Clear out unused buttons
            for (int i = sysMenuList.Count; i < 6; i++ ) {
                SysMenuListUpdate((ushort)i, "", 169);
            }

            // Send new list size
            TriggerUpdateAnalogOutput((ushort)AnalogJoins.SysMenu_NumberOfItems, (ushort)sysMenuList.Count);
        }

        /**
         * Method: RefreshLiftFb
         * Access: public
         * @return: void
         * Description: Request current feedback status from currentZone.lift
         */
        public void RefreshLiftFb() {
            if (!currentZone.hasLift)
                return;

            TriggerSendLiftFb(1, (ushort)(currentZone.lift.lastDirection == 1 ? 1 : 0)); // Raise
            TriggerSendLiftFb(2, (ushort)(currentZone.lift.lastDirection == 0 && currentZone.lift.type != LiftType.SingleRelay ? 1 : 0)); // Stop
            TriggerSendLiftFb(3, (ushort)(currentZone.lift.lastDirection == -1 ? 1 : 0)); // Lower
            TriggerSendLiftFb(4, (ushort)(currentZone.lift.holdEnabled ? 1 : 0)); // Hold
            TriggerSendLiftFb(5, (ushort)(currentZone.lift.type == LiftType.SingleRelay ? 0 : 1)); // Stop Visible
        }

        /**
         * Method: ZoneVolumeEvent
         * Access: public
         * @param: ushort _action
         * @param: ushort _state
         * @return: void
         * Description: Handles volume button presses for CurrentZone
         */
        public void ZoneVolumeEvent(ushort _action, ushort _state) {
            if(_action == 0) { // Volume Down
                this.currentZone.VolumeDown(_state == 0 ? false : true);
            } else if(_action == 1) { // Volume Up
                this.currentZone.VolumeUp(_state == 0 ? false : true);
            } else if (_action == 3) { // Mute
                this.currentZone.VolumeMuteToggle(_state);
                if (_state == 1)
                    this.currentZone.ToggleMuteState();
            }
        }

        /**
         * Method: LightingLoadEvent
         * Access: public
         * @param: ushort _index
         * @param: ushort _action
         * @param: ushort _state
         * @return: void
         * Description: Handles lighting button presses
         */
        public void LightingLoadEvent(ushort _index, ushort _action, ushort _state) {

            if (_index < currentZone.lightingLoads.Count)
                currentZone.lightingLoads[_index].sendCommand((LightCommand)_action, _state);

        }

        /**
         * Method: LightingPresetEvent
         * Access: public
         * @param: ushort _index
         * @param: ushort _action
         * @param: ushort _state
         * @return: void
         * Description: Handles lighting button presses
         */
        public void LightingPresetEvent(ushort _index, ushort _action, ushort _state) {

            if (_index < currentZone.lightingPresets.Count)
                currentZone.lightingPresets[_index].sendCommand((LightCommand)_action, _state);

        }

        /**
         * Method: ShadeEvent
         * Access: public
         * Description: Handles Shade button presses
         */
        public void ShadeEvent(ushort _index, ushort _action, ushort _state) {

            if (_index < currentZone.shades.Count)
                currentZone.shades[_index].SendShadeCommand((ShadeCommand)_action, _state);

        }

        /**
         * Method: HVACEvent
         * Access: public
         * Description: Handles HVAC button presses
         */
        public void HVACEvent(ushort _index, ushort _action, ushort _state) {

            if (_index < currentZone.hvacs.Count)
                currentZone.hvacs[_index].SendHVACCommand((HVACCommand)_action, _state);

        }

        /**
         * Method: SecurityKeypadEvent
         * Access: public
         * Description: Handles Security Keypad button presses
         */
        public void SecurityKeypadEvent(ushort _action, ushort _state) {

            if (currentSecurityKeypad != null)
                currentSecurityKeypad.SendSecurityCommand((SecurityCommand)_action, _state);

        }

        /**
         * Method: PoolEvent
         * Access: public
         * Description: Handles Security Keypad button presses
         */
        public void PoolEvent(ushort _action, ushort _state, string _text) {

            if (Core.pool != null)
                Core.pool.SendPoolCommand((PoolCommand)_action, _state, _text);

        }

        /**
         * Method: VolumeDirectInput
         * Access: public
         * Description: Receives analog value from slider on touchpanel and passes to current Zone
         */
        public void VolumeDirectInput(ushort _level) {

            if (!currentZone.hasDirectVolume)
                return;

            currentZone.SetVolumeDirect(_level);

        }

        /**
         * Method: TriggerUpdateDigitalOutput
         * Access: internal
         * @param: ushort _join
         * @param: ushort _state
         * @return: void
         * Description: ...
         */
        internal void TriggerUpdateDigitalOutput(ushort _join, ushort _state) {
            if (UpdateDigitalOutput != null) {
                UpdateDigitalOutput(_join, _state);
            }
        }

        /**
         * Method: TriggerUpdateAnalogOutput
         * Access: internal
         * @param: ushort _join
         * @param: ushort _state
         * @return: void
         * Description: ...
         */
        internal void TriggerUpdateAnalogOutput(ushort _join, ushort _state) {
            if (UpdateAnalogOutput != null) {
                UpdateAnalogOutput(_join, _state);
            }
        }

        /**
         * Method: TriggerUpdateStringOutput
         * Access: internal
         * @param: ushort _join
         * @param: string _string
         * @return: void 
         * Description: ...
         */
        internal void TriggerUpdateStringOutput(ushort _join, string _string) {
            if (UpdateStringOutput != null) {
                UpdateStringOutput(_join, (SimplSharpString)_string);
            }
        }


        /**
         * Method: TriggerUpdateStringOutput
         * Access: internal
         * Description: Sends text from Voice Control setup menu to Core when 'Send' button is pressed on Interface
         */
        public void VoiceControlSendRegistrationCode(string _code) {

            Core.VoiceControlSendRegistrationCode(_code);

        }

        //===================// Event Handlers //===================//

        /**
         * Method: ZoneCurrentSourceNameHandler
         * Access: public
         * @param: SimplSharpString _name
         * @return: void
         * Description: ...
         */
        public void ZoneCurrentSourceNameHandler(SimplSharpString _name) {
            // Send new source name to S+
            TriggerUpdateStringOutput((ushort)SerialJoins.CurrentSource, currentZone.currentSourceName);
        }

        /**
         * Method: ZoneCurrentSourceIDHandler
         * Access: public
         * @param: ushort _sid
         * @return: void
         * Description: ...
         */
        public void ZoneCurrentSourceIDHandler(ushort _sid) {
            // Send new source ID to S+
            TriggerUpdateAnalogOutput((ushort)AnalogJoins.CurrentSource, _sid);

            // Update Watch/Listen List Fb
            UpdateSourceListFb();

            // Update Share List
            UpdateShareList();
        }

        /**
         * Method: ZoneCurrentVolumeHandler
         * Access: public
         * @param: ushort _vol
         * @return: void
         * Description: Subscribes to CurrentZone's UpdateCurrentVolumeEvent and forwards volume
         *              state to S+
         */
        public void ZoneCurrentVolumeHandler(ushort _vol) {
            TriggerUpdateAnalogOutput((ushort)AnalogJoins.Volume_Fb, _vol);
        }

        /**
         * Method: ZoneMuteStateHandler
         * Access: public
         * @param: ushort _state
         * @return: void
         * Description: Subscribes to CurrentZone's UpdateMuteStateEvent and forwards mute
         *              state to S+
         */
        public void ZoneMuteStateHandler(ushort _state) {
            TriggerUpdateDigitalOutput((ushort)DigitalJoins.Mute_Fb, _state);
        }

        /**
         * Method: ZoneUpdateDisplayAvailableHandler
         * Access: public
         * @param: ushort _state
         * @return: void
         * Description: ...
         */
        public void ZoneUpdateDisplayAvailableHandler(ushort _state) {
            //CrestronConsole.PrintLine("Interface {0} ZoneDisplayAvailable event: {1}", this.id, _state);
        }

        /**
         * Method: ZoneUpdateLightsStateEvent
         * Description: Receives update from CurrentZone's UpdateLightsStateEvent to pass zone light fb status
         *              to interface.
         */
        public void ZoneUpdateLightsStateEvent(Zone _zn, bool _state) {

            // Send light status to interface's ZoneList
            ushort pos = (ushort)zoneList.IndexOf(_zn);

            if (ZoneListLightsUpdate != null)
                ZoneListLightsUpdate(pos, (ushort)(_state ? 1 : 0));

        }

        /**
         * Method: ZoneListCurrentSourceUpdateHandler
         * Access: public
         * @param: Zone _zn
         * @param: ushort _sid
         * @param: ushort _sic
         * @param: string _snm
         * @return: void
         * Description: ...
         */
        public void ZoneListCurrentSourceUpdateHandler(Zone _zn, ushort _sid, ushort _sic, string _snm) {
            // Get position of Zone in ZoneList
            ushort pos = (ushort)zoneList.IndexOf(_zn);

            // Fire delegate to S+ class with list update details
            if (ZoneListUpdate != null)
                ZoneListUpdate(pos, _sic, _zn.name, _snm);
        }

        /**
         * Method: LiftCommandFbHandler
         * Access: public
         * @param: ushort _cmd
         * @param: ushort _state
         * @return: void
         * Description: Accepts Lift Fb events from currentZone.lift and forwards to S+
         */
        public void LiftCommandFbHandler(ushort _cmd, ushort _state) {
            TriggerSendLiftFb(_cmd, _state);
        }

        /**
         * Method: LightLoadFbHandler
         * Access: public
         * @return: void
         * Description: 
         */
        public void LightingLoadFbHandler(ushort _lightID, LightCommand _action, ushort _state) {
            int loadSlot   = -1;
            int presetSlot = -1;
            int maxLoad    = currentZone.lightingLoads.Count;
            int maxPreset  = currentZone.lightingPresets.Count;

            // Lookup LightID in currentZone.lightingLoads to get loadSlot
            for (int i = 0; i < maxLoad; i++) {
                if (currentZone.lightingLoads[i].id == _lightID) {
                    loadSlot = i+1;
                    break;
                }
            }

            // Lookup LightID in currentZone.lightingPresets to get presetSlot
            for (int i = 0; i < maxPreset; i++) {
                if (currentZone.lightingPresets[i].id == _lightID) {
                    presetSlot = i+1;
                    break;
                }
            }

            // Send Action and State to S+ with slot number
            if (LightingLoadFbUpdate != null && loadSlot != -1)
                LightingLoadFbUpdate((ushort)loadSlot, (ushort)_action, _state);

            if (LightingPresetFbUpdate != null && presetSlot != -1)
                LightingPresetFbUpdate((ushort)presetSlot, (ushort)_action, _state);


        }

        /**
         * Method: ShadeFbHandler
         * Access: public
         * @return: void
         * Description: 
         */
        public void ShadeFbHandler(ushort _shadeID, ShadeCommand _action, ushort _state) {
            int slot = -1;
            int maxLoad = currentZone.shades.Count;

            // Lookup ShadeiD in currentZone.shades to get slot
            for (int i = 0; i < maxLoad; i++) {
                if (currentZone.shades[i].id == _shadeID) {
                    slot = i + 1;
                    break;
                }
            }

            // Send Action and State to S+ with slot number
            if (ShadeFbUpdate != null && slot != -1)
                ShadeFbUpdate((ushort)slot, (ushort)_action, _state);

        }

        /**
         * Method: HVACFbHandler
         * Access: public
         * @return: void
         * Description: 
         */
        public void HVACFbHandler(ushort _hvacID, HVACCommand _action, ushort _state) {
            int slot = -1;
            int max = currentZone.hvacs.Count;

            // Lookup HVACID in currentZone.hvacs to get slot
            for (int i = 0; i < max; i++) {
                if (currentZone.hvacs[i].id == _hvacID) {
                    slot = i + 1;
                    break;
                }
            }

            // Send Action and State to S+ with slot number
            if (HVACFbUpdate != null && slot != -1)
                HVACFbUpdate((ushort)slot, (ushort)_action, _state);

        }

        /**
         * Method: SecurityKeypadFbHandler
         * Access: public
         * @return: void
         * Description: 
         */
        public void SecurityKeypadFbHandler(SecurityCommand _action, ushort _state) {

            // Send Action and State to S+
            if (SecurityKeypadFbUpdate != null)
                SecurityKeypadFbUpdate((ushort)_action, _state);

        }

        /**
         * Method: SecurityKeypadTextFbHandler
         * Access: public
         * @return: void
         * Description: 
         */
        public void SecurityKeypadTextFbHandler(SecurityCommand _action, string _txt) {

            // Send Action and State to S+
            if (SecurityKeypadTextFbUpdate != null)
                SecurityKeypadTextFbUpdate((ushort)_action, _txt);

        }

        /**
         * Method: PoolFbHandler
         * Access: public
         * @return: void
         * Description: 
         */
        public void PoolFbHandler(PoolCommand _action, ushort _state, string _txt) {

            // Send Action and State to S+
            if (PoolFbUpdate != null)
                PoolFbUpdate((ushort)_action, _state, _txt);

        }

        /**
         * Method: StartupCompleteEventHandler
         * Description: ...
         */
        public void StartupCompleteEventHandler() {
            TriggerUpdateDigitalOutput((ushort)DigitalJoins.StartupComplete, 1);
        }

        /**
         * Method: StartupProgressEventHandler
         * Description: ...
         */
        public void StartupProgressEventHandler(int _val) {
            TriggerUpdateAnalogOutput((ushort)AnalogJoins.StartupProgress, (ushort)_val);

        }

        //===================// Get / Set //===================//

        /**
         * Method: invertSourceLists
         * Description: ...
         */
        public ushort invertSourceLists { 
            set { _invertSourceLists = value == 1 ? true : false; } 
            get { return (ushort)(_invertSourceLists ? 1 : 0); }
        }

        /**
         * Method: enableLightingControl
         * Description: ...
         */
        public ushort enableLightingControl { 
            set { _enableLightingControl = value == 1 ? true : false; } 
            get { return (ushort)(_enableLightingControl ? 1 : 0); } 
        }

        /**
         * Method: enableShadeControl
         * Description: ...
         */
        public ushort enableShadeControl { 
            set { _enableShadeControl = value == 1 ? true : false; } 
            get { return (ushort)(_enableShadeControl ? 1 : 0); } 
        }

        /**
         * Method: enableHVACControl
         * Description: ...
         */
        public ushort enableHVACControl { 
            set { _enableHVACControl = value == 1 ? true : false; } 
            get { return (ushort)(_enableHVACControl ? 1 : 0); } 
        }

        /**
         * Method: enableSecurityControl
         * Description: ...
         */
        public ushort enableSecurityControl { 
            set { _enableSecurityControl = value == 1 ? true : false; } 
            get { return (ushort)(_enableSecurityControl ? 1 : 0); } 
        }

        /**
         * Method: enablePoolControl
         * Description: ...
         */
        public ushort enablePoolControl { 
            set { _enablePoolControl = value == 1 ? true : false; } 
            get { return (ushort)(_enablePoolControl ? 1 : 0); } 
        }

        /**
         * Method: enableVoiceControlSetup
         * Description: ...
         */
        public ushort enableVoiceControlSetup { 
            set { _enableVoiceControlSetup = value == 1 ? true : false; } 
            get { return (ushort)(_enableVoiceControlSetup ? 1 : 0); } 
        }

        /**
         * Method: enableStartupEvents
         * Description: ...
         */
        public ushort enableStartupEvents {
            set {
                _enableStartupEvents = value == 1 ? true : false;
                if (_enableStartupEvents) {
                    Core.startupProgressEvent += new DelegateInt(this.StartupProgressEventHandler);
                    Core.startupCompleteEvent += new DelegateEmpty(this.StartupCompleteEventHandler);
                }
            }
            get {
                return (ushort)(_enableStartupEvents ? 1 : 0);
            }
        }


    } // End Interface Class

    internal class sysMenuItem {
        public string label;
        public ushort icon;
        public string action;

        public sysMenuItem(string _label, ushort _icon, string _action) {
            label  = _label;
            icon   = _icon;
            action = _action;
        }
    }

    // Stores event handlers tied to Share-enabled Zones. When a Zone event is invoked, this class injects the index of the Zone on
    // the touchpanel's Share list, then fires a delegate on the Interface to pass the data to S+
    internal class shareListEvents {
        public ushort index;
        public Interface intRef;
        public Zone znRef;

        // Register/Unregister from Zone
        public void Register() {
            znRef.UpdateCurrentSourceIDEvent += new DelegateUshort(UpdateShareSourceHandler);
            znRef.UpdateCurrentVolumeEvent   += new DelegateUshort(UpdateShareVolumeHandler);
            znRef.UpdateMuteStateEvent       += new DelegateUshort(UpdateShareMuteStateHandler);
        }

        public void Unregister() {
            znRef.UpdateCurrentSourceIDEvent -= UpdateShareSourceHandler;
            znRef.UpdateCurrentVolumeEvent   -= UpdateShareVolumeHandler;
            znRef.UpdateMuteStateEvent       -= UpdateShareMuteStateHandler;
        }


        // Event Handlers
        public void UpdateShareMuteStateHandler(ushort _state) {
            intRef.TriggerSendShareMuteState(index, _state);
        }

        public void UpdateShareVolumeHandler(ushort _vol) {
            intRef.TriggerSendShareVolume(index, _vol);
        }

        public void UpdateShareSourceHandler(ushort _src) {
            if(_src == intRef.currentZone.currentSource)
                intRef.TriggerSendShareState(index, 1);
            else
                intRef.TriggerSendShareState(index, 0);
        }
    }

    public enum InterfaceType {

        Unknown,
        Type_Keypad,
        Type_HR150,
        Type_TSR302,
        Type_iPad,
        Type_iPhone,
        Type_TSW,
        Type_TSD,
        Type_3SMD,
        Type_4SM,
        Type_TST,
        Type_XPanel

    }

    public enum DigitalJoins {

        Popup_Request_SysMenu = 1,
        Popup_Request_RoomList,
        Popup_Request_SourceList_Audio,
        Popup_Request_SourceList_Video,
        Popup_Request_Power,
        Popup_Request_About,
        Popup_Request_Display_Commands,
        Popup_Request_Lift_Commands,
        Popup_Request_AVR_Commands,
        Popup_Request_Voice_Control,
        Mute_Fb = 16,
        EnterSetup = 21,
        ShareListAvailable = 31,
        LightingListAvailable,
        ShadeListAvailable,
        HVACListAvailable,
        SecurityKeypadAvailable,
        PoolControlAvailable,
        DirectVolumeAvailable,
        StartupComplete
    }

    public enum AnalogJoins {

        CurrentZone = 1,
        CurrentSource,
        ZoneList_NumberOfItems,
        SourceList_Listen_NumberOfItems,
        SourceList_Watch_NumberOfItems,
        SysMenu_NumberOfItems,
        ShareList_NumberOfItems,
        Volume_Fb = 16,
        ZoneListIcons = 30,
        LightingList_NumberOfLoads = 32,
        LightingList_NumberOfPresets,
        ShadeList_NumberOfItems = 35,
        HVACList_NumberOfItems,
        SecurityKeypadList_NumberOfItems,
        StartupProgress

    }

    public enum SerialJoins {

        CurrentZone = 1,
        CurrentSource,
        ListenListFb,
        WatchListFb

    }
}