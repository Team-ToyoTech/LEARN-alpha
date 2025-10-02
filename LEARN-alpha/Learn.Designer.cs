namespace LEARN_alpha
{
    partial class Learn
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            components = new System.ComponentModel.Container();
            toolPanel = new FlowLayoutPanel();
            pointerButton = new Button();
            penButton = new Button();
            eraserButton = new Button();
            andGateButton = new Button();
            orGateButton = new Button();
            notGateButton = new Button();
            xorGateButton = new Button();
            toolPanel.SuspendLayout();
            SuspendLayout();
            //
            // toolPanel
            //
            toolPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            toolPanel.AutoSize = true;
            toolPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            toolPanel.BackColor = Color.FromArgb(245, 245, 245);
            toolPanel.Controls.Add(pointerButton);
            toolPanel.Controls.Add(penButton);
            toolPanel.Controls.Add(eraserButton);
            toolPanel.Controls.Add(andGateButton);
            toolPanel.Controls.Add(orGateButton);
            toolPanel.Controls.Add(notGateButton);
            toolPanel.Controls.Add(xorGateButton);
            toolPanel.FlowDirection = FlowDirection.LeftToRight;
            toolPanel.Location = new Point(12, 12);
            toolPanel.Margin = new Padding(4);
            toolPanel.Name = "toolPanel";
            toolPanel.Padding = new Padding(8, 6, 8, 6);
            toolPanel.Size = new Size(667, 64);
            toolPanel.TabIndex = 0;
            toolPanel.WrapContents = false;
            //
            // pointerButton
            //
            pointerButton.BackColor = SystemColors.ControlLight;
            pointerButton.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            pointerButton.Location = new Point(12, 9);
            pointerButton.Margin = new Padding(4, 3, 4, 3);
            pointerButton.Name = "pointerButton";
            pointerButton.Size = new Size(90, 44);
            pointerButton.TabIndex = 0;
            pointerButton.Text = "포인터";
            pointerButton.UseVisualStyleBackColor = false;
            //
            // penButton
            //
            penButton.BackColor = SystemColors.ControlLight;
            penButton.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            penButton.Location = new Point(110, 9);
            penButton.Margin = new Padding(4, 3, 4, 3);
            penButton.Name = "penButton";
            penButton.Size = new Size(90, 44);
            penButton.TabIndex = 1;
            penButton.Text = "펜";
            penButton.UseVisualStyleBackColor = false;
            //
            // eraserButton
            //
            eraserButton.BackColor = SystemColors.ControlLight;
            eraserButton.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            eraserButton.Location = new Point(208, 9);
            eraserButton.Margin = new Padding(4, 3, 4, 3);
            eraserButton.Name = "eraserButton";
            eraserButton.Size = new Size(90, 44);
            eraserButton.TabIndex = 2;
            eraserButton.Text = "지우개";
            eraserButton.UseVisualStyleBackColor = false;
            //
            // andGateButton
            //
            andGateButton.BackColor = SystemColors.ControlLight;
            andGateButton.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            andGateButton.Location = new Point(306, 9);
            andGateButton.Margin = new Padding(4, 3, 4, 3);
            andGateButton.Name = "andGateButton";
            andGateButton.Size = new Size(90, 44);
            andGateButton.TabIndex = 3;
            andGateButton.Text = "AND";
            andGateButton.UseVisualStyleBackColor = false;
            //
            // orGateButton
            //
            orGateButton.BackColor = SystemColors.ControlLight;
            orGateButton.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            orGateButton.Location = new Point(404, 9);
            orGateButton.Margin = new Padding(4, 3, 4, 3);
            orGateButton.Name = "orGateButton";
            orGateButton.Size = new Size(90, 44);
            orGateButton.TabIndex = 4;
            orGateButton.Text = "OR";
            orGateButton.UseVisualStyleBackColor = false;
            //
            // notGateButton
            //
            notGateButton.BackColor = SystemColors.ControlLight;
            notGateButton.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            notGateButton.Location = new Point(502, 9);
            notGateButton.Margin = new Padding(4, 3, 4, 3);
            notGateButton.Name = "notGateButton";
            notGateButton.Size = new Size(90, 44);
            notGateButton.TabIndex = 5;
            notGateButton.Text = "NOT";
            notGateButton.UseVisualStyleBackColor = false;
            //
            // xorGateButton
            //
            xorGateButton.BackColor = SystemColors.ControlLight;
            xorGateButton.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            xorGateButton.Location = new Point(600, 9);
            xorGateButton.Margin = new Padding(4, 3, 4, 3);
            xorGateButton.Name = "xorGateButton";
            xorGateButton.Size = new Size(90, 44);
            xorGateButton.TabIndex = 6;
            xorGateButton.Text = "XOR";
            xorGateButton.UseVisualStyleBackColor = false;
            //
            // Learn
            //
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1063, 610);
            Controls.Add(toolPanel);
            DoubleBuffered = true;
            KeyPreview = true;
            Name = "Learn";
            Text = "LEARN α test";
            KeyDown += OnKeyDown;
            MouseDown += OnMouseDown;
            MouseMove += OnMouseMove;
            MouseUp += OnMouseUp;
            MouseLeave += OnMouseLeave;
            toolPanel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private FlowLayoutPanel toolPanel;
        private Button pointerButton;
        private Button penButton;
        private Button eraserButton;
        private Button andGateButton;
        private Button orGateButton;
        private Button notGateButton;
        private Button xorGateButton;
    }
}
