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

        public ushort id;
        public string name;

        public SecurityArmState currentArmState;
        public string currentKeypadText;

    }

    public enum SecurityArmState {
        Unknown,
        Disarmed,
        ArmAway,
        ArmHome
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
        Keypad_Star,
        Keypad_Pound,
        Keypad_Exit,
        Keypad_Stay,
        Keypad_Chime,
        Keypad_Bypass,
        Disarm,
        Arm,
        ArmAway,
        ArmHome
    }

}