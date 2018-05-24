/*
 * Author   : Küenzi Jean-Daniel
 * Date     : 09.05.2018
 * Desc.    : This is the main program, where logic and calls to other classes are made
 * Version  : 1.0.0
 */
using System;
using System.Collections;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;

using GHI.Pins;
using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;
using Microsoft.SPOT.Hardware;

namespace RFIDPiggyBank
{
    public partial class Program
    {
        /// <summary>
        /// The state of the menu
        /// </summary>
        private enum MENU_STATE { initial, addCard, deleteCard, displayCards, secretCode };

        /// <summary>
        /// The state of the servomotor, we us open and close because lock is a reserved type (open = unlock & close = lock)
        /// </summary>
        private enum SERVO_STATE { open, close };

        /// <summary>
        /// The secret sequel to the code (it looks like konami code)
        /// </summary>
        private enum SECRET_CODE { up1, up2, down1, down2, left1, right1, left2, right2, success, error };

        /// <summary>
        /// The state sequel when a card (RFID badge) is scan
        /// </summary>
        private enum SCAN_CARD_STATE { waitRFID, RFIDDetected, RFIDValid, RFIDInvalid };

        /// <summary>
        /// The state sequel when we add a card (RFID Badge)
        /// </summary>
        private enum ADD_CARD_STATE { waitRFID, RFIDDetected, badgeExist, bageDontExist, save, errorMSG, successMSG };

        /// <summary>
        /// The state sequel when we display all cards (RFID Badges)
        /// </summary>
        private enum DISPLAY_CARDS_STATE { listIsEmpty, errorMSG, displayAllCards };

        /// <summary>
        /// The state sequel when we delete a card (RFID Badge)
        /// </summary>
        private enum DELETE_CARD_STATE { listIsEmpty, selectCard, save, errorMSG, success };

        /// <summary>
        /// Constant for the menu position on the LCD
        /// </summary>
        private const int MENU1_Y = 10;
        private const int MENU2_Y = 30;
        private const int MENU3_Y = 50;
        private const int MENU4_Y = 70;

        /// <summary>
        /// Constant of the value when the joystick is up or right /!\ It's can change with the orientation of the josytick /!\
        /// </summary>
        private const double JOYSTICK_UP_RIGHT = 0.4;

        /// <summary>
        /// Constant of the value if the joystick is down ot left /!\ It's can change with the orientation of the josytick /!\
        /// </summary>
        private const double JOYSTICK_DOWN_LEFT = 0.6;

        /// <summary>
        /// This is the logic version of the main menu
        /// </summary>
        private int _menu = 0;

        private MENU_STATE _menuState = MENU_STATE.initial;
        private SERVO_STATE _servoState = SERVO_STATE.close;
        private SECRET_CODE _secretState = SECRET_CODE.up1;
        private SCAN_CARD_STATE _scanCardState = SCAN_CARD_STATE.waitRFID;
        private ADD_CARD_STATE _addCardState = ADD_CARD_STATE.waitRFID;
        private DISPLAY_CARDS_STATE _displayCardsState = DISPLAY_CARDS_STATE.listIsEmpty;
        private DELETE_CARD_STATE _deleteCardState = DELETE_CARD_STATE.listIsEmpty;

        /// <summary>
        /// 90000ms = 1min30 -> this is the secure timer if we have forgotten to close the box
        /// </summary>
        private GT.Timer _secuTimer = new GT.Timer(90000);

        /// <summary>
        /// The axe X of the joystick
        /// </summary>
        private AnalogInput _joystickX = new AnalogInput(FEZSpider.Socket9.AnalogInput4);

        /// <summary>
        /// The axe Y of the joystick
        /// </summary>
        private AnalogInput _joystickY = new AnalogInput(FEZSpider.Socket9.AnalogInput5);

        /// <summary>
        /// The joystick button
        /// </summary>
        private InterruptPort _joystickButton = new InterruptPort(FEZSpider.Socket9.Pin3, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);

        /// <summary>
        /// This method is run when the mainboard is powered up or reset.
        /// </summary>
        void ProgramStarted()
        {
            InitializesClassesData();

            GT.Timer timer = new GT.Timer(100);// Perdiod = every 0.1 seconds (100ms)
            timer.Tick += timer_Tick;          // We do a timer because with Gageteer (GHI) while(true) blocks the thread
            timer.Start();                     // and the acces to the components

            _joystickButton.OnInterrupt += _joystickButton_OnInterrupt;

            _secuTimer.Tick += _secuTimer_Tick;
        }

