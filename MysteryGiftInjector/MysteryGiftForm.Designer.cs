using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MysteryGiftInjector
{
    partial class MysteryGiftForm
    {
        private IContainer components = null;

        private RadioButton radioTopOptionA;
        private RadioButton radioTopOptionB;

        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private GroupBox groupBox3;

        private RadioButton radioGroup1Opt1;

        private RadioButton radioGroup2Opt1;
        private RadioButton radioGroup2Opt2;

        private Label labelStatus;
        private Label labelTicketName;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MysteryGiftForm));
            this.radioTopOptionA = new System.Windows.Forms.RadioButton();
            this.radioTopOptionB = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioGroup1Opt1 = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.radioGroup2Opt2 = new System.Windows.Forms.RadioButton();
            this.radioGroup2Opt1 = new System.Windows.Forms.RadioButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.labelStatus = new System.Windows.Forms.Label();
            this.labelTicketName = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // radioTopOptionA
            // 
            this.radioTopOptionA.AutoSize = true;
            this.radioTopOptionA.Location = new System.Drawing.Point(12, 12);
            this.radioTopOptionA.Name = "radioTopOptionA";
            this.radioTopOptionA.Size = new System.Drawing.Size(63, 17);
            this.radioTopOptionA.TabIndex = 0;
            this.radioTopOptionA.Text = "Emerald";
            this.radioTopOptionA.Visible = false;
            // 
            // radioTopOptionB
            // 
            this.radioTopOptionB.AutoSize = true;
            this.radioTopOptionB.Location = new System.Drawing.Point(81, 12);
            this.radioTopOptionB.Name = "radioTopOptionB";
            this.radioTopOptionB.Size = new System.Drawing.Size(129, 17);
            this.radioTopOptionB.TabIndex = 1;
            this.radioTopOptionB.Text = "Fire Red / Leaf Green";
            this.radioTopOptionB.Visible = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioGroup1Opt1);
            this.groupBox1.Location = new System.Drawing.Point(12, 35);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(260, 138);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Emerald Tickets";
            this.groupBox1.Visible = false;
            // 
            // radioGroup1Opt1
            // 
            this.radioGroup1Opt1.AutoSize = true;
            this.radioGroup1Opt1.Location = new System.Drawing.Point(7, 20);
            this.radioGroup1Opt1.Name = "radioGroup1Opt1";
            this.radioGroup1Opt1.Size = new System.Drawing.Size(89, 17);
            this.radioGroup1Opt1.TabIndex = 0;
            this.radioGroup1Opt1.Text = "Aurora Ticket";
            this.radioGroup1Opt1.Visible = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.radioGroup2Opt2);
            this.groupBox2.Controls.Add(this.radioGroup2Opt1);
            this.groupBox2.Location = new System.Drawing.Point(12, 35);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(260, 138);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Fire Red / Leaf Green Tickets";
            this.groupBox2.Visible = false;
            // 
            // radioGroup2Opt2
            // 
            this.radioGroup2Opt2.AutoSize = true;
            this.radioGroup2Opt2.Location = new System.Drawing.Point(7, 44);
            this.radioGroup2Opt2.Name = "radioGroup2Opt2";
            this.radioGroup2Opt2.Size = new System.Drawing.Size(89, 17);
            this.radioGroup2Opt2.TabIndex = 1;
            this.radioGroup2Opt2.Text = "Aurora Ticket";
            this.radioGroup2Opt2.Visible = false;
            // 
            // radioGroup2Opt1
            // 
            this.radioGroup2Opt1.AutoSize = true;
            this.radioGroup2Opt1.Location = new System.Drawing.Point(7, 20);
            this.radioGroup2Opt1.Name = "radioGroup2Opt1";
            this.radioGroup2Opt1.Size = new System.Drawing.Size(88, 17);
            this.radioGroup2Opt1.TabIndex = 0;
            this.radioGroup2Opt1.Text = "Mystic Ticket";
            this.radioGroup2Opt1.Visible = false;
            // 
            // groupBox3
            // 
            this.groupBox3.Location = new System.Drawing.Point(12, 35);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(260, 138);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Select Version";
            this.groupBox3.Visible = false;
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(16, 200);
            this.labelStatus.MaximumSize = new System.Drawing.Size(240, 0);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(0, 13);
            this.labelStatus.TabIndex = 8;
            this.labelStatus.Visible = false;
            // 
            // labelTicketName
            // 
            this.labelTicketName.AutoSize = true;
            this.labelTicketName.Location = new System.Drawing.Point(237, 237);
            this.labelTicketName.Name = "labelTicketName";
            this.labelTicketName.Size = new System.Drawing.Size(0, 13);
            this.labelTicketName.TabIndex = 9;
            this.labelTicketName.Visible = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.labelTicketName);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.radioTopOptionB);
            this.Controls.Add(this.radioTopOptionA);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Sean\'s Mystery Gift Injector";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
