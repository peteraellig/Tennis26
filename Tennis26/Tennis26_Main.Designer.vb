<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Tennis26_Main
    Inherits System.Windows.Forms.Form

    'Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Wird vom Windows Form-Designer benötigt.
    Private components As System.ComponentModel.IContainer

    'Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
    'Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
    'Das Bearbeiten mit dem Code-Editor ist nicht möglich.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Tennis26_Main))
        Me.btn_exit = New System.Windows.Forms.Button()
        Me.btn_live = New System.Windows.Forms.Button()
        Me.DataGridView_Players = New System.Windows.Forms.DataGridView()
        Me.btn_save = New System.Windows.Forms.Button()
        Me.btn_update = New System.Windows.Forms.Button()
        Me.btn_delete = New System.Windows.Forms.Button()
        Me.btn_new = New System.Windows.Forms.Button()
        Me.txt_home_player = New System.Windows.Forms.TextBox()
        Me.txt_away_player = New System.Windows.Forms.TextBox()
        Me.lbl_home_player = New System.Windows.Forms.Label()
        Me.lbl_away_player = New System.Windows.Forms.Label()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Btn_loadSettings = New System.Windows.Forms.Button()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Btn_SelectPairing1 = New System.Windows.Forms.Button()
        Me.Btn_SelectPairing2 = New System.Windows.Forms.Button()
        Me.Btn_SelectPairing3 = New System.Windows.Forms.Button()
        Me.Btn_SelectPairing4 = New System.Windows.Forms.Button()
        Me.Btn_Load_File = New System.Windows.Forms.Button()
        Me.Btn_SaveAs = New System.Windows.Forms.Button()
        Me.txt_away_player2 = New System.Windows.Forms.TextBox()
        Me.txt_home_player2 = New System.Windows.Forms.TextBox()
        Me.CheckBox1 = New System.Windows.Forms.CheckBox()
        Me.Btn_open_pairings = New System.Windows.Forms.Button()
        CType(Me.DataGridView_Players, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'btn_exit
        '
        Me.btn_exit.BackColor = System.Drawing.Color.IndianRed
        Me.btn_exit.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btn_exit.Font = New System.Drawing.Font("Segoe UI", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btn_exit.ForeColor = System.Drawing.Color.White
        Me.btn_exit.Location = New System.Drawing.Point(1033, 490)
        Me.btn_exit.Name = "btn_exit"
        Me.btn_exit.Size = New System.Drawing.Size(153, 59)
        Me.btn_exit.TabIndex = 0
        Me.btn_exit.Text = "Exit"
        Me.btn_exit.UseVisualStyleBackColor = False
        '
        'btn_live
        '
        Me.btn_live.BackColor = System.Drawing.Color.OldLace
        Me.btn_live.Font = New System.Drawing.Font("Segoe UI", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btn_live.Location = New System.Drawing.Point(1033, 276)
        Me.btn_live.Name = "btn_live"
        Me.btn_live.Size = New System.Drawing.Size(153, 59)
        Me.btn_live.TabIndex = 1
        Me.btn_live.Text = "live"
        Me.btn_live.UseVisualStyleBackColor = False
        '
        'DataGridView_Players
        '
        Me.DataGridView_Players.AllowUserToOrderColumns = True
        Me.DataGridView_Players.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.DataGridView_Players.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridView_Players.Location = New System.Drawing.Point(12, 12)
        Me.DataGridView_Players.Name = "DataGridView_Players"
        Me.DataGridView_Players.RowHeadersWidth = 51
        Me.DataGridView_Players.Size = New System.Drawing.Size(1010, 323)
        Me.DataGridView_Players.TabIndex = 2
        '
        'btn_save
        '
        Me.btn_save.BackColor = System.Drawing.Color.LimeGreen
        Me.btn_save.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btn_save.ForeColor = System.Drawing.Color.White
        Me.btn_save.Location = New System.Drawing.Point(358, 342)
        Me.btn_save.Name = "btn_save"
        Me.btn_save.Size = New System.Drawing.Size(100, 25)
        Me.btn_save.TabIndex = 3
        Me.btn_save.Text = "Save Player Data"
        Me.btn_save.UseVisualStyleBackColor = False
        '
        'btn_update
        '
        Me.btn_update.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btn_update.Location = New System.Drawing.Point(118, 342)
        Me.btn_update.Name = "btn_update"
        Me.btn_update.Size = New System.Drawing.Size(100, 25)
        Me.btn_update.TabIndex = 4
        Me.btn_update.Text = "Update"
        Me.btn_update.UseVisualStyleBackColor = True
        '
        'btn_delete
        '
        Me.btn_delete.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btn_delete.Location = New System.Drawing.Point(224, 342)
        Me.btn_delete.Name = "btn_delete"
        Me.btn_delete.Size = New System.Drawing.Size(100, 25)
        Me.btn_delete.TabIndex = 5
        Me.btn_delete.Text = "Delete"
        Me.btn_delete.UseVisualStyleBackColor = True
        '
        'btn_new
        '
        Me.btn_new.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btn_new.Location = New System.Drawing.Point(12, 342)
        Me.btn_new.Name = "btn_new"
        Me.btn_new.Size = New System.Drawing.Size(100, 25)
        Me.btn_new.TabIndex = 6
        Me.btn_new.Text = "New"
        Me.btn_new.UseVisualStyleBackColor = True
        '
        'txt_home_player
        '
        Me.txt_home_player.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txt_home_player.Location = New System.Drawing.Point(24, 487)
        Me.txt_home_player.Multiline = True
        Me.txt_home_player.Name = "txt_home_player"
        Me.txt_home_player.ReadOnly = True
        Me.txt_home_player.Size = New System.Drawing.Size(300, 41)
        Me.txt_home_player.TabIndex = 7
        Me.txt_home_player.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'txt_away_player
        '
        Me.txt_away_player.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txt_away_player.Location = New System.Drawing.Point(686, 490)
        Me.txt_away_player.Multiline = True
        Me.txt_away_player.Name = "txt_away_player"
        Me.txt_away_player.ReadOnly = True
        Me.txt_away_player.Size = New System.Drawing.Size(300, 41)
        Me.txt_away_player.TabIndex = 8
        Me.txt_away_player.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'lbl_home_player
        '
        Me.lbl_home_player.AutoSize = True
        Me.lbl_home_player.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbl_home_player.Location = New System.Drawing.Point(25, 468)
        Me.lbl_home_player.Name = "lbl_home_player"
        Me.lbl_home_player.Size = New System.Drawing.Size(92, 17)
        Me.lbl_home_player.TabIndex = 9
        Me.lbl_home_player.Text = "HOME PLAYER"
        '
        'lbl_away_player
        '
        Me.lbl_away_player.AutoSize = True
        Me.lbl_away_player.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbl_away_player.Location = New System.Drawing.Point(686, 471)
        Me.lbl_away_player.Name = "lbl_away_player"
        Me.lbl_away_player.Size = New System.Drawing.Size(86, 17)
        Me.lbl_away_player.TabIndex = 10
        Me.lbl_away_player.Text = "AWAY PLAYER"
        '
        'PictureBox1
        '
        Me.PictureBox1.BackColor = System.Drawing.Color.Silver
        Me.PictureBox1.Location = New System.Drawing.Point(12, 420)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(1010, 219)
        Me.PictureBox1.TabIndex = 12
        Me.PictureBox1.TabStop = False
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Segoe UI", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(291, 430)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(130, 25)
        Me.Label1.TabIndex = 13
        Me.Label1.Text = "current match"
        '
        'Btn_loadSettings
        '
        Me.Btn_loadSettings.BackColor = System.Drawing.Color.PaleGreen
        Me.Btn_loadSettings.Font = New System.Drawing.Font("Segoe UI", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Btn_loadSettings.Location = New System.Drawing.Point(1034, 12)
        Me.Btn_loadSettings.Name = "Btn_loadSettings"
        Me.Btn_loadSettings.Size = New System.Drawing.Size(153, 59)
        Me.Btn_loadSettings.TabIndex = 32
        Me.Btn_loadSettings.Text = "settings"
        Me.Btn_loadSettings.UseVisualStyleBackColor = False
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Segoe UI", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.Location = New System.Drawing.Point(426, 430)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(69, 25)
        Me.Label2.TabIndex = 33
        Me.Label2.Text = "best of"
        '
        'Btn_SelectPairing1
        '
        Me.Btn_SelectPairing1.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Btn_SelectPairing1.Location = New System.Drawing.Point(12, 396)
        Me.Btn_SelectPairing1.Name = "Btn_SelectPairing1"
        Me.Btn_SelectPairing1.Size = New System.Drawing.Size(245, 22)
        Me.Btn_SelectPairing1.TabIndex = 34
        Me.Btn_SelectPairing1.Text = "Pairing 1 (empty)"
        Me.Btn_SelectPairing1.UseVisualStyleBackColor = True
        '
        'Btn_SelectPairing2
        '
        Me.Btn_SelectPairing2.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Btn_SelectPairing2.Location = New System.Drawing.Point(267, 396)
        Me.Btn_SelectPairing2.Name = "Btn_SelectPairing2"
        Me.Btn_SelectPairing2.Size = New System.Drawing.Size(245, 22)
        Me.Btn_SelectPairing2.TabIndex = 35
        Me.Btn_SelectPairing2.Text = "Pairing 2 (empty)"
        Me.Btn_SelectPairing2.UseVisualStyleBackColor = True
        '
        'Btn_SelectPairing3
        '
        Me.Btn_SelectPairing3.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Btn_SelectPairing3.Location = New System.Drawing.Point(522, 396)
        Me.Btn_SelectPairing3.Name = "Btn_SelectPairing3"
        Me.Btn_SelectPairing3.Size = New System.Drawing.Size(245, 22)
        Me.Btn_SelectPairing3.TabIndex = 36
        Me.Btn_SelectPairing3.Text = "Pairing 3 (empty)"
        Me.Btn_SelectPairing3.UseVisualStyleBackColor = True
        '
        'Btn_SelectPairing4
        '
        Me.Btn_SelectPairing4.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Btn_SelectPairing4.Location = New System.Drawing.Point(777, 396)
        Me.Btn_SelectPairing4.Name = "Btn_SelectPairing4"
        Me.Btn_SelectPairing4.Size = New System.Drawing.Size(245, 22)
        Me.Btn_SelectPairing4.TabIndex = 37
        Me.Btn_SelectPairing4.Text = "Pairing 4 (empty)"
        Me.Btn_SelectPairing4.UseVisualStyleBackColor = True
        '
        'Btn_Load_File
        '
        Me.Btn_Load_File.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Btn_Load_File.Location = New System.Drawing.Point(543, 341)
        Me.Btn_Load_File.Name = "Btn_Load_File"
        Me.Btn_Load_File.Size = New System.Drawing.Size(100, 25)
        Me.Btn_Load_File.TabIndex = 35
        Me.Btn_Load_File.Text = "Load File"
        Me.Btn_Load_File.UseVisualStyleBackColor = True
        '
        'Btn_SaveAs
        '
        Me.Btn_SaveAs.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Btn_SaveAs.Location = New System.Drawing.Point(670, 341)
        Me.Btn_SaveAs.Name = "Btn_SaveAs"
        Me.Btn_SaveAs.Size = New System.Drawing.Size(100, 25)
        Me.Btn_SaveAs.TabIndex = 36
        Me.Btn_SaveAs.Text = "Save as.."
        Me.Btn_SaveAs.UseVisualStyleBackColor = True
        '
        'txt_away_player2
        '
        Me.txt_away_player2.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txt_away_player2.Location = New System.Drawing.Point(686, 585)
        Me.txt_away_player2.Multiline = True
        Me.txt_away_player2.Name = "txt_away_player2"
        Me.txt_away_player2.ReadOnly = True
        Me.txt_away_player2.Size = New System.Drawing.Size(300, 41)
        Me.txt_away_player2.TabIndex = 38
        Me.txt_away_player2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'txt_home_player2
        '
        Me.txt_home_player2.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txt_home_player2.Location = New System.Drawing.Point(24, 582)
        Me.txt_home_player2.Multiline = True
        Me.txt_home_player2.Name = "txt_home_player2"
        Me.txt_home_player2.ReadOnly = True
        Me.txt_home_player2.Size = New System.Drawing.Size(300, 41)
        Me.txt_home_player2.TabIndex = 37
        Me.txt_home_player2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'CheckBox1
        '
        Me.CheckBox1.AutoSize = True
        Me.CheckBox1.Location = New System.Drawing.Point(365, 532)
        Me.CheckBox1.Name = "CheckBox1"
        Me.CheckBox1.Size = New System.Drawing.Size(93, 17)
        Me.CheckBox1.TabIndex = 39
        Me.CheckBox1.Text = "Double Match"
        Me.CheckBox1.UseVisualStyleBackColor = True
        '
        'Btn_open_pairings
        '
        Me.Btn_open_pairings.Location = New System.Drawing.Point(1034, 381)
        Me.Btn_open_pairings.Name = "Btn_open_pairings"
        Me.Btn_open_pairings.Size = New System.Drawing.Size(152, 74)
        Me.Btn_open_pairings.TabIndex = 40
        Me.Btn_open_pairings.Text = "Main2"
        Me.Btn_open_pairings.UseVisualStyleBackColor = True
        '
        'Tennis26_Main
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1199, 674)
        Me.Controls.Add(Me.Btn_open_pairings)
        Me.Controls.Add(Me.CheckBox1)
        Me.Controls.Add(Me.txt_away_player2)
        Me.Controls.Add(Me.txt_home_player2)
        Me.Controls.Add(Me.Btn_SaveAs)
        Me.Controls.Add(Me.Btn_Load_File)
        Me.Controls.Add(Me.Btn_SelectPairing1)
        Me.Controls.Add(Me.Btn_SelectPairing2)
        Me.Controls.Add(Me.Btn_SelectPairing3)
        Me.Controls.Add(Me.Btn_SelectPairing4)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Btn_loadSettings)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.lbl_away_player)
        Me.Controls.Add(Me.lbl_home_player)
        Me.Controls.Add(Me.txt_away_player)
        Me.Controls.Add(Me.txt_home_player)
        Me.Controls.Add(Me.btn_new)
        Me.Controls.Add(Me.btn_delete)
        Me.Controls.Add(Me.btn_update)
        Me.Controls.Add(Me.btn_save)
        Me.Controls.Add(Me.DataGridView_Players)
        Me.Controls.Add(Me.btn_live)
        Me.Controls.Add(Me.btn_exit)
        Me.Controls.Add(Me.PictureBox1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "Tennis26_Main"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        CType(Me.DataGridView_Players, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents btn_exit As Button
    Friend WithEvents btn_live As Button
    Friend WithEvents DataGridView_Players As DataGridView
    Friend WithEvents btn_save As Button
    Friend WithEvents btn_update As Button
    Friend WithEvents btn_delete As Button
    Friend WithEvents btn_new As Button
    Friend WithEvents txt_home_player As TextBox
    Friend WithEvents txt_away_player As TextBox
    Friend WithEvents lbl_home_player As Label
    Friend WithEvents lbl_away_player As Label
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents Label1 As Label
    Friend WithEvents Btn_loadSettings As Button
    Friend WithEvents Label2 As Label
    Friend WithEvents Btn_SelectPairing1 As Button
    Friend WithEvents Btn_SelectPairing2 As Button
    Friend WithEvents Btn_SelectPairing3 As Button
    Friend WithEvents Btn_SelectPairing4 As Button
    Friend WithEvents Btn_Load_File As Button
    Friend WithEvents Btn_SaveAs As Button
    Friend WithEvents txt_away_player2 As TextBox
    Friend WithEvents txt_home_player2 As TextBox
    Friend WithEvents CheckBox1 As CheckBox
    Friend WithEvents Btn_open_pairings As Button
End Class
