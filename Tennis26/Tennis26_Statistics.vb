' Separate Statistik-Anzeige, aus dem Scorer heraus aufrufbar. Liest ihre Werte direkt aus
' der TennisMatchEngine-Instanz des Scorers - die Form hält keinen eigenen Spielzustand und
' beeinflusst die Zähllogik oder den vMix-Versand in keiner Weise.
Public Class Tennis26_Statistics

    Private matchEngine As TennisMatchEngine

    ' Zeilen werden über einen Schlüssel statt über den nackten Index angesprochen (rowOf(...)).
    ' Damit verschiebt eine neu eingefügte Zeile nicht mehr sämtliche Rows(N)-Zugriffe darunter -
    ' genau das ist wiederholt passiert, als Zeilen ergänzt wurden.
    Private ReadOnly rowIndex As New Dictionary(Of String, Integer)()

    Private Sub Tennis26_Statistics_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        SetupDataGridView()
        RefreshStatistics()
    End Sub

    ' Wird vom Scorer beim Öffnen aufgerufen, um die Engine bekanntzugeben.
    Public Sub AttachMatch(engine As TennisMatchEngine)
        matchEngine = engine
        If DataGridView_Stats.Rows.Count > 0 Then RefreshStatistics()
    End Sub

    ' Fügt eine Datenzeile hinzu und merkt sich ihren Index unter "key" (key muss nicht mit
    ' dem angezeigten Text übereinstimmen - wichtig für die mehreren gleich leeren Trennzeilen).
    Private Sub AddRow(key As String, displayText As String)
        rowIndex(key) = DataGridView_Stats.Rows.Add(displayText, "", "")
    End Sub

    Private Function rowOf(key As String) As DataGridViewRow
        Return DataGridView_Stats.Rows(rowIndex(key))
    End Function

    Private Sub SetupDataGridView()
        ' DataGridView konfigurieren
        DataGridView_Stats.AllowUserToAddRows = False
        DataGridView_Stats.AllowUserToDeleteRows = False
        DataGridView_Stats.AllowUserToResizeColumns = False
        DataGridView_Stats.AllowUserToResizeRows = False
        DataGridView_Stats.ReadOnly = True
        DataGridView_Stats.RowHeadersVisible = False
        DataGridView_Stats.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        ' ScrollBars sowie Grösse/Position des Grids kommen bewusst aus dem Designer und
        ' werden hier NICHT überschrieben.
        DataGridView_Stats.DefaultCellStyle.Font = New Font("Segoe UI", 9, FontStyle.Regular)
        DataGridView_Stats.ColumnHeadersDefaultCellStyle.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        DataGridView_Stats.ColumnHeadersDefaultCellStyle.BackColor = Color.LightBlue

        ' Spalten definieren
        DataGridView_Stats.Columns.Clear()
        DataGridView_Stats.Columns.Add("Property", "Statistic")
        DataGridView_Stats.Columns.Add("Home", "Home")
        DataGridView_Stats.Columns.Add("Away", "Away")

        ' Spaltenbreiten setzen
        DataGridView_Stats.Columns(0).Width = 140
        DataGridView_Stats.Columns(1).Width = 70
        DataGridView_Stats.Columns(2).Width = 70

        ' Zeilen hinzufügen
        rowIndex.Clear()
        AddRow("HeaderCurrentGame", "═══ CURRENT GAME ═══")
        AddRow("Points", "Points")
        AddRow("Games", "Games")
        AddRow("Sets", "Sets")
        AddRow("Tiebreak", "Tiebreak")
        AddRow("Blank1", "") ' Leerzeile

        AddRow("HeaderMatchInfo", "═══ MATCH INFO ═══")
        AddRow("CurrentSet", "Current Set")
        AddRow("MatchType", "Match Type")
        AddRow("Serving", "Serving")
        AddRow("EndsChanged", "Ends Changed")
        AddRow("Blank2", "") ' Leerzeile

        AddRow("HeaderStatistics", "═══ STATISTICS ═══")
        AddRow("TotalPoints", "Total Points")
        AddRow("ServiceGames", "Service Games")
        AddRow("ServiceGamesWon", "Service Games Won")
        AddRow("ServiceWinPct", "Service Win %")
        AddRow("Breaks", "Breaks")
        AddRow("TiebreaksWon", "Tiebreaks Won")
        AddRow("LongestGame", "Longest Game")
        AddRow("PointsWinPct", "Points Win %")
        AddRow("Blank3", "") ' Leerzeile

        AddRow("HeaderBreakPoints", "═══ BREAK POINTS ═══")
        AddRow("BreakPtsWonTotal", "Break Pts won/total")
        AddRow("BpConversionPct", "BP Conversion %")
        AddRow("BpSaved", "BP saved")
        AddRow("BreakPointNow", "BREAK POINT now")
        AddRow("MiniBreaks", "Mini-Breaks (TB)")

        ' Header-Zeilen formatieren
        For i As Integer = 0 To DataGridView_Stats.Rows.Count - 1
            DataGridView_Stats.Rows(i).Cells(0).Style.BackColor = Color.LightGray
            DataGridView_Stats.Rows(i).Cells(0).Style.Font = New Font("Segoe UI", 8, FontStyle.Bold)

            ' Header-Zeilen hervorheben
            If DataGridView_Stats.Rows(i).Cells(0).Value.ToString().Contains("═══") Then
                DataGridView_Stats.Rows(i).DefaultCellStyle.BackColor = Color.Navy
                DataGridView_Stats.Rows(i).DefaultCellStyle.ForeColor = Color.White
                DataGridView_Stats.Rows(i).DefaultCellStyle.Font = New Font("Segoe UI", 8, FontStyle.Bold)
            End If
        Next

        ' Leerzeilen formatieren
        rowOf("Blank1").DefaultCellStyle.BackColor = Color.WhiteSmoke
        rowOf("Blank2").DefaultCellStyle.BackColor = Color.WhiteSmoke
        rowOf("Blank3").DefaultCellStyle.BackColor = Color.WhiteSmoke
    End Sub

    ' Vom Scorer nach jeder Zustandsänderung aufgerufen (und beim Öffnen der Form).
    Public Sub RefreshStatistics()
        If matchEngine Is Nothing OrElse rowIndex.Count = 0 Then Return

        UpdatePlayerNameHeaders()

        ' Current Game
        rowOf("Points").Cells(1).Value = matchEngine.ConvertPointsToTennisScore(matchEngine.HomePoints, matchEngine.AwayPoints)
        rowOf("Points").Cells(2).Value = matchEngine.ConvertPointsToTennisScore(matchEngine.AwayPoints, matchEngine.HomePoints)
        rowOf("Games").Cells(1).Value = matchEngine.HomeGames.ToString()
        rowOf("Games").Cells(2).Value = matchEngine.AwayGames.ToString()
        rowOf("Sets").Cells(1).Value = matchEngine.HomeSets.ToString()
        rowOf("Sets").Cells(2).Value = matchEngine.AwaySets.ToString()

        Dim tiebreakStatus = If(matchEngine.IsMatchTiebreakSet(), "MATCH-TB", If(matchEngine.IsTiebreak, "ACTIVE", "No"))
        rowOf("Tiebreak").Cells(1).Value = tiebreakStatus
        rowOf("Tiebreak").Cells(2).Value = ""

        ' Match Info
        rowOf("CurrentSet").Cells(1).Value = matchEngine.CurrentSet.ToString()
        rowOf("CurrentSet").Cells(2).Value = ""

        ' String-Vergleich statt Vergleich mit der Zahl 3: TextBoxValues(50) ist ein String,
        ' und mit Option Strict Off würde ein leerer oder nicht numerischer Wert entweder
        ' eine Ausnahme auslösen oder stillschweigend "Best of 5" anzeigen.
        Dim matchType = If(Tennis26_Settings.TextBoxValues(50) = "5", "Best of 5", "Best of 3")
        rowOf("MatchType").Cells(1).Value = matchType
        rowOf("MatchType").Cells(2).Value = ""

        Dim serving = If(matchEngine.IsHomeServing, "Home", "Away")
        rowOf("Serving").Cells(1).Value = serving
        rowOf("Serving").Cells(2).Value = ""

        rowOf("EndsChanged").Cells(1).Value = If(matchEngine.AreSidesCurrentlySwapped(), "Yes", "No")
        rowOf("EndsChanged").Cells(2).Value = ""

        ' Statistics - alles hier zählt die Punkt-für-Punkt-Historie mit, ist also nach
        ' einem Zwischeneinstieg (IsMidMatchEntry) nur ab dem Einstiegspunkt korrekt, nicht
        ' für das ganze Match. Statt eine irreführende Teil-Zahl zu zeigen: "-".
        If matchEngine.IsMidMatchEntry Then
            For Each key In {"TotalPoints", "ServiceGames", "ServiceGamesWon", "ServiceWinPct", "Breaks", "TiebreaksWon", "LongestGame", "PointsWinPct"}
                rowOf(key).Cells(1).Value = "-"
                rowOf(key).Cells(2).Value = "-"
            Next
        Else
            rowOf("TotalPoints").Cells(1).Value = matchEngine.HomeTotalPoints.ToString()
            rowOf("TotalPoints").Cells(2).Value = matchEngine.AwayTotalPoints.ToString()

            ' Service Games = Service Games Won + Break Points (gegen mich)
            Dim homeServiceGamesTotal = matchEngine.HomeServiceGamesWon + matchEngine.AwayBreaks
            Dim awayServiceGamesTotal = matchEngine.AwayServiceGamesWon + matchEngine.HomeBreaks

            rowOf("ServiceGames").Cells(1).Value = homeServiceGamesTotal.ToString()
            rowOf("ServiceGames").Cells(2).Value = awayServiceGamesTotal.ToString()

            rowOf("ServiceGamesWon").Cells(1).Value = matchEngine.HomeServiceGamesWon.ToString()
            rowOf("ServiceGamesWon").Cells(2).Value = matchEngine.AwayServiceGamesWon.ToString()

            ' Service Win Percentage
            Dim homeServiceWinPct = If(homeServiceGamesTotal > 0, Math.Round((matchEngine.HomeServiceGamesWon / homeServiceGamesTotal) * 100, 1), 0)
            Dim awayServiceWinPct = If(awayServiceGamesTotal > 0, Math.Round((matchEngine.AwayServiceGamesWon / awayServiceGamesTotal) * 100, 1), 0)
            rowOf("ServiceWinPct").Cells(1).Value = $"{homeServiceWinPct}%"
            rowOf("ServiceWinPct").Cells(2).Value = $"{awayServiceWinPct}%"

            rowOf("Breaks").Cells(1).Value = matchEngine.HomeBreaks.ToString()
            rowOf("Breaks").Cells(2).Value = matchEngine.AwayBreaks.ToString()

            rowOf("TiebreaksWon").Cells(1).Value = matchEngine.HomeTiebreaksWon.ToString()
            rowOf("TiebreaksWon").Cells(2).Value = matchEngine.AwayTiebreaksWon.ToString()

            rowOf("LongestGame").Cells(1).Value = $"{matchEngine.LongestGame} pts"
            rowOf("LongestGame").Cells(2).Value = ""

            ' Total Points Win Percentage
            Dim totalPointsPlayed = matchEngine.HomeTotalPoints + matchEngine.AwayTotalPoints
            Dim homePointsWinPct = If(totalPointsPlayed > 0, Math.Round((matchEngine.HomeTotalPoints / totalPointsPlayed) * 100, 1), 0)
            Dim awayPointsWinPct = If(totalPointsPlayed > 0, Math.Round((matchEngine.AwayTotalPoints / totalPointsPlayed) * 100, 1), 0)
            rowOf("PointsWinPct").Cells(1).Value = $"{homePointsWinPct}%"
            rowOf("PointsWinPct").Cells(2).Value = $"{awayPointsWinPct}%"
        End If

        UpdateBreakPointRows()
        HighlightStatistics()
    End Sub

    Private Sub UpdateBreakPointRows()
        ' Wie oben: Breakball-/Mini-Break-Statistik basiert auf der Punkt-für-Punkt-Historie -
        ' nach einem Zwischeneinstieg nur ab dem Einstiegspunkt korrekt. BreakPointNow ist
        ' dagegen eine reine Live-Anzeige aus dem AKTUELLEN Punktestand, bleibt also gültig.
        If matchEngine.IsMidMatchEntry Then
            For Each key In {"BreakPtsWonTotal", "BpConversionPct", "BpSaved", "MiniBreaks"}
                rowOf(key).Cells(1).Value = "-"
                rowOf(key).Cells(2).Value = "-"
            Next
            Dim miniBreaksRowSuppressed = rowOf("MiniBreaks")
            miniBreaksRowSuppressed.DefaultCellStyle.Font = New Font("Segoe UI", 9, FontStyle.Regular)
            miniBreaksRowSuppressed.Cells(1).Style.BackColor = Color.White
            miniBreaksRowSuppressed.Cells(2).Style.BackColor = Color.White
            UpdateBreakPointNowRow()
            Return
        End If

        ' Verwertete / gehabte Breakbälle (Chancen als Returner)
        rowOf("BreakPtsWonTotal").Cells(1).Value = $"{matchEngine.HomeBreakPointsConverted}/{matchEngine.HomeBreakPointsTotal}"
        rowOf("BreakPtsWonTotal").Cells(2).Value = $"{matchEngine.AwayBreakPointsConverted}/{matchEngine.AwayBreakPointsTotal}"

        Dim homeConversion = If(matchEngine.HomeBreakPointsTotal > 0, Math.Round((matchEngine.HomeBreakPointsConverted / matchEngine.HomeBreakPointsTotal) * 100, 1), 0)
        Dim awayConversion = If(matchEngine.AwayBreakPointsTotal > 0, Math.Round((matchEngine.AwayBreakPointsConverted / matchEngine.AwayBreakPointsTotal) * 100, 1), 0)
        rowOf("BpConversionPct").Cells(1).Value = $"{homeConversion}%"
        rowOf("BpConversionPct").Cells(2).Value = $"{awayConversion}%"

        ' Abgewehrte Breakbälle am eigenen Aufschlag = Chancen des Gegners, die er nicht nutzte
        Dim homeSaved = matchEngine.AwayBreakPointsTotal - matchEngine.AwayBreakPointsConverted
        Dim awaySaved = matchEngine.HomeBreakPointsTotal - matchEngine.HomeBreakPointsConverted
        rowOf("BpSaved").Cells(1).Value = $"{homeSaved}/{matchEngine.AwayBreakPointsTotal}"
        rowOf("BpSaved").Cells(2).Value = $"{awaySaved}/{matchEngine.HomeBreakPointsTotal}"

        UpdateBreakPointNowRow()

        ' Mini-Breaks: nur im (Match-)Tiebreak aussagekräftig
        Dim miniBreaksRow = rowOf("MiniBreaks")
        miniBreaksRow.Cells(1).Value = matchEngine.HomeMiniBreaks.ToString()
        miniBreaksRow.Cells(2).Value = matchEngine.AwayMiniBreaks.ToString()

        If matchEngine.IsInAnyTiebreak() Then
            ' Wer im laufenden Tiebreak mehr Mini-Breaks hat, liegt effektiv vorne
            miniBreaksRow.DefaultCellStyle.Font = New Font("Segoe UI", 9, FontStyle.Bold)
            miniBreaksRow.Cells(1).Style.BackColor = If(matchEngine.HomeMiniBreaks > matchEngine.AwayMiniBreaks, Color.LightGreen, Color.White)
            miniBreaksRow.Cells(2).Style.BackColor = If(matchEngine.AwayMiniBreaks > matchEngine.HomeMiniBreaks, Color.LightGreen, Color.White)
        Else
            miniBreaksRow.DefaultCellStyle.Font = New Font("Segoe UI", 9, FontStyle.Regular)
            miniBreaksRow.Cells(1).Style.BackColor = Color.White
            miniBreaksRow.Cells(2).Style.BackColor = Color.White
        End If
    End Sub

    ' Reine Live-Anzeige aus dem AKTUELLEN Punktestand (nicht aus der Historie) - bleibt
    ' auch nach einem Zwischeneinstieg gültig, deshalb aus der IsMidMatchEntry-Unterdrückung
    ' herausgezogen und von beiden Zweigen in UpdateBreakPointRows aufgerufen.
    Private Sub UpdateBreakPointNowRow()
        Dim breakPointHolder As String = matchEngine.BreakPointHolder()
        Dim breakPointCount As Integer = matchEngine.CurrentBreakPointCount()
        Dim breakPointText As String = If(breakPointCount > 1, $"{breakPointCount} BREAK POINTS", "BREAK POINT")

        Dim breakPointRow = rowOf("BreakPointNow")
        breakPointRow.Cells(1).Value = If(breakPointHolder = "home", breakPointText, "")
        breakPointRow.Cells(2).Value = If(breakPointHolder = "away", breakPointText, "")

        ' Breakball-Zeile auffällig einfärben, solange die Situation läuft
        If breakPointHolder = "" Then
            breakPointRow.DefaultCellStyle.BackColor = Color.White
            breakPointRow.DefaultCellStyle.ForeColor = Color.Black
            breakPointRow.DefaultCellStyle.Font = New Font("Segoe UI", 9, FontStyle.Regular)
        Else
            breakPointRow.DefaultCellStyle.BackColor = Color.Red
            breakPointRow.DefaultCellStyle.ForeColor = Color.White
            breakPointRow.DefaultCellStyle.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        End If
    End Sub

    Private Sub UpdatePlayerNameHeaders()
        Dim homePlayerName As String = If(String.IsNullOrEmpty(Tennis26_Main.HomePlayer(0)), "HOME", Tennis26_Main.HomePlayer(0))
        Dim awayPlayerName As String = If(String.IsNullOrEmpty(Tennis26_Main.AwayPlayer(0)), "AWAY", Tennis26_Main.AwayPlayer(0))

        If DataGridView_Stats.Columns.Count >= 3 Then
            DataGridView_Stats.Columns(1).HeaderText = homePlayerName
            DataGridView_Stats.Columns(2).HeaderText = awayPlayerName
        End If
    End Sub

    Private Sub HighlightStatistics()
        ' Serving Player hervorheben
        Dim servingRow = rowOf("Serving")
        If matchEngine.IsHomeServing Then
            servingRow.Cells(1).Style.BackColor = Color.LightGreen
            servingRow.Cells(2).Style.BackColor = Color.White
        Else
            servingRow.Cells(1).Style.BackColor = Color.White
            servingRow.Cells(2).Style.BackColor = Color.LightGreen
        End If

        ' Tiebreak hervorheben
        Dim tiebreakRow = rowOf("Tiebreak")
        If matchEngine.IsTiebreak OrElse matchEngine.IsMatchTiebreakSet() Then
            tiebreakRow.DefaultCellStyle.BackColor = Color.Yellow
            tiebreakRow.DefaultCellStyle.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        Else
            tiebreakRow.DefaultCellStyle.BackColor = Color.White
            tiebreakRow.DefaultCellStyle.Font = New Font("Segoe UI", 9, FontStyle.Regular)
        End If

        ' Breaks hervorheben
        Dim breaksRow = rowOf("Breaks")
        breaksRow.Cells(1).Style.BackColor = Color.White
        breaksRow.Cells(2).Style.BackColor = Color.White
        If matchEngine.HomeBreaks > matchEngine.AwayBreaks Then
            breaksRow.Cells(1).Style.BackColor = Color.LightGreen
        ElseIf matchEngine.AwayBreaks > matchEngine.HomeBreaks Then
            breaksRow.Cells(2).Style.BackColor = Color.LightGreen
        End If
    End Sub

    Private Sub Btn_Close_Click(sender As Object, e As EventArgs) Handles Btn_Close.Click
        Me.Close()
    End Sub
End Class
