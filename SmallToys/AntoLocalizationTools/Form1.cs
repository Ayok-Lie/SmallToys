using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace AntoLocalizationTools
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        /// <summary>
        /// �Ƿ�ȡ��ͬ
        /// </summary>
        public static bool IsDifferent = false;
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "All Files(*.*)|*.*|txt Files(*.txt)|*.txt";
            openFile.RestoreDirectory = true;
            openFile.Title = "���ļ��Ŀ�";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.FromUrlOld.Text = this.openFileDialog1.FileName;
                this.textBox1.Text = this.FromUrlOld.Text.Substring(FromUrlOld.Text.LastIndexOf("\\") + 1);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "All Files(*.*)|*.*|txt Files(*.txt)|*.txt";
            openFile.RestoreDirectory = true;
            openFile.Title = "���ļ��Ŀ�";
            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                this.FromUrlNew.Text = this.openFileDialog2.FileName;
                this.textBox3.Text = this.FromUrlNew.Text.Substring(FromUrlNew.Text.LastIndexOf("\\") + 1);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.textBox2.Text = "";
            if (this.FromUrlNew.Text != "" && this.FromUrlOld.Text != "")
            {
                try
                {
                    MessageBoxButtons messButton = MessageBoxButtons.YesNo;
                    DialogResult dr = MessageBox.Show("ȷ�ϸ��²�����Դ�ļ���", "��ʾ", messButton);
                    if (dr == DialogResult.OK)
                    {
                        FillTheJson(this.FromUrlOld.Text, Data(this.FromUrlOld.Text, this.FromUrlNew.Text), "Only");
                    }
                    else
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"����Json�ļ��Ƿ�ѡ����ȷ\n\r{ex.ToString()}");
                    Environment.Exit(0);
                }
                this.textBox2.Text = this.FromUrlOld.Text;
                MessageBox.Show("���³ɹ�������");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.textBox2.Text = "";
            if (this.FromUrlNew.Text != "" && this.FromUrlOld.Text != "")
            {
                try
                {
                    FillTheJson(this.FromUrlOld.Text, Data(this.FromUrlOld.Text, this.FromUrlNew.Text), "ALL");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"����Json�ļ��Ƿ�ѡ����ȷ\n\r{ex.ToString()}");
                    Environment.Exit(0);
                }
                this.textBox2.Text = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "//Newjson.json";
                MessageBox.Show($"�ļ�����ɹ�������");
            }
        }

        /// <summary>
        /// �������
        /// </summary>
        /// <param name="oldpath"></param>
        /// <param name="newpath"></param>
        /// <returns></returns>
        private static string Data(string oldpath, string newpath)
        {
            var oldDit = JsonConvert.DeserializeObject<Dictionary<string, string>>(GetFileJson(oldpath));

            var newDit = JsonConvert.DeserializeObject<Dictionary<string, string>>(GetFileJson(newpath));

            var Dit = GetComparison(oldDit!, newDit!);
            var Jsons = JsonConvert.SerializeObject(Dit);
            return Jsons;
        }

        /// <summary>
        /// �������  ȥ��
        /// </summary>
        /// <param name="oldDit"></param>
        /// <param name="newDit"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GetComparison(Dictionary<string, string> oldDit, Dictionary<string, string> newDit)
        {
            var ret = new Dictionary<string, string>();
            if (!IsDifferent)
            {
                ret = oldDit;
            }
            foreach (var add in newDit)
            {
                var isAny = oldDit.Any(x => x.Key == add.Key);
                if (!isAny)
                {
                    ret.TryAdd(add.Key, add.Value);
                }
            }

            return ret;
        }


        /// <summary>
        /// ��ȡjson
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string GetFileJson(string path)
        {
            var ret = "";
            using var file = File.OpenText(path);

            using var reader = new JsonTextReader(file);
            try
            {
                var o = (JObject)JToken.ReadFrom(reader);
                var ageToken = o["texts"];

                if (ageToken != null)
                {
                    ret = ageToken.ToString();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"����Json�ļ��Ƿ�ѡ����ȷ\n\r{e.ToString()}");
                Environment.Exit(0);
            }
            return ret;
        }

        /// <summary>
        /// ���json��ָ��λ��
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        private static void FillTheJson(string path, string data, string type)
        {
            using var file = File.OpenText(path);

            using var reader = new JsonTextReader(file);
            var o = (JObject)JToken.ReadFrom(reader);
            var jsons = JToken.Parse(data);
            o["texts"] = jsons;
            file.Close();
            var Jsons = JsonConvert.SerializeObject(o);
            if (type == "ALL")
            {
                File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "//Newjson.json", Jsons, Encoding.UTF8);
            }
            else
            {
                File.WriteAllText(path, Jsons, Encoding.UTF8);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {

            IsDifferent = true;
            try
            {
                File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "//DifferentJson.json", Data(this.FromUrlOld.Text, this.FromUrlNew.Text), Encoding.UTF8);
                MessageBox.Show($"�������ݻ�ȡ�ɹ�������");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"����Json�ļ��Ƿ�ѡ����ȷ\n\r{ex.ToString()}");
                IsDifferent = false;
                Environment.Exit(0);
            }
            this.textBox2.Text = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "//DifferentJson.json";
            IsDifferent = false;
        }
    }
}