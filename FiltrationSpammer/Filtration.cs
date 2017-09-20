using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ion;


namespace FiltrationSpammer {
    public partial class Filtration : Form {

        /*
         *  TODO from lvt
         *      1. actually run 'spam' function (x)
         *      2. give each SPins (Spam instance) it's own set of numbers, or all of them to loop through. (x)
         *      2.1 properly clean up running instances instead of just removing them. (x)
         *      2.5 other misc stuff (??)
         * 
         */

        private io ion2 = new io();
        private ion.User u;

        private Dictionary<string, Spammer> Q;
        private ContextMenuStrip ls;

        private ion.strings strings = new ion.strings();

        public Filtration() {
            InitializeComponent();
        }

        private void Loaded(Object ss, EventArgs ee) {
            try {
                textBox3.Text = (string)ion2.settings.Get("accid");
                textBox4.Text = (string)ion2.settings.Get("authsecret");
                voiceMsgTxt.Text = (string)ion2.settings.Get("tts");
                checkBox1.Checked = (bool)ion2.settings.Get("autoStartJob");
                checkBox2.Checked = (bool)ion2.settings.Get("recordAudio");
                offenderNumberTxt.Text = (string)ion2.settings.Get("faceNumber");
                System.Collections.ArrayList ar = (System.Collections.ArrayList)ion2.settings.Get("tNum");
                foreach (string at in ar.Cast<string>()) {
                    outboundNumbers.Items.Add(at);



                }





            } catch (Exception ec) {
                Console.WriteLine(ec.Message);
            }

        }

        private void load(object s, EventArgs e) {
            ion2.loadSettings(); // automatically initialize settings class
            // then loads settings to dictionary object

            // -------- initialize context menu
            listView1.ContextMenu = new ContextMenu() {
                Name = "taskListMenu"
            };

            MenuItem ls1 = new MenuItem("Remove Task");
            MenuItem ls2 = new MenuItem("Start Task");
            MenuItem ls3 = new MenuItem("Pause Task");
            ls1.Click += (ee, bb) => {
                if (listView1.SelectedItems.Count < 1) return;
                removeTask((string)listView1.SelectedItems[0].Tag);
            };
            ls2.Click += (ci, eo) => {
                if (listView1.SelectedItems.Count < 1) return;
                startTask((string)listView1.SelectedItems[0].Tag);
            };
            ls3.Click += (wi, ck) => {
                if (listView1.SelectedItems.Count < 1) return;
                stopTask((string)listView1.SelectedItems[0].Tag);
            };
            listView1.ContextMenu.MenuItems.Add(ls1);
            listView1.ContextMenu.MenuItems.Add(ls2);
            listView1.ContextMenu.MenuItems.Add(ls3);
            listView1.ContextMenu.Popup += (df, vn) => {
                foreach (MenuItem lt in listView1.ContextMenu.MenuItems) {
                    lt.Enabled = false;
                }
                if (listView1.SelectedItems.Count < 1) return;
                listView1.ContextMenu.MenuItems[0].Enabled = true;
                if (taskIsActive((string)listView1.SelectedItems[0].Tag)) {
                    ls3.Enabled = true;
                } else {
                    ls2.Enabled = true;
                }
            };

            // -------------
            FormClosing += (ee, vv) => {
                foreach (Spammer spc in Q.Values) {
                    spc.Stop();

                }
                ion2.saveSettings();
            };

            // -------------

            Q = new Dictionary<string, Spammer>();


            // -------------- timer 1
            System.Windows.Forms.Timer tt = new System.Windows.Forms.Timer();
            tt.Interval = 350;
            tt.Tick += (df, gd) => {
                counterLbl.Text = (string)"Queue: " + Q.Count;
            };
            tt.Start();
            // -------------- status timer
            System.Windows.Forms.Timer ty = new System.Windows.Forms.Timer();
            ty.Interval = 1000;
            ty.Tick += (em, cl) => {
                // this is a very crude, ineffective way of doing this, but
                // I'm lazy and it works.
                foreach (ListViewItem ii in listView1.Items) {
                    Spammer spc = Q[(string)ii.Tag];
                    ii.SubItems[1].Text = spc.Status();
                }
            };
            ty.Start();
        }


        ~Filtration() {
            ion2.saveSettings();
            Dispose(false);
            return;
        }


