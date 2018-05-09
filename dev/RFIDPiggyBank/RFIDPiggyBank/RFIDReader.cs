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
        private GTM.GHIElectronics.RFIDReader _reader;
        private static RFIDReader _instance;

        private RFIDReader()
        {
            _reader = new GTM.GHIElectronics.RFIDReader(8);
            _reader.IdReceived += _reader_IdReceived;
            _reader.MalformedIdReceived += _reader_MalformedIdReceived;

        }

        private void _reader_MalformedIdReceived(GTM.GHIElectronics.RFIDReader sender, EventArgs e)
        {
            Debug.Print("Badge mal scanné, veuillez recommencé");
        }

        private void _reader_IdReceived(GTM.GHIElectronics.RFIDReader sender, string e)
        {
            Debug.Print("Uid : " + e.ToString());
        }

        public static RFIDReader Istance
        {
            get { return _instance; }
        }

        public static RFIDReader GetInstance()
        {
            if (_instance == null)
            {
                _instance = new RFIDReader();
            }
            return _instance;
        }
    }
}
