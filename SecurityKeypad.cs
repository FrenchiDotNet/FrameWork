using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace FrameWork {

    /**
     * Class:       SecurityKeypad
     * @author:     Ryan French
     * @version:    1.0
     * Description: Class for storing details and control functions for an individual Security Keypad.
     */
    public class SecurityKeypad {

        //===================// Members //===================//

        public ushort id;
        public string name;

        public int customButtonCount;

        public SecurityArmState currentArmState;
        public string           currentStatusText_Line1,
                                currentStatusText_Line2,
                                currentKeypadStars;
        public List<string>     customButtonLabels;
        public string[]         functionButtonLabels;
        public bool[]           customButtonFeedback;

        // Delegates for sending commands to hardware via S+
        public DelegateUshort2 TriggerSendCommand { get; set; }

        // Delegates for sending feedback to subscribed Interfaces
        public delegate void DelegateSecurityKeypadFbUpdate(SecurityCommand action, ushort state);
        public event DelegateSecurityKeypadFbUpdate UpdateEvent;

        public delegate void DelegateSecurityKeypadTextFbUpdate(SecurityCommand action, string text);
        public event DelegateSecurityKeypadTextFbUpdate TextUpdateEvent;

        //===================// Constructor //===================//

        public SecurityKeypad () {

            currentStatusText_Line1 = "";
            currentStatusText_Line2 = "";
            currentKeypadStars      = "";
            customButtonCount       = 0;
            customButtonLabels      = new List<string>();
            functionButtonLabels    = new string[4] {"", "", "", ""};
            customButtonFeedback    = new bool[6] {false, false, false, false, false, false};
            currentArmState         = SecurityArmState.Unknown;

        }

        //===================// Methods //===================//

        // Function to initialize class variables
        public void CreateSecurityKeypad (ushort _id, string _name) {

            this.id   = _id;
            this.name = _name;

        }

        // Function to set Custom Button labels from S+
        public void AddCustomButton(string _label) {

            this.customButtonLabels.Add(_label);
            this.customButtonCount += 1;

        }

        // Function to set Function Button labels from S+
        public void AddFunctionButton(ushort _position, string _label) {

            this.functionButtonLabels[_position] = _label;
             
        }

        // Function to send Custom Button labels to Interfaces
        
        // Function to send Function Button labels to Interfaces

        /**
        * Method: SendSecurityCommand
        * Access: public
        * Description: Receive Commands from Interfaces and pass to S+ via TriggerSendCommand delegate
        */
        public void SendSecurityCommand (SecurityCommand _cmd, ushort _state) {

            if (TriggerSendCommand != null)
                TriggerSendCommand((ushort)_cmd, _state);

        }

        /**
        * Method: FeedbackEvent
        * Access: public
        * Description: Receive hardware feedback from S+, store in variable, and push value to subscribed Interfaces via event
        */
        public void FeedbackEvent(ushort _action, ushort _state) {

            SecurityCommand act = (SecurityCommand)_action;

            switch (act) {

                case SecurityCommand.ArmAway_Fb:
                    if (_state == 1)
                        currentArmState = SecurityArmState.ArmAway;
                    break;
                case SecurityCommand.ArmHome_Fb:
                    if (_state == 1)
                        currentArmState = SecurityArmState.ArmHome;
                    break;
                case SecurityCommand.ArmNight_Fb:
                    if (_state == 1)
                        currentArmState = SecurityArmState.ArmNight;
                    break;
                case SecurityCommand.Disarm_Fb:
                    if (_state == 1)
                        currentArmState = SecurityArmState.Disarmed;
                    break;
                case SecurityCommand.Custom_01_Fb:
                    customButtonFeedback[0] = _state == 1 ? true : false;
                    break;
                case SecurityCommand.Custom_02_Fb:
                    customButtonFeedback[1] = _state == 1 ? true : false;
                    break;
                case SecurityCommand.Custom_03_Fb:
                    customButtonFeedback[2] = _state == 1 ? true : false;
                    break;
                case SecurityCommand.Custom_04_Fb:
                    customButtonFeedback[3] = _state == 1 ? true : false;
                    break;
                case SecurityCommand.Custom_05_Fb:
                    customButtonFeedback[4] = _state == 1 ? true : false;
                    break;
                case SecurityCommand.Custom_06_Fb:
                    customButtonFeedback[5] = _state == 1 ? true : false;
                    break;

            }

            // Broadcast to UpdateEvent subscribers
            if (this.UpdateEvent != null)
                UpdateEvent(act, _state);

        }

        /**
        * Method: TextFeedbackEvent
        * Access: public
        * Description: Receive hardware status text feedback from S+, store in variable, and push value to subscribed Interfaces via event
        */
        public void TextFeedbackEvent(ushort _action, string _txt) {

            SecurityCommand act = (SecurityCommand)_action;

            switch (act) {

                case SecurityCommand.Text_StatusFeedback_Line1:
                    currentStatusText_Line1 = _txt;
                    break;
                case SecurityCommand.Text_StatusFeedback_Line2:
                    currentStatusText_Line2 = _txt;
                    break;
                case SecurityCommand.Text_KeypadStars:
                    currentKeypadStars = _txt;
                    break;

            }

            
            if (TextUpdateEvent != null)
                TextUpdateEvent (act, _txt);

        }

        /**
        * Method: getEncodedFunctionStates
        * Access: public
        * Description: Returns a string-encoded integer representation of the availability of each Function button
        */
        public string getEncodedFunctionStates() {

            string states = "";

            for (int i = 0; i < 4; i++) {
                states = states + (functionButtonLabels[i] != "" ? '1' : '0');
            }

            return states;

        }

        //===================// Event Handlers //===================//

    }

    public enum SecurityArmState {
        Unknown,
        Disarmed,
        ArmAway,
        ArmHome,
        ArmNight
    }

    public enum SecurityCommand {
        Unknown,
        Keypad_0,
        Keypad_1,
        Keypad_2,
        Keypad_3,
        Keypad_4,
        Keypad_5,
        Keypad_6,
        Keypad_7,
        Keypad_8,
        Keypad_9,
        Keypad_Enter,
        Keypad_Clear,
        Keypad_Backspace,
        Keypad_Star,
        Keypad_Pound,
        Keypad_Exit,
        Keypad_Stay,
        Keypad_Chime,
        Keypad_Bypass,
        Disarm,
        ArmAway,
        ArmHome,
        ArmNight,
        Function_01,
        Function_02,
        Function_03,
        Function_04,
        Custom_01,
        Custom_02,
        Custom_03,
        Custom_04,
        Custom_05,
        Custom_06,
        Disarm_Fb,
        ArmAway_Fb,
        ArmHome_Fb,
        ArmNight_Fb,
        Custom_01_Fb,
        Custom_02_Fb,
        Custom_03_Fb,
        Custom_04_Fb,
        Custom_05_Fb,
        Custom_06_Fb,
        Text_KeypadStars,
        Text_StatusFeedback_Line1,
        Text_StatusFeedback_Line2,
        Number_Of_Custom_Buttons,
        Function_States,
        Function_01_Label,
        Function_02_Label,
        Function_03_Label,
        Function_04_Label,
        Custom_01_Label,
        Custom_02_Label,
        Custom_03_Label,
        Custom_04_Label,
        Custom_05_Label,
        Custom_06_Label
    }

}