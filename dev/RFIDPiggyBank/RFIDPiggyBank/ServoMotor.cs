using System;
using GHI.Pins;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace RFIDPiggyBank
{
    public class ServoMotor
    {
        private static ServoMotor _instance;
        private PWM _servo;

        private ServoMotor()
        {
            _servo = new PWM(FEZSpider.Socket11.Pwm8, 2000, 1000, PWM.ScaleFactor.Microseconds, false);
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
            _servo.DutyCycle = 750;
        }
    }
}
