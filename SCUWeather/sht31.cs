using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Gpio;
using System.Diagnostics;
using Windows.Devices.I2c;
using Windows.Devices.Enumeration;
using Windows.System.Threading;
using Windows.UI.Core;
using System.Threading.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace App5
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {



        I2cDevice sensor;
        DispatcherTimer timer;
        private ThreadPoolTimer TPtimer;
        private ThreadPoolTimer Audiotimer;
        public string GPIOStatus;
        private const int ENVTime = 1000;



        public MainPage()
        {
            this.InitializeComponent();

            Debug.WriteLine(DateTime.Now + " Main Page Initialization Starting");

            Debug.WriteLine(DateTime.Now + " Init GPIO");
            InitGPIO();

            Debug.WriteLine(DateTime.Now + " Init SPI");
            InitSPI();

            Debug.WriteLine(DateTime.Now + " Initialization Complete");


        }

        private void Timer_Tick(ThreadPoolTimer timer)
        {
            if (GPIOStatus == "Connected")
            {

        //        Debug.WriteLine("Reset Command!");


                
      //          Debug.WriteLine("Command 1");

                byte[] tempCommand1 = { 0x24 };
                sensor.Write(tempCommand1);

    //            Debug.WriteLine("Command 2!");

                byte[] tempCommand2 = { 0x24, 0x00, 0xFF };
//                sensor.Write(tempCommand2);

  //              Debug.WriteLine("Commands Done!");

                byte[] tempData = new byte[8];
                
                try

                {
//                    Debug.WriteLine("Sensor Read Done!");
//                    sensor.Read(tempData);
                    sensor.WriteRead(tempCommand2, tempData);
                    
                    var rawTempReading = tempData[0] << 8;
                    double stemp = rawTempReading;
                    Debug.WriteLine("RAW : " + stemp.ToString());

                    stemp *= 175;
                    stemp /= 0xffff;
                    stemp = -45 + stemp;
                    stemp *= 1.8;
                    stemp += 32;

                    Debug.WriteLine("Temp: " + stemp.ToString());


                    var rawHumReading = tempData[3] << 8;// | tempData[4];
                    double shum = rawHumReading;
                    shum *= 100;
                    shum /= 0xffff;

                    Debug.WriteLine("Humidity: " + shum.ToString());
                }

                    


                catch
                {
                    Debug.WriteLine("Buss is mad!");
                }





            }
        }

        private void Sleep(int v)
        {
            throw new NotImplementedException();
        }

        public async void InitSPI()
        {

            String aqs = I2cDevice.GetDeviceSelector("I2C1");
            var deviceInfo = await DeviceInformation.FindAllAsync(aqs);
            sensor = await I2cDevice.FromIdAsync(deviceInfo[0].Id, new I2cConnectionSettings(0x44));
            

            if (sensor == null)
            {
                Debug.WriteLine("Sensor Device Not Found");
            }

            else
            {


                Debug.WriteLine("Sensor Device Found");

                //byte[] resetCommand = { 0x30, 0xA2 };
                //sensor.Write(resetCommand);


                TPtimer = ThreadPoolTimer.CreatePeriodicTimer(Timer_Tick, TimeSpan.FromMilliseconds(ENVTime)); // .FromMilliseconds(1000));

            }
            
            
        }

        private async void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                // GpioStatus.Text = "There is no GPIO controller on this device.";
                Debug.WriteLine("There is no GPIO controller on this device.");
                return;
            }

            else
            {
                Debug.WriteLine("GPIO controller - OK!");
                GPIOStatus = "Connected";
              

            }




        }



    }
}
