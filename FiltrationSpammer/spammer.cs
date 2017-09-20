using System;
using System.Collections.Generic;
using System.Text;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using FiltrationSpammer;
using ion;
using System.Windows.Forms;
using System.Threading;


namespace FiltrationSpammer {
    public class Spammer {
        //private List<string> numbers = new List<string>(new string[] { "+441344207846" });
        //private List<string> numbersInUse = new List<string>();

        public bool autoStart { get; set; }

        public string numberToCall { get; set; }
        public bool recordAudio { get; set; }


        // ------------------------------------------
        public string accountID { get; set; }
        public string authToken { get; set; }

        public bool active { get; set; }
        public string taskID { get; set; }
        private bool setBreak = false;

        private int threadsRunning = 0;

        private int cnt = 0;
        private string subStatus = "";

        private CallResource mc;

        private System.Windows.Forms.Timer tw = null;

        public void spammerold(string spamnumber) {
            /*
            try {
                numberToCall = spamnumber;
                TwilioClient.Init(core.accountID, core.authToken);
                do {
                    foreach (string number in numbers) {
                        Call(number);
                    }
                } while (true);
            } catch (Exception ex) {
                //StatusTextChanged?.Invoke(this, "Error: {0}" + ex.Message);
            }
            */
        }

        private string status = "";

        public void Go() {
            active = true;
            tStart();
            if (autoStart)
                cycleStart();
        }
        public void manualStart() {
            if (active) return; // don't try to run the same thread twice.
            if (tw == null) tStart();
            cycleStart();
        }
        public void Stop() {
            if (tw != null) tw.Stop(); tw = null;
            active = false;
            setBreak = true;

        }
        public string Status() {
            return status;
        }

        private void tStart() {
            tw = new System.Windows.Forms.Timer();
            tw.Interval = 250;
            tw.Tick += (ecv, vn) => {
                if(active) {
                    status = (string)"Active -> (" + cnt + "s)" + " | " + subStatus;
                } else {
                    status = (string)"Paused | " + subStatus;
                }
               
            };
            tw.Start();

        }

        ~Spammer() {
            this.Stop();

            
        }

        private void cycleStart() { // threaded
            new Thread(() => {

                loop();
            }).Start();
        }

        private void loop() {
            while (true) {

                if (setBreak) break;
                subStatus = "Running.";
                cnt += 1;
                Thread.Sleep(1000);
            }
            active = false;
        }

        private void Call(string number) {
            try {
                mc = CallResource.Create(
                    to: new PhoneNumber(numberToCall),
                    from: new PhoneNumber(number),
                    record: recordAudio,
                    url: new Uri(String.Format("http://twimlets.com/echo?Twiml=<Response><Say>{0}</Say></Response>", core.spamMessage.Replace(" ", "+")))
                );
                subStatus = "Calling {numberToCall} with the number {number}";

            } catch (Exception ex) {
                subStatus = "Error: {0}" + ex.Message;
            }
        }
    }
}