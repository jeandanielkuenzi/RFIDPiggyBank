/*
 * Author   : Küenzi Jean-Daniel
 * Date     : 09.05.2018
 * Desc.    : L'objet Card => Badge RFID
 * Version  : 1.0.0
 */
using System;
using Microsoft.SPOT;

namespace RFIDPiggyBank
{
    [Serializable]
    public class Card
    {

        private string _name;
        private string _uid;

        public Card(string pbName, string pbUid)
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
