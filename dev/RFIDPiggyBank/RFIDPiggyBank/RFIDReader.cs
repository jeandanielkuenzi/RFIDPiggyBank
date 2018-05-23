/*
 * Author   : Küenzi Jean-Daniel
 * Date     : 09.05.2018
 * Desc.    : Class thaht handles the RFID module from GHI
 * Version  : 1.0.0
 */
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHI.Pins;
using Gadgeteer;
using GTM = Gadgeteer.Modules;

namespace RFIDPiggyBank
{
    public class RFIDReader
    {
        /// <summary>
        /// The instance of the class RFIDReader (Not the RFID Reader module from GHI)
        /// </summary>
        private static RFIDReader _instance;

        /// <summary>
        /// The RFID Reader module using socket 8 of the mainboard
        /// </summary>
        private Gadgeteer.Modules.GHIElectronics.RFIDReader _rfidReader;

        /// <summary>
        /// Has a badge been scanned
        /// </summary>
        private bool _isBadgeScan;

        /// <summary>
        /// The Uid of the badge that was scanned
        /// </summary>
        private string _currentUid;

        /// <summary>
        /// The constructor of the class, he's private because the class use the design pattern Singleton
        /// </summary>
        private RFIDReader()
        {
            _rfidReader = new GTM.GHIElectronics.RFIDReader(8);
            _rfidReader.IdReceived += _rfidReader_IdReceived;
            _rfidReader.MalformedIdReceived += _rfidReader_MalformedIdReceived;
            _isBadgeScan = false;
            _currentUid = "";
        }

        /// <summary>
        /// Getter for the instance of the class
        /// </summary>
        public static RFIDReader Istance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Getter and Setter for the current uid
        /// </summary>
        public string CurrentUid
        {
            get { return _currentUid; }
            set { _currentUid = value; }
        }

        /// <summary>
        /// Getter and Setter
        /// </summary>
        public bool IsBadgeScan
        {
            get { return _isBadgeScan; }
            set { _isBadgeScan = value; }
        }

        /// <summary>
        /// Method that allow access to the class
        /// </summary>
        /// <returns>Instance of the class RFIDReader</returns>
        public static RFIDReader GetInstance()
        {
            if (_instance == null)
            {
                _instance = new RFIDReader();
            }
            return _instance;
        }

        /// <summary>
        /// This method is if a badge is wrong scanned
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _rfidReader_MalformedIdReceived(GTM.GHIElectronics.RFIDReader sender, EventArgs e)
        {
            Debug.Print("Badge mal scanné");
            _currentUid = "";
            _isBadgeScan = false;
        }

        /// <summary>
        /// This method is when a badge is correctly scanned
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="id"></param>
        private void _rfidReader_IdReceived(GTM.GHIElectronics.RFIDReader sender, string id)
        {
            Debug.Print("Uid : " + id);
            _currentUid = id;
            _isBadgeScan = true;
        }
    }
}