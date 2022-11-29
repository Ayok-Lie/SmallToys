namespace SmallToys_QRCoder
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.pic = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.textcontent = new System.Windows.Forms.TextBox();
            this.versionTxt = new System.Windows.Forms.ComboBox();
            this.iconsizeCom = new System.Windows.Forms.ComboBox();
            this.pixelCom = new System.Windows.Forms.ComboBox();
            this.iconborderCom = new System.Windows.Forms.ComboBox();
            this.whiteborder = new System.Windows.Forms.RadioButton();
            this.whiteborder1 = new System.Windows.Forms.RadioButton();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.levelCom = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.darkColorBtn = new System.Windows.Forms.Button();
            this.lightColorBtn = new System.Windows.Forms.Button();
            this.colorDialog2 = new System.Windows.Forms.ColorDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.button3 = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.button4 = new System.Windows.Forms.Button();
            this.printDialog1 = new System.Windows.Forms.PrintDialog();
            this.printDocument1 = new System.Drawing.Printing.PrintDocument();
            this.printPreviewDialog1 = new System.Windows.Forms.PrintPreviewDialog();
            ((System.ComponentModel.ISupportInitialize)(this.pic)).BeginInit();
            this.SuspendLayout();
            // 
            // pic
            // 
            this.pic.Location = new System.Drawing.Point(59, 39);
            this.pic.Name = "pic";
            this.pic.Size = new System.Drawing.Size(590, 400);
            this.pic.TabIndex = 0;
            this.pic.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 445);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "版本：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 502);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 20);
            this.label2.TabIndex = 2;
            this.label2.Text = "图片尺寸：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 613);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 20);
            this.label3.TabIndex = 3;
            this.label3.Text = "白边：";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(363, 445);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(54, 20);
            this.label4.TabIndex = 4;
            this.label4.Text = "像素：";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(363, 502);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(84, 20);
            this.label5.TabIndex = 5;
            this.label5.Text = "图片边线：";
            // 
            // textcontent
            // 
            this.textcontent.Location = new System.Drawing.Point(20, 671);
            this.textcontent.Multiline = true;
            this.textcontent.Name = "textcontent";
            this.textcontent.Size = new System.Drawing.Size(600, 135);
            this.textcontent.TabIndex = 6;
            this.textcontent.Text = "初秋";
            // 
            // versionTxt
            // 
            this.versionTxt.FormattingEnabled = true;
            this.versionTxt.Items.AddRange(new object[] {
            "-1",
            "0",
            "1",
            "2",
            "3",
            "4",
            "5"});
            this.versionTxt.Location = new System.Drawing.Point(109, 445);
            this.versionTxt.Name = "versionTxt";
            this.versionTxt.Size = new System.Drawing.Size(151, 28);
            this.versionTxt.TabIndex = 7;
            // 
            // iconsizeCom
            // 
            this.iconsizeCom.FormattingEnabled = true;
            this.iconsizeCom.Items.AddRange(new object[] {
            "5",
            "10",
            "15",
            "20",
            "25",
            "30"});
            this.iconsizeCom.Location = new System.Drawing.Point(110, 502);
            this.iconsizeCom.Name = "iconsizeCom";
            this.iconsizeCom.Size = new System.Drawing.Size(151, 28);
            this.iconsizeCom.TabIndex = 8;
            // 
            // pixelCom
            // 
            this.pixelCom.FormattingEnabled = true;
            this.pixelCom.Items.AddRange(new object[] {
            "10",
            "20",
            "30",
            "40",
            "50",
            "60"});
            this.pixelCom.Location = new System.Drawing.Point(469, 445);
            this.pixelCom.Name = "pixelCom";
            this.pixelCom.Size = new System.Drawing.Size(151, 28);
            this.pixelCom.TabIndex = 9;
            // 
            // iconborderCom
            // 
            this.iconborderCom.FormattingEnabled = true;
            this.iconborderCom.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9"});
            this.iconborderCom.Location = new System.Drawing.Point(469, 502);
            this.iconborderCom.Name = "iconborderCom";
            this.iconborderCom.Size = new System.Drawing.Size(151, 28);
            this.iconborderCom.TabIndex = 10;
            // 
            // whiteborder
            // 
            this.whiteborder.AutoSize = true;
            this.whiteborder.Location = new System.Drawing.Point(109, 613);
            this.whiteborder.Name = "whiteborder";
            this.whiteborder.Size = new System.Drawing.Size(45, 24);
            this.whiteborder.TabIndex = 11;
            this.whiteborder.TabStop = true;
            this.whiteborder.Text = "有";
            this.whiteborder.UseVisualStyleBackColor = true;
            // 
            // whiteborder1
            // 
            this.whiteborder1.AutoSize = true;
            this.whiteborder1.Location = new System.Drawing.Point(215, 613);
            this.whiteborder1.Name = "whiteborder1";
            this.whiteborder1.Size = new System.Drawing.Size(45, 24);
            this.whiteborder1.TabIndex = 12;
            this.whiteborder1.TabStop = true;
            this.whiteborder1.Text = "无";
            this.whiteborder1.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(32, 840);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(94, 29);
            this.button1.TabIndex = 13;
            this.button1.Text = "生成";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(371, 840);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(94, 29);
            this.button2.TabIndex = 14;
            this.button2.Text = "保存";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // levelCom
            // 
            this.levelCom.FormattingEnabled = true;
            this.levelCom.Items.AddRange(new object[] {
            "L",
            "Q",
            "H",
            "M"});
            this.levelCom.Location = new System.Drawing.Point(109, 554);
            this.levelCom.Name = "levelCom";
            this.levelCom.Size = new System.Drawing.Size(151, 28);
            this.levelCom.TabIndex = 15;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(20, 557);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(84, 20);
            this.label6.TabIndex = 16;
            this.label6.Text = "容错等级：";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(363, 565);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(0, 20);
            this.label7.TabIndex = 17;
            // 
            // darkColorBtn
            // 
            this.darkColorBtn.Location = new System.Drawing.Point(369, 561);
            this.darkColorBtn.Name = "darkColorBtn";
            this.darkColorBtn.Size = new System.Drawing.Size(94, 29);
            this.darkColorBtn.TabIndex = 21;
            this.darkColorBtn.Text = "二维码颜色";
            this.darkColorBtn.UseVisualStyleBackColor = true;
            this.darkColorBtn.Click += new System.EventHandler(this.darkColorBtn_Click);
            // 
            // lightColorBtn
            // 
            this.lightColorBtn.Location = new System.Drawing.Point(526, 561);
            this.lightColorBtn.Name = "lightColorBtn";
            this.lightColorBtn.Size = new System.Drawing.Size(94, 29);
            this.lightColorBtn.TabIndex = 22;
            this.lightColorBtn.Text = "二维码底色";
            this.lightColorBtn.UseVisualStyleBackColor = true;
            this.lightColorBtn.Click += new System.EventHandler(this.lightColorBtn_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(369, 613);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(94, 29);
            this.button3.TabIndex = 23;
            this.button3.Text = "自定义图片";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(214, 840);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(94, 29);
            this.button4.TabIndex = 24;
            this.button4.Text = "打印";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // printDialog1
            // 
            this.printDialog1.UseEXDialog = true;
            // 
            // printDocument1
            // 
            this.printDocument1.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.printDocument1_PrintPage);
            // 
            // printPreviewDialog1
            // 
            this.printPreviewDialog1.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.printPreviewDialog1.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.printPreviewDialog1.ClientSize = new System.Drawing.Size(400, 300);
            this.printPreviewDialog1.Enabled = true;
            this.printPreviewDialog1.Icon = ((System.Drawing.Icon)(resources.GetObject("printPreviewDialog1.Icon")));
            this.printPreviewDialog1.Name = "printPreviewDialog1";
            this.printPreviewDialog1.Visible = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CausesValidation = false;
            this.ClientSize = new System.Drawing.Size(673, 881);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.lightColorBtn);
            this.Controls.Add(this.darkColorBtn);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.levelCom);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.whiteborder1);
            this.Controls.Add(this.whiteborder);
            this.Controls.Add(this.iconborderCom);
            this.Controls.Add(this.pixelCom);
            this.Controls.Add(this.iconsizeCom);
            this.Controls.Add(this.versionTxt);
            this.Controls.Add(this.textcontent);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pic);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pic)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private PictureBox pic;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private TextBox textcontent;
        private ComboBox versionTxt;
        private ComboBox iconsizeCom;
        private ComboBox pixelCom;
        private ComboBox iconborderCom;
        private RadioButton whiteborder;
        private RadioButton whiteborder1;
        private Button button1;
        private Button button2;
        private ComboBox levelCom;
        private Label label6;
        private Label label7;
        private ColorDialog colorDialog1;
        private Button darkColorBtn;
        private Button lightColorBtn;
        private ColorDialog colorDialog2;
        private OpenFileDialog openFileDialog1;
        private Button button3;
        private FolderBrowserDialog folderBrowserDialog1;
        private Button button4;
        private PrintDialog printDialog1;
        private System.Drawing.Printing.PrintDocument printDocument1;
        private PrintPreviewDialog printPreviewDialog1;
    }
}