        private void _secuTimer_Tick(GT.Timer pbTimer)
        {
            _servoState = SERVO_STATE.close;
        }

        /// <summary>
        /// This timer event serves as the main sequencer of the program
        /// </summary>
        /// <param name="pbTimer">The GT.Timer who's calling (GT = Gadgeteer)</param>
        private void timer_Tick(GT.Timer pbTimer)
        {
            // Main menu
            if (_menuState == MENU_STATE.initial)
            {
                InitialState();
            }

            // Add card event
            if (_menuState == MENU_STATE.addCard)
            {
                AddCard();
            }

            // Delete card event
            if (_menuState == MENU_STATE.deleteCard)
            {
                DeleteCard();
            }

            // Display cards event
            if (_menuState == MENU_STATE.displayCards)
            {
                DisplayCards();
            }

            // Unlock with the secret code event
            if (_menuState == MENU_STATE.secretCode)
            {
                UnlockSecretCode();
            }
        }

        /// <summary>
        /// This method initializes the different classes and data
        /// </summary>
        private void InitializesClassesData()
        {
            ServoMotor.GetInstance().Lock();
            RFIDReader.GetInstance();
            DisplayLoad();
            ListOfCards.GetInstance().CardsList = SDCard.GetInstance().LoadCards();
            if (ListOfCards.GetInstance().CardsList == null)
            {
                ListOfCards.GetInstance().CardsList = new ArrayList();
            }
            RestoreInitialState();
        }

        /// <summary>
        /// This method interrupt all threads when the joystick is press
        /// </summary>
        /// <param name="pbData1"></param>
        /// <param name="pbData2"></param>
        /// <param name="pbTime"></param>
        private void _joystickButton_OnInterrupt(uint pbData1, uint pbData2, DateTime pbTime)
        {
            // We do this only if we are on the main menu
            if (_menuState == MENU_STATE.initial)
            {
                switch (_menu)
                {
                    case 0:
                        _menuState = MENU_STATE.initial;
                        break;
                    case 1:
                        _menuState = MENU_STATE.addCard;
                        break;
                    case 2:
                        _menuState = MENU_STATE.deleteCard;
                        break;
                    case 3:
                        _menuState = MENU_STATE.displayCards;
                        break;
                    case 4:
                        _menuState = MENU_STATE.secretCode;
                        break;
                    default:
                        _menuState = MENU_STATE.initial;
                        break;
                }
                DeleteCurrentBadgescan();
                LCD.GetInstance().Clear();
            }
        }

        /// <summary>
        /// This method clear the current badge that is scan by the RFIDReader
        /// </summary>
        private void DeleteCurrentBadgescan()
        {
            RFIDReader.GetInstance().CurrentUid = "";
            RFIDReader.GetInstance().IsBadgeScan = false;
        }

        /// <summary>
        /// This method restore to initial state (main menu)
        /// </summary>
        private void RestoreInitialState()
        {
            _menu = 0;
            _menuState = MENU_STATE.initial;
            _servoState = SERVO_STATE.close;
            _secretState = SECRET_CODE.up1;
            _scanCardState = SCAN_CARD_STATE.waitRFID;
            _addCardState = ADD_CARD_STATE.waitRFID;
            _displayCardsState = DISPLAY_CARDS_STATE.listIsEmpty;
            _deleteCardState = DELETE_CARD_STATE.listIsEmpty;

            // Refresh the LCD text fields
            LCDTextFields.Content = Card.DEFAULT_NAME;
            LCDTextFields.CursorPosition = 0;
            LCDTextFields.ShouldBeRefresh = true;

            DeleteCurrentBadgescan();
            LCD.GetInstance().Clear();
            DisplayMainMenu(_menu);
        }

        /// <summary>
        /// This method display an error message on the lcd after clearing it
        /// </summary>
        private void DisplayError()
        {
            LCD.GetInstance().Clear();
            LCD.GetInstance().DisplayText(GT.Color.Red, "/!\\ Une erreur est survenue /!\\", 10, LCD.GetInstance().LcdHeight - 20);
        }

        /// <summary>
        /// This method display an save message on the lcd after clearing it
        /// </summary>
        private void DisplaySave()
        {
            LCD.GetInstance().Clear();
            LCD.GetInstance().DisplayText(GT.Color.Gray, "Sauvegarde en cours...", 10, LCD.GetInstance().LcdHeight - 20);
        }

