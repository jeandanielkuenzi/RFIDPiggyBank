/*
 * Author   : Küenzi Jean-Daniel
 * Date     : 09.05.2018
 * Desc.    : Classe qui va gérer le TE35 (LCD Module) de GHI
 * Version  : 1.0.0
 */
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Gadgeteer;

using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Touch;

using GTM = Gadgeteer.Modules;
using Gadgeteer.Networking;
using System.IO;
using System.Text;

namespace RFIDPiggyBank
{
    public class LCD
    {
        private static LCD _instance;

        private int _lcdWidth;
        private int _lcdHeight;
        /// <summary>The Display TE35 module using sockets 14, 13, 12 and 10 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.DisplayTE35 _lcd;

        private LCD()
        {
            _lcd = new GTM.GHIElectronics.DisplayTE35(14, 13, 12, 10);

            _lcdWidth = _lcd.Width;
            _lcdHeight = _lcd.Height;
            _lcd.BacklightEnabled = true;
            _lcd.SimpleGraphics.BackgroundColor = Gadgeteer.Color.White;
        }

        public static LCD Instance
        {
            get { return _instance; }
        }

        public int LcdWidth
        {
            get { return _lcdWidth; }
        }

        public int LcdHeight
        {
            get { return _lcdHeight; }
        }

        public static LCD GetInstance()
        {
            if (_instance == null)
            {
                _instance = new LCD();
            }
            return Instance;
        }

        public void DisplayText(Gadgeteer.Color pbColor, string pbText = "", int pbPositionX = 10, int pbPositionY = 10)
        {
            _lcd.SimpleGraphics.DisplayTextInRectangle(pbText, pbPositionX, pbPositionY, _lcdWidth, _lcdHeight, pbColor, Resources.GetFont(Resources.FontResources.NinaB));
        }

        public void Clear()
        {
            _lcd.SimpleGraphics.ClearNoRedraw();
        }
    }
}
