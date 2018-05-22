/*
 * Author   : Küenzi Jean-Daniel
 * Date     : 22.05.2018
 * Desc.    : 
 * Version  : 1.0.0
 */
using System;
using Microsoft.SPOT;

namespace RFIDPiggyBank
{
    public static class LCDTextField
    {

        private static string _content;
        private static bool _shouldBeRefresh;
        private static int _cursorPosition;

        public static string Content
        {
            get { return _content; }
            set { _content = value; }
        }

        public static bool ShouldBeRefresh
        {
            get { return _shouldBeRefresh; }
            set { _shouldBeRefresh = value; }
        }

        public static int CursorPosition
        {
            get { return _cursorPosition; }
            set { _cursorPosition = value; }
        }
    }
}
