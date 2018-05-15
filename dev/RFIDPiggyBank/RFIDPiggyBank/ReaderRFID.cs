using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHI.Pins;
using Gadgeteer;
using GTM = Gadgeteer.Modules;

namespace RFIDPiggyBank
{
    public class ReaderRFID
    {
        private static ReaderRFID _instance;

        //<summary>The RFID Reader module using socket 8 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.RFIDReader _rfidReader;

        private ReaderRFID()
        {
            _rfidReader = new GTM.GHIElectronics.RFIDReader(8);
            _rfidReader.IdReceived += _rfidReader_IdReceived;
            _rfidReader.MalformedIdReceived += _rfidReader_MalformedIdReceived;
        }

        private void _rfidReader_MalformedIdReceived(GTM.GHIElectronics.RFIDReader sender, EventArgs e)
        {
            Debug.Print("Badge mal scanné");
        }

        private void _rfidReader_IdReceived(GTM.GHIElectronics.RFIDReader sender, string id)
        {
            Debug.Print("Uid : " + id);
        }

        public static ReaderRFID Istance
        {
            get { return _instance; }
        }

        public static ReaderRFID GetInstance()
        {
            if (_instance == null)
            {
                _instance = new ReaderRFID();
            }
            return _instance;
        }
    }
}