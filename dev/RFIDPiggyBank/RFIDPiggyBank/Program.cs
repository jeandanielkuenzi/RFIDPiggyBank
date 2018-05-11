using System;
using Microsoft.SPOT;
using Gadgeteer;
using Microsoft.SPOT.Hardware;
using GHI.Pins;

using GTM = Gadgeteer.Modules;
using System.Threading;

namespace RFIDPiggyBank
{
    public class Program
    {
        private static Mainboard mainboard = new GHIElectronics.Gadgeteer.FEZSpider();
        private static bool test = false;
        static GTM.GHIElectronics.RFIDReader _reader = new GTM.GHIElectronics.RFIDReader(8);
        static OutputPort mainled = new OutputPort(FEZSpider.DebugLed, false);

        public static void Main()
        {
            Debug.Print(Resources.GetString(Resources.StringResources.String1));

            AnalogInput joystick_y = new AnalogInput(FEZSpider.Socket9.AnalogInput4); //Initialise l'axe Y par rapport à l'orientation de mon joystick

            _reader.IdReceived += _reader_IdReceived;
            _reader.MalformedIdReceived += _reader_MalformedIdReceived;

            ListOfCards.GetInstance().AddCardToList("Badge", "U1 2D 34 56 FC");
            ListOfCards.GetInstance().AddCardToList("Badge", "U9 65 B2 3C F5");
            ListOfCards.GetInstance().AddCardToList("Badge", "U1 2D 34 56 FC");
            ListOfCards.GetInstance().AddCardToList("Badge", "U9 65 B2 3C F5");
            ListOfCards.GetInstance().AddCardToList("Badge", "U1 2D 34 56 FC");
            ListOfCards.GetInstance().AddCardToList("Badge", "U9 65 B2 3C F5");
            ListOfCards.GetInstance().AddCardToList("Marty", "U1 2D 34 56 FC");
            ListOfCards.GetInstance().AddCardToList("Professeur", "U9 65 B2 3C F5");

            //SDCardSerializer.GetInstance().SerializecCardsToSDCard(ListOfCards.GetInstance().CardsList);
            //SDCardSerializer.GetInstance().showDirectory();
            //RFIDReader.GetInstance();
            ServoMotor.GetInstance();

            while (true)
            {
                Debug.Print(joystick_y.Read().ToString());
                if (joystick_y.Read() == 1)
                {
                    ServoMotor.GetInstance().Unlock();
                }
                else if (joystick_y.Read() == 0)
                {
                    ServoMotor.GetInstance().Lock();
                }
            }
        }

        private static void _reader_MalformedIdReceived(GTM.GHIElectronics.RFIDReader sender, EventArgs e)
        {
            Debug.Print("Badge mal scanné, veuillez recommencé");
        }

        private static void _reader_IdReceived(GTM.GHIElectronics.RFIDReader sender, string e)
        {
            Debug.Print("Uid : " + e.ToString());
        }

        private void AddBadge()
        {

        }

        private void DeleteBadge()
        {

        }
    }
}
