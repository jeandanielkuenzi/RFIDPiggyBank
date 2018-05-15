using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;
using Microsoft.SPOT.Hardware;

namespace RFIDPiggyBank
{
    enum MENU_ETAT { initial, addBadge, deleteBadge, mdpAdmin };
    enum SERVO_ETAT { open, close, secuClose };
    public partial class Program
    {
        private int _menu = 0;
        private MENU_ETAT Menu = MENU_ETAT.initial;
        private SERVO_ETAT Servo = SERVO_ETAT.close;
        private GT.Timer _secuTimer = new GT.Timer(90000); // 90000ms = 1min30 -> securité si on oublie de fermer la boite
        // This method is run when the mainboard is powered up or reset.
        void ProgramStarted()
        {
            Initialize();

            ListOfCards.GetInstance().AddCardToList("Badge", "U12D3456FC");
            ListOfCards.GetInstance().AddCardToList("Badge", "U965B23CF5");
            ListOfCards.GetInstance().AddCardToList("Badge", "U12D3456FC");
            ListOfCards.GetInstance().AddCardToList("Badge", "U965B23CF5");
            ListOfCards.GetInstance().AddCardToList("Badge", "U12D3456FC");
            ListOfCards.GetInstance().AddCardToList("Badge", "U965B23CF5");
            ListOfCards.GetInstance().AddCardToList("Marty", "U12D3456FC");
            ListOfCards.GetInstance().AddCardToList("Professeur", "6A0076490F");

            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");

            LCD.GetInstance().DisplayMenu();
            SDCardSerializer.GetInstance().SaveCards();

            _joystick.JoystickPressed += _joystick_JoystickPressed;

            GT.Timer timer = new GT.Timer(500); // every 1/2 second (500ms)
            timer.Tick += timer_Tick;
            timer.Start();

            _secuTimer.Tick += _secuTimer_Tick;
        }

        private void _secuTimer_Tick(GT.Timer timer)
        {
            Servo = SERVO_ETAT.secuClose;
        }

        private void timer_Tick(GT.Timer timer)
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
                        }
                        _secuTimer.Start();
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
                Debug.Print("addBadge");
            }

            if (Menu == MENU_ETAT.deleteBadge)
            {
                Debug.Print("deleteBadge");
            }

            if (Menu == MENU_ETAT.mdpAdmin)
            {
                Debug.Print("mdpAdmin");
            }
        }

        private void _joystick_JoystickPressed(Joystick sender, Joystick.ButtonState state)
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
                    Menu = MENU_ETAT.mdpAdmin;
                    break;
                default:
                    Menu = MENU_ETAT.initial;
                    break;
            }
            DeleteCurrentBadgescan();
        }

        private void Initialize()
        {
            ServoMotor.GetInstance().Lock();
            RFIDReader.GetInstance();
            LCD.GetInstance();
            SDCardSerializer.GetInstance();
        }

        private void JoystickMenuChange()
        {
            if (_joystick.GetPosition().X > 0.9)
            {
                _menu++;
                if (_menu > 3)
                {
                    _menu = 0;
                }
            }
            else if (_joystick.GetPosition().X < -0.9)
            {
                _menu--;
                if (_menu < 0)
                {
                    _menu = 3;
                }
            }
        }

        private void DeleteCurrentBadgescan()
        {
            RFIDReader.GetInstance().CurrentUid = "";
            RFIDReader.GetInstance().IsBadgeScan = false;
        }
    }
}
