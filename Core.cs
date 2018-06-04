using System;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using Crestron.SimplSharp;

namespace FrameWork {

    /**
     * Class:       Core
     * @author:     Ryan French
     * @version:    1.3a
     * Description: ...
     */
    public static class Core {

        //===================// Members //===================//

        internal static Dictionary<ushort, Zone>           Zones;
        internal static Dictionary<ushort, Source>         Sources;
        internal static Dictionary<ushort, Display>        Displays;
        internal static Dictionary<ushort, Interface>      Interfaces;
        internal static Dictionary<ushort, Lift>           Lifts;
        internal static Dictionary<ushort, Light>          Lights;
        internal static Dictionary<ushort, Shade>          Shades;
        internal static Dictionary<ushort, HVAC>           HVACs;
        internal static Dictionary<ushort, SecurityKeypad> SecurityKeypads;

        internal static List<Zone>           ZoneList;
        internal static List<Source>         SourceList;
        internal static List<Display>        DisplayList;
        internal static List<Interface>      InterfaceList;
        internal static List<Lift>           LiftList;
        internal static List<Light>          LightList;
        internal static List<Shade>          ShadeList;
        internal static List<HVAC>           HVACList;
        internal static List<SecurityKeypad> SecurityKeypadList;

        internal static Pool pool;

        internal static SysInfoServer siServer;

        private static CTimer    startupTimer;
        private static Stopwatch startTime;

        // Voice Control Registration Variables
        public static bool   VoiceControlEnabled;
        public static bool   VoiceControlConnected;
        public static bool   VoiceControlRegistered;
        public static bool   VoiceControlReady;
        public static string VoiceControlRegistrationID;

        // Startup Variables
        internal static int  RegZones;
        internal static int  RegInterfaces;
        internal static int  StartupCounter;
        internal static int  _startupProgress;
        internal static bool ZoneRefComplete;

        internal static int startupSeconds = 30;
        internal static int startupTickInterval = 250; // In milliseconds
        internal static int startupTickSize = 32768 / ((startupSeconds * 1000) / startupTickInterval);

        public static event DelegateInt startupProgressEvent;
        public static event DelegateEmpty startupCompleteEvent;

        public static DelegateString VoiceControlRegistrationCodeEvent { get; set; }
        public static DelegateEmpty startupComplete { get; set; }

        //===================// Constructor //===================//

        static Core() {

            startTime = new Stopwatch();
            startTime.Start();

            Sources         = new Dictionary<ushort, Source>();
            Zones           = new Dictionary<ushort, Zone>();
            Displays        = new Dictionary<ushort, Display>();
            Interfaces      = new Dictionary<ushort, Interface>();
            Lifts           = new Dictionary<ushort, Lift>();
            Lights          = new Dictionary<ushort, Light>();
            Shades          = new Dictionary<ushort, Shade>();
            HVACs           = new Dictionary<ushort, HVAC>();
            SecurityKeypads = new Dictionary<ushort, SecurityKeypad>();

            ZoneList           = new List<Zone>();
            SourceList         = new List<Source>();
            DisplayList        = new List<Display>();
            InterfaceList      = new List<Interface>();
            LiftList           = new List<Lift>();
            LightList          = new List<Light>();
            ShadeList          = new List<Shade>();
            HVACList           = new List<HVAC>();
            SecurityKeypadList = new List<SecurityKeypad>();

            RegZones = RegInterfaces = StartupCounter = 0;

            startupCompleteEvent += new DelegateEmpty(StartupCompleteEventHandler);

            // Begin 30 second timer to allow class objects to register with Core
            startupTimer = new CTimer(startupTimerHandler, startupTickInterval);

            CrestronConsole.PrintLine("..."); // Send newline to console for prettification
            ConsoleMessage("[STARTUP] FrameWork Core Initialized, waiting for assets...");

            // Create default 'Off' Source 
            RegisterSource(new Source("Off", 0, 169, 169));

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

            int count;

            count = ZoneList.Count;
            for (int i = 0; i < count; i++) {

                CrestronInvoke.BeginInvoke(unused => {
                    ZoneList[i].registerAssets();
                });

            }

            // Link displays to zones
            count = DisplayList.Count;
            for (int i = 0; i < count; i++) {

                ushort zid = DisplayList[i].zoneID;
                if (Zones.ContainsKey(zid)) {
                    if (!Zones[zid].hasDisplay)
                        Zones[zid].associateDisplay(DisplayList[i]);
                    else
                        ConsoleMessage(String.Format("[ERROR] Error associating Display with ZoneID {0}: Zone already has Display.", zid));
                } else
                    ConsoleMessage(String.Format("[ERROR] Error associating Display with ZoneID {0}: Zone not found.", zid));

            }

            // Link lifts to zones
            count = LiftList.Count;
            for (int i = 0; i < count; i++) {

                ushort zid = LiftList[i].zoneID;
                if (Zones.ContainsKey(zid)) {
                    Zones[zid].associateLift(LiftList[i]);
                }

            }

        }

