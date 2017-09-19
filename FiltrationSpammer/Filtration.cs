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
         *  TODO from ionbladez
         *      1. actually run 'spam' function
         *      2. properly clean up running instances instead of just removing them.
         *      2.5 other misc stuff
         * 
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
                checkBox1.Checked = (bool)ion2.settings.Get("autoStartJob");
                checkBox2.Checked = (bool)ion2.settings.Get("recordAudio");
                twilioNumList = (ListBox)ion2.settings.Get("twilioNumbers");
            } catch (Exception ec) {

            }

        }

        private void load(object s, EventArgs e) {
            u = new ion.User();
            ion2.loadSettings(); // automatically initialize settings class
            // then loads settings to dictionary object


            // -------- initialize context menu
            listView1.ContextMenu = new ContextMenu() {
                Name = "taskListMenu"
            };

            MenuItem ls1 = new MenuItem("Remove Task");
            MenuItem ls2 = new MenuItem("Start Task");
            ls1.Click += (ee, bb) => {
                if (listView1.SelectedItems.Count < 1) return;
                removeTask((string)listView1.SelectedItems[0].Tag);
            };
            ls2.Click += (ci, eo) => {
                if (listView1.SelectedItems.Count < 1) return;
                startTask((string)listView1.SelectedItems[0].Tag);
            };
            listView1.ContextMenu.MenuItems.Add(ls1);
            listView1.ContextMenu.MenuItems.Add(ls2);
            listView1.ContextMenu.Popup += (df, vn) => {
                if (listView1.SelectedItems.Count < 1) return;
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
        public void addTask(Spammer sp) {
            ListViewItem tmp = new ListViewItem() {
                Name = "JobTask_" + sp.taskID,
                Tag = (string)sp.taskID,
                Text = sp.taskID
            };
            tmp.SubItems.Add("Idle");
            listView1.Items.Add(tmp);
            sp.Go();
            Q.Add(sp.taskID, sp);
            sp = null;
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

        private void startTask(string threadID) {
            if (!Q.ContainsKey(threadID)) return;
            Spammer s = Q[threadID];
            s.Go();
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
            if (MessageBox.Show("Stop all tasks?","Are you sure?", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                foreach (Spammer spc in Q.Values) {
                    spc.Stop();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e) {

        }

        private void textBox3_TextChanged(object sender, EventArgs e) {
            ion2.settings.Set("accid", textBox3.Text);

        }

        private void textBox4_TextChanged(object sender, EventArgs e) {
            ion2.settings.Set("authsecret", textBox4.Text);

        }

        private void taskAddBtn_Click(object sender, EventArgs e) {
            string tmpTaskID = strings.TruncateString(u.genID, 10);
            addTask(new Spammer() {
                numberToCall = offenderNumberTxt.Text,
                recordAudio = checkBox2.Checked,
                autoStart = checkBox1.Checked,
                taskID = tmpTaskID,
                accountID = (string)ion2.settings.Get("accid"),
                authToken = (string)ion2.settings.Get("authsecret")
            });
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
            twilioNumList.Items.Add(twilioNumberTxt.Text.Trim());
            ion2.settings.Set("twilioNumbers", twilioNumList);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) {
            if (twilioNumList.SelectedIndex < 0) { button4.Enabled = false; return; }
            button4.Enabled = true;

        }

        private void button4_Click(object sender, EventArgs e) {
            if (twilioNumList.SelectedIndex == -1) return;
            twilioNumList.Items.Remove(twilioNumList.SelectedIndex);
        }
    }
}
