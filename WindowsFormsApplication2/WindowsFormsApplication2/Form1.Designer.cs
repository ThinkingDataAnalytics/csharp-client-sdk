using System.Drawing;

namespace WindowsFormsApplication2
{
    partial class Form1
    {

        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Button init_button;

        private System.Windows.Forms.Button login_button;
        private System.Windows.Forms.Button disticid_button;
        private System.Windows.Forms.Button logout_button;

        private System.Windows.Forms.Button dyldsuperProperty_button;
        private System.Windows.Forms.Button superProperty_button;
        private System.Windows.Forms.Button cleanSuperProperty_button;
        private System.Windows.Forms.Button cleanOnce_SuperProperty_button;

        private System.Windows.Forms.Button track_button;
        
        private System.Windows.Forms.Button firstEvent_button;
        private System.Windows.Forms.Button updateEvent_button;
        private System.Windows.Forms.Button overWrite_button;

        private System.Windows.Forms.Button userset_button;
        private System.Windows.Forms.Button userunset_button;

        private System.Windows.Forms.Button userserOnce_button;
        private System.Windows.Forms.Button useradd_button;
        private System.Windows.Forms.Button useraddppend_button;
        private System.Windows.Forms.Button userunidppend_button;
        private System.Windows.Forms.Button userdelete_button;
        private System.Windows.Forms.Button flush_button;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(634, 288);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseClick_1);
            this.ResumeLayout(false);

            this.init_button = new System.Windows.Forms.Button();
            this.init_button.Text = "init SDK ";
            this.init_button.Location = new Point(20, 20);
            this.init_button.Size = new Size(100, 40);
            this.Controls.Add(this.init_button);

            this.login_button = new System.Windows.Forms.Button();
            this.login_button.Text = "login";
            this.login_button.Location = new Point(20, 20 + 60);
            this.login_button.Size = new Size(100, 40);
            this.Controls.Add(this.login_button);

            this.logout_button = new System.Windows.Forms.Button();
            this.logout_button.Text = "logout";
            this.logout_button.Location = new Point(20, 20 + 60 + 60);
            this.login_button.Size = new Size(100, 40);
            this.Controls.Add(this.logout_button);

            this.disticid_button = new System.Windows.Forms.Button();
            this.disticid_button.Text = "disticid";
            this.disticid_button.Location = new Point(20, 20 + 60 + 60 + 60);
            this.disticid_button.Size = new Size(100, 40);
            this.Controls.Add(this.disticid_button);

            this.dyldsuperProperty_button = new System.Windows.Forms.Button();
            this.dyldsuperProperty_button.Text = "dyldsuperProperty";
            this.dyldsuperProperty_button.Location = new Point(20, 20 + 60 + 60 + 60 + 60);
            this.dyldsuperProperty_button.Size = new Size(100, 40);
            this.Controls.Add(this.dyldsuperProperty_button);

            this.superProperty_button = new System.Windows.Forms.Button();
            this.superProperty_button.Text = "superProperty";
            this.superProperty_button.Location = new Point(20, 20 + 60 + 60 + 60 + 60 + 60);
            this.superProperty_button.Size = new Size(100, 40);
            this.Controls.Add(this.superProperty_button);

            this.cleanSuperProperty_button = new System.Windows.Forms.Button();
            this.cleanSuperProperty_button.Text = "cleanSuperProperty";
            this.cleanSuperProperty_button.Location = new Point(20, 20 + 60 + 60 + 60 + 60 + 60 + 60);
            this.cleanSuperProperty_button.Size = new Size(100, 40);
            this.Controls.Add(this.cleanSuperProperty_button);

            this.cleanOnce_SuperProperty_button = new System.Windows.Forms.Button();
            this.cleanOnce_SuperProperty_button.Text = "cleanSuperProperty";
            this.cleanOnce_SuperProperty_button.Location = new Point(20, 20 + 60 + 60 + 60 + 60 + 60 + 60 + 60);
            this.cleanOnce_SuperProperty_button.Size = new Size(100, 40);
            this.Controls.Add(this.cleanOnce_SuperProperty_button);