        /**
         * Method: InitializeInterfaces
         * Access: public
         * @return: void
         * Description: Creates threads to initialize all Interface objects
         */
        internal static void InitializeInterfaces() {

            int count = InterfaceList.Count;
            for (int i = 0; i < count; i++) {

                CrestronInvoke.BeginInvoke(unused => {
                    if (InterfaceList[i] != null) {
                        InterfaceList[i].Initialize();
                    } else
                        ConsoleMessage(String.Format("[ERROR] Error initializing Interface {0}: Object is Null.", InterfaceList[i].id));
                });

            }

        }


        /**
         * Method: RegCompleteCallback
         * Access: public
         * @return: void
         * Description: Waits for all assets to finish registration before completing startup
         *              1 = Zone, 2 = interface
         */
        public static void RegCompleteCallback(int type) {

            if (type == 1)
                RegZones++;
            else if (type == 2)
                RegInterfaces++;

            // Update Startup Progress
            int zlc = ZoneList.Count;
            int ilc = InterfaceList.Count;
            int seg = 32768 / (zlc + ilc);

            startupProgress = (32768 + (seg * (RegZones + RegInterfaces)));

            // Wait for Zones to finish asset referencing before starting Interface referencing
            if (RegZones == ZoneList.Count && !ZoneRefComplete) {
                ZoneRefComplete = true;
                InitializeInterfaces();
            }

            if (RegZones == ZoneList.Count && RegInterfaces == InterfaceList.Count) {

                // Start SysInfoServer
                if (siServer != null)
                    siServer.startServer();

                // Finish startup progress notification and send Complete event
                startupProgress = 65535;
                if (startupCompleteEvent != null)
                    new CTimer(unused => {
                        startupCompleteEvent();
                    }, 1000);

                startTime.Stop();

                ConsoleMessage(String.Format("[STARTUP] Reference building complete. FrameWork startup completed in {0} seconds.", (startTime.ElapsedMilliseconds/1000)));

            }


        }

        /**
         * Method: ConsoleMessage
         * Access: public
         * @return: void
         * Description: ...
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
                SourceList.Add(_src);

                ConsoleMessage(String.Format("[STARTUP] Registered Source {0} [{1}]", _src.id, _src.name));
                return 1;

            } else {

                ConsoleMessage(String.Format("[ERROR] Error registering Source {0} [{1}]: SourceID already registered.", _src.id, _src.name));
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
                LightList.Add(_lite);

                ConsoleMessage(String.Format("[STARTUP] Registered Light {0} [{1}]", _lite.id, _lite.name));
                return 1;

            } else {

                ConsoleMessage(String.Format("[ERROR] Error registering Light {0} [{1}]: LightID already registered.", _lite.id, _lite.name));
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
                ShadeList.Add(_shd);

                ConsoleMessage(String.Format("[STARTUP] Registered Shade {0} [{1}]", _shd.id, _shd.name));
                return 1;

            } else {

                ConsoleMessage(String.Format("[ERROR] Error registering Shade {0} [{1}]: ShadeID already registered.", _shd.id, _shd.name));
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
                HVACList.Add(_ac);

                ConsoleMessage(String.Format("[STARTUP] Registered HVAC {0} [{1}]", _ac.id, _ac.name));
                return 1;

            } else {

                ConsoleMessage(String.Format("[ERROR] Error registering HVAC {0} [{1}]: HVACID already registered.", _ac.id, _ac.name));
                return 0;

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
                ZoneList.Add(_zn);

                ConsoleMessage(String.Format("[STARTUP] Registered Zone {0} [{1}]", _zn.id, _zn.name));
                return 1;

            } else {

                ConsoleMessage(String.Format("[ERROR] Error registering Zone {0} [{1}]: Zone already registered", _zn.id, _zn.name));
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
                DisplayList.Add(_dsp);

                ConsoleMessage(String.Format("[STARTUP] Registered Display {0}", _zid));
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
                InterfaceList.Add(_inf);

                ConsoleMessage(String.Format("[STARTUP] Registered Interface {0} [{1}]", _intID, _inf.name));
                return 1;

            } else {

                ConsoleMessage(String.Format("[ERROR] Error registering Interface {0} [{1}]: Interface already registered", _intID, _inf.name));
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
                LiftList.Add(_lift);

                ConsoleMessage(String.Format("[STARTUP] Registered Lift {0}", _zoneID));
                return 1;

            } else {

                ConsoleMessage(String.Format("[ERROR] Error registering Lift {0}: Zone already has registered Lift", _zoneID));
                return 0;

            }

        }

        /**
         * Method: RegisterSecurityKeypad
         * Access: public
         * @return: ushort
         * Description: ...
         */
        public static ushort RegisterSecurityKeypad(SecurityKeypad _keypad) {

            if (!SecurityKeypads.ContainsKey(_keypad.id)) {

                SecurityKeypads.Add(_keypad.id, _keypad);
                SecurityKeypadList.Add(_keypad);

                ConsoleMessage(String.Format("[STARTUP] Registered Security Keypad {0} [{1}]", _keypad.id, _keypad.name));
                return 1;

            } else {

                ConsoleMessage(String.Format("[ERROR] Error registering Security Keypad {0} [{1}]: Security Keypad already registered", _keypad.id, _keypad.name));
                return 0;

            }

        }

