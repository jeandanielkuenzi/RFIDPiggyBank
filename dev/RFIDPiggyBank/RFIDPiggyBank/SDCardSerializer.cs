/*
 * Author   : Küenzi Jean-Daniel
 * Date     : 09.05.2018
 * Desc.    : Classe qui gère l'écriture et la lecture des badges sur la carte SD (SDCard module)
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

        public void SaveCards()
        {
            // Mount the file system
            _sdCard.Mount();

            // Assume only one storage device is available
            // and that the media is formatted

            do
            {

            } while (!_sdCard.IsCardMounted);

            string rootDirectory = VolumeInfo.GetVolumes()[0].RootDirectory;

            FileStream writer = new FileStream(rootDirectory + @"\" + FILE_NAME, FileMode.Create);

            //XmlWriter xmlWriter = XmlWriter.Create(writer);

            //xmlWriter.WriteStartElement("CardsList");
            //foreach (Card card in CardsList)
            //{
            //    xmlWriter.WriteStartElement("Card");

            //    xmlWriter.WriteElementString("Name", card.Name);
            //    xmlWriter.WriteElementString("Uid", card.Uid);

            //    xmlWriter.WriteEndElement();
            //}
            //xmlWriter.WriteEndElement();

            //xmlWriter.Close();

            writer.Close();

            _sdCard.Unmount();
        }

        public void LoadCards()
        {
            do
            {
                Debug.Print("Insérer une carte dans le lecteur");
            } while (!_sdCard.IsCardInserted);

            // Mount the file system
            _sdCard.Mount();

            // Assume only one storage device is available
            // and that the media is formatted

            do
            {
                Debug.Print("Veuillez attendre que la carte soit montée");
            } while (!_sdCard.IsCardMounted);

            string rootDirectory = VolumeInfo.GetVolumes()[0].RootDirectory;

            FileStream reader = new FileStream(rootDirectory + @"\" + FILE_NAME, FileMode.Create);

            XmlReader xmlReader = XmlReader.Create(reader);
        }
    }
}
