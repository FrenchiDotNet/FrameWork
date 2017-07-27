using System;
using System.Text;
using System.Collections.Generic;
using Crestron.SimplSharp;

namespace FrameWork {

    /**
     * Class:       Core
     * @author:     Ryan French
     * @version:    1.1
     * Description: ...
     */
    public static class Core {

        //===================// Members //===================//

        internal static Dictionary<ushort, Zone> Zones;
        internal static Dictionary<ushort, Source> Sources;
        internal static Dictionary<ushort, Display> Displays;
        internal static Dictionary<ushort, Interface> Interfaces;
        internal static Dictionary<ushort, Lift> Lifts;
        internal static Dictionary<ushort, Light> Lights;
        internal static Dictionary<ushort, Shade> Shades;
        internal static Dictionary<ushort, HVAC> HVACs;
        //internal static List<AudioDevice> AudioDevices;

        internal static SysInfoServer siServer;

        static private CTimer startupTimer;

        static public DelegateUshort startupCompleteEvent { get; set; }

        //===================// Constructor //===================//

        static Core() {
            Sources    = new Dictionary<ushort, Source>();
            Zones      = new Dictionary<ushort, Zone>();
            Displays   = new Dictionary<ushort, Display>();
            Interfaces = new Dictionary<ushort, Interface>();
            Lifts      = new Dictionary<ushort, Lift>();
            Lights     = new Dictionary<ushort, Light>();
            Shades     = new Dictionary<ushort, Shade>();
            HVACs      = new Dictionary<ushort, HVAC>();

            startupTimer = new CTimer(startupTimerHandler, 30000);

            // Create default 'Off' Source 
            RegisterSource(new Source("Off", 0, 169, 169));

            ConsoleMessage("[STARTUP] FrameWork Core Initialized...");
        }

        //===================// Methods //===================//

        /**
         * Method: initializeSystem
         * Access: internal
         * @return: void
         * Description: Invoked by startupTimer to give all SIMPL modules time to register with Core class.
         *              This method then calls individual Initialize functions on registered subclasses.
         */
        internal static void initializeSystem() {

            // Link Sources to Zones
            // {{-- TODO: Move this into a Zone.Initialize function --}}
            foreach (KeyValuePair<ushort, Zone> zn in Zones) {
                try {
                    zn.Value.ParseAudioSources();
                    zn.Value.ParseVideoSources();
                    zn.Value.ParseLights();
                    zn.Value.ParseShades();
                    zn.Value.ParseHVAC();
                }
                catch (Exception e) {
                    ConsoleMessage(String.Format("[ERROR] Error initializing Zone {0}: '{1}'", zn.Key, e));
                }
            }

            // Link displays to zones
            foreach (KeyValuePair<ushort, Display> dsp in Displays) {
                if (Zones.ContainsKey(dsp.Key)) {
                    if (!Zones[dsp.Key].hasDisplay)
                        Zones[dsp.Key].associateDisplay(dsp.Value);
                    else
                        ConsoleMessage(String.Format("[ERROR] Error associating Display with ZoneID {0}: Zone already has Display.", dsp.Key));
                }
                else
                    ConsoleMessage(String.Format("[ERROR] Error associating Display with ZoneID {0}: Zone not found.", dsp.Key));
            }

            // Link lifts to zones
            foreach (KeyValuePair<ushort, Lift> lift in Lifts) {
                if(Zones.ContainsKey(lift.Key)) {
                    Zones[lift.Key].associateLift(lift.Value);
                }
            }

            // Initialize Interfaces
            foreach (KeyValuePair<ushort, Interface> inf in Interfaces) {
                try {
                    if (inf.Value != null) {
                        inf.Value.Initialize();
                    }  else
                        ConsoleMessage(String.Format("[ERROR] Error initializing Interface {0}: Object is Null.", inf.Key));
                }
                catch (Exception e) {
                    ConsoleMessage(String.Format("[ERROR] Error initializing Interface {0}: '{1}'", inf.Key, e));
                }
            }

            if (startupCompleteEvent != null)
                startupCompleteEvent(1);
            else
                ConsoleMessage(String.Format("[ERROR] Core.StartupCompleteEvent delegate is Null, skipping."));

            ConsoleMessage(String.Format("[STARTUP] ... FrameWork Initialization Complete."));

        }

