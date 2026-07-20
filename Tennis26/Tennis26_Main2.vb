' Prototyp/Vorschlag für eine horizontale Vorbereitungs-Ansicht (Peters Skizze: Daten links,
' mehrere Paarungen rechts zum Vorbereiten). Bewusst als eigene, separate Form gebaut statt
' Tennis26_Main direkt zu verändern - so lässt sich das Konzept ausprobieren, ohne den
' bestehenden Ablauf zu berühren. Erst wenn sich das bewährt, würde man entscheiden, ob es
' Tennis26_Main ersetzt oder ergänzt.
'
' Links: derselbe Spieler-Datenbestand wie in Main (dieselbe XML-Datei), als Drag-Quelle.
' Rechts: 4 Paarungs-Slots, jeweils mit Home/Away (+ bei Doppel Home2/Away2, nur sichtbar
' wenn das jeweilige Doubles-Häkchen gesetzt ist), einem eigenen Einzel/Doppel-Schalter pro
' Paarung, einem "X"-Button zum Leeren (mit Rückfrage) und einem Button, der die Paarung als
' aktuelles Match übernimmt (schreibt in dieselben HomePlayer/AwayPlayer/HomePlayer2/
' AwayPlayer2-Arrays und dieselbe CheckBox1, die auch Tennis26_Main nutzt).
Public Class Tennis26_Main2

    Private Const XML_FILE_PATH As String = "c:\vmix\tennis\data\tennisdata.xml"
    Private Const PAIRINGS_FILE_PATH As String = "c:\vmix\tennis\data\pairings.xml"
    Private Shared ReadOnly FIELD_NAMES As String() = {"Name", "FirstName", "Country", "CountryISO3", "Age", "Height", "Data1", "Data2", "Data3"}
    Private Const PAIRING_COUNT As Integer = 4

    Private _dragStartPoint As Point
    Private _dragRowIndex As Integer = -1

    ' Ein vorbereiteter Paarungs-Slot - hält dieselbe Feldstruktur wie HomePlayer/AwayPlayer.
    Private Class PairingSlot
        Public Property Home As String() = New String(8) {}
        Public Property Away As String() = New String(8) {}
        Public Property Home2 As String() = New String(8) {}
        Public Property Away2 As String() = New String(8) {}
        Public Property Doubles As Boolean = False
    End Class

    Private ReadOnly pairings As PairingSlot() = {New PairingSlot(), New PairingSlot(), New PairingSlot(), New PairingSlot()}

    ' Index (0-3) der aktuell "aktiven" Paarung (die zuletzt per "Use this pairing" ins
    ' laufende Match übernommen wurde) - -1 heisst keine. Steuert die LightBlue-Markierung/
    ' "... ACTIVE"-Beschriftung der jeweiligen GroupBox.
    Private activeIndex As Integer = -1

    ' Array-Zugriff statt einem wachsenden Select Case pro Methode - skaliert ohne
    ' Codeverdopplung auf beliebig viele Paarungen (aktuell 4).
    Private ReadOnly Property PairingGroupBoxes As GroupBox()
        Get
            Return {GroupBox_Pairing1, GroupBox_Pairing2, GroupBox_Pairing3, GroupBox_Pairing4}
        End Get
    End Property

    Private ReadOnly Property PairingDoublesCheckBoxes As CheckBox()
        Get
            Return {CheckBox_Pairing1Doubles, CheckBox_Pairing2Doubles, CheckBox_Pairing3Doubles, CheckBox_Pairing4Doubles}
        End Get
    End Property

    Private ReadOnly Property PairingHomeTextBoxes As TextBox()
        Get
            Return {TextBox_Pairing1Home, TextBox_Pairing2Home, TextBox_Pairing3Home, TextBox_Pairing4Home}
        End Get
    End Property

    Private ReadOnly Property PairingAwayTextBoxes As TextBox()
        Get
            Return {TextBox_Pairing1Away, TextBox_Pairing2Away, TextBox_Pairing3Away, TextBox_Pairing4Away}
        End Get
    End Property

    Private ReadOnly Property PairingHome2TextBoxes As TextBox()
        Get
            Return {TextBox_Pairing1Home2, TextBox_Pairing2Home2, TextBox_Pairing3Home2, TextBox_Pairing4Home2}
        End Get
    End Property

    Private ReadOnly Property PairingAway2TextBoxes As TextBox()
        Get
            Return {TextBox_Pairing1Away2, TextBox_Pairing2Away2, TextBox_Pairing3Away2, TextBox_Pairing4Away2}
        End Get
    End Property

    Private ReadOnly Property PairingHome2Labels As Label()
        Get
            Return {Label_Pairing1Home2, Label_Pairing2Home2, Label_Pairing3Home2, Label_Pairing4Home2}
        End Get
    End Property

    Private ReadOnly Property PairingAway2Labels As Label()
        Get
            Return {Label_Pairing1Away2, Label_Pairing2Away2, Label_Pairing3Away2, Label_Pairing4Away2}
        End Get
    End Property

    Private Sub Tennis26_Main2_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        InitializeDataGrid()
        LoadPlayersFromXml()
        LoadPairingsFromXml()
        RefreshAllPairingDisplays()
        For i = 0 To PAIRING_COUNT - 1
            UpdateDoublesFieldsVisibility(i)
        Next
    End Sub

    Private Sub InitializeDataGrid()
        DataGridView_Players2.Columns.Clear()
        DataGridView_Players2.Columns.Add("Name", "Name")
        DataGridView_Players2.Columns.Add("FirstName", "First Name")
        DataGridView_Players2.Columns.Add("Country", "Country")
        DataGridView_Players2.Columns.Add("CountryISO3", "ISO3")
        DataGridView_Players2.Columns.Add("Age", "Age")
        DataGridView_Players2.Columns.Add("Height", "Height")
        DataGridView_Players2.Columns.Add("Data1", "Ranking")
        DataGridView_Players2.Columns.Add("Data2", "Points")
        DataGridView_Players2.Columns.Add("Data3", "Association")

        DataGridView_Players2.AllowUserToAddRows = False
        DataGridView_Players2.AllowUserToDeleteRows = False
        DataGridView_Players2.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        DataGridView_Players2.MultiSelect = False
    End Sub

    Private Sub LoadPlayersFromXml()
        Try
            If Not IO.File.Exists(XML_FILE_PATH) Then Return

            Dim xmlDoc As New Xml.XmlDocument()
            xmlDoc.Load(XML_FILE_PATH)

            DataGridView_Players2.Rows.Clear()

            Dim playersNode = xmlDoc.SelectSingleNode("//TennisData/Players")
            If playersNode IsNot Nothing Then
                For Each playerNode As Xml.XmlNode In playersNode.SelectNodes("Player")
                    Dim values(8) As String
                    For i = 0 To 8
                        Dim fieldNode = playerNode.SelectSingleNode(FIELD_NAMES(i))
                        values(i) = If(fieldNode IsNot Nothing, fieldNode.InnerText, "")
                    Next
                    DataGridView_Players2.Rows.Add(values)
                Next
            End If
        Catch ex As Exception
            MessageBox.Show($"Error loading player data: {ex.Message}", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ' Drag-Start aus dem Grid - identisches Muster wie DataGridView_Players in Tennis26_Main.
    Private Sub DataGridView_Players2_MouseDown(sender As Object, e As MouseEventArgs) Handles DataGridView_Players2.MouseDown
        If e.Button = MouseButtons.Left Then
            Dim hit = DataGridView_Players2.HitTest(e.X, e.Y)
            If hit.Type = DataGridViewHitTestType.Cell AndAlso hit.RowIndex >= 0 Then
                _dragStartPoint = e.Location
                _dragRowIndex = hit.RowIndex
            Else
                _dragRowIndex = -1
            End If
        End If
    End Sub

    Private Sub DataGridView_Players2_MouseUp(sender As Object, e As MouseEventArgs) Handles DataGridView_Players2.MouseUp
        _dragRowIndex = -1
    End Sub

    Private Sub DataGridView_Players2_MouseMove(sender As Object, e As MouseEventArgs) Handles DataGridView_Players2.MouseMove
        If (e.Button And MouseButtons.Left) = MouseButtons.Left AndAlso _dragRowIndex >= 0 Then
            Dim dragSize = SystemInformation.DragSize
            Dim dragRect As New Rectangle(_dragStartPoint.X - dragSize.Width \ 2, _dragStartPoint.Y - dragSize.Height \ 2, dragSize.Width, dragSize.Height)
            If Not dragRect.Contains(e.Location) Then
                Dim row = DataGridView_Players2.Rows(_dragRowIndex)
                Dim playerData = String.Join("|", row.Cells.Cast(Of DataGridViewCell)().Select(Function(c) If(c.Value?.ToString(), "")))
                DataGridView_Players2.DoDragDrop(playerData, DragDropEffects.Copy)
                _dragRowIndex = -1
            End If
        End If
    End Sub

    ' Gemeinsamer Drop-Handler für alle Zielfelder (4 Paarungen x Home/Away/Home2/Away2) -
    ' welches Feld welcher Paarungs-Slot ist, steckt im Tag der jeweiligen TextBox
    ' (siehe SetupDropTarget), damit nicht 16 fast identische Handler nötig sind.
    Private Sub DropTarget_DragEnter(sender As Object, e As DragEventArgs)
        If e.Data.GetDataPresent(DataFormats.Text) Then
            e.Effect = DragDropEffects.Copy
        End If
    End Sub

    Private Sub DropTarget_DragDrop(sender As Object, e As DragEventArgs)
        Try
            Dim textBox = DirectCast(sender, TextBox)
            Dim target = DirectCast(textBox.Tag, Tuple(Of Integer, String))
            Dim pairingIndex = target.Item1
            Dim slotName = target.Item2

            Dim playerData = e.Data.GetData(DataFormats.Text).ToString()
            Dim playerFields = playerData.Split("|"c)
            If playerFields.Length < 9 Then Return

            If IsPlayerAlreadyInPairing(pairingIndex, slotName, playerFields) Then
                MessageBox.Show($"{playerFields(1)} {playerFields(0)} is already assigned elsewhere in this pairing.", "Duplicate Player", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Dim targetArray As String() = Nothing
            Select Case slotName
                Case "Home" : targetArray = pairings(pairingIndex).Home
                Case "Away" : targetArray = pairings(pairingIndex).Away
                Case "Home2" : targetArray = pairings(pairingIndex).Home2
                Case "Away2" : targetArray = pairings(pairingIndex).Away2
            End Select

            For i = 0 To 8
                targetArray(i) = playerFields(i)
            Next

            RefreshPairingDisplay(pairingIndex)
        Catch ex As Exception
            MessageBox.Show($"Error assigning player: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Verhindert, denselben Spieler zweimal in dieselbe Paarung zu ziehen (z.B. versehentlich
    ' als Home UND als Away, oder als Home UND als Home2 im Doppel) - Name+Vorname als
    ' Identität, reicht hier aus (zwei verschiedene Spieler mit identischem Namen+Vornamen
    ' gleichzeitig in derselben Paarung wäre ohnehin ein Sonderfall).
    Private Function IsPlayerAlreadyInPairing(pairingIndex As Integer, targetSlotName As String, playerFields As String()) As Boolean
        Dim slot = pairings(pairingIndex)
        Dim otherSlots = New List(Of (Name As String, Fields As String())) From {
            ("Home", slot.Home),
            ("Away", slot.Away),
            ("Home2", slot.Home2),
            ("Away2", slot.Away2)
        }

        For Each entry In otherSlots
            If entry.Name = targetSlotName Then Continue For
            If String.IsNullOrEmpty(entry.Fields(0)) AndAlso String.IsNullOrEmpty(entry.Fields(1)) Then Continue For
            If entry.Fields(0) = playerFields(0) AndAlso entry.Fields(1) = playerFields(1) Then
                Return True
            End If
        Next
        Return False
    End Function

    ' Verknüpft eine TextBox mit ihrem Paarungs-Slot/Feld (via Tag) und meldet sie für den
    ' gemeinsamen Drag&Drop-Handler an - wird einmal pro Zielfeld beim Laden aufgerufen.
    Private Sub SetupDropTarget(textBox As TextBox, pairingIndex As Integer, slotName As String)
        textBox.Tag = New Tuple(Of Integer, String)(pairingIndex, slotName)
        textBox.AllowDrop = True
        textBox.ReadOnly = True
        AddHandler textBox.DragEnter, AddressOf DropTarget_DragEnter
        AddHandler textBox.DragDrop, AddressOf DropTarget_DragDrop
    End Sub

    ' Leer, wenn kein Spieler zugewiesen ist - die feste Instruktion ("Drag HOME player
    ' here") steht als eigenes Label über dem Feld (siehe Designer), damit ein leeres Feld
    ' auch wirklich leer aussieht statt mit Platzhaltertext gefüllt zu wirken.
    Private Function PlayerDisplayText(fields As String()) As String
        If String.IsNullOrEmpty(fields(0)) AndAlso String.IsNullOrEmpty(fields(1)) Then Return ""
        Return $"{fields(1)} {fields(0)} ({fields(3)})"
    End Function

    Private Sub RefreshPairingDisplay(pairingIndex As Integer)
        Dim slot = pairings(pairingIndex)
        PairingHomeTextBoxes(pairingIndex).Text = PlayerDisplayText(slot.Home)
        PairingAwayTextBoxes(pairingIndex).Text = PlayerDisplayText(slot.Away)
        PairingHome2TextBoxes(pairingIndex).Text = PlayerDisplayText(slot.Home2)
        PairingAway2TextBoxes(pairingIndex).Text = PlayerDisplayText(slot.Away2)
    End Sub

    ' Home2/Away2 (samt ihrer Labels) nur sichtbar, wenn für DIESE Paarung Doubles gesetzt
    ' ist - unabhängig von den anderen Paarungen.
    Private Sub UpdateDoublesFieldsVisibility(pairingIndex As Integer)
        Dim isDoubles = PairingDoublesCheckBoxes(pairingIndex).Checked
        PairingHome2Labels(pairingIndex).Visible = isDoubles
        PairingAway2Labels(pairingIndex).Visible = isDoubles
        PairingHome2TextBoxes(pairingIndex).Visible = isDoubles
        PairingAway2TextBoxes(pairingIndex).Visible = isDoubles
    End Sub

    Private Sub RefreshAllPairingDisplays()
        For i = 0 To PAIRING_COUNT - 1
            SetupDropTarget(PairingHomeTextBoxes(i), i, "Home")
            SetupDropTarget(PairingAwayTextBoxes(i), i, "Away")
            SetupDropTarget(PairingHome2TextBoxes(i), i, "Home2")
            SetupDropTarget(PairingAway2TextBoxes(i), i, "Away2")
            RefreshPairingDisplay(i)
        Next
    End Sub

    Private Sub ClearPairing(pairingIndex As Integer)
        Dim confirmResult = MessageBox.Show(
            $"Clear Pairing {pairingIndex + 1}? This cannot be undone.",
            "Clear Pairing", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
        If confirmResult <> DialogResult.Yes Then Return

        pairings(pairingIndex) = New PairingSlot()
        PairingDoublesCheckBoxes(pairingIndex).Checked = False
        RefreshPairingDisplay(pairingIndex)
        UpdateDoublesFieldsVisibility(pairingIndex)
    End Sub

    Private Sub Btn_Pairing1Clear_Click(sender As Object, e As EventArgs) Handles Btn_Pairing1Clear.Click
        ClearPairing(0)
    End Sub

    Private Sub Btn_Pairing2Clear_Click(sender As Object, e As EventArgs) Handles Btn_Pairing2Clear.Click
        ClearPairing(1)
    End Sub

    Private Sub Btn_Pairing3Clear_Click(sender As Object, e As EventArgs) Handles Btn_Pairing3Clear.Click
        ClearPairing(2)
    End Sub

    Private Sub Btn_Pairing4Clear_Click(sender As Object, e As EventArgs) Handles Btn_Pairing4Clear.Click
        ClearPairing(3)
    End Sub

    Private Sub CheckBox_Pairing1Doubles_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox_Pairing1Doubles.CheckedChanged
        UpdateDoublesFieldsVisibility(0)
    End Sub

    Private Sub CheckBox_Pairing2Doubles_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox_Pairing2Doubles.CheckedChanged
        UpdateDoublesFieldsVisibility(1)
    End Sub

    Private Sub CheckBox_Pairing3Doubles_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox_Pairing3Doubles.CheckedChanged
        UpdateDoublesFieldsVisibility(2)
    End Sub

    Private Sub CheckBox_Pairing4Doubles_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox_Pairing4Doubles.CheckedChanged
        UpdateDoublesFieldsVisibility(3)
    End Sub

    ' Markiert eine Paarung als "aktiv" (LightBlue + "Pairing X ACTIVE") und setzt die
    ' anderen auf ihr normales Aussehen zurück - immer nur eine Paarung kann aktiv sein,
    ' analog zu "das ist gerade das laufende Match".
    Private Sub SetActivePairing(pairingIndex As Integer)
        activeIndex = pairingIndex
        Dim groupBoxes = PairingGroupBoxes
        For i = 0 To PAIRING_COUNT - 1
            If i = pairingIndex Then
                groupBoxes(i).BackColor = Color.LightBlue
                groupBoxes(i).Text = $"Pairing {i + 1} ACTIVE"
            Else
                groupBoxes(i).BackColor = SystemColors.ControlLight
                groupBoxes(i).Text = $"Pairing {i + 1}"
            End If
        Next
    End Sub

    ' Übernimmt eine vorbereitete Paarung als aktuelles Match: schreibt in dieselben
    ' HomePlayer/AwayPlayer/HomePlayer2/AwayPlayer2-Arrays und dieselbe CheckBox1, die auch
    ' Tennis26_Main verwendet - Main und Scorer merken vom Mechanismus her nichts davon, dass
    ' die Auswahl aus Main2 statt per Drag in Main selbst kam.
    Private Sub ActivatePairing(pairingIndex As Integer, isDoubles As Boolean)
        Dim slot = pairings(pairingIndex)

        If String.IsNullOrEmpty(slot.Home(0)) OrElse String.IsNullOrEmpty(slot.Away(0)) Then
            MessageBox.Show("This pairing needs at least a Home and an Away player before it can be activated.", "Pairing incomplete", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        slot.Doubles = isDoubles

        For i = 0 To 8
            Tennis26_Main.HomePlayer(i) = slot.Home(i)
            Tennis26_Main.AwayPlayer(i) = slot.Away(i)
            Tennis26_Main.HomePlayer2(i) = If(isDoubles, slot.Home2(i), "")
            Tennis26_Main.AwayPlayer2(i) = If(isDoubles, slot.Away2(i), "")
        Next
        Tennis26_Main.CheckBox1.Checked = isDoubles

        Tennis26_Main.RefreshAndSavePlayerSelection()
        SetActivePairing(pairingIndex)
    End Sub

    Private Sub Btn_Pairing1Activate_Click(sender As Object, e As EventArgs) Handles Btn_Pairing1Activate.Click
        ActivatePairing(0, CheckBox_Pairing1Doubles.Checked)
    End Sub

    Private Sub Btn_Pairing2Activate_Click(sender As Object, e As EventArgs) Handles Btn_Pairing2Activate.Click
        ActivatePairing(1, CheckBox_Pairing2Doubles.Checked)
    End Sub

    Private Sub Btn_Pairing3Activate_Click(sender As Object, e As EventArgs) Handles Btn_Pairing3Activate.Click
        ActivatePairing(2, CheckBox_Pairing3Doubles.Checked)
    End Sub

    Private Sub Btn_Pairing4Activate_Click(sender As Object, e As EventArgs) Handles Btn_Pairing4Activate.Click
        ActivatePairing(3, CheckBox_Pairing4Doubles.Checked)
    End Sub

    ' Speichert alle vorbereiteten Paarungen (inkl. Doubles-Häkchen und welche gerade aktiv
    ' ist) in eine eigene XML-Datei - unabhängig von tennisdata.xml, damit die Vorbereitung
    ' auch über einen Neustart von Tennis26_Main2 hinweg erhalten bleibt.
    Private Sub Btn_save_Click(sender As Object, e As EventArgs) Handles Btn_save.Click
        Try
            SavePairingsToXml()
            MessageBox.Show("Pairings saved.", "Save Pairings", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show($"Error saving pairings: {ex.Message}", "Save Pairings", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub Btn_exit_Click(sender As Object, e As EventArgs) Handles Btn_exit.Click
        Me.Close()
    End Sub

    Private Sub SavePairingsToXml()
        Dim directoryPath = IO.Path.GetDirectoryName(PAIRINGS_FILE_PATH)
        If Not IO.Directory.Exists(directoryPath) Then IO.Directory.CreateDirectory(directoryPath)

        Dim xmlDoc As New Xml.XmlDocument()
        Dim xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", Nothing)
        xmlDoc.AppendChild(xmlDeclaration)

        Dim root = xmlDoc.CreateElement("TennisPairings")
        xmlDoc.AppendChild(root)

        Dim activeIndexElement = xmlDoc.CreateElement("ActiveIndex")
        activeIndexElement.InnerText = activeIndex.ToString()
        root.AppendChild(activeIndexElement)

        For i = 0 To PAIRING_COUNT - 1
            Dim pairingElement = xmlDoc.CreateElement("Pairing")
            pairingElement.SetAttribute("index", i.ToString())
            root.AppendChild(pairingElement)

            Dim doublesElement = xmlDoc.CreateElement("Doubles")
            doublesElement.InnerText = PairingDoublesCheckBoxes(i).Checked.ToString()
            pairingElement.AppendChild(doublesElement)

            AppendPlayerElement(xmlDoc, pairingElement, "Home", pairings(i).Home)
            AppendPlayerElement(xmlDoc, pairingElement, "Away", pairings(i).Away)
            AppendPlayerElement(xmlDoc, pairingElement, "Home2", pairings(i).Home2)
            AppendPlayerElement(xmlDoc, pairingElement, "Away2", pairings(i).Away2)
        Next

        Using writer As New Xml.XmlTextWriter(PAIRINGS_FILE_PATH, System.Text.Encoding.UTF8)
            writer.Formatting = Xml.Formatting.Indented
            xmlDoc.Save(writer)
        End Using
    End Sub

    Private Sub AppendPlayerElement(xmlDoc As Xml.XmlDocument, parent As Xml.XmlElement, elementName As String, fields As String())
        Dim playerElement = xmlDoc.CreateElement(elementName)
        parent.AppendChild(playerElement)
        For i = 0 To 8
            Dim fieldElement = xmlDoc.CreateElement(FIELD_NAMES(i))
            fieldElement.InnerText = If(fields(i), "")
            playerElement.AppendChild(fieldElement)
        Next
    End Sub

    Private Sub LoadPairingsFromXml()
        Try
            If Not IO.File.Exists(PAIRINGS_FILE_PATH) Then Return

            Dim xmlDoc As New Xml.XmlDocument()
            xmlDoc.Load(PAIRINGS_FILE_PATH)

            Dim activeIndexNode = xmlDoc.SelectSingleNode("//TennisPairings/ActiveIndex")
            Dim loadedActiveIndex As Integer = -1
            If activeIndexNode IsNot Nothing Then Integer.TryParse(activeIndexNode.InnerText, loadedActiveIndex)

            For Each pairingNode As Xml.XmlNode In xmlDoc.SelectNodes("//TennisPairings/Pairing")
                Dim indexAttr = pairingNode.Attributes("index")
                If indexAttr Is Nothing Then Continue For
                Dim pairingIndex As Integer
                If Not Integer.TryParse(indexAttr.Value, pairingIndex) OrElse pairingIndex < 0 OrElse pairingIndex >= PAIRING_COUNT Then Continue For

                Dim doublesNode = pairingNode.SelectSingleNode("Doubles")
                Dim isDoubles As Boolean = False
                If doublesNode IsNot Nothing Then Boolean.TryParse(doublesNode.InnerText, isDoubles)
                pairings(pairingIndex).Doubles = isDoubles
                PairingDoublesCheckBoxes(pairingIndex).Checked = isDoubles

                ReadPlayerElement(pairingNode, "Home", pairings(pairingIndex).Home)
                ReadPlayerElement(pairingNode, "Away", pairings(pairingIndex).Away)
                ReadPlayerElement(pairingNode, "Home2", pairings(pairingIndex).Home2)
                ReadPlayerElement(pairingNode, "Away2", pairings(pairingIndex).Away2)
            Next

            If loadedActiveIndex >= 0 AndAlso loadedActiveIndex < PAIRING_COUNT Then
                SetActivePairing(loadedActiveIndex)
            End If
        Catch ex As Exception
            MessageBox.Show($"Error loading saved pairings: {ex.Message}", "Load Pairings", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub ReadPlayerElement(pairingNode As Xml.XmlNode, elementName As String, fields As String())
        Dim playerNode = pairingNode.SelectSingleNode(elementName)
        If playerNode Is Nothing Then Return
        For i = 0 To 8
            Dim fieldNode = playerNode.SelectSingleNode(FIELD_NAMES(i))
            fields(i) = If(fieldNode IsNot Nothing, fieldNode.InnerText, "")
        Next
    End Sub

End Class
