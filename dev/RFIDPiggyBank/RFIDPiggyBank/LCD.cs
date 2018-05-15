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

namespace RFIDPiggyBank
{
    public class LCD
    {
        private static LCD _instance;

        /// <summary>The Display TE35 module using sockets 14, 13, 12 and 10 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.DisplayTE35 _lcd;

        private LCD()
        {
            _lcd = new GTM.GHIElectronics.DisplayTE35(14, 13, 12, 10);
        }

        public static LCD Instance
        {
            get { return _instance; }
        }

        public static LCD GetInstance()
        {
            if (_instance == null)
            {
                _instance = new LCD();
            }
            return Instance;
        }

        public void DisplayMenu()
        {
            _lcd.SimpleGraphics.DisplayText("Ajouter un badge", Resources.GetFont(Resources.FontResources.NinaB), Gadgeteer.Color.Red, 10, 10);
            _lcd.SimpleGraphics.DisplayText("Suprimmer un badge", Resources.GetFont(Resources.FontResources.NinaB), Gadgeteer.Color.Red, 10, 30);
            _lcd.SimpleGraphics.DisplayText("Deverouiller avec le mot de passe", Resources.GetFont(Resources.FontResources.NinaB), Gadgeteer.Color.Red, 10, 50);
        }
    }
}
