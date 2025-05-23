﻿using Microsoft.Xna.Framework.Input;
using System;

namespace Maze
{
    /// <summary>
    /// Contains useful utilities.
    /// </summary>
    public static class KeyboardStateExt
    {
        /// <summary>
        /// Returns whether or not the specified key is a letter.
        /// </summary>
        public static bool IsLetter(Keys key)
        {
            return key >= Keys.A && key <= Keys.Z;
        }

        /// <summary>
        /// Returns whether or not the specified key is a digit.
        /// </summary>
        public static bool IsDigit(Keys key)
        {
            return (key >= Keys.D0 && key <= Keys.D9) || (key >= Keys.NumPad0 && key <= Keys.NumPad9);
        }

        /// <summary>
        /// Adds the string representation of the given key to another
        /// string. Backspace removes a character. Applies the second
        /// character on a US keyboard if shift is pressed, e.g.
        /// uppercase and symbols instead of numbers.
        /// </summary>
        /// <param name="str">
        /// A string to append the key to.
        /// </param>
        /// <param name="key">
        /// The key to append to the string.
        /// </param>
        /// <param name="shiftPressed">
        /// Whether or not shift is active. Capslock is detected automatically.
        /// </param>
        public static string KeyToString(
            string str,
            Keys key,
            bool shiftPressed)
        {
            //Checks for backspace.
            if (key == Keys.Back && str.Length != 0)
            {
                str = str.Remove(str.Length - 1);
            }

            //Checks for letters.
            else if (IsLetter(key))
            {
                if (Console.CapsLock == true || shiftPressed)
                {
                    str += key.ToString();
                }
                else
                {
                    str += key.ToString().ToLower();
                }
            }

            //Checks for keys that have shift-based variants.
            else if (shiftPressed)
            {
                //Digits
                if (key == Keys.D0 || key == Keys.NumPad0) { str += ")"; }
                else if (key == Keys.D1 || key == Keys.NumPad1) { str += "!"; }
                else if (key == Keys.D2 || key == Keys.NumPad2) { str += "@"; }
                else if (key == Keys.D3 || key == Keys.NumPad3) { str += "#"; }
                else if (key == Keys.D4 || key == Keys.NumPad4) { str += "$"; }
                else if (key == Keys.D5 || key == Keys.NumPad5) { str += "%"; }
                else if (key == Keys.D6 || key == Keys.NumPad6) { str += "^"; }
                else if (key == Keys.D7 || key == Keys.NumPad7) { str += "&"; }
                else if (key == Keys.D8 || key == Keys.NumPad8) { str += "*"; }
                else if (key == Keys.D9 || key == Keys.NumPad9) { str += "("; }

                //Punctuation
                else if (key == Keys.OemTilde) { str += "~"; }
                else if (key == Keys.OemOpenBrackets) { str += "{"; }
                else if (key == Keys.OemCloseBrackets) { str += "}"; }
                else if (key == Keys.OemSemicolon) { str += ":"; }
                else if (key == Keys.OemQuotes) { str += "\""; }
                else if (key == Keys.OemComma) { str += "<"; }
                else if (key == Keys.OemPeriod || key == Keys.Decimal)
                    { str += ">"; }
                else if (key == Keys.OemQuestion) { str += "?"; }
                else if (key == Keys.OemMinus) { str += "_"; }
                else if (key == Keys.OemPlus) { str += "+"; }
                else if (key == Keys.OemPipe) { str += "|"; }
            }
            else
            {
                //Digits
                if (key == Keys.D0 || key == Keys.NumPad0) { str += "0"; }
                else if (key == Keys.D1 || key == Keys.NumPad1) { str += "1"; }
                else if (key == Keys.D2 || key == Keys.NumPad2) { str += "2"; }
                else if (key == Keys.D3 || key == Keys.NumPad3) { str += "3"; }
                else if (key == Keys.D4 || key == Keys.NumPad4) { str += "4"; }
                else if (key == Keys.D5 || key == Keys.NumPad5) { str += "5"; }
                else if (key == Keys.D6 || key == Keys.NumPad6) { str += "6"; }
                else if (key == Keys.D7 || key == Keys.NumPad7) { str += "7"; }
                else if (key == Keys.D8 || key == Keys.NumPad8) { str += "8"; }
                else if (key == Keys.D9 || key == Keys.NumPad9) { str += "9"; }

                //Punctuation
                if (key == Keys.OemTilde) { str += "`"; }
                else if (key == Keys.OemOpenBrackets) { str += "["; }
                else if (key == Keys.OemCloseBrackets) { str += "]"; }
                else if (key == Keys.OemSemicolon) { str += ";"; }
                else if (key == Keys.OemQuotes) { str += "'"; }
                else if (key == Keys.OemComma) { str += ","; }
                else if (key == Keys.OemPeriod || key == Keys.Decimal)
                    { str += "."; }
                else if (key == Keys.OemQuestion) { str += "/"; }
                else if (key == Keys.OemMinus) { str += "-"; }
                else if (key == Keys.OemPlus) { str += "="; }
                else if (key == Keys.OemPipe) { str += @"\"; }
            }

            //Miscellaneous keys and punctuation
            if (key == Keys.Space) { str += " "; }
            else if (key == Keys.Divide) { str += "/"; }
            else if (key == Keys.Add) { str += "+"; }
            else if (key == Keys.Subtract) { str += "-"; }
            else if (key == Keys.Multiply) { str += "*"; }

            //If the key pressed was other (like unprintable chars)
            return str;
        }
    }
}