<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Tennis26_Statistics
    Inherits System.Windows.Forms.Form

    'Das Formular überschreibt den Löschvorgang zur Bereinigung der Komponentenliste.
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

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Tennis26_Statistics))
        Me.DataGridView_Stats = New System.Windows.Forms.DataGridView()
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        Me.Btn_Close = New System.Windows.Forms.Button()
        CType(Me.DataGridView_Stats, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'DataGridView_Stats
        '
        Me.DataGridView_Stats.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells
        Me.DataGridView_Stats.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells
        Me.DataGridView_Stats.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridView_Stats.Location = New System.Drawing.Point(12, 3)
        Me.DataGridView_Stats.Name = "DataGridView_Stats"
        Me.DataGridView_Stats.RowHeadersWidth = 62
        Me.DataGridView_Stats.ScrollBars = System.Windows.Forms.ScrollBars.None
        Me.DataGridView_Stats.Size = New System.Drawing.Size(439, 774)
        Me.DataGridView_Stats.TabIndex = 0
        '
        'TextBox1
        '
        Me.TextBox1.Font = New System.Drawing.Font("Segoe UI", 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TextBox1.Location = New System.Drawing.Point(462, 12)
        Me.TextBox1.Multiline = True
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.Size = New System.Drawing.Size(476, 388)
        Me.TextBox1.TabIndex = 27
        Me.TextBox1.Text = resources.GetString("TextBox1.Text")
        '
        'Btn_Close
        '
        Me.Btn_Close.BackColor = System.Drawing.Color.IndianRed
        Me.Btn_Close.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Btn_Close.ForeColor = System.Drawing.Color.White
        Me.Btn_Close.Location = New System.Drawing.Point(813, 643)
        Me.Btn_Close.Name = "Btn_Close"
        Me.Btn_Close.Size = New System.Drawing.Size(125, 106)
        Me.Btn_Close.TabIndex = 28
        Me.Btn_Close.Text = "close"
        Me.Btn_Close.UseVisualStyleBackColor = False
        '
        'Tennis26_Statistics
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(954, 761)
        Me.Controls.Add(Me.Btn_Close)
        Me.Controls.Add(Me.TextBox1)
        Me.Controls.Add(Me.DataGridView_Stats)
        Me.MinimumSize = New System.Drawing.Size(800, 800)
        Me.Name = "Tennis26_Statistics"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Tennis26 - Statistik"
        CType(Me.DataGridView_Stats, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents DataGridView_Stats As DataGridView
    Friend WithEvents TextBox1 As TextBox
    Friend WithEvents Btn_Close As Button
End Class
