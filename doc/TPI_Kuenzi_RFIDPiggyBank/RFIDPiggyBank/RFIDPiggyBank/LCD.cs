/*
 * Author   : Küenzi Jean-Daniel
 * Date     : 09.05.2018
 * Desc.    : Class that handles the TE35 (LCD Module) from GHI
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
        /// <summary>
        /// The instance of the class LCD
        /// </summary>
        private static LCD _instance;

        /// <summary>
        /// The width of the TE35 Screen
        /// </summary>
        private int _lcdWidth;

        /// <summary>
        /// The height of the TE35 Screen
        /// </summary>
        private int _lcdHeight;
        /// <summary>
        /// The Display TE35 module using sockets 14, 13, 12 and 10 of the mainboard.
        /// </summary>
        private Gadgeteer.Modules.GHIElectronics.DisplayTE35 _lcd;

        /// <summary>
        /// The constructor of the class, he's private because the class use the design pattern Singleton
        /// </summary>
        private LCD()
        {
            _lcd = new GTM.GHIElectronics.DisplayTE35(14, 13, 12, 10);

            _lcdWidth = _lcd.Width;
            _lcdHeight = _lcd.Height;
            _lcd.BacklightEnabled = true;
            _lcd.SimpleGraphics.BackgroundColor = Gadgeteer.Color.White;
        }

        /// <summary>
        /// Getter fot the instance of the class
        /// </summary>
        public static LCD Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Getter for the lcd width
        /// </summary>
        public int LcdWidth
        {
            get { return _lcdWidth; }
        }

        /// <summary>
        /// Getter for the lcd height
        /// </summary>
        public int LcdHeight
        {
            get { return _lcdHeight; }
        }

        /// <summary>
        /// Method that allow access to the class
        /// </summary>
        /// <returns>Instance of the class LCD</returns>
        public static LCD GetInstance()
        {
            if (_instance == null)
            {
                _instance = new LCD();
            }
            return Instance;
        }

        /// <summary>
        /// This method allow to write on the TE35 Display
        /// </summary>
        /// <param name="pbColor">The color of the text (Gadgeteer.Color)</param>
        /// <param name="pbText">The text that we want to write</param>
        /// <param name="pbPositionX">The position X on the screen (Default = 10)</param>
        /// <param name="pbPositionY">The position Y on the screen (Default = 10)</param>
        public void DisplayText(Gadgeteer.Color pbColor, string pbText = "", int pbPositionX = 10, int pbPositionY = 10)
        {
            _lcd.SimpleGraphics.DisplayTextInRectangle(pbText, pbPositionX, pbPositionY, _lcdWidth, _lcdHeight, pbColor, Resources.GetFont(Resources.FontResources.NinaB));
        }

        /// <summary>
        /// This method clear the lcd but without a redraw
        /// </summary>
        public void Clear()
        {
            _lcd.SimpleGraphics.ClearNoRedraw();
        }
    }
}
