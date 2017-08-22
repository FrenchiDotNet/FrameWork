using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace FrameWork {

    /**
     * Class:       HVAC
     * @author:     Ryan French
     * @version:    1.0
     * Description: Class for storing details and control functions for an individual HVAC zone.
     */
    public class HVAC {

        //===================// Members //===================//

        public ushort id;
        public string name;
        public HVACControlType controlType;

        public bool hasFanModeControl;
        public bool hasFanSpeedControl;

        public ushort currentSetpointTemp;
        public ushort currentAmbientTemp;

        public string currentSystemMode;
        public string currentFanMode;
        public string currentFanSpeed;

        public bool coolCallActive;
        public bool heatCallActive;
        public bool fanCallActive;

        // Delegates for sending commands to hardware via S+
        public DelegateUshort2 TriggerSendCommand { get; set; }
 
        // Delegates for sending feedback to subscribed Interfaces
        public delegate void DelegateHVACFbUpdate(ushort id, HVACCommand action, ushort state);
        public event DelegateHVACFbUpdate UpdateEvent;

        //===================// Constructor //===================//

        public HVAC() {

            // Set empty values to be populated by S+
            id          = 0;
            name        = "";
            controlType = 0;

        }

        //===================// Methods //===================//

        /**
        * Method: CreateHVAC
        * Access: public
        * Description: Populate object with values passed from S+
        */
        public void CreateHVAC(ushort _id, string _name, ushort _controlType, ushort _hasFanModeControl, ushort _hasFanSpeedControl) {

            this.id                 = _id;
            this.name               = _name;
            this.controlType        = (HVACControlType)_controlType;
            this.hasFanModeControl  = _hasFanModeControl == 1 ? true : false;
            this.hasFanSpeedControl = _hasFanSpeedControl == 1 ? true : false;

        }

        /**
        * Method: FeedbackEvent
        * Access: public
        * Description: Receive hardware feedback from S+, store in variable, and push value to subscribed Interfaces via event
        */
        public void FeedbackEvent(ushort _action, ushort _state) {

            HVACCommand act = (HVACCommand)_action;

            switch (act) {

                case HVACCommand.SystemMode_Off_Fb:
                    if (_state == 1)
                        currentSystemMode = "Off";
                    break;
                case HVACCommand.SystemMode_Cool_Fb:
                    if (_state == 1)
                        currentSystemMode = "Cool";
                    break;
                case HVACCommand.SystemMode_Heat_Fb:
                    if (_state == 1)
                        currentSystemMode = "Heat";
                    break;
                case HVACCommand.SystemMode_Auto_Fb:
                    if (_state == 1)
                        currentSystemMode = "Auto";
                    break;
                case HVACCommand.FanMode_On_Fb:
                    if (_state == 1)
                        currentFanMode = "On";
                    break;
                case HVACCommand.FanMode_Auto_Fb:
                    if (_state == 1)
                        currentFanMode = "Auto";
                    break;
                case HVACCommand.FanSpeed_Low_Fb:
                    if (_state == 1)
                        currentFanSpeed = "Low";
                    break;
                case HVACCommand.FanSpeed_Medium_Fb:
                    if (_state == 1)
                        currentFanSpeed = "Medium";
                    break;
                case HVACCommand.FanSpeed_High_Fb:
                    if (_state == 1)
                        currentFanSpeed = "High";
                    break;
                case HVACCommand.FanSpeed_Max_Fb:
                    if (_state == 1)
                        currentFanSpeed = "Max";
                    break;
                case HVACCommand.CoolCall_Fb:
                    coolCallActive = _state == 1 ? true : false;
                    break;
                case HVACCommand.HeatCall_Fb:
                    heatCallActive = _state == 1 ? true : false;
                    break;
                case HVACCommand.FanCall_Fb:
                    fanCallActive = _state == 1 ? true : false;
                    break;
                case HVACCommand.TempSetpoint:
                    currentSetpointTemp = _state;
                    break;
                case HVACCommand.TempAmbient:
                    currentAmbientTemp = _state;
                    break;

            }

            // Broadcast to UpdateEvent subscribers
            if (this.UpdateEvent != null)
                UpdateEvent(this.id, act, _state);

        }

        /**
        * Method: SendHVACCommand
        * Access: public
        * Description: Receive Commands from Interfaces and pass to S+ via TriggerSendCommand delegate
        */
        public void SendHVACCommand(HVACCommand _cmd, ushort _state) {

            if (TriggerSendCommand != null)
                TriggerSendCommand((ushort)_cmd, _state);

        }

        /**
        * Method: subscribeToEvents
        * Access: public
        * @return: void
        * Description: ...
        */
        public void subscribeToEvents(Interface _int) {

            this.UpdateEvent += _int.HVACFbHandler;

            SendHVACCommand(HVACCommand.InUseState, (ushort)(UpdateEvent != null ? 1 : 0));

        }

        /**
        * Method: unsubscribeFromEvents
        * Access: public
        * @return: void
        * Description: ...
        */
        public void unsubscribeFromEvents(Interface _int) {

            this.UpdateEvent -= _int.HVACFbHandler;

            SendHVACCommand(HVACCommand.InUseState, (ushort)(UpdateEvent != null ? 1 : 0));

        }

        //===================// Event Handlers //===================//


    } // End HVAC Class

    public enum HVACControlType {
        Auto,
        HeatCool,
        HeatCoolAuto
    }

    public enum HVACCommand {
        Unknown,
        Setpoint_Up,
        Setpoint_Down,
        SystemMode_Off,
        SystemMode_Cool,
        SystemMode_Heat,
        SystemMode_Auto,
        FanMode_On,
        FanMode_Auto,
        FanSpeed_Low,
        FanSpeed_Medium,
        FanSpeed_High,
        FanSpeed_Max,
        SystemMode_Off_Fb,
        SystemMode_Cool_Fb,
        SystemMode_Heat_Fb,
        SystemMode_Auto_Fb,
        FanMode_On_Fb,
        FanMode_Auto_Fb,
        FanSpeed_Low_Fb,
        FanSpeed_Medium_Fb,
        FanSpeed_High_Fb,
        FanSpeed_Max_Fb,
        CoolCall_Fb,
        HeatCall_Fb,
        FanCall_Fb,
        TempSetpoint,
        TempAmbient,
        InUseState
    }

}