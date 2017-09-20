using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ion {
    public class io {
        private string myFolder = Application.UserAppDataPath + "\\";
        private string name;
        public string appName {
            get { return this.name; }
            set { this.name = value; }
        }
        public mySettings settings;
        public io() {
            this.settings = new mySettings();
        }
        ~io() {
            saveSettings(); // manually write settings on destruct
        }

        //-------------------------------------
        #region "mySettings"
        public class mySettings {
            public Dictionary<string, Object> _s;
            public mySettings() {
                _s = new Dictionary<string, object>();
            }
            public void Set(string name, object obj) {
                if (!_s.ContainsKey(name)) {
                    _s.Add(name, obj);
                } else {
                    _s[name] = obj; // could probably be more efficient using remove then add...
                }
            }
            public object Get(string name) {
                if (!_s.ContainsKey(name)) return null; // don't throw an error, just return null type.
                return (object)_s[name];
            }
        }

        public string serializeObject(object inp) {
            return new JavaScriptSerializer().Serialize(inp);
        }
        public void loadSettings() {
            if (System.IO.File.Exists(myFolder + "\\settings.json") == false) {
                saveSettings();
            }
            this.settings = (mySettings)new JavaScriptSerializer().Deserialize(System.IO.File.ReadAllText(myFolder + "\\settings.json"), typeof(mySettings));
        }
        public void saveSettings() {
            System.IO.File.WriteAllText(myFolder + "\\settings.json", serializeObject(this.settings));
        }
        #endregion
        // ------------------------------------
    }
    public class User {
        public string username { get; set; }
        public string password { get; set; }
        public DateTime last_used { get; set; }
        public int status { get; set; } // 0=untested, 1=ok, 2=banned/bad
        public string uniqueid = "";
        public User(string uid = "") {
            if (uid == "") this.uniqueid = genID; // this assumes we're creating a new instance of a 'User'
            if (this.last_used == null) this.last_used = DateTime.Now;
        }
        public string genID { get { return Guid.NewGuid().ToString("N"); } }

    }

    public class strings {
        public string TruncateString(string inp, int length = 10) {
            var outp = ""; var tmp = inp;
            if (tmp.Length <= length) return tmp; // if the input string is less than, return it.
            if (length < 5)
                length = 5; // let's keep the length at least 5 chars
            for(int i2 = 0; i2<length; i2++) {
                outp += inp[i2];
            }
            return outp;
        }

    }
}