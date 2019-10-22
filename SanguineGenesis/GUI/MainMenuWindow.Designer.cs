namespace SanguineGenesis.GUI
{
    partial class MainMenuWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.mapPB = new System.Windows.Forms.PictureBox();
            this.playB = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rainforestRB = new System.Windows.Forms.RadioButton();
            this.savannaRB = new System.Windows.Forms.RadioButton();
            this.newMapB = new System.Windows.Forms.Button();
            this.loadB = new System.Windows.Forms.Button();
            this.saveB = new System.Windows.Forms.Button();
            this.mapNamesCB = new System.Windows.Forms.ComboBox();
            this.newNameTB = new System.Windows.Forms.TextBox();
            this.widthTB = new System.Windows.Forms.TextBox();
            this.heightTB = new System.Windows.Forms.TextBox();
            this.widthL = new System.Windows.Forms.Label();
            this.heightL = new System.Windows.Forms.Label();
            this.newMapPanel = new System.Windows.Forms.Panel();
            this.loadMapPanel = new System.Windows.Forms.Panel();
            this.deleteB = new System.Windows.Forms.Button();
            this.editB = new System.Windows.Forms.Button();
            this.playPanel = new System.Windows.Forms.Panel();
            this.deepWaterRB = new System.Windows.Forms.RadioButton();
            this.drawOptionsGB = new System.Windows.Forms.GroupBox();
            this.addBuildingRB = new System.Windows.Forms.RadioButton();
            this.buildingSelectionGB = new System.Windows.Forms.GroupBox();
            this.rockRB = new System.Windows.Forms.RadioButton();
            this.main1RB = new System.Windows.Forms.RadioButton();
            this.bigRockRB = new System.Windows.Forms.RadioButton();
            this.main0RB = new System.Windows.Forms.RadioButton();
            this.nutrientsRB = new System.Windows.Forms.RadioButton();
            this.landRB = new System.Windows.Forms.RadioButton();
            this.shallowWaterRB = new System.Windows.Forms.RadioButton();
            this.errorMessageL = new System.Windows.Forms.Label();
            this.coordinatesL = new System.Windows.Forms.Label();
            this.brushSizeNUD = new System.Windows.Forms.NumericUpDown();
            this.brushSizeL = new System.Windows.Forms.Label();
            this.nutrientsRateL = new System.Windows.Forms.Label();
            this.nutrientsRateNUD = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.mapPB)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.newMapPanel.SuspendLayout();
            this.loadMapPanel.SuspendLayout();
            this.playPanel.SuspendLayout();
            this.drawOptionsGB.SuspendLayout();
            this.buildingSelectionGB.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.brushSizeNUD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nutrientsRateNUD)).BeginInit();
            this.SuspendLayout();
            // 
            // mapPB
            // 
            this.mapPB.BackColor = System.Drawing.Color.Black;
            this.mapPB.Location = new System.Drawing.Point(291, 17);
            this.mapPB.Name = "mapPB";
            this.mapPB.Size = new System.Drawing.Size(300, 300);
            this.mapPB.TabIndex = 0;
            this.mapPB.TabStop = false;
            this.mapPB.Paint += new System.Windows.Forms.PaintEventHandler(this.MapPB_Paint);
            this.mapPB.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MapPB_MouseDownAction);
            this.mapPB.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MapPB_MouseDownAction);
            // 
            // playB
            // 
            this.playB.Location = new System.Drawing.Point(196, 3);
            this.playB.Name = "playB";
            this.playB.Size = new System.Drawing.Size(101, 64);
            this.playB.TabIndex = 1;
            this.playB.Text = "Play";
            this.playB.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rainforestRB);
            this.groupBox1.Controls.Add(this.savannaRB);
            this.groupBox1.Location = new System.Drawing.Point(38, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(141, 64);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Faction";
            // 
            // rainforestRB
            // 
            this.rainforestRB.AutoSize = true;
            this.rainforestRB.Location = new System.Drawing.Point(7, 40);
            this.rainforestRB.Name = "rainforestRB";
            this.rainforestRB.Size = new System.Drawing.Size(73, 17);
            this.rainforestRB.TabIndex = 1;
            this.rainforestRB.TabStop = true;
            this.rainforestRB.Text = "Rainforest";
            this.rainforestRB.UseVisualStyleBackColor = true;
            // 
            // savannaRB
            // 
            this.savannaRB.AutoSize = true;
            this.savannaRB.Checked = true;
            this.savannaRB.Location = new System.Drawing.Point(7, 20);
            this.savannaRB.Name = "savannaRB";
            this.savannaRB.Size = new System.Drawing.Size(68, 17);
            this.savannaRB.TabIndex = 0;
            this.savannaRB.TabStop = true;
            this.savannaRB.Text = "Savanna";
            this.savannaRB.UseVisualStyleBackColor = true;
            // 
            // newMapB
            // 
            this.newMapB.Location = new System.Drawing.Point(192, 59);
            this.newMapB.Name = "newMapB";
            this.newMapB.Size = new System.Drawing.Size(72, 30);
            this.newMapB.TabIndex = 3;
            this.newMapB.Text = "New";
            this.newMapB.UseVisualStyleBackColor = true;
            this.newMapB.Click += new System.EventHandler(this.NewMapB_Click);
            // 
            // loadB
            // 
            this.loadB.Location = new System.Drawing.Point(198, 40);
            this.loadB.Name = "loadB";
            this.loadB.Size = new System.Drawing.Size(51, 30);
            this.loadB.TabIndex = 4;
            this.loadB.Text = "Load";
            this.loadB.UseVisualStyleBackColor = true;
            this.loadB.Click += new System.EventHandler(this.LoadB_Click);
            // 
            // saveB
            // 
            this.saveB.Location = new System.Drawing.Point(73, 40);
            this.saveB.Name = "saveB";
            this.saveB.Size = new System.Drawing.Size(54, 30);
            this.saveB.TabIndex = 5;
            this.saveB.Text = "Save";
            this.saveB.UseVisualStyleBackColor = true;
            this.saveB.Click += new System.EventHandler(this.SaveB_Click);
            // 
            // mapNamesCB
            // 
            this.mapNamesCB.FormattingEnabled = true;
            this.mapNamesCB.Location = new System.Drawing.Point(21, 13);
            this.mapNamesCB.Name = "mapNamesCB";
            this.mapNamesCB.Size = new System.Drawing.Size(228, 21);
            this.mapNamesCB.TabIndex = 6;
            // 
            // newNameTB
            // 
            this.newNameTB.Location = new System.Drawing.Point(14, 17);
            this.newNameTB.Name = "newNameTB";
            this.newNameTB.Size = new System.Drawing.Size(141, 20);
            this.newNameTB.TabIndex = 7;
            // 
            // widthTB
            // 
            this.widthTB.Location = new System.Drawing.Point(67, 43);
            this.widthTB.Name = "widthTB";
            this.widthTB.Size = new System.Drawing.Size(88, 20);
            this.widthTB.TabIndex = 8;
            // 
            // heightTB
            // 
            this.heightTB.Location = new System.Drawing.Point(67, 69);
            this.heightTB.Name = "heightTB";
            this.heightTB.Size = new System.Drawing.Size(88, 20);
            this.heightTB.TabIndex = 9;
            // 
            // widthL
            // 
            this.widthL.AutoSize = true;
            this.widthL.Location = new System.Drawing.Point(18, 43);
            this.widthL.Name = "widthL";
            this.widthL.Size = new System.Drawing.Size(35, 13);
            this.widthL.TabIndex = 10;
            this.widthL.Text = "Width";
            // 
            // heightL
            // 
            this.heightL.AutoSize = true;
            this.heightL.Location = new System.Drawing.Point(18, 69);
            this.heightL.Name = "heightL";
            this.heightL.Size = new System.Drawing.Size(38, 13);
            this.heightL.TabIndex = 11;
            this.heightL.Text = "Height";
            // 
            // newMapPanel
            // 
            this.newMapPanel.Controls.Add(this.newNameTB);
            this.newMapPanel.Controls.Add(this.heightL);
            this.newMapPanel.Controls.Add(this.newMapB);
            this.newMapPanel.Controls.Add(this.widthL);
            this.newMapPanel.Controls.Add(this.widthTB);
            this.newMapPanel.Controls.Add(this.heightTB);
            this.newMapPanel.Location = new System.Drawing.Point(12, 268);
            this.newMapPanel.Name = "newMapPanel";
            this.newMapPanel.Size = new System.Drawing.Size(273, 103);
            this.newMapPanel.TabIndex = 13;
            // 
            // loadMapPanel
            // 
            this.loadMapPanel.Controls.Add(this.deleteB);
            this.loadMapPanel.Controls.Add(this.mapNamesCB);
            this.loadMapPanel.Controls.Add(this.editB);
            this.loadMapPanel.Controls.Add(this.loadB);
            this.loadMapPanel.Controls.Add(this.saveB);
            this.loadMapPanel.Location = new System.Drawing.Point(12, 377);
            this.loadMapPanel.Name = "loadMapPanel";
            this.loadMapPanel.Size = new System.Drawing.Size(273, 75);
            this.loadMapPanel.TabIndex = 14;
            // 
            // deleteB
            // 
            this.deleteB.Location = new System.Drawing.Point(133, 40);
            this.deleteB.Name = "deleteB";
            this.deleteB.Size = new System.Drawing.Size(59, 30);
            this.deleteB.TabIndex = 13;
            this.deleteB.Text = "Delete";
            this.deleteB.UseVisualStyleBackColor = true;
            this.deleteB.Click += new System.EventHandler(this.DeleteB_Click);
            // 
            // editB
            // 
            this.editB.Location = new System.Drawing.Point(20, 40);
            this.editB.Name = "editB";
            this.editB.Size = new System.Drawing.Size(47, 30);
            this.editB.TabIndex = 12;
            this.editB.Text = "Edit";
            this.editB.UseVisualStyleBackColor = true;
            this.editB.Click += new System.EventHandler(this.EditB_Click);
            // 
            // playPanel
            // 
            this.playPanel.Controls.Add(this.playB);
            this.playPanel.Controls.Add(this.groupBox1);
            this.playPanel.Location = new System.Drawing.Point(291, 377);
            this.playPanel.Name = "playPanel";
            this.playPanel.Size = new System.Drawing.Size(300, 75);
            this.playPanel.TabIndex = 15;
            // 
            // deepWaterRB
            // 
            this.deepWaterRB.AutoSize = true;
            this.deepWaterRB.Checked = true;
            this.deepWaterRB.Location = new System.Drawing.Point(6, 19);
            this.deepWaterRB.Name = "deepWaterRB";
            this.deepWaterRB.Size = new System.Drawing.Size(80, 17);
            this.deepWaterRB.TabIndex = 2;
            this.deepWaterRB.TabStop = true;
            this.deepWaterRB.Text = "Deep water";
            this.deepWaterRB.UseVisualStyleBackColor = true;
            this.deepWaterRB.Click += new System.EventHandler(this.DrawOptionsRB_Click);
            // 
            // drawOptionsGB
            // 
            this.drawOptionsGB.Controls.Add(this.nutrientsRateL);
            this.drawOptionsGB.Controls.Add(this.nutrientsRateNUD);
            this.drawOptionsGB.Controls.Add(this.brushSizeL);
            this.drawOptionsGB.Controls.Add(this.brushSizeNUD);
            this.drawOptionsGB.Controls.Add(this.coordinatesL);
            this.drawOptionsGB.Controls.Add(this.addBuildingRB);
            this.drawOptionsGB.Controls.Add(this.buildingSelectionGB);
            this.drawOptionsGB.Controls.Add(this.nutrientsRB);
            this.drawOptionsGB.Controls.Add(this.landRB);
            this.drawOptionsGB.Controls.Add(this.shallowWaterRB);
            this.drawOptionsGB.Controls.Add(this.deepWaterRB);
            this.drawOptionsGB.Enabled = false;
            this.drawOptionsGB.Location = new System.Drawing.Point(12, 68);
            this.drawOptionsGB.Name = "drawOptionsGB";
            this.drawOptionsGB.Size = new System.Drawing.Size(273, 194);
            this.drawOptionsGB.TabIndex = 13;
            this.drawOptionsGB.TabStop = false;
            this.drawOptionsGB.Text = "Draw options";
            // 
            // addBuildingRB
            // 
            this.addBuildingRB.AutoSize = true;
            this.addBuildingRB.Location = new System.Drawing.Point(7, 111);
            this.addBuildingRB.Name = "addBuildingRB";
            this.addBuildingRB.Size = new System.Drawing.Size(83, 17);
            this.addBuildingRB.TabIndex = 7;
            this.addBuildingRB.TabStop = true;
            this.addBuildingRB.Text = "Add building";
            this.addBuildingRB.UseVisualStyleBackColor = true;
            this.addBuildingRB.Click += new System.EventHandler(this.DrawOptionsRB_Click);
            // 
            // buildingSelectionGB
            // 
            this.buildingSelectionGB.Controls.Add(this.rockRB);
            this.buildingSelectionGB.Controls.Add(this.main1RB);
            this.buildingSelectionGB.Controls.Add(this.bigRockRB);
            this.buildingSelectionGB.Controls.Add(this.main0RB);
            this.buildingSelectionGB.Location = new System.Drawing.Point(142, 18);
            this.buildingSelectionGB.Name = "buildingSelectionGB";
            this.buildingSelectionGB.Size = new System.Drawing.Size(107, 119);
            this.buildingSelectionGB.TabIndex = 6;
            this.buildingSelectionGB.TabStop = false;
            this.buildingSelectionGB.Text = "Building";
            // 
            // rockRB
            // 
            this.rockRB.AutoSize = true;
            this.rockRB.Location = new System.Drawing.Point(6, 65);
            this.rockRB.Name = "rockRB";
            this.rockRB.Size = new System.Drawing.Size(51, 17);
            this.rockRB.TabIndex = 9;
            this.rockRB.TabStop = true;
            this.rockRB.Text = "Rock";
            this.rockRB.UseVisualStyleBackColor = true;
            this.rockRB.Click += new System.EventHandler(this.BuildingRB_Click);
            // 
            // main1RB
            // 
            this.main1RB.AutoSize = true;
            this.main1RB.Location = new System.Drawing.Point(6, 42);
            this.main1RB.Name = "main1RB";
            this.main1RB.Size = new System.Drawing.Size(88, 17);
            this.main1RB.TabIndex = 8;
            this.main1RB.TabStop = true;
            this.main1RB.Text = "Main player 1";
            this.main1RB.UseVisualStyleBackColor = true;
            this.main1RB.Click += new System.EventHandler(this.BuildingRB_Click);
            // 
            // bigRockRB
            // 
            this.bigRockRB.AutoSize = true;
            this.bigRockRB.Location = new System.Drawing.Point(6, 88);
            this.bigRockRB.Name = "bigRockRB";
            this.bigRockRB.Size = new System.Drawing.Size(64, 17);
            this.bigRockRB.TabIndex = 8;
            this.bigRockRB.TabStop = true;
            this.bigRockRB.Text = "Big rock";
            this.bigRockRB.UseVisualStyleBackColor = true;
            this.bigRockRB.Click += new System.EventHandler(this.BuildingRB_Click);
            // 
            // main0RB
            // 
            this.main0RB.AutoSize = true;
            this.main0RB.Checked = true;
            this.main0RB.Location = new System.Drawing.Point(6, 19);
            this.main0RB.Name = "main0RB";
            this.main0RB.Size = new System.Drawing.Size(88, 17);
            this.main0RB.TabIndex = 7;
            this.main0RB.TabStop = true;
            this.main0RB.Text = "Main player 0";
            this.main0RB.UseVisualStyleBackColor = true;
            this.main0RB.Click += new System.EventHandler(this.BuildingRB_Click);
            // 
            // nutrientsRB
            // 
            this.nutrientsRB.AutoSize = true;
            this.nutrientsRB.Location = new System.Drawing.Point(7, 88);
            this.nutrientsRB.Name = "nutrientsRB";
            this.nutrientsRB.Size = new System.Drawing.Size(67, 17);
            this.nutrientsRB.TabIndex = 5;
            this.nutrientsRB.TabStop = true;
            this.nutrientsRB.Text = "Nutrients";
            this.nutrientsRB.UseVisualStyleBackColor = true;
            this.nutrientsRB.Click += new System.EventHandler(this.DrawOptionsRB_Click);
            // 
            // landRB
            // 
            this.landRB.AutoSize = true;
            this.landRB.Location = new System.Drawing.Point(7, 65);
            this.landRB.Name = "landRB";
            this.landRB.Size = new System.Drawing.Size(49, 17);
            this.landRB.TabIndex = 4;
            this.landRB.TabStop = true;
            this.landRB.Text = "Land";
            this.landRB.UseVisualStyleBackColor = true;
            this.landRB.Click += new System.EventHandler(this.DrawOptionsRB_Click);
            // 
            // shallowWaterRB
            // 
            this.shallowWaterRB.AutoSize = true;
            this.shallowWaterRB.Location = new System.Drawing.Point(7, 42);
            this.shallowWaterRB.Name = "shallowWaterRB";
            this.shallowWaterRB.Size = new System.Drawing.Size(91, 17);
            this.shallowWaterRB.TabIndex = 3;
            this.shallowWaterRB.TabStop = true;
            this.shallowWaterRB.Text = "Shallow water";
            this.shallowWaterRB.UseVisualStyleBackColor = true;
            this.shallowWaterRB.Click += new System.EventHandler(this.DrawOptionsRB_Click);
            // 
            // errorMessageL
            // 
            this.errorMessageL.BackColor = System.Drawing.Color.White;
            this.errorMessageL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.errorMessageL.Location = new System.Drawing.Point(16, 17);
            this.errorMessageL.Name = "errorMessageL";
            this.errorMessageL.Size = new System.Drawing.Size(269, 39);
            this.errorMessageL.TabIndex = 16;
            this.errorMessageL.Text = "Error messages";
            this.errorMessageL.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // coordinatesL
            // 
            this.coordinatesL.AutoSize = true;
            this.coordinatesL.Location = new System.Drawing.Point(145, 140);
            this.coordinatesL.Name = "coordinatesL";
            this.coordinatesL.Size = new System.Drawing.Size(39, 13);
            this.coordinatesL.TabIndex = 8;
            this.coordinatesL.Text = "X=; Y=";
            // 
            // brushSizeNUD
            // 
            this.brushSizeNUD.Location = new System.Drawing.Point(90, 138);
            this.brushSizeNUD.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.brushSizeNUD.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.brushSizeNUD.Name = "brushSizeNUD";
            this.brushSizeNUD.Size = new System.Drawing.Size(46, 20);
            this.brushSizeNUD.TabIndex = 9;
            this.brushSizeNUD.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // brushSizeL
            // 
            this.brushSizeL.AutoSize = true;
            this.brushSizeL.Location = new System.Drawing.Point(6, 140);
            this.brushSizeL.Name = "brushSizeL";
            this.brushSizeL.Size = new System.Drawing.Size(58, 13);
            this.brushSizeL.TabIndex = 10;
            this.brushSizeL.Text = "Brush size:";
            // 
            // nutrientsRateL
            // 
            this.nutrientsRateL.AutoSize = true;
            this.nutrientsRateL.Location = new System.Drawing.Point(6, 164);
            this.nutrientsRateL.Name = "nutrientsRateL";
            this.nutrientsRateL.Size = new System.Drawing.Size(73, 13);
            this.nutrientsRateL.TabIndex = 12;
            this.nutrientsRateL.Text = "Nutrients rate:";
            // 
            // nutrientsRateNUD
            // 
            this.nutrientsRateNUD.Location = new System.Drawing.Point(90, 162);
            this.nutrientsRateNUD.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nutrientsRateNUD.Name = "nutrientsRateNUD";
            this.nutrientsRateNUD.Size = new System.Drawing.Size(46, 20);
            this.nutrientsRateNUD.TabIndex = 11;
            this.nutrientsRateNUD.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // MainMenuWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(606, 462);
            this.Controls.Add(this.errorMessageL);
            this.Controls.Add(this.drawOptionsGB);
            this.Controls.Add(this.playPanel);
            this.Controls.Add(this.loadMapPanel);
            this.Controls.Add(this.newMapPanel);
            this.Controls.Add(this.mapPB);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainMenuWindow";
            this.Text = "MainMenu";
            ((System.ComponentModel.ISupportInitialize)(this.mapPB)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.newMapPanel.ResumeLayout(false);
            this.newMapPanel.PerformLayout();
            this.loadMapPanel.ResumeLayout(false);
            this.playPanel.ResumeLayout(false);
            this.drawOptionsGB.ResumeLayout(false);
            this.drawOptionsGB.PerformLayout();
            this.buildingSelectionGB.ResumeLayout(false);
            this.buildingSelectionGB.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.brushSizeNUD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nutrientsRateNUD)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox mapPB;
        private System.Windows.Forms.Button playB;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton savannaRB;
        private System.Windows.Forms.RadioButton rainforestRB;
        private System.Windows.Forms.Button newMapB;
        private System.Windows.Forms.Button loadB;
        private System.Windows.Forms.Button saveB;
        private System.Windows.Forms.ComboBox mapNamesCB;
        private System.Windows.Forms.TextBox newNameTB;
        private System.Windows.Forms.TextBox widthTB;
        private System.Windows.Forms.TextBox heightTB;
        private System.Windows.Forms.Label widthL;
        private System.Windows.Forms.Label heightL;
        private System.Windows.Forms.Panel newMapPanel;
        private System.Windows.Forms.Panel loadMapPanel;
        private System.Windows.Forms.Button editB;
        private System.Windows.Forms.Panel playPanel;
        private System.Windows.Forms.RadioButton deepWaterRB;
        private System.Windows.Forms.GroupBox drawOptionsGB;
        private System.Windows.Forms.RadioButton addBuildingRB;
        private System.Windows.Forms.GroupBox buildingSelectionGB;
        private System.Windows.Forms.RadioButton rockRB;
        private System.Windows.Forms.RadioButton main1RB;
        private System.Windows.Forms.RadioButton bigRockRB;
        private System.Windows.Forms.RadioButton main0RB;
        private System.Windows.Forms.RadioButton nutrientsRB;
        private System.Windows.Forms.RadioButton landRB;
        private System.Windows.Forms.RadioButton shallowWaterRB;
        private System.Windows.Forms.Label errorMessageL;
        private System.Windows.Forms.Button deleteB;
        private System.Windows.Forms.Label coordinatesL;
        private System.Windows.Forms.Label brushSizeL;
        private System.Windows.Forms.NumericUpDown brushSizeNUD;
        private System.Windows.Forms.Label nutrientsRateL;
        private System.Windows.Forms.NumericUpDown nutrientsRateNUD;
    }
}