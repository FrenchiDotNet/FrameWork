using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace FrameWork {

    /**
     * Class:       Lift
     * @author:     Ryan French
     * @version:    1.0
     * Description: ...
     */
    public class Lift {

        //===================// Members //===================//

        public ushort zoneID;
        public bool holdEnabled;
        public LiftType type;
        public int lastDirection;

        public ushort holdEnabledUshort { get { return (ushort)(holdEnabled ? 1 : 0); } }

        // S+ delegate for latching relay outputs
        public DelegateUshort2 LiftCommand { get; set; }

        // S+ delegate for pulsing relay outputs
        public DelegateUshort LiftCommandPulse { get; set; }

        // Event for sending feedback states to Interfaces
        public event DelegateUshort2 LiftCommandFb;

        //===================// Constructor //===================//

        public Lift() {
            lastDirection = 0;
        }

        //===================// Methods //===================//

        /**
         * Method: SetType
         * Access: public
         * @return: void
         * @param: ushort _type
         * Description: Coverts ushort to it's corresponding LiftType enum
         */
        public void SetType(ushort _type) {
            type = (LiftType)_type;
        }

        /**
         * Method: EnableHold
         * Access: public
         * @return: void
         * Description: ...
         */
        public void EnableHold() {
            holdEnabled = true;

            // Send Feedback State
            if (LiftCommandFb != null)
                LiftCommandFb(4, 1); // Hold
        }

        /**
         * Method: DisableHold
         * Access: public
         * @return: void
         * Description: ...
         */
        public void DisableHold() {
            holdEnabled = false;

            // Send Feedback State
            if (LiftCommandFb != null)
                LiftCommandFb(4, 0); // Hold
        }

        /**
         * Method: ToggleHold
         * Access: public
         * @return: void
         * Description: ...
         */
        public void ToggleHold() {
            holdEnabled = !holdEnabled;

            // Send Feedback State
            if (LiftCommandFb != null)
                LiftCommandFb(4, (ushort)(holdEnabled ? 1 : 0)); // Hold
        }

        /**
         * Method: Raise
         * Access: public
         * @return: void
         * Description: ...
         */
        public void Raise() {
            if (holdEnabled)
                return;

            switch (type) {
                case LiftType.SingleRelay: {
                    LiftCommand((ushort)LiftCommands.SingleRelay, 1);
                }
                break;
                case LiftType.DualRelayPulse: {
                    LiftCommandPulse((ushort)LiftCommands.RelayUp);
                }
                break;
                case LiftType.DualRelaySustain: {
                    LiftCommand((ushort)LiftCommands.RelayDown, 0);
                    LiftCommand((ushort)LiftCommands.RelayUp, 1);
                }
                break;
            }

            lastDirection = 1;

            // Send Feedback States
            if (LiftCommandFb != null) {
                LiftCommandFb(1, 1); // Raise
                LiftCommandFb(2, 0); // Stop
                LiftCommandFb(3, 0); // Lower
            }
        }

        /**
         * Method: Lower
         * Access: public
         * @return: void
         * Description: ...
         */
        public void Lower() {
            if (holdEnabled)
                return;

            switch (type) {
                case LiftType.SingleRelay: {
                    LiftCommand((ushort)LiftCommands.SingleRelay, 0);
                }
                break;
                case LiftType.DualRelayPulse: {
                    LiftCommandPulse((ushort)LiftCommands.RelayDown);
                }
                break;
                case LiftType.DualRelaySustain: {
                    LiftCommand((ushort)LiftCommands.RelayUp, 0);
                    LiftCommand((ushort)LiftCommands.RelayDown, 1);
                }
                break;
            }

            lastDirection = -1;

            // Send Feedback States
            if (LiftCommandFb != null) {
                LiftCommandFb(1, 0); // Raise
                LiftCommandFb(2, 0); // Stop
                LiftCommandFb(3, 1); // Lower
            }
        }

        /**
         * Method: Stop
         * Access: public
         * @return: void
         * Description: If lift is type DualRelaySustain, releases both relays. 
         *              If lift is type DualRelayPulse, pulse last known lift direction.
         */
        public void Stop() {
            if (holdEnabled)
                return;

            switch (type) {
                case LiftType.SingleRelay: {
                    // Cannot stop a single relay lift
                }
                break;
                case LiftType.DualRelayPulse: {
                    if(lastDirection == 1)
                        LiftCommandPulse((ushort)LiftCommands.RelayUp);
                    else if(lastDirection == -1)
                        LiftCommandPulse((ushort)LiftCommands.RelayDown);
                }
                break;
                case LiftType.DualRelaySustain: {
                    LiftCommand((ushort)LiftCommands.RelayDown, 0);
                    LiftCommand((ushort)LiftCommands.RelayUp, 0);
                }
                break;
            }

            lastDirection = 0;

            // Send Feedback States
            if (LiftCommandFb != null) {
                LiftCommandFb(1, 0); // Raise
                LiftCommandFb(2, 1); // Stop
                LiftCommandFb(3, 0); // Lower
            }
        }

        //===================// Event Handlers //===================//

        /**
         * Method:      ZoneSourceUpdateHandler
         * Access:      public
         * @param:      ushort _sourceID
         * Description: Triggered by parent Zone when it's currentSource is changed. Used to control lift state for AV routing.
         */
        public void ZoneSourceUpdateHandler(ushort _sourceID) {
            // Check if Source is Audio or Video
            if (!Core.Sources[_sourceID].isVideoSource) {
                Lower();
            } else {
                Raise();
            }
        }

    } // End Lift Class

    public enum LiftType {
        SingleRelay = 1,
        DualRelayPulse,
        DualRelaySustain
    }

    public enum LiftCommands {
        RelayUp = 1,
        RelayDown,
        SingleRelay
    }

}