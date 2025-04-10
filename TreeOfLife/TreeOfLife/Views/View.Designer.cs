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
            textBox1 = new TextBox();
            button1 = new Button();
            SuspendLayout();
            // 
            // treeView1
            // 
            treeView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            treeView1.DrawMode = TreeViewDrawMode.OwnerDrawText;
            treeView1.Location = new Point(131, 29);
            treeView1.Name = "treeView1";
            treeView1.Size = new Size(419, 425);
            treeView1.TabIndex = 3;
            treeView1.DrawNode += treeview_ancestor;
            treeView1.AfterSelect += treeView1_Select;
            // 
            // labelInfo
            // 
            labelInfo.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            labelInfo.BorderStyle = BorderStyle.FixedSingle;
            labelInfo.Location = new Point(556, 9);
            labelInfo.Name = "labelInfo";
            labelInfo.Size = new Size(232, 442);
            labelInfo.TabIndex = 1;
            

            // 
            // listBox1
            // 
            listBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 15;
            listBox1.Location = new Point(0, 0);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(131, 454);
            listBox1.TabIndex = 2;
            listBox1.SelectedIndexChanged += listbox_select;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(131, 0);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(419, 23);
            textBox1.TabIndex = 3;
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // button1
            // 
            button1.Location = new Point(475, 29);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 4;
            button1.Text = "reset_tree";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;

            // 
            // View
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 460);
            Controls.Add(button1);
            Controls.Add(textBox1);
            Controls.Add(listBox1);
            Controls.Add(labelInfo);
            Controls.Add(treeView1);
            Name = "View";
            Text = "Tree Of Life ";
            Load += MainView_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBox1;
        private Button button1;
    }
}
