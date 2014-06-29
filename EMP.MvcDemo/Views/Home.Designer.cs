namespace EMP.MvcDemo.Views
{
    partial class Home
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
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.基础ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEmp = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSubView = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.基础ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(735, 25);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // 基础ToolStripMenuItem
            // 
            this.基础ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuEmp,
            this.mnuSubView});
            this.基础ToolStripMenuItem.Name = "基础ToolStripMenuItem";
            this.基础ToolStripMenuItem.Size = new System.Drawing.Size(44, 21);
            this.基础ToolStripMenuItem.Text = "基础";
            // 
            // mnuEmp
            // 
            this.mnuEmp.Name = "mnuEmp";
            this.mnuEmp.Size = new System.Drawing.Size(152, 22);
            this.mnuEmp.Text = "emp";
            // 
            // mnuSubView
            // 
            this.mnuSubView.Name = "mnuSubView";
            this.mnuSubView.Size = new System.Drawing.Size(152, 22);
            this.mnuSubView.Text = "sub view";
            // 
            // Home
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(735, 479);
            this.Controls.Add(this.menuStrip1);
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Home";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Home_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 基础ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mnuEmp;
        private System.Windows.Forms.ToolStripMenuItem mnuSubView;

    }
}

