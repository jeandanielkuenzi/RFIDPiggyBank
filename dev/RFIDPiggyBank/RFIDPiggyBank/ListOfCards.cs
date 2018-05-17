/*
 * Author   : Küenzi Jean-Daniel
 * Date     : 09.05.2018
 * Desc.    : Classe qui contient la liste des Card (Badge) et sert à les gérer
 * Version  : 1.0.0
 */
using System;
using System.Collections;
using Microsoft.SPOT;

namespace RFIDPiggyBank
{
    [Serializable]
    public class ListOfCards
    {
        private ArrayList _cardsList;
        private static ListOfCards _instance;

        private ListOfCards()
        {
            _cardsList = new ArrayList();
        }

        public ArrayList CardsList
        {
            get { return _cardsList; }
            set { _cardsList = value; }
        }

        public static ListOfCards Instance
        {
            get { return _instance; }
        }

        public static ListOfCards GetInstance()
        {
            if (_instance == null)
            {
                _instance = new ListOfCards();
            }
            return Instance;
        }

        public void AddCardToList(string pbName, string pbUid)
        {
            pbName = (pbName == Card.DEFAULT_NAME) ? pbName + CardsList.Count : pbName;
            Card card = new Card(pbName, pbUid);
            CardsList.Add(card);
        }

        public void DeleteCardFromList(string pbUid)
        {

        }

        public bool FindCardInlist(string pbUid)
        {
            bool result = false;

            foreach (Card card in CardsList)
            {
                if (card.Uid == pbUid)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }
    }
}
