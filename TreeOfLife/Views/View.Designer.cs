namespace TreeOfLifeVisualization.Views
{
    partial class View
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.ListBox listBox1;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            treeView1 = new TreeView();
            labelInfo = new Label();
            listBox1 = new ListBox();
            SuspendLayout();
            // 
            // treeView1
            // 
            treeView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left;
            treeView1.Location = new Point(131, 0);
            treeView1.Name = "treeView1";
            treeView1.Size = new Size(672, 460);
            treeView1.TabIndex = 0;
            treeView1.AfterSelect += treeView1_Select;
            // 
            // labelInfo
            // 
            labelInfo.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right ;
            labelInfo.BorderStyle = BorderStyle.FixedSingle;
            labelInfo.Location = new Point(556, 12);
            labelInfo.Name = "labelInfo";
            labelInfo.Size = new Size(232, 440);
            labelInfo.TabIndex = 1;
            // 
            // listBox1
            // 
            listBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 15;
            listBox1.Location = new Point(0, 0);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(131, 467);
            listBox1.TabIndex = 2;
            // 
            // MainView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 460);
            Controls.Add(listBox1);
            Controls.Add(labelInfo);
            Controls.Add(treeView1);
            Name = "MainView";
            Text = "Tree Of Life Visualization";
            Load += MainView_Load;
            ResumeLayout(false);
        }

        #endregion
    }
}
