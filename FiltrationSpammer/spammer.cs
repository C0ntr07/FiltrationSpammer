using System;
using System.Collections.Generic;
using System.Text;
using FiltrationSpammer;
using ion;
using System.Windows.Forms;
using System.Threading;

using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace FiltrationSpammer {
    public class Spammer {

        public bool autoStart { get; set; }

        public string numberToCall { get; set; }

        public string[] outgoingNumbers { get; set; }

        public bool recordAudio { get; set; }


        // ------------------------------------------
        public string accountID { get; set; }
        public string authToken { get; set; }

        public string sayMessage { get; set; }

        public bool active { get; set; }
        public string taskID { get; set; }
        private bool setBreak;// = false;


        private int cnt = 0;

        private string status { get; set; }
        private static string subStatus { get; set; }

        private int numberIndex = 0;

        private CallResource mc;

        private bool inProgress = false;

        private string tmpNumber;

        private System.Windows.Forms.Timer tw;

        public Spammer() {
            tw = new System.Windows.Forms.Timer();
            tw.Interval = 250;
            tw.Tick += (ecv, vn) => {
                if (this.active == true) {
                    status = (string)"Active -> (" + cnt + "s)" + " | " + subStatus;
                } else {
                    status = (string)"Paused -> " + subStatus;
                }
            };
            tw.Start();

            subStatus = "";

            if (autoStart)
                cycleStart();

        }




        public Spammer manualStart(bool startThis) {
            if (!startThis) return this;
            if (active) return this; // don't try to run the same thread twice.
            setBreak = false; // reset this.
            active = true;
            if (startThis == true) cycleStart();

            return this;
        }
        public void Stop() {
            // if (tw != null) tw.Stop();
            TwilioClient.Invalidate();
            active = false;
            setBreak = true;
            subStatus = "Loop stopped";

        }
        public string Status() {
            return status;
        }


        ~Spammer() {
            this.Stop();


        }

        private void cycleStart() {
            TwilioClient.Init(accountID, authToken);
            active = true;
            setBreak = false;

            subStatus = "Creating Thread";

            new Thread(() => {
                while (active) {
                    if (setBreak != false) break; // breaks out of loop, kills thread.
                    if (inProgress) return;
                    if (numberIndex < outgoingNumbers.Length) {
                        tmpNumber = outgoingNumbers[numberIndex];
                        numberIndex++;
                    } else {
                        numberIndex = 0;
                        tmpNumber = outgoingNumbers[0];
                    }

                    Call(numberToCall, tmpNumber);

                    cnt += 1;
                    Thread.Sleep(1000);
                }



            }).Start();
        }

        private void Call(string number, string fromNumber) {

            if (mc != null) {
                if (mc.Status == CallResource.UpdateStatusEnum.Completed) {
                    inProgress = false;
                }

                if (mc.Status == CallResource.StatusEnum.InProgress) {
                    subStatus = "Call in progess (" + number + ")";
                    inProgress = true;
                } else if (mc.Status == CallResource.StatusEnum.Ringing) {
                    subStatus = "Call ringing... (" + number + ")";
                    inProgress = true;
                } else if (mc.Status == CallResource.StatusEnum.Busy) {
                    inProgress = false;
                } else if (mc.Status == CallResource.StatusEnum.Canceled) {
                    inProgress = false;
                } else if (mc.Status == CallResource.StatusEnum.Failed) {
                    inProgress = false;
                }
            } else {
                inProgress = false;
                subStatus = "Calling {" + number + "} from {" + number + "}";
            }


            if (inProgress) return;
            Console.WriteLine("Trying to call {" + number + "} from {" + fromNumber + "}");

            mc = CallResource.Create(
                to: new PhoneNumber(numberToCall),
                from: new PhoneNumber(fromNumber),
                record: recordAudio,
                url: new Uri(String.Format("http://twimlets.com/echo?Twiml=<Response><Say>{0}</Say></Response>", core.spamMessage.Replace(" ", "+")))
            );


        }
    }
}