/*
 * Author   : Küenzi Jean-Daniel
 * Date     : 09.05.2018
 * Desc.    : Class that contains the list of cards (Badge) and serves to manage them
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
        /// <summary>
        /// The list of cards
        /// </summary>
        private ArrayList _cardsList;

        /// <summary>
        /// The instance of the class ListOfCards
        /// </summary>
        private static ListOfCards _instance;

        /// <summary>
        /// The constructor of the class, he's private because the class use the design pattern Singleton
        /// </summary>
        private ListOfCards()
        {
            _cardsList = new ArrayList();
        }

        /// <summary>
        /// Getter and setter for the ArrayList that handle the cards
        /// </summary>
        public ArrayList CardsList
        {
            get { return _cardsList; }
            set { _cardsList = value; }
        }

        /// <summary>
        /// Getter for the instance of the class
        /// </summary>
        public static ListOfCards Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Method that allow access to the class
        /// </summary>
        /// <returns>Instance of the class ListOfCards</returns>
        public static ListOfCards GetInstance()
        {
            if (_instance == null)
            {
                _instance = new ListOfCards();
            }
            return Instance;
        }

        /// <summary>
        /// This method create an new object Card and add it into the ArrayList
        /// </summary>
        /// <param name="pbName">The name of the card</param>
        /// <param name="pbUid">The uid (RFID) of the card</param>
        public void AddCardToList(string pbName, string pbUid)
        {
            pbName = (pbName == Card.DEFAULT_NAME) ? pbName + CardsList.Count : pbName;
            Card card = new Card(pbName, pbUid);
            CardsList.Add(card);
        }

        /// <summary>
        /// This method delete an element of the ArrayList at a point given (index)
        /// </summary>
        /// <param name="pbIndex">The index where we want to delete</param>
        public void DeleteCardFromList(int pbIndex)
        {
            CardsList.RemoveAt(pbIndex);
        }

        /// <summary>
        /// This method is used to find out if the card is in the list
        /// </summary>
        /// <param name="pbUid">The uid of the card that we want to search for</param>
        /// <returns>true if we find the card | else false</returns>
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

        /// <summary>
        /// This method allow to know if the ArrayList is empty
        /// </summary>
        /// <returns>true if he's empty | else false</returns>
        public bool IsEmpty()
        {
            bool result = (CardsList.Count > 0) ? false : true;
            return result;
        }
    }
}
