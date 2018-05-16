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

        private const int MENU1_Y = 10;
        private const int MENU2_Y = 30;
        private const int MENU3_Y = 50;
        private const int MENU4_Y = 70;

        private int _lcdWidth;
        private int _lcdHeight;
        /// <summary>The Display TE35 module using sockets 14, 13, 12 and 10 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.DisplayTE35 _lcd;

        private LCD()
        {
            _lcd = new GTM.GHIElectronics.DisplayTE35(14, 13, 12, 10);

            _lcdWidth = _lcd.Width;
            _lcdHeight = _lcd.Height;
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

        public void DisplayMainMenu(int _menu)
        {
            _lcd.SimpleGraphics.DisplayText("Ajouter un badge", Resources.GetFont(Resources.FontResources.NinaB), Gadgeteer.Color.Red, 10, MENU1_Y);
            _lcd.SimpleGraphics.DisplayText("Supprimer un badge", Resources.GetFont(Resources.FontResources.NinaB), Gadgeteer.Color.Red, 10, MENU2_Y);
            _lcd.SimpleGraphics.DisplayText("Afficher la liste des badges", Resources.GetFont(Resources.FontResources.NinaB), Gadgeteer.Color.Red, 10, MENU3_Y);
            _lcd.SimpleGraphics.DisplayText("Deverouiller avec le mot de passe", Resources.GetFont(Resources.FontResources.NinaB), Gadgeteer.Color.Red, 10, MENU4_Y);
            switch (_menu)
            {
                case 1:
                    _lcd.SimpleGraphics.DisplayText("Ajouter un badge", Resources.GetFont(Resources.FontResources.NinaB), Gadgeteer.Color.White, 10, MENU1_Y);
                    break;
                case 2:
                    _lcd.SimpleGraphics.DisplayText("Supprimer un badge", Resources.GetFont(Resources.FontResources.NinaB), Gadgeteer.Color.White, 10, MENU2_Y);
                    break;
                case 3:
                    _lcd.SimpleGraphics.DisplayText("Afficher la liste des badges", Resources.GetFont(Resources.FontResources.NinaB), Gadgeteer.Color.White, 10, MENU3_Y);
                    break;
                case 4:
                    _lcd.SimpleGraphics.DisplayText("Deverouiller avec le mot de passe", Resources.GetFont(Resources.FontResources.NinaB), Gadgeteer.Color.White, 10, MENU4_Y);
                    break;
            }
        }

        public void DisplayText(Gadgeteer.Color color, string Text = "", int positionX = 10, int positionY = 10)
        {
            _lcd.SimpleGraphics.DisplayTextInRectangle(Text, positionX, positionY, _lcdWidth, _lcdHeight, color, Resources.GetFont(Resources.FontResources.NinaB));
        }

        public void Clear()
        {
            _lcd.SimpleGraphics.ClearNoRedraw();
        }
    }
}
