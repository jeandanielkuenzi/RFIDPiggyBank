using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RFIDPiggyBank;

namespace UnitTestRFIDPiggyBank
{
    [TestClass]
    public class ListOfCardsTest
    {
        [TestMethod]
        public void AddCard()
        {
            string name = "Badge";
            string uid = "6F5G446H0Z";

            ListOfCards.GetInstance().AddCardToList(name, uid);
            Assert.AreEqual(1, ListOfCards.GetInstance().CardsList.Count);
        }

        [TestMethod]
        public void DeleteCard()
        {
            string name = "Badge";
            string uid = "6F5G446H0Z";

            ListOfCards.GetInstance().DeleteCardFromList(0);
            Assert.AreEqual(0, ListOfCards.GetInstance().CardsList.Count);
        }

        [TestMethod]
        public void FindCard()
        {
            string name = "Badge";
            string uid = "6F5G446H0Z";
            ListOfCards.GetInstance().AddCardToList(name, uid);
            Assert.AreEqual(true, ListOfCards.GetInstance().FindCardInlist(uid));
        }

        [TestMethod]
        public void IsEmpty()
        {
            Assert.AreEqual(false, ListOfCards.GetInstance().IsEmpty());
        }
    }
}