        /// <summary>
        /// This method display an load message on the lcd after clearing it
        /// </summary>
        private void DisplayLoad()
        {
            LCD.GetInstance().Clear();
            LCD.GetInstance().DisplayText(GT.Color.Gray, "Chargement en cours...", 10, LCD.GetInstance().LcdHeight - 20);
        }

        /// <summary>
        /// This method display the main menu on the LCD
        /// </summary>
        /// <param name="pbMenu">The logic version of the main menu (Index)</param>
        private void DisplayMainMenu(int pbMenu)
        {
            LCD.GetInstance().DisplayText(Gadgeteer.Color.Black, "Ajouter un badge", 10, MENU1_Y);
            LCD.GetInstance().DisplayText(Gadgeteer.Color.Black, "Supprimer un badge", 10, MENU2_Y);
            LCD.GetInstance().DisplayText(Gadgeteer.Color.Black, "Afficher la liste des badges", 10, MENU3_Y);
            LCD.GetInstance().DisplayText(Gadgeteer.Color.Black, "Deverouiller avec le code secret", 10, MENU4_Y);
            switch (pbMenu)
            {
                case 1:
                    LCD.GetInstance().DisplayText(Gadgeteer.Color.Blue, "Ajouter un badge", 10, MENU1_Y);
                    break;
                case 2:
                    LCD.GetInstance().DisplayText(Gadgeteer.Color.Blue, "Supprimer un badge", 10, MENU2_Y);
                    break;
                case 3:
                    LCD.GetInstance().DisplayText(Gadgeteer.Color.Blue, "Afficher la liste des badges", 10, MENU3_Y);
                    break;
                case 4:
                    LCD.GetInstance().DisplayText(Gadgeteer.Color.Blue, "Deverouiller avec le code secret", 10, MENU4_Y);
                    break;
            }
        }

        /// <summary>
        /// When no menu is selected
        /// </summary>
        private void InitialState()
        {
            switch (_scanCardState)
            {
                case SCAN_CARD_STATE.waitRFID:
                    if (RFIDReader.GetInstance().IsBadgeScan)
                    {
                        _scanCardState = SCAN_CARD_STATE.RFIDDetected;
                    }
                    break;
                case SCAN_CARD_STATE.RFIDDetected:
                    if (_servoState == SERVO_STATE.close) // If the servo is lock
                    {
                        bool isValid = ListOfCards.GetInstance().FindCardInlist(RFIDReader.GetInstance().CurrentUid);
                        if (isValid)
                        {
                            _scanCardState = SCAN_CARD_STATE.RFIDValid;
                        }
                        else
                        {
                            _scanCardState = SCAN_CARD_STATE.RFIDInvalid;
                        }
                    }
                    else
                    {
                        _scanCardState = SCAN_CARD_STATE.RFIDInvalid;
                    }
                    break;
                case SCAN_CARD_STATE.RFIDValid:
                    _servoState = SERVO_STATE.open;
                    _scanCardState = SCAN_CARD_STATE.waitRFID;
                    DeleteCurrentBadgescan();
                    break;
                case SCAN_CARD_STATE.RFIDInvalid:
                    _servoState = SERVO_STATE.close;
                    _scanCardState = SCAN_CARD_STATE.waitRFID;
                    DeleteCurrentBadgescan();
                    break;
                default:
                    _scanCardState = SCAN_CARD_STATE.waitRFID;
                    break;
            }

            switch (_servoState)
            {
                case SERVO_STATE.open:
                    ServoMotor.GetInstance().Unlock();
                    _secuTimer.Start();
                    break;
                case SERVO_STATE.close:
                    ServoMotor.GetInstance().Lock();
                    _secuTimer.Stop();
                    break;
                default:
                    _servoState = SERVO_STATE.close;
                    break;
            }

            if (_joystickX.Read() > JOYSTICK_DOWN_LEFT) // If the joystick is down
            {
                _menu++;
                if (_menu > 4)
                {
                    _menu = 0;
                }
                DisplayMainMenu(_menu); // It is not removed from the test to prevent the lcd flashing
                Thread.Sleep(100); // Wait 0.1 second to prevent the menu from scrolling too fast
            }
            else if (_joystickX.Read() < JOYSTICK_UP_RIGHT) // The joystick is up
            {
                _menu--;
                if (_menu < 0)
                {
                    _menu = 4;
                }
                DisplayMainMenu(_menu); // It is not removed from the test to prevent the lcd flashing
                Thread.Sleep(100); // Wait 0.1 second to prevent the menu from scrolling too fast
            }
        }

