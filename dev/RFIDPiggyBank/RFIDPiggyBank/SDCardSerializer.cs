/*
 * Author   : Küenzi Jean-Daniel
 * Date     : 09.05.2018
 * Desc.    : Classe qui gère l'écriture et la lecture des badges sur la carte SD (SDCard module)
 * Version  : 1.0.0
 */
using System;
using System.IO;
using System.Collections;
using System.Xml;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.IO;

using GHI.IO;
using GHI.IO.Storage;
using System.Text;

namespace RFIDPiggyBank
{
    public class SDCardSerializer
    {
        private const string FILE_NAME = "Cards.xml";

        private SDCard _sdCard;
        private static SDCardSerializer _instance;

        private SDCardSerializer()
        {
            _sdCard = new SDCard();
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

        public void SerializecCardsToSDCard(ArrayList CardsList)
        {
            // Mount the file system
            _sdCard.Mount();

            // Assume only one storage device is available
            // and that the media is formatted
            string rootDirectory = VolumeInfo.GetVolumes()[0].RootDirectory;
            FileStream FileHandle = new FileStream(rootDirectory + @"\" + FILE_NAME, FileMode.Create);
          
            FileHandle.Close();
            _sdCard.Unmount();
        }

        public void showDirectory()
        {

            // this is a non-blocking call 
            // it fires the RemovableMedia.Insert event after 
            // the mount is finished. 
            _sdCard.Mount();


            // Assume one storage device is available, access it through 
            // NETMF and display the available files and folders:
            Debug.Print("Getting files and folders:");
            if (VolumeInfo.GetVolumes()[0].IsFormatted)
            {
                string rootDirectory =
                    VolumeInfo.GetVolumes()[0].RootDirectory;
                string[] files = Directory.GetFiles(rootDirectory);
                string[] folders = Directory.GetDirectories(rootDirectory);

                Debug.Print("Files available on " + rootDirectory + ":");
                for (int i = 0; i < files.Length; i++)
                    Debug.Print(files[i]);

                Debug.Print("Folders available on " + rootDirectory + ":");
                for (int i = 0; i < folders.Length; i++)
                    Debug.Print(folders[i]);
            }
            else
            {
                Debug.Print("Storage is not formatted. " +
                    "Format on PC with FAT32/FAT16 first!");
            }
            // Unmount when done
            _sdCard.Unmount();
        }
    }
}
