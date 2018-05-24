/*
 * Author   : Küenzi Jean-Daniel
 * Date     : 22.05.2018
 * Desc.    : This class manages LCD fields
 * Version  : 1.0.0
 */
using System;
using Microsoft.SPOT;

namespace RFIDPiggyBank
{
    public static class LCDTextFields
    {
        /// <summary>
        /// The content of the fields
        /// </summary>
        private static string _content;

        /// <summary>
        /// If we need to refresh the LCD
        /// </summary>
        private static bool _shouldBeRefresh;

        /// <summary>
        /// The position of the cursor
        /// </summary>
        private static int _cursorPosition;

        /// <summary>
        /// Getter and Setter
        /// </summary>
        public static string Content
        {
            get { return _content; }
            set { _content = value; }
        }

        /// <summary>
        /// Getter and Setter
        /// </summary>
        public static bool ShouldBeRefresh
        {
            get { return _shouldBeRefresh; }
            set { _shouldBeRefresh = value; }
        }

        /// <summary>
        /// Getter and Setter
        /// </summary>
        public static int CursorPosition
        {
            get { return _cursorPosition; }
            set { _cursorPosition = value; }
        }
    }
}
