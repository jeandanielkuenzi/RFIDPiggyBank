/*
 * Author   : K�enzi Jean-Daniel
 * Date     : 09.05.2018
 * Desc.    : Classe qui contient la liste des badges et sert � les g�rer
 * Version  : 1.0.0
 */
using System;
using System.Collections;
using Microsoft.SPOT;

namespace RFIDPiggyBank
{
    [Serializable]
    class ListOfCards
    {
        private const string DEFAULT_NAME = "Bagde";
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
            pbName = (pbName == DEFAULT_NAME) ? pbName + CardsList.Count : pbName;
            Cards card = new Cards(pbName, pbUid);
            CardsList.Add(card);
        }

        public void DeleteCardFromList(string pbUid)
        {

        }
    }
}
