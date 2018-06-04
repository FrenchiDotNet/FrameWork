using System;
using System.Collections.Generic;
using Crestron.SimplSharp;

namespace FrameWork {

    /**
     * Class:       Pool
     * @author:     Ryan French
     * @version:    1.3a
     * Description: ...
     */
    public class Pool {

        public PoolMode currentMode;

        public ushort systemType; // 1 = Pool Only, 2 = Spa Only, 3 = Pool + Spa

        public float currentPoolSetpoint;
        public float currentPoolTemp;
        public float currentSpaSetpoint;
        public float currentSpaTemp;
        public float currentAmbientTemp;

        public bool filterPumpOn;
        public bool poolHeaterOn;
        public bool spaHeaterOn;

        public bool[] currentAuxStatus;
        public string[] auxLabels;


        //===================// Members //===================//

        // Delegates for sending commands to hardware via S+
        public DelegateUshortUshortString TriggerSendCommand { get; set; }

        // Event for sending feedback to subscribed Interfaces
        public delegate void DelegatePoolFbUpdate(PoolCommand action, ushort state, string text);
        public event DelegatePoolFbUpdate UpdateEvent;

        //===================// Constructor //===================//

        public Pool() {

            currentPoolSetpoint = 0f;
            currentPoolTemp     = 0f;
            currentSpaSetpoint  = 0f;
            currentSpaTemp      = 0f;
            currentAmbientTemp  = 0f;

            currentAuxStatus = new bool[12];
            auxLabels        = new string[12];

            currentMode = PoolMode.Unknown;

            systemType  = 0;

        }

        //===================// Methods //===================//

        /**
        * Method: SendPoolCommand
        * Access: public
        * Description: Receive Commands from Interfaces and pass to S+ via TriggerSendCommand delegate
        */
        public void SendPoolCommand(PoolCommand _cmd, ushort _state, string _text) {

            if (TriggerSendCommand != null)
                TriggerSendCommand((ushort)_cmd, _state, _text);

        }

        /**
        * Method: FeedbackEvent
        * Access: public
        * Description: Receive hardware feedback from S+, store in variable, and push value to subscribed Interfaces via event
        */
        public void FeedbackEvent(ushort _action, ushort _state, string _text) {

            PoolCommand act = (PoolCommand)_action;

            switch (act) {

                case PoolCommand.ModeSelect_Pool_Fb:
                    if (_state == 1)
                        currentMode = PoolMode.Pool;
                    break;
                case PoolCommand.ModeSelect_Spa_Fb:
                    if (_state == 1)
                        currentMode = PoolMode.Spa;
                    break;
                case PoolCommand.Pool_Heater_Fb:
                    poolHeaterOn = _state == 1 ? true : false;
                    break;
                case PoolCommand.Spa_Heater_Fb:
                    spaHeaterOn = _state == 1 ? true : false;
                    break;
                case PoolCommand.FilterPump_Fb:
                    filterPumpOn = _state == 1 ? true : false;
                    break;
                case PoolCommand.Aux_Fb:
                    currentAuxStatus[int.Parse(_text)] = _state == 1 ? true : false;
                    break;
                case PoolCommand.Pool_Setpoint_Fb:
                    currentPoolSetpoint = float.Parse(_text);
                    break;
                case PoolCommand.Pool_Temp_Fb:
                    currentPoolTemp = float.Parse(_text);
                    break;
                case PoolCommand.Spa_Setpoint_Fb:
                    currentSpaSetpoint = float.Parse(_text);
                    break;
                case PoolCommand.Spa_Temp_Fb:
                    currentSpaTemp = float.Parse(_text);
                    break;
                case PoolCommand.Ambient_Temp_Fb:
                    currentAmbientTemp = float.Parse(_text);
                    break;
                case PoolCommand.System_Type:
                    systemType = _state;
                    break;

            }

            // Broadcast to UpdateEvent subscribers
            if (this.UpdateEvent != null)
                UpdateEvent(act, _state, _text);

        }

        /**
        * Method: setAuxLabel
        * Access: public
        * Description: Receive string data from S+ to populate auxLabels[] array
        */
        public void setAuxLabel(ushort _index, string _label) {

            auxLabels[_index] = _label;

        }

        //===================// Event Handlers //===================//

    }

    public enum PoolMode {
        Unknown,
        Pool,
        Spa
    }

    public enum PoolCommand {
        Unknown,
        ModeSelect_Pool,
        ModeSelect_Spa,
        Pool_Setpoint_Up,
        Pool_Setpoint_Down,
        Pool_Heater_Toggle,
        Spa_Setpoint_Up,
        Spa_Setpoint_Down,
        Spa_Heater_Toggle,
        FilterPump_Toggle,
        Aux_Toggle,
        ModeSelect_Pool_Fb,
        ModeSelect_Spa_Fb,
        Pool_Heater_Fb,
        Spa_Heater_Fb,
        FilterPump_Fb,
        Aux_Fb,
        Pool_Setpoint_Fb,
        Pool_Temp_Fb,
        Spa_Setpoint_Fb,
        Spa_Temp_Fb,
        Ambient_Temp_Fb,
        Aux_Count_Fb,
        Aux_Label_Fb,
        System_Type
    }

}