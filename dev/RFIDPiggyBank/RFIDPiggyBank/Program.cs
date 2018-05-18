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
        private const double JOYSTICK_UP_RIGHT = 0.2;

        /// <summary>
        /// Constant of the value if the joystick is down ot left
        /// </summary>
        private const double JOYSTICK_DOWN_LEFT = 0.8;

        /// <summary>
        /// This is the logic version of the main menu
        /// </summary>
        private int _menu = 0;

        private MENU_STATE Menu = MENU_STATE.initial;
        private SERVO_STATE Servo = SERVO_STATE.close;
        private SECRET_CODE secretState = SECRET_CODE.up1;

        /// <summary>
        /// 90000ms = 1min30 -> this is the secure timer if we have forgotten to close the box
        /// </summary>
        private GT.Timer _secuTimer = new GT.Timer(90000);

        private const string DEFAULT_NAME = Card.DEFAULT_NAME;

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

        private void timer_Tick(GT.Timer pbTimer)
        {
            if (Menu == MENU_STATE.initial)
            {
                InitialState();
            }

            if (Menu == MENU_STATE.addCard)
            {
                AddCard();
            }

            if (Menu == MENU_STATE.deleteCard)
            {
                DeleteCard();
            }

            if (Menu == MENU_STATE.displayCards)
            {
                DisplayCards();
            }

            if (Menu == MENU_STATE.secretCode)
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
            RestoreInitialEtat();
        }

        private void _joystickButton_OnInterrupt(uint pbData1, uint pbData2, DateTime pbTime)
        {
            if (Menu == MENU_STATE.initial)
            {
                switch (_menu)
                {
                    case 0:
                        Menu = MENU_STATE.initial;
                        break;
                    case 1:
                        Menu = MENU_STATE.addCard;
                        break;
                    case 2:
                        Menu = MENU_STATE.deleteCard;
                        break;
                    case 3:
                        Menu = MENU_STATE.displayCards;
                        break;
                    case 4:
                        Menu = MENU_STATE.secretCode;
                        break;
                    default:
                        Menu = MENU_STATE.initial;
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
        private void RestoreInitialEtat()
        {
            _menu = 0;
            Menu = MENU_STATE.initial;
            DeleteCurrentBadgescan();
            LCD.GetInstance().Clear();
            DisplayMainMenu(_menu);
        }

        /// <summary>
        /// This method display an error message and wait 2 seconds
        /// </summary>
        private void DisplayError()
        {
            LCD.GetInstance().Clear();
            LCD.GetInstance().DisplayText(GT.Color.Red, "/!\\ Une erreur est survenue /!\\", 10, LCD.GetInstance().LcdHeight - 20);
            Thread.Sleep(2000);
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
            if (RFIDReader.GetInstance().IsBadgeScan)
            {
                if (Servo == SERVO_STATE.close)
                {
                    bool isValid = ListOfCards.GetInstance().FindCardInlist(RFIDReader.GetInstance().CurrentUid);
                    if (isValid)
                    {
                        ServoMotor.GetInstance().Unlock();
                        Servo = SERVO_STATE.open;
                        _secuTimer.Start();
                    }
                }
                else
                {
                    ServoMotor.GetInstance().Lock();
                    Servo = SERVO_STATE.close;
                    _secuTimer.Stop();
                }
                DeleteCurrentBadgescan();
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
            LCD.GetInstance().Clear();
            LCD.GetInstance().DisplayText(GT.Color.LightGray, "Veuillez approcher un badge du lecteur");
            if (RFIDReader.GetInstance().IsBadgeScan)
            {
                LCD.GetInstance().DisplayText(GT.Color.Green, "Votre badge a ete correctement scanne", 10, LCD.GetInstance().LcdHeight / 2);
                Thread.Sleep(2000); // On attends 2 seconde

                LCD.GetInstance().Clear();
                string uid = RFIDReader.GetInstance().CurrentUid;

                if (ListOfCards.GetInstance().FindCardInlist(uid)) // Si le badge scanné existe déjà
                {
                    LCD.GetInstance().DisplayText(GT.Color.Red, "/!\\ Erreur : Ce badge est deja sauvegarde /!\\", 10, LCD.GetInstance().LcdHeight / 2);
                    Thread.Sleep(2000); // On attends 2 secondes
                }
                else
                {
                    string name = DEFAULT_NAME;
                    int charIndex = 0;
                    char[] charArray = name.ToCharArray(); // On converti le string en tableau pour modifier les lettres
                    bool isModify = true;
                    do
                    {
                        int x = 110;
                        if (isModify)
                        {
                            LCD.GetInstance().Clear();
                            LCD.GetInstance().DisplayText(GT.Color.Red, "Votre badge :", 10, LCD.GetInstance().LcdHeight / 2);
                            LCD.GetInstance().DisplayText(GT.Color.LightGray, "Pour valider le nom, appuyer sur le joystick", 10, LCD.GetInstance().LcdHeight - 20);
                            for (int i = 0; i < charArray.Length; i++)
                            {
                                if (i == charIndex)
                                    LCD.GetInstance().DisplayText(GT.Color.Black, charArray[i].ToString(), x, LCD.GetInstance().LcdHeight / 2);
                                else
                                    LCD.GetInstance().DisplayText(GT.Color.Gray, charArray[i].ToString(), x, LCD.GetInstance().LcdHeight / 2);
                                x += 10;
                            }
                            isModify = false;
                        }

                        if (_joystickX.Read() < JOYSTICK_UP_RIGHT)
                        {
                            charArray[charIndex]++;
                            isModify = true;
                            Thread.Sleep(100); // On attends 0.1 seconde pour que les lettres ne défilent pas trop
                        }
                        else if (_joystickX.Read() > JOYSTICK_DOWN_LEFT)
                        {
                            charArray[charIndex]--;
                            isModify = true;
                            Thread.Sleep(100); // On attends 0.1 seconde pour que les lettres ne défilent pas trop
                        }


                        if (_joystickY.Read() < JOYSTICK_UP_RIGHT)
                        {
                            charIndex++;
                            if (charIndex > name.Length - 1)
                            {
                                charIndex = 0;
                            }
                            isModify = true;
                            Thread.Sleep(200); // On attends 0.2 seconde pour que le curseur ne défile pas trop
                        }
                        else if (_joystickY.Read() > JOYSTICK_DOWN_LEFT)
                        {
                            charIndex--;
                            if (charIndex < 0)
                            {
                                charIndex = name.Length - 1;
                            }
                            isModify = true;
                            Thread.Sleep(200); // On attends 0.2 seconde pour que le curseur ne défile pas trop
                        }
                    } while (_joystickButton.Read());

                    try
                    {
                        name = new string(charArray); // On remplace la variable name par les lettres du tableau
                        ListOfCards.GetInstance().AddCardToList(name, uid);
                        DisplaySave();
                        LCD.GetInstance().DisplayText(GT.Color.Green, "Le badge a bien ete ajoute", 10, LCD.GetInstance().LcdHeight / 2);
                        SDCard.GetInstance().SaveCards(ListOfCards.GetInstance().CardsList);
                    }
                    catch (Exception e)
                    {
                        DisplayError();
                    }
                }
                RestoreInitialEtat();
            }
        }

        /// <summary>
        /// When the menu to delete a card is selected
        /// </summary>
        private void DeleteCard()
        {
            LCD.GetInstance().Clear();
            if (ListOfCards.GetInstance().IsEmpty())
            {
                LCD.GetInstance().DisplayText(GT.Color.Red, "/!\\ Aucun badge n'est enregistre /!\\", 10, LCD.GetInstance().LcdHeight / 2);
                Thread.Sleep(2000);
            }
            else
            {
                int positionY;
                int index = 0;
                int i;

                do
                {
                    i = 0;
                    positionY = 10;
                    foreach (Card card in ListOfCards.GetInstance().CardsList)
                    {
                        if (index == i)
                            LCD.GetInstance().DisplayText(GT.Color.Black, card.Name, 10, positionY);
                        else
                            LCD.GetInstance().DisplayText(GT.Color.Gray, card.Name, 10, positionY);
                        positionY += 15;
                        i++;
                    }
                    LCD.GetInstance().DisplayText(GT.Color.LightGray, "Pour selectionner le badge, appuyer sur le joystick", 10, LCD.GetInstance().LcdHeight - 30);
                    if (_joystickX.Read() > JOYSTICK_DOWN_LEFT)
                    {
                        index++;
                        if (index > ListOfCards.GetInstance().CardsList.Count - 1)
                        {
                            index = 0;
                        }
                        Thread.Sleep(100);
                    }
                    else if (_joystickX.Read() < JOYSTICK_UP_RIGHT)
                    {
                        index--;
                        if (index < 0)
                        {
                            index = ListOfCards.GetInstance().CardsList.Count - 1;
                        }
                        Thread.Sleep(100);
                    }
                } while (_joystickButton.Read());

                try
                {
                    ListOfCards.GetInstance().DeleteCardFromList(index);
                    DisplaySave();
                    LCD.GetInstance().DisplayText(GT.Color.Green, "Le badge a bien ete supprime", 10, LCD.GetInstance().LcdHeight / 2);
                    SDCard.GetInstance().SaveCards(ListOfCards.GetInstance().CardsList);
                }
                catch (Exception e)
                {
                    DisplayError();
                }
            }
            RestoreInitialEtat();
        }

        /// <summary>
        /// When the menu to display all cards is selected
        /// </summary>
        private void DisplayCards()
        {
            LCD.GetInstance().Clear();
            if (ListOfCards.GetInstance().IsEmpty())
            {
                LCD.GetInstance().DisplayText(GT.Color.Red, "/!\\ Aucun badge n'est enregistre /!\\", 10, LCD.GetInstance().LcdHeight / 2);
                Thread.Sleep(2000);
            }
            else
            {
                int positionY = 10;

                foreach (Card card in ListOfCards.GetInstance().CardsList)
                {
                    LCD.GetInstance().DisplayText(GT.Color.Gray, card.Name, 10, positionY);
                    positionY += 15;
                }

                do
                {
                    LCD.GetInstance().DisplayText(GT.Color.LightGray, "Pour quitter, appuyer sur le joystick", 10, LCD.GetInstance().LcdHeight - 20);
                } while (_joystickButton.Read());
            }
            RestoreInitialEtat();
        }

        /// <summary>
        /// When the menu to unlock the box with the secret code is selected
        /// </summary>
        private void UnlockSecretCode()
        {
            LCD.GetInstance().Clear();

            bool oldJoystickread = (_joystickX.Read() > 0.4 && _joystickX.Read() < 0.6);
            Thread.Sleep(200);
            bool joystickRead = (_joystickX.Read() < 0.4 || _joystickX.Read() > 0.6);

            bool joystickEvent = (_oldJoystickRead && joystickRead);
            if (joystickEvent)
            {
                LCD.GetInstance().Clear();
                switch (secretState)
                {
                    case SECRET_CODE.up1:
                        if (_joystickX.Read() < JOYSTICK_UP_RIGHT)
                        {
                            secretState = SECRET_CODE.up2;
                            Debug.Print("1");
                        }
                        else
                        {
                            secretState = SECRET_CODE.error;
                        }
                        break;
                    case SECRET_CODE.up2:
                        if (_joystickX.Read() < JOYSTICK_UP_RIGHT)
                        {
                            secretState = SECRET_CODE.down1;
                            Debug.Print("2");
                        }
                        else
                        {
                            secretState = SECRET_CODE.error;
                        }
                        break;
                    case SECRET_CODE.down1:
                        if (_joystickX.Read() > JOYSTICK_DOWN_LEFT)
                        {
                            secretState = SECRET_CODE.down2;
                            Debug.Print("3");
                        }
                        else
                        {
                            secretState = SECRET_CODE.error;
                        }
                        break;
                    case SECRET_CODE.down2:
                        if (_joystickX.Read() > JOYSTICK_DOWN_LEFT)
                        {
                            secretState = SECRET_CODE.left1;
                            Debug.Print("4");
                        }
                        else
                        {
                            secretState = SECRET_CODE.error;
                        }
                        break;
                    case SECRET_CODE.left1:
                        ServoMotor.GetInstance().Unlock();
                        break;
                    case SECRET_CODE.right1:
                        break;
                    case SECRET_CODE.left2:
                        break;
                    case SECRET_CODE.right2:
                        break;
                    case SECRET_CODE.success:
                        break;
                    case SECRET_CODE.error:
                        secretState = SECRET_CODE.up1;
                        LCD.GetInstance().Clear();
                        LCD.GetInstance().DisplayText(GT.Color.Red, "Code faux");
                        Thread.Sleep(1000);
                        break;
                    default:
                        secretState = SECRET_CODE.up1;
                        break;
                }
            }
        }
    }
}