        /// <summary>
        /// When the menu to add a card is selected
        /// </summary>
        private void AddCard()
        {
            switch (_addCardState)
            {
                case ADD_CARD_STATE.waitRFID:
                    LCD.GetInstance().Clear();
                    LCD.GetInstance().DisplayText(GT.Color.Gray, "Veuillez approcher un badge du lecteur");
                    if (RFIDReader.GetInstance().IsBadgeScan)
                    {
                        _addCardState = ADD_CARD_STATE.RFIDDetected;
                        LCD.GetInstance().DisplayText(GT.Color.Green, "Votre badge a ete correctement scanne", 10, LCD.GetInstance().LcdHeight / 2);
                        Thread.Sleep(2000); // Wait 2 seconds to see the message
                    }
                    break;
                case ADD_CARD_STATE.RFIDDetected:
                    LCD.GetInstance().Clear();
                    string uid = RFIDReader.GetInstance().CurrentUid;

                    if (ListOfCards.GetInstance().FindCardInlist(uid)) // If the badge scanned already exist
                    {
                        _addCardState = ADD_CARD_STATE.badgeExist;
                    }
                    else
                    {
                        _addCardState = ADD_CARD_STATE.bageDontExist;
                    }
                    break;
                case ADD_CARD_STATE.badgeExist:
                    LCD.GetInstance().DisplayText(GT.Color.Red, "/!\\ Erreur : Ce badge est deja sauvegarde /!\\", 10, LCD.GetInstance().LcdHeight / 2);
                    Thread.Sleep(2000); // Wait 2 seconds to see the message
                    RestoreInitialState();
                    break;
                case ADD_CARD_STATE.bageDontExist:
                    string name = LCDTextFields.Content; // The content value of the LCD field, it's the name of the badge
                    char[] charArray = name.ToCharArray(); // We split the name in a char array to make it easier to modify char by char
                    int x = 110; // The position index where we're gonna write the first char

                    if (LCDTextFields.ShouldBeRefresh) // If we need to refresh because we have modify a char or the position of the cursor
                    {
                        LCD.GetInstance().Clear();
                        LCD.GetInstance().DisplayText(GT.Color.Gray, "Votre badge :", 10, LCD.GetInstance().LcdHeight / 2);
                        LCD.GetInstance().DisplayText(GT.Color.Gray, "Pour valider le nom, appuyer sur le joystick", 10, LCD.GetInstance().LcdHeight - 20);
                        for (int i = 0; i < charArray.Length; i++)
                        {
                            if (i == LCDTextFields.CursorPosition) // If the cursorposition is at this char
                                LCD.GetInstance().DisplayText(GT.Color.Blue, charArray[i].ToString(), x, LCD.GetInstance().LcdHeight / 2);
                            else
                                LCD.GetInstance().DisplayText(GT.Color.Black, charArray[i].ToString(), x, LCD.GetInstance().LcdHeight / 2);
                            x += 10; // Increment the X position on the LCD
                        }
                        LCDTextFields.ShouldBeRefresh = false;
                    }

                    if (_joystickX.Read() < JOYSTICK_UP_RIGHT) // If the joystick is up
                    {
                        charArray[LCDTextFields.CursorPosition]++; // Increment the char ex : A -> B
                        LCDTextFields.ShouldBeRefresh = true;
                        Thread.Sleep(100); // Wait 0.1 second to prevent the letter from scrolling too fast
                    }
                    else if (_joystickX.Read() > JOYSTICK_DOWN_LEFT) // If the joystick is down
                    {
                        charArray[LCDTextFields.CursorPosition]--; // ecrement the char ex : B -> A
                        LCDTextFields.ShouldBeRefresh = true;
                        Thread.Sleep(100); // Wait 0.1 second to prevent the letter from scrolling too fast
                    }


                    if (_joystickY.Read() < JOYSTICK_UP_RIGHT) // If the joystick is right
                    {
                        LCDTextFields.CursorPosition++; // Move the cursor to the next char
                        if (LCDTextFields.CursorPosition > charArray.Length - 1) // If the cursor get out of the range of the char array
                        {
                            LCDTextFields.CursorPosition = 0; // Move to the first position of the char array
                        }
                        LCDTextFields.ShouldBeRefresh = true;
                        Thread.Sleep(200); // Wait 0.2 seconds to prevent the cursor from moving too fast
                    }
                    else if (_joystickY.Read() > JOYSTICK_DOWN_LEFT) // If the joystick is left
                    {
                        LCDTextFields.CursorPosition--; // Move the cursor to the previous char
                        if (LCDTextFields.CursorPosition < 0) // If the cursor get out of the range of the char array
                        {
                            LCDTextFields.CursorPosition = charArray.Length - 1; // Move to the last position of the char array
                        }
                        LCDTextFields.ShouldBeRefresh = true;
                        Thread.Sleep(200); // Wait 0.2 seconds to prevent the cursor from moving too fast
                    }

                    LCDTextFields.Content = new string(charArray); // Set the LCD text field with the value of the modify char array

                    if (!_joystickButton.Read()) // If joystick button is press
                    {
                        _addCardState = ADD_CARD_STATE.save;
                    }
                    break;
                case ADD_CARD_STATE.save:
                    try
                    {
                        uid = RFIDReader.GetInstance().CurrentUid; // Get the uid of the badge that was scanned
                        name = LCDTextFields.Content;
                        ListOfCards.GetInstance().AddCardToList(name, uid);
                        SDCard.GetInstance().SaveCards(ListOfCards.GetInstance().CardsList);
                        _addCardState = ADD_CARD_STATE.successMSG;
                    }
                    catch (Exception e)
                    {
                        _addCardState = ADD_CARD_STATE.errorMSG;
                    }
                    break;
                case ADD_CARD_STATE.errorMSG:
                    DisplayError();
                    Thread.Sleep(2000);
                    RestoreInitialState();
                    break;
                case ADD_CARD_STATE.successMSG:
                    DisplaySave();
                    LCD.GetInstance().DisplayText(GT.Color.Green, "Le badge a bien ete ajoute", 10, LCD.GetInstance().LcdHeight / 2);
                    Thread.Sleep(2000);
                    RestoreInitialState();
                    break;
                default:
                    _addCardState = ADD_CARD_STATE.waitRFID;
                    break;
            }
        }

