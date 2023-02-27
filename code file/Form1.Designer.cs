namespace BaiduCSharp1
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.人员管理ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.设备管理 = new System.Windows.Forms.ToolStripMenuItem();
            this.帮助ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Left;
            this.treeView1.Location = new System.Drawing.Point(0, 25);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(135, 364);
            this.treeView1.TabIndex = 0;
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.textBox1.Location = new System.Drawing.Point(135, 209);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(426, 180);
            this.textBox1.TabIndex = 1;
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(135, 25);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(426, 184);
            this.webBrowser1.TabIndex = 2;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.人员管理ToolStripMenuItem,
            this.设备管理,
            this.帮助ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(561, 25);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // 人员管理ToolStripMenuItem
            // 
            this.人员管理ToolStripMenuItem.Name = "人员管理ToolStripMenuItem";
            this.人员管理ToolStripMenuItem.Size = new System.Drawing.Size(68, 21);
            this.人员管理ToolStripMenuItem.Text = "人员管理";
            this.人员管理ToolStripMenuItem.Click += new System.EventHandler(this.人员管理ToolStripMenuItem_Click);
            // 
            // 设备管理
            // 
            this.设备管理.Name = "设备管理";
            this.设备管理.Size = new System.Drawing.Size(68, 21);
            this.设备管理.Text = "设备管理";
            this.设备管理.Click += new System.EventHandler(this.设备管理_Click);
            // 
            // 帮助ToolStripMenuItem
            // 
            this.帮助ToolStripMenuItem.Checked = true;
            this.帮助ToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.帮助ToolStripMenuItem.Name = "帮助ToolStripMenuItem";
            this.帮助ToolStripMenuItem.Size = new System.Drawing.Size(44, 21);
            this.帮助ToolStripMenuItem.Text = "帮助";
            this.帮助ToolStripMenuItem.Click += new System.EventHandler(this.帮助ToolStripMenuItem_Click);
            // 
            // timer2
            // 
            this.timer2.Interval = 1000;
            this.timer2.Tick += new System.EventHandler(this.timer2_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(561, 389);
            this.Controls.Add(this.webBrowser1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.treeView1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "人员定位监控系统";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 人员管理ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 设备管理;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.ToolStripMenuItem 帮助ToolStripMenuItem;
    }
}

