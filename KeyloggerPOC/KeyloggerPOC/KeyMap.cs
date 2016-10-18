namespace EtwKeylogger
{
    // hxxp://www.usb.org/developers/hidpage/Hut1_12v2.pdf
    public class KeyMap
    {
        public static string[] GetKey(int value)
        {
            switch (value)
            {
                /*
                case 0x00: return new[] { "Reserved" };
                case 0x01: return new[] { "ErrorRollOver" };
                case 0x02: return new[] { "POSTFail" };
                case 0x03: return new[] { "ErrorUndefined" };
                */
                case 0x00: return new string[1]; // unused
                case 0x04: return new[] { "a", "A" };
                case 0x05: return new[] { "b", "B" };
                case 0x06: return new[] { "c", "C" };
                case 0x07: return new[] { "d", "D" };
                case 0x08: return new[] { "e", "E" };
                case 0x09: return new[] { "f", "F" };
                case 0x0A: return new[] { "g", "G" };
                case 0x0B: return new[] { "h", "H" };
                case 0x0C: return new[] { "i", "I" };
                case 0x0D: return new[] { "j", "J" };
                case 0x0E: return new[] { "k", "K" };
                case 0x0F: return new[] { "l", "L" };
                case 0x10: return new[] { "m", "M" };
                case 0x11: return new[] { "n", "N" };
                case 0x12: return new[] { "o", "O" };
                case 0x13: return new[] { "p", "P" };
                case 0x14: return new[] { "q", "Q" };
                case 0x15: return new[] { "r", "R" };
                case 0x16: return new[] { "s", "S" };
                case 0x17: return new[] { "t", "T" };
                case 0x18: return new[] { "u", "U" };
                case 0x19: return new[] { "v", "V" };
                case 0x1A: return new[] { "w", "W" };
                case 0x1B: return new[] { "x", "X" };
                case 0x1C: return new[] { "y", "Y" };
                case 0x1D: return new[] { "z", "Z" };
                case 0x1E: return new[] { "1", "!" };
                case 0x1F: return new[] { "2", "@" };
                case 0x20: return new[] { "3", "#" };
                case 0x21: return new[] { "4", "$" };
                case 0x22: return new[] { "5", "%" };
                case 0x23: return new[] { "6", "^" };
                case 0x24: return new[] { "7", "&" };
                case 0x25: return new[] { "8", "*" };
                case 0x26: return new[] { "9", "(" };
                case 0x27: return new[] { "0", ")" };
                case 0x28: return new[] { "[RET]" };
                case 0x29: return new[] { "[ESC]" };
                case 0x2A: return new[] { "[DEL]" };
                case 0x2B: return new[] { "[TAB]" };
                case 0x2C: return new[] { "[SPACE]" };
                case 0x2D: return new[] { "-", "_" };
                case 0x2E: return new[] { "=", "+" };
                case 0x2F: return new[] { "[", "{" };
                case 0x30: return new[] { "]", "}" };
                case 0x31: return new[] { "\\", "|" };
                case 0x32: return new[] { "#", "~" };       //non-US
                case 0x33: return new[] { ";", ":" };
                case 0x34: return new[] { "\'", "\"" };
                case 0x35: return new[] { "`", "~" };
                case 0x36: return new[] { ";", "<" };
                case 0x37: return new[] { ".", ">" };
                case 0x38: return new[] { "/", "?" };
                case 0x39: return new[] { "[CAPS]" };
                case 0x3A: return new[] { "[F1]" };
                case 0x3B: return new[] { "[F2]" };
                case 0x3C: return new[] { "[F3]" };
                case 0x3D: return new[] { "[F4]" };
                case 0x3E: return new[] { "[F5]" };
                case 0x3F: return new[] { "[F6]" };
                case 0x40: return new[] { "[F7]" };
                case 0x41: return new[] { "[F8]" };
                case 0x42: return new[] { "[F9]" };
                case 0x43: return new[] { "[F10]" };
                case 0x44: return new[] { "[F11]" };
                case 0x45: return new[] { "[F12]" };
                case 0x46: return new[] { "[PRT]" };
                case 0x47: return new[] { "[SCL]" };
                case 0x48: return new[] { "[PAU]" };
                case 0x49: return new[] { "[INS]" };
                case 0x4A: return new[] { "[HOME]" };
                case 0x4B: return new[] { "[P-UP]" };
                case 0x4C: return new[] { "[FWD]" };
                case 0x4D: return new[] { "[END]" };
                case 0x4E: return new[] { "[P-DN]" };
                case 0x4F: return new[] { "[RT]" };
                case 0x50: return new[] { "[LT]" };
                case 0x51: return new[] { "[DN]" };
                case 0x52: return new[] { "[UP]" };
                case 0x53: return new[] { "[NUM]", "[CLR]" };
                case 0x54: return new[] { "/" };
                case 0x55: return new[] { "*" };
                case 0x56: return new[] { "-" };
                case 0x57: return new[] { "+" };
                case 0x58: return new[] { "[ENTER]" };
                case 0x59: return new[] { "1", "[END]" };
                case 0x5A: return new[] { "2", "[DN]" };
                case 0x5B: return new[] { "3", "[P-DN]" };
                case 0x5C: return new[] { "4", "[LT]" };
                case 0x5D: return new[] { "5" };
                case 0x5E: return new[] { "6", "[RT]" };
                case 0x5F: return new[] { "7", "[HOME]" };
                case 0x60: return new[] { "8", "[UP]" };
                case 0x61: return new[] { "9", "[P-UP]" };
                case 0x62: return new[] { "0", "[INS]" };
                case 0x63: return new[] { ".", "[DEL]" };
                case 0x64: return new[] { "\\", "|" };      //non-US
                /*
                case 0x65: return new[] { "[Application]" };
                case 0x66: return new[] { "[Power]" };
                case 0x67: return new[] { "=" };
                case 0x68: return new[] { "[F13]" };
                case 0x69: return new[] { "[F14]" };
                case 0x6A: return new[] { "[F15]" };
                case 0x6B: return new[] { "[F16]" };
                case 0x6C: return new[] { "[F17]" };
                case 0x6D: return new[] { "[F18]" };
                case 0x6E: return new[] { "[F19]" };
                case 0x6F: return new[] { "[F20]" };
                case 0x70: return new[] { "[F21]" };
                case 0x71: return new[] { "[F22]" };
                case 0x72: return new[] { "[F23]" };
                case 0x73: return new[] { "[F24]" };
                case 0x74: return new[] { "[Execute]" };
                case 0x75: return new[] { "[Help]" };
                case 0x76: return new[] { "[Menu]" };
                case 0x77: return new[] { "[Select]" };
                case 0x78: return new[] { "[Stop]" };
                case 0x79: return new[] { "[Again]" };
                case 0x7A: return new[] { "[Undo]" };
                case 0x7B: return new[] { "[Cut]" };
                case 0x7C: return new[] { "[Copy]" };
                case 0x7D: return new[] { "[Paste]" };
                case 0x7E: return new[] { "[Find]" };
                case 0x7F: return new[] { "[Mute]" };
                case 0x80: return new[] { "[VolumeUp]" };
                case 0x81: return new[] { "[VolumeDown]" };
                case 0x82: return new[] { "[CapsLock]" };     //locking
                case 0x83: return new[] { "[NumLock]" };      //locking
                case 0x84: return new[] { "[ScrollLock]" };   //locking
                case 0x85: return new[] { ";" };
                case 0x86: return new[] { "=" };
                case 0x87: return new[] { "[Intl1]" };
                case 0x88: return new[] { "[Intl2]" };
                case 0x89: return new[] { "[Intl3]" };
                case 0x8A: return new[] { "[Intl4]" };
                case 0x8B: return new[] { "[Intl5]" };
                case 0x8C: return new[] { "[Intl6]" };
                case 0x8D: return new[] { "[Intl7]" };
                case 0x8E: return new[] { "[Intl8]" };
                case 0x8F: return new[] { "[Intl9]" };
                case 0x90: return new[] { "[LANG1]" };
                case 0x91: return new[] { "[LANG2]" };
                case 0x92: return new[] { "[LANG3]" };
                case 0x93: return new[] { "[LANG4]" };
                case 0x94: return new[] { "[LANG5]" };
                case 0x95: return new[] { "[LANG6]" };
                case 0x96: return new[] { "[LANG7]" };
                case 0x97: return new[] { "[LANG8]" };
                case 0x98: return new[] { "[LANG9]" };
                case 0x99: return new[] { "[Erase]" };        //alternate
                case 0x9A: return new[] { "[SysReq]" };
                case 0x9B: return new[] { "[Cancel]" };
                case 0x9C: return new[] { "[Clear]" };
                case 0x9D: return new[] { "[Prior]" };
                case 0x9E: return new[] { "[Return]" };
                case 0x9F: return new[] { "[Separator]" };
                case 0xA0: return new[] { "[Out]" };
                case 0xA1: return new[] { "[Oper]" };
                case 0xA2: return new[] { "[Clear]" };        //again
                case 0xA3: return new[] { "[CrSel]" };        //props
                case 0xA4: return new[] { "[ExSel]" };
                case 0xE0: return new[] { "[LtCtrl]" };
                case 0xE1: return new[] { "[LtShift]" };
                case 0xE2: return new[] { "[LtAlt]" };
                case 0xE3: return new[] { "[LtGUI]" };
                case 0xE4: return new[] { "[RtControl]" };
                case 0xE5: return new[] { "[RtShift]" };
                case 0xE6: return new[] { "[RtAlt]" };
                case 0xE7: return new[] { "[RtGUI]" };
                */
                default: return null;
            }
        }
    }
}