        /// <summary>
        /// When the menu to delete a card is selected
        /// </summary>
        private void DeleteCard()
        {

            switch (_deleteCardState)
            {
                case DELETE_CARD_STATE.listIsEmpty:
                    LCD.GetInstance().Clear();
                    if (ListOfCards.GetInstance().IsEmpty())
                    {
                        LCD.GetInstance().DisplayText(GT.Color.Red, "/!\\ Aucun badge n'est enregistre /!\\", 10, LCD.GetInstance().LcdHeight / 2);
                        Thread.Sleep(2000);
                        RestoreInitialState();
                    }
                    else
                    {
                        _deleteCardState = DELETE_CARD_STATE.selectCard;
                    }
                    break;
                case DELETE_CARD_STATE.selectCard:
                    int positionY = 10; // The Y position on the LCD
                    int i = 0;
                    foreach (Card card in ListOfCards.GetInstance().CardsList)
                    {
                        if (LCDTextFields.CursorPosition == i) // If the cursorposition is at this char
                            LCD.GetInstance().DisplayText(GT.Color.Blue, card.Name, 10, positionY);
                        else
                            LCD.GetInstance().DisplayText(GT.Color.Black, card.Name, 10, positionY);
                        positionY += 15; // Increment the Y position
                        i++;
                    }
                    LCD.GetInstance().DisplayText(GT.Color.Gray, "Pour selectionner le badge, appuyer sur le joystick", 10, LCD.GetInstance().LcdHeight - 30);

                    if (_joystickX.Read() > JOYSTICK_DOWN_LEFT) // If joystick is down
                    {
                        LCDTextFields.CursorPosition++; // Move the cursor to the next name
                        if (LCDTextFields.CursorPosition > ListOfCards.GetInstance().CardsList.Count - 1) // If the cursor get out of the range of the list of cards array
                        {
                            LCDTextFields.CursorPosition = 0; // Move to the first position of the list of cards array
                        }
                        Thread.Sleep(100); // Wait 0.1 second to prevent the cursor from moving too fast
                    }
                    else if (_joystickX.Read() < JOYSTICK_UP_RIGHT) // If joystick is up
                    {
                        LCDTextFields.CursorPosition--; // Move the cursor to the previous name
                        if (LCDTextFields.CursorPosition < 0) // If the cursor get out of the range of the list array
                        {
                            LCDTextFields.CursorPosition = ListOfCards.GetInstance().CardsList.Count - 1; // Move to the last position of the list of cards array
                        }
                        Thread.Sleep(100); // Wait 0.1 second to prevent the cursor from moving too fast
                    }
                    if (!_joystickButton.Read()) // If joystick button is press
                    {
                        _deleteCardState = DELETE_CARD_STATE.save;
                    }
                    break;
                case DELETE_CARD_STATE.save:
                    try
                    {
                        ListOfCards.GetInstance().DeleteCardFromList(LCDTextFields.CursorPosition);
                        SDCard.GetInstance().SaveCards(ListOfCards.GetInstance().CardsList);
                        _deleteCardState = DELETE_CARD_STATE.success;
                    }
                    catch (Exception e)
                    {
                        _deleteCardState = DELETE_CARD_STATE.errorMSG;
                    }
                    break;
                case DELETE_CARD_STATE.errorMSG:
                    DisplayError();
                    Thread.Sleep(2000);
                    RestoreInitialState();
                    break;
                case DELETE_CARD_STATE.success:
                    DisplaySave();
                    LCD.GetInstance().DisplayText(GT.Color.Green, "Le badge a bien ete supprime", 10, LCD.GetInstance().LcdHeight / 2);
                    Thread.Sleep(2000);
                    RestoreInitialState();
                    break;
                default:
                    _deleteCardState = DELETE_CARD_STATE.listIsEmpty;
                    break;
            }
        }

