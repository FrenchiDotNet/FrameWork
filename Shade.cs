using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace FrameWork {

    /**
     * Class:       Shade
     * @author:     Ryan French
     * @version:    1.3a
     * Description: Class for storing details and control functions for an individual shade.
     */
    public class Shade {

        //===================// Members //===================//

        public ushort id;
        public string name;
        public ShadeControlType controlType;

        public bool Up_Fb;
        public bool Down_Fb;
        public bool Stop_Fb;
        public bool Moving_Fb;

        // Delegate to send Shade Commands to S+
        public DelegateUshort2 TriggerShadeCommand { get; set; }

        // Delegate to send Feedback to subscribed Interfaces
        public delegate void DelegateShadeFbUpdate(ushort id, ShadeCommand action, ushort state);
        public event DelegateShadeFbUpdate UpdateShadeFb;

        //===================// Constructor //===================//

        public Shade() {

            // Set empty values to be populated by S+
            id = 0;
            name = "";
            controlType = 0;

        }

        //===================// Methods //===================//

        /**
        * Method: CreateShade
        * Access: public
        * Description: Populate object with values passed from S+
        */
        public void CreateShade(ushort _id, string _name, ushort _controlType) {

            this.id          = _id;
            this.name        = _name;
            this.controlType = (ShadeControlType)_controlType;

        }

        /**
        * Method: FeedbackEvent
        * Access: public
        * Description: Receive Feedback signals from S+ and propogate them to UpdateShadeFB event subscribers
        */
        public void FeedbackEvent (ushort _action, ushort _state) {

            ShadeCommand _act = (ShadeCommand)_action;

            // Store Fb in class
            switch (_act) {
                case ShadeCommand.Up_Fb:
                    this.Up_Fb = _state == 1 ? true : false;
                    break;
                case ShadeCommand.Down_Fb:
                    this.Down_Fb = _state == 1 ? true : false;
                    break;
                case ShadeCommand.Stop_Fb:
                    this.Stop_Fb = _state == 1 ? true : false;
                    break;
                case ShadeCommand.Moving_Fb:
                    this.Moving_Fb = _state == 1 ? true : false;
                    break;
            }

            // Broadcast to Event subscribers
            if (this.UpdateShadeFb != null)
                UpdateShadeFb(this.id, _act, _state);
            
        }

        /**
        * Method: SendShadeCommand
        * Access: public
        * Description: Receive Commands from Interfaces and pass to S+ via TriggerShadeCommand delegate
        */
        public void SendShadeCommand (ShadeCommand _cmd, ushort _state) {

            if (TriggerShadeCommand != null)
                TriggerShadeCommand ((ushort)_cmd, _state);

        }

        /**
        * Method: subscribeToEvents
        * Access: public
        * @return: void
        * Description: ...
        */
        public void subscribeToEvents(Interface _int) {

            this.UpdateShadeFb += _int.ShadeFbHandler;

            SendShadeCommand(ShadeCommand.InUseState, (ushort)(UpdateShadeFb != null ? 1 : 0));

        }

        /**
        * Method: unsubscribeFromEvents
        * Access: public
        * @return: void
        * Description: ...
        */
        public void unsubscribeFromEvents(Interface _int) {

            this.UpdateShadeFb -= _int.ShadeFbHandler;

            SendShadeCommand(ShadeCommand.InUseState, (ushort)(UpdateShadeFb != null ? 1 : 0));

        }

        //===================// Event Handlers //===================//

    } // End Shade Class

    public enum ShadeControlType {
        Unknown,
        Toggle,
        UpDown,
        UpDownStop,
        UpDownStopView
    }

    public enum ShadeCommand {
        Unknown,
        Up,
        Down,
        Toggle,
        Stop,
        View,
        Up_Fb = 11,
        Down_Fb,
        Stop_Fb,
        Moving_Fb,
        InUseState
    }

}