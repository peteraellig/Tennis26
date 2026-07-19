' Separate Statistik-Anzeige, aus dem Scorer heraus aufrufbar. Liest ihre Werte direkt aus
' der TennisMatchEngine-Instanz des Scorers - die Form hält keinen eigenen Spielzustand und
' beeinflusst die Zähllogik oder den vMix-Versand in keiner Weise.
Public Class Tennis24_Statistics

    Private matchEngine As TennisMatchEngine

    Private Sub Tennis24_Statistics_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        SetupDataGridView()
        RefreshStatistics()
    End Sub

    ' Wird vom Scorer beim Öffnen aufgerufen, um die Engine bekanntzugeben.
    Public Sub AttachMatch(engine As TennisMatchEngine)
        matchEngine = engine
        If DataGridView_Stats.Rows.Count > 0 Then RefreshStatistics()
    End Sub

    Private Sub SetupDataGridView()
        ' DataGridView konfigurieren
        DataGridView_Stats.AllowUserToAddRows = False
        DataGridView_Stats.AllowUserToDeleteRows = False
        DataGridView_Stats.AllowUserToResizeColumns = False
        DataGridView_Stats.AllowUserToResizeRows = False
        DataGridView_Stats.ReadOnly = True
        DataGridView_Stats.RowHeadersVisible = False
        DataGridView_Stats.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        ' Vertikale Scrollleiste zulassen - die Liste ist mit den Breakball-Zeilen länger
        ' geworden und soll auch bei kleinerem Fenster vollständig erreichbar bleiben.
        DataGridView_Stats.ScrollBars = ScrollBars.Vertical
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
        DataGridView_Stats.Rows.Add("═══ CURRENT GAME ═══", "", "")
        DataGridView_Stats.Rows.Add("Points", "", "")
        DataGridView_Stats.Rows.Add("Games", "", "")
        DataGridView_Stats.Rows.Add("Sets", "", "")
        DataGridView_Stats.Rows.Add("", "", "") ' Leerzeile

        DataGridView_Stats.Rows.Add("═══ MATCH INFO ═══", "", "")
        DataGridView_Stats.Rows.Add("Current Set", "", "")
        DataGridView_Stats.Rows.Add("Match Type", "", "")
        DataGridView_Stats.Rows.Add("Serving", "", "")
        DataGridView_Stats.Rows.Add("Tiebreak", "", "")
        DataGridView_Stats.Rows.Add("", "", "") ' Leerzeile

        DataGridView_Stats.Rows.Add("═══ STATISTICS ═══", "", "")
        DataGridView_Stats.Rows.Add("Total Points", "", "")
        DataGridView_Stats.Rows.Add("Service Games", "", "")
        DataGridView_Stats.Rows.Add("Service Games Won", "", "")
        DataGridView_Stats.Rows.Add("Service Win %", "", "")
        DataGridView_Stats.Rows.Add("Breaks", "", "")
        DataGridView_Stats.Rows.Add("Tiebreaks Won", "", "")
        DataGridView_Stats.Rows.Add("Longest Game", "", "")
        DataGridView_Stats.Rows.Add("Points Win %", "", "")
        DataGridView_Stats.Rows.Add("", "", "") ' Leerzeile

        DataGridView_Stats.Rows.Add("═══ BREAK POINTS ═══", "", "")
        DataGridView_Stats.Rows.Add("Break Pts won/total", "", "")
        DataGridView_Stats.Rows.Add("BP Conversion %", "", "")
        DataGridView_Stats.Rows.Add("BP saved", "", "")
        DataGridView_Stats.Rows.Add("BREAK POINT now", "", "")

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
        DataGridView_Stats.Rows(4).DefaultCellStyle.BackColor = Color.WhiteSmoke
        DataGridView_Stats.Rows(10).DefaultCellStyle.BackColor = Color.WhiteSmoke
        DataGridView_Stats.Rows(20).DefaultCellStyle.BackColor = Color.WhiteSmoke
    End Sub

    ' Vom Scorer nach jeder Zustandsänderung aufgerufen (und beim Öffnen der Form).
    Public Sub RefreshStatistics()
        If matchEngine Is Nothing OrElse DataGridView_Stats.Rows.Count < 26 Then Return

        UpdatePlayerNameHeaders()

        ' Current Game
        DataGridView_Stats.Rows(1).Cells(1).Value = matchEngine.ConvertPointsToTennisScore(matchEngine.HomePoints, matchEngine.AwayPoints)
        DataGridView_Stats.Rows(1).Cells(2).Value = matchEngine.ConvertPointsToTennisScore(matchEngine.AwayPoints, matchEngine.HomePoints)
        DataGridView_Stats.Rows(2).Cells(1).Value = matchEngine.HomeGames.ToString()
        DataGridView_Stats.Rows(2).Cells(2).Value = matchEngine.AwayGames.ToString()
        DataGridView_Stats.Rows(3).Cells(1).Value = matchEngine.HomeSets.ToString()
        DataGridView_Stats.Rows(3).Cells(2).Value = matchEngine.AwaySets.ToString()

        ' Match Info
        DataGridView_Stats.Rows(6).Cells(1).Value = matchEngine.CurrentSet.ToString()
        DataGridView_Stats.Rows(6).Cells(2).Value = ""

        Dim matchType = If(Tennis24_Settings.TextBoxValues(50) = 3, "Best of 3", "Best of 5")
        DataGridView_Stats.Rows(7).Cells(1).Value = matchType
        DataGridView_Stats.Rows(7).Cells(2).Value = ""

        Dim serving = If(matchEngine.IsHomeServing, "Home", "Away")
        DataGridView_Stats.Rows(8).Cells(1).Value = serving
        DataGridView_Stats.Rows(8).Cells(2).Value = ""

        Dim tiebreakStatus = If(matchEngine.IsMatchTiebreakSet(), "MATCH-TB", If(matchEngine.IsTiebreak, "ACTIVE", "No"))
        DataGridView_Stats.Rows(9).Cells(1).Value = tiebreakStatus
        DataGridView_Stats.Rows(9).Cells(2).Value = ""

        ' Statistics
        DataGridView_Stats.Rows(12).Cells(1).Value = matchEngine.HomeTotalPoints.ToString()
        DataGridView_Stats.Rows(12).Cells(2).Value = matchEngine.AwayTotalPoints.ToString()

        ' Service Games = Service Games Won + Break Points (gegen mich)
        Dim homeServiceGamesTotal = matchEngine.HomeServiceGamesWon + matchEngine.AwayBreaks
        Dim awayServiceGamesTotal = matchEngine.AwayServiceGamesWon + matchEngine.HomeBreaks

        DataGridView_Stats.Rows(13).Cells(1).Value = homeServiceGamesTotal.ToString()
        DataGridView_Stats.Rows(13).Cells(2).Value = awayServiceGamesTotal.ToString()

        DataGridView_Stats.Rows(14).Cells(1).Value = matchEngine.HomeServiceGamesWon.ToString()
        DataGridView_Stats.Rows(14).Cells(2).Value = matchEngine.AwayServiceGamesWon.ToString()

        ' Service Win Percentage
        Dim homeServiceWinPct = If(homeServiceGamesTotal > 0, Math.Round((matchEngine.HomeServiceGamesWon / homeServiceGamesTotal) * 100, 1), 0)
        Dim awayServiceWinPct = If(awayServiceGamesTotal > 0, Math.Round((matchEngine.AwayServiceGamesWon / awayServiceGamesTotal) * 100, 1), 0)
        DataGridView_Stats.Rows(15).Cells(1).Value = $"{homeServiceWinPct}%"
        DataGridView_Stats.Rows(15).Cells(2).Value = $"{awayServiceWinPct}%"

        DataGridView_Stats.Rows(16).Cells(1).Value = matchEngine.HomeBreaks.ToString()
        DataGridView_Stats.Rows(16).Cells(2).Value = matchEngine.AwayBreaks.ToString()

        DataGridView_Stats.Rows(17).Cells(1).Value = matchEngine.HomeTiebreaksWon.ToString()
        DataGridView_Stats.Rows(17).Cells(2).Value = matchEngine.AwayTiebreaksWon.ToString()

        DataGridView_Stats.Rows(18).Cells(1).Value = $"{matchEngine.LongestGame} pts"
        DataGridView_Stats.Rows(18).Cells(2).Value = ""

        ' Total Points Win Percentage
        Dim totalPointsPlayed = matchEngine.HomeTotalPoints + matchEngine.AwayTotalPoints
        Dim homePointsWinPct = If(totalPointsPlayed > 0, Math.Round((matchEngine.HomeTotalPoints / totalPointsPlayed) * 100, 1), 0)
        Dim awayPointsWinPct = If(totalPointsPlayed > 0, Math.Round((matchEngine.AwayTotalPoints / totalPointsPlayed) * 100, 1), 0)
        DataGridView_Stats.Rows(19).Cells(1).Value = $"{homePointsWinPct}%"
        DataGridView_Stats.Rows(19).Cells(2).Value = $"{awayPointsWinPct}%"

        UpdateBreakPointRows()
        HighlightStatistics()
    End Sub

    Private Sub UpdateBreakPointRows()
        ' Verwertete / gehabte Breakbälle (Chancen als Returner)
        DataGridView_Stats.Rows(22).Cells(1).Value = $"{matchEngine.HomeBreakPointsConverted}/{matchEngine.HomeBreakPointsTotal}"
        DataGridView_Stats.Rows(22).Cells(2).Value = $"{matchEngine.AwayBreakPointsConverted}/{matchEngine.AwayBreakPointsTotal}"

        Dim homeConversion = If(matchEngine.HomeBreakPointsTotal > 0, Math.Round((matchEngine.HomeBreakPointsConverted / matchEngine.HomeBreakPointsTotal) * 100, 1), 0)
        Dim awayConversion = If(matchEngine.AwayBreakPointsTotal > 0, Math.Round((matchEngine.AwayBreakPointsConverted / matchEngine.AwayBreakPointsTotal) * 100, 1), 0)
        DataGridView_Stats.Rows(23).Cells(1).Value = $"{homeConversion}%"
        DataGridView_Stats.Rows(23).Cells(2).Value = $"{awayConversion}%"

        ' Abgewehrte Breakbälle am eigenen Aufschlag = Chancen des Gegners, die er nicht nutzte
        Dim homeSaved = matchEngine.AwayBreakPointsTotal - matchEngine.AwayBreakPointsConverted
        Dim awaySaved = matchEngine.HomeBreakPointsTotal - matchEngine.HomeBreakPointsConverted
        DataGridView_Stats.Rows(24).Cells(1).Value = $"{homeSaved}/{matchEngine.AwayBreakPointsTotal}"
        DataGridView_Stats.Rows(24).Cells(2).Value = $"{awaySaved}/{matchEngine.HomeBreakPointsTotal}"

        ' Live-Anzeige für den aktuellen Punkt
        Dim breakPointHolder As String = matchEngine.BreakPointHolder()
        Dim breakPointCount As Integer = matchEngine.CurrentBreakPointCount()
        Dim breakPointText As String = If(breakPointCount > 1, $"{breakPointCount} BREAK POINTS", "BREAK POINT")

        DataGridView_Stats.Rows(25).Cells(1).Value = If(breakPointHolder = "home", breakPointText, "")
        DataGridView_Stats.Rows(25).Cells(2).Value = If(breakPointHolder = "away", breakPointText, "")

        ' Breakball-Zeile auffällig einfärben, solange die Situation läuft
        If breakPointHolder = "" Then
            DataGridView_Stats.Rows(25).DefaultCellStyle.BackColor = Color.White
            DataGridView_Stats.Rows(25).DefaultCellStyle.ForeColor = Color.Black
            DataGridView_Stats.Rows(25).DefaultCellStyle.Font = New Font("Segoe UI", 9, FontStyle.Regular)
        Else
            DataGridView_Stats.Rows(25).DefaultCellStyle.BackColor = Color.Red
            DataGridView_Stats.Rows(25).DefaultCellStyle.ForeColor = Color.White
            DataGridView_Stats.Rows(25).DefaultCellStyle.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        End If
    End Sub

    Private Sub UpdatePlayerNameHeaders()
        Dim homePlayerName As String = If(String.IsNullOrEmpty(Tennis24_Main.HomePlayer(0)), "HOME", Tennis24_Main.HomePlayer(0))
        Dim awayPlayerName As String = If(String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(0)), "AWAY", Tennis24_Main.AwayPlayer(0))

        If DataGridView_Stats.Columns.Count >= 3 Then
            DataGridView_Stats.Columns(1).HeaderText = homePlayerName
            DataGridView_Stats.Columns(2).HeaderText = awayPlayerName
        End If
    End Sub

    Private Sub HighlightStatistics()
        ' Serving Player hervorheben
        If matchEngine.IsHomeServing Then
            DataGridView_Stats.Rows(8).Cells(1).Style.BackColor = Color.LightGreen
            DataGridView_Stats.Rows(8).Cells(2).Style.BackColor = Color.White
        Else
            DataGridView_Stats.Rows(8).Cells(1).Style.BackColor = Color.White
            DataGridView_Stats.Rows(8).Cells(2).Style.BackColor = Color.LightGreen
        End If

        ' Tiebreak hervorheben
        If matchEngine.IsTiebreak Then
            DataGridView_Stats.Rows(9).DefaultCellStyle.BackColor = Color.Yellow
            DataGridView_Stats.Rows(9).DefaultCellStyle.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        Else
            DataGridView_Stats.Rows(9).DefaultCellStyle.BackColor = Color.White
        End If

        ' Breaks hervorheben
        If matchEngine.HomeBreaks > matchEngine.AwayBreaks Then
            DataGridView_Stats.Rows(16).Cells(1).Style.BackColor = Color.LightGreen
        ElseIf matchEngine.AwayBreaks > matchEngine.HomeBreaks Then
            DataGridView_Stats.Rows(16).Cells(2).Style.BackColor = Color.LightGreen
        End If
    End Sub

End Class