        /// <summary>
        /// When the menu to display all cards is selected
        /// </summary>
        private void DisplayCards()
        {
            switch (_displayCardsState)
            {
                case DISPLAY_CARDS_STATE.listIsEmpty:
                    LCD.GetInstance().Clear();
                    if (ListOfCards.GetInstance().IsEmpty())
                    {
                        _displayCardsState = DISPLAY_CARDS_STATE.errorMSG;
                    }
                    else
                    {
                        _displayCardsState = DISPLAY_CARDS_STATE.displayAllCards;
                    }
                    break;
                case DISPLAY_CARDS_STATE.errorMSG:
                    LCD.GetInstance().DisplayText(GT.Color.Red, "/!\\ Aucun badge n'est enregistre /!\\", 10, LCD.GetInstance().LcdHeight / 2);
                    Thread.Sleep(2000);
                    RestoreInitialState();
                    break;
                case DISPLAY_CARDS_STATE.displayAllCards:
                    int positionY = 10; // The Y position on the LCD
                    LCD.GetInstance().DisplayText(GT.Color.Gray, "Pour quitter, appuyer sur le joystick", 10, LCD.GetInstance().LcdHeight - 20);

                    foreach (Card card in ListOfCards.GetInstance().CardsList)
                    {
                        LCD.GetInstance().DisplayText(GT.Color.Black, card.Name, 10, positionY);
                        positionY += 15; // Increment the Y position
                    }

                    if (!_joystickButton.Read()) // If joystick button is press
                    {
                        RestoreInitialState();
                    }
                    break;
                default:
                    _displayCardsState = DISPLAY_CARDS_STATE.listIsEmpty;
                    break;
            }
        }