        private void saveSettings() {
            ion2.saveSettings();

        }

        #region "TaskPool"
        // -- some stuff goes here that keeps track of the task pool list and interacts with it.
        public void addTask() {
            ion.User u = new ion.User();
            string tmpTaskID = strings.TruncateString(u.genID, 10);
            string[] tmpout = outboundNumbers.Items.OfType<string>().ToArray();
            Spammer sp = new Spammer() {
                numberToCall = offenderNumberTxt.Text,
                recordAudio = checkBox2.Checked,
                autoStart = checkBox1.Checked,
                taskID = tmpTaskID,
                //accountID = (string)ion2.settings.Get("accid"),
                accountID = textBox3.Text,
                //authToken = (string)ion2.settings.Get("authsecret"),
                authToken = textBox4.Text,
                outgoingNumbers = tmpout,
                sayMessage = voiceMsgTxt.Text
            };
            u = null;
            ListViewItem tmp = new ListViewItem() {
                Name = "JobTask_" + sp.taskID,
                Tag = (string)sp.taskID,
                Text = sp.taskID
            };
            tmp.SubItems.Add("Idle");
            listView1.Items.Add(tmp);
            Q.Add(sp.taskID, sp.manualStart(sp.autoStart));
        }

        public void removeTask(string threadID) {
            stopTask(threadID);
            Q.Remove(threadID);
            listView1.Items.RemoveAt(listView1.SelectedItems[0].Index);
        }

        public void stopTask(string threadID) {
            if (Q.ContainsKey(threadID)) {
                if (Q[threadID].active == true) {
                    Q[threadID].Stop();
                }
            }
        }

        public bool taskIsActive(string threadID) {
            bool tmp = false;
            if (threadID == "") return tmp;
            return Q[threadID].active;
        }

        private void startTask(string threadID) {
            if (Q.ContainsKey(threadID)) Q[threadID].manualStart(true);
        }

        public void updateStatus(string threadID, string msg) {
            if (Q.ContainsKey(threadID)) {
                foreach (ListViewItem l in listView1.Items) {
                    if ((string)l.Tag == threadID) l.SubItems[1].Text = msg;
                }
            }

        }
        #endregion




        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e) {
            runSpammer();
        }

        private void runSpammer() {
            // deprecated in favor of "threading".
        }

        private void button1_Click_1(object sender, EventArgs e) {
            if (MessageBox.Show("Stop all tasks?", "Are you sure?", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                foreach (Spammer spc in Q.Values) {
                    spc.Stop();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e) {
            ion2.saveSettings();
        }

        private void textBox3_TextChanged(object sender, EventArgs e) {
            ion2.settings.Set("accid", textBox3.Text);

        }

        private void textBox4_TextChanged(object sender, EventArgs e) {
            ion2.settings.Set("authsecret", textBox4.Text);

        }

        private void taskAddBtn_Click(object sender, EventArgs e) {
            addTask();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e) {
            ion2.settings.Set("autoStartJob", (bool)checkBox1.Checked);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e) {
            ion2.settings.Set("recordAudio", (bool)checkBox2.Checked);
        }

        private void button3_Click(object sender, EventArgs e) {
            if (twilioNumberTxt.Text == "") return; // don't add a blank value.
            string tmp = twilioNumberTxt.Text;
            tmp = tmp.Replace(" ", ""); // replace the space!
            outboundNumbers.Items.Add(tmp.Trim());

            ion2.settings.Set("tNum", outboundNumbers.Items);
            saveSettings();
            twilioNumberTxt.Text = "";
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) {
            if (outboundNumbers.SelectedIndex == -1) { button4.Enabled = false; return; }
            button4.Enabled = true;

        }

        private void button4_Click(object sender, EventArgs e) {
            if (outboundNumbers.SelectedIndex == -1) return;
            outboundNumbers.Items.RemoveAt(outboundNumbers.SelectedIndex);
        }

        private void voiceMsgTxt_TextChanged(object sender, EventArgs e) {
            ion2.settings.Set("tts", voiceMsgTxt.Text);
        }

        private void offenderNumberTxt_TextChanged(object sender, EventArgs e) {
            ion2.settings.Set("faceNumber", (string)offenderNumberTxt.Text);
        }
    }
}
