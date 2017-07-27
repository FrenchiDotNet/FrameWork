using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace FrameWork {

    /**
     * Class:       SWAMP
     * @author:     Ryan French
     * @version:    1.0
     * Description: ...
     */
    public class SWAMP : AudioDevice {

        //===================// Members //===================//

        private List<SWAMPOutput> Outputs;

        private Dictionary<ushort, int> OutputZones;
        private Dictionary<ushort, int> InputSources;

        // S+ delegates for requesting Input and Output ID lists from SIMPL
        public delegate SimplSharpString DelegateRequestIO(ushort _input);
        public DelegateRequestIO RequestInput { get; set; }
        public DelegateRequestIO RequestOutput { get; set; }

        // S+ delegate for sending SWAMP Output Mute state changes to SIMPL
        public event DelegateUshort2 TriggerMuteEvent;

        // S+ delegate for sending SWAMP Output Mono state changes to SIMPL
        public event DelegateUshort2 TriggerMonoEvent;

        // S+ delegate for sending SWAMP Output Loudness state changes to SIMPL
        public event DelegateUshort2 TriggerLoudnessEvent;

        // S+ delegate for sending SWAMP Output Speaker Protect state changes to SIMPL
        public event DelegateUshort2 TriggerSpeakerProtectEvent;

        // S+ delegate for sending SWAMP Output DRC state changes to SIMPL
        public event DelegateUshort2 TriggerDRCEvent;

        // S+ delegate for sending SWAMP Output Source state changes to SIMPL
        public event DelegateUshort2 TriggerSourceEvent;

        // S+ delegate for sending SWAMP Output Volume state changes to SIMPL
        public event DelegateUshort2 TriggerVolumeEvent;

        // S+ delegate for sending SWAMP Output MinVol state changes to SIMPL
        public event DelegateUshort2 TriggerMinVolEvent;

        // S+ delegate for sending SWAMP Output MaxVol state changes to SIMPL
        public event DelegateUshort2 TriggerMaxVolEvent;

        // S+ delegate for sending SWAMP Output StartupVol state changes to SIMPL
        public event DelegateUshort2 TriggerStartupVolEvent;

        // S+ delegate for sending SWAMP Output Bass level changes to SIMPL
        public event DelegateUshort2 TriggerBassEvent;

        // S+ delegate for sending SWAMP Output Treble level changes to SIMPL
        public event DelegateUshort2 TriggerTrebleEvent;

        // S+ delegate for sending SWAMP Output Balance level changes to SIMPL
        public event DelegateUshort2 TriggerBalanceEvent;

        // S+ delegate for sending SWAMP Output EQ state changes to SIMPL
        public event DelegateUshort2 TriggerEQEvent;

        // S+ delegate for sending SWAMP Output Name changes to SIMPL
        public event DelegateUshortString TriggerNameEvent;

        //===================// Constructor //===================//

        public SWAMP() {
            Outputs      = new List<SWAMPOutput>(8);
            OutputZones  = new Dictionary<ushort, int>();
            InputSources = new Dictionary<ushort, int>();

            // Initialize Outputs
            for (int i = 0; i < 8; i++) {
                Outputs.Add(new SWAMPOutput(this, i));
            }
        }

        //===================// Methods //===================//

        /**
         * Method: Initialize
         * Access: public
         * Description: ...
         */
        public override void Initialize() {
            string sTmp;
            ushort idTmp;
            
            //--- Parse Input and Output lists into Dictionaries

            if (RequestInput == null || RequestOutput == null)
                return;

            // Inputs
            for (int i = 1; i <= 24; i++ ) {
                sTmp = RequestInput((ushort)i).ToString();
                foreach (string src in sTmp.Replace(" ", "").Split(',')) {
                    if (src != "") {
                        idTmp = ushort.Parse(src);
                        if (Core.Sources.ContainsKey(idTmp))
                            this.InputSources.Add(idTmp, i);
                    }
                }
            }

            // Outputs
            for (int i = 1; i <= 8; i++) {
                sTmp = RequestOutput((ushort)i).ToString();
                foreach (string zn in sTmp.Replace(" ", "").Split(',')) {
                    if (zn != "") {
                        idTmp = ushort.Parse(zn);
                        if (Core.Zones.ContainsKey(idTmp))
                            this.OutputZones.Add(idTmp, i);
                    }
                }
            }

            //--- Subscribe SWAMPOutputs to Zone UpdateCurrentSourceIDEvent events

        }

        /**
         * Method: RouteRequest
         * Access: public
         * Description: Receives events from SWAMPOutput objects to look up input numbers for specified SourceID's, then
         *              send Source Events to S+ containing the Input and Output route values.
         */
        public void RouteRequest(int _outputNumber, ushort _sourceID) {
            // If source is 0 (Off), just send route
            if (_sourceID == 0) {
                TriggerSourceEvent((ushort)_outputNumber, _sourceID);
                return;
            }

            // Look up Input containing SourceID
            if(!this.InputSources.ContainsKey(_sourceID))
                return;

            ushort inputNumber = (ushort)this.InputSources[_sourceID];

            // Send Source Event to S+
            TriggerSourceEvent((ushort)_outputNumber, inputNumber);
        }

        /**
         * Method: MuteFbEvent
         * Access: public
         * @param: ushort _position
         * @param: ushort _state
         * Description: ...
         */
        public void MuteFbEvent(ushort _position, ushort _state) {
            Outputs[_position - 1].muteEnabled = _state == 1 ? true : false;
        }

        /**
         * Method: MonoFbEvent
         * Access: public
         * @param: ushort _position
         * @param: ushort _state
         * Description: ...
         */
        public void MonoFbEvent(ushort _position, ushort _state) {
            Outputs[_position - 1].monoEnabled = _state == 1 ? true : false;
        }

        /**
         * Method: LoudnessFbEvent
         * Access: public
         * @param: ushort _position
         * @param: ushort _state
         * Description: ...
         */
        public void LoudnessFbEvent(ushort _position, ushort _state) {
            Outputs[_position - 1].loudnessEnabled = _state == 1 ? true : false;
        }

        /**
         * Method: SpeakerProtectFbEvent
         * Access: public
         * @param: ushort _position
         * @param: ushort _state
         * Description: ...
         */
        public void SpeakerProtectFbEvent(ushort _position, ushort _state) {
            Outputs[_position - 1].speakerProtectEnabled = _state == 1 ? true : false;
        }

        /**
         * Method: DRCFbEvent
         * Access: public
         * @param: ushort _position
         * @param: ushort _state
         * Description: ...
         */
        public void DRCFbEvent(ushort _position, ushort _state) {
            Outputs[_position - 1].drcEnabled = _state == 1 ? true : false;
        }

        /**
         * Method: SourceFbEvent
         * Access: public
         * @param: ushort _position
         * @param: ushort _value
         * Description: ...
         */
        public void SourceFbEvent(ushort _position, ushort _value) {
            Outputs[_position - 1].source = (int)_value;
        }

        /**
         * Method: VolumeFbEvent
         * Access: public
         * @param: ushort _position
         * @param: ushort _level
         * Description: ...
         */
        public void VolumeFbEvent(ushort _position, ushort _level) {
            Outputs[_position - 1].volume = (int)_level;
        }

        /**
         * Method: MinVolFbEvent
         * Access: public
         * @param: ushort _position
         * @param: ushort _level
         * Description: ...
         */
        public void MinVolFbEvent(ushort _position, ushort _level) {
            Outputs[_position - 1].minVol = (int)_level;
        }

        /**
         * Method: MaxVolFbEvent
         * Access: public
         * @param: ushort _position
         * @param: ushort _level
         * Description: ...
         */
        public void MaxVolFbEvent(ushort _position, ushort _level) {
            Outputs[_position - 1].maxVol = (int)_level;
        }

        /**
         * Method: StartupVolFbEvent
         * Access: public
         * @param: ushort _position
         * @param: ushort _level
         * Description: ...
         */
        public void StartupVolFbEvent(ushort _position, ushort _level) {
            Outputs[_position - 1].startupVol = (int)_level;
        }

        /**
         * Method: BassFbEvent
         * Access: public
         * @param: ushort _position
         * @param: ushort _level
         * Description: ...
         */
        public void BassFbEvent(ushort _position, ushort _level) {
            Outputs[_position - 1].bass = (int)_level;
        }

        /**
         * Method: TrebleFbEvent
         * Access: public
         * @param: ushort _position
         * @param: ushort _level
         * Description: ...
         */
        public void TrebleFbEvent(ushort _position, ushort _level) {
            Outputs[_position - 1].treble = (int)_level;
        }

        /**
         * Method: BalanceFbEvent
         * Access: public
         * @param: ushort _position
         * @param: ushort _level
         * Description: ...
         */
        public void BalanceFbEvent(ushort _position, ushort _level) {
            Outputs[_position - 1].balance = (int)_level;
        }

        /**
         * Method: EQFbEvent
         * Access: public
         * @param: ushort _position
         * @param: ushort _value
         * Description: ...
         */
        public void EQFbEvent(ushort _position, ushort _value) {
            Outputs[_position - 1].eq = (int)_value;
        }

        //===================// Event Handlers //===================//

    } // End SWAMP Class

    /**
     * Class:       SWAMPOutput
     * @author:     Ryan French
     * @version:    1.0
     * Description: ...
     */
    public class SWAMPOutput {

        //===================// Members //===================//

        public bool muteEnabled;
        public bool monoEnabled;
        public bool loudnessEnabled;
        public bool speakerProtectEnabled;
        public bool drcEnabled;

        public int source;
        public int volume;
        public int minVol;
        public int maxVol;
        public int startupVol;
        public int bass;
        public int treble;
        public int balance;
        public int eq;

        public string name;

        private int outputNumber;

        private SWAMP parent;

        //===================// Constructor //===================//

        public SWAMPOutput(SWAMP _parent, int _outputNumber) {
            parent       = _parent;
            outputNumber = _outputNumber;
        }

        //===================// Event Handlers //===================//

        public void ZoneSourceIDUpdateHandler(ushort _sourceID) {
            // Bubble event to parent SWAMP class with output number attached
            parent.RouteRequest(this.outputNumber, _sourceID);
        }

    }

}