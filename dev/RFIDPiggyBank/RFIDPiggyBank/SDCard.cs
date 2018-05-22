/*
 * Author   : Küenzi Jean-Daniel
 * Date     : 09.05.2018
 * Desc.    : Class that handles the writing and reading of Cards (Badge) on the GHI SD card (SDCard module)
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
    public class SDCard
    {
        /// <summary>
        /// Name of the file where the SDCard wrote and load
        /// </summary>
        private const string FILE_NAME = "Cards.xml";

        /// <summary>
        /// The SD Card module using socket 5 of the mainboard
        /// </summary>
        private GTM.GHIElectronics.SDCard _sdCard;

        /// <summary>
        /// The instance of the class SDCard (Not the SDCard module from GHI)
        /// </summary>
        private static SDCard _instance;

        /// <summary>
        /// The constructor of the class, he's private because the class use the design pattern Singleton
        /// </summary>
        private SDCard()
        {
            _sdCard = new GTM.GHIElectronics.SDCard(5);
        }

        /// <summary>
        /// Getter for the instance of the class
        /// </summary>
        public static SDCard Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Method that allow access to the class
        /// </summary>
        /// <returns>Instance of the class SDCard</returns>
        public static SDCard GetInstance()
        {
            if (_instance == null)
            {
                _instance = new SDCard();
            }
            return Instance;
        }

        /// <summary>
        /// This method serialize an ArrayList to a byte[] and save it in a file
        /// </summary>
        /// <param name="pbList">The ArrayList we want to save</param>
        public void SaveCards(ArrayList pbList)
        {
            if (!_sdCard.IsCardMounted) // If the SDCard isn't mounted
            {
                _sdCard.Mount(); // Mount the file system
            }

            do
            {
                Debug.Print("Veuillez attendre que la carte soit montée");
            } while (!_sdCard.IsCardMounted); // We wait that the SD card is correctly mounted

            string sdPath = VolumeInfo.GetVolumes()[0].RootDirectory; // Get the path to the storage

            FileStream writer = new FileStream(sdPath + @"\" + FILE_NAME, FileMode.Create, FileAccess.Write);

            if (pbList is ArrayList)
            {
                byte[] SerializedData = Reflection.Serialize(pbList, typeof(ArrayList));
                writer.Write(SerializedData, 0, SerializedData.Length);
            }

            writer.Close();

            if (_sdCard.IsCardMounted) // If the card is mounted
            {
                _sdCard.Unmount(); // Unmount the card
            }
        }

        /// <summary>
        /// This method load the file where the byte[] is wrote and deserialize it to an ArrayList
        /// </summary>
        /// <returns>An ArrayList</returns>
        public ArrayList LoadCards()
        {
            if (!_sdCard.IsCardMounted) // If the SDCard isn't mounted
            {
                _sdCard.Mount(); // Mount the file system
            }

            do
            {
                Debug.Print("Veuillez attendre que la carte soit montée");
            } while (!_sdCard.IsCardMounted); // We wait that the SD card is correctly mounted

            string rootDirectory = VolumeInfo.GetVolumes()[0].RootDirectory;

            FileStream reader = new FileStream(rootDirectory + @"\" + FILE_NAME, FileMode.Open, FileAccess.Read);

            byte[] SerializedData = new byte[reader.Length];
            reader.Read(SerializedData, 0, SerializedData.Length);

            ArrayList list = null;

            list = (ArrayList)Reflection.Deserialize(SerializedData, typeof(ArrayList));

            if (_sdCard.IsCardMounted) // If the card is mounted
            {
                _sdCard.Unmount(); // Unmount the card
            }

            return list;
        }
    }
}
