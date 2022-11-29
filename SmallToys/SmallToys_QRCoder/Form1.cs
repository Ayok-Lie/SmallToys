namespace SmallToys_QRCoder
{
    public partial class Form1 : Form
    {
        //��ά����ɫ
        private static Color darkColor = Color.Black;
        //������ɫ
        private static Color lightColor = Color.White;
        //Logo��ַ
        private static string path = "";
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //�汾
            versionTxt.SelectedIndex = 0;
            //���ش�С
            pixelCom.SelectedIndex = 0;
            //ͼƬ��С
            iconsizeCom.SelectedIndex = 0;
            //ͼƬ�߿�
            iconborderCom.SelectedIndex = 0;
            //�ݴ�ȼ�
            levelCom.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //�汾
            int version = Convert.ToInt16(versionTxt.Text);
            //���ش�С
            int pixel = Convert.ToInt16(pixelCom.Text);
            //��ά������
            string msg = textcontent.Text;
            //ͼƬ��С
            int iconSize = Convert.ToInt16(iconsizeCom.Text);
            //ͼƬ�߿�
            int iconBorder = Convert.ToInt16(iconborderCom.Text);
            //��ά��հױ߿�
            bool Iswhiteborder = whiteborder.Checked ? true : false;
            //�ݴ�ȼ�
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
            //��ʾ��ɫ�Ի���
            DialogResult dr = colorDialog1.ShowDialog();
            if (dr == DialogResult.OK)
            {
                darkColor = colorDialog1.Color;
            }
        }

        private void lightColorBtn_Click(object sender, EventArgs e)
        {
            //��ʾ��ɫ�Ի���
            DialogResult dr = colorDialog2.ShowDialog();
            if (dr == DialogResult.OK)
            {
                lightColor = colorDialog2.Color;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var dr = new OpenFileDialog();
            //f.Multiselect = true; //��ѡ
            if (dr.ShowDialog() == DialogResult.OK)
            {
                path = dr.FileName;
                String filename = dr.SafeFileName;
            }
            //ѡ���ļ���
            //FolderBrowserDialog dr = new FolderBrowserDialog();
            //if (dr.ShowDialog() == DialogResult.OK)
            //{
            //    String DirPath = dr.SelectedPath;
            //}
        }

        private void button4_Click(object sender, EventArgs e)
        {
            button4.Visible = false;//�ڴ�ӡʱ���ذ�ť
            printDialog1.Document = printDocument1;
            if (printDialog1.ShowDialog() == DialogResult.OK)
            {
                printDocument1.Print();
            }
            button4.Visible = true;//��ӡ��ɻظ���ť��ʾ
        }

        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            Bitmap _NewBitmap = new Bitmap(pic.Width, pic.Height);
            pic.DrawToBitmap(_NewBitmap, new Rectangle(0, 0, _NewBitmap.Width, _NewBitmap.Height));
            e.Graphics.DrawImage(_NewBitmap, 0, 0, _NewBitmap.Width, _NewBitmap.Height);
        }
    }
}