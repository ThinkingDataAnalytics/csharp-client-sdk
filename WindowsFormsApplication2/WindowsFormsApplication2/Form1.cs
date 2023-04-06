using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


using ThinkingData.Analytics;

namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        public static ThinkingdataAnalytics ta;
        public Form1()
        {
            InitializeComponent();
            this.init_button.Click += new EventHandler(this.taInit);
            this.login_button.Click += new EventHandler(this.talogin);
            this.disticid_button.Click += new EventHandler(this.taidentityid);
            this.logout_button.Click += new EventHandler(this.talogout);
            this.dyldsuperProperty_button.Click += new EventHandler(this.tadyldsuperProperty);
            this.superProperty_button.Click += new EventHandler(this.tasuperProperty);
            this.cleanSuperProperty_button.Click += new EventHandler(this.tacleanSuperProperty);
            this.cleanOnce_SuperProperty_button.Click += new EventHandler(this.tacleanOneSuperProperty);
            this.track_button.Click += new EventHandler(this.taTrack);
            this.firstEvent_button.Click += new EventHandler(this.tafirstEvent);
            this.updateEvent_button.Click += new EventHandler(this.taupdateEvent);
            this.overWrite_button.Click += new EventHandler(this.taoverWriteEvent);
            this.userset_button.Click += new EventHandler(this.tauser_set);
            this.userunset_button.Click += new EventHandler(this.tauser_unset);
            this.userserOnce_button.Click += new EventHandler(this.tauser_setOnce);
            this.useradd_button.Click += new EventHandler(this.tauser_add);
            this.useraddppend_button.Click += new EventHandler(this.tauser_append);
            this.userunidppend_button.Click += new EventHandler(this.tauser_uni_append);
            this.userdelete_button.Click += new EventHandler(this.tauser_delete);
            this.flush_button.Click += new EventHandler(this.taflush);
        }

        public void taflush(Object obj, EventArgs param)
        {
            ta.Flush();
        }
        public void tauser_delete(Object obj, EventArgs param)
        {
            ta.UserDelete();
        }
        public void tauser_uni_append(Object obj, EventArgs param)
        {

        }
        public void tauser_append(Object obj, EventArgs param)
        {

            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            List<string> list6 = new List<string>();
            list6.Add("true");
            list6.Add("test");
            dictionary.Add("arrkey4", list6);
            ta.UserAppend(dictionary);
        }
        public void tauser_add(Object obj, EventArgs param)
        {
            Dictionary<string, object> dic4 = new Dictionary<string, object>();
            dic4.Add("TotalRevenue", 648);
            ta.UserAdd(dic4);
        }

        public void tauser_setOnce(Object obj, EventArgs param)
        {
            Dictionary<string, object> dic5 = new Dictionary<string, object>();
            dic5.Add("login_name", "皮2");
            dic5.Add("#time", new DateTime(2019, 12, 10, 15, 12, 11, 444));
            dic5.Add("#ip", "192.168.1.1");
            //dic5.Add("#uuid",Guid.NewGuid().ToString("D")); 上传#uuid为标准格式(8-4-4-4-12)的string,服务端比较稳定，可不上传
            ta.UserSetOnce(dic5);
        }

        public void tauser_unset(Object obj, EventArgs param)
        {
            List<string> list2 = new List<string>();
            list2.Add("nickname");
            list2.Add("age");
            ta.UserUnSet(list2);
        }

        public void tauser_set(Object obj, EventArgs param)
        {
            Dictionary<string, object> dic7 = new Dictionary<string, object>();
            dic7.Add("double1", (double)1);
            dic7.Add("string1", "string");
            dic7.Add("boolean1", true);
            dic7.Add("DateTime4", DateTime.Now);
            List<string> list5 = new List<string>();
            list5.Add("6.66");
            list5.Add("test");
            dic7.Add("arrkey4", list5);
            ta.UserSet(dic7);
        }

        public void taoverWriteEvent(Object obj, EventArgs param)
        {
            Dictionary<string, object> dic2 = new Dictionary<string, object>();
            dic2.Add("id", 618834);
            dic2.Add("create_date", Convert.ToDateTime("2019-7-8 20:23:22"));
            dic2.Add("group_no", "T22514");
            dic2.Add("group_title", "【爆款拼装来袭】");
            dic2.Add("group_purchase_id", 438);
            dic2.Add("group_order_is_vip", 3);
            dic2.Add("service_id", 0);
  
            ta.TrackOverwrite("overwriteEventName", "overwriteEventId", dic2);
        }

        public void taupdateEvent(Object obj, EventArgs param)
        {
            Dictionary<string, object> dic2 = new Dictionary<string, object>();
            dic2.Add("id", 618834);
            dic2.Add("create_date", Convert.ToDateTime("2019-7-8 20:23:22"));
            dic2.Add("group_no", "T22514");
            dic2.Add("group_title", "【爆款拼装来袭】");
            dic2.Add("group_purchase_id", 438);
            dic2.Add("group_order_is_vip", 3);
            dic2.Add("service_id", 0);
   
            ta.TrackUpdate("updateEventName", "updateEventId", dic2);
        }

        public void tafirstEvent(Object obj, EventArgs param)
        {
            Dictionary<string, object> dic2 = new Dictionary<string, object>();
            dic2.Add("id", 618834);
            dic2.Add("create_date", Convert.ToDateTime("2019-7-8 20:23:22"));
            dic2.Add("group_no", "T22514");
            dic2.Add("group_title", "【爆款拼装来袭】");
            dic2.Add("group_purchase_id", 438);
            dic2.Add("group_order_is_vip", 3);
            dic2.Add("service_id", 0);

            ta.TrackFirst("firstEventName", "firstEventId", dic2);
            ta.TrackFirst("firstEventName", dic2);
        }
        public void taTrack(Object obj, EventArgs param)
        {

            Dictionary<string, object> dic1 = new Dictionary<string, object>();
            dic1.Add("key", "value");

            Dictionary<string, object> json = new Dictionary<string, object>();
            json.Add("key1", "value1");
            dic1.Add("json", json);

            List<Dictionary<string, object>> arr1 = new List<Dictionary<string, object>>();
            Dictionary<string, object> json1 = new Dictionary<string, object>();
            json1.Add("key2", "value12");
            arr1.Add(json1);
            dic1.Add("jsons", arr1);

            ta.Track("csTrack", dic1);
        }
        public void tacleanOneSuperProperty(Object obj, EventArgs param)
        {
            
        }
        public void tacleanSuperProperty(Object obj, EventArgs param)
        {
            ta.ClearPublicProperties();
        }

        public void tasuperProperty(Object obj, EventArgs param)
        {
            Dictionary<string, object> dic1 = new Dictionary<string, object>();
            dic1.Add("key", "value");
     
            Dictionary<string, object> json = new Dictionary<string, object>();
            json.Add("key1", "value1");
            dic1.Add("json", json);

            List<Dictionary<string, object>> arr1 = new List<Dictionary<string, object>>();
            Dictionary<string, object> json1 = new Dictionary<string, object>();
            json1.Add("key2", "value12");
            arr1.Add(json1);
            dic1.Add("jsons", arr1);

            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("super_id", 12345);
            dic.Add("super_create_date", Convert.ToDateTime("2019-7-8 20:23:22"));
            dic.Add("super_group_no", "T22514");
            dic.Add("group_title", "【爆款拼装来袭super_】");
            dic.Add("super_group_purchase_id", 438);
            dic.Add("super_group_order_is_vip", 3);
            dic.Add("super_service_id", 0);
            dic.Add("super_json", json);
            dic.Add("super_jsons", arr1);
            ta.SetPublicProperties(dic);
        }

        public void tadyldsuperProperty(Object obj, EventArgs param)
        {
            ta.dynamicPropertyAction = this.dynamicPropertyAction;
        }

        public void talogout(Object obj, EventArgs param)
        {
            ta.Logout();
        }
        public void taInit(Object obj, EventArgs param)
        {
            ThinkingdataAnalytics.setLoggingType(TALogging.TALoggingLog);
            ta = new ThinkingdataAnalytics("35a15b58ae934f3994c1abf77910e390", "http://receiver.ta.thinkingdata.cn/");
        }

        public void talogin(Object obj, EventArgs param)
        {
            ta.Login("loginid");
        }

        public void taidentityid(Object obj, EventArgs param)
        {
            ta.SetIdentity("identityid");
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            
        }

        private void Form1_MouseClick_1(object sender, MouseEventArgs e)
        {
   

    

        }

        public Dictionary<string, object> dynamicPropertyAction() {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("super_dynami_id", 618834);
            dic.Add("super_dynami_create_date", Convert.ToDateTime("2019-7-8 20:23:22"));
            dic.Add("super_dynami_group_no", "T22514");
            dic.Add("group_title", "【爆款拼装来袭supe_dynami_】");
            dic.Add("super_dynami_group_purchase_id", 438);
            dic.Add("super_dynami_group_order_is_vip", 3);
            dic.Add("super_dynami_service_id", 0);
            return dic;
        }

    }
}
