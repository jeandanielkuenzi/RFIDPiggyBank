/*
 * Author   : Küenzi Jean-Daniel
 * Date     : 09.05.2018
 * Desc.    : Classe qui sert à instancier les badges RFID
 * Version  : 1.0.0
 */
using System;
using Microsoft.SPOT;

namespace RFIDPiggyBank
{
    [Serializable]
    public class Cards
    {

        private string _name;
        private string _uid;

        public Cards(string pbName, string pbUid)
        {
            Name = pbName;
            Uid = pbUid;
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Uid
        {
            get { return _uid; }
            set { _uid = value; }
        }
    }
}
