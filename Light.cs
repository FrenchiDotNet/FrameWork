using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace FrameWork {

    /**
     * Class:       Light
     * @author:     Ryan French
     * @version:    1.3a
     * Description: Class for storing details and control functions for an individual lighting load or preset.
     */
    public class Light {

        //===================// Members //===================//

        public ushort              id;
        public string              name;
        public LightingControlType controlType;
        public bool                hasFeedback;

        public bool   isOn;
        public ushort level;

        // Delegates for Hardware commands via S+
        public DelegateUshort2 TriggerSendCommand { get; set; } // ushort1 = LightCommand, ushort2 = press/release
        public DelegateUshort  TriggerSendLevel { get; set; }

        public event DelegateEmpty UpdateEvent; // Send notification to subscribed Zones
        
        public delegate void DelegateLightFbUpdate(ushort id, LightCommand action, ushort state);
        public event DelegateLightFbUpdate UpdateFeedback;
        


        //===================// Constructor //===================//

        public Light () {

            // Set empty values to be populated by S+
            id          = 0;
            name        = "";
            controlType = 0;
            hasFeedback = false;
            isOn        = false;
            level       = 0;

        }

        //===================// Methods //===================//

        /**
         * Method: createLight
         * Access: public
         * @param: ushort _id
         * @param: string _name
         * @param: ushort _controlType
         * @param: ushort _hasFeedback
         * @return: void
         * Description: Populate object with values passed from S+
         */
        public void createLight(ushort _id, string _name, ushort _controlType, ushort _hasFeedback) {

            this.id          = _id;
            this.name        = _name;
            this.controlType = (LightingControlType)_controlType;
            this.hasFeedback = _hasFeedback == 1 ? true : false;

        }

        /**
        * Method: setLevelFeedback
        * Access: public
        * @param: ushort _level
        * @return: void
        * Description: Receive hardware level feedback from S+, store in level variable, and push value to subscribed Interfaces via event
        */
        public void setLevelFeedback (ushort _level) {

            this.level = _level;

            //if (this.UpdateLevelEvent != null)
            //    this.UpdateLevelEvent (_level);
            if (this.UpdateFeedback != null)
                this.UpdateFeedback(this.id, LightCommand.Level, _level);

        }

        /**
        * Method: setIsOnFeedback
        * Access: public
        * @param: ushort _isOn
        * @return: void
        * Description: S+ accessor for setIsOnFeedback, converts ushort value to bool and passes to boolean version of setIsOnFeedback
        */
        public void setIsOnFeedback (ushort _isOn) {

            setIsOnFeedback(_isOn == 1 ? true : false);

        }

        /**
        * Method: setIsOnFeedback
        * Access: public
        * @param: bool _isOn
        * @return: void
        * Description: Receive hardware power status feedback, store in isOn variable, and push value to subscribed Interfaces via event
        */
        public void setIsOnFeedback(bool _isOn) {

            this.isOn = _isOn;

            if (this.UpdateFeedback != null) {
                if (this.isOn) {
                    this.UpdateFeedback(this.id, LightCommand.On, 1);
                    this.UpdateFeedback(this.id, LightCommand.Off, 0);
                } else {
                    this.UpdateFeedback(this.id, LightCommand.On, 0);
                    this.UpdateFeedback(this.id, LightCommand.Off, 1);
                }
            }

            if (this.UpdateEvent != null)
                UpdateEvent();

        }

        /**
        * Method: setLevel
        * Access: public
        * @param: ushort _level
        * @return: void
        * Description: Receive hardware level feedback, store in level variable, and push value to subscribed Interfaces via event
        */
        public void setLevel (ushort _level) {

            if (TriggerSendLevel != null)
                TriggerSendLevel (_level);

        }

        /**
        * Method: sendCommand
        * Access: public
        * @param: LightCommand _command
        * @param: ushort _state
        * @return: void
        * Description: Receive button presses from Interface and forward to lighting hardware. _state variable represents
        *              whether button is being pressed or released.
        */
        public void sendCommand(LightCommand _command, ushort _state) {

            if (TriggerSendCommand != null)
                TriggerSendCommand ((ushort)_command, _state);

        }

        /**
        * Method: subscribeToEvents
        * Access: public
        * @return: void
        * Description: ...
        */
        public void subscribeToEvents(Interface _int) {

            this.UpdateFeedback += _int.LightingLoadFbHandler;

            sendCommand(LightCommand.InUseState, (ushort)(UpdateFeedback != null ? 1 : 0));

        }

        /**
        * Method: unsubscribeFromEvents
        * Access: public
        * @return: void
        * Description: ...
        */
        public void unsubscribeFromEvents(Interface _int) {

            this.UpdateFeedback -= _int.LightingLoadFbHandler;

            sendCommand(LightCommand.InUseState, (ushort)(UpdateFeedback != null ? 1 : 0));

        }

        //===================// Event Handlers //===================//

    } // End Light class

    public enum LightCommand {
        Unknown,
        Off,
        On,
        Raise,
        Lower,
        CycleDim,
        Toggle,
        Speed1,
        Speed2,
        Speed3,
        Level,
        InUseState
    }

    public enum LightingControlType {
        Unknown,
        SingleButton,
        OnOff,
        Dim,
        DimToggle,
        Slider,
        SliderToggle
    }

}