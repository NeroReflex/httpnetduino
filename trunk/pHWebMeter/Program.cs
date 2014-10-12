using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace pHWebMeter
{
    public class Program
    {

        private static OutputPort led = new OutputPort(Pins.ONBOARD_LED, false);
        private static bool isLedOn = false;
        public static void Main()
        {
            //Start the http web server
            System.Threading.ThreadStart webServerProcess = new System.Threading.ThreadStart(WebServerService);
            System.Threading.Thread webServer = new System.Threading.Thread(webServerProcess);
            webServer.Start();

            InterruptPort btn = new InterruptPort(SecretLabs.NETMF.Hardware.Netduino.Pins.ONBOARD_BTN, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
            btn.OnInterrupt += new NativeEventHandler(button_OnInterrupt);
            
            Thread.Sleep(Timeout.Infinite);

            return;
        }

        private static void button_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            if (isLedOn)
                led.Write(false);
            else
                led.Write(true);
            isLedOn = !isLedOn;
        }

        public static void WebServerService()
        {
            //select the pin to be used
            /* Cpu.Pin conversionPIN = SecretLabs.NETMF.Hardware.Netduino.Pins.GPIO_PIN_A0;
            SecretLabs.NETMF.Hardware.AnalogInput analog = new SecretLabs.NETMF.Hardware.AnalogInput(conversionPIN);
            
            //read the digital result
            int ValoreMisurato = analog.Read(); */
            //initialize and start the HTTPDuino library
            try
            {
                HTTPDuino.Configuration config = new HTTPDuino.Configuration(80, "\\pHMeter");
                config.IncludeMD5 = true;
                HTTPDuino.HTTPServer myWebServer = new HTTPDuino.HTTPServer(config);
                myWebServer.Start();
            }
            catch (Exception error)
            {
                Debug.Print(error.Message + "\r\n");
            }
            
        }

    }
}
