/*
 * Author   : Küenzi Jean-Daniel
 * Date     : 09.05.2018
 * Desc.    : Class that handles the servo motor from Makeblock
 * Version  : 1.0.0
 */
using System;
using GHI.Pins;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Gadgeteer;
using GHIElectronics.Gadgeteer;

namespace RFIDPiggyBank
{
    public class ServoMotor
    {
        /// <summary>
        /// The instance of the class ServoMotor
        /// </summary>
        private static ServoMotor _instance;

        /// <summary>
        /// The lock position for MY servo, it's not the same for all
        /// </summary>
        private const uint LOCK_POSITION = 1500;

        /// <summary>
        /// The unlock position for MY servo, it's not the same for all
        /// </summary>
        private const uint UNLOCK_POSITION = 500;

        /// <summary>
        /// This is the object PWM who controll the servo
        /// </summary>
        private PWM _servo;

        /// <summary>
        /// The constructor of the class, he's private because the class use the design pattern Singleton
        /// </summary>
        private ServoMotor()
        {
            _servo = new PWM(GHI.Pins.FEZSpider.Socket11.Pwm8, 2000, LOCK_POSITION, PWM.ScaleFactor.Microseconds, false);
        } 

        /// <summary>
        /// Getter for the instance of the class
        /// </summary>
        public static ServoMotor Instance
        {
            get {return _instance;}
        }

        /// <summary>
        /// Method that allow access to the class
        /// </summary>
        /// <returns>Instance of the class ServoMotor</returns>
        public static ServoMotor GetInstance()
        {
            if (_instance == null)
            {
                _instance = new ServoMotor();
            }
            return Instance;
        }

        /// <summary>
        /// This method unlock the servo when call
        /// </summary>
        public void Unlock()
        {
            _servo.Duration = UNLOCK_POSITION;
        }

        /// <summary>
        /// This method lock the servo when call
        /// </summary>
        public void Lock()
        {
            _servo.Duration = LOCK_POSITION;
        }
    }
}
