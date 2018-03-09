using FirstKeJian;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace GoodKeJian
{
    public partial class Form1 : Form
    {
        SynchronizationContext m_SyncContext = null;
        private readonly object Locker = new object();
        private readonly object Locker2 = new object();
        Dictionary<string, string> cookie = new Dictionary<string, string>();
        ConfigFile config;
        public Form1()
        {
            InitializeComponent();
            m_SyncContext = SynchronizationContext.Current;
            config = ConfigFile.LoadOrCreateFile("config.ini");
        }

        public string getProxy()
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("http://localhost:5000/get/");


            HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
            
            Stream stream = webResponse.GetResponseStream(); // 获取接收到的流

            System.IO.StreamReader streamReader = new StreamReader(stream, System.Text.Encoding.UTF8);

            // 建立一个流读取器，可以设置流编码，不设置则默认为UTF-8

            string content = streamReader.ReadToEnd();// 读取流字符串内容

            streamReader.Close();// 关闭相关对象 
            webResponse.Close();
            return content;
        }

        public byte[] GZipDecompress(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (GZipStream gZipStream = new GZipStream(new MemoryStream(data), CompressionMode.Decompress))
                {
                    byte[] bytes = new byte[40960];
                    int n;
                    while ((n = gZipStream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        stream.Write(bytes, 0, n);
                    }
                    gZipStream.Close();
                }
                return stream.ToArray();
            }
        }


        public string GetHtml(string url)

        {
            int flag = 1;
            StringBuilder s = new StringBuilder();
            while (flag == 1)
            {
                try
                {

                    WebClient MyWebClient = new WebClient();
                    MyWebClient.Headers.Add("Accept: */*");
                    MyWebClient.Headers.Add("User-Agent: Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.108 Safari/537.36 gwnet 2345Explorer/8.5.1.15403");
                    MyWebClient.Headers.Add("Accept-Language: zh-cn");
                    MyWebClient.Headers.Add("Content-Type: multipart/form-data");
                    MyWebClient.Headers.Add("Accept-Encoding: gzip, deflate");
                    MyWebClient.Headers.Add("Cache-Control: no-cache");

                    MyWebClient.Credentials = CredentialCache.DefaultCredentials;//获取或设置用于向Internet资源的请求进行身份验证的网络凭据
                    s = new StringBuilder(102400);
                    Byte[] pageData = MyWebClient.DownloadData(url); //从指定网站下载数据
                    GZipStream g = new GZipStream((Stream)(new MemoryStream(pageData)), CompressionMode.Decompress);
                    byte[] d = new byte[20480];
                    int l = g.Read(d, 0, 20480);
                    while (l > 0)
                    {
                        s.Append(Encoding.Default.GetString(d, 0, l));
                        l = g.Read(d, 0, 20480);
                    }
                }
                catch (Exception e)
                {
                    continue;
                }
                flag = 0;
            }
            return s.ToString();
            //  return Encoding.Default.GetString(s.ToString());  //如果获取网站页面采用的是GB2312，则使用这句            

            // return Encoding.UTF8.GetString(s); //如果获取网站页面采用的是UTF-8，则使用这句

        }
        public void HtmlDownload(string softid)
        {

            try
            {
               // string html = GetHtml("http://www.eduwg.com/soft/dl"+softid+".html");
                
               // m_SyncContext.Post(SetTextSafePost, "ID： " + softid + ",免费，开始下载");
               // HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
               // htmlDocument.LoadHtml(html);
               // string id = htmlDocument.DocumentNode.SelectSingleNode("//input[@name='id']").GetAttributeValue("value","");

               // string rnd = htmlDocument.DocumentNode.SelectSingleNode("//input[@name='rnd']").GetAttributeValue("value", "");
               // string downid = htmlDocument.DocumentNode.SelectSingleNode("//input[@name='downid']").GetAttributeValue("value", "");
               // string url= htmlDocument.DocumentNode.SelectSingleNode("//img[@src='/skins/icons/icon_download.gif']").NextSibling.GetAttributeValue("href","");
                //url = url.Replace("&amp;","&");
               // string filename= htmlDocument.DocumentNode.SelectSingleNode("//title").InnerText;
                //string hash = htmlDocument.DocumentNode.SelectSingleNode("//span[@id='down']").PreviousSibling.InnerText;
                //hash = GetHash(hash.Substring(7,hash.Length-9));
                string file = HttpDownloadFile(softid,null );
               
                
            }
            catch (Exception e)
            {
                return;
                //MessageBox.Show(e.Message);
            }
        }

        public void Login()
        {
            HttpWebRequest request;
            HttpWebResponse response;
            request = WebRequest.Create("https://www.ks5u.com/user/inc/UserLogin_Index.asp") as HttpWebRequest;
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
            request.KeepAlive = true;
            //request.Referer = "http://www.jiaokedu.com/kejian/" + softid+".html";
            request.AllowAutoRedirect = true;
            request.Headers.Add("Accept-Encoding", "gzip, deflate");
            request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8,en-US;q=0.6,en;q=0.4");
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";
            string param = string.Format("username=251123458@qq.com&password=123456&c_add=0");
            byte[] postData = Encoding.UTF8.GetBytes(param);
            Stream output = request.GetRequestStream();
            output.Write(postData,0,postData.Length);
            output.Close();
            Cookie c = new Cookie("ASPSESSIONIDSGDDRSAQ", "KGFCABOADEMOJBNJKDEKFNDG");
            c.Domain = "www.ks5u.com";
            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(c);
            c = new Cookie("ASPSESSIONIDQGCAQSDR", "OEIGCBOAOLBIDGHGJAAMOBAF");
            c.Domain = "www.ks5u.com";          
            request.CookieContainer.Add(c);

            //发送请求并获取相应回应数据
            response = request.GetResponse() as HttpWebResponse;

            byte[] bytes = new byte[response.ContentLength];
            response.GetResponseStream().Read(bytes,0,bytes.Length);
            bytes=GZipDecompress(bytes);
            string str = Encoding.GetEncoding("GB2312").GetString(bytes);
            string cook = response.Headers["Set-Cookie"].Split(',').Last().Split(';').First();
            cookie.Clear();
            cookie.Add(cook.Split('=').First(),cook.Split('=').Last());
            response.Close();

        }

        public string HttpDownloadFile(string softid,string filename2)
        {
            // 设置参数
            string path = filepath;
            HttpWebRequest request;
            HttpWebResponse response;
            System.GC.Collect();
            request = WebRequest.Create("https://www.ks5u.com/USER/INC/Downsch.asp?id=" + softid) as HttpWebRequest;
            try
            {
                //string newip = GetLocalIP();
                /* if(!newip.Equals(myip))
                    {
                        myip = newip;
                        Relogin();
                    }*/
                //WebProxy gProxy = new WebProxy(getProxy());
                // string param = string.Format("softid={0}&id={1}&rnd={2}&downid={3}", softid, id,rnd,downid);
                // byte[] postData = Encoding.UTF8.GetBytes(param);
                
                // request.Proxy = gProxy;
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
                request.Method = "GET";
                request.KeepAlive = true;
                //request.Referer = "http://www.jiaokedu.com/kejian/" + softid+".html";
                request.AllowAutoRedirect = true;
                request.Headers.Add("Accept-Encoding", "gzip, deflate");
                request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8,en-US;q=0.6,en;q=0.4");
                request.ContentType = "application/x-www-form-urlencoded";
                request.Timeout = 20000;
                request.AllowAutoRedirect = true;
                //request.ContentLength = 100;
                // Stream output = request.GetRequestStream();
                // output.Write(postData,0,postData.Length);
                // output.Close();
                Cookie c = new Cookie("ASPSESSIONIDSGDDRSAQ", "KGFCABOADEMOJBNJKDEKFNDG");
                c.Domain = "www.ks5u.com";
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(c);
                c = new Cookie("ASPSESSIONIDQGCAQSDR", "OEIGCBOAOLBIDGHGJAAMOBAF");
                c.Domain = "www.ks5u.com";
                request.CookieContainer.Add(c);
                foreach(string key in cookie.Keys)
                {
                    c = new Cookie(key,cookie[key]);
                    c.Domain = "www.ks5u.com";
                    request.CookieContainer.Add(c);
                }

                //发送请求并获取相应回应数据
                response = request.GetResponse() as HttpWebResponse;

                //Dictionary<string,string> dic= FullWebBrowserCookie.GetCookieList(new Uri("http://www.ks5u.com"),false);
                //php下载头与asp不同
                string filename = response.Headers["Content-Disposition"].Split('=').Last();
                
                if (filename == null)
                {
                    response.Close();
                    request.Abort();               
                    return null;
                }
                //filename = filename.Split('/').Last();
                filename = System.Web.HttpUtility.UrlDecode(filename);
                if (filename.Equals("Down.asp"))
                {
                    response.Close();
                    request.Abort();
                    m_SyncContext.Post(SetTextSafePost, filename+" Abort");
                    return null;
                }
                m_SyncContext.Post(SetTextSafePost, filename );
                //long len = Convert.ToInt64(response.Headers["Accept-Length"]);
                long len = response.ContentLength;
                if (len < 2000)
                {
                    return null;
                }
                long curpos = 0;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                Stream responseStream = response.GetResponseStream();
                //创建本地文件写入流
                Stream stream = new FileStream(path+'/' + filename, FileMode.Create);
                byte[] bArr = new byte[4096];
                int size = responseStream.Read(bArr, 0, (int)bArr.Length);
                int count = 0;
                while (size > 0 || curpos < len)
                {
                    curpos += size;
                    stream.Write(bArr, 0, size);
                    size = responseStream.Read(bArr, 0, (int)bArr.Length);
                    if (size == 0) count++;
                    if (count > 100) break;
                }
                stream.Close();
                responseStream.Close();
                response.Close();
                request.Abort();
                return path;
            }
            catch (Exception e)
            {
                request.Abort();
                return null;
            }
            
        }
        int start, end, cnumber,threadcount;
        string filepath;

        private void button1_Click(object sender, EventArgs e)
        {
            start = Convert.ToInt32(textBox1.Text) - 1;
            end = Convert.ToInt32(textBox2.Text);
            int k = Convert.ToInt32(textBox3.Text);
            cnumber = start;
            filepath = label4.Text;
            threadcount = 0;
            System.Net.ServicePointManager.DefaultConnectionLimit = 200;

            Login();

            for (int i = 0; i < k; i++)
            {
                lock (Locker2)
                {
                    threadcount++;
                }
                Thread mythread = new Thread(new ThreadStart(ThreadProcSafePost));
                mythread.Start();
            }

        }

         private void ThreadProcSafePost()
        {
            int temp = 0;

            for (; start < end;)
            {
                lock (Locker)
                {
                    temp = start + 1;
                    start++;
                    m_SyncContext.Post(setStart, start.ToString());
                }

                try
                {
                    HtmlDownload(temp.ToString());
                    //string file = HttpDownloadFile(temp.ToString(), filepath);
                       
                    Thread.Sleep(1000);
                    
                }
                catch (Exception e)
                {

                }
            }
            lock (Locker2)
            {
                threadcount--;

            }
            if (threadcount == 0)
                m_SyncContext.Post(SetTextSafePost, "任务完成");
            //...执行线程其他任务
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择文件路径";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                label4.Text = dialog.SelectedPath;
                filepath = label4.Text;
                //MessageBox.Show("已选择文件夹:" + foldPath, "选择文件夹提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void SetTextSafePost(object text)
        {
            listBox1.Items.Add(text);
            this.listBox1.SelectedIndex = this.listBox1.Items.Count - 1;
            this.listBox1.SelectedIndex = -1;
        }

        private void setStart(object text)
        {
            textBox1.Text = text.ToString();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Exit();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            System.Environment.Exit(0);
        }

        public string GetHash(string hash)
        {
            int i;
            int xa;
            string vreeS7 = "";
            for (i = 0; i < hash.Length; i++)
            {
                xa = (int)hash.ElementAt(i);
                if (xa < 128)
                {
                    xa = xa ^ 7;
        
                }
                vreeS7 += Convert.ToChar(xa);
                if (vreeS7.Length > 80)
                {
                    vreeS7 = "";
        
                 }
        
            }
            return vreeS7;
        }
    }
}
