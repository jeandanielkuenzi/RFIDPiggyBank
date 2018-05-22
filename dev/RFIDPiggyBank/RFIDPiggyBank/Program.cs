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
        /// The state of the servomotor
        /// </summary>
        private enum SERVO_STATE { open, close };

        /// <summary>
        /// The secret sequel to the code (it looks like konami code)
        /// </summary>
        private enum SECRET_CODE { up1, up2, down1, down2, left1, right1, left2, right2, success, error };

        private enum SUB_STATE { waitRFID, RFIDDetected, RFIDValid, RFIDInvalid };

        private enum ADD_CARD_STATE { waitRFID, RFIDDetected, badgeExist, bageDontExist, save, errorMSG, successMSG };

        private enum DISPLAY_CARDS_SATE { listIsEmpty, errorMSG, displayAllCards };

        private enum DELETE_CARD_STATE { listIsEmpty, selectCard, save, errorMSG, success };

        /// <summary>
        /// Constant for the menu position on the LCD
        /// </summary>
        private const int MENU1_Y = 10;
        private const int MENU2_Y = 30;
        private const int MENU3_Y = 50;
        private const int MENU4_Y = 70;

        /// <summary>
        /// Constant of the value when the joystick is up or right
        /// </summary>
        private const double JOYSTICK_UP_RIGHT = 0.4;

        /// <summary>
        /// Constant of the value if the joystick is down ot left
        /// </summary>
        private const double JOYSTICK_DOWN_LEFT = 0.6;

        /// <summary>
        /// This is the logic version of the main menu
        /// </summary>
        private int _menu = 0;

        private MENU_STATE _menuState = MENU_STATE.initial;
        private SERVO_STATE _servoState = SERVO_STATE.close;
        private SECRET_CODE _secretState = SECRET_CODE.up1;
        private SUB_STATE _subState = SUB_STATE.waitRFID;
        private ADD_CARD_STATE _addCardState = ADD_CARD_STATE.waitRFID;
        private DISPLAY_CARDS_SATE _displayCardsState = DISPLAY_CARDS_SATE.listIsEmpty;
        private DELETE_CARD_STATE _deleteCardState = DELETE_CARD_STATE.listIsEmpty;

        /// <summary>
        /// 90000ms = 1min30 -> this is the secure timer if we have forgotten to close the box
        /// </summary>
        private GT.Timer _secuTimer = new GT.Timer(90000);

        private AnalogInput _joystickX = new AnalogInput(FEZSpider.Socket9.AnalogInput4); // Le potentiomètre du joystick sur l'axe X
        private AnalogInput _joystickY = new AnalogInput(FEZSpider.Socket9.AnalogInput5); // Le potentiomètre du joystick sur l'axe Y

        private InterruptPort _joystickButton = new InterruptPort(FEZSpider.Socket9.Pin3, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);

        private bool _joystickRead = false;
        private bool _oldJoystickRead = false;

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
            ServoMotor.GetInstance().Lock();
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
            _subState = SUB_STATE.waitRFID;
            _addCardState = ADD_CARD_STATE.waitRFID;
            _displayCardsState = DISPLAY_CARDS_SATE.listIsEmpty;
            _deleteCardState = DELETE_CARD_STATE.listIsEmpty;

            LCDTextField.Content = Card.DEFAULT_NAME;
            LCDTextField.CursorPosition = 0;
            LCDTextField.ShouldBeRefresh = true;

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
            LCD.GetInstance().DisplayText(GT.Color.LightGray, "Sauvegarde en cours...", 10, LCD.GetInstance().LcdHeight - 20);
        }

        /// <summary>
        /// This method display an load message on the lcd after clearing it
        /// </summary>
        private void DisplayLoad()
        {
            LCD.GetInstance().Clear();
            LCD.GetInstance().DisplayText(GT.Color.LightGray, "Chargement en cours...", 10, LCD.GetInstance().LcdHeight - 20);
        }

        /// <summary>
        /// This method display the main menu on the LCD
        /// </summary>
        /// <param name="pbMenu"></param>
        private void DisplayMainMenu(int pbMenu)
        {
            LCD.GetInstance().DisplayText(Gadgeteer.Color.Gray, "Ajouter un badge", 10, MENU1_Y);
            LCD.GetInstance().DisplayText(Gadgeteer.Color.Gray, "Supprimer un badge", 10, MENU2_Y);
            LCD.GetInstance().DisplayText(Gadgeteer.Color.Gray, "Afficher la liste des badges", 10, MENU3_Y);
            LCD.GetInstance().DisplayText(Gadgeteer.Color.Gray, "Deverouiller avec le mot de passe", 10, MENU4_Y);
            switch (pbMenu)
            {
                case 1:
                    LCD.GetInstance().DisplayText(Gadgeteer.Color.Black, "Ajouter un badge", 10, MENU1_Y);
                    break;
                case 2:
                    LCD.GetInstance().DisplayText(Gadgeteer.Color.Black, "Supprimer un badge", 10, MENU2_Y);
                    break;
                case 3:
                    LCD.GetInstance().DisplayText(Gadgeteer.Color.Black, "Afficher la liste des badges", 10, MENU3_Y);
                    break;
                case 4:
                    LCD.GetInstance().DisplayText(Gadgeteer.Color.Black, "Deverouiller avec le mot de passe", 10, MENU4_Y);
                    break;
            }
        }

        /// <summary>
        /// When no menu is selected
        /// </summary>
        private void InitialState()
        {
            switch (_subState)
            {
                case SUB_STATE.waitRFID:
                    if (RFIDReader.GetInstance().IsBadgeScan)
                    {
                        _subState = SUB_STATE.RFIDDetected;
                    }
                    break;
                case SUB_STATE.RFIDDetected:
                    if (_servoState == SERVO_STATE.close)
                    {
                        bool isValid = ListOfCards.GetInstance().FindCardInlist(RFIDReader.GetInstance().CurrentUid);
                        if (isValid)
                        {
                            _subState = SUB_STATE.RFIDValid;
                        }
                        else
                        {
                            _subState = SUB_STATE.RFIDInvalid;
                        }
                    }
                    else
                    {
                        _subState = SUB_STATE.RFIDInvalid;
                    }
                    break;
                case SUB_STATE.RFIDValid:
                    ServoMotor.GetInstance().Unlock();
                    _subState = SUB_STATE.waitRFID;
                    _servoState = SERVO_STATE.open;
                    _secuTimer.Start();
                    DeleteCurrentBadgescan();
                    break;
                case SUB_STATE.RFIDInvalid:
                    ServoMotor.GetInstance().Lock();
                    _subState = SUB_STATE.waitRFID;
                    _servoState = SERVO_STATE.close;
                    _secuTimer.Stop();
                    DeleteCurrentBadgescan();
                    break;
                default:
                    _subState = SUB_STATE.waitRFID;
                    break;
            }

            if (_joystickX.Read() > JOYSTICK_DOWN_LEFT)
            {
                _menu++;
                if (_menu > 4)
                {
                    _menu = 0;
                }
                DisplayMainMenu(_menu); // On ne le sort pas du test (if) pour éviter que le lcd clignote
                Thread.Sleep(100);
            }
            else if (_joystickX.Read() < JOYSTICK_UP_RIGHT)
            {
                _menu--;
                if (_menu < 0)
                {
                    _menu = 4;
                }
                DisplayMainMenu(_menu); // On ne le sort pas du test (if) pour éviter que le lcd clignote
                Thread.Sleep(100);
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
                    LCD.GetInstance().DisplayText(GT.Color.LightGray, "Veuillez approcher un badge du lecteur");
                    if (RFIDReader.GetInstance().IsBadgeScan)
                    {
                        _addCardState = ADD_CARD_STATE.RFIDDetected;
                        LCD.GetInstance().DisplayText(GT.Color.Green, "Votre badge a ete correctement scanne", 10, LCD.GetInstance().LcdHeight / 2);
                        Thread.Sleep(2000); // On attends 2 seconde
                    }
                    break;
                case ADD_CARD_STATE.RFIDDetected:
                    LCD.GetInstance().Clear();
                    string uid = RFIDReader.GetInstance().CurrentUid;

                    if (ListOfCards.GetInstance().FindCardInlist(uid)) // Si le badge scanné existe déjà
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
                    Thread.Sleep(2000); // On attends 2 secondes
                    RestoreInitialState();
                    break;
                case ADD_CARD_STATE.bageDontExist:
                    string name = LCDTextField.Content;
                    char[] charArray = name.ToCharArray();
                    int x = 110;
                    if (LCDTextField.ShouldBeRefresh)
                    {
                        LCD.GetInstance().Clear();
                        LCD.GetInstance().DisplayText(GT.Color.Red, "Votre badge :", 10, LCD.GetInstance().LcdHeight / 2);
                        LCD.GetInstance().DisplayText(GT.Color.LightGray, "Pour valider le nom, appuyer sur le joystick", 10, LCD.GetInstance().LcdHeight - 20);
                        for (int i = 0; i < charArray.Length; i++)
                        {
                            if (i == LCDTextField.CursorPosition)
                                LCD.GetInstance().DisplayText(GT.Color.Black, charArray[i].ToString(), x, LCD.GetInstance().LcdHeight / 2);
                            else
                                LCD.GetInstance().DisplayText(GT.Color.Gray, charArray[i].ToString(), x, LCD.GetInstance().LcdHeight / 2);
                            x += 10;
                        }
                        LCDTextField.ShouldBeRefresh = false;
                    }

                    if (_joystickX.Read() < JOYSTICK_UP_RIGHT)
                    {
                        charArray[LCDTextField.CursorPosition]++;
                        LCDTextField.ShouldBeRefresh = true;
                        Thread.Sleep(100); // On attends 0.1 seconde pour que les lettres ne défilent pas trop vite
                    }
                    else if (_joystickX.Read() > JOYSTICK_DOWN_LEFT)
                    {
                        charArray[LCDTextField.CursorPosition]--;
                        LCDTextField.ShouldBeRefresh = true;
                        Thread.Sleep(100); // On attends 0.1 seconde pour que les lettres ne défilent pas trop vite
                    }


                    if (_joystickY.Read() < JOYSTICK_UP_RIGHT)
                    {
                        LCDTextField.CursorPosition++;
                        if (LCDTextField.CursorPosition > name.Length - 1)
                        {
                            LCDTextField.CursorPosition = 0;
                        }
                        LCDTextField.ShouldBeRefresh = true;
                        Thread.Sleep(200); // On attends 0.2 seconde pour que le curseur ne défile pas trop vite
                    }
                    else if (_joystickY.Read() > JOYSTICK_DOWN_LEFT)
                    {
                        LCDTextField.CursorPosition--;
                        if (LCDTextField.CursorPosition < 0)
                        {
                            LCDTextField.CursorPosition = name.Length - 1;
                        }
                        LCDTextField.ShouldBeRefresh = true;
                        Thread.Sleep(200); // On attends 0.2 seconde pour que le curseur ne défile pas trop vite
                    }

                    LCDTextField.Content = new string(charArray);

                    if (!_joystickButton.Read())
                    {
                        _addCardState = ADD_CARD_STATE.save;
                    }
                    break;
                case ADD_CARD_STATE.save:
                    try
                    {
                        uid = RFIDReader.GetInstance().CurrentUid;
                        name = LCDTextField.Content; // On remplace la variable name par les lettres du tableau
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
                    int positionY = 10;
                    int i = 0;
                        positionY = 10;
                        foreach (Card card in ListOfCards.GetInstance().CardsList)
                        {
                            if (LCDTextField.CursorPosition == i)
                                LCD.GetInstance().DisplayText(GT.Color.Black, card.Name, 10, positionY);
                            else
                                LCD.GetInstance().DisplayText(GT.Color.Gray, card.Name, 10, positionY);
                            positionY += 15;
                            i++;
                        }
                        LCD.GetInstance().DisplayText(GT.Color.LightGray, "Pour selectionner le badge, appuyer sur le joystick", 10, LCD.GetInstance().LcdHeight - 30);
                        if (_joystickX.Read() > JOYSTICK_DOWN_LEFT)
                        {
                            LCDTextField.CursorPosition++;
                            if (LCDTextField.CursorPosition > ListOfCards.GetInstance().CardsList.Count - 1)
                            {
                                LCDTextField.CursorPosition = 0;
                            }
                            Thread.Sleep(100);
                        }
                        else if (_joystickX.Read() < JOYSTICK_UP_RIGHT)
                        {
                            LCDTextField.CursorPosition--;
                            if (LCDTextField.CursorPosition < 0)
                            {
                                LCDTextField.CursorPosition = ListOfCards.GetInstance().CardsList.Count - 1;
                            }
                            Thread.Sleep(100);
                        }
                        if (!_joystickButton.Read())
                        {
                            _deleteCardState = DELETE_CARD_STATE.save;
                        }
                    break;
                case DELETE_CARD_STATE.save:
                    try
                    {
                        ListOfCards.GetInstance().DeleteCardFromList(LCDTextField.CursorPosition);
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
                case DISPLAY_CARDS_SATE.listIsEmpty:
                    LCD.GetInstance().Clear();
                    if (ListOfCards.GetInstance().IsEmpty())
                    {
                        _displayCardsState = DISPLAY_CARDS_SATE.errorMSG;
                    }
                    else
                    {
                        _displayCardsState = DISPLAY_CARDS_SATE.displayAllCards;
                    }
                    break;
                case DISPLAY_CARDS_SATE.errorMSG:
                    LCD.GetInstance().DisplayText(GT.Color.Red, "/!\\ Aucun badge n'est enregistre /!\\", 10, LCD.GetInstance().LcdHeight / 2);
                    Thread.Sleep(2000);
                    RestoreInitialState();
                    break;
                case DISPLAY_CARDS_SATE.displayAllCards:
                    int positionY = 10;
                    LCD.GetInstance().DisplayText(GT.Color.LightGray, "Pour quitter, appuyer sur le joystick", 10, LCD.GetInstance().LcdHeight - 20);

                    foreach (Card card in ListOfCards.GetInstance().CardsList)
                    {
                        LCD.GetInstance().DisplayText(GT.Color.Gray, card.Name, 10, positionY);
                        positionY += 15;
                    }

                    if (!_joystickButton.Read())
                    {
                        RestoreInitialState();
                    }
                    break;
                default:
                    _displayCardsState = DISPLAY_CARDS_SATE.listIsEmpty;
                    break;
            }
        }

        /// <summary>
        /// When the menu to unlock the box with the secret code is selected
        /// </summary>
        private void UnlockSecretCode()
        {
            LCD.GetInstance().Clear();
            bool success = false;

            do
            {
                LCD.GetInstance().DisplayText(GT.Color.LightGray, "Pour quitter, appuyer sur le joystick", 10, LCD.GetInstance().LcdHeight - 20);
                bool oldJoystickread = ((_joystickX.Read() > JOYSTICK_UP_RIGHT && _joystickX.Read() < JOYSTICK_DOWN_LEFT) && (_joystickY.Read() > JOYSTICK_UP_RIGHT && _joystickY.Read() < JOYSTICK_DOWN_LEFT));
                Thread.Sleep(200);
                bool joystickRead = ((_joystickX.Read() < JOYSTICK_UP_RIGHT || _joystickX.Read() > JOYSTICK_DOWN_LEFT) || (_joystickY.Read() < JOYSTICK_UP_RIGHT || _joystickY.Read() > JOYSTICK_DOWN_LEFT));

                if ((oldJoystickread && joystickRead) || _secretState == SECRET_CODE.success)
                {
                    LCD.GetInstance().Clear();
                    switch (_secretState)
                    {
                        case SECRET_CODE.up1:
                            if (_joystickX.Read() < JOYSTICK_UP_RIGHT)
                            {
                                _secretState = SECRET_CODE.up2;
                                Debug.Print("1");
                            }
                            else
                            {
                                _secretState = SECRET_CODE.error;
                            }
                            break;
                        case SECRET_CODE.up2:
                            if (_joystickX.Read() < JOYSTICK_UP_RIGHT)
                            {
                                _secretState = SECRET_CODE.down1;
                                Debug.Print("2");
                            }
                            else
                            {
                                _secretState = SECRET_CODE.error;
                            }
                            break;
                        case SECRET_CODE.down1:
                            if (_joystickX.Read() > JOYSTICK_DOWN_LEFT)
                            {
                                _secretState = SECRET_CODE.down2;
                                Debug.Print("3");
                            }
                            else
                            {
                                _secretState = SECRET_CODE.error;
                            }
                            break;
                        case SECRET_CODE.down2:
                            if (_joystickX.Read() > JOYSTICK_DOWN_LEFT)
                            {
                                _secretState = SECRET_CODE.left1;
                                Debug.Print("4");
                            }
                            else
                            {
                                _secretState = SECRET_CODE.error;
                            }
                            break;
                        case SECRET_CODE.left1:
                            if (_joystickY.Read() > JOYSTICK_DOWN_LEFT)
                            {
                                _secretState = SECRET_CODE.right1;
                                Debug.Print("5");
                            }
                            else
                            {
                                _secretState = SECRET_CODE.error;
                            }
                            break;
                        case SECRET_CODE.right1:
                            if (_joystickY.Read() < JOYSTICK_UP_RIGHT)
                            {
                                _secretState = SECRET_CODE.left2;
                                Debug.Print("6");
                            }
                            else
                            {
                                _secretState = SECRET_CODE.error;
                            }
                            break;
                        case SECRET_CODE.left2:
                            if (_joystickY.Read() > JOYSTICK_DOWN_LEFT)
                            {
                                _secretState = SECRET_CODE.right2;
                                Debug.Print("7");
                            }
                            else
                            {
                                _secretState = SECRET_CODE.error;
                            }
                            break;
                        case SECRET_CODE.right2:
                            if (_joystickY.Read() < JOYSTICK_UP_RIGHT)
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
                            ServoMotor.GetInstance().Unlock();
                            success = true;
                            break;
                        case SECRET_CODE.error:
                            _secretState = SECRET_CODE.up1;
                            LCD.GetInstance().DisplayText(GT.Color.Red, "Code faux, veuillez recommencer");
                            LCD.GetInstance().Clear();
                            Thread.Sleep(1000);
                            break;
                        default:
                            _secretState = SECRET_CODE.up1;
                            break;
                    }
                }
            } while (_joystickButton.Read() && !success);
            RestoreInitialState();
        }
    }
}
