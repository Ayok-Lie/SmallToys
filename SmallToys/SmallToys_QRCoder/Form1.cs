namespace SmallToys_QRCoder
{
    public partial class Form1 : Form
    {
        //二维码颜色
        private static Color darkColor = Color.Black;
        //背景颜色
        private static Color lightColor = Color.White;
        //Logo地址
        private static string path = "";
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //版本
            versionTxt.SelectedIndex = 0;
            //像素大小
            pixelCom.SelectedIndex = 0;
            //图片大小
            iconsizeCom.SelectedIndex = 0;
            //图片边框
            iconborderCom.SelectedIndex = 0;
            //容错等级
            levelCom.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //版本
            int version = Convert.ToInt16(versionTxt.Text);
            //像素大小
            int pixel = Convert.ToInt16(pixelCom.Text);
            //二维码内容
            string msg = textcontent.Text;
            //图片大小
            int iconSize = Convert.ToInt16(iconsizeCom.Text);
            //图片边框
            int iconBorder = Convert.ToInt16(iconborderCom.Text);
            //二维码空白边框
            bool Iswhiteborder = whiteborder.Checked ? true : false;
            //容错等级
            string level = levelCom.Text;
            Bitmap bitmap = QRCoderHelper.generateQrCode(msg, level, version, pixel, darkColor, lightColor, path, iconSize, iconBorder, Iswhiteborder);
            pic.Image = bitmap;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (pic.Image != null)

                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "(*.png)|*.png|(*.bmp)|*.bmp";

                    if (sfd.ShowDialog() == DialogResult.OK) pic.Image.Save(sfd.FileName);

                }
        }

        private void darkColorBtn_Click(object sender, EventArgs e)
        {
            //显示颜色对话框
            DialogResult dr = colorDialog1.ShowDialog();
            if (dr == DialogResult.OK)
            {
                darkColor = colorDialog1.Color;
            }
        }

        private void lightColorBtn_Click(object sender, EventArgs e)
        {
            //显示颜色对话框
            DialogResult dr = colorDialog2.ShowDialog();
            if (dr == DialogResult.OK)
            {
                lightColor = colorDialog2.Color;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var dr = new OpenFileDialog();
            //f.Multiselect = true; //多选
            if (dr.ShowDialog() == DialogResult.OK)
            {
                path = dr.FileName;
                String filename = dr.SafeFileName;
            }
            //选择文件夹
            //FolderBrowserDialog dr = new FolderBrowserDialog();
            //if (dr.ShowDialog() == DialogResult.OK)
            //{
            //    String DirPath = dr.SelectedPath;
            //}
        }

        private void button4_Click(object sender, EventArgs e)
        {
            button4.Visible = false;//在打印时隐藏按钮
            printDialog1.Document = printDocument1;
            if (printDialog1.ShowDialog() == DialogResult.OK)
            {
                printDocument1.Print();
            }
            button4.Visible = true;//打印完成回复按钮显示
        }

        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            Bitmap _NewBitmap = new Bitmap(pic.Width, pic.Height);
            pic.DrawToBitmap(_NewBitmap, new Rectangle(0, 0, _NewBitmap.Width, _NewBitmap.Height));
            e.Graphics.DrawImage(_NewBitmap, 0, 0, _NewBitmap.Width, _NewBitmap.Height);
        }
    }
}