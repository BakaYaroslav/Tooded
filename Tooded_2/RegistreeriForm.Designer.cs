using Tooded_2.Shop;
namespace Tooded_2
{
    partial class RegistreeriForm
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
            pealkiriLabel = new Label();
            kasutajaLabel = new Label();
            kasutajaEntry = new TextBox();
            paroolLabel = new Label();
            paroolEntry = new TextBox();
            parool2Label = new Label();
            parool2Entry = new TextBox();
            registreeriBtn = new Button();
            tuhistaBtn = new Button();
            SuspendLayout();
            // 
            // pealkiriLabel
            // 
            pealkiriLabel.AutoSize = true;
            pealkiriLabel.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            pealkiriLabel.Location = new Point(100, 20);
            pealkiriLabel.Name = "pealkiriLabel";
            pealkiriLabel.Size = new Size(150, 30);
            pealkiriLabel.TabIndex = 0;
            pealkiriLabel.Text = T.Legacy("Registreerimine");
            // 
            // kasutajaLabel
            // 
            kasutajaLabel.AutoSize = true;
            kasutajaLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            kasutajaLabel.Location = new Point(30, 78);
            kasutajaLabel.Name = "kasutajaLabel";
            kasutajaLabel.Size = new Size(90, 19);
            kasutajaLabel.TabIndex = 1;
            kasutajaLabel.Text = T.Legacy("Kasutajanimi:");
            // 
            // kasutajaEntry
            // 
            kasutajaEntry.Font = new Font("Segoe UI", 10F);
            kasutajaEntry.Location = new Point(170, 75);
            kasutajaEntry.Name = "kasutajaEntry";
            kasutajaEntry.Size = new Size(160, 25);
            kasutajaEntry.TabIndex = 2;
            // 
            // paroolLabel
            // 
            paroolLabel.AutoSize = true;
            paroolLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            paroolLabel.Location = new Point(30, 118);
            paroolLabel.Name = "paroolLabel";
            paroolLabel.Size = new Size(60, 19);
            paroolLabel.TabIndex = 3;
            paroolLabel.Text = T.Legacy("Parool:");
            // 
            // paroolEntry
            // 
            paroolEntry.Font = new Font("Segoe UI", 10F);
            paroolEntry.Location = new Point(170, 115);
            paroolEntry.Name = "paroolEntry";
            paroolEntry.Size = new Size(160, 25);
            paroolEntry.TabIndex = 4;
            paroolEntry.UseSystemPasswordChar = true;
            // 
            // parool2Label
            // 
            parool2Label.AutoSize = true;
            parool2Label.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            parool2Label.Location = new Point(30, 158);
            parool2Label.Name = "parool2Label";
            parool2Label.Size = new Size(120, 19);
            parool2Label.TabIndex = 5;
            parool2Label.Text = T.Legacy("Korda parooli:");
            // 
            // parool2Entry
            // 
            parool2Entry.Font = new Font("Segoe UI", 10F);
            parool2Entry.Location = new Point(170, 155);
            parool2Entry.Name = "parool2Entry";
            parool2Entry.Size = new Size(160, 25);
            parool2Entry.TabIndex = 6;
            parool2Entry.UseSystemPasswordChar = true;
            // 
            // registreeriBtn
            // 
            registreeriBtn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            registreeriBtn.Location = new Point(30, 205);
            registreeriBtn.Name = "registreeriBtn";
            registreeriBtn.Size = new Size(150, 36);
            registreeriBtn.TabIndex = 7;
            registreeriBtn.Text = T.Legacy("Registreeri");
            registreeriBtn.UseVisualStyleBackColor = true;
            registreeriBtn.Click += RegistreeriBtn_Click;
            // 
            // tuhistaBtn
            // 
            tuhistaBtn.Font = new Font("Segoe UI", 10F);
            tuhistaBtn.Location = new Point(190, 205);
            tuhistaBtn.Name = "tuhistaBtn";
            tuhistaBtn.Size = new Size(140, 36);
            tuhistaBtn.TabIndex = 8;
            tuhistaBtn.Text = T.Legacy("Tühista");
            tuhistaBtn.UseVisualStyleBackColor = true;
            tuhistaBtn.Click += TuhistaBtn_Click;
            // 
            // RegistreeriForm
            // 
            AcceptButton = registreeriBtn;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = tuhistaBtn;
            ClientSize = new Size(360, 265);
            Controls.Add(pealkiriLabel);
            Controls.Add(kasutajaLabel);
            Controls.Add(kasutajaEntry);
            Controls.Add(paroolLabel);
            Controls.Add(paroolEntry);
            Controls.Add(parool2Label);
            Controls.Add(parool2Entry);
            Controls.Add(registreeriBtn);
            Controls.Add(tuhistaBtn);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "RegistreeriForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = T.Legacy("Registreeri");
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label pealkiriLabel;
        private System.Windows.Forms.Label kasutajaLabel;
        private System.Windows.Forms.TextBox kasutajaEntry;
        private System.Windows.Forms.Label paroolLabel;
        private System.Windows.Forms.TextBox paroolEntry;
        private System.Windows.Forms.Label parool2Label;
        private System.Windows.Forms.TextBox parool2Entry;
        private System.Windows.Forms.Button registreeriBtn;
        private System.Windows.Forms.Button tuhistaBtn;
    }
}