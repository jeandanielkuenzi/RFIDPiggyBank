/*
 * Author   : Küenzi Jean-Daniel
 * Date     : 09.05.2018
 * Desc.    : Classe qui va gérer le RFIDModule de GHI
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
        private static RFIDReader _instance;

        //<summary>The RFID Reader module using socket 8 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.RFIDReader _rfidReader;
        private bool _isBadgeScan;
        private string _currentUid;

        private RFIDReader()
        {
            _rfidReader = new GTM.GHIElectronics.RFIDReader(8);
            _rfidReader.IdReceived += _rfidReader_IdReceived;
            _rfidReader.MalformedIdReceived += _rfidReader_MalformedIdReceived;
            _isBadgeScan = false;
            _currentUid = "";
        }

        public static RFIDReader Istance
        {
            get { return _instance; }
        }

        public string CurrentUid
        {
            get { return _currentUid; }
            set { _currentUid = value; }
        }

        public bool IsBadgeScan
        {
            get { return _isBadgeScan; }
            set { _isBadgeScan = value; }
        }

        public static RFIDReader GetInstance()
        {
            if (_instance == null)
            {
                _instance = new RFIDReader();
            }
            return _instance;
        }

        private void _rfidReader_MalformedIdReceived(GTM.GHIElectronics.RFIDReader sender, EventArgs e)
        {
            Debug.Print("Badge mal scanné");
            _currentUid = "";
            _isBadgeScan = false;
        }

        private void _rfidReader_IdReceived(GTM.GHIElectronics.RFIDReader sender, string id)
        {
            Debug.Print("Uid : " + id);
            _currentUid = id;
            _isBadgeScan = true;
        }
    }
}