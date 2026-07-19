<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Tennis24_Statistics
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
        Me.DataGridView_Stats = New System.Windows.Forms.DataGridView()
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
        'Tennis24_Statistics
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(784, 833)
        Me.Controls.Add(Me.DataGridView_Stats)
        Me.MinimumSize = New System.Drawing.Size(800, 872)
        Me.Name = "Tennis24_Statistics"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Tennis24 - Statistik"
        CType(Me.DataGridView_Stats, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents DataGridView_Stats As DataGridView
End Class
