namespace Tooded_2
{
    partial class PoodForm
    {
        private System.ComponentModel.IContainer components = null;

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
            components = new System.ComponentModel.Container();
            topPanel = new Panel();
            otsingLabel = new Label();
            otsingEntry = new TextBox();
            kategooriaLabel = new Label();
            kategooriaBox = new ComboBox();
            flowPanel = new FlowLayoutPanel();
            bottomPanel = new Panel();
            kokkuLabel = new Label();
            ostanBtn = new Button();
            makstaBtn = new Button();
            saadaArveBtn = new Button();
            sulgeBtn = new Button();
            topPanel.SuspendLayout();
            bottomPanel.SuspendLayout();
            SuspendLayout();
            // 
            // topPanel
            // 
            topPanel.BackColor = Color.White;
            topPanel.Controls.Add(otsingLabel);
            topPanel.Controls.Add(otsingEntry);
            topPanel.Controls.Add(kategooriaLabel);
            topPanel.Controls.Add(kategooriaBox);
            topPanel.Dock = DockStyle.Top;
            topPanel.Name = "topPanel";
            topPanel.Size = new Size(984, 55);
            topPanel.TabIndex = 1;
            // 
            // otsingLabel
            // 
            otsingLabel.AutoSize = true;
            otsingLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            otsingLabel.Location = new Point(15, 18);
            otsingLabel.Name = "otsingLabel";
            otsingLabel.Size = new Size(41, 19);
            otsingLabel.TabIndex = 0;
            otsingLabel.Text = "Otsi:";
            // 
            // otsingEntry
            // 
            otsingEntry.Font = new Font("Segoe UI", 10F);
            otsingEntry.Location = new Point(70, 15);
            otsingEntry.Name = "otsingEntry";
            otsingEntry.Size = new Size(220, 25);
            otsingEntry.TabIndex = 1;
            otsingEntry.TextChanged += OtsingEntry_TextChanged;
            // 
            // kategooriaLabel
            // 
            kategooriaLabel.AutoSize = true;
            kategooriaLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            kategooriaLabel.Location = new Point(320, 18);
            kategooriaLabel.Name = "kategooriaLabel";
            kategooriaLabel.Size = new Size(81, 19);
            kategooriaLabel.TabIndex = 2;
            kategooriaLabel.Text = "Kategooria:";
            // 
            // kategooriaBox
            // 
            kategooriaBox.DropDownStyle = ComboBoxStyle.DropDownList;
            kategooriaBox.Font = new Font("Segoe UI", 10F);
            kategooriaBox.FormattingEnabled = true;
            kategooriaBox.Location = new Point(420, 15);
            kategooriaBox.Name = "kategooriaBox";
            kategooriaBox.Size = new Size(200, 25);
            kategooriaBox.TabIndex = 3;
            kategooriaBox.SelectedIndexChanged += KategooriaBox_SelectedIndexChanged;
            // 
            // flowPanel
            // 
            flowPanel.AutoScroll = true;
            flowPanel.Dock = DockStyle.Fill;
            flowPanel.Name = "flowPanel";
            flowPanel.Padding = new Padding(10);
            flowPanel.Size = new Size(984, 536);
            flowPanel.TabIndex = 0;
            // 
            // bottomPanel
            // 
            bottomPanel.BackColor = Color.White;
            bottomPanel.Controls.Add(kokkuLabel);
            bottomPanel.Controls.Add(ostanBtn);
            bottomPanel.Controls.Add(makstaBtn);
            bottomPanel.Controls.Add(saadaArveBtn);
            bottomPanel.Controls.Add(sulgeBtn);
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Name = "bottomPanel";
            bottomPanel.Size = new Size(984, 60);
            bottomPanel.TabIndex = 2;
            // 
            // kokkuLabel
            // 
            kokkuLabel.AutoSize = true;
            kokkuLabel.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            kokkuLabel.Location = new Point(15, 18);
            kokkuLabel.Name = "kokkuLabel";
            kokkuLabel.Size = new Size(120, 20);
            kokkuLabel.TabIndex = 0;
            kokkuLabel.Text = "Valitud: 0";
            // 
            // ostanBtn
            // 
            ostanBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ostanBtn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            ostanBtn.Location = new Point(500, 12);
            ostanBtn.Name = "ostanBtn";
            ostanBtn.Size = new Size(110, 36);
            ostanBtn.TabIndex = 1;
            ostanBtn.Text = "Ostan";
            ostanBtn.UseVisualStyleBackColor = true;
            ostanBtn.Click += OstanBtn_Click;
            // 
            // makstaBtn
            // 
            makstaBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            makstaBtn.Enabled = false;
            makstaBtn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            makstaBtn.Location = new Point(620, 12);
            makstaBtn.Name = "makstaBtn";
            makstaBtn.Size = new Size(110, 36);
            makstaBtn.TabIndex = 2;
            makstaBtn.Text = "Maksta";
            makstaBtn.UseVisualStyleBackColor = true;
            makstaBtn.Click += MakstaBtn_Click;
            // 
            // saadaArveBtn
            // 
            saadaArveBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            saadaArveBtn.Enabled = false;
            saadaArveBtn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            saadaArveBtn.Location = new Point(740, 12);
            saadaArveBtn.Name = "saadaArveBtn";
            saadaArveBtn.Size = new Size(120, 36);
            saadaArveBtn.TabIndex = 3;
            saadaArveBtn.Text = "Saada arve";
            saadaArveBtn.UseVisualStyleBackColor = true;
            saadaArveBtn.Click += SaadaArveBtn_Click;
            // 
            // sulgeBtn
            // 
            sulgeBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            sulgeBtn.Font = new Font("Segoe UI", 10F);
            sulgeBtn.Location = new Point(870, 12);
            sulgeBtn.Name = "sulgeBtn";
            sulgeBtn.Size = new Size(100, 36);
            sulgeBtn.TabIndex = 4;
            sulgeBtn.Text = "Sulge";
            sulgeBtn.UseVisualStyleBackColor = true;
            sulgeBtn.Click += SulgeBtn_Click;
            // 
            // PoodForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.WhiteSmoke;
            ClientSize = new Size(984, 651);
            Controls.Add(flowPanel);
            Controls.Add(topPanel);
            Controls.Add(bottomPanel);
            Name = "PoodForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Pood";
            topPanel.ResumeLayout(false);
            topPanel.PerformLayout();
            bottomPanel.ResumeLayout(false);
            bottomPanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel topPanel;
        private System.Windows.Forms.Label otsingLabel;
        private System.Windows.Forms.TextBox otsingEntry;
        private System.Windows.Forms.Label kategooriaLabel;
        private System.Windows.Forms.ComboBox kategooriaBox;
        private System.Windows.Forms.FlowLayoutPanel flowPanel;
        private System.Windows.Forms.Panel bottomPanel;
        private System.Windows.Forms.Label kokkuLabel;
        private System.Windows.Forms.Button ostanBtn;
        private System.Windows.Forms.Button makstaBtn;
        private System.Windows.Forms.Button saadaArveBtn;
        private System.Windows.Forms.Button sulgeBtn;
    }
}