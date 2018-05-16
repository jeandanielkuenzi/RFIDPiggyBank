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
        public const string DEFAULT_NAME = "Bagde";
        private ArrayList _cardsList;

        public ListOfCards()
        {
            _cardsList = new ArrayList();
        }

        public ArrayList CardsList
        {
            get { return _cardsList; }
            set { _cardsList = value; }
        }

        public void AddCardToList(string pbName, string pbUid)
        {
            pbName = (pbName == DEFAULT_NAME) ? pbName : pbName + CardsList.Count;
            Card card = new Card(pbName, pbUid);
            CardsList.Add(card);
        }

        public void DeleteCardFromList(string pbUid)
        {

        }

        public bool FindCardInlist(string Uid)
        {
            bool result = false;

            foreach (Card card in CardsList)
            {
                if (card.Uid == Uid)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }
    }
}
