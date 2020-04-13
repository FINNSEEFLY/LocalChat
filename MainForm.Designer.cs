namespace LocalChat
{
    partial class MainForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlControlArea = new System.Windows.Forms.Panel();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnDisconnect = new System.Windows.Forms.Button();
            this.pnlNameArea = new System.Windows.Forms.Panel();
            this.btnAcceptName = new System.Windows.Forms.Button();
            this.txtNickname = new System.Windows.Forms.TextBox();
            this.lblYourNickname = new System.Windows.Forms.Label();
            this.btnExit = new System.Windows.Forms.Button();
            this.pnlChatArea = new System.Windows.Forms.Panel();
            this.txtMessageHistory = new System.Windows.Forms.RichTextBox();
            this.pnlChatControlArea = new System.Windows.Forms.Panel();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.pnlControlArea.SuspendLayout();
            this.pnlNameArea.SuspendLayout();
            this.pnlChatArea.SuspendLayout();
            this.pnlChatControlArea.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlControlArea
            // 
            this.pnlControlArea.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlControlArea.Controls.Add(this.btnConnect);
            this.pnlControlArea.Controls.Add(this.btnDisconnect);
            this.pnlControlArea.Controls.Add(this.pnlNameArea);
            this.pnlControlArea.Controls.Add(this.btnExit);
            this.pnlControlArea.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlControlArea.Location = new System.Drawing.Point(0, 0);
            this.pnlControlArea.Name = "pnlControlArea";
            this.pnlControlArea.Size = new System.Drawing.Size(230, 470);
            this.pnlControlArea.TabIndex = 0;
            // 
            // btnConnect
            // 
            this.btnConnect.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnConnect.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnConnect.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnConnect.Location = new System.Drawing.Point(0, 339);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(228, 43);
            this.btnConnect.TabIndex = 3;
            this.btnConnect.Text = "Подключится";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnDisconnect
            // 
            this.btnDisconnect.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnDisconnect.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnDisconnect.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnDisconnect.Location = new System.Drawing.Point(0, 382);
            this.btnDisconnect.Name = "btnDisconnect";
            this.btnDisconnect.Size = new System.Drawing.Size(228, 43);
            this.btnDisconnect.TabIndex = 2;
            this.btnDisconnect.Text = "Отключится";
            this.btnDisconnect.UseVisualStyleBackColor = true;
            this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);
            // 
            // pnlNameArea
            // 
            this.pnlNameArea.Controls.Add(this.btnAcceptName);
            this.pnlNameArea.Controls.Add(this.txtNickname);
            this.pnlNameArea.Controls.Add(this.lblYourNickname);
            this.pnlNameArea.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlNameArea.Location = new System.Drawing.Point(0, 0);
            this.pnlNameArea.Name = "pnlNameArea";
            this.pnlNameArea.Size = new System.Drawing.Size(228, 122);
            this.pnlNameArea.TabIndex = 1;
            // 
            // btnAcceptName
            // 
            this.btnAcceptName.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnAcceptName.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnAcceptName.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnAcceptName.Location = new System.Drawing.Point(0, 86);
            this.btnAcceptName.Name = "btnAcceptName";
            this.btnAcceptName.Size = new System.Drawing.Size(228, 36);
            this.btnAcceptName.TabIndex = 2;
            this.btnAcceptName.Text = "Подтвердить имя";
            this.btnAcceptName.UseVisualStyleBackColor = true;
            this.btnAcceptName.Click += new System.EventHandler(this.btnAcceptName_Click);
            // 
            // txtNickname
            // 
            this.txtNickname.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtNickname.Location = new System.Drawing.Point(0, 39);
            this.txtNickname.Name = "txtNickname";
            this.txtNickname.Size = new System.Drawing.Size(228, 24);
            this.txtNickname.TabIndex = 1;
            // 
            // lblYourNickname
            // 
            this.lblYourNickname.AutoSize = true;
            this.lblYourNickname.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblYourNickname.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblYourNickname.Location = new System.Drawing.Point(48, 8);
            this.lblYourNickname.Name = "lblYourNickname";
            this.lblYourNickname.Size = new System.Drawing.Size(123, 22);
            this.lblYourNickname.TabIndex = 0;
            this.lblYourNickname.Text = "Ваш никнейм";
            this.lblYourNickname.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnExit
            // 
            this.btnExit.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnExit.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnExit.Location = new System.Drawing.Point(0, 425);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(228, 43);
            this.btnExit.TabIndex = 0;
            this.btnExit.Text = "Выход";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // pnlChatArea
            // 
            this.pnlChatArea.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlChatArea.Controls.Add(this.txtMessageHistory);
            this.pnlChatArea.Controls.Add(this.pnlChatControlArea);
            this.pnlChatArea.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlChatArea.Location = new System.Drawing.Point(230, 0);
            this.pnlChatArea.Name = "pnlChatArea";
            this.pnlChatArea.Size = new System.Drawing.Size(618, 470);
            this.pnlChatArea.TabIndex = 1;
            // 
            // txtMessageHistory
            // 
            this.txtMessageHistory.BackColor = System.Drawing.SystemColors.Window;
            this.txtMessageHistory.CausesValidation = false;
            this.txtMessageHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMessageHistory.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtMessageHistory.Location = new System.Drawing.Point(0, 0);
            this.txtMessageHistory.Name = "txtMessageHistory";
            this.txtMessageHistory.ReadOnly = true;
            this.txtMessageHistory.Size = new System.Drawing.Size(616, 441);
            this.txtMessageHistory.TabIndex = 1;
            this.txtMessageHistory.Text = "";
            // 
            // pnlChatControlArea
            // 
            this.pnlChatControlArea.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlChatControlArea.Controls.Add(this.txtMessage);
            this.pnlChatControlArea.Controls.Add(this.btnSend);
            this.pnlChatControlArea.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlChatControlArea.Location = new System.Drawing.Point(0, 441);
            this.pnlChatControlArea.Name = "pnlChatControlArea";
            this.pnlChatControlArea.Size = new System.Drawing.Size(616, 27);
            this.pnlChatControlArea.TabIndex = 0;
            // 
            // txtMessage
            // 
            this.txtMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtMessage.Location = new System.Drawing.Point(0, 0);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(499, 26);
            this.txtMessage.TabIndex = 1;
            // 
            // btnSend
            // 
            this.btnSend.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnSend.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnSend.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnSend.Location = new System.Drawing.Point(499, 0);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(115, 25);
            this.btnSend.TabIndex = 0;
            this.btnSend.Text = "Отправить";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(848, 470);
            this.Controls.Add(this.pnlChatArea);
            this.Controls.Add(this.pnlControlArea);
            this.Name = "MainForm";
            this.Text = "LocalChat";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.pnlControlArea.ResumeLayout(false);
            this.pnlNameArea.ResumeLayout(false);
            this.pnlNameArea.PerformLayout();
            this.pnlChatArea.ResumeLayout(false);
            this.pnlChatControlArea.ResumeLayout(false);
            this.pnlChatControlArea.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlControlArea;
        private System.Windows.Forms.Panel pnlChatArea;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Panel pnlNameArea;
        private System.Windows.Forms.TextBox txtNickname;
        private System.Windows.Forms.Label lblYourNickname;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnDisconnect;
        private System.Windows.Forms.Button btnAcceptName;
        private System.Windows.Forms.Panel pnlChatControlArea;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.RichTextBox txtMessageHistory;
    }
}

