/*
 * Author   : K�enzi Jean-Daniel
 * Date     : 09.05.2018
 * Desc.    : Classe qui g�re l'�criture et la lecture des Cards (Badge) sur la carte SD (SDCard module) de GHI
 * Version  : 1.0.0
 */
using System;
using System.IO;
using System.Collections;
using System.Ext.Xml;
using System.Xml;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.IO;

using GHI.IO;
using GHI.IO.Storage;
using System.Text;

using GTM = Gadgeteer.Modules;

namespace RFIDPiggyBank
{
    public class SDCardSerializer
    {
        private const string FILE_NAME = "Cards.xml";

        /// <summary>The SD Card module using socket 5 of the mainboard.</summary>
        private GTM.GHIElectronics.SDCard _sdCard;
        private static SDCardSerializer _instance;

        private SDCardSerializer()
        {
            _sdCard = new GTM.GHIElectronics.SDCard(5);
        }

        public static SDCardSerializer Instance
        {
            get { return _instance; }
        }

        public static SDCardSerializer GetInstance()
        {
            if (_instance == null)
            {
                _instance = new SDCardSerializer();
            }
            return Instance;
        }

        public ArrayList DeserializeCardsToList()
        {
            ArrayList CardsList = new ArrayList();

            return CardsList;
        }

        public void SaveCards(ListOfCards list)
        {
            // Mount the file system
            _sdCard.Mount();

            // Assume only one storage device is available
            // and that the media is formatted

            do
            {
                Debug.Print("Veuillez attendre que la carte soit mont�e");
            } while (!_sdCard.IsCardMounted);

            string rootDirectory = VolumeInfo.GetVolumes()[0].RootDirectory;

            FileStream writer = new FileStream(rootDirectory + @"\" + FILE_NAME, FileMode.Create, FileAccess.Write);

            byte[] SerializedData = Reflection.Serialize(list, typeof(ListOfCards));

            writer.Write(SerializedData, 0, SerializedData.Length);

            writer.Close();

            _sdCard.Unmount();
        }

        public ListOfCards LoadCards()
        {
            do
            {
                Debug.Print("Ins�rer une carte dans le lecteur");
            } while (!_sdCard.IsCardInserted);

            // Mount the file system
            _sdCard.Mount();

            // Assume only one storage device is available
            // and that the media is formatted
            do
            {
                Debug.Print("Veuillez attendre que la carte soit mont�e");
            } while (!_sdCard.IsCardMounted);

            string rootDirectory = VolumeInfo.GetVolumes()[0].RootDirectory;

            FileStream reader = new FileStream(rootDirectory + @"\" + FILE_NAME, FileMode.Open, FileAccess.Read);

            byte[] SerializedData = new byte[reader.Length];
            reader.Read(SerializedData, 0, SerializedData.Length);

            ListOfCards list = null;

            list = (ListOfCards)Reflection.Deserialize(SerializedData, typeof(ListOfCards));

            return list;
        }
    }
}