            this.track_button = new System.Windows.Forms.Button();
            this.track_button.Text = "track ";
            this.track_button.Location = new Point(20 + 120, 20);
            this.track_button.Size = new Size(100, 40);
            this.Controls.Add(this.track_button);

            this.firstEvent_button = new System.Windows.Forms.Button();
            this.firstEvent_button.Text = "firstEvent ";
            this.firstEvent_button.Location = new Point(20 + 120, 20 + 60);
            this.firstEvent_button.Size = new Size(100, 40);
            this.Controls.Add(this.firstEvent_button);

            this.updateEvent_button = new System.Windows.Forms.Button();
            this.updateEvent_button.Text = "firstEvent ";
            this.updateEvent_button.Location = new Point(20 + 120, 20 + 60 + 60);
            this.updateEvent_button.Size = new Size(100, 40);
            this.Controls.Add(this.updateEvent_button);

            this.overWrite_button = new System.Windows.Forms.Button();
            this.overWrite_button.Text = "overWrite ";
            this.overWrite_button.Location = new Point(20 + 120, 20 + 60 + 60 + 60);
            this.overWrite_button.Size = new Size(100, 40);
            this.Controls.Add(this.overWrite_button);

            this.userset_button = new System.Windows.Forms.Button();
            this.userset_button.Text = "user_set ";
            this.userset_button.Location = new Point(20 + 120, 20 + 60 + 60 + 60 + 60);
            this.userset_button.Size = new Size(100, 40);
            this.Controls.Add(this.userset_button);

            this.userunset_button = new System.Windows.Forms.Button();
            this.userunset_button.Text = "user_unset ";
            this.userunset_button.Location = new Point(20 + 120, 20 + 60 + 60 + 60 + 60 + 60);
            this.userunset_button.Size = new Size(100, 40);
            this.Controls.Add(this.userunset_button);

            this.userserOnce_button = new System.Windows.Forms.Button();
            this.userserOnce_button.Text = "user_setOnce ";
            this.userserOnce_button.Location = new Point(20 + 120, 20 + 60 + 60 + 60 + 60 + 60);
            this.userserOnce_button.Size = new Size(100, 40);
            this.Controls.Add(this.userserOnce_button);

            this.useradd_button = new System.Windows.Forms.Button();
            this.useradd_button.Text = "user_add ";
            this.useradd_button.Location = new Point(20 + 120 + 120, 20);
            this.useradd_button.Size = new Size(100, 40);
            this.Controls.Add(this.useradd_button);

            this.useraddppend_button = new System.Windows.Forms.Button();
            this.useraddppend_button.Text = "user_append ";
            this.useraddppend_button.Location = new Point(20 + 120 + 120, 20 + 60);
            this.useraddppend_button.Size = new Size(100, 40);
            this.Controls.Add(this.useraddppend_button);

            this.userunidppend_button = new System.Windows.Forms.Button();
            this.userunidppend_button.Text = "user_uni_append ";
            this.userunidppend_button.Location = new Point(20 + 120 + 120, 20 + 60 + 60);
            this.userunidppend_button.Size = new Size(100, 40);
            this.Controls.Add(this.userunidppend_button);

            this.userdelete_button = new System.Windows.Forms.Button();
            this.userdelete_button.Text = "user_delete ";
            this.userdelete_button.Location = new Point(20 + 120 + 120, 20 + 60 + 60 + 60);
            this.userunidppend_button.Size = new Size(100, 40);
            this.Controls.Add(this.userdelete_button);

            this.flush_button = new System.Windows.Forms.Button();
            this.flush_button.Text = "flush ";
            this.flush_button.Location = new Point(20 + 120 + 120, 20 + 60 + 60 + 60 + 60);
            this.flush_button.Size = new Size(100, 40);
            this.Controls.Add(this.flush_button);

        }

        #endregion
    }
}