        /**
         * Method: ConsoleMessage
         * Access: public
         * @return: void
         * Description: 
         */
        public static void ConsoleMessage(string _message) {

            CrestronConsole.PrintLine("[{0}] {1}", DateTime.Now.ToString("HH:mm:ss"), _message);

        }

        /**
         * Method: RegisterSource
         * Access: public
         * @return: void
         * @param: Source _src
         * Description: S+ interface for associating a Source SIMPL module with a S# object
         */
        public static ushort RegisterSource(Source _src) {
            if (!Sources.ContainsKey(_src.id)) {
                Sources.Add(_src.id, _src);
                ConsoleMessage(String.Format("[STARTUP] Registered Source {0} @ ID {1}", _src.name, _src.id));
                return 1;
            } else {
                ConsoleMessage(String.Format("[ERROR] Error registering Source {0} @ ID {1}: SourceID already registered.", _src.name, _src.id));
                return 0;
            }
        }

        /**
         * Method: RegisterLight
         * Access: public
         * @return: void
         * @param: Light _lite
         * Description:
         */
        public static ushort RegisterLight(Light _lite) {
            if (!Lights.ContainsKey(_lite.id)) {
                Lights.Add(_lite.id, _lite);
                ConsoleMessage(String.Format("[STARTUP] Registered Light {0} @ ID {1}", _lite.name, _lite.id));
                return 1;
            } else {
                ConsoleMessage(String.Format("[ERROR] Error registering Light {0} @ ID {1}: LightID already registered.", _lite.name, _lite.id));
                return 0;
            }
        }

        /**
         * Method: RegisterShade
         * Access: public
         * @return: void
         * @param: Shade _shd
         * Description:
         */
        public static ushort RegisterShade(Shade _shd) {
            if (!Shades.ContainsKey(_shd.id)) {
                Shades.Add(_shd.id, _shd);
                ConsoleMessage(String.Format("[STARTUP] Registered Shade {0} @ ID {1}", _shd.name, _shd.id));
                return 1;
            } else {
                ConsoleMessage(String.Format("[ERROR] Error registering Shade {0} @ ID {1}: ShadeID already registered.", _shd.name, _shd.id));
                return 0;
            }
        }

        /**
         * Method: RegisterHVAC
         * Access: public
         * @return: void
         * @param: HVAC _ac
         * Description:
         */
        public static ushort RegisterHVAC(HVAC _ac) {
            if (!HVACs.ContainsKey(_ac.id)) {
                HVACs.Add(_ac.id, _ac);
                ConsoleMessage(String.Format("[STARTUP] Registered HVAC {0} @ ID {1}", _ac.name, _ac.id));
                return 1;
            } else {
                ConsoleMessage(String.Format("[ERROR] Error registering HVAC {0} @ ID {1}: HVACID already registered.", _ac.name, _ac.id));
                return 0;
            }
        }

        /**
         * Method: GetSourceInfo
         * Access: public
         * @return: void
         * @param: Source _src
         * @param: ushort _id
         * Description: S+ interface for passing a reference to a Source object based on SourceID
         */
        public static void GetSourceInfo(ref Source _src, ushort _id) {
            if (Sources.ContainsKey(_id)) {
                _src = Sources[_id];
            }
        }

        /**
         * Method: RegisterZone
         * Access: public
         * @return: ushort
         * @param: Zone _zn
         * Description: S+ interface for associating a Zone SIMPL module with a S# object
         */
        public static ushort RegisterZone(Zone _zn) {
            if (!Zones.ContainsKey(_zn.id)) {
                Zones.Add(_zn.id, _zn);
                ConsoleMessage(String.Format("[STARTUP] Registered Zone {0} @ ID {1}", _zn.name, _zn.id));
                return 1;
            } else {
                ConsoleMessage(String.Format("[ERROR] Error registering Zone {0}: Zone already registered", _zn.id));
                return 0;
            }
        }