        /**
         * Method: RegisterPool
         * Access: public
         * @return: ushort
         * Description: ...
         */
        public static ushort RegisterPool(Pool _pool) {

            if (pool == null) {

                pool = _pool;
                ConsoleMessage(String.Format("[STARTUP] Registered Pool Controller"));
                return 1;

            } else {

                ConsoleMessage(String.Format("[ERROR] Error registering Pool Controller: Pool Controller already registered."));
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

        /**
         * Method: ConfigureSysInfoServer
         * Access: public
         * Description: Start telnet server for SysInfo service
         */
        static public void ConfigureSysInfoServer (ushort _portnum) {

            siServer = new SysInfoServer((int)_portnum, 256);

        }

        // Voice Control Methods

        /**
         * Method: VoiceControlEnable
         * Access: public
         * Description: Used to indicate that a Voice Control Link module is present in program
         */
        static public void VoiceControlEnable(ushort _val) {

            VoiceControlEnabled = _val == 1 ? true : false;

        }

        /**
         * Method: VoiceControlRegistrationEvent
         * Access: public
         * Description: Forwards updates from Voice Control hardware module to Interfaces
         */
        static public void VoiceControlRegistrationEvent(string _eType, ushort _eVal) {

            // Store event details in memory
            switch (_eType) {

                case "connected":
                    VoiceControlConnected  = _eVal == 1 ? true : false;
                    break;
                case "registered":
                    VoiceControlRegistered = _eVal == 1 ? true : false;
                    break;
                case "ready":
                    VoiceControlReady      = _eVal == 1 ? true : false;
                    break;

            }

            // Pass events to Interfaces with Voice Control Setup permission
            foreach (KeyValuePair<ushort, Interface> inf in Interfaces) {
                if (inf.Value.enableVoiceControlSetup == 1 && inf.Value.VoiceControlEvent != null) {
                    inf.Value.VoiceControlEvent(_eVal, _eType);
                }
            }

        }

        /**
         * Method: VoiceControlRegistrationIDEvent
         * Access: public
         * Description: Forwards Registration ID from Voice Control hardware module to Interfaces
         */
        static public void VoiceControlRegistrationIDEvent(string _id) {

            // Store ID in memory
            VoiceControlRegistrationID = _id;

            // Pass ID to Interfaces with Voice Control Setup permission
            foreach (KeyValuePair<ushort, Interface> inf in Interfaces) {
                if (inf.Value.enableVoiceControlSetup == 1 && inf.Value.VoiceControlIDEvent != null) {
                    inf.Value.VoiceControlIDEvent(_id);
                }
            }

        }

        /**
         * Method: VoiceControlSendRegistrationCode
         * Access: public
         * Description: Forwards Registration Code from Interfaces to Voice Control hardware module via S+ delegate
         */
        static public void VoiceControlSendRegistrationCode(string _code) {

            if (VoiceControlRegistrationCodeEvent != null) {
                VoiceControlRegistrationCodeEvent(_code);
            }

        }

        //===================// Event Handlers //===================//

        /**
         * Method: startupTimerHandler
         * Description: ...
         */
        private static void startupTimerHandler(object o) {

            StartupCounter++;

            if (StartupCounter < (startupSeconds * 1000) / startupTickInterval) {

                startupProgress = startupTickSize * StartupCounter;
                startupTimer.Reset(startupTickInterval);

            } else {

                ConsoleMessage(String.Format("[STARTUP] Asset registration closed, building references..."));
                initializeSystem();

            }

        }

        /**
         * Method: StartupCompleteEventHandler
         * Description: ...
         */
        private static void StartupCompleteEventHandler() {
            if(startupComplete != null)
                startupComplete();
        }

        //===================// Get / Set //===================//

        /**
         * Method: startupProgress
         * Description: ...
         */
        static public int startupProgress {

            get { return _startupProgress; }

            set {
                _startupProgress = value;
                if (startupProgressEvent != null) {
                    startupProgressEvent(value);
                }
            }

        }

    } // End Core Class

    public delegate void DelegateEmpty();
    public delegate void DelegateInt(int value);
    public delegate void DelegateUshort(ushort value);
    public delegate void DelegateUshort2(ushort value1, ushort value2);
    public delegate void DelegateUshort3(ushort value1, ushort value2, ushort value3);
    public delegate void DelegateString(SimplSharpString value);
    public delegate void DelegateUshortString(ushort value1, SimplSharpString value2);
    public delegate void DelegateUshortUshortString(ushort value1, ushort value2, SimplSharpString value23);
    public delegate SimplSharpString DelegateStringRequest();

}
