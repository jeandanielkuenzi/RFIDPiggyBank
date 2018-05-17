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
        private enum MENU_ETAT { initial, addBadge, deleteBadge, displayBadge, mdpAdmin }; // Etat du menu
        private enum SERVO_ETAT { open, close, secuClose }; // Etat du servo moteur

        private const int MENU1_Y = 10;
        private const int MENU2_Y = 30;
        private const int MENU3_Y = 50;
        private const int MENU4_Y = 70;

        private int _menu = 0;
        private MENU_ETAT Menu = MENU_ETAT.initial;
        private SERVO_ETAT Servo = SERVO_ETAT.close;
        private GT.Timer _secuTimer = new GT.Timer(90000); // 90000ms = 1min30 -> securité si on oublie de fermer la boite
        private string DEFAULT_NAME = Card.DEFAULT_NAME;

        private AnalogInput _joystickX = new AnalogInput(FEZSpider.Socket9.AnalogInput4); // Le potentiomètre du joystick sur l'axe X
        private AnalogInput _joystickY = new AnalogInput(FEZSpider.Socket9.AnalogInput5); // Le potentiomètre du joystick sur l'axe Y

        private InterruptPort _joystickButton = new InterruptPort(FEZSpider.Socket9.Pin3, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);

        // This method is run when the mainboard is powered up or reset.
        void ProgramStarted()
        {
            Initialize();

            GT.Timer timer = new GT.Timer(100); // every 0.1 seconds (100ms) on fait des timer parce que GHI n'aime pas les while(true) 
            timer.Tick += timer_Tick;
            timer.Start();

            _joystickButton.OnInterrupt += _joystickButton_OnInterrupt;

            _secuTimer.Tick += _secuTimer_Tick;
        }

        private void _secuTimer_Tick(GT.Timer pbTimer)
        {
            Servo = SERVO_ETAT.secuClose;
        }

        private void timer_Tick(GT.Timer pbTimer)
        {
            JoystickMenuChange();
            if (Menu == MENU_ETAT.initial || Servo == SERVO_ETAT.secuClose)
            {
                if (RFIDReader.GetInstance().IsBadgeScan || Servo == SERVO_ETAT.secuClose)
                {
                    if (Servo == SERVO_ETAT.close)
                    {
                        bool isValid = ListOfCards.GetInstance().FindCardInlist(RFIDReader.GetInstance().CurrentUid);
                        if (isValid)
                        {
                            ServoMotor.GetInstance().Unlock();
                            Servo = SERVO_ETAT.open;
                            _secuTimer.Start();
                        }
                    }
                    else
                    {
                        ServoMotor.GetInstance().Lock();
                        Servo = SERVO_ETAT.close;
                        _secuTimer.Stop();
                    }
                    DeleteCurrentBadgescan();
                }
            }

            if (Menu == MENU_ETAT.addBadge)
            {
                LCD.GetInstance().Clear();
                LCD.GetInstance().DisplayText(GT.Color.White, "Veuillez approcher un badge du lecteur");

                if (RFIDReader.GetInstance().IsBadgeScan)
                {
                    LCD.GetInstance().DisplayText(GT.Color.Green, "Votre badge a ete correctement scanne", 10, LCD.GetInstance().LcdHeight / 2);
                    Thread.Sleep(2000); // On attends 2 seconde

                    LCD.GetInstance().Clear();
                    string uid = RFIDReader.GetInstance().CurrentUid;

                    if (ListOfCards.GetInstance().FindCardInlist(uid)) // Si le badge scanné existe déjà
                    {
                        LCD.GetInstance().DisplayText(GT.Color.Red, "/!\\ Erreur : Ce badge est deja sauvegarde /!\\", 10, LCD.GetInstance().LcdHeight / 2);
                        Thread.Sleep(3000); // On attends 3 seconde
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
                                LCD.GetInstance().DisplayText(GT.Color.White, "Pour valider le nom, appuyer sur le joystick", 10, LCD.GetInstance().LcdHeight - 20);
                                for (int i = 0; i < charArray.Length; i++)
                                {
                                    if (i == charIndex)
                                        LCD.GetInstance().DisplayText(GT.Color.White, charArray[i].ToString(), x, LCD.GetInstance().LcdHeight / 2);
                                    else
                                        LCD.GetInstance().DisplayText(GT.Color.Red, charArray[i].ToString(), x, LCD.GetInstance().LcdHeight / 2);
                                    x += 10;
                                }
                                isModify = false;
                            }

                            if (_joystickX.Read() > 0.9)
                            {
                                charArray[charIndex]++;
                                isModify = true;
                                Thread.Sleep(100); // On attends 0.1 seconde pour que les lettres ne défilent pas trop
                            }
                            else if (_joystickX.Read() < 0.1)
                            {
                                charArray[charIndex]--;
                                isModify = true;
                                Thread.Sleep(100); // On attends 0.1 seconde pour que les lettres ne défilent pas trop
                            }


                            if (_joystickY.Read() > 0.9)
                            {
                                charIndex++;
                                if (charIndex > name.Length - 1)
                                {
                                    charIndex = 0;
                                }
                                isModify = true;
                                Thread.Sleep(200); // On attends 0.2 seconde pour que le curseur ne défile pas trop
                            }
                            else if (_joystickY.Read() < 0.1)
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

                        name = new string(charArray); // On remplace la variable name par les lettres du tableau
                        ListOfCards.GetInstance().AddCardToList(name, uid);
                        DisplaySave();
                        SDCard.GetInstance().SaveCards(ListOfCards.GetInstance().CardsList);
                    }
                    DeleteCurrentBadgescan();
                    InitialEtat();
                }
            }

            if (Menu == MENU_ETAT.deleteBadge)
            {
                LCD.GetInstance().Clear();
                int positionY = 10;
                int index = 0;
                int i = 0;
                foreach (Card card in ListOfCards.GetInstance().CardsList)
                {
                    if (index == i)
                        LCD.GetInstance().DisplayText(GT.Color.White, card.Name, 10, positionY);
                    else
                        LCD.GetInstance().DisplayText(GT.Color.Red, card.Name, 10, positionY);
                    positionY += 15;
                    i++;
                }
                do
                {
                    LCD.GetInstance().DisplayText(GT.Color.White, "Pour quitter, appuyer sur le joystick", 10, LCD.GetInstance().LcdHeight - 20);
                } while (_joystickButton.Read());
                InitialEtat();
            }

            if (Menu == MENU_ETAT.displayBadge)
            {
                LCD.GetInstance().Clear();
                int positionY = 10;

                foreach (Card card in ListOfCards.GetInstance().CardsList)
                {
                    LCD.GetInstance().DisplayText(GT.Color.White, card.Name, 10, positionY);
                    positionY += 15;
                }

                do
                {
                    LCD.GetInstance().DisplayText(GT.Color.White, "Pour quitter, appuyer sur le joystick", 10, LCD.GetInstance().LcdHeight - 20);
                } while (_joystickButton.Read());
                InitialEtat();
            }

            if (Menu == MENU_ETAT.mdpAdmin)
            {
                Debug.Print("mdpAdmin");
            }
        }

        private void Initialize()
        {
            ServoMotor.GetInstance().Lock();
            RFIDReader.GetInstance();
            DisplayLoad();
            ListOfCards.GetInstance().CardsList = SDCard.GetInstance().LoadCards();
            if (ListOfCards.GetInstance().CardsList == null)
            {
                ListOfCards.GetInstance().CardsList = new ArrayList();
            }
            InitialEtat();
        }

        private void _joystickButton_OnInterrupt(uint pbData1, uint pbData2, DateTime pbTime)
        {
            if (Menu == MENU_ETAT.initial)
            {
                switch (_menu)
                {
                    case 0:
                        Menu = MENU_ETAT.initial;
                        break;
                    case 1:
                        Menu = MENU_ETAT.addBadge;
                        break;
                    case 2:
                        Menu = MENU_ETAT.deleteBadge;
                        break;
                    case 3:
                        Menu = MENU_ETAT.displayBadge;
                        break;
                    case 4:
                        Menu = MENU_ETAT.mdpAdmin;
                        break;
                    default:
                        Menu = MENU_ETAT.initial;
                        break;
                }
                DeleteCurrentBadgescan();
            }
        }

        private void JoystickMenuChange()
        {
            if (Menu == MENU_ETAT.initial)
            {
                if (_joystickX.Read() > 0.9)
                {
                    _menu++;
                    if (_menu > 4)
                    {
                        _menu = 0;
                    }
                    DisplayMainMenu(_menu); // On ne le sort pas du test (if) pour éviter que le lcd clignote
                    Thread.Sleep(100);
                }
                else if (_joystickX.Read() < 0.1)
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
        }

        private void DeleteCurrentBadgescan()
        {
            RFIDReader.GetInstance().CurrentUid = "";
            RFIDReader.GetInstance().IsBadgeScan = false;
        }

        private void InitialEtat()
        {
            _menu = 0;
            Menu = MENU_ETAT.initial;
            LCD.GetInstance().Clear();
            DisplayMainMenu(_menu);
        }

        private void DisplaySave()
        {
            LCD.GetInstance().Clear();
            LCD.GetInstance().DisplayText(GT.Color.White, "Sauvegarde en cours...", 10, LCD.GetInstance().LcdHeight - 20);
        }

        private void DisplayLoad()
        {
            LCD.GetInstance().Clear();
            LCD.GetInstance().DisplayText(GT.Color.White, "Chargement en cours...", 10, LCD.GetInstance().LcdHeight - 20);
        }

        private void DisplayMainMenu(int pbMenu)
        {
            LCD.GetInstance().DisplayText(Gadgeteer.Color.Red, "Ajouter un badge", 10, MENU1_Y);
            LCD.GetInstance().DisplayText(Gadgeteer.Color.Red, "Supprimer un badge", 10, MENU2_Y);
            LCD.GetInstance().DisplayText(Gadgeteer.Color.Red, "Afficher la liste des badges", 10, MENU3_Y);
            LCD.GetInstance().DisplayText(Gadgeteer.Color.Red, "Deverouiller avec le mot de passe", 10, MENU4_Y);
            switch (pbMenu)
            {
                case 1:
                    LCD.GetInstance().DisplayText(Gadgeteer.Color.White, "Ajouter un badge", 10, MENU1_Y);
                    break;
                case 2:
                    LCD.GetInstance().DisplayText(Gadgeteer.Color.White, "Supprimer un badge", 10, MENU2_Y);
                    break;
                case 3:
                    LCD.GetInstance().DisplayText(Gadgeteer.Color.White, "Afficher la liste des badges", 10, MENU3_Y);
                    break;
                case 4:
                    LCD.GetInstance().DisplayText(Gadgeteer.Color.White, "Deverouiller avec le mot de passe", 10, MENU4_Y);
                    break;
            }
        }
    }
}
