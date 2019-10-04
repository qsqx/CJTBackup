using System;

namespace CJTBackup
{
    partial class FormMain
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.backupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.restoreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logTextBox = new System.Windows.Forms.RichTextBox();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.backupToolStripMenuItem,
            this.restoreToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 32);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // backupToolStripMenuItem
            // 
            this.backupToolStripMenuItem.Name = "backupToolStripMenuItem";
            this.backupToolStripMenuItem.Size = new System.Drawing.Size(62, 28);
            this.backupToolStripMenuItem.Text = "备份";
            this.backupToolStripMenuItem.Click += new System.EventHandler(this.backupToolStripMenuItem_Click);
            // 
            // restoreToolStripMenuItem
            // 
            this.restoreToolStripMenuItem.Name = "restoreToolStripMenuItem";
            this.restoreToolStripMenuItem.Size = new System.Drawing.Size(62, 28);
            this.restoreToolStripMenuItem.Text = "恢复";
            this.restoreToolStripMenuItem.Click += new System.EventHandler(this.restoreToolStripMenuItem_Click);
            // 
            // logTextBox
            // 
            this.logTextBox.Location = new System.Drawing.Point(0, 35);
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.ReadOnly = true;
            this.logTextBox.Size = new System.Drawing.Size(800, 416);
            this.logTextBox.TabIndex = 1;
            this.logTextBox.Text = "";
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.logTextBox);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FormMain";
            this.Text = "畅捷通T3备份工具";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem backupToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem restoreToolStripMenuItem;

        private Mgr.Logger GetLogger()
        {
            return (text) => {
                Action action = new Action(() => {
                    logTextBox.AppendText(text);
                });

                if (logTextBox.InvokeRequired)
                {
                    logTextBox.Invoke(action);
                }
                else
                {
                    action.Invoke();
                }
            };
        }

        private void backupToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            System.Windows.Forms.SaveFileDialog path = new System.Windows.Forms.SaveFileDialog();
            path.Filter = "ZIP压缩包|*.zip";
            path.ShowDialog();

            if (path.FileName == "")
                return;

            Mgr mgr = new Mgr(GetLogger());
            new System.Threading.Thread(() => { mgr.Backup(path.FileName); }).Start();
        }

        private void restoreToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            System.Windows.Forms.OpenFileDialog path = new System.Windows.Forms.OpenFileDialog();
            path.Filter = "ZIP压缩包|*.zip";
            path.ShowDialog();
            if (path.FileName == "")
                return;

            Mgr mgr = new Mgr(GetLogger());
            new System.Threading.Thread(() => { mgr.Restore(path.FileName); }).Start();
        }

        private System.Windows.Forms.RichTextBox logTextBox;
    }
}

