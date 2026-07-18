Public Class Tennis24_Main
    ' Array to store selected players for the match
    Public Shared HomePlayer As String() = New String(8) {} ' Name, FirstName, Country, ISO3, Age, Height, Data1, Data2, Data3
    Public Shared AwayPlayer As String() = New String(8) {} ' Name, FirstName, Country, ISO3, Age, Height, Data1, Data2, Data3

    ' XML file path
    Private Const XML_FILE_PATH As String = "c:\vmix\tennis\data\tennisdata.xml"

    ' Feldnamen als konstante Array
    Private Shared ReadOnly FIELD_NAMES As String() = {"Name", "FirstName", "Country", "CountryISO3", "Age", "Height", "Data1", "Data2", "Data3"}

    Private _dragStartPoint As Point
    Private _dragRowIndex As Integer = -1

    Private Sub Tennis24_Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            ' macht kopie der spielerdaten
            CreateAutoBackup()

            InitializeDataGrid()
            InitializePlayerFields()

            LoadDataFromXML() 'reads player data and selected players from XML for main form

            Tennis24_Settings.LoadSettingsFromXml() 'reads settings from XLM
            Tennis24_Settings.SetLabels() ' sets labels in settings form

            ' Sichere Anzeige mit Fallback
            UpdateBestOfLabel()

            Me.Text = My.Application.Info.AssemblyName + " " + My.Application.Info.Version.ToString() + " | " + My.Application.Info.Copyright.ToString()
        Catch ex As Exception
            MessageBox.Show($"Fehler beim Initialisieren der Anwendung: {ex.Message}", "Initialisierungsfehler", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub UpdateBestOfLabel()
        Dim bestOfValue = If(String.IsNullOrEmpty(Tennis24_Settings.TextBoxValues(50)), "3", Tennis24_Settings.TextBoxValues(50))
        Label2.Text = "best of: " + bestOfValue
    End Sub


    Private Sub Btn_live_Click(sender As Object, e As EventArgs) Handles btn_live.Click
        Try
            ' Prüfe zuerst die Spielerpaarung
            If Not CheckCurrentMatchPairing() Then
                Return ' Beende die Funktion hier wenn Prüfung fehlschlägt
            End If

            Tennis24_Settings.SetVariables()
            Tennis24_Scorer.Show()

            ' Sichere String-Konkatenation mit Fallbacks
            Dim bestOf = If(String.IsNullOrEmpty(Tennis24_Settings.TextBoxValues(50)), "3", Tennis24_Settings.TextBoxValues(50))
            Dim vmixIP = If(String.IsNullOrEmpty(Tennis24_Settings.TextBoxValues(45)), "localhost", Tennis24_Settings.TextBoxValues(45))

            Tennis24_Scorer.Text = $"LIVE | Best of {bestOf} | IP: {vmixIP}"
            Me.Hide()
        Catch ex As Exception
            MessageBox.Show($"Fehler beim Starten des Live-Modus: {ex.Message}", "Live-Modus Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub InitializeDataGrid()
        ' Clear existing columns
        DataGridView_Players.Columns.Clear()

        ' Add columns with specified fields
        DataGridView_Players.Columns.Add("Name", "Name")
        DataGridView_Players.Columns.Add("FirstName", "First Name")
        DataGridView_Players.Columns.Add("Country", "Country")
        DataGridView_Players.Columns.Add("CountryISO3", "Country ISO3")
        DataGridView_Players.Columns.Add("Age", "Age")
        DataGridView_Players.Columns.Add("Height", "Height")
        DataGridView_Players.Columns.Add("Data1", "Ranking")
        DataGridView_Players.Columns.Add("Data2", "Points")
        DataGridView_Players.Columns.Add("Data3", "Association")

        ' Sortieren aktivieren
        For Each colName In New String() {"Name", "FirstName", "Country", "CountryISO3", "Age", "Height", "Data1", "Data2", "Data3"}
            DataGridView_Players.Columns(colName).SortMode = DataGridViewColumnSortMode.Automatic
        Next

        ' Spaltentypen setzen (für numerische Sortierung wichtig)
        DataGridView_Players.Columns("Age").ValueType = GetType(Integer)
        DataGridView_Players.Columns("Height").ValueType = GetType(Integer)
        DataGridView_Players.Columns("Data1").ValueType = GetType(Integer) ' Ranking
        DataGridView_Players.Columns("Data2").ValueType = GetType(Integer) ' Points

        ' Optionale Ausrichtung für Zahlen
        DataGridView_Players.Columns("Age").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
        DataGridView_Players.Columns("Height").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
        DataGridView_Players.Columns("Data1").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
        DataGridView_Players.Columns("Data2").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight

        ' Set grid properties
        DataGridView_Players.AllowUserToAddRows = False
        DataGridView_Players.AllowUserToDeleteRows = False
        DataGridView_Players.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        DataGridView_Players.MultiSelect = False

        DataGridView_Players.DefaultCellStyle.SelectionBackColor = Color.LightBlue
        DataGridView_Players.DefaultCellStyle.SelectionForeColor = Color.Black

        DataGridView_Players.CellBorderStyle = DataGridViewCellBorderStyle.Single
        DataGridView_Players.GridColor = Color.DarkGray

        ' Event-Handler für bessere Zellenanzeige
        AddHandler DataGridView_Players.CurrentCellDirtyStateChanged, AddressOf DataGridView_CurrentCellDirtyStateChanged

        ' Adjust column widths
        DataGridView_Players.Columns("Name").Width = 120
        DataGridView_Players.Columns("FirstName").Width = 120
        DataGridView_Players.Columns("Country").Width = 100
        DataGridView_Players.Columns("CountryISO3").Width = 80
        DataGridView_Players.Columns("Age").Width = 60
        DataGridView_Players.Columns("Height").Width = 70
        DataGridView_Players.Columns("Data1").Width = 90
        DataGridView_Players.Columns("Data2").Width = 90
        DataGridView_Players.Columns("Data3").Width = 90

        DataGridView_Players.DefaultCellStyle.Font = New Font("Segoe UI", 8, FontStyle.Regular)
        DataGridView_Players.ColumnHeadersDefaultCellStyle.Font = New Font("Segoe UI", 8, FontStyle.Bold)
    End Sub

    ' Sorgt für korrekte Sortierung (numerisch für Age/Height/Ranking/Points, sonst case-insensitive Text)
    ' Data3 mit Sekundärfeld Name immer aufsteigend, sonst case-insensitive Text)
    Private Sub DataGridView_Players_SortCompare(sender As Object, e As DataGridViewSortCompareEventArgs) Handles DataGridView_Players.SortCompare
        Select Case e.Column.Name
            Case "Age", "Height", "Data1", "Data2"
                Dim v1 As Integer, v2 As Integer
                If Not Integer.TryParse(Convert.ToString(e.CellValue1), v1) Then v1 = Integer.MinValue
                If Not Integer.TryParse(Convert.ToString(e.CellValue2), v2) Then v2 = Integer.MinValue
                e.SortResult = v1.CompareTo(v2)

                If e.SortResult = 0 Then
                    e.SortResult = String.Compare(
                    Convert.ToString(DataGridView_Players.Rows(e.RowIndex1).Cells("Name").Value),
                    Convert.ToString(DataGridView_Players.Rows(e.RowIndex2).Cells("Name").Value),
                    StringComparison.CurrentCultureIgnoreCase)
                End If

                e.Handled = True

            Case "Data3"
                ' Primär: Association (Data3)
                e.SortResult = String.Compare(
                Convert.ToString(e.CellValue1),
                Convert.ToString(e.CellValue2),
                StringComparison.CurrentCultureIgnoreCase)

                If e.SortResult = 0 Then
                    ' Sekundär: Name immer aufsteigend (unabhängig von Data3-Richtung)
                    Dim nameCompare = String.Compare(
                    Convert.ToString(DataGridView_Players.Rows(e.RowIndex1).Cells("Name").Value),
                    Convert.ToString(DataGridView_Players.Rows(e.RowIndex2).Cells("Name").Value),
                    StringComparison.CurrentCultureIgnoreCase)

                    ' Wenn Data3 absteigend sortiert wird, kehrt die DataGridView das Gesamtergebnis um.
                    ' Damit der Name trotzdem aufsteigend bleibt, invertieren wir hier das Vorzeichen.
                    If DataGridView_Players.SortedColumn Is e.Column AndAlso DataGridView_Players.SortOrder = SortOrder.Descending Then
                        nameCompare = -nameCompare
                    End If

                    If nameCompare = 0 Then
                        ' Tertiär: FirstName ebenfalls immer aufsteigend behandeln
                        Dim firstNameCompare = String.Compare(
                        Convert.ToString(DataGridView_Players.Rows(e.RowIndex1).Cells("FirstName").Value),
                        Convert.ToString(DataGridView_Players.Rows(e.RowIndex2).Cells("FirstName").Value),
                        StringComparison.CurrentCultureIgnoreCase)

                        If DataGridView_Players.SortedColumn Is e.Column AndAlso DataGridView_Players.SortOrder = SortOrder.Descending Then
                            firstNameCompare = -firstNameCompare
                        End If

                        e.SortResult = firstNameCompare
                    Else
                        e.SortResult = nameCompare
                    End If
                End If

                e.Handled = True

            Case Else
                ' Textsortierung (case-insensitive) mit Sekundärfeld FirstName
                e.SortResult = String.Compare(
                Convert.ToString(e.CellValue1),
                Convert.ToString(e.CellValue2),
                StringComparison.CurrentCultureIgnoreCase)

                If e.SortResult = 0 Then
                    e.SortResult = String.Compare(
                    Convert.ToString(DataGridView_Players.Rows(e.RowIndex1).Cells("FirstName").Value),
                    Convert.ToString(DataGridView_Players.Rows(e.RowIndex2).Cells("FirstName").Value),
                    StringComparison.CurrentCultureIgnoreCase)
                End If

                e.Handled = True
        End Select
    End Sub

    ' Event-Handler für Zellwechsel
    Private Sub DataGridView_Players_CurrentCellChanged(sender As Object, e As EventArgs) Handles DataGridView_Players.CurrentCellChanged
        If DataGridView_Players.CurrentCell IsNot Nothing Then
            ' Zeige aktuelle Zellenposition in der Statusleiste oder einem Label
            Dim row As Integer = DataGridView_Players.CurrentCell.RowIndex + 1
            Dim col As String = DataGridView_Players.CurrentCell.OwningColumn.HeaderText

            ' Optional: Label oder Statusleiste aktualisieren
            ' Me.Text = $"Tennis24 - Aktuelle Zelle: {col} (Zeile {row})"

            ' Alternative: ToolTip anzeigen
            Dim tooltip As New ToolTip()
            tooltip.SetToolTip(DataGridView_Players, $"Aktuelle Zelle: {col} - Zeile {row}")
        End If
    End Sub

    ' Event-Handler für Bearbeitungsmodus
    Private Sub DataGridView_Players_CellBeginEdit(sender As Object, e As DataGridViewCellCancelEventArgs) Handles DataGridView_Players.CellBeginEdit
        ' Zelle beim Bearbeiten zusätzlich hervorheben
        DataGridView_Players.CurrentCell.Style.BackColor = Color.Yellow
        DataGridView_Players.CurrentCell.Style.ForeColor = Color.Black
    End Sub

    Private Sub DataGridView_Players_CellEndEdit(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView_Players.CellEndEdit
        ' Hervorhebung nach Bearbeitung zurücksetzen
        DataGridView_Players.CurrentCell.Style.BackColor = Color.Empty
        DataGridView_Players.CurrentCell.Style.ForeColor = Color.Empty
    End Sub

    Private Sub DataGridView_CurrentCellDirtyStateChanged(sender As Object, e As EventArgs)
        ' Sofortiges Speichern von Änderungen
        If DataGridView_Players.IsCurrentCellDirty Then
            DataGridView_Players.CommitEdit(DataGridViewDataErrorContexts.Commit)
        End If
    End Sub

    Private Sub InitializePlayerFields()
        ' Enable drag and drop for the textboxes
        txt_home_player.AllowDrop = True
        txt_away_player.AllowDrop = True

        ' Make them read-only so users can't type directly
        txt_home_player.ReadOnly = True
        txt_away_player.ReadOnly = True
    End Sub

    Private Sub LoadSampleData()
        ' Add some sample data
        DataGridView_Players.Rows.Add("Federer", "Roger", "Switzerland", "CHE", 42, 185, "Sample1", "Sample2", "Sample3")
        DataGridView_Players.Rows.Add("Nadal", "Rafael", "Spain", "ESP", 37, 186, "Data1", "Data2", "Data3")
        DataGridView_Players.Rows.Add("Djokovic", "Novak", "Serbia", "SRB", 36, 188, "Info1", "Info2", "Info3")
        DataGridView_Players.Rows.Add("Murray", "Andy", "United Kingdom", "GBR", 36, 190, "Test1", "Test2", "Test3")
    End Sub

    Private Sub LoadDataFromXML()
        Try
            If System.IO.File.Exists(XML_FILE_PATH) Then
                Dim xmlDoc As New System.Xml.XmlDocument()
                xmlDoc.Load(XML_FILE_PATH)

                ' Clear existing data
                DataGridView_Players.Rows.Clear()

                ' Load players data
                Dim playersNode = xmlDoc.SelectSingleNode("//TennisData/Players")
                If playersNode IsNot Nothing Then
                    For Each playerNode As System.Xml.XmlNode In playersNode.SelectNodes("Player")
                        Dim name = GetXmlNodeValue(playerNode, "Name")
                        Dim firstName = GetXmlNodeValue(playerNode, "FirstName")
                        Dim country = GetXmlNodeValue(playerNode, "Country")
                        Dim countryISO3 = GetXmlNodeValue(playerNode, "CountryISO3")
                        Dim age = GetXmlNodeValue(playerNode, "Age")
                        Dim height = GetXmlNodeValue(playerNode, "Height")
                        Dim data1 = GetXmlNodeValue(playerNode, "Data1")
                        Dim data2 = GetXmlNodeValue(playerNode, "Data2")
                        Dim data3 = GetXmlNodeValue(playerNode, "Data3")

                        DataGridView_Players.Rows.Add(name, firstName, country, countryISO3, age, height, data1, data2, data3)
                    Next
                End If

                ' Load selected home player
                Dim homePlayerNode = xmlDoc.SelectSingleNode("//TennisData/SelectedPlayers/HomePlayer")
                If homePlayerNode IsNot Nothing AndAlso homePlayerNode.HasChildNodes Then
                    HomePlayer(0) = GetXmlNodeValue(homePlayerNode, "Name")
                    HomePlayer(1) = GetXmlNodeValue(homePlayerNode, "FirstName")
                    HomePlayer(2) = GetXmlNodeValue(homePlayerNode, "Country")
                    HomePlayer(3) = GetXmlNodeValue(homePlayerNode, "CountryISO3")
                    HomePlayer(4) = GetXmlNodeValue(homePlayerNode, "Age")
                    HomePlayer(5) = GetXmlNodeValue(homePlayerNode, "Height")
                    HomePlayer(6) = GetXmlNodeValue(homePlayerNode, "Data1")
                    HomePlayer(7) = GetXmlNodeValue(homePlayerNode, "Data2")
                    HomePlayer(8) = GetXmlNodeValue(homePlayerNode, "Data3")
                End If

                ' Load selected away player
                Dim awayPlayerNode = xmlDoc.SelectSingleNode("//TennisData/SelectedPlayers/AwayPlayer")
                If awayPlayerNode IsNot Nothing AndAlso awayPlayerNode.HasChildNodes Then
                    AwayPlayer(0) = GetXmlNodeValue(awayPlayerNode, "Name")
                    AwayPlayer(1) = GetXmlNodeValue(awayPlayerNode, "FirstName")
                    AwayPlayer(2) = GetXmlNodeValue(awayPlayerNode, "Country")
                    AwayPlayer(3) = GetXmlNodeValue(awayPlayerNode, "CountryISO3")
                    AwayPlayer(4) = GetXmlNodeValue(awayPlayerNode, "Age")
                    AwayPlayer(5) = GetXmlNodeValue(awayPlayerNode, "Height")
                    AwayPlayer(6) = GetXmlNodeValue(awayPlayerNode, "Data1")
                    AwayPlayer(7) = GetXmlNodeValue(awayPlayerNode, "Data2")
                    AwayPlayer(8) = GetXmlNodeValue(awayPlayerNode, "Data3")
                End If

                ' Update the display after loading both players
                UpdatePlayerDisplay()

            Else
                ' If XML file doesn't exist, load sample data and set default display
                LoadSampleData()
                SetDefaultPlayerDisplay()
            End If
        Catch ex As Exception
            MessageBox.Show("Error loading data from XML: " & ex.Message & vbNewLine & "Loading sample data instead.", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            LoadSampleData()
            SetDefaultPlayerDisplay()
        End Try
    End Sub

    Private Sub UpdatePlayerDisplay()
        ' Update Home Player display
        If Not String.IsNullOrEmpty(HomePlayer(0)) AndAlso Not String.IsNullOrEmpty(HomePlayer(1)) Then
            txt_home_player.Text = $"{HomePlayer(1)} {HomePlayer(0)} ({HomePlayer(3)})"
            txt_home_player.BackColor = Color.LightGreen
        Else
            txt_home_player.Text = "Drag player here for HOME"
            txt_home_player.BackColor = SystemColors.Window
        End If

        ' Update Away Player display
        If Not String.IsNullOrEmpty(AwayPlayer(0)) AndAlso Not String.IsNullOrEmpty(AwayPlayer(1)) Then
            txt_away_player.Text = $"{AwayPlayer(1)} {AwayPlayer(0)} ({AwayPlayer(3)})"
            txt_away_player.BackColor = Color.LightBlue
        Else
            txt_away_player.Text = "Drag player here for AWAY"
            txt_away_player.BackColor = SystemColors.Window
        End If
    End Sub

    Private Sub SetDefaultPlayerDisplay()
        ' Set default text when no XML file exists
        txt_home_player.Text = "Drag player here for HOME"
        txt_away_player.Text = "Drag player here for AWAY"
        txt_home_player.BackColor = SystemColors.Window
        txt_away_player.BackColor = SystemColors.Window
    End Sub

    Private Function GetXmlNodeValue(parentNode As System.Xml.XmlNode, nodeName As String) As String
        Dim node = parentNode.SelectSingleNode(nodeName)
        Return If(node IsNot Nothing, node.InnerText, "")
    End Function

    Private Sub SaveDataToXML()
        Try
            ' Create directory if it doesn't exist
            Dim xmlDirectory = System.IO.Path.GetDirectoryName(XML_FILE_PATH)
            If Not System.IO.Directory.Exists(xmlDirectory) Then
                System.IO.Directory.CreateDirectory(xmlDirectory)
            End If

            Dim xmlDoc As New System.Xml.XmlDocument()

            ' Create XML declaration
            Dim xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", Nothing)
            xmlDoc.AppendChild(xmlDeclaration)

            ' Create root element
            Dim rootElement = xmlDoc.CreateElement("TennisData")
            xmlDoc.AppendChild(rootElement)

            ' Create Players section
            Dim playersElement = xmlDoc.CreateElement("Players")
            rootElement.AppendChild(playersElement)

            ' Add all players from DataGridView
            For Each row As DataGridViewRow In DataGridView_Players.Rows
                If Not row.IsNewRow Then
                    Dim playerElement = xmlDoc.CreateElement("Player")
                    playersElement.AppendChild(playerElement)

                    AddXmlElement(xmlDoc, playerElement, "Name", row.Cells("Name").Value?.ToString())
                    AddXmlElement(xmlDoc, playerElement, "FirstName", row.Cells("FirstName").Value?.ToString())
                    AddXmlElement(xmlDoc, playerElement, "Country", row.Cells("Country").Value?.ToString())
                    AddXmlElement(xmlDoc, playerElement, "CountryISO3", row.Cells("CountryISO3").Value?.ToString())
                    AddXmlElement(xmlDoc, playerElement, "Age", row.Cells("Age").Value?.ToString())
                    AddXmlElement(xmlDoc, playerElement, "Height", row.Cells("Height").Value?.ToString())
                    AddXmlElement(xmlDoc, playerElement, "Data1", row.Cells("Data1").Value?.ToString())
                    AddXmlElement(xmlDoc, playerElement, "Data2", row.Cells("Data2").Value?.ToString())
                    AddXmlElement(xmlDoc, playerElement, "Data3", row.Cells("Data3").Value?.ToString())
                End If
            Next

            ' Create SelectedPlayers section
            Dim selectedPlayersElement = xmlDoc.CreateElement("SelectedPlayers")
            rootElement.AppendChild(selectedPlayersElement)

            ' Add Home Player
            Dim homePlayerElement = xmlDoc.CreateElement("HomePlayer")
            selectedPlayersElement.AppendChild(homePlayerElement)
            For i = 0 To 8
                AddXmlElement(xmlDoc, homePlayerElement, FIELD_NAMES(i), HomePlayer(i))
            Next

            ' Add Away Player
            Dim awayPlayerElement = xmlDoc.CreateElement("AwayPlayer")
            selectedPlayersElement.AppendChild(awayPlayerElement)
            For i = 0 To 8
                AddXmlElement(xmlDoc, awayPlayerElement, FIELD_NAMES(i), AwayPlayer(i))
            Next

            ' Save the XML file with indentation for readability
            Using writer As New System.Xml.XmlTextWriter(XML_FILE_PATH, System.Text.Encoding.UTF8)
                writer.Formatting = System.Xml.Formatting.Indented
                xmlDoc.Save(writer)
            End Using

        Catch ex As Exception
            MessageBox.Show("Error saving data to XML: " & ex.Message, "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub AddXmlElement(xmlDoc As System.Xml.XmlDocument, parentElement As System.Xml.XmlElement, elementName As String, elementValue As String)
        Dim element = xmlDoc.CreateElement(elementName)
        element.InnerText = If(elementValue, "")
        parentElement.AppendChild(element)
    End Sub

    Private Sub Btn_new_Click(sender As Object, e As EventArgs) Handles btn_new.Click
        ' Add new empty row 
        DataGridView_Players.Rows.Add("", "", "", "", 0, 0, "", "", "")
        DataGridView_Players.CurrentCell = DataGridView_Players.Rows(DataGridView_Players.Rows.Count - 1).Cells(0)
    End Sub

    Private Sub Btn_save_Click(sender As Object, e As EventArgs) Handles btn_save.Click
        btn_clear_players.PerformClick()

        Try
            ' Validate current row data
            If DataGridView_Players.CurrentRow IsNot Nothing Then
                Dim currentRow = DataGridView_Players.CurrentRow

                ' Basic validation
                If String.IsNullOrEmpty(currentRow.Cells("Name").Value?.ToString()) Or
                   String.IsNullOrEmpty(currentRow.Cells("FirstName").Value?.ToString()) Then
                    MessageBox.Show("Name and First Name are required fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    Return
                End If

                ' Save all data to XML
                SaveDataToXML()
                MessageBox.Show("Player data saved successfully. Please re-enter the player combination.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        Catch ex As Exception
            MessageBox.Show("Error saving data: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub Btn_update_Click(sender As Object, e As EventArgs) Handles btn_update.Click
        btn_clear_players.PerformClick()

        Try
            If DataGridView_Players.CurrentRow IsNot Nothing Then
                ' Validate current row data
                Dim currentRow = DataGridView_Players.CurrentRow

                If String.IsNullOrEmpty(currentRow.Cells("Name").Value?.ToString()) Or
                   String.IsNullOrEmpty(currentRow.Cells("FirstName").Value?.ToString()) Then
                    MessageBox.Show("Name and First Name are required fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    Return
                End If

                ' Save all data to XML
                SaveDataToXML()
                MessageBox.Show("Player data updated successfully. Please re-enter the player combination", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Else
                MessageBox.Show("Please select a row to update.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        Catch ex As Exception
            MessageBox.Show("Error updating data: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub Btn_delete_Click(sender As Object, e As EventArgs) Handles btn_delete.Click
        Try
            If DataGridView_Players.CurrentRow IsNot Nothing And DataGridView_Players.Rows.Count > 0 Then
                Dim result = MessageBox.Show("Are you sure you want to delete this player?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question)

                If result = DialogResult.Yes Then
                    DataGridView_Players.Rows.RemoveAt(DataGridView_Players.CurrentRow.Index)
                    ' Save changes to XML
                    SaveDataToXML()
                    MessageBox.Show("Player deleted successfully and XML updated!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
            Else
                MessageBox.Show("Please select a row to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        Catch ex As Exception
            MessageBox.Show("Error deleting data: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub DataGridView_Players_MouseMove(sender As Object, e As MouseEventArgs) Handles DataGridView_Players.MouseMove
        If (e.Button And MouseButtons.Left) = MouseButtons.Left AndAlso _dragRowIndex >= 0 Then
            Dim dragSize = SystemInformation.DragSize
            Dim dragRect As New Rectangle(_dragStartPoint.X - dragSize.Width \ 2, _dragStartPoint.Y - dragSize.Height \ 2, dragSize.Width, dragSize.Height)
            If Not dragRect.Contains(e.Location) Then
                Dim row = DataGridView_Players.Rows(_dragRowIndex)
                Dim playerData = String.Join("|", row.Cells.Cast(Of DataGridViewCell)().Select(Function(c) If(c.Value?.ToString(), "")))
                DataGridView_Players.DoDragDrop(playerData, DragDropEffects.Copy)
                _dragRowIndex = -1
            End If
        End If
    End Sub

    Private Sub DataGridView_Players_MouseUp(sender As Object, e As MouseEventArgs) Handles DataGridView_Players.MouseUp
        _dragRowIndex = -1
    End Sub

    ' Drag and Drop Events for DataGridView
    Private Sub DataGridView_Players_MouseDown(sender As Object, e As MouseEventArgs) Handles DataGridView_Players.MouseDown
        If e.Button = MouseButtons.Left Then
            Dim hit = DataGridView_Players.HitTest(e.X, e.Y)
            If hit.Type = DataGridViewHitTestType.Cell AndAlso hit.RowIndex >= 0 Then
                _dragStartPoint = e.Location
                _dragRowIndex = hit.RowIndex
                ' optional: aktuelle Zelle setzen
                DataGridView_Players.CurrentCell = DataGridView_Players.Rows(hit.RowIndex).Cells(hit.ColumnIndex)
            Else
                ' Kein Drag über Header/Leisten/etc.
                _dragRowIndex = -1
            End If
        End If
    End Sub

    ' Drag and Drop Events for Home Player TextBox
    Private Sub Txt_home_player_DragEnter(sender As Object, e As DragEventArgs) Handles txt_home_player.DragEnter
        If e.Data.GetDataPresent(DataFormats.Text) Then
            e.Effect = DragDropEffects.Copy
        End If
    End Sub

    Private Sub Txt_home_player_DragDrop(sender As Object, e As DragEventArgs) Handles txt_home_player.DragDrop
        Try
            Dim playerData = e.Data.GetData(DataFormats.Text).ToString()
            Dim playerFields = playerData.Split("|"c)

            If playerFields.Length >= 9 Then
                ' Store in HomePlayer array
                For i = 0 To 8
                    HomePlayer(i) = playerFields(i)
                Next

                ' Display in textbox
                txt_home_player.Text = $"{playerFields(1)} {playerFields(0)} ({playerFields(3)})"
                txt_home_player.BackColor = Color.LightGreen

                ' Save player selection
                SavePlayerSelection()
            End If
        Catch ex As Exception
            MessageBox.Show("Error setting home player: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Drag and Drop Events for Away Player TextBox
    Private Sub Txt_away_player_DragEnter(sender As Object, e As DragEventArgs) Handles txt_away_player.DragEnter
        If e.Data.GetDataPresent(DataFormats.Text) Then
            e.Effect = DragDropEffects.Copy
        End If
    End Sub

    Private Sub Txt_away_player_DragDrop(sender As Object, e As DragEventArgs) Handles txt_away_player.DragDrop
        Try
            Dim playerData = e.Data.GetData(DataFormats.Text).ToString()
            Dim playerFields = playerData.Split("|"c)

            If playerFields.Length >= 9 Then
                ' Store in AwayPlayer array
                For i = 0 To 8
                    AwayPlayer(i) = playerFields(i)
                Next

                ' Display in textbox
                txt_away_player.Text = $"{playerFields(1)} {playerFields(0)} ({playerFields(3)})"
                txt_away_player.BackColor = Color.LightBlue

                ' Save player selection
                SavePlayerSelection()
            End If
        Catch ex As Exception
            MessageBox.Show("Error setting away player: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub SavePlayerSelection()
        Try
            ' Save to XML automatically when players are selected
            SaveDataToXML()

            ' Show confirmation if both players are selected
            If Not String.IsNullOrEmpty(HomePlayer(0)) AndAlso Not String.IsNullOrEmpty(AwayPlayer(0)) Then
                MessageBox.Show($"Match players set and saved to XML:{vbNewLine}HOME: {HomePlayer(1)} {HomePlayer(0)}{vbNewLine}AWAY: {AwayPlayer(1)} {AwayPlayer(0)}",
                               "Players Selected", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        Catch ex As Exception
            MessageBox.Show("Error saving player selection: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub Btn_clear_players_Click(sender As Object, e As EventArgs) Handles btn_clear_players.Click
        ' Clear player selections
        Array.Clear(HomePlayer, 0, HomePlayer.Length)
        Array.Clear(AwayPlayer, 0, AwayPlayer.Length)

        ' Update display
        UpdatePlayerDisplay()

        ' Save cleared selections to XML
        SaveDataToXML()
        MessageBox.Show("Player selections cleared and XML updated!", "Cleared", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub


    Private Function CheckCurrentMatchPairing() As Boolean
        ' Prüft, ob tatsächlich Spieler aus der DataGrid ausgewählt sind
        Dim homePlayerFilled As Boolean = Not String.IsNullOrEmpty(HomePlayer(0)) AndAlso Not String.IsNullOrEmpty(HomePlayer(1))
        Dim awayPlayerFilled As Boolean = Not String.IsNullOrEmpty(AwayPlayer(0)) AndAlso Not String.IsNullOrEmpty(AwayPlayer(1))

        ' Detaillierte Prüfung welcher Spieler fehlt
        If Not homePlayerFilled And Not awayPlayerFilled Then
            MessageBox.Show("Bitte Spielerpaarung eintragen!" & vbNewLine & vbNewLine &
                       "Es fehlen beide Spieler (HOME und AWAY)." & vbNewLine &
                       "Ziehen Sie Spieler aus der Tabelle in die entsprechenden Felder.",
                       "Fehlende Spielerpaarung",
                       MessageBoxButtons.OK,
                       MessageBoxIcon.Warning)
            Return False
        ElseIf Not homePlayerFilled Then
            MessageBox.Show("Bitte HOME Spieler eintragen!" & vbNewLine & vbNewLine &
                       "Ziehen Sie einen Spieler aus der Tabelle in das HOME Feld.",
                       "Fehlender HOME Spieler",
                       MessageBoxButtons.OK,
                       MessageBoxIcon.Warning)
            Return False
        ElseIf Not awayPlayerFilled Then
            MessageBox.Show("Bitte AWAY Spieler eintragen!" & vbNewLine & vbNewLine &
                       "Ziehen Sie einen Spieler aus der Tabelle in das AWAY Feld.",
                       "Fehlender AWAY Spieler",
                       MessageBoxButtons.OK,
                       MessageBoxIcon.Warning)
            Return False
        End If

        Return True ' Beide Spieler sind korrekt ausgewählt
    End Function

    Private Sub Btn_exit_Click(sender As Object, e As EventArgs) Handles btn_exit.Click
        Try
            If MessageBox.Show("Do you really want to end the program?", "Tennis24", MessageBoxButtons.YesNo) = DialogResult.Yes Then
                Application.Exit()
            End If
        Catch ex As Exception
            MessageBox.Show($"Error exiting application: {ex.Message}", "Error10", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub Btn_loadSettings_Click(sender As Object, e As EventArgs) Handles Btn_loadSettings.Click
        Tennis24_Settings.Show()
        Me.Hide()
        UpdateBestOfLabel()
    End Sub

    Private Sub Tennis24_Main_Activated(sender As Object, e As EventArgs) Handles Me.Activated
        UpdateBestOfLabel()
    End Sub

    Private Sub Btn_Load_File_Click(sender As Object, e As EventArgs) Handles Btn_Load_File.Click
        Try
            ' Bestätigungsdialog
            Dim confirmResult = MessageBox.Show(
                "Are you sure you want to load a different data file?" & vbNewLine & vbNewLine &
                "This may affect the current data.",
                "Load Data Filen",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question)

            If confirmResult <> DialogResult.Yes Then
                Return
            End If

            ' OpenFileDialog für XML-Datei
            Using openFileDialog As New OpenFileDialog()
                openFileDialog.Title = "choose XML-Datenfile"
                openFileDialog.Filter = "XML-File (*.xml)|*.xml|all files (*.*)|*.*"
                openFileDialog.FilterIndex = 1
                openFileDialog.InitialDirectory = "c:\vmix\tennis\data\"

                If openFileDialog.ShowDialog() = DialogResult.OK Then
                    Dim selectedFile = openFileDialog.FileName

                    ' Dateistruktur prüfen
                    If ValidateXmlStructure(selectedFile) Then
                        ' Frage nach Anhängen oder Ersetzen
                        Dim actionResult = MessageBox.Show(
                            "How should the data from the selected file be handled?" & vbNewLine & vbNewLine &
                            "JA/YES = Append to existing data" & vbNewLine &
                            "NEIN/NO = Replace existing data" & vbNewLine &
                            "ABBRECHEN/Cancel = Cancel operation",
                            "Load Data",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question)

                        Select Case actionResult
                            Case DialogResult.Yes
                                LoadDataFromExternalXML(selectedFile, True) ' Anhängen
                            Case DialogResult.No
                                LoadDataFromExternalXML(selectedFile, False) ' Ersetzen
                            Case DialogResult.Cancel
                                Return ' Abbrechen
                        End Select
                    Else
                        MessageBox.Show(
                            "The selected XML file does not have the expected structure." & vbNewLine &
                            "Please select a valid tennis data file.",
                            "Invalid file structure",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error)
                    End If
                End If
            End Using

        Catch ex As Exception
            MessageBox.Show($"Fehler beim Laden der Datei: {ex.Message}", "Ladefehler", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function ValidateXmlStructure(filePath As String) As Boolean
        Try
            If Not System.IO.File.Exists(filePath) Then
                Return False
            End If

            Dim xmlDoc As New System.Xml.XmlDocument()
            xmlDoc.Load(filePath)

            ' Prüfe Grundstruktur
            Dim rootNode = xmlDoc.SelectSingleNode("//TennisData")
            If rootNode Is Nothing Then
                Return False
            End If

            ' Prüfe Players-Sektion
            Dim playersNode = xmlDoc.SelectSingleNode("//TennisData/Players")
            If playersNode Is Nothing Then
                Return False
            End If

            ' Prüfe mindestens einen Player mit den erforderlichen Feldern
            Dim playerNodes = playersNode.SelectNodes("Player")
            If playerNodes IsNot Nothing AndAlso playerNodes.Count > 0 Then
                Dim firstPlayer = playerNodes(0)
                ' Prüfe ob die wichtigsten Felder vorhanden sind
                If firstPlayer.SelectSingleNode("Name") Is Nothing OrElse
                   firstPlayer.SelectSingleNode("FirstName") Is Nothing Then
                    Return False
                End If
            End If

            Return True

        Catch ex As Exception
            Return False
        End Try
    End Function

    Private Sub LoadDataFromExternalXML(filePath As String, appendData As Boolean)
        Try
            Dim xmlDoc As New System.Xml.XmlDocument()
            xmlDoc.Load(filePath)

            ' Wenn nicht anhängen, dann bestehende Daten löschen
            If Not appendData Then
                DataGridView_Players.Rows.Clear()
                ' Spielerauswahl auch zurücksetzen
                Array.Clear(HomePlayer, 0, HomePlayer.Length)
                Array.Clear(AwayPlayer, 0, AwayPlayer.Length)
                UpdatePlayerDisplay()
            End If

            ' Spielerdaten laden
            Dim playersNode = xmlDoc.SelectSingleNode("//TennisData/Players")
            If playersNode IsNot Nothing Then
                Dim loadedPlayersCount = 0

                For Each playerNode As System.Xml.XmlNode In playersNode.SelectNodes("Player")
                    Dim name = GetXmlNodeValue(playerNode, "Name")
                    Dim firstName = GetXmlNodeValue(playerNode, "FirstName")

                    ' Prüfe bei Anhängen, ob Spieler bereits existiert (Name + Vorname)
                    If appendData Then
                        Dim playerExists = False
                        For Each row As DataGridViewRow In DataGridView_Players.Rows
                            If Not row.IsNewRow AndAlso
                               row.Cells("Name").Value?.ToString() = name AndAlso
                               row.Cells("FirstName").Value?.ToString() = firstName Then
                                playerExists = True
                                Exit For
                            End If
                        Next

                        ' Überspringe wenn Spieler bereits existiert
                        If playerExists Then
                            Continue For
                        End If
                    End If

                    ' Lade alle Spielerdaten
                    Dim country = GetXmlNodeValue(playerNode, "Country")
                    Dim countryISO3 = GetXmlNodeValue(playerNode, "CountryISO3")
                    Dim age = GetXmlNodeValue(playerNode, "Age")
                    Dim height = GetXmlNodeValue(playerNode, "Height")
                    Dim data1 = GetXmlNodeValue(playerNode, "Data1")
                    Dim data2 = GetXmlNodeValue(playerNode, "Data2")
                    Dim data3 = GetXmlNodeValue(playerNode, "Data3")

                    DataGridView_Players.Rows.Add(name, firstName, country, countryISO3, age, height, data1, data2, data3)
                    loadedPlayersCount += 1
                Next

                ' Wenn beim Ersetzen auch SelectedPlayers vorhanden sind, diese laden
                If Not appendData Then
                    LoadSelectedPlayersFromXML(xmlDoc)
                End If

                ' Erfolg melden
                Dim actionText = If(appendData, "attached", "replaced")
                MessageBox.Show(
                    $"Data successfully {actionText}!" & vbNewLine & vbNewLine &
                    $"Number of players loaded: {loadedPlayersCount}" & vbNewLine &
                    "ATTENTION: The data is NOT automatically saved in the main file.",
                    "Loading successfully",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information)

                ' Automatisch in Hauptdatei speichern
                'SaveDataToXML()
            End If

        Catch ex As Exception
            MessageBox.Show($"Fehler beim Laden der externen XML-Datei: {ex.Message}", "Ladefehler", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LoadSelectedPlayersFromXML(xmlDoc As System.Xml.XmlDocument)
        Try
            ' Lade ausgewählte Home Player
            Dim homePlayerNode = xmlDoc.SelectSingleNode("//TennisData/SelectedPlayers/HomePlayer")
            If homePlayerNode IsNot Nothing AndAlso homePlayerNode.HasChildNodes Then
                HomePlayer(0) = GetXmlNodeValue(homePlayerNode, "Name")
                HomePlayer(1) = GetXmlNodeValue(homePlayerNode, "FirstName")
                HomePlayer(2) = GetXmlNodeValue(homePlayerNode, "Country")
                HomePlayer(3) = GetXmlNodeValue(homePlayerNode, "CountryISO3")
                HomePlayer(4) = GetXmlNodeValue(homePlayerNode, "Age")
                HomePlayer(5) = GetXmlNodeValue(homePlayerNode, "Height")
                HomePlayer(6) = GetXmlNodeValue(homePlayerNode, "Data1")
                HomePlayer(7) = GetXmlNodeValue(homePlayerNode, "Data2")
                HomePlayer(8) = GetXmlNodeValue(homePlayerNode, "Data3")
            End If

            ' Lade ausgewählte Away Player
            Dim awayPlayerNode = xmlDoc.SelectSingleNode("//TennisData/SelectedPlayers/AwayPlayer")
            If awayPlayerNode IsNot Nothing AndAlso awayPlayerNode.HasChildNodes Then
                AwayPlayer(0) = GetXmlNodeValue(awayPlayerNode, "Name")
                AwayPlayer(1) = GetXmlNodeValue(awayPlayerNode, "FirstName")
                AwayPlayer(2) = GetXmlNodeValue(awayPlayerNode, "Country")
                AwayPlayer(3) = GetXmlNodeValue(awayPlayerNode, "CountryISO3")
                AwayPlayer(4) = GetXmlNodeValue(awayPlayerNode, "Age")
                AwayPlayer(5) = GetXmlNodeValue(awayPlayerNode, "Height")
                AwayPlayer(6) = GetXmlNodeValue(awayPlayerNode, "Data1")
                AwayPlayer(7) = GetXmlNodeValue(awayPlayerNode, "Data2")
                AwayPlayer(8) = GetXmlNodeValue(awayPlayerNode, "Data3")
            End If

            ' Display aktualisieren
            UpdatePlayerDisplay()

        Catch ex As Exception
            ' Fehler beim Laden der Spielerauswahl sind nicht kritisch
        End Try
    End Sub

    Private Sub Btn_saveAs_Click(sender As Object, e As EventArgs) Handles Btn_SaveAs.Click
        Try
            ' SaveFileDialog für XML-Datei
            Using saveFileDialog As New SaveFileDialog()
                saveFileDialog.Title = "Save player database as..."
                saveFileDialog.Filter = "XML-File (*.xml)|*.xml|Alle Dateien (*.*)|*.*"
                saveFileDialog.FilterIndex = 1
                saveFileDialog.InitialDirectory = "c:\vmix\tennis\data\"
                saveFileDialog.DefaultExt = "xml"

                ' Vorschlag für Dateiname basierend auf aktuellem Datum
                Dim suggestedName = $"tennisdata_{DateTime.Now:yyyyMMdd_HHmmss}.xml"
                saveFileDialog.FileName = suggestedName

                If saveFileDialog.ShowDialog() = DialogResult.OK Then
                    Dim selectedFilePath = saveFileDialog.FileName

                    ' Bestätigung anzeigen
                    Dim confirmResult = MessageBox.Show(
                        $"Player database will be saved as:{vbNewLine}{vbNewLine}" &
                        $"{selectedFilePath}{vbNewLine}{vbNewLine}" &
                        "Would you like to continue?",
                        "Confirm save",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question)

                    If confirmResult = DialogResult.Yes Then
                        ' Speichere in die ausgewählte Datei
                        SaveDataToCustomXML(selectedFilePath)

                        MessageBox.Show(
                            $"Player Database successfully saved!{vbNewLine}{vbNewLine}" &
                            $"File: {selectedFilePath}{vbNewLine}" &
                            $"Number of Players: {DataGridView_Players.Rows.Count - 1}",
                            "Successfully saved",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information)
                    End If
                End If
            End Using

        Catch ex As Exception
            MessageBox.Show($"Error saving the file: {ex.Message}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub SaveDataToCustomXML(filePath As String)
        Try
            ' Verzeichnis erstellen falls es nicht existiert
            Dim xmlDirectory = System.IO.Path.GetDirectoryName(filePath)
            If Not System.IO.Directory.Exists(xmlDirectory) Then
                System.IO.Directory.CreateDirectory(xmlDirectory)
            End If

            Dim xmlDoc As New System.Xml.XmlDocument()

            ' XML-Deklaration erstellen
            Dim xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", Nothing)
            xmlDoc.AppendChild(xmlDeclaration)

            ' Root-Element erstellen
            Dim rootElement = xmlDoc.CreateElement("TennisData")
            xmlDoc.AppendChild(rootElement)

            ' Metadaten hinzufügen
            Dim metaElement = xmlDoc.CreateElement("Metadata")
            rootElement.AppendChild(metaElement)
            AddXmlElement(xmlDoc, metaElement, "CreatedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
            AddXmlElement(xmlDoc, metaElement, "CreatedBy", "Tennis24 Application")
            AddXmlElement(xmlDoc, metaElement, "Version", My.Application.Info.Version.ToString())

            ' Players-Sektion erstellen
            Dim playersElement = xmlDoc.CreateElement("Players")
            rootElement.AppendChild(playersElement)

            ' Alle Spieler aus DataGridView hinzufügen
            For Each row As DataGridViewRow In DataGridView_Players.Rows
                If Not row.IsNewRow Then
                    Dim playerElement = xmlDoc.CreateElement("Player")
                    playersElement.AppendChild(playerElement)

                    AddXmlElement(xmlDoc, playerElement, "Name", row.Cells("Name").Value?.ToString())
                    AddXmlElement(xmlDoc, playerElement, "FirstName", row.Cells("FirstName").Value?.ToString())
                    AddXmlElement(xmlDoc, playerElement, "Country", row.Cells("Country").Value?.ToString())
                    AddXmlElement(xmlDoc, playerElement, "CountryISO3", row.Cells("CountryISO3").Value?.ToString())
                    AddXmlElement(xmlDoc, playerElement, "Age", row.Cells("Age").Value?.ToString())
                    AddXmlElement(xmlDoc, playerElement, "Height", row.Cells("Height").Value?.ToString())
                    AddXmlElement(xmlDoc, playerElement, "Data1", row.Cells("Data1").Value?.ToString())
                    AddXmlElement(xmlDoc, playerElement, "Data2", row.Cells("Data2").Value?.ToString())
                    AddXmlElement(xmlDoc, playerElement, "Data3", row.Cells("Data3").Value?.ToString())
                End If
            Next

            ' SelectedPlayers-Sektion erstellen
            Dim selectedPlayersElement = xmlDoc.CreateElement("SelectedPlayers")
            rootElement.AppendChild(selectedPlayersElement)

            ' Home Player hinzufügen
            Dim homePlayerElement = xmlDoc.CreateElement("HomePlayer")
            selectedPlayersElement.AppendChild(homePlayerElement)
            For i = 0 To 8
                AddXmlElement(xmlDoc, homePlayerElement, FIELD_NAMES(i), HomePlayer(i))
            Next

            ' Away Player hinzufügen
            Dim awayPlayerElement = xmlDoc.CreateElement("AwayPlayer")
            selectedPlayersElement.AppendChild(awayPlayerElement)
            For i = 0 To 8
                AddXmlElement(xmlDoc, awayPlayerElement, FIELD_NAMES(i), AwayPlayer(i))
            Next

            ' XML-Datei mit Formatierung speichern
            Using writer As New System.Xml.XmlTextWriter(filePath, System.Text.Encoding.UTF8)
                writer.Formatting = System.Xml.Formatting.Indented
                xmlDoc.Save(writer)
            End Using

        Catch ex As Exception
            Throw New Exception($"Fehler beim Speichern in {filePath}: {ex.Message}")
        End Try
    End Sub

    Private Sub CreateAutoBackup()
        Try
            ' backup verzeichnis vorhanden?
            If System.IO.File.Exists(XML_FILE_PATH) Then
                ' Backup-Verzeichnis erstellen
                Dim backupDirectory As String = "c:\vmix\tennis\data\backup"
                If Not System.IO.Directory.Exists(backupDirectory) Then
                    System.IO.Directory.CreateDirectory(backupDirectory)
                End If

                ' Backup-Dateiname mit Zeitstempel erstellen
                Dim backupFileName As String = $"tennisdata_backup_{DateTime.Now:yyyyMMdd_HHmmss}.xml"
                Dim backupFilePath As String = System.IO.Path.Combine(backupDirectory, backupFileName)

                ' Datei kopieren
                System.IO.File.Copy(XML_FILE_PATH, backupFilePath, True)

                ' alte Backups löschen (älter als 30 Tage)
                CleanupOldBackups(backupDirectory, 30)
            End If

        Catch ex As Exception
            ' Nur bei Fehlern Meldung anzeigen
            MessageBox.Show($"Error creating backup: {ex.Message}", "Backup-Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub CleanupOldBackups(backupDirectory As String, daysToKeep As Integer)
        Try
            If System.IO.Directory.Exists(backupDirectory) Then
                Dim files = System.IO.Directory.GetFiles(backupDirectory, "tennisdata_backup_*.xml")
                Dim cutoffDate = DateTime.Now.AddDays(-daysToKeep)

                For Each file In files
                    Dim fileInfo As New System.IO.FileInfo(file)
                    If fileInfo.CreationTime < cutoffDate Then
                        System.IO.File.Delete(file)
                    End If
                Next
            End If
        Catch ex As Exception
            ' hier unnötig
        End Try
    End Sub
End Class
