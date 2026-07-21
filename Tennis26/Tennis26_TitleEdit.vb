' Bearbeitungs-Dialog für die 6 Textzeilen (TextBlock1.Text - TextBlock6.Text) einer Title-
' oder Info-Tafel (title.gtzip, info1-4.gtzip - alle mit identischem Feld-Layout). Der Aufrufer
' setzt BoardDisplayName/InitialLines vor ShowDialog und liest bei DialogResult.OK ResultLines
' aus - kein parametrisierter Konstruktor, damit der WinForms-Designer die Form weiterhin ohne
' Probleme öffnen kann (siehe Tennis26_MidMatchEntry für dasselbe Muster).
Public Class Tennis26_TitleEdit

    Public Property BoardDisplayName As String = ""
    Public Property InitialLines As String() = {"", "", "", "", "", ""}

    Private resultLinesValue As String()
    Public ReadOnly Property ResultLines As String()
        Get
            Return resultLinesValue
        End Get
    End Property

    Private Sub Tennis26_TitleEdit_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = $"Edit {BoardDisplayName}"
        TextBox_Line1.Text = InitialLines(0)
        TextBox_Line2.Text = InitialLines(1)
        TextBox_Line3.Text = InitialLines(2)
        TextBox_Line4.Text = InitialLines(3)
        TextBox_Line5.Text = InitialLines(4)
        TextBox_Line6.Text = InitialLines(5)
    End Sub

    Private Sub Btn_Save_Click(sender As Object, e As EventArgs) Handles Btn_Save.Click
        resultLinesValue = {TextBox_Line1.Text, TextBox_Line2.Text, TextBox_Line3.Text, TextBox_Line4.Text, TextBox_Line5.Text, TextBox_Line6.Text}
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Btn_Cancel_Click(sender As Object, e As EventArgs) Handles Btn_Cancel.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

End Class
