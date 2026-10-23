namespace Tooded_2
{
    partial class Form2
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
            nimetus = new Label();
            kogus = new Label();
            hind = new Label();
            kategooria = new Label();
            toodeEntry = new TextBox();
            kogusEntry = new TextBox();
            hindEntry = new TextBox();
            kategooriaBox = new ComboBox();
            imageList1 = new ImageList(components);
            lisaKategooriaBtn = new Button();
            kustutaKategooriaBtn = new Button();
            otsiFailBtn = new Button();
            toodePilt = new PictureBox();
            lisaBtn = new Button();
            kustutaBtn = new Button();
            uuendaBtn = new Button();
            puhastaBtn = new Button();
            poodBtn = new Button();
            dataGridView1 = new DataGridView();
            muukLabel = new Label();
            ((System.ComponentModel.ISupportInitialize)toodePilt).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // nimetus
            // 
            nimetus.AutoSize = true;
            nimetus.Font = new Font("MS Reference Sans Serif", 12F, FontStyle.Bold);
            nimetus.Location = new Point(46, 35);
            nimetus.Margin = new Padding(4, 0, 4, 0);
            nimetus.Name = "nimetus";
            nimetus.Size = new Size(159, 20);
            nimetus.TabIndex = 0;
            nimetus.Text = "Toode nimetus: ";
            // 
            // kogus
            // 
            kogus.AutoSize = true;
            kogus.Font = new Font("MS Reference Sans Serif", 12F, FontStyle.Bold);
            kogus.Location = new Point(46, 80);
            kogus.Margin = new Padding(4, 0, 4, 0);
            kogus.Name = "kogus";
            kogus.Size = new Size(79, 20);
            kogus.TabIndex = 1;
            kogus.Text = "Kogus: ";
            // 
            // hind
            // 
            hind.AutoSize = true;
            hind.Font = new Font("MS Reference Sans Serif", 12F, FontStyle.Bold);
            hind.Location = new Point(46, 121);
            hind.Margin = new Padding(4, 0, 4, 0);
            hind.Name = "hind";
            hind.Size = new Size(65, 20);
            hind.TabIndex = 2;
            hind.Text = "Hind: ";
            // 
            // kategooria
            // 
            kategooria.AutoSize = true;
            kategooria.Font = new Font("MS Reference Sans Serif", 12F, FontStyle.Bold);
            kategooria.Location = new Point(49, 162);
            kategooria.Margin = new Padding(4, 0, 4, 0);
            kategooria.Name = "kategooria";
            kategooria.Size = new Size(134, 20);
            kategooria.TabIndex = 3;
            kategooria.Text = "Kategooriad: ";
            // 
            // toodeEntry
            // 
            toodeEntry.Location = new Point(196, 32);
            toodeEntry.Margin = new Padding(4, 3, 4, 3);
            toodeEntry.Name = "toodeEntry";
            toodeEntry.Size = new Size(116, 23);
            toodeEntry.TabIndex = 4;
            // 
            // kogusEntry
            // 
            kogusEntry.Location = new Point(195, 76);
            kogusEntry.Margin = new Padding(4, 3, 4, 3);
            kogusEntry.Name = "kogusEntry";
            kogusEntry.Size = new Size(116, 23);
            kogusEntry.TabIndex = 5;
            // 
            // hindEntry
            // 
            hindEntry.Location = new Point(195, 118);
            hindEntry.Margin = new Padding(4, 3, 4, 3);
            hindEntry.Name = "hindEntry";
            hindEntry.Size = new Size(116, 23);
            hindEntry.TabIndex = 6;
            // 
            // kategooriaBox
            // 
            kategooriaBox.FormattingEnabled = true;
            kategooriaBox.Location = new Point(195, 157);
            kategooriaBox.Margin = new Padding(4, 3, 4, 3);
            kategooriaBox.Name = "kategooriaBox";
            kategooriaBox.Size = new Size(140, 23);
            kategooriaBox.TabIndex = 7;
            // 
            // imageList1
            // 
            imageList1.ColorDepth = ColorDepth.Depth8Bit;
            imageList1.ImageSize = new Size(16, 16);
            imageList1.TransparentColor = Color.Transparent;
            // 
            // lisaKategooriaBtn
            // 
            lisaKategooriaBtn.Font = new Font("Microsoft YaHei", 12F, FontStyle.Bold);
            lisaKategooriaBtn.Location = new Point(140, 233);
            lisaKategooriaBtn.Margin = new Padding(4, 3, 4, 3);
            lisaKategooriaBtn.Name = "lisaKategooriaBtn";
            lisaKategooriaBtn.Size = new Size(172, 27);
            lisaKategooriaBtn.TabIndex = 8;
            lisaKategooriaBtn.Text = "Lisa kategooriad";
            lisaKategooriaBtn.UseVisualStyleBackColor = true;
            // 
            // kustutaKategooriaBtn
            // 
            kustutaKategooriaBtn.Font = new Font("Microsoft YaHei", 12F, FontStyle.Bold);
            kustutaKategooriaBtn.Location = new Point(318, 233);
            kustutaKategooriaBtn.Margin = new Padding(4, 3, 4, 3);
            kustutaKategooriaBtn.Name = "kustutaKategooriaBtn";
            kustutaKategooriaBtn.Size = new Size(198, 27);
            kustutaKategooriaBtn.TabIndex = 9;
            kustutaKategooriaBtn.Text = "Kustuta kategooriad";
            kustutaKategooriaBtn.UseVisualStyleBackColor = true;
            // 
            // otsiFailBtn
            // 
            otsiFailBtn.Font = new Font("Microsoft YaHei", 12F, FontStyle.Bold);
            otsiFailBtn.Location = new Point(524, 233);
            otsiFailBtn.Margin = new Padding(4, 3, 4, 3);
            otsiFailBtn.Name = "otsiFailBtn";
            otsiFailBtn.Size = new Size(106, 27);
            otsiFailBtn.TabIndex = 10;
            otsiFailBtn.Text = "Otsi pilt";
            otsiFailBtn.UseVisualStyleBackColor = true;
            // 
            // toodePilt
            // 
            toodePilt.Location = new Point(524, 32);
            toodePilt.Margin = new Padding(4, 3, 4, 3);
            toodePilt.Name = "toodePilt";
            toodePilt.Size = new Size(382, 184);
            toodePilt.TabIndex = 11;
            toodePilt.TabStop = false;
            // 
            // lisaBtn
            // 
            lisaBtn.Font = new Font("Microsoft YaHei", 12F, FontStyle.Bold);
            lisaBtn.Location = new Point(140, 265);
            lisaBtn.Margin = new Padding(4, 3, 4, 3);
            lisaBtn.Name = "lisaBtn";
            lisaBtn.Size = new Size(62, 27);
            lisaBtn.TabIndex = 15;
            lisaBtn.Text = "Lisa";
            lisaBtn.UseVisualStyleBackColor = true;
            lisaBtn.Click += Lisabtn_Click;
            // 
            // kustutaBtn
            // 
            kustutaBtn.Font = new Font("Microsoft YaHei", 12F, FontStyle.Bold);
            kustutaBtn.Location = new Point(293, 265);
            kustutaBtn.Margin = new Padding(4, 3, 4, 3);
            kustutaBtn.Name = "kustutaBtn";
            kustutaBtn.Size = new Size(88, 27);
            kustutaBtn.TabIndex = 16;
            kustutaBtn.Text = "Kustuta";
            kustutaBtn.UseVisualStyleBackColor = true;
            kustutaBtn.Click += KustutaBtn_Click;
            // 
            // uuendaBtn
            // 
            uuendaBtn.Font = new Font("Microsoft YaHei", 12F, FontStyle.Bold);
            uuendaBtn.Location = new Point(209, 265);
            uuendaBtn.Margin = new Padding(4, 3, 4, 3);
            uuendaBtn.Name = "uuendaBtn";
            uuendaBtn.Size = new Size(77, 27);
            uuendaBtn.TabIndex = 17;
            uuendaBtn.Text = "Uuenda";
            uuendaBtn.UseVisualStyleBackColor = true;
            uuendaBtn.Click += UuendaBtn_Click;
            // 
            // puhastaBtn
            // 
            puhastaBtn.Font = new Font("Microsoft YaHei", 12F, FontStyle.Bold);
            puhastaBtn.Location = new Point(387, 265);
            puhastaBtn.Margin = new Padding(4, 3, 4, 3);
            puhastaBtn.Name = "puhastaBtn";
            puhastaBtn.Size = new Size(129, 27);
            puhastaBtn.TabIndex = 18;
            puhastaBtn.Text = "Puhasta";
            puhastaBtn.UseVisualStyleBackColor = true;
            // 
            // poodBtn
            // 
            poodBtn.Font = new Font("Microsoft YaHei", 12F, FontStyle.Bold);
            poodBtn.Location = new Point(638, 233);
            poodBtn.Margin = new Padding(4, 3, 4, 3);
            poodBtn.Name = "poodBtn";
            poodBtn.Size = new Size(253, 61);
            poodBtn.TabIndex = 20;
            poodBtn.Text = "Pood";
            poodBtn.UseVisualStyleBackColor = true;
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(140, 300);
            dataGridView1.Margin = new Padding(4, 3, 4, 3);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(490, 328);
            dataGridView1.TabIndex = 22;
            // 
            // muukLabel
            // 
            muukLabel.AutoSize = true;
            muukLabel.Font = new Font("Microsoft YaHei", 12F, FontStyle.Bold);
            muukLabel.Location = new Point(140, 640);
            muukLabel.Name = "muukLabel";
            muukLabel.Size = new Size(200, 21);
            muukLabel.TabIndex = 23;
            muukLabel.Text = "Müüdud: 0 tk";
            // 
            // Form2
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(933, 680);
            Controls.Add(muukLabel);
            Controls.Add(dataGridView1);
            Controls.Add(poodBtn);
            Controls.Add(puhastaBtn);
            Controls.Add(uuendaBtn);
            Controls.Add(kustutaBtn);
            Controls.Add(lisaBtn);
            Controls.Add(toodePilt);
            Controls.Add(otsiFailBtn);
            Controls.Add(kustutaKategooriaBtn);
            Controls.Add(lisaKategooriaBtn);
            Controls.Add(kategooriaBox);
            Controls.Add(hindEntry);
            Controls.Add(kogusEntry);
            Controls.Add(toodeEntry);
            Controls.Add(kategooria);
            Controls.Add(hind);
            Controls.Add(kogus);
            Controls.Add(nimetus);
            Margin = new Padding(4, 3, 4, 3);
            Name = "Form2";
            Text = "Tooded";
            ((System.ComponentModel.ISupportInitialize)toodePilt).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label nimetus;
        private System.Windows.Forms.Label kogus;
        private System.Windows.Forms.Label hind;
        private System.Windows.Forms.Label kategooria;
        private System.Windows.Forms.TextBox toodeEntry;
        private System.Windows.Forms.TextBox kogusEntry;
        private System.Windows.Forms.TextBox hindEntry;
        private System.Windows.Forms.ComboBox kategooriaBox;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Button lisaKategooriaBtn;
        private System.Windows.Forms.Button kustutaKategooriaBtn;
        private System.Windows.Forms.Button otsiFailBtn;
        private System.Windows.Forms.PictureBox toodePilt;
        private System.Windows.Forms.Button lisaBtn;
        private System.Windows.Forms.Button kustutaBtn;
        private System.Windows.Forms.Button uuendaBtn;
        private System.Windows.Forms.Button puhastaBtn;
        private System.Windows.Forms.Button poodBtn;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Label muukLabel;
    }
}