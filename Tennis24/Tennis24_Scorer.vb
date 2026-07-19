Imports System.Net

Public Class Tennis24_Scorer

    'zeigt keine Spielerdetails an wie alter etc.
    Private hidedetails As Boolean = False

    ' Toggle-Status für Scorebug- und Sponsor-Buttons (eigene vMix-Layer, daher nicht Teil der
    ' gemeinsam ausschliessenden Overlay-Registry weiter unten)
    Private scorebugtoggleStatus As Boolean = False
    Private sponsor1ToggleStatus As Boolean = False
    Private sponsor2ToggleStatus As Boolean = False

    ' "Freeze Set" (Settings-Checkbox2): bleibt bei Satzende auf dem Scorebug des gerade
    ' beendeten Satzes stehen (mit gelb markiertem Endstand), statt sofort auf den neuen Satz
    ' zu springen. displayedScorebugSet ist der Satz, dessen Vorlage aktuell "aktiv" ist/beim
    ' nächsten Einschalten gezeigt würde - normalerweise identisch mit currentSet, bleibt bei
    ' aktivem Freeze Set aber auf dem alten Satz stehen, bis der Bediener den Scorebug manuell
    ' ausschaltet. freezeSetAdvanceTimer schaltet dann nach der Out-Animation (~1s) auf den
    ' neuen Satz weiter.
    Private freezeSetEnabled As Boolean = False
    Private displayedScorebugSet As Integer = 1
    Private WithEvents freezeSetAdvanceTimer As New Timer()

    ' Separate Statistik-Form; Nothing/IsDisposed solange sie nicht geöffnet ist.
    Private statisticsForm As Tennis24_Statistics

    ' Live-JSON-Datenquelle (Settings-Checkbox3): stellt den aktuellen Spielstand
    ' geräteunabhängig per HTTP bereit, unabhängig von vMix. Läuft nur, wenn aktiviert.
    Private ReadOnly jsonServer As New TennisJsonServer()
    Private jsonServerRunningPort As Integer = 0

    ' Ein Eintrag pro Overlay-Button, der sich gegenseitig mit den anderen ausschliesst
    ' (immer nur einer dieser Layer-1-Overlays gleichzeitig sichtbar). Ersetzt die vorher
    ' 22 einzelnen ...ToggleStatus-Variablen plus die von Hand gepflegten Select-Case-Listen
    ' in ResetOtherOverlayToggles/ResetAllOverlayButtons.
    Private Class OverlayToggle
        Public Property Key As String
        Public Property Button As Button
        Public Property Template As String
        Public Property ComboIndex As Integer
        Public Property Status As Boolean
        ' Text, der beim (Fremd-)Zurücksetzen auf den Button geschrieben wird; Nothing = Text unangetastet lassen
        Public Property ResetText As Func(Of String)
    End Class

    Private overlayToggles As List(Of OverlayToggle)


    ' Reine Zähllogik (Punkte/Spiele/Sätze/Tiebreak/Statistik/Undo-Stack) lebt in einer
    ' eigenen, UI-freien Klasse (TennisMatchEngine.vb). Die folgenden Properties spiegeln
    ' die frühere Feld-Namen 1:1 auf diese Engine, damit der restliche Code in dieser
    ' Datei (UpdateScore, CheckForMatchEnd, UpdateDataGridView, ...) unverändert bleiben kann.
    Private ReadOnly match As New TennisMatchEngine()

    Private Property isTiebreak As Boolean
        Get
            Return match.IsTiebreak
        End Get
        Set(value As Boolean)
            match.IsTiebreak = value
        End Set
    End Property

    Private Property homePoints As Integer
        Get
            Return match.HomePoints
        End Get
        Set(value As Integer)
            match.HomePoints = value
        End Set
    End Property

    Private Property awayPoints As Integer
        Get
            Return match.AwayPoints
        End Get
        Set(value As Integer)
            match.AwayPoints = value
        End Set
    End Property

    Private Property homeGames As Integer
        Get
            Return match.HomeGames
        End Get
        Set(value As Integer)
            match.HomeGames = value
        End Set
    End Property

    Private Property awayGames As Integer
        Get
            Return match.AwayGames
        End Get
        Set(value As Integer)
            match.AwayGames = value
        End Set
    End Property

    Private Property homeSets As Integer
        Get
            Return match.HomeSets
        End Get
        Set(value As Integer)
            match.HomeSets = value
        End Set
    End Property

    Private Property awaySets As Integer
        Get
            Return match.AwaySets
        End Get
        Set(value As Integer)
            match.AwaySets = value
        End Set
    End Property

    Private Property currentSet As Integer
        Get
            Return match.CurrentSet
        End Get
        Set(value As Integer)
            match.CurrentSet = value
        End Set
    End Property

    ' Variable für Match-Ende Status
    Private Property isMatchFinished As Boolean
        Get
            Return match.IsMatchFinished
        End Get
        Set(value As Boolean)
            match.IsMatchFinished = value
        End Set
    End Property

    ' Variable für No-Tiebreak-Regel
    Private Property noTiebreakMode As Boolean
        Get
            Return match.NoTiebreakMode
        End Get
        Set(value As Boolean)
            match.NoTiebreakMode = value
        End Set
    End Property

    ' True, wenn gerade der Match-Tiebreak (statt eines regulären 3. Satzes bei Best of 3
    ' und 1:1 Sätzen) gespielt wird - abgeleitet aus den Settings + aktuellem Satzstand.
    Private ReadOnly Property isMatchTiebreakSet As Boolean
        Get
            Return match.IsMatchTiebreakSet()
        End Get
    End Property

    ' Statistik-Variablen
    Private Property homeTotalPoints As Integer
        Get
            Return match.HomeTotalPoints
        End Get
        Set(value As Integer)
            match.HomeTotalPoints = value
        End Set
    End Property

    Private Property awayTotalPoints As Integer
        Get
            Return match.AwayTotalPoints
        End Get
        Set(value As Integer)
            match.AwayTotalPoints = value
        End Set
    End Property

    Private Property homeBreaks As Integer
        Get
            Return match.HomeBreaks
        End Get
        Set(value As Integer)
            match.HomeBreaks = value
        End Set
    End Property

    Private Property awayBreaks As Integer
        Get
            Return match.AwayBreaks
        End Get
        Set(value As Integer)
            match.AwayBreaks = value
        End Set
    End Property

    Private Property homeServiceGamesWon As Integer
        Get
            Return match.HomeServiceGamesWon
        End Get
        Set(value As Integer)
            match.HomeServiceGamesWon = value
        End Set
    End Property

    Private Property awayServiceGamesWon As Integer
        Get
            Return match.AwayServiceGamesWon
        End Get
        Set(value As Integer)
            match.AwayServiceGamesWon = value
        End Set
    End Property

    Private Property isHomeServing As Boolean
        Get
            Return match.IsHomeServing
        End Get
        Set(value As Boolean)
            match.IsHomeServing = value
        End Set
    End Property

    Private Property homeTiebreaksWon As Integer
        Get
            Return match.HomeTiebreaksWon
        End Get
        Set(value As Integer)
            match.HomeTiebreaksWon = value
        End Set
    End Property

    Private Property awayTiebreaksWon As Integer
        Get
            Return match.AwayTiebreaksWon
        End Get
        Set(value As Integer)
            match.AwayTiebreaksWon = value
        End Set
    End Property

    Private Property longestGame As Integer
        Get
            Return match.LongestGame
        End Get
        Set(value As Integer)
            match.LongestGame = value
        End Set
    End Property

    Private Property currentGamePoints As Integer
        Get
            Return match.CurrentGamePoints
        End Get
        Set(value As Integer)
            match.CurrentGamePoints = value
        End Set
    End Property

    ' Variable für ersten Punkt Check
    Private Property firstPointPlayed As Boolean
        Get
            Return match.FirstPointPlayed
        End Get
        Set(value As Boolean)
            match.FirstPointPlayed = value
        End Set
    End Property

    ' Server-Tracking zwischen Sets
    Private Property firstServerOfCurrentSet As Boolean
        Get
            Return match.FirstServerOfCurrentSet
        End Get
        Set(value As Boolean)
            match.FirstServerOfCurrentSet = value
        End Set
    End Property

    'keypress handling
    Private Declare Function GetAsyncKeyState Lib "User32" (ByVal vkey As Integer) As Integer

    Private Sub Tennis24_Scorer_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        ' Port sauber freigeben, damit ein erneuter Start (oder ein anderes Programm) ihn
        ' sofort wieder verwenden kann.
        jsonServer.Stop()
    End Sub

    Private Sub Tennis24_Scorer_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        InitOverlayToggles()

        freezeSetAdvanceTimer.Interval = 1000

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

    ' Registriert alle sich gegenseitig ausschliessenden Overlay-Buttons (Layer 1). Muss erst
    ' nach InitializeComponent laufen, da hier auf die Designer-Button-Controls zugegriffen wird.
    Private Sub InitOverlayToggles()
        overlayToggles = New List(Of OverlayToggle) From {
            New OverlayToggle With {.Key = "home", .Button = Btn_Name_Home, .Template = "lower_name.gtzip", .ComboIndex = 1,
                .ResetText = Function() "lower" & vbNewLine & If(String.IsNullOrEmpty(Tennis24_Main.HomePlayer(0)), "HOME", Tennis24_Main.HomePlayer(0))},
            New OverlayToggle With {.Key = "away", .Button = Btn_Name_Away, .Template = "lower_name.gtzip", .ComboIndex = 1,
                .ResetText = Function() "lower" & vbNewLine & If(String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(0)), "AWAY", Tennis24_Main.AwayPlayer(0))},
            New OverlayToggle With {.Key = "largeresult", .Button = Btn_LargeResult, .Template = "large_result.gtzip", .ComboIndex = 1,
                .ResetText = Function() "Large Result OFF"},
            New OverlayToggle With {.Key = "title", .Button = Btn_Title, .Template = "title.gtzip", .ComboIndex = 1,
                .ResetText = Function() "Title"},
            New OverlayToggle With {.Key = "matchpairing", .Button = Btn_matchpairing, .Template = "match_pairing.gtzip", .ComboIndex = 1,
                .ResetText = Function() "match pairing"},
            New OverlayToggle With {.Key = "matchpairing1", .Button = Btn_matchpairing1, .Template = "match_pairing1.gtzip", .ComboIndex = 1,
                .ResetText = Function() "Match Pairing 1"},
            New OverlayToggle With {.Key = "matchpairing2", .Button = Btn_matchpairing2, .Template = "match_pairing2.gtzip", .ComboIndex = 1,
                .ResetText = Function() "Match Pairing 2"},
            New OverlayToggle With {.Key = "matchpairing3", .Button = Btn_matchpairing3, .Template = "match_pairing3.gtzip", .ComboIndex = 1,
                .ResetText = Function() "Match Pairing 3"},
            New OverlayToggle With {.Key = "matchpairing4", .Button = Btn_matchpairing4, .Template = "match_pairing4.gtzip", .ComboIndex = 1,
                .ResetText = Function() "Match Pairing 4"},
            New OverlayToggle With {.Key = "info1", .Button = Btn_info1, .Template = "info1.gtzip", .ComboIndex = 1,
                .ResetText = Function() "Info1"},
            New OverlayToggle With {.Key = "info2", .Button = Btn_info2, .Template = "info2.gtzip", .ComboIndex = 1,
                .ResetText = Function() "Info2"},
            New OverlayToggle With {.Key = "info3", .Button = Btn_info3, .Template = "info3.gtzip", .ComboIndex = 1,
                .ResetText = Function() "Info3"},
            New OverlayToggle With {.Key = "info4", .Button = Btn_info4, .Template = "info4.gtzip", .ComboIndex = 1,
                .ResetText = Function() "Info4"},
            New OverlayToggle With {.Key = "freename1", .Button = Btn_freename1, .Template = "name.gtzip", .ComboIndex = 1,
                .ResetText = Function() "Free Name 1"},
            New OverlayToggle With {.Key = "freename2", .Button = Btn_freename2, .Template = "name.gtzip", .ComboIndex = 1,
                .ResetText = Function() "Free Name 2"},
            New OverlayToggle With {.Key = "freename3", .Button = Btn_freename3, .Template = "name.gtzip", .ComboIndex = 1,
                .ResetText = Function() "Free Name 3"},
            New OverlayToggle With {.Key = "freename4", .Button = Btn_freename4, .Template = "name.gtzip", .ComboIndex = 1,
                .ResetText = Function() "Free Name 4"},
            New OverlayToggle With {.Key = "freename5", .Button = Btn_freename5, .Template = "name.gtzip", .ComboIndex = 1,
                .ResetText = Function() "Free Name 5"},
            New OverlayToggle With {.Key = "ref1", .Button = Btn_ref1, .Template = "name.gtzip", .ComboIndex = 1,
                .ResetText = Function() "Referee 1"},
            New OverlayToggle With {.Key = "ref2", .Button = Btn_ref2, .Template = "name.gtzip", .ComboIndex = 1,
                .ResetText = Function() "Referee 2"},
            New OverlayToggle With {.Key = "com1", .Button = Btn_com1, .Template = "name.gtzip", .ComboIndex = 1,
                .ResetText = Function() "Commentator 1"},
            New OverlayToggle With {.Key = "com2", .Button = Btn_com2, .Template = "name.gtzip", .ComboIndex = 1,
                .ResetText = Function() "Commentator 2"}
        }
    End Sub

    Private Function GetToggle(key As String) As OverlayToggle
        Return overlayToggles.First(Function(t) t.Key = key)
    End Function

    ' Invertiert nur den Status (ohne vMix-Befehl zu senden) und liefert den neuen Wert zurück,
    ' damit Aufrufer bei Bedarf zwischen Statuswechsel und Versand noch etwas tun können
    ' (z.B. Lower1()/Lower2() vor dem Einblenden aufrufen).
    Private Function ToggleStatus(entry As OverlayToggle) As Boolean
        entry.Status = Not entry.Status
        Return entry.Status
    End Function

    Private Sub SendOverlayCommand(entry As OverlayToggle, isOn As Boolean)
        Dim direction As String = If(isOn, "In", "Out")
        Dim sendstring As String = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(entry.ComboIndex) + direction + "&Input=" + entry.Template + "&Mix=0"
        SendHTMLtovMix(sendstring)
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

    ' Wird nach jeder Zustandsänderung aufgerufen: aktualisiert die (optionale) Statistik-
    ' Anzeige UND schickt den aktuellen Stand an vMix.
    '
    ' WICHTIG: Der vMix-Versand ist bewusst NICHT an die Statistik gekoppelt. Früher stand er
    ' am Ende von UpdateScoreDisplays() innerhalb von "If DataGridView1.Rows.Count >= 19" -
    ' seit die Statistik in einer eigenen, evtl. geschlossenen Form liegt, wäre der Scorebug-
    ' Update in vMix dadurch stillschweigend ausgefallen.
    Private Sub UpdateScoreDisplays()
        UpdateStatisticsDisplay()

        ' Grafik-Engine aktualisieren - unabhängig davon, ob die Statistik geöffnet ist
        SendDataToGraphicsEngine()

        ' Live-JSON-Datenquelle aktualisieren - unabhängig davon, ob sie gerade läuft
        ' (UpdateState puffert nur; ein evtl. nicht laufender Server liest sie nie)
        jsonServer.UpdateState(BuildLiveStateJson())

        If isMatchFinished Then Hidepoints()
    End Sub

    ' Startet/stoppt die Live-JSON-Datenquelle passend zu den aktuellen Settings
    ' (Checkbox3 = an/aus, NumericUpDown2 -> TextBoxValues(44) = Port).
    Private Sub SyncJsonServerFromSettings()
        Dim shouldRun As Boolean = Tennis24_Settings.CheckBoxValues(3)
        Dim port As Integer = 41200
        Integer.TryParse(Tennis24_Settings.TextBoxValues(44), port)

        If shouldRun Then
            If Not jsonServer.IsRunning OrElse jsonServerRunningPort <> port Then
                jsonServer.Stop()
                If jsonServer.Start(port) Then
                    jsonServerRunningPort = port
                Else
                    jsonServerRunningPort = 0
                    ' Häufigste Ursache: fehlende Berechtigung für "http://+:PORT/" ohne
                    ' vorherige "netsh http add urlacl" (siehe TennisJsonServer.vb) oder der
                    ' Port ist bereits belegt.
                    MessageBox.Show(
                        $"Live-JSON-Datenquelle konnte auf Port {port} nicht gestartet werden:" & vbNewLine & vbNewLine &
                        jsonServer.LastError & vbNewLine & vbNewLine &
                        "Häufigste Ursache: Windows erlaubt einem Nicht-Administrator-Programm standardmässig nicht, netzwerkweit auf einem Port zu lauschen." & vbNewLine &
                        "Einmalig als Administrator ausführen:" & vbNewLine &
                        $"netsh http add urlacl url=http://+:{port}/ user=S-1-1-0",
                        "Live-JSON-Datenquelle", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                End If
            End If
        Else
            jsonServer.Stop()
            jsonServerRunningPort = 0
        End If
    End Sub

    Private Function ParseLabelInt(text As String) As Integer
        Dim result As Integer
        Integer.TryParse(text, result)
        Return result
    End Function

    ' Baut den kompletten aktuellen Spielstand als JSON - unabhängig von vMix, für beliebige
    ' Grafik-Software geeignet, die die Live-JSON-Datenquelle abfragt.
    Private Function BuildLiveStateJson() As String
        Dim homePlayerName As String = If(String.IsNullOrEmpty(Tennis24_Main.HomePlayer(0)), "HOME", Tennis24_Main.HomePlayer(0))
        Dim awayPlayerName As String = If(String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(0)), "AWAY", Tennis24_Main.AwayPlayer(0))
        Dim homeCountry As String = If(String.IsNullOrEmpty(Tennis24_Main.HomePlayer(3)), "", Tennis24_Main.HomePlayer(3))
        Dim awayCountry As String = If(String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(3)), "", Tennis24_Main.AwayPlayer(3))

        Dim homeSetScores As Integer() = {ParseLabelInt(lbl_home_s1.Text), ParseLabelInt(lbl_home_s2.Text), ParseLabelInt(lbl_home_s3.Text), ParseLabelInt(lbl_home_s4.Text), ParseLabelInt(lbl_home_s5.Text)}
        Dim awaySetScores As Integer() = {ParseLabelInt(lbl_away_s1.Text), ParseLabelInt(lbl_away_s2.Text), ParseLabelInt(lbl_away_s3.Text), ParseLabelInt(lbl_away_s4.Text), ParseLabelInt(lbl_away_s5.Text)}

        Dim homeObj As New JsonObjectBuilder()
        homeObj.AddString("name", homePlayerName) _
               .AddString("country", homeCountry) _
               .AddString("points", ConvertPointsToTennisScore(homePoints, awayPoints)) _
               .AddInt("games", homeGames) _
               .AddInt("sets", homeSets) _
               .AddBool("serving", isHomeServing) _
               .AddIntArray("setScores", homeSetScores) _
               .AddInt("breaks", homeBreaks) _
               .AddInt("breakPointsWon", match.HomeBreakPointsConverted) _
               .AddInt("breakPointsTotal", match.HomeBreakPointsTotal) _
               .AddInt("miniBreaks", match.HomeMiniBreaks)

        Dim awayObj As New JsonObjectBuilder()
        awayObj.AddString("name", awayPlayerName) _
               .AddString("country", awayCountry) _
               .AddString("points", ConvertPointsToTennisScore(awayPoints, homePoints)) _
               .AddInt("games", awayGames) _
               .AddInt("sets", awaySets) _
               .AddBool("serving", Not isHomeServing) _
               .AddIntArray("setScores", awaySetScores) _
               .AddInt("breaks", awayBreaks) _
               .AddInt("breakPointsWon", match.AwayBreakPointsConverted) _
               .AddInt("breakPointsTotal", match.AwayBreakPointsTotal) _
               .AddInt("miniBreaks", match.AwayMiniBreaks)

        Dim matchType = If(Tennis24_Settings.TextBoxValues(50) = "5", "Best of 5", "Best of 3")

        Dim root As New JsonObjectBuilder()
        root.AddRaw("home", homeObj.ToString()) _
            .AddRaw("away", awayObj.ToString()) _
            .AddInt("currentSet", currentSet) _
            .AddString("matchType", matchType) _
            .AddBool("isTiebreak", isTiebreak) _
            .AddBool("isMatchTiebreak", isMatchTiebreakSet) _
            .AddBool("isMatchFinished", isMatchFinished) _
            .AddString("breakPointHolder", match.BreakPointHolder()) _
            .AddInt("breakPointCount", match.CurrentBreakPointCount()) _
            .AddBool("sidesSwapped", match.AreSidesCurrentlySwapped()) _
            .AddString("updatedAt", DateTime.UtcNow.ToString("o"))

        Return root.ToString()
    End Function

    ' Die Statistik-Anzeige liegt in einer eigenen Form (Tennis24_Statistics) und wird nur
    ' aktualisiert, wenn sie gerade geöffnet ist.
    Private Sub UpdateStatisticsDisplay()
        ' DEBUG: Zeige die tatsächlichen Werte
        Label1.Text = $"H-Won:{homeServiceGamesWon} A-Won:{awayServiceGamesWon} Breaks: H:{homeBreaks} A:{awayBreaks}"

        If statisticsForm IsNot Nothing AndAlso Not statisticsForm.IsDisposed Then
            statisticsForm.RefreshStatistics()
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
            UpdateScoreDisplays()

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

        ' Die Spaltenüberschriften der Statistik setzt Tennis24_Statistics selbst
        ' (UpdatePlayerNameHeaders bei jedem RefreshStatistics).

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

        ' Zustand speichern (für Undo) und Punkt verbuchen
        match.PushState()
        match.RegisterPoint(player)

        ' Operator-Absicherung für Freeze Set: Wurde der Scorebug nach dem Satzgewinn nicht
        ' ausgeblendet, hängt die Anzeige noch auf dem alten Satz. Sobald der erste Punkt des
        ' neuen Satzes fällt, muss sie zwangsweise auf den neuen Satz umschalten.
        AdvanceFrozenScorebugIfStale()

        UpdateScore()
    End Sub

    Private Sub AdvanceFrozenScorebugIfStale()
        If freezeSetEnabled AndAlso displayedScorebugSet <> currentSet Then
            freezeSetAdvanceTimer.Stop()
            displayedScorebugSet = currentSet

            ' Nur wenn der Scorebug gerade eingeblendet ist, muss vMix aktiv umgeschaltet
            ' werden; sonst genügt es, den Zielsatz zu setzen (nächstes Einschalten zeigt ihn).
            If scorebugtoggleStatus Then
                SendHTMLtovMix("Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(2) + "In&Input=scorebug_" & displayedScorebugSet.ToString() & "s.gtzip&Mix=0")
                Btn_Scorebug.Text = $"Scorebug ON (Set {displayedScorebugSet})"
            End If
        End If
    End Sub

    Private Sub UpdateScore()
        lbl_homepoint.Text = ConvertPointsToTennisScore(homePoints, awayPoints)
        lbl_awaypoint.Text = ConvertPointsToTennisScore(awayPoints, homePoints)

        If IsGameWon(homePoints, awayPoints) Then
            If isMatchTiebreakSet Then
                ' Match-Tiebreak entscheidet den ganzen Satz; die Satzanzeige zeigt das
                ' tatsächliche Tiebreak-Ergebnis (z.B. 10:7), analog zum regulären 7:6-Tiebreak-Satz.
                homeGames = homePoints
                awayGames = awayPoints
            Else
                homeGames += 1
            End If
            CheckForBreak("home")
            TrackLongestGame()

            ' KORRIGIERT: Tiebreak-Server-Logik VOR ResetPoints()
            Dim shouldSwitchServer As Boolean = False
            If isTiebreak OrElse isMatchTiebreakSet Then
                ' Im (Match-)Tiebreak: Server wechselt alle 2 Punkte
                ' Aktuelle Gesamtpunkte BEVOR Reset
                Dim totalTiebreakPoints = homeTotalPoints + awayTotalPoints
                If totalTiebreakPoints Mod 2 = 1 Then
                    shouldSwitchServer = True
                End If
            Else
                shouldSwitchServer = True
            End If

            ResetPoints()

            ' Server wechseln nur wenn Match nicht beendet ist
            If Not isMatchFinished AndAlso shouldSwitchServer Then
                isHomeServing = Not isHomeServing
                UpdateServerDisplay()
            End If

        ElseIf IsGameWon(awayPoints, homePoints) Then
            If isMatchTiebreakSet Then
                homeGames = homePoints
                awayGames = awayPoints
            Else
                awayGames += 1
            End If
            CheckForBreak("away")
            TrackLongestGame()

            ' KORRIGIERT: Tiebreak-Server-Logik VOR ResetPoints()
            Dim shouldSwitchServer As Boolean = False
            If isTiebreak OrElse isMatchTiebreakSet Then
                ' Im (Match-)Tiebreak: Server wechselt alle 2 Punkte
                ' Aktuelle Gesamtpunkte BEVOR Reset
                Dim totalTiebreakPoints = homeTotalPoints + awayTotalPoints
                If totalTiebreakPoints Mod 2 = 1 Then
                    shouldSwitchServer = True
                End If
            Else
                shouldSwitchServer = True
            End If

            ResetPoints()

            ' Server wechseln nur wenn Match nicht beendet ist
            If Not isMatchFinished AndAlso shouldSwitchServer Then
                isHomeServing = Not isHomeServing
                UpdateServerDisplay()
            End If
        End If

        ' Aufschlag-Rotation INNERHALB eines laufenden (Match-)Tiebreaks: Der Eröffner
        ' schlägt 1 Punkt auf, danach wechseln sich beide je 2 Punkte ab. Der Block oben
        ' greift nur, wenn der Tiebreak zu Ende ist (IsGameWon) - ohne diese Zeilen blieb
        ' der Aufschlag-Ball während des gesamten Tiebreaks beim selben Spieler stehen.
        ' Die Punktebedingung stellt sicher, dass hier nur mitten im Tiebreak synchronisiert
        ' wird (nach dem Tiebreak-Gewinn sind die Punkte bereits zurückgesetzt).
        If Not isMatchFinished AndAlso match.IsInAnyTiebreak() AndAlso homePoints + awayPoints > 0 Then
            Dim tiebreakServerIsHome As Boolean = match.TiebreakServerIsHome()
            If isHomeServing <> tiebreakServerIsHome Then
                isHomeServing = tiebreakServerIsHome
                UpdateServerDisplay()
            End If
        End If

        UpdateGameLabels()

        ' MUSS vor IsSetWon ausgewertet werden: IsSetWon setzt IsTiebreak intern auf False,
        ' bevor es True zurückgibt. Die frühere Prüfung "If isTiebreak" NACH dem Aufruf war
        ' deshalb immer False - "Tiebreaks Won" blieb dauerhaft auf 0.
        Dim wasTiebreak As Boolean = isTiebreak OrElse isMatchTiebreakSet

        If IsSetWon(homeGames, awayGames) Then
            homeSets += 1
            If wasTiebreak Then
                homeTiebreaksWon += 1
            End If
            UpdateSetLabel("home")
            If freezeSetEnabled Then HighlightWonSet("home")
            CheckForMatchEnd(wasTiebreak)
        ElseIf IsSetWon(awayGames, homeGames) Then
            awaySets += 1
            If wasTiebreak Then
                awayTiebreaksWon += 1
            End If
            UpdateSetLabel("away")
            If freezeSetEnabled Then HighlightWonSet("away")
            CheckForMatchEnd(wasTiebreak)
        End If

        UpdateScoreDisplays()
        LargeResult()
    End Sub


    Private Sub CheckForBreak(winner As String)
        match.CheckForBreak(winner)
    End Sub

    Private Sub ResetMatch()
        match.ResetMatch()

        ' Match-Tiebreak-Einstellungen aus den Settings übernehmen
        match.MatchTiebreakEnabled = Tennis24_Settings.CheckBoxValues(1)
        Dim matchTiebreakTarget As Integer
        If Integer.TryParse(Tennis24_Settings.TextBoxValues(42), matchTiebreakTarget) AndAlso matchTiebreakTarget > 0 Then
            match.MatchTiebreakTarget = matchTiebreakTarget
        Else
            match.MatchTiebreakTarget = 10
        End If

        ' Freeze-Set-Einstellung aus den Settings übernehmen
        freezeSetEnabled = Tennis24_Settings.CheckBoxValues(2)
        displayedScorebugSet = 1
        freezeSetAdvanceTimer.Stop()

        ' Gelb-Markierungen gewonnener Sätze aus einem vorherigen Match zurücksetzen
        If freezeSetEnabled Then
            For setNumber As Integer = 1 To 5
                SetSetNumberColour("home", setNumber, NORMAL_SET_COLOUR)
                SetSetNumberColour("away", setNumber, NORMAL_SET_COLOUR)
            Next
        End If

        SyncJsonServerFromSettings()

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

        UpdateServerDisplay()
        UpdateScoreDisplays()
        Showpoints()
    End Sub

    Private Sub Btn_undo_Click(sender As Object, e As EventArgs) Handles btn_undo.Click
        Dim lastState = match.PopState()
        If lastState IsNot Nothing Then
            CheckBox_noTiebreak.Checked = noTiebreakMode
            ' Falls der zurückgenommene Punkt das Match beendet hatte, muss der Gewinner-
            ' Banner wieder verschwinden - vorher blieb er nach einem Undo sichtbar.
            Lbl_Winner.Visible = isMatchFinished

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
            UpdateScoreDisplays()
            ' Falls Undo über eine Satzgrenze zurückgeht, muss der ggf. aktive Scorebug-Overlay
            ' auf die Vorlage des (wieder aktuellen) Satzes zurückwechseln.
            UpdateScoreBug()
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
        match.TrackLongestGame()
    End Sub

    Private Sub CheckForMatchEnd(setEndedInTiebreak As Boolean)
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

            ' Tennis-Regel für den Aufschlag zwischen den Sätzen: Der Aufschlag wechselt
            ' einfach spielweise weiter - wer das letzte Spiel des Satzes aufschlug, schlägt
            ' das erste Spiel des neuen Satzes NICHT auf.
            '
            ' Der Wechsel nach dem letzten Spiel ist oben im IsGameWon-Block bereits passiert,
            ' isHomeServing stimmt hier also schon. Die frühere Zeile
            '   isHomeServing = Not firstServerOfCurrentSet
            ' hat stattdessen bei JEDEM Satz den Satz-Eröffner alternieren lassen. Das ist nur
            ' bei ungerader Spielanzahl korrekt: Nach einem 6:4 (10 Spiele) muss derselbe
            ' Spieler wieder eröffnen, die alte Logik gab den Aufschlag aber dem Gegner.
            '
            ' Ausnahme Tiebreak: Dort greift die spielweise Rotation nicht. Regel ist, dass der
            ' Eröffner des Tiebreaks den neuen Satz als Rückschläger beginnt.
            If setEndedInTiebreak Then
                isHomeServing = Not match.TiebreakStartServerIsHome
            End If
            firstServerOfCurrentSet = isHomeServing

            ResetGames()
            UpdateServerDisplay()
            UpdateScoreDisplays()
            UpdateScoreBug()
        End If
    End Sub

    Private Function IsSetWon(playerGames As Integer, opponentGames As Integer) As Boolean
        Return match.IsSetWon(playerGames, opponentGames)
    End Function

    Private Sub ResetPoints()
        match.ResetPoints()
        lbl_homepoint.Text = "0"
        lbl_awaypoint.Text = "0"
    End Sub

    Private Sub ResetGames()
        match.ResetGames()
    End Sub

    Private Function IsGameWon(playerPoints As Integer, opponentPoints As Integer) As Boolean
        Return match.IsGameWon(playerPoints, opponentPoints)
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
        Return match.ConvertPointsToTennisScore(playerPoints, opponentPoints)
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

            sendstring(index) = BuildVmixSetCommand("SetText", scorebugtitle, "hpoint.Text", lbl_homepoint.Text)
            index += 1
            sendstring(index) = BuildVmixSetCommand("SetText", scorebugtitle, "apoint.Text", lbl_awaypoint.Text)
            index += 1

            Dim homePlayerName As String = If(String.IsNullOrEmpty(Tennis24_Main.HomePlayer(0)), "HOME", Tennis24_Main.HomePlayer(0))
            Dim awayPlayerName As String = If(String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(0)), "AWAY", Tennis24_Main.AwayPlayer(0))

            sendstring(index) = BuildVmixSetCommand("SetText", scorebugtitle, "hname.Text", homePlayerName)
            index += 1
            sendstring(index) = BuildVmixSetCommand("SetText", scorebugtitle, "aname.Text", awayPlayerName)
            index += 1

            Dim homeCountryISO3 As String = If(String.IsNullOrEmpty(Tennis24_Main.HomePlayer(3)), "HOM", Tennis24_Main.HomePlayer(3))
            Dim awayCountryISO3 As String = If(String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(3)), "AWY", Tennis24_Main.AwayPlayer(3))

            sendstring(index) = BuildVmixSetCommand("SetText", scorebugtitle, "hcountry.Text", homeCountryISO3)
            index += 1
            sendstring(index) = BuildVmixSetCommand("SetText", scorebugtitle, "acountry.Text", awayCountryISO3)
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

            sendstring(index) = BuildVmixSetCommand("SetText", scorebugtitle, "h1.Text", lbl_home_s1.Text)
            index += 1
            sendstring(index) = BuildVmixSetCommand("SetText", scorebugtitle, "h2.Text", lbl_home_s2.Text)
            index += 1
            sendstring(index) = BuildVmixSetCommand("SetText", scorebugtitle, "h3.Text", lbl_home_s3.Text)
            index += 1
            sendstring(index) = BuildVmixSetCommand("SetText", scorebugtitle, "h4.Text", lbl_home_s4.Text)
            index += 1
            sendstring(index) = BuildVmixSetCommand("SetText", scorebugtitle, "h5.Text", lbl_home_s5.Text)
            index += 1

            sendstring(index) = BuildVmixSetCommand("SetText", scorebugtitle, "a1.Text", lbl_away_s1.Text)
            index += 1
            sendstring(index) = BuildVmixSetCommand("SetText", scorebugtitle, "a2.Text", lbl_away_s2.Text)
            index += 1
            sendstring(index) = BuildVmixSetCommand("SetText", scorebugtitle, "a3.Text", lbl_away_s3.Text)
            index += 1
            sendstring(index) = BuildVmixSetCommand("SetText", scorebugtitle, "a4.Text", lbl_away_s4.Text)
            index += 1
            sendstring(index) = BuildVmixSetCommand("SetText", scorebugtitle, "a5.Text", lbl_away_s5.Text)

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
        sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "hname.Text", homePlayerName) : SendHTMLtovMix(sendstring)
        sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "aname.Text", awayPlayerName) : SendHTMLtovMix(sendstring)
        sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "hcountry.Text", homecountry) : SendHTMLtovMix(sendstring)
        sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "acountry.Text", awaycountry) : SendHTMLtovMix(sendstring)

        sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "h1.Text", lbl_home_s1.Text) : SendHTMLtovMix(sendstring)
        sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "h2.Text", lbl_home_s2.Text) : SendHTMLtovMix(sendstring)
        sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "h3.Text", lbl_home_s3.Text) : SendHTMLtovMix(sendstring)
        sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "h4.Text", lbl_home_s4.Text) : SendHTMLtovMix(sendstring)
        sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "h5.Text", lbl_home_s5.Text) : SendHTMLtovMix(sendstring)

        sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "a1.Text", lbl_away_s1.Text) : SendHTMLtovMix(sendstring)
        sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "a2.Text", lbl_away_s2.Text) : SendHTMLtovMix(sendstring)
        sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "a3.Text", lbl_away_s3.Text) : SendHTMLtovMix(sendstring)
        sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "a4.Text", lbl_away_s4.Text) : SendHTMLtovMix(sendstring)
        sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "a5.Text", lbl_away_s5.Text) : SendHTMLtovMix(sendstring)

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
            sendstring = BuildVmixSetCommand("SetImage", scorebugtitle, "hcountry_flag.Source", flagInfo.Path) : SendHTMLtovMix(sendstring)
        Else
            sendstring = BuildVmixSetCommand("SetImage", scorebugtitle, "hcountry_flag.Source", "C:\VMIX\tennis\flags\transparent.png") : SendHTMLtovMix(sendstring)
        End If

        flagInfo = GetFlagInfo(awaycountry)
        If flagInfo.Exists Then
            sendstring = BuildVmixSetCommand("SetImage", scorebugtitle, "acountry_flag.Source", flagInfo.Path) : SendHTMLtovMix(sendstring)
        Else
            sendstring = BuildVmixSetCommand("SetImage", scorebugtitle, "acountry_flag.Source", "C:\VMIX\tennis\flags\transparent.png") : SendHTMLtovMix(sendstring)
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

    ' Baut einen "SetText"/"SetImage"/"SetTextColour"-vMix-Befehl und kodiert den Wert dabei
    ' konsequent URL-sicher. Vorher wurden Spielernamen/Freitexte meist unkodiert eingefügt,
    ' sodass "&" oder "#" die vMix-Request verfälschen bzw. abschneiden konnten.
    '
    ' WICHTIG: Uri.EscapeDataString statt WebUtility.UrlEncode - letzteres kodiert
    ' Leerzeichen als "+" (alte Formular-Kodierung), was in einer vMix-URL als literales
    ' Plus-Zeichen ankommt ("Roger+Federer"). Uri.EscapeDataString erzeugt "%20".
    Private Function EncodeVmixValue(value As String) As String
        Return Uri.EscapeDataString(If(value, ""))
    End Function

    Private Function BuildVmixSetCommand(func As String, input As String, selectedName As String, value As String) As String
        Return "Function=" + func + "&Input=" + input + "&SelectedName=" + selectedName + "&Value=" + EncodeVmixValue(value)
    End Function

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

    Private Sub Btn_statistics_Click(sender As Object, e As EventArgs) Handles Btn_statistics.Click
        ' Statistik-Fenster öffnen bzw. nach vorne holen (nicht-modal, damit weitergezählt
        ' werden kann, während die Statistik offen ist)
        If statisticsForm Is Nothing OrElse statisticsForm.IsDisposed Then
            statisticsForm = New Tennis24_Statistics()
            statisticsForm.AttachMatch(match)
            statisticsForm.Show(Me)
        Else
            statisticsForm.AttachMatch(match)
            statisticsForm.BringToFront()
        End If

        statisticsForm.RefreshStatistics()
        PictureBox1.Focus() 'Fokus zurück, damit die Pfeiltasten weiter funktionieren
    End Sub

    Private Sub Btn_Scorebug_Click(sender As Object, e As EventArgs) Handles Btn_Scorebug.Click
        'blendet scorebug ein und aus
        Dim sendstring As String

        scorebugtoggleStatus = Not scorebugtoggleStatus

        If scorebugtoggleStatus Then
            sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(2) + "In&Input=scorebug_" & displayedScorebugSet.ToString() & "s.gtzip&Mix=0"
            Btn_Scorebug.BackColor = Color.Red
            Btn_Scorebug.Text = $"Scorebug ON (Set {displayedScorebugSet})"
        Else
            sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(2) + "Out&Input=scorebug_" & displayedScorebugSet.ToString() & "s.gtzip&Mix=0"
            Btn_Scorebug.BackColor = SystemColors.ButtonHighlight
            Btn_Scorebug.Text = "Scorebug OFF"

            ' Freeze Set: nach dem Ausblenden ca. 1 Sekunde warten (Out-Animation in vMix),
            ' dann erst auf den neuen Satz weiterschalten, damit das nächste Einschalten
            ' den (leeren) neuen Satz statt des eingefrorenen alten Satzes zeigt.
            If freezeSetEnabled AndAlso displayedScorebugSet <> currentSet Then
                freezeSetAdvanceTimer.Start()
            End If
        End If

        SendHTMLtovMix(sendstring)
    End Sub

    Private Sub FreezeSetAdvanceTimer_Tick(sender As Object, e As EventArgs) Handles freezeSetAdvanceTimer.Tick
        freezeSetAdvanceTimer.Stop()
        displayedScorebugSet = currentSet
    End Sub

    Private Sub Btn_LargeResult_Click(sender As Object, e As EventArgs) Handles Btn_LargeResult.Click
        'blendet grosses resultat ein und aus
        Dim entry = GetToggle("largeresult")

        ' Reset other toggles first
        ResetOtherOverlayToggles(entry.Key)

        Dim isOn = ToggleStatus(entry)
        SendOverlayCommand(entry, isOn)

        If isOn Then
            Btn_LargeResult.BackColor = Color.Red
            Btn_LargeResult.Text = $"Large Result ON (Set {currentSet})"
        Else
            Btn_LargeResult.BackColor = SystemColors.ButtonHighlight
            Btn_LargeResult.Text = "Large Result OFF"
        End If
    End Sub

    ' Farbe der hervorgehobenen Spielzahl des gewonnenen Satzes - in den Settings wählbar
    ' (Btn_gamewon_colour, gespeichert als "#RRGGBB" in TextBoxValues(43)).
    Private ReadOnly Property WonSetColour As String
        Get
            Dim colour As String = Tennis24_Settings.TextBoxValues(43)
            Return If(String.IsNullOrWhiteSpace(colour), Tennis24_Settings.DEFAULT_GAMEWON_COLOUR, colour.Trim())
        End Get
    End Property

    Private Const NORMAL_SET_COLOUR As String = "#FFFFFF"   ' weiss (Standard-Textfarbe)

    ' Färbt die Spielzahl des soeben gewonnenen Satzes in allen Scorebug-Vorlagen gelb ein
    ' (nur bei aktivem Freeze Set), analog zum Datenmuster in SendDataToGraphicsEngine, das
    ' h1-h5/a1-a5 ohnehin bereits an alle 5 Vorlagen sendet.
    '
    ' WICHTIG: Der Farbwert MUSS URL-kodiert werden. vMix erwartet ihn mit führendem "#"
    ' (z.B. #FFFF00), und ein rohes "#" in einer URL leitet den Fragment-Teil ein - alles
    ' danach (inkl. &SelectedName=...) wird gar nicht erst an vMix gesendet. BuildVmixSetCommand
    ' kodiert das "#" zu "%23", damit der komplette Befehl ankommt.
    Private Sub SetSetNumberColour(player As String, setNumber As Integer, colour As String)
        Dim fieldName As String = If(player = "home", "h", "a") & setNumber.ToString() & ".Text"
        Dim scorebugtitles() As String = {"scorebug_1s.gtzip", "scorebug_2s.gtzip", "scorebug_3s.gtzip", "scorebug_4s.gtzip", "scorebug_5s.gtzip"}

        For Each scorebugtitle As String In scorebugtitles
            SendHTMLtovMix(BuildVmixSetCommand("SetTextColour", scorebugtitle, fieldName, colour))
        Next
    End Sub

    Private Sub HighlightWonSet(winner As String)
        SetSetNumberColour(winner, currentSet, WonSetColour)
    End Sub

    Private Sub UpdateScoreBug()
        'Es gibt verschiedene Scorebug-Templates für verschiedene Sets gibt (scorebug_1s.gtzip, scorebug_2s.gtzip, etc.), lädt automatisch das richtige Template
        If Not freezeSetEnabled Then
            ' Ohne Freeze Set: wie bisher sofort auf den aktuellen Satz zeigen
            displayedScorebugSet = currentSet
        End If
        ' Mit Freeze Set bleibt displayedScorebugSet bewusst auf dem alten Satz stehen, bis
        ' der Bediener den Scorebug manuell ausschaltet (siehe Btn_Scorebug_Click).

        If scorebugtoggleStatus Then
            Dim sendstring As String
            sendstring = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(2) + "In&Input=scorebug_" & displayedScorebugSet.ToString() & "s.gtzip&Mix=0"
            Btn_Scorebug.Text = $"Scorebug ON (Set {displayedScorebugSet})"
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
            sendstring = BuildVmixSetCommand("SetImage", nametemplate, "bg" + Trim(Str(i)) + ".Source", image) : SendHTMLtovMix(sendstring)
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
            sendstring = BuildVmixSetCommand("SetImage", nametemplate, "bg" + Trim(Str(i)) + ".Source", image) : SendHTMLtovMix(sendstring)
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

        sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "name.Text", PlayerName) : SendHTMLtovMix(sendstring)
        sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "country.Text", country) : SendHTMLtovMix(sendstring)
        sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "age.Text", age) : SendHTMLtovMix(sendstring)
        sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "height.Text", height) : SendHTMLtovMix(sendstring)
        sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "info1.Text", info1) : SendHTMLtovMix(sendstring)
        sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "info2.Text", info2) : SendHTMLtovMix(sendstring)
        sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "info3.Text", info3) : SendHTMLtovMix(sendstring)

        Dim flagInfo = GetFlagInfo(country)
        If flagInfo.Exists Then
            sendstring = BuildVmixSetCommand("SetImage", scorebugtitle, "country_flag.Source", flagInfo.Path) : SendHTMLtovMix(sendstring)
        Else
            sendstring = BuildVmixSetCommand("SetImage", scorebugtitle, "country_flag.Source", "C:\VMIX\tennis\flags\transparent.png") : SendHTMLtovMix(sendstring)
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

        sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "name.Text", PlayerName) : SendHTMLtovMix(sendstring)
        sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "country.Text", country) : SendHTMLtovMix(sendstring)
        sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "age.Text", age) : SendHTMLtovMix(sendstring)
        sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "height.Text", height) : SendHTMLtovMix(sendstring)
        sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "info1.Text", info1) : SendHTMLtovMix(sendstring)
        sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "info2.Text", info2) : SendHTMLtovMix(sendstring)
        sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "info3.Text", info3) : SendHTMLtovMix(sendstring)

        Dim flagInfo = GetFlagInfo(country)
        If flagInfo.Exists Then
            sendstring = BuildVmixSetCommand("SetImage", scorebugtitle, "country_flag.Source", flagInfo.Path) : SendHTMLtovMix(sendstring)
        Else
            sendstring = BuildVmixSetCommand("SetImage", scorebugtitle, "country_flag.Source", "C:\VMIX\tennis\flags\transparent.png") : SendHTMLtovMix(sendstring)
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

            sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "hname.Text", hPlayerName) : SendHTMLtovMix(sendstring)
            sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "hcountry.Text", hcountry) : SendHTMLtovMix(sendstring)
            sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "hage.Text", hage) : SendHTMLtovMix(sendstring)
            sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "hheight.Text", hheight) : SendHTMLtovMix(sendstring)
            sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "hdata1.Text", hdata1) : SendHTMLtovMix(sendstring)
            sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "hdata2.Text", hdata2) : SendHTMLtovMix(sendstring)
            sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "hdata3.Text", hdata3) : SendHTMLtovMix(sendstring)

            Dim flagInfo = GetFlagInfo(hcountry)
            If flagInfo.Exists Then
                sendstring = BuildVmixSetCommand("SetImage", scorebugtitle, "hcountry_flag.Source", flagInfo.Path) : SendHTMLtovMix(sendstring)
            Else
                sendstring = BuildVmixSetCommand("SetImage", scorebugtitle, "hcountry_flag.Source", "C:\VMIX\tennis\flags\transparent.png") : SendHTMLtovMix(sendstring)
            End If

            ' Away Player Daten
            Dim aPlayerName As String = If(String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(0)), "Away", Tennis24_Main.AwayPlayer(1) & " " & Tennis24_Main.AwayPlayer(0))
            Dim acountry As String = If(String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(0)), "Away", Tennis24_Main.AwayPlayer(3))

            Dim aage As String = If(hidedetails OrElse String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(4)), "", "Age: " & Tennis24_Main.AwayPlayer(4))
            Dim aheight As String = If(hidedetails OrElse String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(5)), "", "Height: " & Tennis24_Main.AwayPlayer(5))
            Dim adata1 As String = If(hidedetails OrElse String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(6)), "", Tennis24_Main.AwayPlayer(6))
            Dim adata2 As String = If(hidedetails OrElse String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(7)), "", Tennis24_Main.AwayPlayer(7))
            Dim adata3 As String = If(hidedetails OrElse String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(8)), "", Tennis24_Main.AwayPlayer(8))

            sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "aname.Text", aPlayerName) : SendHTMLtovMix(sendstring)
            sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "acountry.Text", acountry) : SendHTMLtovMix(sendstring)
            sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "aage.Text", aage) : SendHTMLtovMix(sendstring)
            sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "aheight.Text", aheight) : SendHTMLtovMix(sendstring)
            sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "adata1.Text", adata1) : SendHTMLtovMix(sendstring)
            sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "adata2.Text", adata2) : SendHTMLtovMix(sendstring)
            sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "adata3.Text", adata3) : SendHTMLtovMix(sendstring)

            flagInfo = GetFlagInfo(acountry)
            If flagInfo.Exists Then
                sendstring = BuildVmixSetCommand("SetImage", scorebugtitle, "acountry_flag.Source", flagInfo.Path) : SendHTMLtovMix(sendstring)
            Else
                sendstring = BuildVmixSetCommand("SetImage", scorebugtitle, "acountry_flag.Source", "C:\VMIX\tennis\flags\transparent.png") : SendHTMLtovMix(sendstring)
            End If
        Next
    End Sub


    Private Sub Btn_Name_Home_Click(sender As Object, e As EventArgs) Handles Btn_Name_Home.Click
        'blendet spielername1 ein und aus
        Dim entry = GetToggle("home")
        Dim homePlayerName As String = If(String.IsNullOrEmpty(Tennis24_Main.HomePlayer(0)), "HOME", Tennis24_Main.HomePlayer(0))
        Dim Playername = "lower" & vbNewLine & homePlayerName

        ' Reset other toggles first
        ResetOtherOverlayToggles(entry.Key)
        Dim isOn = ToggleStatus(entry)

        If isOn Then Lower1()
        SendOverlayCommand(entry, isOn)

        Btn_Name_Home.BackColor = If(isOn, Color.Red, SystemColors.ButtonHighlight)
        Btn_Name_Home.Text = Playername
    End Sub

    Private Sub Btn_Name_Away_Click(sender As Object, e As EventArgs) Handles Btn_Name_Away.Click
        'blendet spielername2 ein und aus
        Dim entry = GetToggle("away")
        Dim awayPlayerName As String = If(String.IsNullOrEmpty(Tennis24_Main.AwayPlayer(0)), "AWAY", Tennis24_Main.AwayPlayer(0))
        Dim Playername = "lower" & vbNewLine & awayPlayerName

        ' Reset other toggles first
        ResetOtherOverlayToggles(entry.Key)
        Dim isOn = ToggleStatus(entry)

        If isOn Then Lower2()
        SendOverlayCommand(entry, isOn)

        Btn_Name_Away.BackColor = If(isOn, Color.Red, SystemColors.ButtonHighlight)
        Btn_Name_Away.Text = Playername
    End Sub

    Private Sub Btn_Title_Click(sender As Object, e As EventArgs) Handles Btn_Title.Click
        'blendet Titel ein und aus
        Dim entry = GetToggle("title")

        ' Reset other toggles first
        ResetOtherOverlayToggles(entry.Key)

        SendHTMLtovMix(BuildVmixSetCommand("SetText", entry.Template, "TextBlock1.Text", Tennis24_Settings.TextBoxValues(1)))
        SendHTMLtovMix(BuildVmixSetCommand("SetText", entry.Template, "TextBlock2.Text", Tennis24_Settings.TextBoxValues(2)))

        Dim isOn = ToggleStatus(entry)
        SendOverlayCommand(entry, isOn)

        Btn_Title.BackColor = If(isOn, Color.Red, SystemColors.ButtonHighlight)
        Btn_Title.Text = "Title"
    End Sub

    Private Sub Btn_matchpairing_Click(sender As Object, e As EventArgs) Handles Btn_matchpairing.Click
        'blendet paarung ein und aus
        Pairing()

        Dim entry = GetToggle("matchpairing")

        ' Reset other toggles first
        ResetOtherOverlayToggles(entry.Key)

        Dim isOn = ToggleStatus(entry)
        SendOverlayCommand(entry, isOn)

        Btn_matchpairing.BackColor = If(isOn, Color.Red, SystemColors.ButtonHighlight)
        Btn_matchpairing.Text = "match pairing"
    End Sub



    ' Blendet alle anderen aktiven Overlay-Buttons (Layer 1) aus ausser dem angegebenen.
    ' HINWEIS: "matchpairing"/"matchpairing1-4" haben hier jetzt den korrekten Template-Namen
    ' ("match_pairing...gtzip" statt "matchpairing...gtzip") - vorher wurde beim Zurücksetzen
    ' der falsche vMix-Input adressiert, wodurch das Ausblenden dieser Overlays nie ankam.
    Private Sub ResetOtherOverlayToggles(excludeKey As String)
        For Each entry In overlayToggles
            If Not entry.Key.Equals(excludeKey, StringComparison.OrdinalIgnoreCase) AndAlso entry.Status Then
                entry.Status = False
                entry.Button.BackColor = SystemColors.ButtonHighlight
                If entry.ResetText IsNot Nothing Then entry.Button.Text = entry.ResetText.Invoke()

                Dim sendstring As String = "Function=OverlayInput" + Tennis24_Settings.ComboBoxValues(entry.ComboIndex) + "Off&Input=" + entry.Template + "&Mix=0"
                SendHTMLtovMix(sendstring)
            End If
        Next
    End Sub

    Private Sub Btn_info_Click(sender As Object, e As EventArgs) Handles Btn_info1.Click, Btn_info2.Click, Btn_info3.Click, Btn_info4.Click
        'blendet Info-Overlay ein und aus
        Dim entry = overlayToggles.First(Function(t) t.Button Is sender)

        ' Reset other toggles first
        ResetOtherOverlayToggles(entry.Key)

        Dim isOn = ToggleStatus(entry)
        SendOverlayCommand(entry, isOn)
        entry.Button.BackColor = If(isOn, Color.Red, SystemColors.ButtonHighlight)
    End Sub

    Private Sub Btn_matchpairing1_Click(sender As Object, e As EventArgs) Handles Btn_matchpairing1.Click, Btn_matchpairing2.Click, Btn_matchpairing3.Click, Btn_matchpairing4.Click
        Pairing()

        Dim entry = overlayToggles.First(Function(t) t.Button Is sender)

        ' Reset other toggles first
        ResetOtherOverlayToggles(entry.Key)

        Dim isOn = ToggleStatus(entry)
        SendOverlayCommand(entry, isOn)
        entry.Button.BackColor = If(isOn, Color.Red, SystemColors.ButtonHighlight)
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
        UpdateScoreDisplays()
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
        ' Scorebug (eigener Layer, nicht Teil der Overlay-Registry)
        scorebugtoggleStatus = False
        Btn_Scorebug.BackColor = SystemColors.ButtonHighlight
        Btn_Scorebug.Text = "Scorebug OFF"

        ' Alle registrierten Overlay-Toggles zurücksetzen (Status + Anzeige). Vorher wurden hier
        ' nur die Name-/Info-Buttons visuell zurückgesetzt; LargeResult/Title/MatchPairing/Home/Away
        ' blieben optisch "aktiv", obwohl ihr Overlay unten bereits ausgeblendet wird - das ist jetzt konsistent.
        For Each entry In overlayToggles
            entry.Status = False
            entry.Button.BackColor = SystemColors.ButtonHighlight
            If entry.ResetText IsNot Nothing Then entry.Button.Text = entry.ResetText.Invoke()
        Next

        ' Sponsor-Paar (eigener Layer, gegenseitig exklusiv, nicht Teil der Registry)
        sponsor1ToggleStatus = False
        sponsor2ToggleStatus = False
        Btn_sponsor1.BackColor = SystemColors.ButtonHighlight
        Btn_sponsor2.BackColor = SystemColors.ButtonHighlight

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
        Dim entry As OverlayToggle

        ' Zugehörige TextBox anhand des geklickten Buttons bestimmen
        Dim textBox As TextBox
        Select Case button.Name
            Case "Btn_freename1"
                textBox = Tennis24_Settings.TextBox4
                entry = GetToggle("freename1")
            Case "Btn_freename2"
                textBox = Tennis24_Settings.TextBox5
                entry = GetToggle("freename2")
            Case "Btn_freename3"
                textBox = Tennis24_Settings.TextBox6
                entry = GetToggle("freename3")
            Case "Btn_freename4"
                textBox = Tennis24_Settings.TextBox7
                entry = GetToggle("freename4")
            Case "Btn_freename5"
                textBox = Tennis24_Settings.TextBox8
                entry = GetToggle("freename5")
            Case "Btn_ref1"
                textBox = Tennis24_Settings.TextBox20
                entry = GetToggle("ref1")
            Case "Btn_ref2"
                textBox = Tennis24_Settings.TextBox21
                entry = GetToggle("ref2")

            Case "Btn_com1"
                textBox = Tennis24_Settings.TextBox22
                entry = GetToggle("com1")
                ' SPEZIELLE BEHANDLUNG für Btn_com1: Template abhängig von Commentator2 setzen
                If String.IsNullOrEmpty(Tennis24_Settings.TextBox23.Text.Trim()) Then
                    nametemplate = "name.gtzip"  ' Nur ein Kommentator
                Else
                    nametemplate = "name2.gtzip" ' Beide Kommentatoren
                End If

            Case "Btn_com2"
                textBox = Tennis24_Settings.TextBox23
                entry = GetToggle("com2")

            Case Else
                MessageBox.Show("No matching button found", "Error24", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Exit Sub
        End Select

        ' Reset other overlays first
        ResetOtherOverlayToggles(entry.Key)

        ' Toggle the status
        Dim currentToggleStatus = ToggleStatus(entry)

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
                sendstring = BuildVmixSetCommand("SetText", nametemplate, "name1.Text", com1Line1)
                SendHTMLtovMix(sendstring)

                ' name2 = Text nach dem Komma von TextBox22
                sendstring = BuildVmixSetCommand("SetText", nametemplate, "name2.Text", com1Line2)
                SendHTMLtovMix(sendstring)

                ' name3 = Commentator2 (vor dem Komma von TextBox23)
                sendstring = BuildVmixSetCommand("SetText", nametemplate, "name3.Text", com2Line1)
                SendHTMLtovMix(sendstring)

                ' name4 = Text nach dem Komma von TextBox23
                sendstring = BuildVmixSetCommand("SetText", nametemplate, "name4.Text", com2Line2)
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
                sendstring = BuildVmixSetCommand("SetText", nametemplate, "name1.Text", line1)
                SendHTMLtovMix(sendstring)

                If line2 <> String.Empty Then
                    sendstring = BuildVmixSetCommand("SetText", nametemplate, "name2.Text", line2)
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