        /**
         * Method: RegisterDisplay
         * Access: public
         * @return: ushort
         * @param: ushort _zid
         * @param: Display _dsp
         * Description: S+ interface for associating a Display SIMPL module with a S# object
         */
        public static ushort RegisterDisplay(ushort _zid, Display _dsp) {
            if (!Displays.ContainsKey(_zid)) {
                Displays.Add(_zid, _dsp);
                ConsoleMessage(String.Format("[STARTUP] Registered Display @ ZoneID {0}", _zid));
                return 1;
            } else {
                ConsoleMessage(String.Format("[ERROR] Error registering Display {0}: Display already registered for Zone", _zid));
                return 0;
            }
        }

        /**
         * Method: RegisterInterface
         * Access: public
         * @return: ushort
         * @param: ushort _intID
         * @param: Interface _inf
         * Description: S+ interface for associating a Interface SIMPL module with a S# object
         */
        public static ushort RegisterInterface(ushort _intID, Interface _inf) {

            if (!Interfaces.ContainsKey(_intID)) {

                Interfaces.Add(_intID, _inf);
                ConsoleMessage(String.Format("[STARTUP] Registered Interface {0} @ ID {1}", _inf.name, _intID));

                return 1;
            } else {

                ConsoleMessage(String.Format("[ERROR] Error registering Interface {0} @ ID {1}: Interface already registered", _inf.name, _intID));
                return 0;

            }

        }

        /**
         * Method: RegisterLift
         * Access: public
         * @return: ushort
         * @param: ushort _zoneID
         * @param: Lift _lift
         * Description: S+ interface for associating a Lift SIMPL module with a S# object
         */
        public static ushort RegisterLift(ushort _zoneID, Lift _lift) {
            if (!Lifts.ContainsKey(_zoneID)) {
                Lifts.Add(_zoneID, _lift);
                return 1;
            } else {
                ConsoleMessage(String.Format("[ERROR] Error registering Lift {0}: Lift already registered for Zone", _zoneID));
                return 0;
            }
        }

        /**
         * Method: RegisterAudioDevice
         * Access: public
         * @return: ushort
         * @param: AudioDevice _dev
         * Description: S+ interface for declaring a new AudioDevice; adds device to static List to be Initialized later.
         */
        //public static ushort RegisterAudioDevice(AudioDevice _dev) {
            //AudioDevices.Add(_dev);
            //return 1;
        //}

        /**
         * Method: SetZoneSource
         * Access: public
         * Description: S+ interface for setting a Zone to a Source for simple remotes
         */
        static public void SetZoneSource (ushort _zoneID, ushort _sourceID) {

            if (Zones.ContainsKey(_zoneID) && Sources.ContainsKey(_sourceID)) {
                if (_sourceID == 0 || Zones[_zoneID].hasSource(Sources[_sourceID])) {
                    Zones[_zoneID].SetCurrentSource(_sourceID);
                }
            }

        }

        static public void ConfigureSysInfoServer (ushort _portnum) {
            // Start SysInfoServer
            siServer = new SysInfoServer((int)_portnum, 256);
            siServer.startServer();
        }

        //===================// Event Handlers //===================//

        static void startupTimerHandler(object o) {
            ConsoleMessage(String.Format("[STARTUP] Initializing Classes..."));
            initializeSystem();
        }

    } // End Core Class

    public delegate void DelegateEmpty();
    public delegate void DelegateUshort(ushort value);
    public delegate void DelegateUshort2(ushort value1, ushort value2);
    public delegate void DelegateUshort3(ushort value1, ushort value2, ushort value3);
    public delegate void DelegateString(SimplSharpString value);
    public delegate void DelegateUshortString(ushort value1, SimplSharpString value2);

}
