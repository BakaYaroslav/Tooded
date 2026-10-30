using Tooded_2.Shop;
namespace Tooded_2
{
    partial class LoginForm
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
            loginBtn = new Button();
            registreeriBtn = new Button();
            SuspendLayout();
            // 
            // pealkiriLabel
            // 
            pealkiriLabel.AutoSize = true;
            pealkiriLabel.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            pealkiriLabel.Location = new Point(110, 20);
            pealkiriLabel.Name = "pealkiriLabel";
            pealkiriLabel.Size = new Size(130, 30);
            pealkiriLabel.TabIndex = 0;
            pealkiriLabel.Text = T.Legacy("Sisselogimine");
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
            kasutajaEntry.Location = new Point(140, 75);
            kasutajaEntry.Name = "kasutajaEntry";
            kasutajaEntry.Size = new Size(180, 25);
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
            paroolEntry.Location = new Point(140, 115);
            paroolEntry.Name = "paroolEntry";
            paroolEntry.Size = new Size(180, 25);
            paroolEntry.TabIndex = 4;
            paroolEntry.UseSystemPasswordChar = true;
            // 
            // loginBtn
            // 
            loginBtn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            loginBtn.Location = new Point(30, 165);
            loginBtn.Name = "loginBtn";
            loginBtn.Size = new Size(140, 36);
            loginBtn.TabIndex = 5;
            loginBtn.Text = T.Legacy("Logi sisse");
            loginBtn.UseVisualStyleBackColor = true;
            loginBtn.Click += LoginBtn_Click;
            // 
            // registreeriBtn
            // 
            registreeriBtn.Font = new Font("Segoe UI", 10F);
            registreeriBtn.Location = new Point(180, 165);
            registreeriBtn.Name = "registreeriBtn";
            registreeriBtn.Size = new Size(140, 36);
            registreeriBtn.TabIndex = 6;
            registreeriBtn.Text = T.Legacy("Registreeri");
            registreeriBtn.UseVisualStyleBackColor = true;
            registreeriBtn.Click += RegistreeriBtn_Click;
            // 
            // LoginForm
            // 
            AcceptButton = loginBtn;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(350, 225);
            Controls.Add(pealkiriLabel);
            Controls.Add(kasutajaLabel);
            Controls.Add(kasutajaEntry);
            Controls.Add(paroolLabel);
            Controls.Add(paroolEntry);
            Controls.Add(loginBtn);
            Controls.Add(registreeriBtn);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "LoginForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = T.Legacy("Logi sisse");
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label pealkiriLabel;
        private System.Windows.Forms.Label kasutajaLabel;
        private System.Windows.Forms.TextBox kasutajaEntry;
        private System.Windows.Forms.Label paroolLabel;
        private System.Windows.Forms.TextBox paroolEntry;
        private System.Windows.Forms.Button loginBtn;
        private System.Windows.Forms.Button registreeriBtn;
    }
}