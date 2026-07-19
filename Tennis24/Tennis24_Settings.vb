Imports System.IO
Imports System.Net
Imports System.Xml

Public Class Tennis24_Settings

    ' Konstante für den Datenpfad
    Private Const SETTINGS_DATA_PATH As String = "C:\VMIX\tennis\data"

    '  öffentliche Arrays 
    Public Shared TextBoxValues(50) As String
    Public Shared CheckBoxValues(20) As Boolean
    Public Shared RadioButtonValues(20) As Boolean
    Public Shared ComboBoxValues(20) As String

    Public Shared IP As String
    Public Shared PORT As String



    'textbox1 = Turniername 
    'textbox2 = Veranstalter    
    'textbox3 = Runde
    '
    'textbox4 = Freitext 1
    'textbox5 = Freitext 2
    'textbox6 = Freitext 3
    'textbox7 = Freitext 4
    'textbox8 = Freitext 5

    'textbox20 = Schiedsrichter 1
    'textbox21 = Schiedsrichter 2
    'textbox22 = Kommentator 1
    'textbox23 = Kommentator 2

    'textbox24= Button Name sponsor 1
    'textbox25= Button Name sponsor 2   
    'textbox26= Button Info1
    'textbox27= Button Info2
    'textbox28= Button Info3
    'textbox29= Button Info4





    'textbox40 = vMix IP
    'textbox41 = vMix Port
    'textbox42 = Match-Tiebreak: Punkte bis zum Sieg (Standard 10)
    'textbox43 = Farbe für gewonnenen Satz im Scorebug, Format #RRGGBB (Standard #FFFF00 = gelb)
    'textbox44 = Port für die live JSON-Datenquelle (Standard 41200)
    'textbox47 = Standard Overlay
    'textbox48 = ScoreBug Overlay
    'textbox49 = Werbe Overlay
    'textbox50 = Anzahl Sätze (3, 5)

    'RadioButtonValues(1) = Best of x

    'checkbox1 = Match-Tiebreak bei 1:1 Sätzen ersetzt den 3. Satz (nur Best of 3)
    'checkbox2 = Freeze Set - Scorebug bleibt bei Satzende auf dem alten Satz stehen,
    '            bis der Scorebug manuell ausgeschaltet wird
    'checkbox3 = Live JSON-Datenquelle aktivieren (siehe TennisJsonServer.vb)
    'radiobutton1 = Best of 3
    'radiobutton2 = Best of 5

    'combobox1 = Overlay 1 (Standard)   
    'combobox2 = Overlay 2 (ScoreBug)   
    'combobox3 = Overlay 3 (Sponsor)    

    Private Sub Tennis24_Settings_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadSettingsFromXml()
        SetLabels()
    End Sub

    Private Sub Btn_Save_settings_Click(sender As Object, e As EventArgs) Handles Btn_Save_settings.Click
        SetVariables()
        SetLabels()
        SaveSettingsToXml()
    End Sub

    ' Standardfarbe für den gewonnenen Satz, falls nichts gespeichert ist
    Public Const DEFAULT_GAMEWON_COLOUR As String = "#FFFF00"

    ' TextBoxValues(42) (Match-Tiebreak: Punkte bis zum Sieg) und TextBoxValues(43)
    ' (Farbe für gewonnenen Satz) werden über NumericUpDown1 bzw. Btn_gamewon_colour
    ' bedient statt über eine TextBox{i} - deshalb nicht Teil der generischen TextBox-
    ' Schleifen unten und separat synchronisiert.
    Private Sub SyncMatchTiebreakTargetControl()
        If NumericUpDown1 IsNot Nothing Then
            Dim matchTiebreakTarget As Integer
            If Integer.TryParse(TextBoxValues(42), matchTiebreakTarget) Then
                NumericUpDown1.Value = Math.Max(NumericUpDown1.Minimum, Math.Min(NumericUpDown1.Maximum, matchTiebreakTarget))
            End If
        End If
    End Sub

    ' Wandelt "#RRGGBB" in eine Color um; bei ungültigem/leerem Wert die Standardfarbe.
    Public Shared Function ParseGamewonColour(hexValue As String) As Color
        Try
            If Not String.IsNullOrWhiteSpace(hexValue) Then
                Return ColorTranslator.FromHtml(hexValue.Trim())
            End If
        Catch ex As Exception
            ' ungültiger Wert -> Standardfarbe unten
        End Try
        Return ColorTranslator.FromHtml(DEFAULT_GAMEWON_COLOUR)
    End Function

    Private Sub SyncGamewonColourControl()
        If Btn_gamewon_colour IsNot Nothing Then
            Btn_gamewon_colour.BackColor = ParseGamewonColour(TextBoxValues(43))
        End If
    End Sub

    ' TextBoxValues(44) (Port der Live-JSON-Datenquelle) wird über NumericUpDown2 bedient.
    ' Label28 zeigt zusätzlich die fertige URL zum Kopieren an - rein informativ, wird
    ' nirgends aus gelesen.
    Private Sub SyncJsonServerControls()
        If NumericUpDown2 IsNot Nothing Then
            Dim jsonPort As Integer
            If Integer.TryParse(TextBoxValues(44), jsonPort) Then
                NumericUpDown2.Value = Math.Max(NumericUpDown2.Minimum, Math.Min(NumericUpDown2.Maximum, jsonPort))
            End If
        End If

        If Label28 IsNot Nothing Then
            Dim port As String = If(NumericUpDown2 IsNot Nothing, NumericUpDown2.Value.ToString("0"), "41200")
            Label28.Text = $"http://{GetLocalIPAddress()}:{port}/status.json"
        End If
    End Sub

    ' Erste gefundene IPv4-Adresse dieses Rechners im lokalen Netzwerk - rein informativ für
    ' Label28, damit man die URL nicht selbst zusammensuchen muss. Fällt auf "localhost"
    ' zurück, falls sich keine Netzwerkadresse ermitteln lässt.
    Private Shared Function GetLocalIPAddress() As String
        Try
            For Each address In Dns.GetHostEntry(Dns.GetHostName()).AddressList
                If address.AddressFamily = Sockets.AddressFamily.InterNetwork Then
                    Return address.ToString()
                End If
            Next
        Catch ex As Exception
            ' Fällt unten auf localhost zurück
        End Try
        Return "localhost"
    End Function

    ' Richtet einmalig die Windows-Netzwerkfreigabe für die Live-JSON-Datenquelle ein, damit
    ' auch andere Geräte im Netzwerk zugreifen können (siehe TennisJsonServer.vb). Führt dazu
    ' "netsh http add urlacl" mit dem runas-Verb aus - Windows fragt dabei SELBST per UAC-
    ' Dialog nach Bestätigung, das Programm erhält zu keinem Zeitpunkt automatisch erhöhte
    ' Rechte ohne diese Bestätigung durch den Benutzer.
    Private Sub Btn_setup_json_urlacl_Click(sender As Object, e As EventArgs) Handles Btn_setup_json_urlacl.Click
        Dim port As Integer = If(NumericUpDown2 IsNot Nothing, CInt(NumericUpDown2.Value), 41200)
        Dim arguments As String = $"http add urlacl url=http://+:{port}/ user=Everyone"

        Dim confirmResult = MessageBox.Show(
            "Es wird folgender Windows-Befehl mit Administratorrechten ausgeführt (Windows fragt danach separat per UAC-Dialog um Bestätigung):" & vbNewLine & vbNewLine &
            $"netsh {arguments}" & vbNewLine & vbNewLine &
            "Damit erlaubt Windows diesem Programm dauerhaft, auf dem gewählten Port netzwerkweit zu lauschen (nötig für die Live-JSON-Datenquelle). Fortfahren?",
            "Netzwerkfreigabe einrichten", MessageBoxButtons.YesNo, MessageBoxIcon.Question)

        If confirmResult <> DialogResult.Yes Then Return

        Try
            Dim startInfo As New ProcessStartInfo("netsh.exe", arguments) With {
                .Verb = "runas",
                .UseShellExecute = True,
                .WindowStyle = ProcessWindowStyle.Hidden
            }

            Using proc = Process.Start(startInfo)
                proc.WaitForExit()
                If proc.ExitCode = 0 Then
                    MessageBox.Show("Netzwerkfreigabe erfolgreich eingerichtet.", "Erfolg", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Else
                    MessageBox.Show(
                        $"netsh meldete einen Fehler (Code {proc.ExitCode})." & vbNewLine &
                        "Falls der Port bereits freigegeben ist, kann das ignoriert werden.",
                        "Hinweis", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                End If
            End Using
        Catch ex As ComponentModel.Win32Exception When ex.NativeErrorCode = 1223
            ' Benutzer hat die UAC-Bestätigung abgebrochen - kein Programmfehler
            MessageBox.Show("Abgebrochen - die Bestätigung wurde nicht erteilt.", "Abgebrochen", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show($"Fehler beim Ausführen: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub Btn_gamewon_colour_Click(sender As Object, e As EventArgs) Handles Btn_gamewon_colour.Click
        Using colourDialog As New ColorDialog()
            colourDialog.Color = Btn_gamewon_colour.BackColor
            colourDialog.FullOpen = True

            If colourDialog.ShowDialog() = DialogResult.OK Then
                Btn_gamewon_colour.BackColor = colourDialog.Color
                ' vMix erwartet das Format #RRGGBB
                TextBoxValues(43) = "#" & colourDialog.Color.R.ToString("X2") &
                                          colourDialog.Color.G.ToString("X2") &
                                          colourDialog.Color.B.ToString("X2")
            End If
        End Using
    End Sub

    Public Sub SetLabels()

        For i As Integer = 1 To 50
            Dim textBoxControl = Me.Controls.Find($"TextBox{i}", True).FirstOrDefault()
            If textBoxControl IsNot Nothing AndAlso TypeOf textBoxControl Is TextBox Then
                DirectCast(textBoxControl, TextBox).Text = TextBoxValues(i)
            End If
        Next
        SyncMatchTiebreakTargetControl()
        SyncGamewonColourControl()
        SyncJsonServerControls()

        ' RadioButtons setzen - nur wenn Controls existieren (Form geladen)
        If RadioButton1 IsNot Nothing Then
            If TextBoxValues(50) = "3" Then RadioButton1.Checked = True
            If TextBoxValues(50) = "5" Then RadioButton2.Checked = True
        End If

        ' ComboBox1 (Standard Overlay) - ComboBoxValues(1)
        If ComboBox1 IsNot Nothing Then
            Dim standardValue As String = ComboBoxValues(1)
            If String.IsNullOrEmpty(standardValue) OrElse Not IsNumeric(standardValue) OrElse CInt(standardValue) < 1 OrElse CInt(standardValue) > 8 Then
                ComboBox1.Text = "1"
            Else
                ComboBox1.Text = standardValue
            End If
        End If

        ' ComboBox2 (ScoreBug Overlay) - ComboBoxValues(2)
        If ComboBox2 IsNot Nothing Then
            Dim scoreBugValue As String = ComboBoxValues(2)
            If String.IsNullOrEmpty(scoreBugValue) OrElse Not IsNumeric(scoreBugValue) OrElse CInt(scoreBugValue) < 1 OrElse CInt(scoreBugValue) > 8 Then
                ComboBox2.Text = "2"
            Else
                ComboBox2.Text = scoreBugValue
            End If
        End If

        ' ComboBox3 (Werbe/Sponsor Overlay) - ComboBoxValues(3)
        If ComboBox3 IsNot Nothing Then
            Dim werbeValue As String = ComboBoxValues(3)
            If String.IsNullOrEmpty(werbeValue) OrElse Not IsNumeric(werbeValue) OrElse CInt(werbeValue) < 1 OrElse CInt(werbeValue) > 8 Then
                ComboBox3.Text = "3"
            Else
                ComboBox3.Text = werbeValue
            End If
        End If

        Tennis24_Scorer.Btn_ref1.Text = TextBoxValues(20).Split(","c)(0).Trim()
        Tennis24_Scorer.Btn_ref2.Text = TextBoxValues(21).Split(","c)(0).Trim()

        Dim com1Name As String = If(String.IsNullOrEmpty(TextBox22.Text.Trim()), "Commentator 1", TextBox22.Text.Trim())
        Dim com2Name As String = TextBox23.Text.Trim()

        If String.IsNullOrEmpty(TextBox23.Text.Trim()) Then
            ' Nur ein Kommentator
            Tennis24_Scorer.Label8.Text = com1Name
        Else
            ' Beide Kommentatoren
            Tennis24_Scorer.Label8.Text = com1Name & vbNewLine & com2Name
        End If

        'Tennis24_Scorer.Btn_com1.Text = TextBoxValues(22).Split(","c)(0).Trim()
        'Tennis24_Scorer.Btn_com2.Text = TextBoxValues(23).Split(","c)(0).Trim()

        Tennis24_Scorer.Btn_freename1.Text = TextBoxValues(4).Split(","c)(0).Trim()
        Tennis24_Scorer.Btn_freename2.Text = TextBoxValues(5).Split(","c)(0).Trim()
        Tennis24_Scorer.Btn_freename3.Text = TextBoxValues(6).Split(","c)(0).Trim()
        Tennis24_Scorer.Btn_freename4.Text = TextBoxValues(7).Split(","c)(0).Trim()
        Tennis24_Scorer.Btn_freename5.Text = TextBoxValues(8).Split(","c)(0).Trim()

        Tennis24_Scorer.Btn_sponsor1.Text = TextBoxValues(24)
        Tennis24_Scorer.Btn_sponsor2.Text = TextBoxValues(25)

        Tennis24_Scorer.Btn_info1.Text = TextBoxValues(26)
        Tennis24_Scorer.Btn_info2.Text = TextBoxValues(27)
        Tennis24_Scorer.Btn_info3.Text = TextBoxValues(28)
        Tennis24_Scorer.Btn_info4.Text = TextBoxValues(29)



    End Sub

    Public Sub SetVariables()
        ' Stelle sicher, dass alle Werte verfügbar sind, sonst setze Defaults
        UpdateArraysFromControls()
    End Sub

    Private Sub LoadDefaultSettings()
        MsgBox("Lade Standardwerte, Dadenbank leer oder nicht vorhanden...", MsgBoxStyle.Information, "Standardwerte")
        ' falls keine daten vorhanden sind können dummy daten geladen werden
        For i As Integer = 1 To 50
            TextBoxValues(i) = ""
        Next
        TextBoxValues(1) = "Bausch & Lomb Championships 2025"
        TextBoxValues(2) = "Amelia Island"
        TextBoxValues(3) = "Round 1"
        TextBoxValues(4) = "Roger Federer, Tennis player (retired, 20-time Grand Slam champion)"
        TextBoxValues(5) = "Rafael Nadal, Tennis player (record French Open winner)"
        TextBoxValues(6) = "Novak Djokovic, Tennis player (men’s Grand Slam title record holder)"
        TextBoxValues(7) = "Serena Williams, Tennis player (23-time Grand Slam champion)"
        TextBoxValues(8) = "Steffi Graf, Tennis player (Golden Slam 1988)"

        TextBoxValues(20) = "Carlos Bernardes, main referee"
        TextBoxValues(21) = "Marija Čičak, main referee"
        TextBoxValues(22) = "Marcel Meinert, Commentary"
        TextBoxValues(23) = "Michel Kratochvil, Expert"

        TextBoxValues(24) = "Sponsor1"
        TextBoxValues(25) = "Sponsor2"

        TextBoxValues(26) = "Info1 Table1"
        TextBoxValues(27) = "Info1 Table2"
        TextBoxValues(28) = "Info1 Table3"
        TextBoxValues(29) = "Info1 Table4"


        For i As Integer = 30 To 39
            TextBoxValues(i) = "Dummy Value " + Str(i)
        Next

        TextBoxValues(42) = "10"                    ' Match-Tiebreak bis X Punkte
        TextBoxValues(43) = DEFAULT_GAMEWON_COLOUR  ' Farbe für gewonnenen Satz im Scorebug
        TextBoxValues(44) = "41200"                 ' Port für die live JSON-Datenquelle

        TextBoxValues(45) = "localhost"         ' vMix IP
        TextBoxValues(46) = "8088"              ' vMix Port

        TextBoxValues(50) = "3"                 'best of x

        For i As Integer = 1 To 20
            CheckBoxValues(i) = False
        Next

        For i As Integer = 1 To 20
            RadioButtonValues(i) = False
        Next

        RadioButtonValues(1) = True  ' Best of 3

        For i As Integer = 1 To 20
            ComboBoxValues(i) = ""
        Next
        ComboBoxValues(1) = "1"  ' Standard Overlay
        ComboBoxValues(2) = "2"  ' ScoreBug Overlay
        ComboBoxValues(3) = "3"  ' Sponsor Overlay

        ' Controls aktualisieren
        UpdateControlsFromArrays()
    End Sub

    Public Sub SaveSettingsToXml()
        Try
            MsgBox("Speichere Einstellungen...", MsgBoxStyle.Information, "Einstellungen speichern")
            ' WICHTIG: Zuerst Arrays aus Controls aktualisieren
            UpdateArraysFromControls()

            ' XML erstellen
            Dim xmlDoc As New XmlDocument()
            Dim root As XmlNode = xmlDoc.CreateElement("Tennis24_Settings")
            xmlDoc.AppendChild(root)

            ' TextBoxes speichern
            Dim textBoxNode As XmlNode = xmlDoc.CreateElement("TextBoxSettings")
            root.AppendChild(textBoxNode)
            For i As Integer = 1 To 50
                Dim element As XmlNode = xmlDoc.CreateElement($"TextBox{i}")
                element.InnerText = TextBoxValues(i)
                textBoxNode.AppendChild(element)
            Next

            ' CheckBoxes speichern
            Dim checkBoxNode As XmlNode = xmlDoc.CreateElement("CheckBoxSettings")
            root.AppendChild(checkBoxNode)
            For i As Integer = 1 To 20
                Dim element As XmlNode = xmlDoc.CreateElement($"CheckBox{i}")
                element.InnerText = CheckBoxValues(i).ToString()
                checkBoxNode.AppendChild(element)
            Next

            ' RadioButtons speichern
            Dim radioButtonNode As XmlNode = xmlDoc.CreateElement("RadioButtonSettings")
            root.AppendChild(radioButtonNode)
            For i As Integer = 1 To 20
                Dim element As XmlNode = xmlDoc.CreateElement($"RadioButton{i}")
                element.InnerText = RadioButtonValues(i).ToString()
                radioButtonNode.AppendChild(element)
            Next

            ' ComboBoxes speichern
            Dim comboBoxNode As XmlNode = xmlDoc.CreateElement("ComboBoxSettings")
            root.AppendChild(comboBoxNode)
            For i As Integer = 1 To 20
                Dim element As XmlNode = xmlDoc.CreateElement($"ComboBox{i}")
                element.InnerText = ComboBoxValues(i)
                comboBoxNode.AppendChild(element)
            Next

            ' XML-Datei speichern
            Dim dataPath As String = SETTINGS_DATA_PATH
            If Not Directory.Exists(dataPath) Then
                Directory.CreateDirectory(dataPath)
            End If

            Dim xmlFilePath As String = Path.Combine(dataPath, "Tennis24_Settings.xml")
            xmlDoc.Save(xmlFilePath)

            'MessageBox.Show($"Einstellungen erfolgreich gespeichert unter: {xmlFilePath}", "Erfolg", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show($"Fehler beim Speichern der Einstellungen: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Public Sub LoadSettingsFromXml()
        Try
            Dim dataPath As String = SETTINGS_DATA_PATH
            Dim xmlFilePath As String = Path.Combine(dataPath, "Tennis24_Settings.xml")

            If Not File.Exists(xmlFilePath) Then
                MessageBox.Show("Keine Einstellungsdatei gefunden. Standardwerte werden verwendet.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information)
                LoadDefaultSettings()
                Return
            End If

            Dim xmlDoc As New XmlDocument()
            xmlDoc.Load(xmlFilePath)

            ' TextBoxes laden
            Dim textBoxNode As XmlNode = xmlDoc.SelectSingleNode("//TextBoxSettings")
            If textBoxNode IsNot Nothing Then
                For i As Integer = 1 To 50
                    Dim element As XmlNode = textBoxNode.SelectSingleNode($"TextBox{i}")
                    If element IsNot Nothing Then
                        TextBoxValues(i) = element.InnerText
                        Dim textBoxControl = Me.Controls.Find($"TextBox{i}", True).FirstOrDefault()
                        If textBoxControl IsNot Nothing AndAlso TypeOf textBoxControl Is TextBox Then
                            DirectCast(textBoxControl, TextBox).Text = TextBoxValues(i)
                        End If
                    End If
                Next
            End If
            SyncMatchTiebreakTargetControl()
            SyncGamewonColourControl()
            SyncJsonServerControls()

            ' CheckBoxes laden
            Dim checkBoxNode As XmlNode = xmlDoc.SelectSingleNode("//CheckBoxSettings")
            If checkBoxNode IsNot Nothing Then
                For i As Integer = 1 To 20
                    Dim element As XmlNode = checkBoxNode.SelectSingleNode($"CheckBox{i}")
                    If element IsNot Nothing Then
                        CheckBoxValues(i) = Boolean.Parse(element.InnerText)
                        Dim checkBoxControl = Me.Controls.Find($"CheckBox{i}", True).FirstOrDefault()
                        If checkBoxControl IsNot Nothing AndAlso TypeOf checkBoxControl Is CheckBox Then
                            DirectCast(checkBoxControl, CheckBox).Checked = CheckBoxValues(i)
                        End If
                    End If
                Next
            End If

            ' RadioButtons laden
            Dim radioButtonNode As XmlNode = xmlDoc.SelectSingleNode("//RadioButtonSettings")
            If radioButtonNode IsNot Nothing Then
                For i As Integer = 1 To 20
                    Dim element As XmlNode = radioButtonNode.SelectSingleNode($"RadioButton{i}")
                    If element IsNot Nothing Then
                        RadioButtonValues(i) = Boolean.Parse(element.InnerText)
                        Dim radioButtonControl = Me.Controls.Find($"RadioButton{i}", True).FirstOrDefault()
                        If radioButtonControl IsNot Nothing AndAlso TypeOf radioButtonControl Is RadioButton Then
                            DirectCast(radioButtonControl, RadioButton).Checked = RadioButtonValues(i)
                        End If
                    End If
                Next
            End If

            ' ComboBoxes laden
            Dim comboBoxNode As XmlNode = xmlDoc.SelectSingleNode("//ComboBoxSettings")
            If comboBoxNode IsNot Nothing Then
                For i As Integer = 1 To 20
                    Dim element As XmlNode = comboBoxNode.SelectSingleNode($"ComboBox{i}")
                    If element IsNot Nothing Then
                        ComboBoxValues(i) = element.InnerText
                        Dim comboBoxControl = Me.Controls.Find($"ComboBox{i}", True).FirstOrDefault()
                        If comboBoxControl IsNot Nothing AndAlso TypeOf comboBoxControl Is ComboBox Then
                            DirectCast(comboBoxControl, ComboBox).Text = ComboBoxValues(i)
                        End If
                    End If
                Next
            End If

            'MessageBox.Show("Einstellungen erfolgreich geladen.", "Erfolg", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show($"Fehler beim Laden der Einstellungen: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub UpdateArraysFromControls()
        ' TextBoxes - Werte aus Controls in Arrays übertragen
        For i As Integer = 1 To 50
            Dim textBoxControl = Me.Controls.Find($"TextBox{i}", True).FirstOrDefault()
            If textBoxControl IsNot Nothing AndAlso TypeOf textBoxControl Is TextBox Then
                TextBoxValues(i) = DirectCast(textBoxControl, TextBox).Text.Trim
            Else
                TextBoxValues(i) = ""
            End If
        Next
        If NumericUpDown1 IsNot Nothing Then TextBoxValues(42) = NumericUpDown1.Value.ToString()
        If Btn_gamewon_colour IsNot Nothing Then
            Dim colour As Color = Btn_gamewon_colour.BackColor
            TextBoxValues(43) = "#" & colour.R.ToString("X2") & colour.G.ToString("X2") & colour.B.ToString("X2")
        End If
        If NumericUpDown2 IsNot Nothing Then TextBoxValues(44) = NumericUpDown2.Value.ToString()

        ' CheckBoxes - Werte aus Controls in Arrays übertragen
        For i As Integer = 1 To 20
            Dim checkBoxControl = Me.Controls.Find($"CheckBox{i}", True).FirstOrDefault()
            If checkBoxControl IsNot Nothing AndAlso TypeOf checkBoxControl Is CheckBox Then
                CheckBoxValues(i) = DirectCast(checkBoxControl, CheckBox).Checked
            Else
                CheckBoxValues(i) = False
            End If
        Next

        ' RadioButtons - Werte aus Controls in Arrays übertragen
        For i As Integer = 1 To 20
            Dim radioButtonControl = Me.Controls.Find($"RadioButton{i}", True).FirstOrDefault()
            If radioButtonControl IsNot Nothing AndAlso TypeOf radioButtonControl Is RadioButton Then
                RadioButtonValues(i) = DirectCast(radioButtonControl, RadioButton).Checked
            Else
                RadioButtonValues(i) = False
            End If
        Next

        ' ComboBoxes - Werte aus Controls in Arrays übertragen
        For i As Integer = 4 To 20
            Dim comboBoxControl = Me.Controls.Find($"ComboBox{i}", True).FirstOrDefault()
            If comboBoxControl IsNot Nothing AndAlso TypeOf comboBoxControl Is ComboBox Then
                ComboBoxValues(i) = DirectCast(comboBoxControl, ComboBox).Text
            Else
                ComboBoxValues(i) = ""
            End If
        Next

        ' Spezielle Behandlung für ComboBoxes → TextBoxValues
        If ComboBox1 IsNot Nothing Then ComboBoxValues(1) = ComboBox1.Text.Trim
        If ComboBox2 IsNot Nothing Then ComboBoxValues(2) = ComboBox2.Text.Trim
        If ComboBox3 IsNot Nothing Then ComboBoxValues(3) = ComboBox3.Text.Trim
    End Sub


    Private Sub UpdateControlsFromArrays()
        ' TextBoxes
        For i As Integer = 1 To 50
            Dim textBoxControl = Me.Controls.Find($"TextBox{i}", True).FirstOrDefault()
            If textBoxControl IsNot Nothing AndAlso TypeOf textBoxControl Is TextBox Then
                DirectCast(textBoxControl, TextBox).Text = TextBoxValues(i)
            End If
        Next
        SyncMatchTiebreakTargetControl()
        SyncGamewonColourControl()
        SyncJsonServerControls()

        ' CheckBoxes
        For i As Integer = 1 To 20
            Dim checkBoxControl = Me.Controls.Find($"CheckBox{i}", True).FirstOrDefault()
            If checkBoxControl IsNot Nothing AndAlso TypeOf checkBoxControl Is CheckBox Then
                DirectCast(checkBoxControl, CheckBox).Checked = CheckBoxValues(i)
            End If
        Next

        ' RadioButtons
        For i As Integer = 1 To 20
            Dim radioButtonControl = Me.Controls.Find($"RadioButton{i}", True).FirstOrDefault()
            If radioButtonControl IsNot Nothing AndAlso TypeOf radioButtonControl Is RadioButton Then
                DirectCast(radioButtonControl, RadioButton).Checked = RadioButtonValues(i)
            End If
        Next

        ' ComboBoxes
        For i As Integer = 1 To 20
            Dim comboBoxControl = Me.Controls.Find($"ComboBox{i}", True).FirstOrDefault()
            If comboBoxControl IsNot Nothing AndAlso TypeOf comboBoxControl Is ComboBox Then
                DirectCast(comboBoxControl, ComboBox).Text = ComboBoxValues(i)
            End If
        Next
    End Sub


    Private Sub Btn_clear_values_Click(sender As Object, e As EventArgs) Handles Btn_clear_values.Click
        Dim result As DialogResult = MessageBox.Show("Möchten Sie wirklich alle Werte löschen?", "Alle Werte löschen", MessageBoxButtons.YesNo, MessageBoxIcon.Question)

        If result = DialogResult.Yes Then
            For Each ctrl As Control In Me.Controls
                If TypeOf ctrl Is TextBox Then
                    DirectCast(ctrl, TextBox).Text = ""
                ElseIf TypeOf ctrl Is CheckBox Then
                    DirectCast(ctrl, CheckBox).Checked = False
                ElseIf TypeOf ctrl Is RadioButton Then
                    DirectCast(ctrl, RadioButton).Checked = False
                ElseIf TypeOf ctrl Is ComboBox Then
                    DirectCast(ctrl, ComboBox).Text = ""
                End If
            Next
            MessageBox.Show("Alle Werte wurden gelöscht.", "Erfolgreich", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
    End Sub

    Private Sub Btn_setdefaultvalues_Click(sender As Object, e As EventArgs) Handles Btn_setdefaultvalues.Click
        Dim result As DialogResult = MessageBox.Show("Möchten Sie die Standardwerte setzen?", "Standardwerte setzen", MessageBoxButtons.YesNo, MessageBoxIcon.Question)

        If result = DialogResult.Yes Then
            LoadDefaultSettings()
            MessageBox.Show("Standardwerte wurden gesetzt.", "Erfolgreich", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
    End Sub

    Private Sub RadioButton1_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton1.CheckedChanged, RadioButton2.CheckedChanged

        ' Nur ausführen wenn ein RadioButton aktiviert (nicht deaktiviert) wird
        Dim radioButton As RadioButton = DirectCast(sender, RadioButton)
        If Not radioButton.Checked Then Return ' Exit wenn RadioButton deaktiviert wird

        Select Case radioButton.Name
            Case "RadioButton1"
                TextBox50.Text = "3"
                TextBoxValues(50) = "3"
            Case "RadioButton2"
                TextBox50.Text = "5"
                TextBoxValues(50) = "5"
        End Select
    End Sub

    Private Sub Btn_Exit_settings_Click(sender As Object, e As EventArgs) Handles Btn_Exit_settings.Click
        UpdateArraysFromControls()
        SetVariables()
        Me.Hide()
        Tennis24_Main.Show()
    End Sub
End Class