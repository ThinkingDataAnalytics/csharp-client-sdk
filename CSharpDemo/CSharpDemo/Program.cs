using ThinkingData.Analytics;

namespace CSharpDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            string appId = "app_id";
            string serverUrl = "server_url";
            
            // 开启文本日志
            TDAnalytics.EnableLogType(TDLogType.LogTxt);
            //初始化SDK
            TDAnalytics.Init(appId, serverUrl);
            //如果用户已登录，可以设置用户的账号ID作为身份唯一标识
            TDAnalytics.Login("TA");
            //发送事件
            Dictionary<string, Object> dic = new Dictionary<string, object>();
            dic.Add("channel", "ta");//字符串
            dic.Add("id", 618834);//数字
            dic.Add("isSuccess", true);//布尔
            dic.Add("create_date", Convert.ToDateTime("2019-7-8 20:23:22"));//时间
            List<string> arr = new List<string>();
            arr.Add("value");
            dic.Add("arr", arr);//数组
            TDAnalytics.Track("product_buy", dic);
            
            //设置用户属性
            TDAnalytics.UserSet(new Dictionary<string, object>() { { "user_name", "TA" } });
            // 立即上报数据
            TDAnalytics.Flush();

            Console.ReadKey();
        }
    }
}