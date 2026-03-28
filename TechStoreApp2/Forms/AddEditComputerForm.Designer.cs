namespace TechStoreApp2.Forms
{
    partial class AddEditComputerForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.TextBox txtSpecs;
        private System.Windows.Forms.TextBox txtPricePerHour;
        private System.Windows.Forms.TextBox txtWeight;
        private System.Windows.Forms.ComboBox comboCategory;
        private System.Windows.Forms.Button btnSelectPhoto;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.txtName = new System.Windows.Forms.TextBox();
            this.txtSpecs = new System.Windows.Forms.TextBox();
            this.txtPricePerHour = new System.Windows.Forms.TextBox();
            this.txtWeight = new System.Windows.Forms.TextBox();
            this.comboCategory = new System.Windows.Forms.ComboBox();
            this.btnSelectPhoto = new System.Windows.Forms.Button();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();

            // txtName
            this.txtName.Location = new System.Drawing.Point(120, 20);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(200, 22);
            this.txtName.TabIndex = 0;

            // txtSpecs
            this.txtSpecs.Location = new System.Drawing.Point(120, 55);
            this.txtSpecs.Name = "txtSpecs";
            this.txtSpecs.Size = new System.Drawing.Size(200, 22);
            this.txtSpecs.TabIndex = 1;

            // txtPricePerHour
            this.txtPricePerHour.Location = new System.Drawing.Point(120, 90);
            this.txtPricePerHour.Name = "txtPricePerHour";
            this.txtPricePerHour.Size = new System.Drawing.Size(200, 22);
            this.txtPricePerHour.TabIndex = 2;

            // txtWeight
            this.txtWeight.Location = new System.Drawing.Point(120, 125);
            this.txtWeight.Name = "txtWeight";
            this.txtWeight.Size = new System.Drawing.Size(200, 22);
            this.txtWeight.TabIndex = 3;

            // comboCategory
            this.comboCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboCategory.FormattingEnabled = true;
            this.comboCategory.Location = new System.Drawing.Point(120, 160);
            this.comboCategory.Name = "comboCategory";
            this.comboCategory.Size = new System.Drawing.Size(200, 24);
            this.comboCategory.TabIndex = 4;

            // btnSelectPhoto
            this.btnSelectPhoto.Location = new System.Drawing.Point(120, 195);
            this.btnSelectPhoto.Name = "btnSelectPhoto";
            this.btnSelectPhoto.Size = new System.Drawing.Size(100, 30);
            this.btnSelectPhoto.TabIndex = 5;
            this.btnSelectPhoto.Text = "Выбрать фото";
            this.btnSelectPhoto.UseVisualStyleBackColor = true;
            this.btnSelectPhoto.Click += new System.EventHandler(this.btnSelectPhoto_Click);

            // pictureBox
            this.pictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox.Location = new System.Drawing.Point(120, 235);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(150, 150);
            this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox.TabIndex = 6;
            this.pictureBox.TabStop = false;

            // btnSave
            this.btnSave.Location = new System.Drawing.Point(245, 400);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 30);
            this.btnSave.TabIndex = 7;
            this.btnSave.Text = "Сохранить";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);

            // labels
            this.label1.AutoSize = true; this.label1.Location = new System.Drawing.Point(20, 23); this.label1.Text = "Название:";
            this.label2.AutoSize = true; this.label2.Location = new System.Drawing.Point(20, 58); this.label2.Text = "Характеристики:";
            this.label3.AutoSize = true; this.label3.Location = new System.Drawing.Point(20, 93); this.label3.Text = "Цена за час:";
            this.label4.AutoSize = true; this.label4.Location = new System.Drawing.Point(20, 128); this.label4.Text = "Вес:";
            this.label5.AutoSize = true; this.label5.Location = new System.Drawing.Point(20, 163); this.label5.Text = "Категория:";
            this.label6.AutoSize = true; this.label6.Location = new System.Drawing.Point(20, 203); this.label6.Text = "Фотография:";

            // AddEditComputerForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(350, 450);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.btnSelectPhoto);
            this.Controls.Add(this.comboCategory);
            this.Controls.Add(this.txtWeight);
            this.Controls.Add(this.txtPricePerHour);
            this.Controls.Add(this.txtSpecs);
            this.Controls.Add(this.txtName);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "AddEditComputerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Добавление/Редактирование компьютера";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}