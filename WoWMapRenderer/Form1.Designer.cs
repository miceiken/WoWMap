namespace WoWMapRenderer
{
    partial class Form1
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.GL = new OpenTK.GLControl();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.loadCASCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.localToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.onlineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this._backgroundTaskProgress = new System.Windows.Forms.ToolStripProgressBar();
            this._feedbackText = new System.Windows.Forms.ToolStripStatusLabel();
            this._cameraPos = new System.Windows.Forms.ToolStripStatusLabel();
            this._mapListBox = new System.Windows.Forms.ListBox();
            this.filterBox = new System.Windows.Forms.TextBox();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // GL
            // 
            this.GL.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GL.BackColor = System.Drawing.Color.White;
            this.GL.Location = new System.Drawing.Point(210, 27);
            this.GL.Name = "GL";
            this.GL.Size = new System.Drawing.Size(781, 437);
            this.GL.TabIndex = 0;
            this.GL.VSync = false;
            this.GL.Load += new System.EventHandler(this.OnRenderLoaded);
            this.GL.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaint);
            this.GL.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.OnKeyPress);
            this.GL.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
            this.GL.Resize += new System.EventHandler(this.OnRenderResize);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadCASCToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1003, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // loadCASCToolStripMenuItem
            // 
            this.loadCASCToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.localToolStripMenuItem,
            this.onlineToolStripMenuItem});
            this.loadCASCToolStripMenuItem.Name = "loadCASCToolStripMenuItem";
            this.loadCASCToolStripMenuItem.Size = new System.Drawing.Size(90, 20);
            this.loadCASCToolStripMenuItem.Text = "Load CASC ...";
            // 
            // localToolStripMenuItem
            // 
            this.localToolStripMenuItem.Name = "localToolStripMenuItem";
            this.localToolStripMenuItem.Size = new System.Drawing.Size(109, 22);
            this.localToolStripMenuItem.Text = "Local";
            this.localToolStripMenuItem.Click += new System.EventHandler(this.LoadLocalCASC);
            // 
            // onlineToolStripMenuItem
            // 
            this.onlineToolStripMenuItem.Name = "onlineToolStripMenuItem";
            this.onlineToolStripMenuItem.Size = new System.Drawing.Size(109, 22);
            this.onlineToolStripMenuItem.Text = "Online";
            this.onlineToolStripMenuItem.Click += new System.EventHandler(this.LoadOnlineCASC);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._backgroundTaskProgress,
            this._feedbackText,
            this._cameraPos});
            this.statusStrip1.Location = new System.Drawing.Point(0, 476);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1003, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // _backgroundTaskProgress
            // 
            this._backgroundTaskProgress.Name = "_backgroundTaskProgress";
            this._backgroundTaskProgress.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this._backgroundTaskProgress.Size = new System.Drawing.Size(100, 16);
            this._backgroundTaskProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // _feedbackText
            // 
            this._feedbackText.Name = "_feedbackText";
            this._feedbackText.Size = new System.Drawing.Size(0, 17);
            // 
            // _cameraPos
            // 
            this._cameraPos.Name = "_cameraPos";
            this._cameraPos.Size = new System.Drawing.Size(0, 17);
            // 
            // _mapListBox
            // 
            this._mapListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this._mapListBox.FormattingEnabled = true;
            this._mapListBox.Location = new System.Drawing.Point(13, 57);
            this._mapListBox.Name = "_mapListBox";
            this._mapListBox.Size = new System.Drawing.Size(191, 407);
            this._mapListBox.TabIndex = 3;
            this._mapListBox.SelectedIndexChanged += new System.EventHandler(this.MapSelected);
            // 
            // filterBox
            // 
            this.filterBox.Location = new System.Drawing.Point(13, 28);
            this.filterBox.Name = "filterBox";
            this.filterBox.Size = new System.Drawing.Size(191, 20);
            this.filterBox.TabIndex = 4;
            this.filterBox.TextChanged += new System.EventHandler(this.FilterChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1003, 498);
            this.Controls.Add(this.filterBox);
            this.Controls.Add(this._mapListBox);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.GL);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.OnLoad);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private OpenTK.GLControl GL;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem loadCASCToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem localToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem onlineToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel _feedbackText;
        private System.Windows.Forms.ListBox _mapListBox;
        private System.Windows.Forms.TextBox filterBox;
        private System.Windows.Forms.ToolStripProgressBar _backgroundTaskProgress;
        private System.Windows.Forms.ToolStripStatusLabel _cameraPos;
    }
}

