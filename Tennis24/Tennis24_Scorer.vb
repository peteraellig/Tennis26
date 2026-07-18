Imports System.Net

Public Class Tennis24_Scorer

    'zeigt keine Spielerdetails an wie alter etc.
    Private hidedetails As Boolean = False

    ' Toggle-Status für Scorebug Button
    Private scorebugtoggleStatus As Boolean = False
    Private largeResulttoggleStatus As Boolean = False
    Private lower1toggleStatus As Boolean = False
    Private lower2toggleStatus As Boolean = False
    Private titleToggleStatus As Boolean = False
    Private matchpairingToggleStatus As Boolean = False
    Private matchpairing1ToggleStatus As Boolean = False
    Private matchpairing2ToggleStatus As Boolean = False
    Private matchpairing3ToggleStatus As Boolean = False
    Private matchpairing4ToggleStatus As Boolean = False
    Private info1ToggleStatus As Boolean = False
    Private info2ToggleStatus As Boolean = False
    Private info3ToggleStatus As Boolean = False
    Private info4ToggleStatus As Boolean = False
    Private sponsor1ToggleStatus As Boolean = False
    Private sponsor2ToggleStatus As Boolean = False
    Private freename1ToggleStatus As Boolean = False
    Private freename2ToggleStatus As Boolean = False
    Private freename3ToggleStatus As Boolean = False
    Private freename4ToggleStatus As Boolean = False
    Private freename5ToggleStatus As Boolean = False
    Private ref1ToggleStatus As Boolean = False
    Private ref2ToggleStatus As Boolean = False
    Private com1ToggleStatus As Boolean = False
    Private com2ToggleStatus As Boolean = False


    ' Bestehende Variablen...
    Private isTiebreak As Boolean = False
    Private homePoints As Integer = 0
    Private awayPoints As Integer = 0
    Private homeGames As Integer = 0
    Private awayGames As Integer = 0
    Private homeSets As Integer = 0
    Private awaySets As Integer = 0

    Private stack As New Stack(Of MatchState)
    Private currentSet As Integer = 1

    ' Variable für Match-Ende Status
    Private isMatchFinished As Boolean = False

    ' Variable für No-Tiebreak-Regel
    Private noTiebreakMode As Boolean = False

    ' Statistik-Variablen
    Private homeTotalPoints As Integer = 0
    Private awayTotalPoints As Integer = 0
    Private homeBreaks As Integer = 0
    Private awayBreaks As Integer = 0
    Private homeServiceGamesWon As Integer = 0
    Private awayServiceGamesWon As Integer = 0
    Private isHomeServing As Boolean = True
    Private homeTiebreaksWon As Integer = 0
    Private awayTiebreaksWon As Integer = 0
    Private longestGame As Integer = 0
    Private currentGamePoints As Integer = 0

    ' Variable für ersten Punkt Check
    Private firstPointPlayed As Boolean = False

    Private autoSwitchServerBetweenSets As Boolean = True

    ' Server-Tracking zwischen Sets
    Private firstServerOfCurrentSet As Boolean = True  ' True = Home begann Set, False = Away begann Set

    'keypress handling
    Private Declare Function GetAsyncKeyState Lib "User32" (ByVal vkey As Integer) As Integer

    Private Class MatchState
        Public Property HomePoints As Integer
        Public Property AwayPoints As Integer
        Public Property HomeGames As Integer
        Public Property AwayGames As Integer
        Public Property HomeSets As Integer
        Public Property AwaySets As Integer
        Public Property CurrentSet As Integer
        Public Property IsTiebreak As Boolean
        Public Property HomeTotalPoints As Integer
        Public Property AwayTotalPoints As Integer
        Public Property HomeBreaks As Integer
        Public Property AwayBreaks As Integer
        Public Property HomeServiceGamesWon As Integer
        Public Property AwayServiceGamesWon As Integer
        Public Property IsHomeServing As Boolean
        Public Property FirstServerOfCurrentSet As Boolean
        Public Property HomeTiebreaksWon As Integer
        Public Property AwayTiebreaksWon As Integer
        Public Property LongestGame As Integer
        Public Property CurrentGamePoints As Integer
        Public Property FirstPointPlayed As Boolean
        Public Property IsMatchFinished As Boolean
        Public Property NoTiebreakMode As Boolean
    End Class

    Private Sub Tennis24_Scorer_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Tennis24_Settings.SetVariables()

        If Tennis24_Settings.TextBoxValues(50) = 3 Then
            lbl_home_s3.Visible = True
            lbl_away_s3.Visible = True
            lbl_home_s4.Visible = False
            lbl_away_s4.Visible = False
            lbl_home_s5.Visible = False
            lbl_away_s5.Visible = False
            Hidegames()
        Else
            lbl_home_s3.Visible = True
            lbl_away_s3.Visible = True
            lbl_home_s4.Visible = True
            lbl_away_s4.Visible = True
            lbl_home_s5.Visible = True
            lbl_away_s5.Visible = True
            Showgames()
        End If

        ' Scorebug Button initialisieren
        scorebugtoggleStatus = False
        Btn_Scorebug.BackColor = SystemColors.ButtonHighlight
        Btn_Scorebug.Text = "Scorebug OFF"

        SetupDataGridView()
        ResetMatch()
        'needed for keypress handling 
        Me.KeyPreview = True
        SetServerTo("home")

        ' CheckBox aus Settings laden
        CheckBox_keypress_Mode.Checked = My.Settings.keypress_mode
        CheckBox_hidedetails.Checked = My.Settings.hidedetails
        hidedetails = CheckBox_hidedetails.Checked

        ' CheckBox-Text entsprechend setzen
        If CheckBox_keypress_Mode.Checked Then
            CheckBox_keypress_Mode.Text = "Server/Returner mode"
        Else
            CheckBox_keypress_Mode.Text = "Home/Away mode"
        End If

        noTiebreakMode = CheckBox_noTiebreak.Checked
        If noTiebreakMode Then
            CheckBox_noTiebreak.Text = "No Tiebreak (Advantage Set)"
        Else
            CheckBox_noTiebreak.Text = "Normal (mit Tiebreak)"
        End If

        ' Button-Namen initial setzen
        UpdateButtonNames()
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, ByVal keyData As Keys) As Boolean
        Label1.Text = ($"Taste: {keyData} (Nummer: {CInt(keyData)})") 'Debug-Ausgabe der gedrückten Taste

        Select Case keyData
        'Left Arrow (37)
            Case 37
                If CheckBox_keypress_Mode.Checked Then
                    ' Server/Returner Modus (aktuelles Verhalten)
                    If isHomeServing Then
                        btn_homepoint.PerformClick()  ' Server macht Punkt
                    Else
                        btn_awaypoint.PerformClick()  ' Server macht Punkt
                    End If
                Else
                    ' Home/Away Modus (feste Zuordnung)
                    btn_homepoint.PerformClick()  ' LINKS = immer Home
                End If

        'Right Arrow (39)
            Case 39
                If CheckBox_keypress_Mode.Checked Then
                    ' Server/Returner Modus (aktuelles Verhalten)
                    If isHomeServing Then
                        btn_awaypoint.PerformClick()  ' Returner macht Punkt
                    Else
                        btn_homepoint.PerformClick()  ' Returner macht Punkt
                    End If
                Else
                    ' Home/Away Modus (feste Zuordnung)
                    btn_awaypoint.PerformClick()  ' RECHTS = immer Away
                End If

        'Up Arrow (38) - Undo
            Case 38
                btn_undo.PerformClick()

        'S (83) - Server wechseln
            Case 83
                BtnChooseService.PerformClick()

        End Select
        PictureBox1.Focus() 'set focus to the logo picturebox to avoid wrong keypresses
        Return True
    End Function

    Private Sub SetupDataGridView()
        ' DataGridView konfigurieren
        DataGridView1.AllowUserToAddRows = False
        DataGridView1.AllowUserToDeleteRows = False
        DataGridView1.AllowUserToResizeColumns = False
        DataGridView1.AllowUserToResizeRows = False
        DataGridView1.ReadOnly = True
        DataGridView1.RowHeadersVisible = False
        DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        DataGridView1.ScrollBars = ScrollBars.None
        DataGridView1.DefaultCellStyle.Font = New Font("Segoe UI", 9, FontStyle.Regular)
        DataGridView1.ColumnHeadersDefaultCellStyle.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        DataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.LightBlue

        ' Spalten definieren
        DataGridView1.Columns.Clear()
        DataGridView1.Columns.Add("Property", "Statistic")
        DataGridView1.Columns.Add("Home", "Home")
        DataGridView1.Columns.Add("Away", "Away")

        ' Spaltenbreiten setzen
        DataGridView1.Columns(0).Width = 140
        DataGridView1.Columns(1).Width = 70
        DataGridView1.Columns(2).Width = 70

        ' Zeilen hinzufügen
        DataGridView1.Rows.Add("═══ CURRENT GAME ═══", "", "")
        DataGridView1.Rows.Add("Points", "", "")
        DataGridView1.Rows.Add("Games", "", "")
        DataGridView1.Rows.Add("Sets", "", "")
        DataGridView1.Rows.Add("", "", "") ' Leerzeile

        DataGridView1.Rows.Add("═══ MATCH INFO ═══", "", "")
        DataGridView1.Rows.Add("Current Set", "", "")
        DataGridView1.Rows.Add("Match Type", "", "")
        DataGridView1.Rows.Add("Serving", "", "")
        DataGridView1.Rows.Add("Tiebreak", "", "")
        DataGridView1.Rows.Add("", "", "") ' Leerzeile

        DataGridView1.Rows.Add("═══ STATISTICS ═══", "", "")
        DataGridView1.Rows.Add("Total Points", "", "")
        DataGridView1.Rows.Add("Service Games", "", "")
        DataGridView1.Rows.Add("Service Games Won", "", "")
        DataGridView1.Rows.Add("Service Win %", "", "")
        DataGridView1.Rows.Add("Break Points", "", "")
        DataGridView1.Rows.Add("Tiebreaks Won", "", "")
        DataGridView1.Rows.Add("Longest Game", "", "")
        DataGridView1.Rows.Add("Points Win %", "", "")

        ' Header-Zeilen formatieren
        For i As Integer = 0 To DataGridView1.Rows.Count - 1
            DataGridView1.Rows(i).Cells(0).Style.BackColor = Color.LightGray
            DataGridView1.Rows(i).Cells(0).Style.Font = New Font("Segoe UI", 8, FontStyle.Bold)

            ' Header-Zeilen hervorheben
            If DataGridView1.Rows(i).Cells(0).Value.ToString().Contains("═══") Then
                DataGridView1.Rows(i).DefaultCellStyle.BackColor = Color.Navy
                DataGridView1.Rows(i).DefaultCellStyle.ForeColor = Color.White
                DataGridView1.Rows(i).DefaultCellStyle.Font = New Font("Segoe UI", 8, FontStyle.Bold)
            End If
        Next

        ' Leerzeilen formatieren
        DataGridView1.Rows(4).DefaultCellStyle.BackColor = Color.WhiteSmoke
        DataGridView1.Rows(10).DefaultCellStyle.BackColor = Color.WhiteSmoke
    End Sub

    Private Sub UpdateDataGridView()
        If DataGridView1.Rows.Count >= 19 Then
            ' DEBUG: Zeige die tatsächlichen Werte
            Label1.Text = $"H-Won:{homeServiceGamesWon} A-Won:{awayServiceGamesWon} Breaks: H:{homeBreaks} A:{awayBreaks}"

            ' Current Game
            DataGridView1.Rows(1).Cells(1).Value = ConvertPointsToTennisScore(homePoints, awayPoints)
            DataGridView1.Rows(1).Cells(2).Value = ConvertPointsToTennisScore(awayPoints, homePoints)
            DataGridView1.Rows(2).Cells(1).Value = homeGames.ToString()
            DataGridView1.Rows(2).Cells(2).Value = awayGames.ToString()
            DataGridView1.Rows(3).Cells(1).Value = homeSets.ToString()
            DataGridView1.Rows(3).Cells(2).Value = awaySets.ToString()

            ' Match Info
            DataGridView1.Rows(6).Cells(1).Value = currentSet.ToString()
            DataGridView1.Rows(6).Cells(2).Value = ""

            Dim matchType = If(Tennis24_Settings.TextBoxValues(50) = 3, "Best of 3", "Best of 5")
            DataGridView1.Rows(7).Cells(1).Value = matchType
            DataGridView1.Rows(7).Cells(2).Value = ""

            Dim serving = If(isHomeServing, "Home", "Away")
            DataGridView1.Rows(8).Cells(1).Value = serving
            DataGridView1.Rows(8).Cells(2).Value = ""

            Dim tiebreakStatus = If(isTiebreak, "ACTIVE", "No")
            DataGridView1.Rows(9).Cells(1).Value = tiebreakStatus
            DataGridView1.Rows(9).Cells(2).Value = ""

            ' Statistics
            DataGridView1.Rows(12).Cells(1).Value = homeTotalPoints.ToString()
            DataGridView1.Rows(12).Cells(2).Value = awayTotalPoints.ToString()

            ' KORRIGIERTE Service Games Berechnung:
            ' Service Games = Service Games Won + Break Points (gegen mich)
            Dim homeServiceGamesTotal = homeServiceGamesWon + awayBreaks
            Dim awayServiceGamesTotal = awayServiceGamesWon + homeBreaks

            DataGridView1.Rows(13).Cells(1).Value = homeServiceGamesTotal.ToString()
            DataGridView1.Rows(13).Cells(2).Value = awayServiceGamesTotal.ToString()

            DataGridView1.Rows(14).Cells(1).Value = homeServiceGamesWon.ToString()
            DataGridView1.Rows(14).Cells(2).Value = awayServiceGamesWon.ToString()

            ' Service Win Percentage
            Dim homeServiceWinPct = If(homeServiceGamesTotal > 0, Math.Round((homeServiceGamesWon / homeServiceGamesTotal) * 100, 1), 0)
            Dim awayServiceWinPct = If(awayServiceGamesTotal > 0, Math.Round((awayServiceGamesWon / awayServiceGamesTotal) * 100, 1), 0)
            DataGridView1.Rows(15).Cells(1).Value = $"{homeServiceWinPct}%"
            DataGridView1.Rows(15).Cells(2).Value = $"{awayServiceWinPct}%"

            DataGridView1.Rows(16).Cells(1).Value = homeBreaks.ToString()
            DataGridView1.Rows(16).Cells(2).Value = awayBreaks.ToString()

            DataGridView1.Rows(17).Cells(1).Value = homeTiebreaksWon.ToString()
            DataGridView1.Rows(17).Cells(2).Value = awayTiebreaksWon.ToString()

            DataGridView1.Rows(18).Cells(1).Value = $"{longestGame} pts"
            DataGridView1.Rows(18).Cells(2).Value = ""

            ' Total Points Win Percentage
            Dim totalPointsPlayed = homeTotalPoints + awayTotalPoints
            Dim homePointsWinPct = If(totalPointsPlayed > 0, Math.Round((homeTotalPoints / totalPointsPlayed) * 100, 1), 0)
            Dim awayPointsWinPct = If(totalPointsPlayed > 0, Math.Round((awayTotalPoints / totalPointsPlayed) * 100, 1), 0)
            DataGridView1.Rows(19).Cells(1).Value = $"{homePointsWinPct}%"
            DataGridView1.Rows(19).Cells(2).Value = $"{awayPointsWinPct}%"

            ' Hervorhebungen
            HighlightStatistics()

            ' AM ENDE: Grafik-Engine aktualisieren
            SendDataToGraphicsEngine()

            If isMatchFinished Then Hidepoints()


        End If
    End Sub

    Private Sub HighlightStatistics()
        ' Serving Player hervorheben
        If isHomeServing Then
            DataGridView1.Rows(8).Cells(1).Style.BackColor = Color.LightGreen
            DataGridView1.Rows(8).Cells(2).Style.BackColor = Color.White
        Else
            DataGridView1.Rows(8).Cells(1).Style.BackColor = Color.White
            DataGridView1.Rows(8).Cells(2).Style.BackColor = Color.LightGreen
        End If

        ' Tiebreak hervorheben
        If isTiebreak Then
            DataGridView1.Rows(9).DefaultCellStyle.BackColor = Color.Yellow
            DataGridView1.Rows(9).DefaultCellStyle.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        Else
            DataGridView1.Rows(9).DefaultCellStyle.BackColor = Color.White
        End If

        ' Breaks hervorheben
        If homeBreaks > awayBreaks Then
            DataGridView1.Rows(16).Cells(1).Style.BackColor = Color.LightGreen
        ElseIf awayBreaks > homeBreaks Then
            DataGridView1.Rows(16).Cells(2).Style.BackColor = Color.LightGreen
        End If
    End Sub

    Private Sub Btn_homepoint_Click(sender As Object, e As EventArgs) Handles btn_homepoint.Click
        UpdatePoints("home")
    End Sub

    Private Sub Btn_awaypoint_Click(sender As Object, e As EventArgs) Handles btn_awaypoint.Click
        UpdatePoints("away")
    End Sub

    Private Sub BtnChooseService_Click(sender As Object, e As EventArgs) Handles BtnChooseService.Click
        SetServerTo("toggle")
    End Sub

    ' Zentrale Server-Setz-Methode
    Private Sub SetServerTo(serverType As String)
        ' Nur erlaubt, wenn noch kein Punkt gespielt wurde
        If Not firstPointPlayed AndAlso homePoints = 0 AndAlso awayPoints = 0 AndAlso homeTotalPoints = 0 AndAlso awayTotalPoints = 0 Then

            Select Case serverType.ToLower()
                Case "home"
                    isHomeServing = True
                Case "away"
                    isHomeServing = False
                Case "toggle"
                    isHomeServing = Not isHomeServing
            End Select

            ' Visuelle Updates
            UpdateServerDisplay()
            UpdateDataGridView()

            ' Button-Text aktualisieren
            BtnChooseService.Text = If(isHomeServing, "Home schlägt auf", "Away schlägt auf")

        Else
            MessageBox.Show("Server kann nur vor dem ersten Punkt gewechselt werden!",
                       "Server-Wechsel",
                       MessageBoxButtons.OK,
                       MessageBoxIcon.Information)
        End If
    End Sub

    Private Sub UpdateServerDisplay()
        ' PictureBoxes je nach Server einfärben
        If Not isMatchFinished Then
            If isHomeServing Then
                PBHome.Visible = True
                PBAway.Visible = False
            Else
                PBAway.Visible = True
                PBHome.Visible = False
            End If


            ' Button-Farben anpassen
            If isHomeServing Then
                btn_homepoint.BackColor = Color.LightGreen
                btn_awaypoint.BackColor = SystemColors.ButtonHighlight
            Else
                btn_awaypoint.BackColor = Color.LightGreen
                btn_homepoint.BackColor = SystemColors.ButtonHighlight
            End If

            ' Button-Namen aktualisieren (wichtig für Server/Returner Modus)
            UpdateButtonNames()
            LargeResult()
        End If
    End Sub

    Private Sub UpdateButtonNames()
        ' Spielernamen abrufen
        Dim homePlayerName As String = If(String.IsNullOrEmpty(Tennis24_Main.HomePlayer(0)), "HOME", Tennis24_Main.HomePlayer(0))
        Dim awayPlayerName As String = If(String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(0)), "AWAY", Tennis24_Main.AwayPlayer(0))

        ' DataGridView Spaltenüberschriften mit Spielernamen aktualisieren
        If DataGridView1.Columns.Count >= 3 Then
            DataGridView1.Columns(1).HeaderText = homePlayerName
            DataGridView1.Columns(2).HeaderText = awayPlayerName
        End If

        If CheckBox_keypress_Mode.Checked Then
            ' Server/Returner Modus
            If isHomeServing Then
                ' Home ist Server
                btn_homepoint.Text = "← SERVER" & vbNewLine & homePlayerName
                btn_awaypoint.Text = "→ RETURNER" & vbNewLine & awayPlayerName
            Else
                ' Away ist Server
                btn_homepoint.Text = "← RETURNER" & vbNewLine & homePlayerName
                btn_awaypoint.Text = "→ SERVER" & vbNewLine & awayPlayerName
            End If
        Else
            ' Home/Away Modus
            btn_homepoint.Text = "← " + homePlayerName
            btn_awaypoint.Text = "→ " + awayPlayerName
        End If
        ' Schreibt undo Button mit aufwärtspfeil an
        btn_undo.Text = "↑" & vbNewLine & "UNDO"

        'schreibt spielernamen_lower an
        Btn_Name_Home.Text = "lower" & vbNewLine & homePlayerName
        Btn_Name_Away.Text = "lower" & vbNewLine & awayPlayerName
    End Sub

    Private Sub UpdatePoints(player As String)
        ' Match beendet? Keine weiteren Punkte erlauben
        If isMatchFinished Then
            Return
        End If

        ' Ersten Punkt markieren
        If Not firstPointPlayed Then
            firstPointPlayed = True
            BtnChooseService.Enabled = False
            BtnChooseService.Text = "Server festgelegt"
        End If

        ' Zustand speichern
        stack.Push(New MatchState With {
            .HomePoints = homePoints,
            .AwayPoints = awayPoints,
            .HomeGames = homeGames,
            .AwayGames = awayGames,
            .HomeSets = homeSets,
            .AwaySets = awaySets,
            .CurrentSet = currentSet,
            .IsTiebreak = isTiebreak,
            .HomeTotalPoints = homeTotalPoints,
            .AwayTotalPoints = awayTotalPoints,
            .HomeBreaks = homeBreaks,
            .AwayBreaks = awayBreaks,
            .HomeServiceGamesWon = homeServiceGamesWon,
            .AwayServiceGamesWon = awayServiceGamesWon,
            .IsHomeServing = isHomeServing,
            .FirstServerOfCurrentSet = firstServerOfCurrentSet,
            .HomeTiebreaksWon = homeTiebreaksWon,
            .AwayTiebreaksWon = awayTiebreaksWon,
            .LongestGame = longestGame,
            .CurrentGamePoints = currentGamePoints,
            .FirstPointPlayed = firstPointPlayed,
            .IsMatchFinished = isMatchFinished,
            .NoTiebreakMode = noTiebreakMode
        })

        If player = "home" Then
            homePoints += 1
            homeTotalPoints += 1
        Else
            awayPoints += 1
            awayTotalPoints += 1
        End If

        currentGamePoints += 1
        UpdateScore()
    End Sub

    Private Sub UpdateScore()
        lbl_homepoint.Text = ConvertPointsToTennisScore(homePoints, awayPoints)
        lbl_awaypoint.Text = ConvertPointsToTennisScore(awayPoints, homePoints)

        If IsGameWon(homePoints, awayPoints) Then
            homeGames += 1
            CheckForBreak("home")
            TrackLongestGame()

            ' KORRIGIERT: Tiebreak-Server-Logik VOR ResetPoints()
            Dim shouldSwitchServer As Boolean = False
            If Not isTiebreak Then
                shouldSwitchServer = True
            Else
                ' Im Tiebreak: Server wechselt alle 2 Punkte
                ' Aktuelle Gesamtpunkte BEVOR Reset
                Dim totalTiebreakPoints = homeTotalPoints + awayTotalPoints
                If totalTiebreakPoints Mod 2 = 1 Then
                    shouldSwitchServer = True
                End If
            End If

            ResetPoints()

            ' Server wechseln nur wenn Match nicht beendet ist
            If Not isMatchFinished AndAlso shouldSwitchServer Then
                isHomeServing = Not isHomeServing
                UpdateServerDisplay()
            End If

        ElseIf IsGameWon(awayPoints, homePoints) Then
            awayGames += 1
            CheckForBreak("away")
            TrackLongestGame()

            ' KORRIGIERT: Tiebreak-Server-Logik VOR ResetPoints()
            Dim shouldSwitchServer As Boolean = False
            If Not isTiebreak Then
                shouldSwitchServer = True
            Else
                ' Im Tiebreak: Server wechselt alle 2 Punkte
                ' Aktuelle Gesamtpunkte BEVOR Reset
                Dim totalTiebreakPoints = homeTotalPoints + awayTotalPoints
                If totalTiebreakPoints Mod 2 = 1 Then
                    shouldSwitchServer = True
                End If
            End If

            ResetPoints()

            ' Server wechseln nur wenn Match nicht beendet ist
            If Not isMatchFinished AndAlso shouldSwitchServer Then
                isHomeServing = Not isHomeServing
                UpdateServerDisplay()
            End If
        End If

        UpdateGameLabels()

        If IsSetWon(homeGames, awayGames) Then
            homeSets += 1
            If isTiebreak Then
                homeTiebreaksWon += 1
            End If
            UpdateSetLabel("home")
            CheckForMatchEnd()
        ElseIf IsSetWon(awayGames, homeGames) Then
            awaySets += 1
            If isTiebreak Then
                awayTiebreaksWon += 1
            End If
            UpdateSetLabel("away")
            CheckForMatchEnd()
        End If

        UpdateDataGridView()
        LargeResult()
    End Sub


    Private Sub CheckForBreak(winner As String)
        If Not isTiebreak Then
            If winner = "home" Then
                If Not isHomeServing Then
                    ' Home hat Away's Aufschlag gebrochen
                    homeBreaks += 1
                Else
                    ' Home hat eigenen Aufschlag gehalten
                    homeServiceGamesWon += 1
                End If
            Else ' winner = "away"
                If isHomeServing Then
                    ' Away hat Home's Aufschlag gebrochen
                    awayBreaks += 1
                Else
                    ' Away hat eigenen Aufschlag gehalten
                    awayServiceGamesWon += 1
                End If
            End If
        End If
    End Sub

    Private Sub ResetMatch()
        homePoints = 0
        awayPoints = 0
        homeGames = 0
        awayGames = 0
        homeSets = 0
        awaySets = 0
        currentSet = 1
        isTiebreak = False
        firstPointPlayed = False
        isMatchFinished = False  ' Match-Status zurücksetzen

        homeTotalPoints = 0
        awayTotalPoints = 0
        homeBreaks = 0
        awayBreaks = 0
        homeServiceGamesWon = 0
        awayServiceGamesWon = 0
        isHomeServing = True
        firstServerOfCurrentSet = True
        homeTiebreaksWon = 0
        awayTiebreaksWon = 0
        longestGame = 0
        currentGamePoints = 0

        ' UI Updates...
        lbl_homepoint.Text = "0"
        lbl_awaypoint.Text = "0"
        lbl_home_s1.Text = "0"
        lbl_home_s2.Text = "0"
        lbl_home_s3.Text = "0"
        lbl_home_s4.Text = "0"
        lbl_home_s5.Text = "0"
        lbl_away_s1.Text = "0"
        lbl_away_s2.Text = "0"
        lbl_away_s3.Text = "0"
        lbl_away_s4.Text = "0"
        lbl_away_s5.Text = "0"
        lbl_current_set.Text = "Set 1"

        Lbl_Winner.Visible = False

        BtnChooseService.Enabled = True
        BtnChooseService.Text = "Wähle Server"

        stack.Clear()
        UpdateServerDisplay()
        UpdateDataGridView()
        Showpoints()
    End Sub

    Private Sub Btn_undo_Click(sender As Object, e As EventArgs) Handles btn_undo.Click
        If stack.Count > 0 Then
            Dim lastState = stack.Pop()
            homePoints = lastState.HomePoints
            awayPoints = lastState.AwayPoints
            homeGames = lastState.HomeGames
            awayGames = lastState.AwayGames
            homeSets = lastState.HomeSets
            awaySets = lastState.AwaySets
            currentSet = lastState.CurrentSet
            isTiebreak = lastState.IsTiebreak

            homeTotalPoints = lastState.HomeTotalPoints
            awayTotalPoints = lastState.AwayTotalPoints
            homeBreaks = lastState.HomeBreaks
            awayBreaks = lastState.AwayBreaks
            homeServiceGamesWon = lastState.HomeServiceGamesWon
            awayServiceGamesWon = lastState.AwayServiceGamesWon
            isHomeServing = lastState.IsHomeServing
            firstServerOfCurrentSet = lastState.FirstServerOfCurrentSet
            homeTiebreaksWon = lastState.HomeTiebreaksWon
            awayTiebreaksWon = lastState.AwayTiebreaksWon
            longestGame = lastState.LongestGame
            currentGamePoints = lastState.CurrentGamePoints
            firstPointPlayed = lastState.FirstPointPlayed
            isMatchFinished = lastState.IsMatchFinished
            noTiebreakMode = lastState.NoTiebreakMode
            CheckBox_noTiebreak.Checked = noTiebreakMode

            If homeTotalPoints = 0 AndAlso awayTotalPoints = 0 Then
                firstPointPlayed = False
                BtnChooseService.Enabled = True
                BtnChooseService.Text = "Wähle Server"
            Else
                firstPointPlayed = True
                BtnChooseService.Enabled = False
                BtnChooseService.Text = "Server festgelegt"
            End If

            lbl_homepoint.Text = ConvertPointsToTennisScore(homePoints, awayPoints)
            lbl_awaypoint.Text = ConvertPointsToTennisScore(awayPoints, homePoints)
            UpdateGameLabels()
            UpdateSetLabel(If(homeSets > lastState.HomeSets, "home", "away"))
            lbl_current_set.Text = $"Set {currentSet}"

            UpdateServerDisplay()
            UpdateDataGridView()
        End If
    End Sub

    Private Sub Btn_reset_match_Click(sender As Object, e As EventArgs) Handles btn_reset_match.Click
        ResetMatch()

        Dim sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "Off&Mix=0"
        Btn_Scorebug.BackColor = SystemColors.ButtonHighlight
        Btn_Scorebug.Text = "Scorebug OFF"
        ' Befehl an vMix senden
        SendHTMLtovMix(sendstring)

        Showpoints()
    End Sub

    Private Sub TrackLongestGame()
        If currentGamePoints > longestGame Then
            longestGame = currentGamePoints
        End If
        currentGamePoints = 0
    End Sub

    Private Sub CheckForMatchEnd()
        Dim setsToWin As Integer = Math.Ceiling(Tennis24_Settings.TextBoxValues(50) / 2.0)

        If homeSets = setsToWin OrElse awaySets = setsToWin Then
            ' Match ist beendet
            isMatchFinished = True
            Dim homePlayerName As String = If(String.IsNullOrEmpty(Tennis24_Main.HomePlayer(0)), "HOME", Tennis24_Main.HomePlayer(0))
            Dim awayPlayerName As String = If(String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(0)), "AWAY", Tennis24_Main.AwayPlayer(0))

            ' Match-Gewinner bestimmen
            If homeSets = setsToWin Then
                Lbl_Winner.Visible = True
                lbl_current_set.Text = $"{homePlayerName} wins the match! ({homeSets}:{awaySets} sets)"
                Lbl_Winner.Text = $"{homePlayerName} wins the match! ({homeSets}:{awaySets} sets)"
            ElseIf awaySets = setsToWin Then
                Lbl_Winner.Visible = True
                lbl_current_set.Text = $"{awayPlayerName} wins the match! ({awaySets}:{homeSets} sets)"
                Lbl_Winner.Text = $"{awayPlayerName} wins the match! ({awaySets}:{homeSets} sets)"
            End If
            UpdateScoreBug()
        Else
            ' Set-Wechsel
            currentSet += 1
            lbl_current_set.Text = $"Set {currentSet}"

            ' Tennis-Regel für Server zwischen Sets
            isHomeServing = Not firstServerOfCurrentSet
            firstServerOfCurrentSet = isHomeServing

            ResetGames()
            UpdateServerDisplay()
            UpdateDataGridView()
            UpdateScoreBug()
        End If
    End Sub

    Private Function IsSetWon(playerGames As Integer, opponentGames As Integer) As Boolean
        ' Check if no-tiebreak mode is active
        If noTiebreakMode Then
            ' No-Tiebreak-Modus: Set wird nur mit 2 Games Vorsprung gewonnen, kein Limit
            If playerGames >= 6 AndAlso playerGames - opponentGames >= 2 Then
                Return True
            End If
            Return False
        Else
            ' Normaler Modus: OHNE Tiebreak: 6 Games mit 2 Games Vorsprung
            If Not isTiebreak Then
                If playerGames >= 6 AndAlso playerGames - opponentGames >= 2 Then
                    Return True
                End If

                ' Tiebreak bei 6:6 starten
                If playerGames = 6 AndAlso opponentGames = 6 Then
                    isTiebreak = True
                    Return False
                End If
            Else
                ' IM Tiebreak: Set-Gewinn bei 7+ Punkten mit 2 Punkten Vorsprung
                ' ACHTUNG: playerGames/opponentGames sind hier die TIEBREAK-PUNKTE!
                If playerGames >= 7 AndAlso playerGames - opponentGames >= 2 Then
                    isTiebreak = False
                    Return True
                End If
            End If
        End If

        Return False
    End Function

    Private Sub ResetPoints()
        homePoints = 0
        awayPoints = 0
        lbl_homepoint.Text = "0"
        lbl_awaypoint.Text = "0"
    End Sub

    Private Sub ResetGames()
        homeGames = 0
        awayGames = 0
    End Sub

    Private Function IsGameWon(playerPoints As Integer, opponentPoints As Integer) As Boolean
        ' Prüft, ob ein Spieler das Game gewonnen hat
        If isTiebreak Then
            Return playerPoints >= 7 AndAlso playerPoints - opponentPoints >= 2
        Else
            Return playerPoints >= 4 AndAlso playerPoints - opponentPoints >= 2
        End If
    End Function

    Private Sub UpdateSetLabel(player As String)
        ' Set-Label aktualisieren   
        UpdateGameLabels()
    End Sub

    Private Sub UpdateGameLabels()
        ' Aktuelle Games im aktuellen Set aktualisieren
        Select Case currentSet
            Case 1
                lbl_home_s1.Text = homeGames.ToString()
                lbl_away_s1.Text = awayGames.ToString()
            Case 2
                lbl_home_s2.Text = homeGames.ToString()
                lbl_away_s2.Text = awayGames.ToString()
            Case 3
                lbl_home_s3.Text = homeGames.ToString()
                lbl_away_s3.Text = awayGames.ToString()
            Case 4
                lbl_home_s4.Text = homeGames.ToString()
                lbl_away_s4.Text = awayGames.ToString()
            Case 5
                lbl_home_s5.Text = homeGames.ToString()
                lbl_away_s5.Text = awayGames.ToString()
        End Select
    End Sub

    Private Function ConvertPointsToTennisScore(playerPoints As Integer, opponentPoints As Integer) As String
        ' Konvertiert Punktestand in Tennis-Score (0, 15, 30, 40, A)
        If isTiebreak Then
            Return playerPoints.ToString()
        ElseIf playerPoints >= 3 AndAlso opponentPoints >= 3 Then
            If playerPoints = opponentPoints Then
                Return "40"
            ElseIf playerPoints = opponentPoints + 1 Then
                Return "A"
            Else
                Return "40"
            End If
        Else
            Select Case playerPoints
                Case 0
                    Return "0"
                Case 1
                    Return "15"
                Case 2
                    Return "30"
                Case 3
                    Return "40"
                Case Else
                    Return "A"
            End Select
        End If
    End Function

    Private Sub CheckBox_keypress_Mode_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox_keypress_Mode.CheckedChanged
        ' CheckBox bedienung setzen und in Settings speichern
        If CheckBox_keypress_Mode.Checked Then
            CheckBox_keypress_Mode.Text = "Server/Returner Modus"
            My.Settings.keypress_mode = True
        Else
            CheckBox_keypress_Mode.Text = "Home/Away Modus"
            My.Settings.keypress_mode = False
        End If
        My.Settings.Save()

        UpdateButtonNames()
    End Sub

    Private Sub SendDataToGraphicsEngine()
        ' Scorebug(s) aktualisieren 
        Dim scorebugtitles() As String = {"scorebug_1s.gtzip", "scorebug_2s.gtzip", "scorebug_3s.gtzip", "scorebug_4s.gtzip", "scorebug_5s.gtzip"}

        For Each scorebugtitle As String In scorebugtitles
            Dim sendstring() As String
            Dim index As Integer = 0

            ReDim sendstring(17)

            sendstring(index) = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=hpoint.Text&Value=" + lbl_homepoint.Text
            index += 1
            sendstring(index) = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=apoint.Text&Value=" + lbl_awaypoint.Text
            index += 1

            Dim homePlayerName As String = If(String.IsNullOrEmpty(Tennis24_Main.HomePlayer(0)), "HOME", Tennis24_Main.HomePlayer(0))
            Dim awayPlayerName As String = If(String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(0)), "AWAY", Tennis24_Main.AwayPlayer(0))

            sendstring(index) = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=hname.Text&Value=" + homePlayerName
            index += 1
            sendstring(index) = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=aname.Text&Value=" + awayPlayerName
            index += 1

            Dim homeCountryISO3 As String = If(String.IsNullOrEmpty(Tennis24_Main.HomePlayer(3)), "HOM", Tennis24_Main.HomePlayer(3))
            Dim awayCountryISO3 As String = If(String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(3)), "AWY", Tennis24_Main.AwayPlayer(3))

            sendstring(index) = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=hcountry.Text&Value=" + homeCountryISO3
            index += 1
            sendstring(index) = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=acountry.Text&Value=" + awayCountryISO3
            index += 1

            If Not isMatchFinished Then
                If PBHome.Visible = True Then
                    sendstring(index) = "Function=SetImageVisibleOn&Input=" + scorebugtitle + "&SelectedName=hserve.Source"
                    index += 1
                    sendstring(index) = "Function=SetImageVisibleOff&Input=" + scorebugtitle + "&SelectedName=aserve.Source"
                    index += 1
                Else
                    sendstring(index) = "Function=SetImageVisibleOff&Input=" + scorebugtitle + "&SelectedName=hserve.Source"
                    index += 1
                    sendstring(index) = "Function=SetImageVisibleOn&Input=" + scorebugtitle + "&SelectedName=aserve.Source"
                    index += 1
                End If
            End If

            sendstring(index) = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=h1.Text&Value=" + lbl_home_s1.Text
            index += 1
            sendstring(index) = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=h2.Text&Value=" + lbl_home_s2.Text
            index += 1
            sendstring(index) = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=h3.Text&Value=" + lbl_home_s3.Text
            index += 1
            sendstring(index) = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=h4.Text&Value=" + lbl_home_s4.Text
            index += 1
            sendstring(index) = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=h5.Text&Value=" + lbl_home_s5.Text
            index += 1

            sendstring(index) = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=a1.Text&Value=" + lbl_away_s1.Text
            index += 1
            sendstring(index) = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=a2.Text&Value=" + lbl_away_s2.Text
            index += 1
            sendstring(index) = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=a3.Text&Value=" + lbl_away_s3.Text
            index += 1
            sendstring(index) = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=a4.Text&Value=" + lbl_away_s4.Text
            index += 1
            sendstring(index) = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=a5.Text&Value=" + lbl_away_s5.Text

            SendToGraphicsEngine(sendstring)
        Next
    End Sub

    Private Sub LargeResult()
        ' Grosse Ergebnisanzeige aktualisieren
        Dim scorebugtitle As String = "large_result.gtzip"
        Dim sendstring As String
        Dim homePlayerName As String = If(String.IsNullOrEmpty(Tennis24_Main.HomePlayer(0)), "HOME", Tennis24_Main.HomePlayer(1) & " " & Tennis24_Main.HomePlayer(0))
        Dim awayPlayerName As String = If(String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(0)), "AWAY", Tennis24_Main.AwayPlayer(1) & " " & Tennis24_Main.AwayPlayer(0))
        Dim homecountry As String = If(String.IsNullOrEmpty(Tennis24_Main.HomePlayer(0)), "HOME", Tennis24_Main.HomePlayer(3))
        Dim awaycountry As String = If(String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(0)), "AWAY", Tennis24_Main.AwayPlayer(3))
        sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=hname.Text&Value=" + homePlayerName : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=aname.Text&Value=" + awayPlayerName : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=hcountry.Text&Value=" + homecountry : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=acountry.Text&Value=" + awaycountry : SendHTMLtovMix(sendstring)

        sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=h1.Text&Value=" + lbl_home_s1.Text : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=h2.Text&Value=" + lbl_home_s2.Text : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=h3.Text&Value=" + lbl_home_s3.Text : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=h4.Text&Value=" + lbl_home_s4.Text : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=h5.Text&Value=" + lbl_home_s5.Text : SendHTMLtovMix(sendstring)

        sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=a1.Text&Value=" + lbl_away_s1.Text : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=a2.Text&Value=" + lbl_away_s2.Text : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=a3.Text&Value=" + lbl_away_s3.Text : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=a4.Text&Value=" + lbl_away_s4.Text : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=a5.Text&Value=" + lbl_away_s5.Text : SendHTMLtovMix(sendstring)

        If Not isMatchFinished Then
            If PBHome.Visible = True Then
                sendstring = "Function=SetImageVisibleOn&Input=" + scorebugtitle + "&SelectedName=hserve.Source" : SendHTMLtovMix(sendstring)
                sendstring = "Function=SetImageVisibleOff&Input=" + scorebugtitle + "&SelectedName=aserve.Source" : SendHTMLtovMix(sendstring)
            Else
                sendstring = "Function=SetImageVisibleOff&Input=" + scorebugtitle + "&SelectedName=hserve.Source" : SendHTMLtovMix(sendstring)
                sendstring = "Function=SetImageVisibleOn&Input=" + scorebugtitle + "&SelectedName=aserve.Source" : SendHTMLtovMix(sendstring)
            End If
        End If

        Dim flagInfo = GetFlagInfo(homecountry)
        If flagInfo.Exists Then
            sendstring = "Function=SetImage&Input=" + scorebugtitle + "&SelectedName=hcountry_flag.Source&Value=" + flagInfo.Path : SendHTMLtovMix(sendstring)
        Else
            sendstring = "Function=SetImage&Input=" + scorebugtitle + "&SelectedName=hcountry_flag.Source&Value=C:\VMIX\tennis\flags\transparent.png" : SendHTMLtovMix(sendstring)
        End If

        flagInfo = GetFlagInfo(awaycountry)
        If flagInfo.Exists Then
            sendstring = "Function=SetImage&Input=" + scorebugtitle + "&SelectedName=acountry_flag.Source&Value=" + flagInfo.Path : SendHTMLtovMix(sendstring)
        Else
            sendstring = "Function=SetImage&Input=" + scorebugtitle + "&SelectedName=acountry_flag.Source&Value=C:\VMIX\tennis\flags\transparent.png" : SendHTMLtovMix(sendstring)
        End If

    End Sub

    Private Function GetFlagInfo(isoCode As String) As (Path As String, Exists As Boolean)
        ' prüft auf leeren oder ungültigen ISO-Code und erstellt dann einen kompletten Pfad mit der flagge  
        ' Basis-Pfad für Flaggen
        Dim basePath As String = "C:\VMIX\tennis\flags\"

        ' Pfad zusammensetzen
        Dim flagPath As String = basePath & isoCode.ToUpper() & ".png"

        ' Prüfen ob die Flaggendatei existiert
        Dim flagExists As Boolean = System.IO.File.Exists(flagPath)

        Return (flagPath, flagExists)
    End Function

    Private Sub SendToGraphicsEngine(sendArray() As String)
        'falls ein array mit mehreren befehlen übergeben wird, werden diese nacheinander an vMix geschickt
        For i As Integer = 0 To sendArray.Length - 1
            If sendArray(i) IsNot Nothing Then
                SendHTMLtovMix(sendArray(i))
            End If
        Next
    End Sub

    Public Sub SendHTMLtovMix(ByVal HTML_URL As String)
        'Sendet einen HTML-Befehl an vMix   
        HTML_URL = "http://" + Tennis24_Settings.TextBoxValues(45) + ":" + Tennis24_Settings.TextBoxValues(46) + "/API/?" + HTML_URL
        Dim responseData As String
        Label12.Text = HTML_URL

        Try
            Dim cookieJar As New Net.CookieContainer()
            Dim hwrequest As Net.HttpWebRequest = Net.WebRequest.Create(HTML_URL)
            hwrequest.CookieContainer = cookieJar
            hwrequest.Accept = "*/*"
            hwrequest.AllowAutoRedirect = True
            hwrequest.UserAgent = "http_requester/0.1"
            hwrequest.Method = "GET"
            hwrequest.Timeout = 30

            Dim hwresponse As Net.HttpWebResponse = hwrequest.GetResponse()
            If hwresponse.StatusCode = Net.HttpStatusCode.OK Then
                Dim responseStream As New IO.StreamReader(hwresponse.GetResponseStream())
                responseData = responseStream.ReadToEnd()
                Label7.Text = responseData
            End If
            hwresponse.Close()
        Catch ex As Exception
            Label7.Text = ("Exception Error in VTX (vMix running?): " & ex.Message)
        End Try
    End Sub

    Private Sub Btn_exit_Click(sender As Object, e As EventArgs) Handles Btn_exit.Click
        'beendet das programm und öffnet das hauptfenster   
        Me.Close()
        Tennis24_Main.Show()
    End Sub

    Private Sub Btn_Scorebug_Click(sender As Object, e As EventArgs) Handles Btn_Scorebug.Click
        'blendet scorebug ein und aus
        Dim sendstring As String

        scorebugtoggleStatus = Not scorebugtoggleStatus

        If scorebugtoggleStatus Then
            sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(2) + "In&Input=scorebug_" & currentSet.ToString() & "s.gtzip&Mix=0"
            Btn_Scorebug.BackColor = Color.Red
            Btn_Scorebug.Text = $"Scorebug ON (Set {currentSet})"
        Else
            sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(2) + "Out&Input=scorebug_" & currentSet.ToString() & "s.gtzip&Mix=0"
            Btn_Scorebug.BackColor = SystemColors.ButtonHighlight
            Btn_Scorebug.Text = "Scorebug OFF"
        End If

        SendHTMLtovMix(sendstring)
    End Sub

    Private Sub Btn_LargeResult_Click(sender As Object, e As EventArgs) Handles Btn_LargeResult.Click
        'blendet grosses resultat ein und aus
        Dim sendstring As String

        ' Reset other toggles first
        ResetOtherOverlayToggles("largeresult")

        largeResulttoggleStatus = Not largeResulttoggleStatus

        If largeResulttoggleStatus Then
            sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "In&Input=large_result.gtzip&Mix=0"
            Btn_LargeResult.BackColor = Color.Red
            Btn_LargeResult.Text = $"Large Result ON (Set {currentSet})"
        Else
            sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "Out&Input=large_result.gtzip&Mix=0"
            Btn_LargeResult.BackColor = SystemColors.ButtonHighlight
            Btn_LargeResult.Text = "Large Result OFF"
        End If

        SendHTMLtovMix(sendstring)
    End Sub

    Private Sub UpdateScoreBug()
        'Es gibt verschiedene Scorebug-Templates für verschiedene Sets gibt (scorebug_1s.gtzip, scorebug_2s.gtzip, etc.), lädt automatisch das richtige Template
        If scorebugtoggleStatus Then
            Dim sendstring As String
            sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(2) + "In&Input=scorebug_" & currentSet.ToString() & "s.gtzip&Mix=0"
            Btn_Scorebug.Text = $"Scorebug ON (Set {currentSet})"
            SendHTMLtovMix(sendstring)
        End If
    End Sub

    Private Sub Hidepoints()
        'versteckt die punkte und tennisball
        Dim sendstring As String
        Dim nametemplate As String

        For i = 2 To 5
            nametemplate = "scorebug_" + Trim(Str(i)) + "s.gtzip"
            Dim image = "C:\VMIX\tennis\graphical templates\scorebug_bg_" + Trim(Str(i)) + "_bl.png"
            sendstring = "Function=SetImage&Input=" & nametemplate & "&SelectedName=bg" + Trim(Str(i)) + ".Source&Value=" & image : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetTextVisibleOff&Input=" & nametemplate & "&SelectedName=hpoint.Text" : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetTextVisibleOff&Input=" & nametemplate & "&SelectedName=apoint.Text" : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetImageVisibleOff&Input=" & nametemplate & "&SelectedName=hserve.Source" : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetImageVisibleOff&Input=" & nametemplate & "&SelectedName=aserve.Source" : SendHTMLtovMix(sendstring)
        Next

        If currentSet = 2 Then
            nametemplate = "large_result.gtzip"
            sendstring = "Function=SetTextVisibleOff&Input=" & nametemplate & "&SelectedName=h3.Text" : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetTextVisibleOff&Input=" & nametemplate & "&SelectedName=a3.Text" : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetImageVisibleOff&Input=" & nametemplate & "&SelectedName=h3_bg.Source" : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetImageVisibleOff&Input=" & nametemplate & "&SelectedName=a3_bg.Source" : SendHTMLtovMix(sendstring)

        End If

        If currentSet = 3 Then
            nametemplate = "large_result.gtzip"
            sendstring = "Function=SetTextVisibleOff&Input=" & nametemplate & "&SelectedName=h4.Text" : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetTextVisibleOff&Input=" & nametemplate & "&SelectedName=a4.Text" : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetTextVisibleOff&Input=" & nametemplate & "&SelectedName=h5.Text" : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetTextVisibleOff&Input=" & nametemplate & "&SelectedName=a5.Text" : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetImageVisibleOff&Input=" & nametemplate & "&SelectedName=h4_bg.Source" : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetImageVisibleOff&Input=" & nametemplate & "&SelectedName=a4_bg.Source" : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetImageVisibleOff&Input=" & nametemplate & "&SelectedName=h5_bg.Source" : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetImageVisibleOff&Input=" & nametemplate & "&SelectedName=a5_bg.Source" : SendHTMLtovMix(sendstring)
        End If

        If currentSet = 4 Then
            nametemplate = "large_result.gtzip"
            sendstring = "Function=SetTextVisibleOff&Input=" & nametemplate & "&SelectedName=h5.Text" : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetTextVisibleOff&Input=" & nametemplate & "&SelectedName=a5.Text" : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetImageVisibleOff&Input=" & nametemplate & "&SelectedName=h5_bg.Source" : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetImageVisibleOff&Input=" & nametemplate & "&SelectedName=a5_bg.Source" : SendHTMLtovMix(sendstring)

        End If

        nametemplate = "large_result.gtzip"
        sendstring = "Function=SetImageVisibleOff&Input=" & nametemplate & "&SelectedName=hserve.Source" : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetImageVisibleOff&Input=" & nametemplate & "&SelectedName=aserve.Source" : SendHTMLtovMix(sendstring)
    End Sub

    Private Sub Hidegames()
        Dim sendstring As String
        Dim nametemplate As String = "large_result.gtzip"

        sendstring = "Function=SetTextVisibleOff&Input=" & nametemplate & "&SelectedName=h4.Text" : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetTextVisibleOff&Input=" & nametemplate & "&SelectedName=h5.Text" : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetTextVisibleOff&Input=" & nametemplate & "&SelectedName=a4.Text" : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetTextVisibleOff&Input=" & nametemplate & "&SelectedName=a5.Text" : SendHTMLtovMix(sendstring)

        sendstring = "Function=SetImageVisibleOff&Input=" & nametemplate & "&SelectedName=h4_bg.Source" : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetImageVisibleOff&Input=" & nametemplate & "&SelectedName=h5_bg.Source" : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetImageVisibleOff&Input=" & nametemplate & "&SelectedName=a4_bg.Source" : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetImageVisibleOff&Input=" & nametemplate & "&SelectedName=a5_bg.Source" : SendHTMLtovMix(sendstring)

    End Sub

    Private Sub Showgames()
        Dim sendstring As String
        Dim nametemplate As String = "large_result.gtzip"

        sendstring = "Function=SetTextVisibleOn&Input=" & nametemplate & "&SelectedName=h4.Text" : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetTextVisibleOn&Input=" & nametemplate & "&SelectedName=h5.Text" : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetTextVisibleOn&Input=" & nametemplate & "&SelectedName=a4.Text" : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetTextVisibleOn&Input=" & nametemplate & "&SelectedName=a5.Text" : SendHTMLtovMix(sendstring)

        sendstring = "Function=SetImageVisibleOn&Input=" & nametemplate & "&SelectedName=h4_bg.Source" : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetImageVisibleOn&Input=" & nametemplate & "&SelectedName=h5_bg.Source" : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetImageVisibleOn&Input=" & nametemplate & "&SelectedName=a4_bg.Source" : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetImageVisibleOn&Input=" & nametemplate & "&SelectedName=a5_bg.Source" : SendHTMLtovMix(sendstring)

    End Sub

    Private Sub Showpoints()
        'zeigt die punkte und tennisball
        Dim sendstring As String
        Dim nametemplate As String

        For i = 2 To 5
            nametemplate = "scorebug_" + Trim(Str(i)) + "s.gtzip"
            Dim image = "C:\VMIX\tennis\graphical templates\scorebug_bg_" + Trim(Str(i)) + ".png"
            sendstring = "Function=SetImage&Input=" & nametemplate & "&SelectedName=bg" + Trim(Str(i)) + ".Source&Value=" & image : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetTextVisibleOn&Input=" & nametemplate & "&SelectedName=hpoint.Text" : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetTextVisibleOn&Input=" & nametemplate & "&SelectedName=apoint.Text" : SendHTMLtovMix(sendstring)
        Next

        nametemplate = "large_result.gtzip"
        sendstring = "Function=SetTextVisibleOn&Input=" & nametemplate & "&SelectedName=h3.Text" : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetTextVisibleOn&Input=" & nametemplate & "&SelectedName=a3.Text" : SendHTMLtovMix(sendstring)

        sendstring = "Function=SetImageVisibleOn&Input=" & nametemplate & "&SelectedName=h3_bg.Source" : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetImageVisibleOn&Input=" & nametemplate & "&SelectedName=a3_bg.Source" : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetImageVisibleOn&Input=" & nametemplate & "&SelectedName=h3_bg.Source" : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetImageVisibleOn&Input=" & nametemplate & "&SelectedName=a3_bg.Source" : SendHTMLtovMix(sendstring)

        If Tennis24_Settings.TextBoxValues(50) = 5 Then
            sendstring = "Function=SetTextVisibleOn&Input=" & nametemplate & "&SelectedName=h4.Text" : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetTextVisibleOn&Input=" & nametemplate & "&SelectedName=a4.Text" : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetTextVisibleOn&Input=" & nametemplate & "&SelectedName=h5.Text" : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetTextVisibleOn&Input=" & nametemplate & "&SelectedName=a5.Text" : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetImageVisibleOn&Input=" & nametemplate & "&SelectedName=h4_bg.Source" : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetImageVisibleOn&Input=" & nametemplate & "&SelectedName=a4_bg.Source" : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetImageVisibleOn&Input=" & nametemplate & "&SelectedName=h5_bg.Source" : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetImageVisibleOn&Input=" & nametemplate & "&SelectedName=a5_bg.Source" : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetImageVisibleOn&Input=" & nametemplate & "&SelectedName=hserve.Source" : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetImageVisibleOn&Input=" & nametemplate & "&SelectedName=aserve.Source" : SendHTMLtovMix(sendstring)
        End If

    End Sub



    Private Sub Lower1()
        Dim scorebugtitle As String = "lower_name.gtzip"
        Dim sendstring As String
        Dim PlayerName As String = If(String.IsNullOrEmpty(Tennis24_Main.HomePlayer(0)), "HOME", Tennis24_Main.HomePlayer(1) & " " & Tennis24_Main.HomePlayer(0))
        Dim country As String = If(String.IsNullOrEmpty(Tennis24_Main.HomePlayer(0)), "HOME", Tennis24_Main.HomePlayer(3))

        ' Vereinfachte Logik: Präfix nur hinzufügen wenn nicht versteckt UND Wert vorhanden
        Dim age As String = If(hidedetails OrElse String.IsNullOrEmpty(Tennis24_Main.HomePlayer(4)), " ", "Age: " & Tennis24_Main.HomePlayer(4))
        Dim height As String = If(hidedetails OrElse String.IsNullOrEmpty(Tennis24_Main.HomePlayer(5)), " ", "Height: " & Tennis24_Main.HomePlayer(5))
        Dim info1 As String = If(hidedetails OrElse String.IsNullOrEmpty(Tennis24_Main.HomePlayer(6)), " ", Tennis24_Main.HomePlayer(6))
        Dim info2 As String = If(hidedetails OrElse String.IsNullOrEmpty(Tennis24_Main.HomePlayer(7)), " ", Tennis24_Main.HomePlayer(7))
        Dim info3 As String = If(hidedetails OrElse String.IsNullOrEmpty(Tennis24_Main.HomePlayer(8)), " ", Tennis24_Main.HomePlayer(8))

        sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=name.Text&Value=" + PlayerName : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=country.Text&Value=" + country : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=age.Text&Value=" + age : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=height.Text&Value=" + height : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=info1.Text&Value=" + info1 : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=info2.Text&Value=" + info2 : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=info3.Text&Value=" + info3 : SendHTMLtovMix(sendstring)

        Dim flagInfo = GetFlagInfo(country)
        If flagInfo.Exists Then
            sendstring = "Function=SetImage&Input=" + scorebugtitle + "&SelectedName=country_flag.Source&Value=" + flagInfo.Path : SendHTMLtovMix(sendstring)
        Else
            sendstring = "Function=SetImage&Input=" + scorebugtitle + "&SelectedName=country_flag.Source&Value=C:\VMIX\tennis\flags\transparent.png" : SendHTMLtovMix(sendstring)
        End If
    End Sub

    Private Sub Lower2()
        Dim scorebugtitle As String = "lower_name.gtzip"
        Dim sendstring As String
        Dim PlayerName As String = If(String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(0)), "AWAY", Tennis24_Main.AwayPlayer(1) & " " & Tennis24_Main.AwayPlayer(0))
        Dim country As String = If(String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(0)), "AWAY", Tennis24_Main.AwayPlayer(3))

        ' Vereinfachte Logik: Präfix nur hinzufügen wenn nicht versteckt UND Wert vorhanden
        Dim age As String = If(hidedetails OrElse String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(4)), " ", "Age: " & Tennis24_Main.AwayPlayer(4))
        Dim height As String = If(hidedetails OrElse String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(5)), " ", "Height: " & Tennis24_Main.AwayPlayer(5))
        Dim info1 As String = If(hidedetails OrElse String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(6)), " ", Tennis24_Main.AwayPlayer(6))
        Dim info2 As String = If(hidedetails OrElse String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(7)), " ", Tennis24_Main.AwayPlayer(7))
        Dim info3 As String = If(hidedetails OrElse String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(8)), " ", Tennis24_Main.AwayPlayer(8))

        sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=name.Text&Value=" + PlayerName : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=country.Text&Value=" + country : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=age.Text&Value=" + age : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=height.Text&Value=" + height : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=info1.Text&Value=" + info1 : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=info2.Text&Value=" + info2 : SendHTMLtovMix(sendstring)
        sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=info3.Text&Value=" + info3 : SendHTMLtovMix(sendstring)

        Dim flagInfo = GetFlagInfo(country)
        If flagInfo.Exists Then
            sendstring = "Function=SetImage&Input=" + scorebugtitle + "&SelectedName=country_flag.Source&Value=" + flagInfo.Path : SendHTMLtovMix(sendstring)
        Else
            sendstring = "Function=SetImage&Input=" + scorebugtitle + "&SelectedName=country_flag.Source&Value=C:\VMIX\tennis\flags\transparent.png" : SendHTMLtovMix(sendstring)
        End If
    End Sub

    Private Sub Pairing(Optional specificTemplate As String = "")
        ' Liste aller match_pairing Templates
        Dim templates() As String

        If String.IsNullOrEmpty(specificTemplate) Then
            ' Alle Templates aktualisieren
            templates = {"match_pairing.gtzip", "match_pairing1.gtzip", "match_pairing2.gtzip", "match_pairing3.gtzip", "match_pairing4.gtzip"}
        Else
            ' Nur spezifisches Template aktualisieren
            templates = {specificTemplate}
        End If

        For Each scorebugtitle As String In templates
            Dim sendstring As String

            ' Home Player Daten
            Dim hPlayerName As String = If(String.IsNullOrEmpty(Tennis24_Main.HomePlayer(0)), "HOME", Tennis24_Main.HomePlayer(1) & " " & Tennis24_Main.HomePlayer(0))
            Dim hcountry As String = If(String.IsNullOrEmpty(Tennis24_Main.HomePlayer(0)), "HOME", Tennis24_Main.HomePlayer(3))

            ' Vereinfachte Logik: Präfix nur hinzufügen wenn nicht versteckt UND Wert vorhanden
            Dim hage As String = If(hidedetails OrElse String.IsNullOrEmpty(Tennis24_Main.HomePlayer(4)), "", "Age: " & Tennis24_Main.HomePlayer(4))
            Dim hheight As String = If(hidedetails OrElse String.IsNullOrEmpty(Tennis24_Main.HomePlayer(5)), "", "Height: " & Tennis24_Main.HomePlayer(5))
            Dim hdata1 As String = If(hidedetails OrElse String.IsNullOrEmpty(Tennis24_Main.HomePlayer(6)), "", Tennis24_Main.HomePlayer(6))
            Dim hdata2 As String = If(hidedetails OrElse String.IsNullOrEmpty(Tennis24_Main.HomePlayer(7)), "", Tennis24_Main.HomePlayer(7))
            Dim hdata3 As String = If(hidedetails OrElse String.IsNullOrEmpty(Tennis24_Main.HomePlayer(8)), "", Tennis24_Main.HomePlayer(8))

            sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=hname.Text&Value=" + hPlayerName : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=hcountry.Text&Value=" + hcountry : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=hage.Text&Value=" + hage : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=hheight.Text&Value=" + hheight : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=hdata1.Text&Value=" + hdata1 : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=hdata2.Text&Value=" + hdata2 : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=hdata3.Text&Value=" + hdata3 : SendHTMLtovMix(sendstring)

            Dim flagInfo = GetFlagInfo(hcountry)
            If flagInfo.Exists Then
                sendstring = "Function=SetImage&Input=" + scorebugtitle + "&SelectedName=hcountry_flag.Source&Value=" + flagInfo.Path : SendHTMLtovMix(sendstring)
            Else
                sendstring = "Function=SetImage&Input=" + scorebugtitle + "&SelectedName=hcountry_flag.Source&Value=C:\VMIX\tennis\flags\transparent.png" : SendHTMLtovMix(sendstring)
            End If

            ' Away Player Daten
            Dim aPlayerName As String = If(String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(0)), "Away", Tennis24_Main.AwayPlayer(1) & " " & Tennis24_Main.AwayPlayer(0))
            Dim acountry As String = If(String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(0)), "Away", Tennis24_Main.AwayPlayer(3))

            Dim aage As String = If(hidedetails OrElse String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(4)), "", "Age: " & Tennis24_Main.AwayPlayer(4))
            Dim aheight As String = If(hidedetails OrElse String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(5)), "", "Height: " & Tennis24_Main.AwayPlayer(5))
            Dim adata1 As String = If(hidedetails OrElse String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(6)), "", Tennis24_Main.AwayPlayer(6))
            Dim adata2 As String = If(hidedetails OrElse String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(7)), "", Tennis24_Main.AwayPlayer(7))
            Dim adata3 As String = If(hidedetails OrElse String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(8)), "", Tennis24_Main.AwayPlayer(8))

            sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=aname.Text&Value=" + aPlayerName : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=acountry.Text&Value=" + acountry : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=aage.Text&Value=" + aage : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=aheight.Text&Value=" + aheight : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=adata1.Text&Value=" + adata1 : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=adata2.Text&Value=" + adata2 : SendHTMLtovMix(sendstring)
            sendstring = "Function=SetText&Input=" + scorebugtitle + "&SelectedName=adata3.Text&Value=" + adata3 : SendHTMLtovMix(sendstring)

            flagInfo = GetFlagInfo(acountry)
            If flagInfo.Exists Then
                sendstring = "Function=SetImage&Input=" + scorebugtitle + "&SelectedName=acountry_flag.Source&Value=" + flagInfo.Path : SendHTMLtovMix(sendstring)
            Else
                sendstring = "Function=SetImage&Input=" + scorebugtitle + "&SelectedName=acountry_flag.Source&Value=C:\VMIX\tennis\flags\transparent.png" : SendHTMLtovMix(sendstring)
            End If
        Next
    End Sub


    Private Sub Btn_Name_Home_Click(sender As Object, e As EventArgs) Handles Btn_Name_Home.Click
        'blendet spielername1 ein und aus
        Dim sendstring As String
        Dim nametemplate As String = "lower_name.gtzip"

        Dim homePlayerName As String = If(String.IsNullOrEmpty(Tennis24_Main.HomePlayer(0)), "HOME", Tennis24_Main.HomePlayer(0))
        Dim awayPlayerName As String = If(String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(0)), "AWAY", Tennis24_Main.AwayPlayer(0))
        'Dim awayPlayerName As String = If(String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(0)), "AWAY", Tennis24_Main.AwayPlayer(0))
        Dim Playername = "lower" & vbNewLine & homePlayerName

        ' Reset other toggles first
        ResetOtherOverlayToggles("home")
        lower1toggleStatus = Not lower1toggleStatus

        If lower1toggleStatus Then
            Lower1()
            sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "In&Input=" + nametemplate + "&Mix=0"
            Btn_Name_Home.BackColor = Color.Red
            Btn_Name_Home.Text = Playername
        Else
            sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "Out&Input=" + nametemplate + "&Mix=0"
            Btn_Name_Home.BackColor = SystemColors.ButtonHighlight
            Btn_Name_Home.Text = Playername
        End If

        SendHTMLtovMix(sendstring)
    End Sub

    Private Sub Btn_Name_Away_Click(sender As Object, e As EventArgs) Handles Btn_Name_Away.Click
        'blendet spielername2 ein und aus
        Dim sendstring As String
        Dim nametemplate As String = "lower_name.gtzip"

        Dim awayPlayerName As String = If(String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(0)), "AWAY", Tennis24_Main.AwayPlayer(0))
        Dim Playername = "lower" & vbNewLine & awayPlayerName

        ' Reset other toggles first
        ResetOtherOverlayToggles("away")

        lower2toggleStatus = Not lower2toggleStatus

        If lower2toggleStatus Then
            Lower2()
            sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "In&Input=" + nametemplate + "&Mix=0"
            Btn_Name_Away.BackColor = Color.Red
            Btn_Name_Away.Text = Playername
        Else
            sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "Out&Input=" + nametemplate + "&Mix=0"
            Btn_Name_Away.BackColor = SystemColors.ButtonHighlight
            Btn_Name_Away.Text = Playername
        End If

        SendHTMLtovMix(sendstring)
    End Sub

    Private Sub Btn_Title_Click(sender As Object, e As EventArgs) Handles Btn_Title.Click
        'blendet Titel ein und aus
        Dim sendstring As String
        Dim nametemplate As String = "title.gtzip"

        Dim buttonname = "Title"

        ' Reset other toggles first
        ResetOtherOverlayToggles("Title")

        sendstring = "Function=SetText&Input=" + nametemplate + "&SelectedName=TextBlock1.Text&Value=" + WebUtility.UrlEncode(Tennis24_Settings.TextBoxValues(1))
        SendHTMLtovMix(sendstring)
        sendstring = "Function=SetText&Input=" + nametemplate + "&SelectedName=TextBlock2.Text&Value=" + WebUtility.UrlEncode(Tennis24_Settings.TextBoxValues(2))
        SendHTMLtovMix(sendstring)

        titleToggleStatus = Not titleToggleStatus

        If titleToggleStatus Then
            sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "In&Input=" + nametemplate + "&Mix=0"
            Btn_Title.BackColor = Color.Red
            Btn_Title.Text = buttonname
        Else
            sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "Out&Input=" + nametemplate + "&Mix=0"
            Btn_Title.BackColor = SystemColors.ButtonHighlight
            Btn_Title.Text = buttonname
        End If

        SendHTMLtovMix(sendstring)
    End Sub

    Private Sub Btn_matchpairing_Click(sender As Object, e As EventArgs) Handles Btn_matchpairing.Click
        'blendet paarung ein und aus
        Pairing()

        Dim sendstring As String
        Dim nametemplate As String = "match_pairing.gtzip"

        Dim buttonname = "match pairing"

        ' Reset other toggles first
        ResetOtherOverlayToggles("matchpairing")

        matchpairingToggleStatus = Not matchpairingToggleStatus

        If matchpairingToggleStatus Then
            sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "In&Input=" + nametemplate + "&Mix=0"
            Btn_matchpairing.BackColor = Color.Red
            Btn_matchpairing.Text = buttonname
        Else
            sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "Out&Input=" + nametemplate + "&Mix=0"
            Btn_matchpairing.BackColor = SystemColors.ButtonHighlight
            Btn_matchpairing.Text = buttonname
        End If

        SendHTMLtovMix(sendstring)
    End Sub



    ' Method to reset all overlay toggles except the specified one
    Private Sub ResetOtherOverlayToggles(excludeButton As String)
        ' Liste aller buttons und deren status und text
        Dim buttons() = {
        ("home", lower1toggleStatus, Btn_Name_Home, "lower" & vbNewLine & If(String.IsNullOrEmpty(Tennis24_Main.HomePlayer(0)), "HOME", Tennis24_Main.HomePlayer(0)), "lower_name.gtzip"),
        ("away", lower2toggleStatus, Btn_Name_Away, "lower" & vbNewLine & If(String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(0)), "AWAY", Tennis24_Main.AwayPlayer(0)), "lower_name.gtzip"),
        ("largeresult", largeResulttoggleStatus, Btn_LargeResult, "Large Result OFF", "large_result.gtzip"),
        ("title", titleToggleStatus, Btn_Title, "Title", "title.gtzip"),
        ("matchpairing", matchpairingToggleStatus, Btn_matchpairing, "match pairing", "matchpairing.gtzip"),
        ("matchpairing1", matchpairing1ToggleStatus, Btn_matchpairing1, "Match Pairing 1", "matchpairing1.gtzip"),
        ("matchpairing2", matchpairing2ToggleStatus, Btn_matchpairing2, "Match Pairing 2", "matchpairing2.gtzip"),
        ("matchpairing3", matchpairing3ToggleStatus, Btn_matchpairing3, "Match Pairing 3", "matchpairing3.gtzip"),
        ("matchpairing4", matchpairing4ToggleStatus, Btn_matchpairing4, "Match Pairing 4", "matchpairing4.gtzip"),
        ("info1", info1ToggleStatus, Btn_info1, "Info1", "info1.gtzip"),
        ("info2", info2ToggleStatus, Btn_info2, "Info2", "info2.gtzip"),
        ("info3", info3ToggleStatus, Btn_info3, "Info3", "info3.gtzip"),
        ("info4", info4ToggleStatus, Btn_info4, "Info4", "info4.gtzip"),
        ("sponsor1", sponsor1ToggleStatus, Btn_sponsor1, "Sponsor1", "sponsor1.gtzip"),
        ("sponsor2", sponsor2ToggleStatus, Btn_sponsor2, "Sponsor2", "sponsor2.gtzip"),
        ("freename1", freename1ToggleStatus, Btn_freename1, "Free Name 1", "name.gtzip"),
        ("freename2", freename2ToggleStatus, Btn_freename2, "Free Name 2", "name.gtzip"),
        ("freename3", freename3ToggleStatus, Btn_freename3, "Free Name 3", "name.gtzip"),
        ("freename4", freename4ToggleStatus, Btn_freename4, "Free Name 4", "name.gtzip"),
        ("freename5", freename5ToggleStatus, Btn_freename5, "Free Name 5", "name.gtzip"),
        ("ref1", ref1ToggleStatus, Btn_ref1, "Referee 1", "name.gtzip"),
        ("ref2", ref2ToggleStatus, Btn_ref2, "Referee 2", "name.gtzip"),
        ("com1", com1ToggleStatus, Btn_com1, "Commentator 1", "name.gtzip"),
        ("com2", com2ToggleStatus, Btn_com2, "Commentator 2", "name.gtzip")
    }

        ' alle anderen ausser dem ausgenommenen zurücksetzen
        For Each btn In buttons
            If btn.Item1.ToLower() <> excludeButton.ToLower() AndAlso btn.Item2 Then
                ' Status zurücksetzen
                Select Case btn.Item1
                    Case "home" : lower1toggleStatus = False
                    Case "away" : lower2toggleStatus = False
                    Case "largeresult" : largeResulttoggleStatus = False
                    Case "title" : titleToggleStatus = False
                    Case "matchpairing" : matchpairingToggleStatus = False
                    Case "matchpairing1" : matchpairing1ToggleStatus = False
                    Case "matchpairing2" : matchpairing2ToggleStatus = False
                    Case "matchpairing3" : matchpairing3ToggleStatus = False
                    Case "matchpairing4" : matchpairing4ToggleStatus = False
                    Case "info1" : info1ToggleStatus = False
                    Case "info2" : info2ToggleStatus = False
                    Case "info3" : info3ToggleStatus = False
                    Case "info4" : info4ToggleStatus = False
                    Case "sponsor1" : sponsor1ToggleStatus = False
                    Case "sponsor2" : sponsor2ToggleStatus = False
                    Case "freename1" : freename1ToggleStatus = False
                    Case "freename2" : freename2ToggleStatus = False
                    Case "freename3" : freename3ToggleStatus = False
                    Case "freename4" : freename4ToggleStatus = False
                    Case "freename5" : freename5ToggleStatus = False
                    Case "ref1" : ref1ToggleStatus = False
                    Case "ref2" : ref2ToggleStatus = False
                    Case "com1" : com1ToggleStatus = False
                    Case "com2" : com2ToggleStatus = False
                End Select

                ' Button zurücksetzen
                btn.Item3.BackColor = SystemColors.ButtonHighlight
                btn.Item3.Text = btn.Item4

                ' Overlay ausblenden
                Dim sendstring As String
                sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "Off&Input=" + btn.Item5 + "&Mix=0"
                SendHTMLtovMix(sendstring)
            End If
        Next
    End Sub

    Private Sub Btn_info1_Click(sender As Object, e As EventArgs) Handles Btn_info1.Click
        'blendet Titel ein und aus
        Dim sendstring As String
        Dim nametemplate As String = "info1.gtzip"

        ' Reset other toggles first
        ResetOtherOverlayToggles("info1")

        info1ToggleStatus = Not info1ToggleStatus

        If info1ToggleStatus Then
            sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "In&Input=" + nametemplate + "&Mix=0"
            Btn_info1.BackColor = Color.Red
        Else
            sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "Out&Input=" + nametemplate + "&Mix=0"
            Btn_info1.BackColor = SystemColors.ButtonHighlight
        End If

        SendHTMLtovMix(sendstring)
    End Sub

    Private Sub Btn_info2_Click(sender As Object, e As EventArgs) Handles Btn_info2.Click
        'blendet Titel ein und aus
        Dim sendstring As String
        Dim nametemplate As String = "info2.gtzip"

        ' Reset other toggles first
        ResetOtherOverlayToggles("info2")

        info2ToggleStatus = Not info2ToggleStatus

        If info2ToggleStatus Then
            sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "In&Input=" + nametemplate + "&Mix=0"
            Btn_info2.BackColor = Color.Red
        Else
            sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "Out&Input=" + nametemplate + "&Mix=0"
            Btn_info2.BackColor = SystemColors.ButtonHighlight
        End If

        SendHTMLtovMix(sendstring)
    End Sub

    Private Sub Btn_info3_Click(sender As Object, e As EventArgs) Handles Btn_info3.Click
        'blendet Titel ein und aus
        Dim sendstring As String
        Dim nametemplate As String = "info3.gtzip"

        ' Reset other toggles first
        ResetOtherOverlayToggles("info3")

        info3ToggleStatus = Not info3ToggleStatus

        If info3ToggleStatus Then
            sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "In&Input=" + nametemplate + "&Mix=0"
            Btn_info3.BackColor = Color.Red
        Else
            sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "Out&Input=" + nametemplate + "&Mix=0"
            Btn_info3.BackColor = SystemColors.ButtonHighlight
        End If

        SendHTMLtovMix(sendstring)
    End Sub

    Private Sub Btn_info4_Click(sender As Object, e As EventArgs) Handles Btn_info4.Click
        'blendet Titel ein und aus
        Dim sendstring As String
        Dim nametemplate As String = "info4.gtzip"

        ' Reset other toggles first
        ResetOtherOverlayToggles("info4")

        info4ToggleStatus = Not info4ToggleStatus

        If info4ToggleStatus Then
            sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "In&Input=" + nametemplate + "&Mix=0"
            Btn_info4.BackColor = Color.Red
        Else
            sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "Out&Input=" + nametemplate + "&Mix=0"
            Btn_info4.BackColor = SystemColors.ButtonHighlight
        End If

        SendHTMLtovMix(sendstring)
    End Sub

    Private Sub Btn_matchpairing1_Click(sender As Object, e As EventArgs) Handles Btn_matchpairing1.Click, Btn_matchpairing2.Click, Btn_matchpairing3.Click, Btn_matchpairing4.Click
        Pairing()

        Dim clickedButton As Button = DirectCast(sender, Button)
        Dim sendstring As String
        Dim nametemplate As String
        Dim excludeButtonName As String

        ' Bestimme welcher Button geklickt wurde
        Select Case clickedButton.Name
            Case "Btn_matchpairing1"
                nametemplate = "match_pairing1.gtzip"
                excludeButtonName = "matchpairing1"

                ' Reset other toggles first
                ResetOtherOverlayToggles(excludeButtonName)
                matchpairing1ToggleStatus = Not matchpairing1ToggleStatus

                If matchpairing1ToggleStatus Then
                    sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "In&Input=" + nametemplate + "&Mix=0"
                    clickedButton.BackColor = Color.Red
                Else
                    sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "Out&Input=" + nametemplate + "&Mix=0"
                    clickedButton.BackColor = SystemColors.ButtonHighlight
                End If

            Case "Btn_matchpairing2"
                nametemplate = "match_pairing2.gtzip"
                excludeButtonName = "matchpairing2"

                ' Reset other toggles first
                ResetOtherOverlayToggles(excludeButtonName)
                matchpairing2ToggleStatus = Not matchpairing2ToggleStatus

                If matchpairing2ToggleStatus Then
                    sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "In&Input=" + nametemplate + "&Mix=0"
                    clickedButton.BackColor = Color.Red
                Else
                    sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "Out&Input=" + nametemplate + "&Mix=0"
                    clickedButton.BackColor = SystemColors.ButtonHighlight
                End If

            Case "Btn_matchpairing3"
                nametemplate = "match_pairing3.gtzip"
                excludeButtonName = "matchpairing3"

                ' Reset other toggles first
                ResetOtherOverlayToggles(excludeButtonName)
                matchpairing3ToggleStatus = Not matchpairing3ToggleStatus

                If matchpairing3ToggleStatus Then
                    sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "In&Input=" + nametemplate + "&Mix=0"
                    clickedButton.BackColor = Color.Red
                Else
                    sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "Out&Input=" + nametemplate + "&Mix=0"
                    clickedButton.BackColor = SystemColors.ButtonHighlight
                End If

            Case "Btn_matchpairing4"
                nametemplate = "match_pairing4.gtzip"
                excludeButtonName = "matchpairing4"

                ' Reset other toggles first
                ResetOtherOverlayToggles(excludeButtonName)
                matchpairing4ToggleStatus = Not matchpairing4ToggleStatus

                If matchpairing4ToggleStatus Then
                    sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "In&Input=" + nametemplate + "&Mix=0"
                    clickedButton.BackColor = Color.Red
                Else
                    sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "Out&Input=" + nametemplate + "&Mix=0"
                    clickedButton.BackColor = SystemColors.ButtonHighlight
                End If

            Case Else
                Return ' Fallback, sollte nicht auftreten
        End Select

        SendHTMLtovMix(sendstring)
    End Sub

    Private Sub CheckBox_noTiebreak_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox_noTiebreak.CheckedChanged
        noTiebreakMode = CheckBox_noTiebreak.Checked

        ' Visuelles Feedback
        If noTiebreakMode Then
            CheckBox_noTiebreak.Text = "No Tiebreak (Advantage Set)"
        Else
            CheckBox_noTiebreak.Text = "Normal (with Tiebreak)"
        End If

        ' DataGridView aktualisieren
        UpdateDataGridView()
    End Sub

    Private Sub Btn_sponsor1_Click(sender As Object, e As EventArgs) Handles Btn_sponsor1.Click
        'blendet sponsor1 ein und aus
        Dim sendstring As String
        Dim nametemplate As String = "sponsor1.gtzip"

        ' Reset other toggles first
        'ResetOtherOverlayToggles("sponsor1")

        sponsor1ToggleStatus = Not sponsor1ToggleStatus

        If sponsor1ToggleStatus Then
            sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(3) + "In&Input=" + nametemplate + "&Mix=0"
            Btn_sponsor1.BackColor = Color.Red
            Btn_sponsor2.BackColor = SystemColors.ButtonHighlight
            sponsor1ToggleStatus = True
            sponsor2ToggleStatus = False
        Else
            sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(3) + "Out&Input=" + nametemplate + "&Mix=0"
            Btn_sponsor1.BackColor = SystemColors.ButtonHighlight
            sponsor1ToggleStatus = False
            sponsor2ToggleStatus = False
        End If

        SendHTMLtovMix(sendstring)
    End Sub

    Private Sub Btn_sponsor2_Click(sender As Object, e As EventArgs) Handles Btn_sponsor2.Click
        'blendet sponsor2 ein und aus
        Dim sendstring As String
        Dim nametemplate As String = "sponsor2.gtzip"

        ' Reset other toggles first
        'ResetOtherOverlayToggles("sponsor2")

        sponsor2ToggleStatus = Not sponsor2ToggleStatus

        If sponsor2ToggleStatus Then
            sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(3) + "In&Input=" + nametemplate + "&Mix=0"
            Btn_sponsor2.BackColor = Color.Red
            Btn_sponsor1.BackColor = SystemColors.ButtonHighlight
            sponsor1ToggleStatus = False
            sponsor2ToggleStatus = True
        Else
            sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(3) + "Out&Input=" + nametemplate + "&Mix=0"
            Btn_sponsor2.BackColor = SystemColors.ButtonHighlight
            sponsor1ToggleStatus = False
            sponsor2ToggleStatus = False
        End If

        SendHTMLtovMix(sendstring)
    End Sub

    Private Sub ResetAllOverlayButtons()
        ' Alle Toggle-Status zurücksetzen
        scorebugtoggleStatus = False
        lower1toggleStatus = False
        lower2toggleStatus = False
        largeResulttoggleStatus = False
        titleToggleStatus = False
        matchpairingToggleStatus = False
        matchpairing1ToggleStatus = False
        matchpairing2ToggleStatus = False
        matchpairing3ToggleStatus = False
        matchpairing4ToggleStatus = False
        info1ToggleStatus = False
        info2ToggleStatus = False
        info3ToggleStatus = False
        info4ToggleStatus = False
        sponsor1ToggleStatus = False
        sponsor2ToggleStatus = False


        ' Name-Button Toggle-Status zurücksetzen
        freename1ToggleStatus = False
        freename2ToggleStatus = False
        freename3ToggleStatus = False
        freename4ToggleStatus = False
        freename5ToggleStatus = False
        ref1ToggleStatus = False
        ref2ToggleStatus = False
        com1ToggleStatus = False
        com2ToggleStatus = False

        ' Alle Buttons visuell zurücksetzen
        Btn_Scorebug.BackColor = SystemColors.ButtonHighlight
        Btn_Scorebug.Text = "Scorebug OFF"
        
        ' Name-Buttons zurücksetzen
        Btn_freename1.BackColor = SystemColors.ButtonHighlight
        Btn_freename2.BackColor = SystemColors.ButtonHighlight
        Btn_freename3.BackColor = SystemColors.ButtonHighlight
        Btn_freename4.BackColor = SystemColors.ButtonHighlight
        Btn_freename5.BackColor = SystemColors.ButtonHighlight
        Btn_ref1.BackColor = SystemColors.ButtonHighlight
        Btn_ref2.BackColor = SystemColors.ButtonHighlight
        Btn_com1.BackColor = SystemColors.ButtonHighlight
        Btn_com2.BackColor = SystemColors.ButtonHighlight
        Btn_info1.BackColor = SystemColors.ButtonHighlight
        Btn_info2.BackColor = SystemColors.ButtonHighlight
        Btn_info3.BackColor = SystemColors.ButtonHighlight
        Btn_info4.BackColor = SystemColors.ButtonHighlight

        ' Alle Overlays ausblenden
        Dim overlayCommands() As String = {
        "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "Off&Input=lower_name.gtzip&Mix=0",
        "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "Off&Input=large_result.gtzip&Mix=0",
        "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "Off&Input=title.gtzip&Mix=0",
        "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "Off&Input=match_pairing.gtzip&Mix=0",
        "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "Off&Input=match_pairing1.gtzip&Mix=0",
        "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "Off&Input=match_pairing2.gtzip&Mix=0",
        "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "Off&Input=match_pairing3.gtzip&Mix=0",
        "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "Off&Input=match_pairing4.gtzip&Mix=0",
        "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "Off&Input=info1.gtzip&Mix=0",
        "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "Off&Input=info2.gtzip&Mix=0",
        "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(3) + "Off&Input=sponsor1.gtzip&Mix=0",
        "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(3) + "Off&Input=sponsor2.gtzip&Mix=0",
        "Function=OverlayInput1Out"  ' Für Name-Overlays
    }

        ' Alle Overlay-Befehle senden
        For Each cmd In overlayCommands
            SendHTMLtovMix(cmd)  ' KORRIGIERT: cmd statt Command
        Next
    End Sub

    Private Sub Btn_clearLayers_Click(sender As Object, e As EventArgs) Handles Btn_clearLayers.Click
        ' Alle Overlays zurücksetzen
        ResetAllOverlayButtons()
        Dim sendstring As String
        sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(1) + "Off" : SendHTMLtovMix(sendstring)
        sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(2) + "Off" : SendHTMLtovMix(sendstring)
        sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(3) + "Off" : SendHTMLtovMix(sendstring)

    End Sub

    Private Sub CheckBox_hidedetails_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox_hidedetails.CheckedChanged
        My.Settings.hidedetails = CheckBox_hidedetails.Checked
        My.Settings.Save()
        hidedetails = CheckBox_hidedetails.Checked
    End Sub

    Private Sub Btn_Name1_Click(sender As Object, e As EventArgs) Handles Btn_freename1.Click, Btn_freename2.Click, Btn_freename3.Click, Btn_freename4.Click, Btn_freename5.Click, Btn_ref1.Click, Btn_ref2.Click, Btn_com1.Click, Btn_com2.Click
        ' Determine which button was clicked
        Dim button As Button = DirectCast(sender, Button)
        Dim nametemplate As String = "name.gtzip"
        Dim sendstring As String
        Dim excludeButtonName
        Dim currentToggleStatus
        Dim com1Name As String = If(String.IsNullOrEmpty(Tennis24_Settings.TextBox22.Text.Trim()), "Commentator 1", Tennis24_Settings.TextBox22.Text.Trim())
        Dim com2Name As String = Tennis24_Settings.TextBox23.Text.Trim()

        ' Initialize TextBox and Toggle Status based on the button clicked
        Dim textBox As TextBox
        Select Case button.Name
            Case "Btn_freename1"
                textBox = Tennis24_Settings.TextBox4
                excludeButtonName = "freename1"
                currentToggleStatus = freename1ToggleStatus
            Case "Btn_freename2"
                textBox = Tennis24_Settings.TextBox5
                excludeButtonName = "freename2"
                currentToggleStatus = freename2ToggleStatus
            Case "Btn_freename3"
                textBox = Tennis24_Settings.TextBox6
                excludeButtonName = "freename3"
                currentToggleStatus = freename3ToggleStatus
            Case "Btn_freename4"
                textBox = Tennis24_Settings.TextBox7
                excludeButtonName = "freename4"
                currentToggleStatus = freename4ToggleStatus
            Case "Btn_freename5"
                textBox = Tennis24_Settings.TextBox8
                excludeButtonName = "freename5"
                currentToggleStatus = freename5ToggleStatus
            Case "Btn_ref1"
                textBox = Tennis24_Settings.TextBox20
                excludeButtonName = "ref1"
                currentToggleStatus = ref1ToggleStatus
            Case "Btn_ref2"
                textBox = Tennis24_Settings.TextBox21
                excludeButtonName = "ref2"
                currentToggleStatus = ref2ToggleStatus

            Case "Btn_com1"
                textBox = Tennis24_Settings.TextBox22
                excludeButtonName = "com1"
                currentToggleStatus = com1ToggleStatus
                ' SPEZIELLE BEHANDLUNG für Btn_com1: Template abhängig von Commentator2 setzen
                If String.IsNullOrEmpty(Tennis24_Settings.TextBox23.Text.Trim()) Then
                    nametemplate = "name.gtzip"  ' Nur ein Kommentator
                Else
                    nametemplate = "name2.gtzip" ' Beide Kommentatoren
                End If

            Case "Btn_com2"
                textBox = Tennis24_Settings.TextBox23
                excludeButtonName = "com2"
                currentToggleStatus = com2ToggleStatus

            Case Else
                MessageBox.Show("No matching button found", "Error24", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Exit Sub
        End Select

        ' Reset other overlays first
        ResetOtherOverlayToggles(excludeButtonName)

        ' Toggle the status
        currentToggleStatus = Not currentToggleStatus

        ' Update the actual toggle status variable
        Select Case excludeButtonName
            Case "freename1" : freename1ToggleStatus = currentToggleStatus
            Case "freename2" : freename2ToggleStatus = currentToggleStatus
            Case "freename3" : freename3ToggleStatus = currentToggleStatus
            Case "freename4" : freename4ToggleStatus = currentToggleStatus
            Case "freename5" : freename5ToggleStatus = currentToggleStatus
            Case "ref1" : ref1ToggleStatus = currentToggleStatus
            Case "ref2" : ref2ToggleStatus = currentToggleStatus
            Case "com1" : com1ToggleStatus = currentToggleStatus
            Case "com2" : com2ToggleStatus = currentToggleStatus
        End Select

        If currentToggleStatus Then
            ' SPEZIELLE BEHANDLUNG für Btn_com1 mit zwei Kommentatoren
            If button.Name = "Btn_com1" AndAlso nametemplate = "name2.gtzip" Then
                ' Beide Kommentatoren anzeigen - KORREKTE ZUORDNUNG

                ' Commentator1 (TextBox22) aufteilen
                Dim commentator1Text As String = Tennis24_Settings.TextBox22.Text.Trim()
                Dim com1Parts() As String
                Dim com1Line1 As String
                Dim com1Line2 As String

                If commentator1Text.Contains(",") Then
                    com1Parts = commentator1Text.Split(New Char() {","c}, 2)
                    com1Line1 = com1Parts(0).Trim()
                    com1Line2 = com1Parts(1).Trim()
                Else
                    com1Line1 = commentator1Text
                    com1Line2 = ""
                End If

                ' Commentator2 (TextBox23) aufteilen
                Dim commentator2Text As String = Tennis24_Settings.TextBox23.Text.Trim()
                Dim com2Parts() As String
                Dim com2Line1 As String
                Dim com2Line2 As String

                If commentator2Text.Contains(",") Then
                    com2Parts = commentator2Text.Split(New Char() {","c}, 2)
                    com2Line1 = com2Parts(0).Trim()
                    com2Line2 = com2Parts(1).Trim()
                Else
                    com2Line1 = commentator2Text
                    com2Line2 = ""
                End If

                ' KORREKTE ZUORDNUNG für name2.gtzip:
                ' name1 = Commentator1 (vor dem Komma von TextBox22)
                sendstring = "Function=SetText&Input=" + nametemplate + "&SelectedName=name1.Text&Value=" + WebUtility.UrlEncode(com1Line1)
                SendHTMLtovMix(sendstring)

                ' name2 = Text nach dem Komma von TextBox22
                sendstring = "Function=SetText&Input=" + nametemplate + "&SelectedName=name2.Text&Value=" + WebUtility.UrlEncode(com1Line2)
                SendHTMLtovMix(sendstring)

                ' name3 = Commentator2 (vor dem Komma von TextBox23)
                sendstring = "Function=SetText&Input=" + nametemplate + "&SelectedName=name3.Text&Value=" + WebUtility.UrlEncode(com2Line1)
                SendHTMLtovMix(sendstring)

                ' name4 = Text nach dem Komma von TextBox23
                sendstring = "Function=SetText&Input=" + nametemplate + "&SelectedName=name4.Text&Value=" + WebUtility.UrlEncode(com2Line2)
                SendHTMLtovMix(sendstring)

            Else
                ' Standard-Verhalten für alle anderen Buttons (inklusive com1 mit nur einem Kommentator)
                Dim fullText As String = textBox.Text.Trim()
                Dim line1 As String
                Dim line2 As String

                ' Check if the text contains a comma
                If fullText.Contains(",") Then
                    ' Split the text at the comma
                    Dim parts() As String = fullText.Split(New Char() {","c}, 2)
                    line1 = parts(0).Trim()
                    line2 = parts(1).Trim()
                Else
                    ' If no comma, assign full text to line1 and set line2 to an empty string
                    line1 = fullText
                    line2 = " "
                End If

                ' Set the text in the template
                sendstring = "Function=SetText&Input=" + nametemplate + "&SelectedName=name1.Text&Value=" + WebUtility.UrlEncode(line1)
                SendHTMLtovMix(sendstring)

                If line2 <> String.Empty Then
                    sendstring = "Function=SetText&Input=" + nametemplate + "&SelectedName=name2.Text&Value=" + WebUtility.UrlEncode(line2)
                    SendHTMLtovMix(sendstring)
                End If
            End If

            ' Show the graphics and set the clicked button's background to red
            sendstring = "Function=OverlayInput1IN&Input=" + nametemplate
            SendHTMLtovMix(sendstring)
            button.BackColor = Color.Red
        Else
            ' Hide the graphics and reset button color
            sendstring = "Function=OverlayInput1Out"
            SendHTMLtovMix(sendstring)
            button.BackColor = SystemColors.ButtonHighlight
        End If
    End Sub

    Private Sub Lbl_Winner_Click(sender As Object, e As EventArgs) Handles Lbl_Winner.Click
        Lbl_Winner.Visible = False
    End Sub


End Class