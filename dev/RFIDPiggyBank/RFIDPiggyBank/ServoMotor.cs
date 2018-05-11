using System;
using GHI.Pins;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Gadgeteer;

namespace RFIDPiggyBank
{
    public class ServoMotor
    {
        private const uint LOCK_POSITION = 1500;
        private const uint UNLOCK_POSITION = 500;
        private static ServoMotor _instance;
        private PWM _servo;

        private ServoMotor()
        {
            _servo = new PWM(FEZSpider.Socket11.Pwm8, 2000, LOCK_POSITION, PWM.ScaleFactor.Microseconds, false);
        } 

        public static ServoMotor Instance
        {
            get {return _instance;}
        }

        public static ServoMotor GetInstance()
        {
            if (_instance == null)
            {
                _instance = new ServoMotor();
            }
            return Instance;
        }

        public void Unlock()
        {
            _servo.Duration = UNLOCK_POSITION;
        }

        public void Lock()
        {
            _servo.Duration = LOCK_POSITION;
        }
    }
}
