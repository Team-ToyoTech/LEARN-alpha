using System.Drawing;
using System.Windows.Forms;

namespace LEARN_alpha
{
    partial class Learn
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer? components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            connectionCanvas = new Panel();
            SuspendLayout();
            //
            // connectionCanvas
            //
            connectionCanvas.BackColor = Color.WhiteSmoke;
            connectionCanvas.BorderStyle = BorderStyle.FixedSingle;
            connectionCanvas.Cursor = Cursors.Cross;
            connectionCanvas.Dock = DockStyle.Fill;
            connectionCanvas.Location = new Point(0, 0);
            connectionCanvas.Name = "connectionCanvas";
            connectionCanvas.Size = new Size(800, 450);
            connectionCanvas.TabIndex = 0;
            //
            // Learn
            //
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(connectionCanvas);
            Name = "Learn";
            Padding = new Padding(12);
            Text = "LEARN ver.α";
            ResumeLayout(false);
        }

        #endregion

        private Panel connectionCanvas;
    }
}
