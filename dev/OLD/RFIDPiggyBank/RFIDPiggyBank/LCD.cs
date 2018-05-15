using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHI.Pins;
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
        private GTM.GHIElectronics.DisplayTE35 _lcd = new GTM.GHIElectronics.DisplayTE35(14, 13, 12);

        private LCD()
        {

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
            //_lcd.SimpleGraphics.DisplayText("Menu à affiché", , Gadgeteer.Color.Blue, 0, 10);
        }
    }
}
