using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using HtmlAgilityPack;
using System.Net;
using System.Threading;

namespace Final_Project
{
    public partial class frmMain : Form
    {
        static List<string> url_list = new List<string>();
        static object lock1 = new object();
        int x = 1;


        public frmMain()
        {
            InitializeComponent();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnShow_Click(object sender, EventArgs e)
        {
            StreamReader sr = new StreamReader(@"E:\mehrnews\dataBase.txt");
            string s = sr.ReadToEnd();
            sr.Close();

            DataTable dt = new DataTable();

            dt.Columns.Add("ردیف");
            dt.Columns.Add("لینک خبر");
            dt.Columns.Add("لینک عکس");
            dt.Columns.Add("روز و تاریخ");
            dt.Columns.Add("عنوان خبر");
            dt.Columns.Add("معرفی خبر");
            dt.Columns.Add("شناسه،روز،تاریخ و ساعت");
            dt.Columns.Add("موضوع");
            dt.Columns.Add("چکیده خبر");
            dt.Columns.Add("متن خبر").MaxLength.ToString();

            string[] row = s.Split('$');

            for (int i = 0; i < row.Length - 1; i++)
            {
                string[] coulmn = row[i].Split('#');
                dt.Rows.Add(coulmn[0], coulmn[1], coulmn[2], coulmn[3], coulmn[4], coulmn[5],
                    coulmn[6], coulmn[7], coulmn[8], coulmn[9]);
            }
            dataGridView1.DataSource = dt;
        }

        private void btnCrawl_Click(object sender, EventArgs e)
        {
            toolStripProgressBar1.Value = 0;
            toolStripStatusLabel1.Text = "Crawling Web Pages...";
            crawl();
        }

        private void btnParser_Click(object sender, EventArgs e)
        {
            toolStripProgressBar1.Value = 0;
            toolStripStatusLabel1.Text = "Parsing Data...";
            parsing();
        }


        public void crawl()
        {
            for (int i = 1; i <= 1; i++)
            {
                string url = "http://www.mehrnews.com/page/archive.xhtml?pi=" + i + "&ms=0";
                url_list.Add(url);
            }
            
            string savepath = @"E:\mehrnews\";
            for (int i = 0; i < url_list.Count; i++)
            {
                webcrawling(url_list[i], savepath + (i + 1) + ".html", i + 1);
                toolStripProgressBar1.Value++;
            }

            while (toolStripProgressBar1.Value < 100)
            {
                toolStripProgressBar1.Value++;
            }
            if (toolStripProgressBar1.Value == 100)
                toolStripStatusLabel1.Text = "Crawling Finished";

        }


        public void parsing()
        {

            List<string[]> list = new List<string[]>();
            string[] HtmlFiles = Directory.GetFiles(@"e:\mehrnews\", "*.html");
            for (int i = 0; i < HtmlFiles.Length; i++)
            {
                string file = HtmlFiles[i];
                HtmlAgilityPack.HtmlDocument Doc1 = new HtmlAgilityPack.HtmlDocument();
                Doc1.Load(file, Encoding.UTF8);
                var Temp = Doc1.DocumentNode.SelectNodes("//div");
                HtmlNode Root = null;
                if (Temp != null)
                {
                    for (int j = 0; j < Temp.Count; j++)
                    {
                        for (int k = 0; k < Temp[j].Attributes.Count; j++)
                        {
                            if (Temp[j].Attributes[k].Name == "class" && Temp[j].Attributes[k].Value == "third-news")
                            {
                                Root = Temp[j];
                                break;
                            }
                        }
                        if (Root != null)
                            break;
                    }
                }
                if (Root != null)
                {
                    List<HtmlNode> li_list = new List<HtmlNode>();
                    Queue<HtmlNode> Q = new Queue<HtmlNode>();
                    Q.Enqueue(Root);
                    HtmlNode ul = null;
                    while (Q.Count > 0)
                    {
                        for (int j = 0; j < Q.Peek().ChildNodes.Count; j++)
                        {
                            Q.Enqueue(Q.Peek().ChildNodes[j]);
                        }
                        Q.Dequeue();
                        while (Q.Count > 0)
                        {
                            if (Q.Peek().Name == "ul")
                                ul = Q.Peek();
                            Q.Dequeue();
                        }
                    }//ul tag found
                    Q.Enqueue(ul);
                    while (Q.Count > 0)
                    {
                        int s = 1;
                        if (Q.Peek().Name == "li")
                        {
                            for (int j = 0; j < Q.Peek().Attributes.Count; j++)
                            {
                                if (Q.Peek().Attributes[j].Name == "class" && Q.Peek().Attributes[j].Value == "clearfix")
                                {
                                    li_list.Add(Q.Peek());
                                    s = 0;
                                    break;
                                }
                            }

                        }
                        if (s == 1)
                        {
                            //MessageBox.Show(Q.Peek().ChildNodes.Count.ToString());
                            for (int j = 0; j < Q.Peek().ChildNodes.Count; j++)
                            {
                                Q.Enqueue(Q.Peek().ChildNodes[j]);
                            }
                        }
                        Q.Dequeue();
                    }



                    for (int j = 0; j < li_list.Count; j++)
                    {
                        string[] Array = new string[10];
                        Q.Enqueue(li_list[j]);
                        while (Q.Count > 0)
                        {
                            int t = 0;
                            if (Q.Peek().Name == "a")
                            {
                                for (int k = 0; k < Q.Peek().Attributes.Count; k++)
                                {
                                    if (Q.Peek().Attributes[k].Name == "href")
                                    {
                                        Array[1] = "http://www.mehrnews.com" + Q.Peek().Attributes[k].Value;
                                        break;
                                    }
                                }
                                t = 1;
                                //finding image link
                                Queue<HtmlNode> qt = new Queue<HtmlNode>();
                                for (int k = 0; k < Q.Peek().ChildNodes.Count; k++)
                                    qt.Enqueue(Q.Peek().ChildNodes[k]);
                                HtmlNode span = null;
                                while (qt.Count > 0)
                                {
                                    if (qt.Peek().Name == "span")
                                    {
                                        span = qt.Peek();
                                        break;
                                    }
                                    qt.Dequeue();
                                }
                                while (qt.Count > 0)
                                    qt.Dequeue();
                                qt.Enqueue(span);
                                while (qt.Count > 0)
                                {
                                    int m = 0;
                                    if (qt.Peek().Name == "img")
                                    {
                                        for (int k = 0; k < qt.Peek().Attributes.Count; k++)
                                        {
                                            if (qt.Peek().Attributes[k].Name == "src")
                                            {
                                                Array[2] = qt.Peek().Attributes[k].Value;
                                                m = 1;
                                                break;
                                            }
                                        }
                                    }
                                    if (m == 1)
                                        break;
                                    if (m == 0)
                                    {
                                        for (int k = 0; k < qt.Peek().ChildNodes.Count; k++)
                                            qt.Enqueue(qt.Peek().ChildNodes[k]);
                                    }
                                    qt.Dequeue();
                                }


                            }
                            else if (Q.Peek().Name == "span")
                            {
                                Array[3] = Q.Peek().InnerText;
                                t = 1;
                            }
                            else if (Q.Peek().Name == "h3")
                            {
                                Queue<HtmlNode> qt = new Queue<HtmlNode>();
                                for (int k = 0; k < Q.Peek().ChildNodes.Count; k++)
                                    qt.Enqueue(Q.Peek().ChildNodes[k]);
                                while (qt.Count > 0)
                                {
                                    if (qt.Peek().Name == "a")
                                    {
                                        for (int k = 0; k < qt.Peek().Attributes.Count; k++)
                                        {
                                            if (qt.Peek().Attributes[k].Name == "href")
                                                Array[4] = qt.Peek().InnerText;
                                            break;
                                        }
                                    }
                                    qt.Dequeue();
                                }
                                t = 1;
                            }
                            else if (Q.Peek().Name == "p")
                            {
                                Array[5] = Q.Peek().InnerText;
                                t = 1;

                            }
                            if (t == 0)
                            {
                                for (int k = 0; k < Q.Peek().ChildNodes.Count; k++)
                                    Q.Enqueue(Q.Peek().ChildNodes[k]);
                            }
                            Q.Dequeue();
                        }

                        if (Array[2] == null)
                            Array[2] = "NO IMAGE";
                        Array[0] = x.ToString();
                        string[] s = new string[4];

                        if (Array[1] != null)
                            s = NewsText(Array[1], x);
                        x++;
                        for (int m = 6, n = 0; m <= 9; m++, n++)
                        {
                            if (s == null)
                                break;
                                Array[m] = s[n];
                        }
                        list.Add(Array);
                    }

                }//end of if(Root!=null)

                toolStripProgressBar1.Value++;
                toolStripStatusLabel1.Text = "Page " + i + " Has Parsed.";

            }//end of for (int i = 0; i < HtmlFiles.Length; i++)

            //********************
            string SavePath = @"E:\mehrnews\dataBase.txt";
            StreamWriter sw = new StreamWriter(SavePath);

            for (int j = 0; j < list.Count; j++)
            {
                for (int k = 0; k < 9; k++)
                {
                    sw.Write(list[j][k] + "#");
                }
                sw.Write(list[j][9] + "$");
                sw.WriteLine();
            }

            sw.Close();
            //********************


            while (toolStripProgressBar1.Value < 100)
            {
                toolStripProgressBar1.Value++;
            }
            if (toolStripProgressBar1.Value == 100)
                toolStripStatusLabel1.Text = "Finished";

        }

        public string[] NewsText(string text, int num)
        {
            
            string[] s = null;
            List<string> w = text.Split('/').ToList();
            

            try
            {
                WebProxy wp = new WebProxy();
                wp.UseDefaultCredentials = true;
                WebClient wc = new WebClient();
                wc.Proxy = wp;
                string path = @"E:\mehrnews\links\" + num + " . " + w[w.Count - 1] + ".html";
                wc.DownloadFile(text, path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            string[] HtmlFiles = Directory.GetFiles(@"e:\mehrnews\links\", "*.html");
            string file = null;
            for (int i = 0; i < HtmlFiles.Length; i++)
            {
                if (HtmlFiles[i] == ("e:\\mehrnews\\links\\" + num + " . " + w[w.Count-1] + ".html"))
                    file = HtmlFiles[i];
                //break;
            }

            if (file != null)
            {
                HtmlAgilityPack.HtmlDocument Doc1 = new HtmlAgilityPack.HtmlDocument();
                Doc1.Load(file, Encoding.UTF8);
                var Temp = Doc1.DocumentNode.SelectNodes("//div");
                var t = Doc1.DocumentNode.SelectNodes("//span");

                HtmlNode Root = null, div_meta = null, div_subtitle = null, span = null;
                s = new string[4];
                if (Temp != null)
                {
                    for (int j = 0; j < Temp.Count; j++)
                    {
                        for (int k = 0; k < Temp[j].Attributes.Count; k++)
                        {
                            if (Temp[j].Attributes[k].Name == "class" && Temp[j].Attributes[k].Value == "full-text")
                            {
                                Root = Temp[j];
                            }
                            else if (Temp[j].Attributes[k].Name == "class" && Temp[j].Attributes[k].Value == "meta")
                            {
                                div_meta = Temp[j];
                            }
                            else if (Temp[j].Attributes[k].Name == "class" && Temp[j].Attributes[k].Value == "subtitle")
                            {
                                div_subtitle = Temp[j];
                            }
                        }
                        if (Root != null)
                            break;
                    }
                }
                if (t != null)
                {
                    for (int j = 0; j < t.Count; j++)
                    {
                        for (int k = 0; k < t[j].Attributes.Count; k++)
                        {
                            if (t[j].Attributes[k].Name == "class" && t[j].Attributes[k].Value == "intro-text")
                            {
                                span = t[j];
                            }
                        }
                    }
                }


                if (div_meta != null)
                    s[0] = div_meta.InnerText;
                if (div_subtitle != null)
                    s[1] = div_subtitle.InnerText;
                if (span != null)
                    s[2] = span.InnerText;

                        if (Root != null)
                        {
                            Queue<HtmlNode> Q = new Queue<HtmlNode>();
                            Q.Enqueue(Root);
                            for (int j = 0; j < Q.Peek().ChildNodes.Count; j++)
                            {
                                Q.Enqueue(Q.Peek().ChildNodes[j]);
                            }
                            Q.Dequeue();
                            while (Q.Count > 0)
                            {
                                if (Q.Peek().Name == "p")
                                {
                                    s[3] += Q.Peek().InnerText;
                                }
                                Q.Dequeue();
                            }
                        }

            }
                return s;
            
        }

        public void webcrawling(string url, string path, int i)
        {
            try
            {
                WebProxy wp = new WebProxy();
                wp.UseDefaultCredentials = true;
                WebClient wc = new WebClient();
                wc.Proxy = wp;
                wc.DownloadFile(url, path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void toolStripProgressBar1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

       

        
    }
}
