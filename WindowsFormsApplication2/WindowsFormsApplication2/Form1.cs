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
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            
        }

        private void Form1_MouseClick_1(object sender, MouseEventArgs e)
        {
            this.BackColor = Color.Red;

            ThinkingdataAnalytics.setLoggingType(TALogging.TALoggingLog);

            ThinkingdataAnalytics ta = new ThinkingdataAnalytics("35a15b58ae934f3994c1abf77910e390", "http://receiver.ta.thinkingdata.cn/");

            Dictionary<string, object> dic1 = new Dictionary<string, object>();
            dic1.Add("key", "value");

            // 复杂数据对象
            Dictionary<string, object> json = new Dictionary<string, object>();
            json.Add("key1", "value1");
            dic1.Add("json", json);

            // 复杂数据对象组
            List<Dictionary<string, object>> arr1 = new List<Dictionary<string, object>>();
            Dictionary<string, object> json1 = new Dictionary<string, object>();
            json1.Add("key2", "value12");
            arr1.Add(json1);
            dic1.Add("jsons", arr1);

            // 公共属性
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

            ta.Logout();
            /*

             ta.ClearPublicProperties();
            ta.dynamicPropertyAction = this.dynamicPropertyAction;
            */

            ta.Track("csTrack", dic1);

             /*
            ta.SetIdentity("identityid");
            ta.Login("loginid");
            
           
            Dictionary<string, object> dic2 = new Dictionary<string, object>();
            dic2.Add("id", 618834);
            dic2.Add("create_date", Convert.ToDateTime("2019-7-8 20:23:22"));
            dic2.Add("group_no", "T22514");
            dic2.Add("group_title", "【爆款拼装来袭】");
            dic2.Add("group_purchase_id", 438);
            dic2.Add("group_order_is_vip", 3);
            dic2.Add("service_id", 0);
            ta.Track( "testEventName2", dic2);

            ta.TrackUpdate("updateEventName", "updateEventId", dic2);
            ta.TrackOverwrite( "overwriteEventName", "overwriteEventId", dic2);
            ta.TrackFirst("firstEventName", "firstEventId", dic2);
            ta.TrackFirst("firstEventName", dic2);
            
            //传入用户属性，只能是数值型的，如果传用户属性，请用UserSet
            Dictionary<string, object> dic4 = new Dictionary<string, object>();
            dic4.Add("TotalRevenue", 648);
            ta.UserAdd( dic4);

            Dictionary<string, object> dic5 = new Dictionary<string, object>();
            dic5.Add("login_name", "皮2");
            dic5.Add("#time", new DateTime(2019, 12, 10, 15, 12, 11, 444));
            dic5.Add("#ip", "192.168.1.1");
            //dic5.Add("#uuid",Guid.NewGuid().ToString("D")); 上传#uuid为标准格式(8-4-4-4-12)的string,服务端比较稳定，可不上传
            ta.UserSetOnce(dic5);

            //删除这个用户的某个属性 必须是string类型的集合，例如：
            List<string> list2 = new List<string>();
            list2.Add("nickname");
            list2.Add("age");
            ta.UserUnSet(list2);
 
            //user_append,追加集合属性
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            List<string> list6 = new List<string>();
            list6.Add("true");
            list6.Add("test");
            dictionary.Add("arrkey4", list6);
            ta.UserAppend( dictionary);

            // user set
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

            // user delete
            // ta.UserDelete();
            */

            //刷新数据，立即上报
            ta.Flush();

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
