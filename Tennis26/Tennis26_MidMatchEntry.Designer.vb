<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Tennis26_MidMatchEntry
    Inherits System.Windows.Forms.Form

    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.Label_CurrentSet = New System.Windows.Forms.Label()
        Me.NumericUpDown_CurrentSet = New System.Windows.Forms.NumericUpDown()
        Me.Label_SetsHeaderSet = New System.Windows.Forms.Label()
        Me.Label_SetsHeaderHome = New System.Windows.Forms.Label()
        Me.Label_SetsHeaderAway = New System.Windows.Forms.Label()
        Me.Label_Set1 = New System.Windows.Forms.Label()
        Me.NumericUpDown_HomeSet1 = New System.Windows.Forms.NumericUpDown()
        Me.NumericUpDown_AwaySet1 = New System.Windows.Forms.NumericUpDown()
        Me.Label_Set2 = New System.Windows.Forms.Label()
        Me.NumericUpDown_HomeSet2 = New System.Windows.Forms.NumericUpDown()
        Me.NumericUpDown_AwaySet2 = New System.Windows.Forms.NumericUpDown()
        Me.Label_Set3 = New System.Windows.Forms.Label()
        Me.NumericUpDown_HomeSet3 = New System.Windows.Forms.NumericUpDown()
        Me.NumericUpDown_AwaySet3 = New System.Windows.Forms.NumericUpDown()
        Me.Label_Set4 = New System.Windows.Forms.Label()
        Me.NumericUpDown_HomeSet4 = New System.Windows.Forms.NumericUpDown()
        Me.NumericUpDown_AwaySet4 = New System.Windows.Forms.NumericUpDown()
        Me.Label_Set5 = New System.Windows.Forms.Label()
        Me.NumericUpDown_HomeSet5 = New System.Windows.Forms.NumericUpDown()
        Me.NumericUpDown_AwaySet5 = New System.Windows.Forms.NumericUpDown()
        Me.Label_CurrentGameScore = New System.Windows.Forms.Label()
        Me.Label_HomePoints = New System.Windows.Forms.Label()
        Me.ComboBox_HomePoints = New System.Windows.Forms.ComboBox()
        Me.Label_AwayPoints = New System.Windows.Forms.Label()
        Me.ComboBox_AwayPoints = New System.Windows.Forms.ComboBox()
        Me.GroupBox_Serving = New System.Windows.Forms.GroupBox()
        Me.RadioButton_HomeServing = New System.Windows.Forms.RadioButton()
        Me.RadioButton_AwayServing = New System.Windows.Forms.RadioButton()
        Me.Label_Elapsed = New System.Windows.Forms.Label()
        Me.NumericUpDown_ElapsedHours = New System.Windows.Forms.NumericUpDown()
        Me.Label_ElapsedHours = New System.Windows.Forms.Label()
        Me.NumericUpDown_ElapsedMinutes = New System.Windows.Forms.NumericUpDown()
        Me.Label_ElapsedMinutes = New System.Windows.Forms.Label()
        Me.Btn_Apply = New System.Windows.Forms.Button()
        Me.Btn_Cancel = New System.Windows.Forms.Button()
        CType(Me.NumericUpDown_CurrentSet, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.NumericUpDown_HomeSet1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.NumericUpDown_AwaySet1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.NumericUpDown_HomeSet2, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.NumericUpDown_AwaySet2, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.NumericUpDown_HomeSet3, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.NumericUpDown_AwaySet3, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.NumericUpDown_HomeSet4, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.NumericUpDown_AwaySet4, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.NumericUpDown_HomeSet5, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.NumericUpDown_AwaySet5, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox_Serving.SuspendLayout()
        CType(Me.NumericUpDown_ElapsedHours, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.NumericUpDown_ElapsedMinutes, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'Label_CurrentSet
        '
        Me.Label_CurrentSet.AutoSize = True
        Me.Label_CurrentSet.Location = New System.Drawing.Point(15, 17)
        Me.Label_CurrentSet.Name = "Label_CurrentSet"
        Me.Label_CurrentSet.Size = New System.Drawing.Size(66, 13)
        Me.Label_CurrentSet.Text = "Current Set:"
        '
        'NumericUpDown_CurrentSet
        '
        Me.NumericUpDown_CurrentSet.Location = New System.Drawing.Point(150, 15)
        Me.NumericUpDown_CurrentSet.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.NumericUpDown_CurrentSet.Maximum = New Decimal(New Integer() {5, 0, 0, 0})
        Me.NumericUpDown_CurrentSet.Name = "NumericUpDown_CurrentSet"
        Me.NumericUpDown_CurrentSet.Size = New System.Drawing.Size(60, 20)
        Me.NumericUpDown_CurrentSet.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'Label_SetsHeaderSet
        '
        Me.Label_SetsHeaderSet.AutoSize = True
        Me.Label_SetsHeaderSet.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.Label_SetsHeaderSet.Location = New System.Drawing.Point(15, 55)
        Me.Label_SetsHeaderSet.Name = "Label_SetsHeaderSet"
        Me.Label_SetsHeaderSet.Size = New System.Drawing.Size(27, 15)
        Me.Label_SetsHeaderSet.Text = "Set"
        '
        'Label_SetsHeaderHome
        '
        Me.Label_SetsHeaderHome.AutoSize = True
        Me.Label_SetsHeaderHome.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.Label_SetsHeaderHome.Location = New System.Drawing.Point(150, 55)
        Me.Label_SetsHeaderHome.Name = "Label_SetsHeaderHome"
        Me.Label_SetsHeaderHome.Size = New System.Drawing.Size(39, 15)
        Me.Label_SetsHeaderHome.Text = "Home"
        '
        'Label_SetsHeaderAway
        '
        Me.Label_SetsHeaderAway.AutoSize = True
        Me.Label_SetsHeaderAway.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.Label_SetsHeaderAway.Location = New System.Drawing.Point(230, 55)
        Me.Label_SetsHeaderAway.Name = "Label_SetsHeaderAway"
        Me.Label_SetsHeaderAway.Size = New System.Drawing.Size(37, 15)
        Me.Label_SetsHeaderAway.Text = "Away"
        '
        'Label_Set1
        '
        Me.Label_Set1.AutoSize = True
        Me.Label_Set1.Location = New System.Drawing.Point(15, 82)
        Me.Label_Set1.Name = "Label_Set1"
        Me.Label_Set1.Size = New System.Drawing.Size(18, 13)
        Me.Label_Set1.Text = "1:"
        '
        'NumericUpDown_HomeSet1
        '
        Me.NumericUpDown_HomeSet1.Location = New System.Drawing.Point(150, 80)
        Me.NumericUpDown_HomeSet1.Maximum = New Decimal(New Integer() {30, 0, 0, 0})
        Me.NumericUpDown_HomeSet1.Name = "NumericUpDown_HomeSet1"
        Me.NumericUpDown_HomeSet1.Size = New System.Drawing.Size(60, 20)
        '
        'NumericUpDown_AwaySet1
        '
        Me.NumericUpDown_AwaySet1.Location = New System.Drawing.Point(230, 80)
        Me.NumericUpDown_AwaySet1.Maximum = New Decimal(New Integer() {30, 0, 0, 0})
        Me.NumericUpDown_AwaySet1.Name = "NumericUpDown_AwaySet1"
        Me.NumericUpDown_AwaySet1.Size = New System.Drawing.Size(60, 20)
        '
        'Label_Set2
        '
        Me.Label_Set2.AutoSize = True
        Me.Label_Set2.Location = New System.Drawing.Point(15, 108)
        Me.Label_Set2.Name = "Label_Set2"
        Me.Label_Set2.Size = New System.Drawing.Size(18, 13)
        Me.Label_Set2.Text = "2:"
        '
        'NumericUpDown_HomeSet2
        '
        Me.NumericUpDown_HomeSet2.Location = New System.Drawing.Point(150, 106)
        Me.NumericUpDown_HomeSet2.Maximum = New Decimal(New Integer() {30, 0, 0, 0})
        Me.NumericUpDown_HomeSet2.Name = "NumericUpDown_HomeSet2"
        Me.NumericUpDown_HomeSet2.Size = New System.Drawing.Size(60, 20)
        '
        'NumericUpDown_AwaySet2
        '
        Me.NumericUpDown_AwaySet2.Location = New System.Drawing.Point(230, 106)
        Me.NumericUpDown_AwaySet2.Maximum = New Decimal(New Integer() {30, 0, 0, 0})
        Me.NumericUpDown_AwaySet2.Name = "NumericUpDown_AwaySet2"
        Me.NumericUpDown_AwaySet2.Size = New System.Drawing.Size(60, 20)
        '
        'Label_Set3
        '
        Me.Label_Set3.AutoSize = True
        Me.Label_Set3.Location = New System.Drawing.Point(15, 134)
        Me.Label_Set3.Name = "Label_Set3"
        Me.Label_Set3.Size = New System.Drawing.Size(18, 13)
        Me.Label_Set3.Text = "3:"
        '
        'NumericUpDown_HomeSet3
        '
        Me.NumericUpDown_HomeSet3.Location = New System.Drawing.Point(150, 132)
        Me.NumericUpDown_HomeSet3.Maximum = New Decimal(New Integer() {30, 0, 0, 0})
        Me.NumericUpDown_HomeSet3.Name = "NumericUpDown_HomeSet3"
        Me.NumericUpDown_HomeSet3.Size = New System.Drawing.Size(60, 20)
        '
        'NumericUpDown_AwaySet3
        '
        Me.NumericUpDown_AwaySet3.Location = New System.Drawing.Point(230, 132)
        Me.NumericUpDown_AwaySet3.Maximum = New Decimal(New Integer() {30, 0, 0, 0})
        Me.NumericUpDown_AwaySet3.Name = "NumericUpDown_AwaySet3"
        Me.NumericUpDown_AwaySet3.Size = New System.Drawing.Size(60, 20)
        '
        'Label_Set4
        '
        Me.Label_Set4.AutoSize = True
        Me.Label_Set4.Location = New System.Drawing.Point(15, 160)
        Me.Label_Set4.Name = "Label_Set4"
        Me.Label_Set4.Size = New System.Drawing.Size(18, 13)
        Me.Label_Set4.Text = "4:"
        '
        'NumericUpDown_HomeSet4
        '
        Me.NumericUpDown_HomeSet4.Location = New System.Drawing.Point(150, 158)
        Me.NumericUpDown_HomeSet4.Maximum = New Decimal(New Integer() {30, 0, 0, 0})
        Me.NumericUpDown_HomeSet4.Name = "NumericUpDown_HomeSet4"
        Me.NumericUpDown_HomeSet4.Size = New System.Drawing.Size(60, 20)
        '
        'NumericUpDown_AwaySet4
        '
        Me.NumericUpDown_AwaySet4.Location = New System.Drawing.Point(230, 158)
        Me.NumericUpDown_AwaySet4.Maximum = New Decimal(New Integer() {30, 0, 0, 0})
        Me.NumericUpDown_AwaySet4.Name = "NumericUpDown_AwaySet4"
        Me.NumericUpDown_AwaySet4.Size = New System.Drawing.Size(60, 20)
        '
        'Label_Set5
        '
        Me.Label_Set5.AutoSize = True
        Me.Label_Set5.Location = New System.Drawing.Point(15, 186)
        Me.Label_Set5.Name = "Label_Set5"
        Me.Label_Set5.Size = New System.Drawing.Size(18, 13)
        Me.Label_Set5.Text = "5:"
        '
        'NumericUpDown_HomeSet5
        '
        Me.NumericUpDown_HomeSet5.Location = New System.Drawing.Point(150, 184)
        Me.NumericUpDown_HomeSet5.Maximum = New Decimal(New Integer() {30, 0, 0, 0})
        Me.NumericUpDown_HomeSet5.Name = "NumericUpDown_HomeSet5"
        Me.NumericUpDown_HomeSet5.Size = New System.Drawing.Size(60, 20)
        '
        'NumericUpDown_AwaySet5
        '
        Me.NumericUpDown_AwaySet5.Location = New System.Drawing.Point(230, 184)
        Me.NumericUpDown_AwaySet5.Maximum = New Decimal(New Integer() {30, 0, 0, 0})
        Me.NumericUpDown_AwaySet5.Name = "NumericUpDown_AwaySet5"
        Me.NumericUpDown_AwaySet5.Size = New System.Drawing.Size(60, 20)
        '
        'Label_CurrentGameScore
        '
        Me.Label_CurrentGameScore.AutoSize = True
        Me.Label_CurrentGameScore.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.Label_CurrentGameScore.Location = New System.Drawing.Point(15, 225)
        Me.Label_CurrentGameScore.Name = "Label_CurrentGameScore"
        Me.Label_CurrentGameScore.Size = New System.Drawing.Size(120, 15)
        Me.Label_CurrentGameScore.Text = "Current Game Score"
        '
        'Label_HomePoints
        '
        Me.Label_HomePoints.AutoSize = True
        Me.Label_HomePoints.Location = New System.Drawing.Point(15, 253)
        Me.Label_HomePoints.Name = "Label_HomePoints"
        Me.Label_HomePoints.Size = New System.Drawing.Size(39, 13)
        Me.Label_HomePoints.Text = "Home:"
        '
        'ComboBox_HomePoints
        '
        Me.ComboBox_HomePoints.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ComboBox_HomePoints.Items.AddRange(New Object() {"0", "15", "30", "40", "Ad"})
        Me.ComboBox_HomePoints.Location = New System.Drawing.Point(150, 250)
        Me.ComboBox_HomePoints.Name = "ComboBox_HomePoints"
        Me.ComboBox_HomePoints.Size = New System.Drawing.Size(60, 21)
        Me.ComboBox_HomePoints.Text = "0"
        '
        'Label_AwayPoints
        '
        Me.Label_AwayPoints.AutoSize = True
        Me.Label_AwayPoints.Location = New System.Drawing.Point(230, 253)
        Me.Label_AwayPoints.Name = "Label_AwayPoints"
        Me.Label_AwayPoints.Size = New System.Drawing.Size(38, 13)
        Me.Label_AwayPoints.Text = "Away:"
        '
        'ComboBox_AwayPoints
        '
        Me.ComboBox_AwayPoints.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ComboBox_AwayPoints.Items.AddRange(New Object() {"0", "15", "30", "40", "Ad"})
        Me.ComboBox_AwayPoints.Location = New System.Drawing.Point(310, 250)
        Me.ComboBox_AwayPoints.Name = "ComboBox_AwayPoints"
        Me.ComboBox_AwayPoints.Size = New System.Drawing.Size(60, 21)
        Me.ComboBox_AwayPoints.Text = "0"
        '
        'GroupBox_Serving
        '
        Me.GroupBox_Serving.Controls.Add(Me.RadioButton_HomeServing)
        Me.GroupBox_Serving.Controls.Add(Me.RadioButton_AwayServing)
        Me.GroupBox_Serving.Location = New System.Drawing.Point(15, 290)
        Me.GroupBox_Serving.Name = "GroupBox_Serving"
        Me.GroupBox_Serving.Size = New System.Drawing.Size(355, 50)
        Me.GroupBox_Serving.TabStop = False
        Me.GroupBox_Serving.Text = "Who is serving now?"
        '
        'RadioButton_HomeServing
        '
        Me.RadioButton_HomeServing.AutoSize = True
        Me.RadioButton_HomeServing.Location = New System.Drawing.Point(15, 20)
        Me.RadioButton_HomeServing.Name = "RadioButton_HomeServing"
        Me.RadioButton_HomeServing.Size = New System.Drawing.Size(53, 17)
        Me.RadioButton_HomeServing.TabStop = True
        Me.RadioButton_HomeServing.Text = "Home"
        Me.RadioButton_HomeServing.UseVisualStyleBackColor = True
        '
        'RadioButton_AwayServing
        '
        Me.RadioButton_AwayServing.AutoSize = True
        Me.RadioButton_AwayServing.Location = New System.Drawing.Point(150, 20)
        Me.RadioButton_AwayServing.Name = "RadioButton_AwayServing"
        Me.RadioButton_AwayServing.Size = New System.Drawing.Size(51, 17)
        Me.RadioButton_AwayServing.Text = "Away"
        Me.RadioButton_AwayServing.UseVisualStyleBackColor = True
        '
        'Label_Elapsed
        '
        Me.Label_Elapsed.AutoSize = True
        Me.Label_Elapsed.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.Label_Elapsed.Location = New System.Drawing.Point(15, 355)
        Me.Label_Elapsed.Name = "Label_Elapsed"
        Me.Label_Elapsed.Size = New System.Drawing.Size(140, 15)
        Me.Label_Elapsed.Text = "Match running for:"
        '
        'NumericUpDown_ElapsedHours
        '
        Me.NumericUpDown_ElapsedHours.Location = New System.Drawing.Point(150, 353)
        Me.NumericUpDown_ElapsedHours.Maximum = New Decimal(New Integer() {10, 0, 0, 0})
        Me.NumericUpDown_ElapsedHours.Name = "NumericUpDown_ElapsedHours"
        Me.NumericUpDown_ElapsedHours.Size = New System.Drawing.Size(50, 20)
        '
        'Label_ElapsedHours
        '
        Me.Label_ElapsedHours.AutoSize = True
        Me.Label_ElapsedHours.Location = New System.Drawing.Point(206, 355)
        Me.Label_ElapsedHours.Name = "Label_ElapsedHours"
        Me.Label_ElapsedHours.Size = New System.Drawing.Size(14, 13)
        Me.Label_ElapsedHours.Text = "h"
        '
        'NumericUpDown_ElapsedMinutes
        '
        Me.NumericUpDown_ElapsedMinutes.Location = New System.Drawing.Point(230, 353)
        Me.NumericUpDown_ElapsedMinutes.Maximum = New Decimal(New Integer() {59, 0, 0, 0})
        Me.NumericUpDown_ElapsedMinutes.Name = "NumericUpDown_ElapsedMinutes"
        Me.NumericUpDown_ElapsedMinutes.Size = New System.Drawing.Size(50, 20)
        '
        'Label_ElapsedMinutes
        '
        Me.Label_ElapsedMinutes.AutoSize = True
        Me.Label_ElapsedMinutes.Location = New System.Drawing.Point(286, 355)
        Me.Label_ElapsedMinutes.Name = "Label_ElapsedMinutes"
        Me.Label_ElapsedMinutes.Size = New System.Drawing.Size(26, 13)
        Me.Label_ElapsedMinutes.Text = "min"
        '
        'Btn_Apply
        '
        Me.Btn_Apply.Location = New System.Drawing.Point(150, 400)
        Me.Btn_Apply.Name = "Btn_Apply"
        Me.Btn_Apply.Size = New System.Drawing.Size(100, 30)
        Me.Btn_Apply.Text = "Apply"
        Me.Btn_Apply.UseVisualStyleBackColor = True
        '
        'Btn_Cancel
        '
        Me.Btn_Cancel.Location = New System.Drawing.Point(260, 400)
        Me.Btn_Cancel.Name = "Btn_Cancel"
        Me.Btn_Cancel.Size = New System.Drawing.Size(100, 30)
        Me.Btn_Cancel.Text = "Cancel"
        Me.Btn_Cancel.UseVisualStyleBackColor = True
        '
        'Tennis26_MidMatchEntry
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(400, 460)
        Me.Controls.Add(Me.Label_CurrentSet)
        Me.Controls.Add(Me.NumericUpDown_CurrentSet)
        Me.Controls.Add(Me.Label_SetsHeaderSet)
        Me.Controls.Add(Me.Label_SetsHeaderHome)
        Me.Controls.Add(Me.Label_SetsHeaderAway)
        Me.Controls.Add(Me.Label_Set1)
        Me.Controls.Add(Me.NumericUpDown_HomeSet1)
        Me.Controls.Add(Me.NumericUpDown_AwaySet1)
        Me.Controls.Add(Me.Label_Set2)
        Me.Controls.Add(Me.NumericUpDown_HomeSet2)
        Me.Controls.Add(Me.NumericUpDown_AwaySet2)
        Me.Controls.Add(Me.Label_Set3)
        Me.Controls.Add(Me.NumericUpDown_HomeSet3)
        Me.Controls.Add(Me.NumericUpDown_AwaySet3)
        Me.Controls.Add(Me.Label_Set4)
        Me.Controls.Add(Me.NumericUpDown_HomeSet4)
        Me.Controls.Add(Me.NumericUpDown_AwaySet4)
        Me.Controls.Add(Me.Label_Set5)
        Me.Controls.Add(Me.NumericUpDown_HomeSet5)
        Me.Controls.Add(Me.NumericUpDown_AwaySet5)
        Me.Controls.Add(Me.Label_CurrentGameScore)
        Me.Controls.Add(Me.Label_HomePoints)
        Me.Controls.Add(Me.ComboBox_HomePoints)
        Me.Controls.Add(Me.Label_AwayPoints)
        Me.Controls.Add(Me.ComboBox_AwayPoints)
        Me.Controls.Add(Me.GroupBox_Serving)
        Me.Controls.Add(Me.Label_Elapsed)
        Me.Controls.Add(Me.NumericUpDown_ElapsedHours)
        Me.Controls.Add(Me.Label_ElapsedHours)
        Me.Controls.Add(Me.NumericUpDown_ElapsedMinutes)
        Me.Controls.Add(Me.Label_ElapsedMinutes)
        Me.Controls.Add(Me.Btn_Apply)
        Me.Controls.Add(Me.Btn_Cancel)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "Tennis26_MidMatchEntry"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Enter Between Running Game"
        CType(Me.NumericUpDown_CurrentSet, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.NumericUpDown_HomeSet1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.NumericUpDown_AwaySet1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.NumericUpDown_HomeSet2, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.NumericUpDown_AwaySet2, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.NumericUpDown_HomeSet3, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.NumericUpDown_AwaySet3, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.NumericUpDown_HomeSet4, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.NumericUpDown_AwaySet4, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.NumericUpDown_HomeSet5, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.NumericUpDown_AwaySet5, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox_Serving.ResumeLayout(False)
        Me.GroupBox_Serving.PerformLayout()
        CType(Me.NumericUpDown_ElapsedHours, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.NumericUpDown_ElapsedMinutes, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents Label_CurrentSet As Label
    Friend WithEvents NumericUpDown_CurrentSet As NumericUpDown
    Friend WithEvents Label_SetsHeaderSet As Label
    Friend WithEvents Label_SetsHeaderHome As Label
    Friend WithEvents Label_SetsHeaderAway As Label
    Friend WithEvents Label_Set1 As Label
    Friend WithEvents NumericUpDown_HomeSet1 As NumericUpDown
    Friend WithEvents NumericUpDown_AwaySet1 As NumericUpDown
    Friend WithEvents Label_Set2 As Label
    Friend WithEvents NumericUpDown_HomeSet2 As NumericUpDown
    Friend WithEvents NumericUpDown_AwaySet2 As NumericUpDown
    Friend WithEvents Label_Set3 As Label
    Friend WithEvents NumericUpDown_HomeSet3 As NumericUpDown
    Friend WithEvents NumericUpDown_AwaySet3 As NumericUpDown
    Friend WithEvents Label_Set4 As Label
    Friend WithEvents NumericUpDown_HomeSet4 As NumericUpDown
    Friend WithEvents NumericUpDown_AwaySet4 As NumericUpDown
    Friend WithEvents Label_Set5 As Label
    Friend WithEvents NumericUpDown_HomeSet5 As NumericUpDown
    Friend WithEvents NumericUpDown_AwaySet5 As NumericUpDown
    Friend WithEvents Label_CurrentGameScore As Label
    Friend WithEvents Label_HomePoints As Label
    Friend WithEvents ComboBox_HomePoints As ComboBox
    Friend WithEvents Label_AwayPoints As Label
    Friend WithEvents ComboBox_AwayPoints As ComboBox
    Friend WithEvents GroupBox_Serving As GroupBox
    Friend WithEvents RadioButton_HomeServing As RadioButton
    Friend WithEvents RadioButton_AwayServing As RadioButton
    Friend WithEvents Label_Elapsed As Label
    Friend WithEvents NumericUpDown_ElapsedHours As NumericUpDown
    Friend WithEvents Label_ElapsedHours As Label
    Friend WithEvents NumericUpDown_ElapsedMinutes As NumericUpDown
    Friend WithEvents Label_ElapsedMinutes As Label
    Friend WithEvents Btn_Apply As Button
    Friend WithEvents Btn_Cancel As Button
End Class
