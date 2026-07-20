Public Class Tennis26_Main
    ' Array to store selected players for the match
    Public Shared HomePlayer As String() = New String(8) {} ' Name, FirstName, Country, ISO3, Age, Height, Data1, Data2, Data3
    Public Shared AwayPlayer As String() = New String(8) {} ' Name, FirstName, Country, ISO3, Age, Height, Data1, Data2, Data3

    ' Doppel-Partner (nur relevant, wenn CheckBox1 = Doppel aktiv) - gleiche Feldstruktur wie
    ' HomePlayer/AwayPlayer. Absichtlich separate Arrays statt eines 2D-Arrays: HomePlayer/
    ' AwayPlayer bleiben dadurch unverändert, jeder bisherige Code (und jede vMix-Grafik für
    ' Einzel) funktioniert unverändert weiter.
    Public Shared HomePlayer2 As String() = New String(8) {}
    Public Shared AwayPlayer2 As String() = New String(8) {}

    ' XML file path
    Private Const XML_FILE_PATH As String = "c:\vmix\tennis\data\tennisdata.xml"

    ' Von Tennis26_Main2 ("Save pairings") geschriebene Datei mit den 4 vorbereiteten
    ' Paarungen - hier nur lesend verwendet, für die 4 "Select Pairing"-Schnellwahl-Buttons.
    Private Const PAIRINGS_FILE_PATH As String = "c:\vmix\tennis\data\pairings.xml"

    ' Feldnamen als konstante Array
    Private Shared ReadOnly FIELD_NAMES As String() = {"Name", "FirstName", "Country", "CountryISO3", "Age", "Height", "Data1", "Data2", "Data3"}

    Private _dragStartPoint As Point
    Private _dragRowIndex As Integer = -1

    Private Sub Tennis26_Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            ' macht kopie der spielerdaten
            CreateAutoBackup()

            InitializeDataGrid()
            InitializePlayerFields()

            LoadDataFromXML() 'reads player data and selected players from XML for main form

            Tennis26_Settings.LoadSettingsFromXml() 'reads settings from XLM
            Tennis26_Settings.SetLabels() ' sets labels in settings form

            ' Sichere Anzeige mit Fallback
            UpdateBestOfLabel()
            LoadPairingButtonCaptions()

            Me.Text = My.Application.Info.AssemblyName + " " + My.Application.Info.Version.ToString() + " | " + My.Application.Info.Copyright.ToString()
        Catch ex As Exception
            MessageBox.Show($"Fehler beim Initialisieren der Anwendung: {ex.Message}", "Initialisierungsfehler", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub UpdateBestOfLabel()
        Dim bestOfValue = If(String.IsNullOrEmpty(Tennis26_Settings.TextBoxValues(50)), "3", Tennis26_Settings.TextBoxValues(50))
        Label2.Text = "best of: " + bestOfValue
    End Sub


    ' Öffnet den Paarungs-Vorbereitungs-Prototyp (siehe Tennis26_Main2.vb) - nicht-modal,
    ' damit gleichzeitig in Main weiter mit der Spielerdatenbank gearbeitet werden kann.
    Private Sub Btn_open_pairings_Click(sender As Object, e As EventArgs) Handles Btn_open_pairings.Click
        Tennis26_Main2.Show()
    End Sub

    Private Sub Btn_live_Click(sender As Object, e As EventArgs) Handles btn_live.Click
        Try
            ' Prüfe zuerst die Spielerpaarung
            If Not CheckCurrentMatchPairing() Then
                Return ' Beende die Funktion hier wenn Prüfung fehlschlägt
            End If

            Tennis26_Settings.SetVariables()
            Tennis26_Scorer.Show()

            ' Sichere String-Konkatenation mit Fallbacks
            Dim bestOf = If(String.IsNullOrEmpty(Tennis26_Settings.TextBoxValues(50)), "3", Tennis26_Settings.TextBoxValues(50))
            Dim vmixIP = If(String.IsNullOrEmpty(Tennis26_Settings.TextBoxValues(45)), "localhost", Tennis26_Settings.TextBoxValues(45))

            Tennis26_Scorer.Text = $"LIVE | Best of {bestOf} | IP: {vmixIP}"
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
        ' Data1 (Ranking) / Data2 (Points) bewusst nicht als Integer typisiert - erlaubt
        ' Freitext wie "ATP Points: 22" oder eine Ranking-Bezeichnung statt reiner Zahl.

        ' Optionale Ausrichtung für Zahlen
        DataGridView_Players.Columns("Age").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
        DataGridView_Players.Columns("Height").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight

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

    ' Sorgt für korrekte Sortierung (numerisch für Age/Height, sonst case-insensitive Text)
    ' Data3 mit Sekundärfeld Name immer aufsteigend, sonst case-insensitive Text)
    Private Sub DataGridView_Players_SortCompare(sender As Object, e As DataGridViewSortCompareEventArgs) Handles DataGridView_Players.SortCompare
        Select Case e.Column.Name
            Case "Age", "Height"
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
            ' Me.Text = $"Tennis26 - Aktuelle Zelle: {col} (Zeile {row})"

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

    ' Fängt fehlgeschlagene Zellwert-Konvertierungen ab (z.B. Text statt Zahl in Age/Height,
    ' die als Integer typisiert sind). Ohne diesen Handler zeigt die
    ' DataGridView selbst den kryptischen Standarddialog "kann kein Commit durchführen
    ' oder die Änderung nicht abbrechen" und die Zelle bleibt in einem inkonsistenten
    ' Bearbeitungszustand hängen.
    Private Sub DataGridView_Players_DataError(sender As Object, e As DataGridViewDataErrorEventArgs) Handles DataGridView_Players.DataError
        e.ThrowException = False
        If e.Context = DataGridViewDataErrorContexts.Commit Then
            Dim columnName = DataGridView_Players.Columns(e.ColumnIndex).HeaderText
            MessageBox.Show($"'{columnName}' benötigt eine ganze Zahl.", "Ungültiger Wert", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            DataGridView_Players.CancelEdit()
        End If
    End Sub

    ' Die Paarungsfelder sind reine Anzeige der aktuell aktiven Paarung - Auswahl per
    ' Drag&Drop läuft nur noch über Tennis26_Main2 (Btn_open_pairings), siehe dort. Deshalb
    ' hier bewusst kein AllowDrop mehr; die zugehörigen Drag&Drop-Handler wurden entfernt.
    Private Sub InitializePlayerFields()
        ' Make them read-only so users can't type directly
        txt_home_player.ReadOnly = True
        txt_away_player.ReadOnly = True
        txt_home_player2.ReadOnly = True
        txt_away_player2.ReadOnly = True

        ' CheckBox1 (Doubles) wird nur noch programmatisch gesetzt (aus Main2 oder den
        ' "Select Pairing"-Buttons) - der Operator muss sie hier nicht mehr sehen oder
        ' anklicken können. Bleibt als Control bestehen (Scorer.IsDoublesMatch() liest sie
        ' weiterhin), nur nicht mehr sichtbar/bedienbar.
        CheckBox1.Visible = False
        CheckBox1.Enabled = False
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

                ' Load selected home player 2 (Doppel-Partner)
                Dim homePlayer2Node = xmlDoc.SelectSingleNode("//TennisData/SelectedPlayers/HomePlayer2")
                If homePlayer2Node IsNot Nothing AndAlso homePlayer2Node.HasChildNodes Then
                    HomePlayer2(0) = GetXmlNodeValue(homePlayer2Node, "Name")
                    HomePlayer2(1) = GetXmlNodeValue(homePlayer2Node, "FirstName")
                    HomePlayer2(2) = GetXmlNodeValue(homePlayer2Node, "Country")
                    HomePlayer2(3) = GetXmlNodeValue(homePlayer2Node, "CountryISO3")
                    HomePlayer2(4) = GetXmlNodeValue(homePlayer2Node, "Age")
                    HomePlayer2(5) = GetXmlNodeValue(homePlayer2Node, "Height")
                    HomePlayer2(6) = GetXmlNodeValue(homePlayer2Node, "Data1")
                    HomePlayer2(7) = GetXmlNodeValue(homePlayer2Node, "Data2")
                    HomePlayer2(8) = GetXmlNodeValue(homePlayer2Node, "Data3")
                End If

                ' Load selected away player 2 (Doppel-Partner)
                Dim awayPlayer2Node = xmlDoc.SelectSingleNode("//TennisData/SelectedPlayers/AwayPlayer2")
                If awayPlayer2Node IsNot Nothing AndAlso awayPlayer2Node.HasChildNodes Then
                    AwayPlayer2(0) = GetXmlNodeValue(awayPlayer2Node, "Name")
                    AwayPlayer2(1) = GetXmlNodeValue(awayPlayer2Node, "FirstName")
                    AwayPlayer2(2) = GetXmlNodeValue(awayPlayer2Node, "Country")
                    AwayPlayer2(3) = GetXmlNodeValue(awayPlayer2Node, "CountryISO3")
                    AwayPlayer2(4) = GetXmlNodeValue(awayPlayer2Node, "Age")
                    AwayPlayer2(5) = GetXmlNodeValue(awayPlayer2Node, "Height")
                    AwayPlayer2(6) = GetXmlNodeValue(awayPlayer2Node, "Data1")
                    AwayPlayer2(7) = GetXmlNodeValue(awayPlayer2Node, "Data2")
                    AwayPlayer2(8) = GetXmlNodeValue(awayPlayer2Node, "Data3")
                End If

                ' Doppel-Umschalter (CheckBox1) laden
                Dim isDoublesNode = xmlDoc.SelectSingleNode("//TennisData/SelectedPlayers/IsDoublesMatch")
                If isDoublesNode IsNot Nothing Then
                    Dim isDoubles As Boolean
                    Boolean.TryParse(isDoublesNode.InnerText, isDoubles)
                    CheckBox1.Checked = isDoubles
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

    ' Für Tennis26_Main2 (Vorbereitungs-Prototyp): erlaubt einer anderen Form, eine dort
    ' vorbereitete Paarung zu übernehmen, ohne UpdatePlayerDisplay/SaveDataToXML einzeln
    ' Public machen zu müssen - beide bleiben intern, nur dieser eine Einstiegspunkt ist es.
    Public Sub RefreshAndSavePlayerSelection()
        UpdatePlayerDisplay()
        SaveDataToXML()
    End Sub

    ' Reine Anzeige der aktuell aktiven Paarung - kein Platzhaltertext mehr (die Auswahl
    ' läuft nur noch über Tennis26_Main2/die 4 "Select Pairing"-Buttons, nicht mehr per
    ' Drag&Drop hier), ein leeres Feld bleibt also einfach leer statt einen Hinweistext zu
    ' zeigen, der ohnehin nicht mehr zutrifft. Home2/Away2 sind komplett unsichtbar, solange
    ' CheckBox1 (Doubles, hier nicht mehr sichtbar) nicht gesetzt ist.
    Private Sub UpdatePlayerDisplay()
        If Not String.IsNullOrEmpty(HomePlayer(0)) AndAlso Not String.IsNullOrEmpty(HomePlayer(1)) Then
            txt_home_player.Text = $"{HomePlayer(1)} {HomePlayer(0)} ({HomePlayer(3)})"
            txt_home_player.BackColor = Color.LightGreen
        Else
            txt_home_player.Text = ""
            txt_home_player.BackColor = SystemColors.Window
        End If

        If Not String.IsNullOrEmpty(AwayPlayer(0)) AndAlso Not String.IsNullOrEmpty(AwayPlayer(1)) Then
            txt_away_player.Text = $"{AwayPlayer(1)} {AwayPlayer(0)} ({AwayPlayer(3)})"
            txt_away_player.BackColor = Color.LightBlue
        Else
            txt_away_player.Text = ""
            txt_away_player.BackColor = SystemColors.Window
        End If

        If Not String.IsNullOrEmpty(HomePlayer2(0)) AndAlso Not String.IsNullOrEmpty(HomePlayer2(1)) Then
            txt_home_player2.Text = $"{HomePlayer2(1)} {HomePlayer2(0)} ({HomePlayer2(3)})"
            txt_home_player2.BackColor = Color.LightGreen
        Else
            txt_home_player2.Text = ""
            txt_home_player2.BackColor = SystemColors.Window
        End If

        If Not String.IsNullOrEmpty(AwayPlayer2(0)) AndAlso Not String.IsNullOrEmpty(AwayPlayer2(1)) Then
            txt_away_player2.Text = $"{AwayPlayer2(1)} {AwayPlayer2(0)} ({AwayPlayer2(3)})"
            txt_away_player2.BackColor = Color.LightBlue
        Else
            txt_away_player2.Text = ""
            txt_away_player2.BackColor = SystemColors.Window
        End If

        txt_home_player2.Visible = CheckBox1.Checked
        txt_away_player2.Visible = CheckBox1.Checked
    End Sub

    Private Sub SetDefaultPlayerDisplay()
        ' Set default text when no XML file exists
        txt_home_player.Text = ""
        txt_away_player.Text = ""
        txt_home_player.BackColor = SystemColors.Window
        txt_away_player.BackColor = SystemColors.Window
        txt_home_player2.Text = ""
        txt_away_player2.Text = ""
        txt_home_player2.BackColor = SystemColors.Window
        txt_away_player2.BackColor = SystemColors.Window
        txt_home_player2.Visible = False
        txt_away_player2.Visible = False
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

            Dim xmlDoc = BuildPlayerDatabaseXml(includeMetadata:=False)

            ' Save the XML file with indentation for readability
            Using writer As New System.Xml.XmlTextWriter(XML_FILE_PATH, System.Text.Encoding.UTF8)
                writer.Formatting = System.Xml.Formatting.Indented
                xmlDoc.Save(writer)
            End Using

        Catch ex As Exception
            MessageBox.Show("Error saving data to XML: " & ex.Message, "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Baut das gemeinsame XML-Dokument (Spielerliste + ausgewählte Paarung) für SaveDataToXML
    ' und SaveDataToCustomXML. includeMetadata fügt einen zusätzlichen Metadata-Block hinzu
    ' (bisher nur bei "Speichern unter" verwendet).
    Private Function BuildPlayerDatabaseXml(includeMetadata As Boolean) As System.Xml.XmlDocument
        Dim xmlDoc As New System.Xml.XmlDocument()

        Dim xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", Nothing)
        xmlDoc.AppendChild(xmlDeclaration)

        Dim rootElement = xmlDoc.CreateElement("TennisData")
        xmlDoc.AppendChild(rootElement)

        If includeMetadata Then
            Dim metaElement = xmlDoc.CreateElement("Metadata")
            rootElement.AppendChild(metaElement)
            AddXmlElement(xmlDoc, metaElement, "CreatedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
            AddXmlElement(xmlDoc, metaElement, "CreatedBy", "Tennis26 Application")
            AddXmlElement(xmlDoc, metaElement, "Version", My.Application.Info.Version.ToString())
        End If

        ' Players-Sektion
        Dim playersElement = xmlDoc.CreateElement("Players")
        rootElement.AppendChild(playersElement)

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

        ' SelectedPlayers-Sektion
        Dim selectedPlayersElement = xmlDoc.CreateElement("SelectedPlayers")
        rootElement.AppendChild(selectedPlayersElement)

        Dim homePlayerElement = xmlDoc.CreateElement("HomePlayer")
        selectedPlayersElement.AppendChild(homePlayerElement)
        For i = 0 To 8
            AddXmlElement(xmlDoc, homePlayerElement, FIELD_NAMES(i), HomePlayer(i))
        Next

        Dim awayPlayerElement = xmlDoc.CreateElement("AwayPlayer")
        selectedPlayersElement.AppendChild(awayPlayerElement)
        For i = 0 To 8
            AddXmlElement(xmlDoc, awayPlayerElement, FIELD_NAMES(i), AwayPlayer(i))
        Next

        ' Doppel-Partner (nur inhaltlich befüllt, wenn CheckBox1 = Doppel aktiv war - werden
        ' aber immer mitgespeichert, damit eine spätere Umschaltung auf Doppel nicht die
        ' zuletzt gezogenen Partner verliert)
        Dim homePlayer2Element = xmlDoc.CreateElement("HomePlayer2")
        selectedPlayersElement.AppendChild(homePlayer2Element)
        For i = 0 To 8
            AddXmlElement(xmlDoc, homePlayer2Element, FIELD_NAMES(i), HomePlayer2(i))
        Next

        Dim awayPlayer2Element = xmlDoc.CreateElement("AwayPlayer2")
        selectedPlayersElement.AppendChild(awayPlayer2Element)
        For i = 0 To 8
            AddXmlElement(xmlDoc, awayPlayer2Element, FIELD_NAMES(i), AwayPlayer2(i))
        Next

        AddXmlElement(xmlDoc, selectedPlayersElement, "IsDoublesMatch", CheckBox1.Checked.ToString())

        Return xmlDoc
    End Function

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
        ClearCurrentPairingSelection()

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
        ClearCurrentPairingSelection()

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

    ' Ersetzt das frühere btn_clear_players.PerformClick() (Button existiert nicht mehr,
    ' siehe Btn_save_Click/Btn_update_Click) - wird dort ausgelöst, weil eine Änderung an der
    ' Spielerdatenbank die aktuell aktive Paarung ungültig machen könnte (z.B. wenn genau der
    ' ausgewählte Spieler bearbeitet wird). Ohne Rückfrage, da nur ein Nebeneffekt von
    ' Save/Update - keine bewusste "Paarung löschen"-Aktion des Operators.
    Private Sub ClearCurrentPairingSelection()
        Array.Clear(HomePlayer, 0, HomePlayer.Length)
        Array.Clear(AwayPlayer, 0, AwayPlayer.Length)
        Array.Clear(HomePlayer2, 0, HomePlayer2.Length)
        Array.Clear(AwayPlayer2, 0, AwayPlayer2.Length)
        CheckBox1.Checked = False
        UpdatePlayerDisplay()
        SaveDataToXML()
    End Sub

    ' Liest pairings.xml (siehe Tennis26_Main2, "Save pairings") und beschriftet die 4
    ' "Select Pairing"-Buttons mit "Nachname vs Nachname" - oder "Pairing N (empty)", falls
    ' die Datei fehlt oder der jeweilige Slot leer ist.
    Private Sub LoadPairingButtonCaptions()
        Dim captions As String() = {"Pairing 1 (empty)", "Pairing 2 (empty)", "Pairing 3 (empty)", "Pairing 4 (empty)"}
        Try
            If IO.File.Exists(PAIRINGS_FILE_PATH) Then
                Dim xmlDoc As New Xml.XmlDocument()
                xmlDoc.Load(PAIRINGS_FILE_PATH)
                For Each pairingNode As Xml.XmlNode In xmlDoc.SelectNodes("//TennisPairings/Pairing")
                    Dim indexAttr = pairingNode.Attributes("index")
                    If indexAttr Is Nothing Then Continue For
                    Dim pairingIndex As Integer
                    If Not Integer.TryParse(indexAttr.Value, pairingIndex) OrElse pairingIndex < 0 OrElse pairingIndex > 3 Then Continue For

                    Dim homeName = GetXmlNodeValue(pairingNode, "Home/Name")
                    Dim awayName = GetXmlNodeValue(pairingNode, "Away/Name")

                    Dim doublesNode = pairingNode.SelectSingleNode("Doubles")
                    Dim isDoubles As Boolean = False
                    If doublesNode IsNot Nothing Then Boolean.TryParse(doublesNode.InnerText, isDoubles)

                    If Not String.IsNullOrEmpty(homeName) OrElse Not String.IsNullOrEmpty(awayName) Then
                        Dim homeText = If(String.IsNullOrEmpty(homeName), "?", homeName)
                        Dim awayText = If(String.IsNullOrEmpty(awayName), "?", awayName)

                        If isDoubles Then
                            ' Bei Doppel alle 4 Namen zeigen (die Buttons sind extra dafür
                            ' höher gemacht worden) - Partner mit "/" angehängt, falls vorhanden.
                            Dim home2Name = GetXmlNodeValue(pairingNode, "Home2/Name")
                            Dim away2Name = GetXmlNodeValue(pairingNode, "Away2/Name")
                            If Not String.IsNullOrEmpty(home2Name) Then homeText &= $" / {home2Name}"
                            If Not String.IsNullOrEmpty(away2Name) Then awayText &= $" / {away2Name}"
                            captions(pairingIndex) = $"{homeText}{vbCrLf}vs{vbCrLf}{awayText}"
                        Else
                            captions(pairingIndex) = $"{homeText} vs {awayText}"
                        End If
                    End If
                Next
            End If
        Catch ex As Exception
            ' Fehler beim Lesen sind hier nicht kritisch - Buttons zeigen dann "(empty)"
        End Try

        Btn_SelectPairing1.Text = captions(0)
        Btn_SelectPairing2.Text = captions(1)
        Btn_SelectPairing3.Text = captions(2)
        Btn_SelectPairing4.Text = captions(3)
    End Sub

    ' Übernimmt eine in Tennis26_Main2 vorbereitete und gespeicherte Paarung direkt aus
    ' pairings.xml - unabhängig davon, ob Main2 gerade geöffnet ist. Schreibt in dieselben
    ' HomePlayer/AwayPlayer/HomePlayer2/AwayPlayer2-Arrays wie Main2 selbst.
    Private Sub ApplyPairingFromFile(pairingIndex As Integer)
        Try
            If Not IO.File.Exists(PAIRINGS_FILE_PATH) Then
                MessageBox.Show("No saved pairings found. Prepare pairings in ""Main2"" first.", "Select Pairing", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            Dim xmlDoc As New Xml.XmlDocument()
            xmlDoc.Load(PAIRINGS_FILE_PATH)
            Dim pairingNode = xmlDoc.SelectSingleNode($"//TennisPairings/Pairing[@index='{pairingIndex}']")
            If pairingNode Is Nothing Then
                MessageBox.Show($"Pairing {pairingIndex + 1} is empty.", "Select Pairing", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            Dim doublesNode = pairingNode.SelectSingleNode("Doubles")
            Dim isDoubles As Boolean = False
            If doublesNode IsNot Nothing Then Boolean.TryParse(doublesNode.InnerText, isDoubles)

            Dim newHome(8) As String
            Dim newAway(8) As String
            Dim newHome2(8) As String
            Dim newAway2(8) As String
            ReadPairingPlayer(pairingNode, "Home", newHome)
            ReadPairingPlayer(pairingNode, "Away", newAway)

            If String.IsNullOrEmpty(newHome(0)) OrElse String.IsNullOrEmpty(newAway(0)) Then
                MessageBox.Show($"Pairing {pairingIndex + 1} needs at least a Home and an Away player.", "Select Pairing", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            If isDoubles Then
                ReadPairingPlayer(pairingNode, "Home2", newHome2)
                ReadPairingPlayer(pairingNode, "Away2", newAway2)
            End If

            For i = 0 To 8
                HomePlayer(i) = newHome(i)
                AwayPlayer(i) = newAway(i)
                HomePlayer2(i) = newHome2(i)
                AwayPlayer2(i) = newAway2(i)
            Next
            CheckBox1.Checked = isDoubles

            RefreshAndSavePlayerSelection()
        Catch ex As Exception
            MessageBox.Show($"Error applying pairing: {ex.Message}", "Select Pairing", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ReadPairingPlayer(pairingNode As Xml.XmlNode, elementName As String, target As String())
        Dim playerNode = pairingNode.SelectSingleNode(elementName)
        If playerNode Is Nothing Then Return
        For i = 0 To 8
            Dim fieldNode = playerNode.SelectSingleNode(FIELD_NAMES(i))
            target(i) = If(fieldNode IsNot Nothing, fieldNode.InnerText, "")
        Next
    End Sub

    Private Sub Btn_SelectPairing1_Click(sender As Object, e As EventArgs) Handles Btn_SelectPairing1.Click
        ApplyPairingFromFile(0)
    End Sub

    Private Sub Btn_SelectPairing2_Click(sender As Object, e As EventArgs) Handles Btn_SelectPairing2.Click
        ApplyPairingFromFile(1)
    End Sub

    Private Sub Btn_SelectPairing3_Click(sender As Object, e As EventArgs) Handles Btn_SelectPairing3.Click
        ApplyPairingFromFile(2)
    End Sub

    Private Sub Btn_SelectPairing4_Click(sender As Object, e As EventArgs) Handles Btn_SelectPairing4.Click
        ApplyPairingFromFile(3)
    End Sub

    ' Speichert sofort, wenn zwischen Einzel/Doppel umgeschaltet wird - sonst würde die
    ' Umschaltung erst beim nächsten Ziehen eines Spielers persistiert (siehe
    ' SavePlayerSelection), was leicht zu einem falsch gespeicherten Zustand führen könnte.
    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        SaveDataToXML()
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
            If MessageBox.Show("Do you really want to end the program?", "Tennis26", MessageBoxButtons.YesNo) = DialogResult.Yes Then
                Application.Exit()
            End If
        Catch ex As Exception
            MessageBox.Show($"Error exiting application: {ex.Message}", "Error10", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub Btn_loadSettings_Click(sender As Object, e As EventArgs) Handles Btn_loadSettings.Click
        Tennis26_Settings.Show()
        Me.Hide()
        UpdateBestOfLabel()
    End Sub

    Private Sub Tennis26_Main_Activated(sender As Object, e As EventArgs) Handles Me.Activated
        UpdateBestOfLabel()
        ' Deckt z.B. den Fall ab, dass der Operator in Main2 gerade neue Paarungen
        ' gespeichert hat und dann zu Main zurückwechselt.
        LoadPairingButtonCaptions()
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

            ' Lade ausgewählten Home Player 2 (Doppel-Partner)
            Dim homePlayer2Node = xmlDoc.SelectSingleNode("//TennisData/SelectedPlayers/HomePlayer2")
            If homePlayer2Node IsNot Nothing AndAlso homePlayer2Node.HasChildNodes Then
                HomePlayer2(0) = GetXmlNodeValue(homePlayer2Node, "Name")
                HomePlayer2(1) = GetXmlNodeValue(homePlayer2Node, "FirstName")
                HomePlayer2(2) = GetXmlNodeValue(homePlayer2Node, "Country")
                HomePlayer2(3) = GetXmlNodeValue(homePlayer2Node, "CountryISO3")
                HomePlayer2(4) = GetXmlNodeValue(homePlayer2Node, "Age")
                HomePlayer2(5) = GetXmlNodeValue(homePlayer2Node, "Height")
                HomePlayer2(6) = GetXmlNodeValue(homePlayer2Node, "Data1")
                HomePlayer2(7) = GetXmlNodeValue(homePlayer2Node, "Data2")
                HomePlayer2(8) = GetXmlNodeValue(homePlayer2Node, "Data3")
            End If

            ' Lade ausgewählten Away Player 2 (Doppel-Partner)
            Dim awayPlayer2Node = xmlDoc.SelectSingleNode("//TennisData/SelectedPlayers/AwayPlayer2")
            If awayPlayer2Node IsNot Nothing AndAlso awayPlayer2Node.HasChildNodes Then
                AwayPlayer2(0) = GetXmlNodeValue(awayPlayer2Node, "Name")
                AwayPlayer2(1) = GetXmlNodeValue(awayPlayer2Node, "FirstName")
                AwayPlayer2(2) = GetXmlNodeValue(awayPlayer2Node, "Country")
                AwayPlayer2(3) = GetXmlNodeValue(awayPlayer2Node, "CountryISO3")
                AwayPlayer2(4) = GetXmlNodeValue(awayPlayer2Node, "Age")
                AwayPlayer2(5) = GetXmlNodeValue(awayPlayer2Node, "Height")
                AwayPlayer2(6) = GetXmlNodeValue(awayPlayer2Node, "Data1")
                AwayPlayer2(7) = GetXmlNodeValue(awayPlayer2Node, "Data2")
                AwayPlayer2(8) = GetXmlNodeValue(awayPlayer2Node, "Data3")
            End If

            ' Doppel-Umschalter (CheckBox1) laden
            Dim isDoublesNode = xmlDoc.SelectSingleNode("//TennisData/SelectedPlayers/IsDoublesMatch")
            If isDoublesNode IsNot Nothing Then
                Dim isDoubles As Boolean
                Boolean.TryParse(isDoublesNode.InnerText, isDoubles)
                CheckBox1.Checked = isDoubles
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

            Dim xmlDoc = BuildPlayerDatabaseXml(includeMetadata:=True)

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
