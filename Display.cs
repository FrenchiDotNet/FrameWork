using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace FrameWork {

    /**
     * Class:       Display
     * @author:     Ryan French
     * @version:    1.3a
     * Description: Defines control methods and variables for video displays. Interfaces with S+/SIMPL via event triggers to send commands to display hardware and
     *              pass device info to Interfaces for end-user interaction.
     */
    public class Display {

        //===================// Members //===================//

        public ushort zoneID;

        public List<string> buttonLabels;
        public int buttonCount;

        private bool isPoweredOn;
        private int currentSource; // Use DisplayCommands enum values

        private CTimer powerOnDelay;
        private long powerOnDelayTime;

        private Dictionary<ushort, DisplayCommands> inputSources;

        // S+ delegate for triggereing commands on Display SIMPL module
        // ushort 1 = DisplayCommand, ushort 2 = 0/1 (release/press)
        public DelegateUshort2 DisplayCommand { get; set; } 
        public DelegateUshort DisplayCommandPulse { get; set; }

        //===================// Constructor //===================//

        public Display() {
            buttonCount      = 0;
            buttonLabels     = new List<string>();
            isPoweredOn      = false;
            currentSource    = 0;
            powerOnDelayTime = 0;
            inputSources     = new Dictionary<ushort, DisplayCommands>();
        }

        //===================// Methods //===================//

        /**
         * Method:      AddButton
         * Access:      public
         * @param:      string _label
         * Description: S+ setup function used to register a new display command button with 
         *              it's Display class object.
         */
        public void AddButton(string _label) {
            this.buttonLabels.Add(_label);
            this.buttonCount += 1;
        }

        /**
         * Method:      SetPowerOnDelayTime
         * Access:      public
         * @param:      ushort _timeInMs
         * Description: Converts S+ ushort value for power-on delay to a long usable by CTimer class
         */
        public void SetPowerOnDelayTime(ushort _timeInMs) {
            powerOnDelayTime = (long)_timeInMs;
        }

        /**
         * Method:      ParseInputSources
         * Access:      public
         * @param:      string _list
         * @param:      ushort _input
         * Description: Parses a comma-separated list of SourceID's passed in from S+ and adds them to a Dictionary
         *              along with a DisplayCommands enum representing the TV input that the source is displayed on.
         */
        public void ParseInputSources(string _list, ushort _input) {
            ushort sourceID;

            foreach (string sid in _list.Replace(" ", "").Split(',')) {
                if (sid != "") {
                    sourceID = ushort.Parse(sid);
                    this.inputSources.Add(sourceID, (DisplayCommands)_input);
                }
            }
        }

        /**
         * Method:      PowerOff
         * Access:      public
         * Description: Convenience function for powering off display.
         */
        public void PowerOff() {
            if (!isPoweredOn)
                return;

            if (this.DisplayCommandPulse != null) {
                DisplayCommandPulse((ushort)DisplayCommands.Power_Off);
                isPoweredOn = false;
            }
        }

        /**
         * Method:      PowerOn
         * Access:      public
         * Description: Convenience function for powering on display.
         */
        public void PowerOn() {
            if (isPoweredOn)
                return;

            if (this.DisplayCommandPulse != null) {
                DisplayCommandPulse((ushort)DisplayCommands.Power_On);
                isPoweredOn = true;
            }
        }

        //===================// Event Handlers //===================//

        /**
         * Method:      ZoneSourceUpdateHandler
         * Access:      public
         * @param:      ushort _sourceID
         * Description: Triggered by parent Zone when it's currentSource is changed. Used to control power and 
         *              source selection for AV routing.
         */
        public void ZoneSourceUpdateHandler(ushort _sourceID) {
            // Check if Source is Audio or Video
            if (!Core.Sources[_sourceID].isVideoSource) {

                this.PowerOff();

            } else {

                // Lookup _sourceID on this display's input lists
                if (!this.inputSources.ContainsKey(_sourceID)) {
                    CrestronConsole.PrintLine("[Error] Display {0} - Cannot find requested SourceID {1} in InputSources dictionary.", this.zoneID, _sourceID);
                    return;
                }

                if (!this.isPoweredOn) {
                    this.PowerOn();
                    powerOnDelay = new CTimer(powerOnDelayHandler, (ushort)this.inputSources[_sourceID], powerOnDelayTime);
                } else {
                    if (DisplayCommandPulse != null)
                        DisplayCommandPulse((ushort)this.inputSources[_sourceID]);
                }
            }
        }

        /**
         * Method:      powerOnDelayHandler
         * Access:      private
         * @param:      object _input
         * Description: ...
         */
        public void powerOnDelayHandler(object _input) {
            if (DisplayCommandPulse != null) {
                DisplayCommandPulse((ushort)_input);
                this.currentSource = (ushort)_input;
            }
        }

    } // End Display Class

    public enum DisplayCommands {
        Power_On = 1,
        Power_Off,
        Power_Toggle,
        Menu_Up,
        Menu_Down,
        Menu_Left,
        Menu_Right,
        Menu_Select,
        Menu,
        Exit,
        Volume_Up,
        Volume_Down,
        Volume_Mute,
        Input_HDMI1,
        Input_HDMI2,
        Input_HDMI3,
        Input_HDMI4,
        Input_Component,
        Input_Composite,
        Input_TV,
        Input_PC
    }

}