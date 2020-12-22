﻿using Microsoft.Xna.Framework;
using System;

namespace Starforge.Util {
    public static class MiscHelper {
        public static float Angle(Vector2 from, Vector2 to) {
            return (float)Math.Atan2(to.Y - from.Y, to.X - from.X);
        }

        public static Vector2 AngleToVector(float angleRadians, float length) {
            return new Vector2((float)Math.Cos(angleRadians) * length, (float)Math.Sin(angleRadians) * length);
        }

        public static string ColorToHex(Color c) {
            return c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

        public static byte HexCharToByte(char c) {
            return (byte)"0123456789ABCDEF".IndexOf(char.ToUpper(c));
        }

        public static Color HexToColor(string hex) {
            int index = 0;
            if (hex.Length >= 1 && hex[0] == '#') index = 1;

            if (hex.Length - index >= 6) {
                return new Color(
                    HexCharToByte(hex[index]) * 16 + HexCharToByte(hex[index + 1]),
                    HexCharToByte(hex[index + 2]) * 16 + HexCharToByte(hex[index + 3]),
                    HexCharToByte(hex[index + 4]) * 16 + HexCharToByte(hex[index + 5])
                );
            } else {
                int hexNum;
                if (int.TryParse(hex.Substring(index), out hexNum)) {
                    return HexToColor(hexNum);
                } else {
                    return Color.White;
                }
            }
        }

        public static Color HexToColor(int hex) {
            Color res = default;
            res.A = byte.MaxValue;
            res.R = (byte)(hex >> 16);
            res.G = (byte)(hex >> 8);
            res.B = (byte)hex;
            return res;
        }
    }
}
