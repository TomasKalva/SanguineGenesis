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
            this.loadB = new System.Windows.Forms.Button();
            this.saveB = new System.Windows.Forms.Button();
            this.mapNamesCB = new System.Windows.Forms.ComboBox();
            this.deleteB = new System.Windows.Forms.Button();
            this.editB = new System.Windows.Forms.Button();
            this.deepWaterRB = new System.Windows.Forms.RadioButton();
            this.drawOptionsGB = new System.Windows.Forms.GroupBox();
            this.removeBuildingRB = new System.Windows.Forms.RadioButton();
            this.nutrientsRateL = new System.Windows.Forms.Label();
            this.nutrientsRateNUD = new System.Windows.Forms.NumericUpDown();
            this.brushSizeL = new System.Windows.Forms.Label();
            this.brushSizeNUD = new System.Windows.Forms.NumericUpDown();
            this.coordinatesL = new System.Windows.Forms.Label();
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
            this.mapNameL = new System.Windows.Forms.Label();
            this.testAnimalsCB = new System.Windows.Forms.CheckBox();
            this.newMapGB = new System.Windows.Forms.GroupBox();
            this.heightNUD = new System.Windows.Forms.NumericUpDown();
            this.newNameTB = new System.Windows.Forms.TextBox();
            this.widthNUD = new System.Windows.Forms.NumericUpDown();
            this.widthL = new System.Windows.Forms.Label();
            this.newMapB = new System.Windows.Forms.Button();
            this.heightL = new System.Windows.Forms.Label();
            this.loadMapGB = new System.Windows.Forms.GroupBox();
            this.playGB = new System.Windows.Forms.GroupBox();
            this.eraseNutrientsRB = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.mapPB)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.drawOptionsGB.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nutrientsRateNUD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.brushSizeNUD)).BeginInit();
            this.buildingSelectionGB.SuspendLayout();
            this.newMapGB.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.heightNUD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.widthNUD)).BeginInit();
            this.loadMapGB.SuspendLayout();
            this.playGB.SuspendLayout();
            this.SuspendLayout();
            // 
            // mapPB
            // 
            this.mapPB.BackColor = System.Drawing.Color.Black;
            this.mapPB.Location = new System.Drawing.Point(436, 12);
            this.mapPB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.mapPB.Name = "mapPB";
            this.mapPB.Size = new System.Drawing.Size(450, 462);
            this.mapPB.TabIndex = 0;
            this.mapPB.TabStop = false;
            this.mapPB.Paint += new System.Windows.Forms.PaintEventHandler(this.MapPB_Paint);
            this.mapPB.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MapPB_MouseDownAction);
            this.mapPB.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MapPB_MouseDownAction);
            // 
            // playB
            // 
            this.playB.Location = new System.Drawing.Point(271, 52);
            this.playB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.playB.Name = "playB";
            this.playB.Size = new System.Drawing.Size(152, 62);
            this.playB.TabIndex = 1;
            this.playB.Text = "Play";
            this.playB.UseVisualStyleBackColor = true;
            this.playB.Click += new System.EventHandler(this.PlayB_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rainforestRB);
            this.groupBox1.Controls.Add(this.savannaRB);
            this.groupBox1.Location = new System.Drawing.Point(32, 27);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.Size = new System.Drawing.Size(212, 88);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Faction";
            // 
            // rainforestRB
            // 
            this.rainforestRB.AutoSize = true;
            this.rainforestRB.Location = new System.Drawing.Point(8, 54);
            this.rainforestRB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rainforestRB.Name = "rainforestRB";
            this.rainforestRB.Size = new System.Drawing.Size(108, 24);
            this.rainforestRB.TabIndex = 1;
            this.rainforestRB.Text = "Rainforest";
            this.rainforestRB.UseVisualStyleBackColor = true;
            // 
            // savannaRB
            // 
            this.savannaRB.AutoSize = true;
            this.savannaRB.Checked = true;
            this.savannaRB.Location = new System.Drawing.Point(8, 25);
            this.savannaRB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.savannaRB.Name = "savannaRB";
            this.savannaRB.Size = new System.Drawing.Size(97, 24);
            this.savannaRB.TabIndex = 0;
            this.savannaRB.TabStop = true;
            this.savannaRB.Text = "Savanna";
            this.savannaRB.UseVisualStyleBackColor = true;
            // 
            // loadB
            // 
            this.loadB.Location = new System.Drawing.Point(287, 74);
            this.loadB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.loadB.Name = "loadB";
            this.loadB.Size = new System.Drawing.Size(76, 46);
            this.loadB.TabIndex = 4;
            this.loadB.Text = "Load";
            this.loadB.UseVisualStyleBackColor = true;
            this.loadB.Click += new System.EventHandler(this.LoadB_Click);
            // 
            // saveB
            // 
            this.saveB.Location = new System.Drawing.Point(100, 74);
            this.saveB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.saveB.Name = "saveB";
            this.saveB.Size = new System.Drawing.Size(81, 46);
            this.saveB.TabIndex = 5;
            this.saveB.Text = "Save";
            this.saveB.UseVisualStyleBackColor = true;
            this.saveB.Click += new System.EventHandler(this.SaveB_Click);
            // 
            // mapNamesCB
            // 
            this.mapNamesCB.FormattingEnabled = true;
            this.mapNamesCB.Location = new System.Drawing.Point(22, 32);
            this.mapNamesCB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.mapNamesCB.Name = "mapNamesCB";
            this.mapNamesCB.Size = new System.Drawing.Size(340, 28);
            this.mapNamesCB.TabIndex = 6;
            // 
            // deleteB
            // 
            this.deleteB.Location = new System.Drawing.Point(190, 74);
            this.deleteB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.deleteB.Name = "deleteB";
            this.deleteB.Size = new System.Drawing.Size(88, 46);
            this.deleteB.TabIndex = 13;
            this.deleteB.Text = "Delete";
            this.deleteB.UseVisualStyleBackColor = true;
            this.deleteB.Click += new System.EventHandler(this.DeleteB_Click);
            // 
            // editB
            // 
            this.editB.Location = new System.Drawing.Point(20, 74);
            this.editB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.editB.Name = "editB";
            this.editB.Size = new System.Drawing.Size(70, 46);
            this.editB.TabIndex = 12;
            this.editB.Text = "Edit";
            this.editB.UseVisualStyleBackColor = true;
            this.editB.Click += new System.EventHandler(this.EditB_Click);
            // 
            // deepWaterRB
            // 
            this.deepWaterRB.AutoSize = true;
            this.deepWaterRB.Checked = true;
            this.deepWaterRB.Location = new System.Drawing.Point(9, 29);
            this.deepWaterRB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.deepWaterRB.Name = "deepWaterRB";
            this.deepWaterRB.Size = new System.Drawing.Size(116, 24);
            this.deepWaterRB.TabIndex = 2;
            this.deepWaterRB.TabStop = true;
            this.deepWaterRB.Text = "Deep water";
            this.deepWaterRB.UseVisualStyleBackColor = true;
            this.deepWaterRB.Click += new System.EventHandler(this.DrawOptionsRB_Click);
            // 
            // drawOptionsGB
            // 
            this.drawOptionsGB.Controls.Add(this.eraseNutrientsRB);
            this.drawOptionsGB.Controls.Add(this.removeBuildingRB);
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
            this.drawOptionsGB.Location = new System.Drawing.Point(13, 78);
            this.drawOptionsGB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.drawOptionsGB.Name = "drawOptionsGB";
            this.drawOptionsGB.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.drawOptionsGB.Size = new System.Drawing.Size(410, 298);
            this.drawOptionsGB.TabIndex = 13;
            this.drawOptionsGB.TabStop = false;
            this.drawOptionsGB.Text = "Draw options";
            // 
            // removeBuildingRB
            // 
            this.removeBuildingRB.AutoSize = true;
            this.removeBuildingRB.Location = new System.Drawing.Point(8, 237);
            this.removeBuildingRB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.removeBuildingRB.Name = "removeBuildingRB";
            this.removeBuildingRB.Size = new System.Drawing.Size(151, 24);
            this.removeBuildingRB.TabIndex = 13;
            this.removeBuildingRB.Text = "Remove building";
            this.removeBuildingRB.UseVisualStyleBackColor = true;
            this.removeBuildingRB.Click += new System.EventHandler(this.DrawOptionsRB_Click);
            // 
            // nutrientsRateL
            // 
            this.nutrientsRateL.AutoSize = true;
            this.nutrientsRateL.Location = new System.Drawing.Point(201, 260);
            this.nutrientsRateL.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.nutrientsRateL.Name = "nutrientsRateL";
            this.nutrientsRateL.Size = new System.Drawing.Size(109, 20);
            this.nutrientsRateL.TabIndex = 12;
            this.nutrientsRateL.Text = "Nutrients rate:";
            // 
            // nutrientsRateNUD
            // 
            this.nutrientsRateNUD.Location = new System.Drawing.Point(327, 257);
            this.nutrientsRateNUD.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.nutrientsRateNUD.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nutrientsRateNUD.Name = "nutrientsRateNUD";
            this.nutrientsRateNUD.Size = new System.Drawing.Size(69, 26);
            this.nutrientsRateNUD.TabIndex = 11;
            this.nutrientsRateNUD.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // brushSizeL
            // 
            this.brushSizeL.AutoSize = true;
            this.brushSizeL.Location = new System.Drawing.Point(201, 223);
            this.brushSizeL.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.brushSizeL.Name = "brushSizeL";
            this.brushSizeL.Size = new System.Drawing.Size(87, 20);
            this.brushSizeL.TabIndex = 10;
            this.brushSizeL.Text = "Brush size:";
            // 
            // brushSizeNUD
            // 
            this.brushSizeNUD.Location = new System.Drawing.Point(327, 220);
            this.brushSizeNUD.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
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
            this.brushSizeNUD.Size = new System.Drawing.Size(69, 26);
            this.brushSizeNUD.TabIndex = 9;
            this.brushSizeNUD.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // coordinatesL
            // 
            this.coordinatesL.AutoSize = true;
            this.coordinatesL.Location = new System.Drawing.Point(33, 266);
            this.coordinatesL.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.coordinatesL.Name = "coordinatesL";
            this.coordinatesL.Size = new System.Drawing.Size(57, 20);
            this.coordinatesL.TabIndex = 8;
            this.coordinatesL.Text = "X=; Y=";
            // 
            // addBuildingRB
            // 
            this.addBuildingRB.AutoSize = true;
            this.addBuildingRB.Location = new System.Drawing.Point(10, 203);
            this.addBuildingRB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.addBuildingRB.Name = "addBuildingRB";
            this.addBuildingRB.Size = new System.Drawing.Size(121, 24);
            this.addBuildingRB.TabIndex = 7;
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
            this.buildingSelectionGB.Location = new System.Drawing.Point(213, 28);
            this.buildingSelectionGB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buildingSelectionGB.Name = "buildingSelectionGB";
            this.buildingSelectionGB.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buildingSelectionGB.Size = new System.Drawing.Size(160, 183);
            this.buildingSelectionGB.TabIndex = 6;
            this.buildingSelectionGB.TabStop = false;
            this.buildingSelectionGB.Text = "Building";
            // 
            // rockRB
            // 
            this.rockRB.AutoSize = true;
            this.rockRB.Location = new System.Drawing.Point(9, 100);
            this.rockRB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rockRB.Name = "rockRB";
            this.rockRB.Size = new System.Drawing.Size(71, 24);
            this.rockRB.TabIndex = 9;
            this.rockRB.Text = "Rock";
            this.rockRB.UseVisualStyleBackColor = true;
            this.rockRB.Click += new System.EventHandler(this.BuildingRB_Click);
            // 
            // main1RB
            // 
            this.main1RB.AutoSize = true;
            this.main1RB.Location = new System.Drawing.Point(9, 65);
            this.main1RB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.main1RB.Name = "main1RB";
            this.main1RB.Size = new System.Drawing.Size(127, 24);
            this.main1RB.TabIndex = 8;
            this.main1RB.Text = "Main player 1";
            this.main1RB.UseVisualStyleBackColor = true;
            this.main1RB.Click += new System.EventHandler(this.BuildingRB_Click);
            // 
            // bigRockRB
            // 
            this.bigRockRB.AutoSize = true;
            this.bigRockRB.Location = new System.Drawing.Point(9, 135);
            this.bigRockRB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.bigRockRB.Name = "bigRockRB";
            this.bigRockRB.Size = new System.Drawing.Size(91, 24);
            this.bigRockRB.TabIndex = 8;
            this.bigRockRB.Text = "Big rock";
            this.bigRockRB.UseVisualStyleBackColor = true;
            this.bigRockRB.Click += new System.EventHandler(this.BuildingRB_Click);
            // 
            // main0RB
            // 
            this.main0RB.AutoSize = true;
            this.main0RB.Checked = true;
            this.main0RB.Location = new System.Drawing.Point(9, 29);
            this.main0RB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.main0RB.Name = "main0RB";
            this.main0RB.Size = new System.Drawing.Size(127, 24);
            this.main0RB.TabIndex = 7;
            this.main0RB.TabStop = true;
            this.main0RB.Text = "Main player 0";
            this.main0RB.UseVisualStyleBackColor = true;
            this.main0RB.Click += new System.EventHandler(this.BuildingRB_Click);
            // 
            // nutrientsRB
            // 
            this.nutrientsRB.AutoSize = true;
            this.nutrientsRB.Location = new System.Drawing.Point(10, 135);
            this.nutrientsRB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.nutrientsRB.Name = "nutrientsRB";
            this.nutrientsRB.Size = new System.Drawing.Size(98, 24);
            this.nutrientsRB.TabIndex = 5;
            this.nutrientsRB.Text = "Nutrients";
            this.nutrientsRB.UseVisualStyleBackColor = true;
            this.nutrientsRB.Click += new System.EventHandler(this.DrawOptionsRB_Click);
            // 
            // landRB
            // 
            this.landRB.AutoSize = true;
            this.landRB.Location = new System.Drawing.Point(10, 100);
            this.landRB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.landRB.Name = "landRB";
            this.landRB.Size = new System.Drawing.Size(70, 24);
            this.landRB.TabIndex = 4;
            this.landRB.Text = "Land";
            this.landRB.UseVisualStyleBackColor = true;
            this.landRB.Click += new System.EventHandler(this.DrawOptionsRB_Click);
            // 
            // shallowWaterRB
            // 
            this.shallowWaterRB.AutoSize = true;
            this.shallowWaterRB.Location = new System.Drawing.Point(10, 65);
            this.shallowWaterRB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.shallowWaterRB.Name = "shallowWaterRB";
            this.shallowWaterRB.Size = new System.Drawing.Size(132, 24);
            this.shallowWaterRB.TabIndex = 3;
            this.shallowWaterRB.Text = "Shallow water";
            this.shallowWaterRB.UseVisualStyleBackColor = true;
            this.shallowWaterRB.Click += new System.EventHandler(this.DrawOptionsRB_Click);
            // 
            // errorMessageL
            // 
            this.errorMessageL.BackColor = System.Drawing.Color.White;
            this.errorMessageL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.errorMessageL.Location = new System.Drawing.Point(13, 13);
            this.errorMessageL.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.errorMessageL.Name = "errorMessageL";
            this.errorMessageL.Size = new System.Drawing.Size(410, 60);
            this.errorMessageL.TabIndex = 16;
            this.errorMessageL.Text = "Error messages";
            this.errorMessageL.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // mapNameL
            // 
            this.mapNameL.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.mapNameL.Location = new System.Drawing.Point(436, 479);
            this.mapNameL.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.mapNameL.Name = "mapNameL";
            this.mapNameL.Size = new System.Drawing.Size(446, 75);
            this.mapNameL.TabIndex = 13;
            this.mapNameL.Text = "<map name>";
            this.mapNameL.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // testAnimalsCB
            // 
            this.testAnimalsCB.AutoSize = true;
            this.testAnimalsCB.Location = new System.Drawing.Point(271, 22);
            this.testAnimalsCB.Name = "testAnimalsCB";
            this.testAnimalsCB.Size = new System.Drawing.Size(124, 24);
            this.testAnimalsCB.TabIndex = 3;
            this.testAnimalsCB.Text = "Test animals";
            this.testAnimalsCB.UseVisualStyleBackColor = true;
            // 
            // newMapGB
            // 
            this.newMapGB.Controls.Add(this.heightNUD);
            this.newMapGB.Controls.Add(this.newNameTB);
            this.newMapGB.Controls.Add(this.widthNUD);
            this.newMapGB.Controls.Add(this.widthL);
            this.newMapGB.Controls.Add(this.newMapB);
            this.newMapGB.Controls.Add(this.heightL);
            this.newMapGB.Location = new System.Drawing.Point(13, 384);
            this.newMapGB.Name = "newMapGB";
            this.newMapGB.Size = new System.Drawing.Size(411, 160);
            this.newMapGB.TabIndex = 16;
            this.newMapGB.TabStop = false;
            this.newMapGB.Text = "New map";
            // 
            // heightNUD
            // 
            this.heightNUD.Location = new System.Drawing.Point(166, 104);
            this.heightNUD.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.heightNUD.Maximum = new decimal(new int[] {
            150,
            0,
            0,
            0});
            this.heightNUD.Minimum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.heightNUD.Name = "heightNUD";
            this.heightNUD.Size = new System.Drawing.Size(69, 26);
            this.heightNUD.TabIndex = 15;
            this.heightNUD.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // newNameTB
            // 
            this.newNameTB.Location = new System.Drawing.Point(23, 27);
            this.newNameTB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.newNameTB.Name = "newNameTB";
            this.newNameTB.Size = new System.Drawing.Size(210, 26);
            this.newNameTB.TabIndex = 7;
            // 
            // widthNUD
            // 
            this.widthNUD.Location = new System.Drawing.Point(166, 64);
            this.widthNUD.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.widthNUD.Maximum = new decimal(new int[] {
            150,
            0,
            0,
            0});
            this.widthNUD.Minimum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.widthNUD.Name = "widthNUD";
            this.widthNUD.Size = new System.Drawing.Size(69, 26);
            this.widthNUD.TabIndex = 14;
            this.widthNUD.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // widthL
            // 
            this.widthL.AutoSize = true;
            this.widthL.Location = new System.Drawing.Point(29, 67);
            this.widthL.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.widthL.Name = "widthL";
            this.widthL.Size = new System.Drawing.Size(50, 20);
            this.widthL.TabIndex = 10;
            this.widthL.Text = "Width";
            // 
            // newMapB
            // 
            this.newMapB.Location = new System.Drawing.Point(290, 92);
            this.newMapB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.newMapB.Name = "newMapB";
            this.newMapB.Size = new System.Drawing.Size(108, 46);
            this.newMapB.TabIndex = 3;
            this.newMapB.Text = "New";
            this.newMapB.UseVisualStyleBackColor = true;
            this.newMapB.Click += new System.EventHandler(this.NewMapB_Click);
            // 
            // heightL
            // 
            this.heightL.AutoSize = true;
            this.heightL.Location = new System.Drawing.Point(29, 107);
            this.heightL.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.heightL.Name = "heightL";
            this.heightL.Size = new System.Drawing.Size(56, 20);
            this.heightL.TabIndex = 11;
            this.heightL.Text = "Height";
            // 
            // loadMapGB
            // 
            this.loadMapGB.Controls.Add(this.deleteB);
            this.loadMapGB.Controls.Add(this.mapNamesCB);
            this.loadMapGB.Controls.Add(this.saveB);
            this.loadMapGB.Controls.Add(this.editB);
            this.loadMapGB.Controls.Add(this.loadB);
            this.loadMapGB.Location = new System.Drawing.Point(13, 550);
            this.loadMapGB.Name = "loadMapGB";
            this.loadMapGB.Size = new System.Drawing.Size(410, 133);
            this.loadMapGB.TabIndex = 17;
            this.loadMapGB.TabStop = false;
            this.loadMapGB.Text = "Load map";
            // 
            // playGB
            // 
            this.playGB.Controls.Add(this.testAnimalsCB);
            this.playGB.Controls.Add(this.playB);
            this.playGB.Controls.Add(this.groupBox1);
            this.playGB.Location = new System.Drawing.Point(436, 550);
            this.playGB.Name = "playGB";
            this.playGB.Size = new System.Drawing.Size(450, 133);
            this.playGB.TabIndex = 18;
            this.playGB.TabStop = false;
            this.playGB.Text = "Play";
            // 
            // eraseNutrientsRB
            // 
            this.eraseNutrientsRB.AutoSize = true;
            this.eraseNutrientsRB.Location = new System.Drawing.Point(10, 169);
            this.eraseNutrientsRB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.eraseNutrientsRB.Name = "eraseNutrientsRB";
            this.eraseNutrientsRB.Size = new System.Drawing.Size(144, 24);
            this.eraseNutrientsRB.TabIndex = 14;
            this.eraseNutrientsRB.Text = "Erase Nutrients";
            this.eraseNutrientsRB.UseVisualStyleBackColor = true;
            this.eraseNutrientsRB.Click += new System.EventHandler(this.DrawOptionsRB_Click);
            // 
            // MainMenuWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(901, 693);
            this.Controls.Add(this.playGB);
            this.Controls.Add(this.loadMapGB);
            this.Controls.Add(this.newMapGB);
            this.Controls.Add(this.mapNameL);
            this.Controls.Add(this.errorMessageL);
            this.Controls.Add(this.drawOptionsGB);
            this.Controls.Add(this.mapPB);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.Name = "MainMenuWindow";
            this.Text = "Sanguine Genesis";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainMenuWindow_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.mapPB)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.drawOptionsGB.ResumeLayout(false);
            this.drawOptionsGB.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nutrientsRateNUD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.brushSizeNUD)).EndInit();
            this.buildingSelectionGB.ResumeLayout(false);
            this.buildingSelectionGB.PerformLayout();
            this.newMapGB.ResumeLayout(false);
            this.newMapGB.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.heightNUD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.widthNUD)).EndInit();
            this.loadMapGB.ResumeLayout(false);
            this.playGB.ResumeLayout(false);
            this.playGB.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox mapPB;
        private System.Windows.Forms.Button playB;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton savannaRB;
        private System.Windows.Forms.RadioButton rainforestRB;
        private System.Windows.Forms.Button loadB;
        private System.Windows.Forms.Button saveB;
        private System.Windows.Forms.ComboBox mapNamesCB;
        private System.Windows.Forms.Button editB;
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
        private System.Windows.Forms.Label mapNameL;
        private System.Windows.Forms.RadioButton removeBuildingRB;
        private System.Windows.Forms.CheckBox testAnimalsCB;
        private System.Windows.Forms.GroupBox newMapGB;
        private System.Windows.Forms.NumericUpDown heightNUD;
        private System.Windows.Forms.TextBox newNameTB;
        private System.Windows.Forms.NumericUpDown widthNUD;
        private System.Windows.Forms.Label widthL;
        private System.Windows.Forms.Button newMapB;
        private System.Windows.Forms.Label heightL;
        private System.Windows.Forms.GroupBox loadMapGB;
        private System.Windows.Forms.GroupBox playGB;
        private System.Windows.Forms.RadioButton eraseNutrientsRB;
    }
}