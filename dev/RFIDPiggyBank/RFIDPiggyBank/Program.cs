using System;
using Microsoft.SPOT;
using Gadgeteer;

namespace RFIDPiggyBank
{
    public class Program
    {
        private static Mainboard mainboard = new GHIElectronics.Gadgeteer.FEZSpider();
        public static void Main()
        {
            Debug.Print(Resources.GetString(Resources.StringResources.String1));


            ListOfCards.GetInstance().AddCardToList("Badge", "U1 2D 34 56 FC");
            ListOfCards.GetInstance().AddCardToList("Badge", "U9 65 B2 3C F5");
            ListOfCards.GetInstance().AddCardToList("Badge", "U1 2D 34 56 FC");
            ListOfCards.GetInstance().AddCardToList("Badge", "U9 65 B2 3C F5");
            ListOfCards.GetInstance().AddCardToList("Badge", "U1 2D 34 56 FC");
            ListOfCards.GetInstance().AddCardToList("Badge", "U9 65 B2 3C F5");

            //SDCardSerializer.GetInstance().SerializecCardsToSDCard(ListOfCards.GetInstance().CardsList);
            //SDCardSerializer.GetInstance().showDirectory();
            RFIDReader.GetInstance();
        }

        private void AddBadge()
        {

        }

        private void DeleteBadge()
        {

        }
    }
}