        /// <summary>
        /// When the menu to unlock the box with the secret code is selected
        /// </summary>
        private void UnlockSecretCode()
        {
            int nbrClue = 0; // This is for the number of * that we're going to wrote on the LCD
            int positionX = 10; // Position X ont the LCD

            LCD.GetInstance().DisplayText(GT.Color.Black, "Progression :", positionX, LCD.GetInstance().LcdHeight / 2);

            LCD.GetInstance().DisplayText(GT.Color.Gray, "Pour quitter, appuyer sur le joystick", positionX, LCD.GetInstance().LcdHeight - 20);
            bool oldJoystickread = ((_joystickX.Read() > JOYSTICK_UP_RIGHT && _joystickX.Read() < JOYSTICK_DOWN_LEFT) && // True if the joystick is int the center 
                                    (_joystickY.Read() > JOYSTICK_UP_RIGHT && _joystickY.Read() < JOYSTICK_DOWN_LEFT));

            Thread.Sleep(200); // Wait 0.2 seconds to allow time to move the joystick
            bool joystickRead = ((_joystickX.Read() < JOYSTICK_UP_RIGHT || _joystickX.Read() > JOYSTICK_DOWN_LEFT) || // True if the joystick isn't at the center
                                 (_joystickY.Read() < JOYSTICK_UP_RIGHT || _joystickY.Read() > JOYSTICK_DOWN_LEFT));

            if ((oldJoystickread && joystickRead) || _secretState == SECRET_CODE.success || _secretState == SECRET_CODE.error) // If the joystick was in the center and then move
            {                                                                                                                  // or if the code is success || error
                LCD.GetInstance().Clear();
                switch (_secretState)
                {
                    case SECRET_CODE.up1:
                        if (_joystickX.Read() <= JOYSTICK_UP_RIGHT) // If joystick is up
                        {
                            _secretState = SECRET_CODE.up2;
                            nbrClue = 1;
                            Debug.Print("1");
                        }
                        else
                        {
                            _secretState = SECRET_CODE.error;
                        }
                        break;
                    case SECRET_CODE.up2:
                        if (_joystickX.Read() <= JOYSTICK_UP_RIGHT) // If joystick is up
                        {
                            _secretState = SECRET_CODE.down1;
                            nbrClue = 2;
                            Debug.Print("2");
                        }
                        else
                        {
                            _secretState = SECRET_CODE.error;
                        }
                        break;
                    case SECRET_CODE.down1:
                        if (_joystickX.Read() >= JOYSTICK_DOWN_LEFT) // If joystick is down
                        {
                            _secretState = SECRET_CODE.down2;
                            nbrClue = 3;
                            Debug.Print("3");
                        }
                        else
                        {
                            _secretState = SECRET_CODE.error;
                        }
                        break;
                    case SECRET_CODE.down2:
                        if (_joystickX.Read() >= JOYSTICK_DOWN_LEFT) // If joystick is down
                        {
                            _secretState = SECRET_CODE.left1;
                            nbrClue = 4;
                            Debug.Print("4");
                        }
                        else
                        {
                            _secretState = SECRET_CODE.error;
                        }
                        break;
                    case SECRET_CODE.left1:
                        if (_joystickY.Read() >= JOYSTICK_DOWN_LEFT) // If joystick is left
                        {
                            _secretState = SECRET_CODE.right1;
                            nbrClue = 5;
                            Debug.Print("5");
                        }
                        else
                        {
                            _secretState = SECRET_CODE.error;
                        }
                        break;
                    case SECRET_CODE.right1:
                        if (_joystickY.Read() <= JOYSTICK_UP_RIGHT) // If joystick is right
                        {
                            _secretState = SECRET_CODE.left2;
                            nbrClue = 6;
                            Debug.Print("6");
                        }
                        else
                        {
                            _secretState = SECRET_CODE.error;
                            nbrClue = 1;
                        }
                        break;
                    case SECRET_CODE.left2:
                        if (_joystickY.Read() >= JOYSTICK_DOWN_LEFT) // If joystick is left
                        {
                            _secretState = SECRET_CODE.right2;
                            nbrClue = 7;
                            Debug.Print("7");
                        }
                        else
                        {
                            _secretState = SECRET_CODE.error;
                        }
                        break;
                    case SECRET_CODE.right2:
                        if (_joystickY.Read() <= JOYSTICK_UP_RIGHT) // If joystick is right
                        {
                            _secretState = SECRET_CODE.success;
                            Debug.Print("8");
                        }
                        else
                        {
                            _secretState = SECRET_CODE.error;
                        }
                        break;
                    case SECRET_CODE.success:
                        RestoreInitialState();
                        _servoState = SERVO_STATE.open;
                        break;
                    case SECRET_CODE.error:
                        _secretState = SECRET_CODE.up1;
                        LCD.GetInstance().DisplayText(GT.Color.Red, "Code faux, veuillez recommencer");
                        Thread.Sleep(1000);
                        break;
                    default:
                        _secretState = SECRET_CODE.up1;
                        break;
                }

                for (int i = 0; i < nbrClue; i++)
                {
                    LCD.GetInstance().DisplayText(GT.Color.Black, "*", 100 + positionX, LCD.GetInstance().LcdHeight / 2 + 5);
                    positionX += 10;
                }
            }

            if (!_joystickButton.Read()) // If the joystick button is press
            {
                RestoreInitialState();
            }
        }
    }
}
