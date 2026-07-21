Imports System.Net

Public Class Tennis26_Scorer

    ' Einzeln ein-/ausschaltbare Spielerdetails (Age/Height/Ranking/Points/Association) für
    ' Lower1/2, LowerHome2/Away2 und Pairing() - vorher ein einzelnes "hidedetails", das alle
    ' Felder gemeinsam ein-/ausblendete.
    Private hideAge As Boolean = False
    Private hideHeight As Boolean = False
    Private hideRank As Boolean = False
    Private hideDataPoints As Boolean = False
    Private hideAssociation As Boolean = False

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
    Private statisticsForm As Tennis26_Statistics

    ' Live-JSON-Datei (Settings-Checkbox3): schreibt den aktuellen Spielstand
    ' geräteunabhängig als Datei, unabhängig von vMix - kann von beliebiger externer
    ' Software (z.B. einem selbst gebauten Python-Server) im Netzwerk verteilt werden,
    ' ohne dass Tennis26 selbst netzwerkweit lauschen muss (siehe TennisJsonExporter.vb).
    Private ReadOnly jsonExporter As New TennisJsonExporter()

    ' Zeitpunkt des letzten Live-JSON-Schreibvorgangs (Event- oder Heartbeat-Schreiben) -
    ' siehe WriteLiveJsonFile/Timer1_Tick. Ermöglicht einen periodischen "Herzschlag" auch
    ' ohne Punkteänderung (z.B. medizinisches Timeout), damit ein externer JSON-Konsument
    ' anhand von "updatedAt" erkennen kann, ob Tennis26 noch aktiv ist oder eingefroren/
    ' abgestürzt ist, statt eine veraltete Datei für einen laufenden Spielstand zu halten.
    Private lastLiveJsonWriteAt As DateTime = DateTime.MinValue
    Private Const LIVE_JSON_HEARTBEAT_SECONDS As Integer = 5
    Private Const LIVE_JSON_FILE_PATH As String = Tennis26_Settings.SETTINGS_DATA_PATH & "\tennis24_live.json"

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

    Private Sub Tennis26_Scorer_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        ' TCP-Verbindung zu vMix sauber trennen, falls die TCP-API gerade aktiv war.
        tcpVmixSender.Dispose()
    End Sub

    Private Sub Tennis26_Scorer_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        InitOverlayToggles()

        freezeSetAdvanceTimer.Interval = 1000

        ' Zeigt die Spielzeit laufend auf Label9 an (Timer1, sekündlich) - anders als die
        ' Live-JSON-Datei, die nur bei jedem Punkt neu geschrieben wird, tickt diese Anzeige
        ' unabhängig davon jede Sekunde mit (siehe Timer1_Tick).
        Timer1.Interval = 1000
        Timer1.Start()

        Tennis26_Settings.SetVariables()

        If Tennis26_Settings.TextBoxValues(50) = 3 Then
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
        hideAge = CheckBox_hidedetails.Checked
        CheckBox_hidehight.Checked = My.Settings.hidehight
        hideHeight = CheckBox_hidehight.Checked
        CheckBox_hiderank.Checked = My.Settings.hiderank
        hideRank = CheckBox_hiderank.Checked
        CheckBox_hidepoints.Checked = My.Settings.hidepoints
        hideDataPoints = CheckBox_hidepoints.Checked
        CheckBox_hideassociation.Checked = My.Settings.hideassociation
        hideAssociation = CheckBox_hideassociation.Checked

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
                .ResetText = Function() "lower" & vbNewLine & GetFullPlayerName(Tennis26_Main.HomePlayer, "HOME")},
            New OverlayToggle With {.Key = "away", .Button = Btn_Name_Away, .Template = "lower_name.gtzip", .ComboIndex = 1,
                .ResetText = Function() "lower" & vbNewLine & GetFullPlayerName(Tennis26_Main.AwayPlayer, "AWAY")},
            New OverlayToggle With {.Key = "home2", .Button = Btn_Name_Home2, .Template = "lower_name.gtzip", .ComboIndex = 1,
                .ResetText = Function() "lower" & vbNewLine & GetFullPlayerName(Tennis26_Main.HomePlayer2, "HOME2")},
            New OverlayToggle With {.Key = "away2", .Button = Btn_Name_Away2, .Template = "lower_name.gtzip", .ComboIndex = 1,
                .ResetText = Function() "lower" & vbNewLine & GetFullPlayerName(Tennis26_Main.AwayPlayer2, "AWAY2")},
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
        Dim sendstring As String = "Function=OverlayInput" + Tennis26_Settings.ComboBoxValues(entry.ComboIndex) + direction + "&Input=" + entry.Template + "&Mix=0"
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

        ' Live-JSON-Datei aktualisieren - unabhängig davon, ob die Statistik-Form gerade
        ' offen ist. Ein Schreibfehler wird hier bewusst nicht gemeldet (das liefe sonst bei
        ' jedem einzelnen Punkt auf denselben Popup hinaus) - ein evtl. dauerhaftes Problem
        ' (z.B. Ordner nicht beschreibbar) zeigt sich bereits beim Matchstart, siehe ResetMatch.
        WriteLiveJsonFile()

        ' Automatische Absturz-/Stromausfall-Sicherung (Btn_recover) - läuft immer, ohne
        ' Einstellungsschalter und unabhängig von Save/Load, weil genau das der Zweck ist:
        ' sie muss auch funktionieren, wenn nie manuell gespeichert wurde.
        SaveAutoRecoveryState()

        If isMatchFinished Then Hidepoints()
    End Sub

    ' Baut einen Snapshot des aktuellen Standes - gemeinsam genutzt von der automatischen
    ' Recovery-Sicherung und dem manuellen "Save Game"-Button.
    Private Function BuildCurrentSnapshot() As MatchStateSnapshot
        Dim homePlayerName = If(String.IsNullOrEmpty(Tennis26_Main.HomePlayer(0)), "HOME", Tennis26_Main.HomePlayer(0))
        Dim awayPlayerName = If(String.IsNullOrEmpty(Tennis26_Main.AwayPlayer(0)), "AWAY", Tennis26_Main.AwayPlayer(0))
        Dim homeSetScores As Integer() = {ParseLabelInt(lbl_home_s1.Text), ParseLabelInt(lbl_home_s2.Text), ParseLabelInt(lbl_home_s3.Text), ParseLabelInt(lbl_home_s4.Text), ParseLabelInt(lbl_home_s5.Text)}
        Dim awaySetScores As Integer() = {ParseLabelInt(lbl_away_s1.Text), ParseLabelInt(lbl_away_s2.Text), ParseLabelInt(lbl_away_s3.Text), ParseLabelInt(lbl_away_s4.Text), ParseLabelInt(lbl_away_s5.Text)}
        Return TennisMatchStateStore.BuildSnapshot(match, homePlayerName, awayPlayerName, homeSetScores, awaySetScores)
    End Function

    ' Schreibt den kompletten Engine-Zustand in die feste Recovery-Datei (siehe
    ' TennisMatchStateStore.AUTO_RECOVERY_FILE_PATH). Fehler werden bewusst verschluckt statt
    ' bei jedem Punkt ein Popup zu zeigen - ein dauerhaftes Problem (z.B. Ordner nicht
    ' beschreibbar) würde auch bei einem manuellen Save/Recover auffallen.
    Private Sub SaveAutoRecoveryState()
        Try
            TennisMatchStateStore.SaveToFile(BuildCurrentSnapshot(), TennisMatchStateStore.AUTO_RECOVERY_FILE_PATH)
        Catch ex As Exception
            ' Bewusst verschluckt - siehe Aufrufstelle
        End Try
    End Sub

    ' Übernimmt einen geladenen Snapshot (Save/Load oder Recover) in die laufende Engine UND
    ' aktualisiert alle betroffenen Anzeigen - Sätze-Labels (die die Engine selbst nicht
    ' kennt, siehe MatchStateSnapshot), Punkte/Spiele/Server sowie Spielernamen, falls auf
    ' diesem Rechner noch keine (oder andere) hinterlegt sind.
    Private Sub ApplySnapshotAndRefresh(snapshot As MatchStateSnapshot)
        TennisMatchStateStore.ApplySnapshot(match, snapshot)

        Tennis26_Main.HomePlayer(0) = snapshot.HomePlayerName
        Tennis26_Main.AwayPlayer(0) = snapshot.AwayPlayerName

        lbl_home_s1.Text = snapshot.HomeSetScores(0).ToString()
        lbl_home_s2.Text = snapshot.HomeSetScores(1).ToString()
        lbl_home_s3.Text = snapshot.HomeSetScores(2).ToString()
        lbl_home_s4.Text = snapshot.HomeSetScores(3).ToString()
        lbl_home_s5.Text = snapshot.HomeSetScores(4).ToString()
        lbl_away_s1.Text = snapshot.AwaySetScores(0).ToString()
        lbl_away_s2.Text = snapshot.AwaySetScores(1).ToString()
        lbl_away_s3.Text = snapshot.AwaySetScores(2).ToString()
        lbl_away_s4.Text = snapshot.AwaySetScores(3).ToString()
        lbl_away_s5.Text = snapshot.AwaySetScores(4).ToString()

        lbl_homepoint.Text = ConvertPointsToTennisScore(homePoints, awayPoints)
        lbl_awaypoint.Text = ConvertPointsToTennisScore(awayPoints, homePoints)
        UpdateGameLabels()
        lbl_current_set.Text = $"Set {currentSet}"
        Lbl_Winner.Visible = isMatchFinished

        BtnChooseService.Enabled = False
        BtnChooseService.Text = "Server festgelegt"

        UpdateServerDisplay()
        UpdateScoreDisplays()
        UpdateScoreBug()
        Label9.Text = FormatMatchDuration(match.MatchElapsed)
    End Sub

    ' Speichert den aktuellen Spielstand unter einem selbst gewählten Namen (Save Game) -
    ' bewusst über einen nativen SaveFileDialog statt einer Eingabebox: erspart eigene
    ' Namens-Bereinigung (ungültige Dateinamenzeichen etc.) und erlaubt trotzdem freie Wahl.
    Private Sub Btn_save_match_Click(sender As Object, e As EventArgs) Handles Btn_save_match.Click
        If Not IO.Directory.Exists(TennisMatchStateStore.SAVES_FOLDER_PATH) Then
            IO.Directory.CreateDirectory(TennisMatchStateStore.SAVES_FOLDER_PATH)
        End If

        Using dialog As New SaveFileDialog()
            dialog.InitialDirectory = TennisMatchStateStore.SAVES_FOLDER_PATH
            dialog.Filter = "Tennis26 saved match (*.xml)|*.xml"
            dialog.DefaultExt = "xml"
            dialog.FileName = $"{Tennis26_Main.HomePlayer(0)} vs {Tennis26_Main.AwayPlayer(0)} {DateTime.Now:yyyy-MM-dd HHmm}.xml"

            If dialog.ShowDialog() = DialogResult.OK Then
                Try
                    TennisMatchStateStore.SaveToFile(BuildCurrentSnapshot(), dialog.FileName)
                    MessageBox.Show($"Match saved as ""{IO.Path.GetFileNameWithoutExtension(dialog.FileName)}"".", "Save Game", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Catch ex As Exception
                    MessageBox.Show($"Could not save the match: {ex.Message}", "Save Game", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If
        End Using
    End Sub

    ' Lädt einen zuvor benannt gespeicherten Spielstand (Load Game).
    Private Sub Btn_load_match_Click(sender As Object, e As EventArgs) Handles Btn_load_match.Click
        Using dialog As New OpenFileDialog()
            dialog.InitialDirectory = TennisMatchStateStore.SAVES_FOLDER_PATH
            dialog.Filter = "Tennis26 saved match (*.xml)|*.xml"

            If dialog.ShowDialog() = DialogResult.OK Then
                Try
                    Dim snapshot = TennisMatchStateStore.LoadFromFile(dialog.FileName)
                    ApplySnapshotAndRefresh(snapshot)
                    MessageBox.Show($"Loaded ""{IO.Path.GetFileNameWithoutExtension(dialog.FileName)}"".", "Load Game", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Catch ex As Exception
                    MessageBox.Show($"Could not load this match: {ex.Message}", "Load Game", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If
        End Using
    End Sub

    ' Stellt den letzten automatisch gesicherten Spielstand wieder her (Recover) - unabhängig
    ' davon, ob je manuell gespeichert wurde (siehe SaveAutoRecoveryState). Gedacht für den
    ' Fall eines Stromausfalls oder Programmabsturzes; fragt vorher nach, da der aktuell
    ' laufende Spielstand dabei überschrieben wird.
    Private Sub Btn_recover_Click(sender As Object, e As EventArgs) Handles Btn_recover.Click
        If Not IO.File.Exists(TennisMatchStateStore.AUTO_RECOVERY_FILE_PATH) Then
            MessageBox.Show("No automatically saved match state was found to recover.", "Recover", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Try
            Dim snapshot = TennisMatchStateStore.LoadFromFile(TennisMatchStateStore.AUTO_RECOVERY_FILE_PATH)
            Dim savedAtLocal = snapshot.SavedAt.ToLocalTime()

            Dim confirmResult = MessageBox.Show(
                "Recover the last automatically saved match state?" & vbNewLine & vbNewLine &
                $"Match: {snapshot.HomePlayerName} vs {snapshot.AwayPlayerName}" & vbNewLine &
                $"Saved at: {savedAtLocal:yyyy-MM-dd HH:mm:ss}" & vbNewLine & vbNewLine &
                "This will overwrite the match currently in progress. Use this after a power outage or program crash.",
                "Recover Match State", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)

            If confirmResult = DialogResult.Yes Then
                ApplySnapshotAndRefresh(snapshot)
                MessageBox.Show("Match state recovered.", "Recover", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        Catch ex As Exception
            MessageBox.Show($"Could not read the saved match state: {ex.Message}", "Recover", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Zwischeneinstieg: öffnet die Eingabe-Form modal, übernimmt das Ergebnis bei "Apply"
    ' über dieselbe ApplySnapshotAndRefresh() wie Save/Load/Recover - keine eigene
    ' Anwende-Logik nötig (siehe Tennis26_MidMatchEntry.vb).
    Private Sub Btn_betweenentry_Click(sender As Object, e As EventArgs) Handles Btn_betweenentry.Click
        Using entryForm As New Tennis26_MidMatchEntry()
            If entryForm.ShowDialog(Me) = DialogResult.OK Then
                ApplySnapshotAndRefresh(entryForm.ResultSnapshot)
            End If
        End Using
    End Sub

    ' Schreibt die Live-JSON-Datei einmal beim Matchstart (Settings-Checkbox3), damit z.B.
    ' Spielernamen schon vor dem ersten Punkt auf einer externen Anzeige verfügbar sind.
    ' Anders als die Datei-Updates pro Punkt wird ein Fehler hier direkt gemeldet, weil er
    ' nur einmal pro Matchstart auftritt statt bei jedem Punkt erneut.
    Private Sub WriteLiveJsonFileOnMatchStart()
        WriteLiveJsonFile(showErrorOnFailure:=True)
    End Sub

    ' Zentraler Schreibpunkt für die Live-JSON-Datei - von drei Stellen aufgerufen:
    ' 1) WriteLiveJsonFileOnMatchStart (einmal beim Matchstart)
    ' 2) UpdateScoreDisplays (bei jeder Zustandsänderung, z.B. jeder Punkt)
    ' 3) Timer1_Tick (Heartbeat alle LIVE_JSON_HEARTBEAT_SECONDS Sekunden, siehe dort) -
    '    stellt sicher, dass "updatedAt" auch bei einer langen Spielunterbrechung ohne
    '    Punkteänderung (z.B. medizinisches Timeout) regelmässig weiterläuft, damit ein
    '    externer JSON-Konsument eine eingefrorene/abgestürzte Anwendung von einer echten
    '    Spielpause unterscheiden kann.
    Private Sub WriteLiveJsonFile(Optional showErrorOnFailure As Boolean = False)
        If Not Tennis26_Settings.CheckBoxValues(3) Then Return

        If jsonExporter.WriteToFile(LIVE_JSON_FILE_PATH, BuildLiveStateJson()) Then
            lastLiveJsonWriteAt = DateTime.UtcNow
        ElseIf showErrorOnFailure Then
            MessageBox.Show(
                $"Live-JSON-Datei konnte nicht geschrieben werden ({LIVE_JSON_FILE_PATH}):" & vbNewLine & vbNewLine &
                jsonExporter.LastError,
                "Live-JSON-Datei", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If
    End Sub

    Private Function ParseLabelInt(text As String) As Integer
        Dim result As Integer
        Integer.TryParse(text, result)
        Return result
    End Function

    ' "HH:MM:SS", ohne Stunden-Deckelung bei 24h (anders als TimeSpan.ToString("hh\:mm\:ss")) -
    ' für ein episch langes Match theoretisch relevant, kostet hier nichts.
    Private Function FormatMatchDuration(elapsed As TimeSpan) As String
        Return $"{Math.Floor(elapsed.TotalHours):00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}"
    End Function

    ' Zeigt die Spielzeit sekündlich auf Label9 an - unabhängig vom Punktgeschehen, im
    ' Gegensatz zur Live-JSON-Datei (die nur bei jedem Punkt neu geschrieben wird). Stoppt
    ' bei Matchende, damit die Anzeige auf der finalen Spielzeit stehen bleibt statt nach
    ' Spielende weiterzulaufen; ResetMatch startet ihn wieder.
    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Label9.Text = FormatMatchDuration(match.MatchElapsed)

        ' Heartbeat: hält "updatedAt" in der Live-JSON-Datei auch ohne Punkteänderung aktuell
        ' (siehe WriteLiveJsonFile). Stoppt automatisch mit Timer1 bei Matchende - die letzte,
        ' vom eigentlichen Matchende-Ereignis geschriebene JSON bleibt dann unverändert stehen.
        If DateTime.UtcNow.Subtract(lastLiveJsonWriteAt).TotalSeconds >= LIVE_JSON_HEARTBEAT_SECONDS Then
            WriteLiveJsonFile()
        End If

        If isMatchFinished Then Timer1.Stop()
    End Sub

    ' Baut den kompletten aktuellen Spielstand als JSON - unabhängig von vMix, für beliebige
    ' externe Software geeignet, die die Live-JSON-Datei ausliest (siehe TennisJsonExporter.vb).
    ' Enthält bewusst auch die vollständigen Spielerdaten (Tennis26_Main) und dieselben
    ' Statistik-Werte wie das Statistik-Fenster (Tennis26_Statistics), damit eine externe
    ' Anzeige keine zweite Datenquelle braucht.
    Private Function BuildLiveStateJson() As String
        Dim homePlayerName As String = If(String.IsNullOrEmpty(Tennis26_Main.HomePlayer(0)), "HOME", Tennis26_Main.HomePlayer(0))
        Dim awayPlayerName As String = If(String.IsNullOrEmpty(Tennis26_Main.AwayPlayer(0)), "AWAY", Tennis26_Main.AwayPlayer(0))

        Dim homeSetScores As Integer() = {ParseLabelInt(lbl_home_s1.Text), ParseLabelInt(lbl_home_s2.Text), ParseLabelInt(lbl_home_s3.Text), ParseLabelInt(lbl_home_s4.Text), ParseLabelInt(lbl_home_s5.Text)}
        Dim awaySetScores As Integer() = {ParseLabelInt(lbl_away_s1.Text), ParseLabelInt(lbl_away_s2.Text), ParseLabelInt(lbl_away_s3.Text), ParseLabelInt(lbl_away_s4.Text), ParseLabelInt(lbl_away_s5.Text)}

        ' Service-Games/Punkte-Prozentsätze exakt wie im Statistik-Fenster berechnet
        ' (siehe Tennis26_Statistics.RefreshStatistics/UpdateBreakPointRows) - hier dupliziert,
        ' da die Statistik-Form nur die Anzeige kennt, nicht die JSON-Ausgabe.
        Dim homeServiceGamesTotal = homeServiceGamesWon + match.AwayBreaks
        Dim awayServiceGamesTotal = awayServiceGamesWon + match.HomeBreaks
        Dim homeServiceWinPct = If(homeServiceGamesTotal > 0, Math.Round((homeServiceGamesWon / homeServiceGamesTotal) * 100, 1), 0)
        Dim awayServiceWinPct = If(awayServiceGamesTotal > 0, Math.Round((awayServiceGamesWon / awayServiceGamesTotal) * 100, 1), 0)

        Dim totalPointsPlayed = homeTotalPoints + awayTotalPoints
        Dim homePointsWinPct = If(totalPointsPlayed > 0, Math.Round((homeTotalPoints / totalPointsPlayed) * 100, 1), 0)
        Dim awayPointsWinPct = If(totalPointsPlayed > 0, Math.Round((awayTotalPoints / totalPointsPlayed) * 100, 1), 0)

        ' Abgewehrte Breakbälle = Chancen des Gegners, die er nicht nutzte
        Dim homeBreakPointsSaved = match.AwayBreakPointsTotal - match.AwayBreakPointsConverted
        Dim awayBreakPointsSaved = match.HomeBreakPointsTotal - match.HomeBreakPointsConverted

        ' Kultur-invariant formatieren: Math.Round liefert ein Double, und unter deutscher
        ' Kultur würde ToString() ein Komma statt Punkt als Dezimaltrennzeichen liefern -
        ' das würde die JSON-Datei kaputt machen (nur bei den Ganzzahl-Feldern unkritisch).
        Dim inv = Globalization.CultureInfo.InvariantCulture

        Dim homePlayerObj As New JsonObjectBuilder()
        homePlayerObj.AddString("name", Tennis26_Main.HomePlayer(0)) _
                      .AddString("firstName", Tennis26_Main.HomePlayer(1)) _
                      .AddString("country", Tennis26_Main.HomePlayer(2)) _
                      .AddString("countryISO3", Tennis26_Main.HomePlayer(3)) _
                      .AddString("age", Tennis26_Main.HomePlayer(4)) _
                      .AddString("height", Tennis26_Main.HomePlayer(5)) _
                      .AddString("data1", Tennis26_Main.HomePlayer(6)) _
                      .AddString("data2", Tennis26_Main.HomePlayer(7)) _
                      .AddString("data3", Tennis26_Main.HomePlayer(8))

        Dim awayPlayerObj As New JsonObjectBuilder()
        awayPlayerObj.AddString("name", Tennis26_Main.AwayPlayer(0)) _
                      .AddString("firstName", Tennis26_Main.AwayPlayer(1)) _
                      .AddString("country", Tennis26_Main.AwayPlayer(2)) _
                      .AddString("countryISO3", Tennis26_Main.AwayPlayer(3)) _
                      .AddString("age", Tennis26_Main.AwayPlayer(4)) _
                      .AddString("height", Tennis26_Main.AwayPlayer(5)) _
                      .AddString("data1", Tennis26_Main.AwayPlayer(6)) _
                      .AddString("data2", Tennis26_Main.AwayPlayer(7)) _
                      .AddString("data3", Tennis26_Main.AwayPlayer(8))

        ' Doppel-Partner (HomePlayer2/AwayPlayer2) - immer mitgegeben, unabhängig von
        ' isDoublesMatch, damit ein externer Konsument nicht zwei verschiedene JSON-Formen
        ' unterscheiden muss (bei Einzel einfach mit leeren Strings befüllt).
        Dim homePlayer2Obj As New JsonObjectBuilder()
        homePlayer2Obj.AddString("name", Tennis26_Main.HomePlayer2(0)) _
                       .AddString("firstName", Tennis26_Main.HomePlayer2(1)) _
                       .AddString("country", Tennis26_Main.HomePlayer2(2)) _
                       .AddString("countryISO3", Tennis26_Main.HomePlayer2(3)) _
                       .AddString("age", Tennis26_Main.HomePlayer2(4)) _
                       .AddString("height", Tennis26_Main.HomePlayer2(5)) _
                       .AddString("data1", Tennis26_Main.HomePlayer2(6)) _
                       .AddString("data2", Tennis26_Main.HomePlayer2(7)) _
                       .AddString("data3", Tennis26_Main.HomePlayer2(8))

        Dim awayPlayer2Obj As New JsonObjectBuilder()
        awayPlayer2Obj.AddString("name", Tennis26_Main.AwayPlayer2(0)) _
                       .AddString("firstName", Tennis26_Main.AwayPlayer2(1)) _
                       .AddString("country", Tennis26_Main.AwayPlayer2(2)) _
                       .AddString("countryISO3", Tennis26_Main.AwayPlayer2(3)) _
                       .AddString("age", Tennis26_Main.AwayPlayer2(4)) _
                       .AddString("height", Tennis26_Main.AwayPlayer2(5)) _
                       .AddString("data1", Tennis26_Main.AwayPlayer2(6)) _
                       .AddString("data2", Tennis26_Main.AwayPlayer2(7)) _
                       .AddString("data3", Tennis26_Main.AwayPlayer2(8))

        Dim homeObj As New JsonObjectBuilder()
        homeObj.AddString("name", homePlayerName) _
               .AddRaw("player", homePlayerObj.ToString()) _
               .AddRaw("player2", homePlayer2Obj.ToString()) _
               .AddString("points", ConvertPointsToTennisScore(homePoints, awayPoints)) _
               .AddInt("games", homeGames) _
               .AddInt("sets", homeSets) _
               .AddBool("serving", isHomeServing) _
               .AddIntArray("setScores", homeSetScores) _
               .AddInt("breaks", homeBreaks) _
               .AddInt("breakPointsWon", match.HomeBreakPointsConverted) _
               .AddInt("breakPointsTotal", match.HomeBreakPointsTotal) _
               .AddInt("breakPointsSaved", homeBreakPointsSaved) _
               .AddInt("miniBreaks", match.HomeMiniBreaks) _
               .AddInt("totalPoints", homeTotalPoints) _
               .AddInt("serviceGamesWon", homeServiceGamesWon) _
               .AddInt("serviceGamesTotal", homeServiceGamesTotal) _
               .AddRaw("serviceWinPct", homeServiceWinPct.ToString(inv)) _
               .AddInt("tiebreaksWon", homeTiebreaksWon) _
               .AddRaw("pointsWinPct", homePointsWinPct.ToString(inv))

        Dim awayObj As New JsonObjectBuilder()
        awayObj.AddString("name", awayPlayerName) _
               .AddRaw("player", awayPlayerObj.ToString()) _
               .AddRaw("player2", awayPlayer2Obj.ToString()) _
               .AddString("points", ConvertPointsToTennisScore(awayPoints, homePoints)) _
               .AddInt("games", awayGames) _
               .AddInt("sets", awaySets) _
               .AddBool("serving", Not isHomeServing) _
               .AddIntArray("setScores", awaySetScores) _
               .AddInt("breaks", awayBreaks) _
               .AddInt("breakPointsWon", match.AwayBreakPointsConverted) _
               .AddInt("breakPointsTotal", match.AwayBreakPointsTotal) _
               .AddInt("breakPointsSaved", awayBreakPointsSaved) _
               .AddInt("miniBreaks", match.AwayMiniBreaks) _
               .AddInt("totalPoints", awayTotalPoints) _
               .AddInt("serviceGamesWon", awayServiceGamesWon) _
               .AddInt("serviceGamesTotal", awayServiceGamesTotal) _
               .AddRaw("serviceWinPct", awayServiceWinPct.ToString(inv)) _
               .AddInt("tiebreaksWon", awayTiebreaksWon) _
               .AddRaw("pointsWinPct", awayPointsWinPct.ToString(inv))

        Dim matchType = If(Tennis26_Settings.TextBoxValues(50) = "5", "Best of 5", "Best of 3")

        ' Spielzeit: läuft ab dem allerersten Punkt (siehe TennisMatchEngine.RegisterPoint).
        ' matchStartTime roh mitgeben, damit eine externe Anzeige selbst weiterticken kann,
        ' matchDuration/-Seconds zusätzlich fertig berechnet für einfache Konsumenten (z.B.
        ' ein vMix-Textfeld, das nicht selbst rechnen kann).
        Dim matchElapsed = match.MatchElapsed
        Dim matchDurationText = FormatMatchDuration(matchElapsed)

        Dim root As New JsonObjectBuilder()
        root.AddRaw("home", homeObj.ToString()) _
            .AddRaw("away", awayObj.ToString()) _
            .AddInt("currentSet", currentSet) _
            .AddString("matchType", matchType) _
            .AddBool("isTiebreak", isTiebreak) _
            .AddBool("isMatchTiebreak", isMatchTiebreakSet) _
            .AddBool("isMatchFinished", isMatchFinished) _
            .AddBool("isMidMatchEntry", match.IsMidMatchEntry) _
            .AddBool("isDoublesMatch", IsDoublesMatch()) _
            .AddString("breakPointHolder", match.BreakPointHolder()) _
            .AddInt("breakPointCount", match.CurrentBreakPointCount()) _
            .AddBool("sidesSwapped", match.AreSidesCurrentlySwapped()) _
            .AddInt("longestGame", longestGame) _
            .AddString("matchStartTime", If(match.MatchStartTime.HasValue, match.MatchStartTime.Value.ToString("o"), "")) _
            .AddInt("matchDurationSeconds", CInt(matchElapsed.TotalSeconds)) _
            .AddString("matchDuration", matchDurationText) _
            .AddString("updatedAt", DateTime.UtcNow.ToString("o")) _
            .AddInt("heartbeatIntervalSeconds", LIVE_JSON_HEARTBEAT_SECONDS)

        ' vMix erwartet bei einer JSON Data Source zwingend ein Array von Objekten (auch bei
        ' nur einer "Zeile") - ein einzelnes JSON-Objekt ohne umschliessende [] erkennt vMix
        ' nicht und zeigt nur ein leeres Fallback-Feld "#text" ohne Spalten an.
        Return "[" & root.ToString() & "]"
    End Function

    ' Die Statistik-Anzeige liegt in einer eigenen Form (Tennis26_Statistics) und wird nur
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

    ' Individueller Anzeigename "Vorname Nachname" eines einzelnen Spielers (mit Fallback,
    ' solange noch keiner ausgewählt ist) - für die vier Namen-Einblender-Buttons (Home/
    ' Home2/Away/Away2), die je genau einen Spieler zeigen, nicht das ganze Team.
    Private Function GetFullPlayerName(player As String(), fallback As String) As String
        Return If(String.IsNullOrEmpty(player(0)), fallback, player(1) & " " & player(0))
    End Function

    ' Team-Anzeigename bei Doppel: beide Nachnamen kombiniert, bei Einzel unverändert nur der
    ' eine Nachname. Separator standardmässig ein Linefeed (für die Punkte-Buttons
    ' btn_homepoint/awaypoint, wo der 2. Name bei langen Namen/grosser Schrift sonst neben dem
    ' 1. abgeschnitten wird) - LargeResult() übergibt stattdessen " / ", da dort als grossflächige
    ' Grafikeinblendung genug Platz für eine Zeile ist.
    Private Function GetTeamDisplayName(player As String(), player2 As String(), fallback As String, Optional separator As String = vbNewLine) As String
        Dim name1 = If(String.IsNullOrEmpty(player(0)), fallback, player(0))
        If IsDoublesMatch() AndAlso Not String.IsNullOrEmpty(player2(0)) Then
            Return name1 & separator & player2(0)
        End If
        Return name1
    End Function

    ' Kombinierter Länder-Text für Doppel-Grafiken ohne Flaggenfeld (z.B. Large Result) -
    ' "USA" wenn beide Partner aus demselben Land kommen, sonst "USA/CHE".
    Private Function GetTeamCountryText(country1 As String, country2 As String) As String
        If String.IsNullOrEmpty(country2) OrElse country1 = country2 Then
            Return country1
        End If
        Return country1 & "/" & country2
    End Function

    ' "large_result.gtzip" (Einzel) bzw. "large_result_double.gtzip" (Doppel) - eigene Vorlage
    ' für Doppel (2 Namen pro Seite + kombinierter Länder-Text statt Flagge, siehe LargeResult()).
    ' Von LargeResult() (Datenversand) UND Btn_LargeResult_Click (Ein-/Ausblenden) verwendet,
    ' damit beide immer dieselbe Vorlage ansprechen.
    Private Function GetLargeResultTemplate() As String
        Return If(IsDoublesMatch(), "large_result_double.gtzip", "large_result.gtzip")
    End Function

    ' "match_pairing.gtzip" (Einzel) bzw. "match_pairing_double.gtzip" (Doppel) - nur für den
    ' Btn_matchpairing-Slot (nicht matchpairing1-4, für die es keine Doppel-Vorlage gibt).
    ' Pairing() sendet die h2xxx/a2xxx-Felder (2. Doppelpartner) bereits unabhängig vom
    ' Template, sobald IsDoublesMatch() true ist - ohne diesen Vorlagen-Wechsel liefen diese
    ' Felder aber ins Leere, weil match_pairing.gtzip sie gar nicht kennt (gleicher Fehler wie
    ' vorher bei Large Result).
    Private Function GetMatchPairingTemplate() As String
        Return If(IsDoublesMatch(), "match_pairing_double.gtzip", "match_pairing.gtzip")
    End Function

    ' Team-Name für den Scorebug (hname/aname) bei Doppel: "Nachname1/Nachname2" - anders als
    ' bei Large Result bleibt die Scorebug-Vorlage dieselbe, das Feld ist also deutlich
    ' schmaler. Überschreitet die Summe beider Nachnamen 10 Zeichen, werden beide auf je 4
    ' Zeichen gekürzt (z.B. "Federer/Nadal" -> "Fede/Nada"), damit die kompakte Grafik nicht
    ' überläuft. Bei Einzel unverändert nur der eine Nachname.
    Private Function GetScorebugTeamName(player As String(), player2 As String(), fallback As String) As String
        Dim name1 = If(String.IsNullOrEmpty(player(0)), fallback, player(0))
        If Not IsDoublesMatch() OrElse String.IsNullOrEmpty(player2(0)) Then Return name1

        Dim name2 = player2(0)
        If name1.Length + name2.Length > 10 Then
            name1 = name1.Substring(0, Math.Min(6, name1.Length))
            name2 = name2.Substring(0, Math.Min(6, name2.Length))
        End If
        Return name1 & " / " & name2
    End Function

    Private Sub UpdateButtonNames()
        ' Spielernamen abrufen - bei Doppel als kombinierter Team-Name (siehe GetTeamDisplayName)
        Dim homePlayerName As String = GetTeamDisplayName(Tennis26_Main.HomePlayer, Tennis26_Main.HomePlayer2, "HOME")
        Dim awayPlayerName As String = GetTeamDisplayName(Tennis26_Main.AwayPlayer, Tennis26_Main.AwayPlayer2, "AWAY")

        ' Die Spaltenüberschriften der Statistik setzt Tennis26_Statistics selbst
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

        ' Namen-Einblender Home/Home2/Away/Away2 zeigen je den vollen Namen (Vorname Nachname)
        ' genau eines Spielers - kein Team-Zusammenzug, im Gegensatz zu homePlayerName/
        ' awayPlayerName oben (die sind nur für die Punkte-Buttons gedacht).
        Btn_Name_Home.Text = "lower" & vbNewLine & GetFullPlayerName(Tennis26_Main.HomePlayer, "HOME")
        Btn_Name_Away.Text = "lower" & vbNewLine & GetFullPlayerName(Tennis26_Main.AwayPlayer, "AWAY")
        Btn_Name_Home2.Text = "lower" & vbNewLine & GetFullPlayerName(Tennis26_Main.HomePlayer2, "HOME2")
        Btn_Name_Away2.Text = "lower" & vbNewLine & GetFullPlayerName(Tennis26_Main.AwayPlayer2, "AWAY2")
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
                SendHTMLtovMix("Function=OverlayInput" + Tennis26_Settings.ComboBoxValues(2) + "In&Input=scorebug_" & displayedScorebugSet.ToString() & "s.gtzip&Mix=0")
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

        ' Timer1 stoppt bei Matchende (siehe Timer1_Tick) - bei neuem Match wieder starten
        ' und die Anzeige sofort auf 00:00:00 zurücksetzen.
        Timer1.Start()
        Label9.Text = FormatMatchDuration(match.MatchElapsed)

        ' Match-Tiebreak-Einstellungen aus den Settings übernehmen
        match.MatchTiebreakEnabled = Tennis26_Settings.CheckBoxValues(1)
        Dim matchTiebreakTarget As Integer
        If Integer.TryParse(Tennis26_Settings.TextBoxValues(42), matchTiebreakTarget) AndAlso matchTiebreakTarget > 0 Then
            match.MatchTiebreakTarget = matchTiebreakTarget
        Else
            match.MatchTiebreakTarget = 10
        End If

        ' Freeze-Set-Einstellung aus den Settings übernehmen
        freezeSetEnabled = Tennis26_Settings.CheckBoxValues(2)
        displayedScorebugSet = 1
        freezeSetAdvanceTimer.Stop()

        ' Gelb-Markierungen gewonnener Sätze aus einem vorherigen Match zurücksetzen
        If freezeSetEnabled Then
            For setNumber As Integer = 1 To 5
                SetSetNumberColour("home", setNumber, NORMAL_SET_COLOUR)
                SetSetNumberColour("away", setNumber, NORMAL_SET_COLOUR)
            Next
        End If

        WriteLiveJsonFileOnMatchStart()

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
        UpdateMatchTypeButtonVisibility()
    End Sub

    ' Blendet die nur für Einzel bzw. nur für Doppel sinnvollen Buttons passend aus - läuft
    ' bei jedem Matchstart (ResetMatch, also auch bei "New Match"), nicht nur einmalig beim
    ' Öffnen des Scorers, da CheckBox1 (Doppel-Flag) sich zwischen zwei Matches ändern kann.
    Private Sub UpdateMatchTypeButtonVisibility()
        Dim isDoubles = IsDoublesMatch()

        Btn_Name_Home2.Visible = isDoubles
        Btn_Name_Away2.Visible = isDoubles

        Btn_matchpairing1.Visible = Not isDoubles
        Btn_matchpairing2.Visible = Not isDoubles
        Btn_matchpairing3.Visible = Not isDoubles
        Btn_matchpairing4.Visible = Not isDoubles
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

        Dim sendstring = "Function=OverlayInput" + Tennis26_Settings.ComboBoxValues(1) + "Off&Mix=0"
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
        Dim setsToWin As Integer = Math.Ceiling(Tennis26_Settings.TextBoxValues(50) / 2.0)

        If homeSets = setsToWin OrElse awaySets = setsToWin Then
            ' Match ist beendet
            isMatchFinished = True
            Dim homePlayerName As String = If(String.IsNullOrEmpty(Tennis26_Main.HomePlayer(0)), "HOME", Tennis26_Main.HomePlayer(0))
            Dim awayPlayerName As String = If(String.IsNullOrEmpty(Tennis26_Main.AwayPlayer(0)), "AWAY", Tennis26_Main.AwayPlayer(0))

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

    ' Doppel-Umschalter lebt in Main (CheckBox1, neben der Spielerauswahl) statt in den
    ' Settings - passt inhaltlich besser dorthin, wo auch die Partner ausgewählt werden.
    Private Function IsDoublesMatch() As Boolean
        Return Tennis26_Main.CheckBox1.Checked
    End Function

    Private Sub SendDataToGraphicsEngine()
        ' Scorebug(s) aktualisieren
        Dim scorebugtitles() As String = {"scorebug_1s.gtzip", "scorebug_2s.gtzip", "scorebug_3s.gtzip", "scorebug_4s.gtzip", "scorebug_5s.gtzip"}
        Dim isDoubles As Boolean = IsDoublesMatch()

        For Each scorebugtitle As String In scorebugtitles
            Dim sendstring() As String
            Dim index As Integer = 0

            ' 4 zusätzliche Slots für h2name/a2name/h2country/a2country (Doppel-Partner) -
            ' bleiben bei Einzel schlicht Nothing und werden von SendToGraphicsEngine
            ' übersprungen (siehe dort).
            ReDim sendstring(21)

            sendstring(index) = BuildVmixSetCommand("SetText", scorebugtitle, "hpoint.Text", lbl_homepoint.Text)
            index += 1
            sendstring(index) = BuildVmixSetCommand("SetText", scorebugtitle, "apoint.Text", lbl_awaypoint.Text)
            index += 1

            Dim homePlayerName As String = GetScorebugTeamName(Tennis26_Main.HomePlayer, Tennis26_Main.HomePlayer2, "HOME")
            Dim awayPlayerName As String = GetScorebugTeamName(Tennis26_Main.AwayPlayer, Tennis26_Main.AwayPlayer2, "AWAY")

            sendstring(index) = BuildVmixSetCommand("SetText", scorebugtitle, "hname.Text", homePlayerName)
            index += 1
            sendstring(index) = BuildVmixSetCommand("SetText", scorebugtitle, "aname.Text", awayPlayerName)
            index += 1

            Dim homeCountryISO3 As String = If(String.IsNullOrEmpty(Tennis26_Main.HomePlayer(3)), "HOM", Tennis26_Main.HomePlayer(3))
            Dim awayCountryISO3 As String = If(String.IsNullOrEmpty(Tennis26_Main.AwayPlayer(3)), "AWY", Tennis26_Main.AwayPlayer(3))

            sendstring(index) = BuildVmixSetCommand("SetText", scorebugtitle, "hcountry.Text", homeCountryISO3)
            index += 1
            sendstring(index) = BuildVmixSetCommand("SetText", scorebugtitle, "acountry.Text", awayCountryISO3)
            index += 1

            If isDoubles Then
                Dim homePlayer2Name As String = If(String.IsNullOrEmpty(Tennis26_Main.HomePlayer2(0)), "", Tennis26_Main.HomePlayer2(0))
                Dim awayPlayer2Name As String = If(String.IsNullOrEmpty(Tennis26_Main.AwayPlayer2(0)), "", Tennis26_Main.AwayPlayer2(0))
                Dim homePlayer2CountryISO3 As String = If(String.IsNullOrEmpty(Tennis26_Main.HomePlayer2(3)), "", Tennis26_Main.HomePlayer2(3))
                Dim awayPlayer2CountryISO3 As String = If(String.IsNullOrEmpty(Tennis26_Main.AwayPlayer2(3)), "", Tennis26_Main.AwayPlayer2(3))

                sendstring(index) = BuildVmixSetCommand("SetText", scorebugtitle, "h2name.Text", homePlayer2Name)
                index += 1
                sendstring(index) = BuildVmixSetCommand("SetText", scorebugtitle, "a2name.Text", awayPlayer2Name)
                index += 1
                sendstring(index) = BuildVmixSetCommand("SetText", scorebugtitle, "h2country.Text", homePlayer2CountryISO3)
                index += 1
                sendstring(index) = BuildVmixSetCommand("SetText", scorebugtitle, "a2country.Text", awayPlayer2CountryISO3)
                index += 1
            End If

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
        ' Grosse Ergebnisanzeige aktualisieren - bei Doppel eine eigene Vorlage
        ' (large_result_double.gtzip statt large_result.gtzip, siehe GetLargeResultTemplate())
        ' mit kombiniertem Team-Namen ("Nachname1 / Nachname2") und Länder-Text ("USA" bzw.
        ' "USA/CHE") statt Einzelspieler-Vorname+Flagge - für 2 Namen pro Seite ist in der
        ' Einzel-Vorlage kein Platz, und eine einzelne Flagge würde nur einen der beiden
        ' Partner repräsentieren.
        Dim scorebugtitle As String = GetLargeResultTemplate()
        Dim sendstring As String

        If IsDoublesMatch() Then
            Dim homeTeamName As String = GetTeamDisplayName(Tennis26_Main.HomePlayer, Tennis26_Main.HomePlayer2, "HOME", " / ")
            Dim awayTeamName As String = GetTeamDisplayName(Tennis26_Main.AwayPlayer, Tennis26_Main.AwayPlayer2, "AWAY", " / ")
            Dim homeCountryText As String = GetTeamCountryText(Tennis26_Main.HomePlayer(3), Tennis26_Main.HomePlayer2(3))
            Dim awayCountryText As String = GetTeamCountryText(Tennis26_Main.AwayPlayer(3), Tennis26_Main.AwayPlayer2(3))

            sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "hname.Text", homeTeamName) : SendHTMLtovMix(sendstring)
            sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "aname.Text", awayTeamName) : SendHTMLtovMix(sendstring)
            sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "hcountry.Text", homeCountryText) : SendHTMLtovMix(sendstring)
            sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "acountry.Text", awayCountryText) : SendHTMLtovMix(sendstring)
        Else
            Dim homePlayerName As String = If(String.IsNullOrEmpty(Tennis26_Main.HomePlayer(0)), "HOME", Tennis26_Main.HomePlayer(1) & " " & Tennis26_Main.HomePlayer(0))
            Dim awayPlayerName As String = If(String.IsNullOrEmpty(Tennis26_Main.AwayPlayer(0)), "AWAY", Tennis26_Main.AwayPlayer(1) & " " & Tennis26_Main.AwayPlayer(0))
            Dim homecountry As String = If(String.IsNullOrEmpty(Tennis26_Main.HomePlayer(0)), "HOME", Tennis26_Main.HomePlayer(3))
            Dim awaycountry As String = If(String.IsNullOrEmpty(Tennis26_Main.AwayPlayer(0)), "AWAY", Tennis26_Main.AwayPlayer(3))
            sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "hname.Text", homePlayerName) : SendHTMLtovMix(sendstring)
            sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "aname.Text", awayPlayerName) : SendHTMLtovMix(sendstring)
            sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "hcountry.Text", homecountry) : SendHTMLtovMix(sendstring)
            sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "acountry.Text", awaycountry) : SendHTMLtovMix(sendstring)

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
        End If

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

    ' Wahl zwischen HTTP- und TCP-API (Settings-RadioButton3/4) wird bei jedem Aufruf frisch
    ' aus RadioButtonValues(4) gelesen (True = TCP) statt einmalig gecacht - so wirkt eine
    ' Settings-Änderung sofort, ohne den Scorer neu zu starten. Die eigentliche Übersetzung
    ' des "Function=X&Param=Y&..."-Strings ins jeweilige Protokoll steckt in
    ' VmixHttpSender/VmixTcpSender (siehe IVmixSender).
    Private ReadOnly httpVmixSender As New VmixHttpSender()
    Private ReadOnly tcpVmixSender As New VmixTcpSender()

    Public Sub SendHTMLtovMix(ByVal command As String)
        Dim useTcp As Boolean = Tennis26_Settings.RadioButtonValues(4)
        Dim sender As IVmixSender = If(useTcp, CType(tcpVmixSender, IVmixSender), httpVmixSender)

        ' Laufzeitmessung: vom Absenden bis die (letzte) Antwort da ist - Send() wartet
        ' intern bereits synchron auf die vollständige Antwort (HTTP-Response bzw. die
        ' vMix-TCP-Bestätigungszeile), die Zeit hier umschliesst also genau das.
        Dim vmixSendTimer = Stopwatch.StartNew()
        Dim result As String = sender.Send(command)
        vmixSendTimer.Stop()

        Dim protocolLabel As String = If(useTcp, "TCP", "HTTP")
        ' ElapsedMilliseconds rundet auf ganze ms ab - bei lokalem vMix (v.a. TCP) liegt die
        ' tatsächliche Zeit meist deutlich darunter und würde fast immer als "0 ms" erscheinen.
        ' TotalMilliseconds nutzt die volle Stopwatch-Auflösung (auf Windows üblich: deutlich
        ' unter 1µs Ticks) für einen aussagekräftigen Vergleich HTTP vs. TCP.
        Label11.Text = $"{protocolLabel}: {vmixSendTimer.Elapsed.TotalMilliseconds:F4} ms"

        Label12.Text = sender.LastCommand
        Label7.Text = result
    End Sub

    Private Sub Btn_exit_Click(sender As Object, e As EventArgs) Handles Btn_exit.Click
        'beendet das programm und öffnet das hauptfenster
        Me.Close()
        Tennis26_Main.Show()
    End Sub

    Private Sub Btn_statistics_Click(sender As Object, e As EventArgs) Handles Btn_statistics.Click
        ' Statistik-Fenster öffnen bzw. nach vorne holen (nicht-modal, damit weitergezählt
        ' werden kann, während die Statistik offen ist)
        If statisticsForm Is Nothing OrElse statisticsForm.IsDisposed Then
            statisticsForm = New Tennis26_Statistics()
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
            sendstring = "Function=OverlayInput" + Tennis26_Settings.ComboBoxValues(2) + "In&Input=scorebug_" & displayedScorebugSet.ToString() & "s.gtzip&Mix=0"
            Btn_Scorebug.BackColor = Color.Red
            Btn_Scorebug.Text = $"Scorebug ON (Set {displayedScorebugSet})"
        Else
            sendstring = "Function=OverlayInput" + Tennis26_Settings.ComboBoxValues(2) + "Out&Input=scorebug_" & displayedScorebugSet.ToString() & "s.gtzip&Mix=0"
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
        entry.Template = GetLargeResultTemplate() ' Einzel/Doppel-Vorlage, siehe LargeResult()

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
            Dim colour As String = Tennis26_Settings.TextBoxValues(43)
            Return If(String.IsNullOrWhiteSpace(colour), Tennis26_Settings.DEFAULT_GAMEWON_COLOUR, colour.Trim())
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
            sendstring = "Function=OverlayInput" + Tennis26_Settings.ComboBoxValues(2) + "In&Input=scorebug_" & displayedScorebugSet.ToString() & "s.gtzip&Mix=0"
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

        If Tennis26_Settings.TextBoxValues(50) = 5 Then
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
        Dim PlayerName As String = If(String.IsNullOrEmpty(Tennis26_Main.HomePlayer(0)), "HOME", Tennis26_Main.HomePlayer(1) & " " & Tennis26_Main.HomePlayer(0))
        Dim country As String = If(String.IsNullOrEmpty(Tennis26_Main.HomePlayer(0)), "HOME", Tennis26_Main.HomePlayer(3))

        ' Vereinfachte Logik: Präfix nur hinzufügen wenn nicht versteckt UND Wert vorhanden
        Dim age As String = If(hideAge OrElse String.IsNullOrEmpty(Tennis26_Main.HomePlayer(4)), " ", "Age: " & Tennis26_Main.HomePlayer(4))
        Dim height As String = If(hideHeight OrElse String.IsNullOrEmpty(Tennis26_Main.HomePlayer(5)), " ", "Height: " & Tennis26_Main.HomePlayer(5))
        Dim info1 As String = If(hideRank OrElse String.IsNullOrEmpty(Tennis26_Main.HomePlayer(6)), " ", Tennis26_Main.HomePlayer(6))
        Dim info2 As String = If(hideDataPoints OrElse String.IsNullOrEmpty(Tennis26_Main.HomePlayer(7)), " ", Tennis26_Main.HomePlayer(7))
        Dim info3 As String = If(hideAssociation OrElse String.IsNullOrEmpty(Tennis26_Main.HomePlayer(8)), " ", Tennis26_Main.HomePlayer(8))

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
        Dim PlayerName As String = If(String.IsNullOrEmpty(Tennis26_Main.AwayPlayer(0)), "AWAY", Tennis26_Main.AwayPlayer(1) & " " & Tennis26_Main.AwayPlayer(0))
        Dim country As String = If(String.IsNullOrEmpty(Tennis26_Main.AwayPlayer(0)), "AWAY", Tennis26_Main.AwayPlayer(3))

        ' Vereinfachte Logik: Präfix nur hinzufügen wenn nicht versteckt UND Wert vorhanden
        Dim age As String = If(hideAge OrElse String.IsNullOrEmpty(Tennis26_Main.AwayPlayer(4)), " ", "Age: " & Tennis26_Main.AwayPlayer(4))
        Dim height As String = If(hideHeight OrElse String.IsNullOrEmpty(Tennis26_Main.AwayPlayer(5)), " ", "Height: " & Tennis26_Main.AwayPlayer(5))
        Dim info1 As String = If(hideRank OrElse String.IsNullOrEmpty(Tennis26_Main.AwayPlayer(6)), " ", Tennis26_Main.AwayPlayer(6))
        Dim info2 As String = If(hideDataPoints OrElse String.IsNullOrEmpty(Tennis26_Main.AwayPlayer(7)), " ", Tennis26_Main.AwayPlayer(7))
        Dim info3 As String = If(hideAssociation OrElse String.IsNullOrEmpty(Tennis26_Main.AwayPlayer(8)), " ", Tennis26_Main.AwayPlayer(8))

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

    ' Wie Lower1()/Lower2(), aber für den 2. Doppelpartner (HomePlayer2/AwayPlayer2) - bewusst
    ' als eigene Methode statt Lower1/Lower2 zu parametrisieren, um den bestehenden, bereits
    ' live erprobten Code für Spieler 1 nicht anzufassen (siehe [[feedback_tennis24_workflow]]
    ' zur Vorsicht bei Änderungen an broadcast-kritischem Code).
    Private Sub LowerHome2()
        Dim scorebugtitle As String = "lower_name.gtzip"
        Dim sendstring As String
        Dim PlayerName As String = If(String.IsNullOrEmpty(Tennis26_Main.HomePlayer2(0)), "HOME2", Tennis26_Main.HomePlayer2(1) & " " & Tennis26_Main.HomePlayer2(0))
        Dim country As String = If(String.IsNullOrEmpty(Tennis26_Main.HomePlayer2(0)), "HOME2", Tennis26_Main.HomePlayer2(3))

        Dim age As String = If(hideAge OrElse String.IsNullOrEmpty(Tennis26_Main.HomePlayer2(4)), " ", "Age: " & Tennis26_Main.HomePlayer2(4))
        Dim height As String = If(hideHeight OrElse String.IsNullOrEmpty(Tennis26_Main.HomePlayer2(5)), " ", "Height: " & Tennis26_Main.HomePlayer2(5))
        Dim info1 As String = If(hideRank OrElse String.IsNullOrEmpty(Tennis26_Main.HomePlayer2(6)), " ", Tennis26_Main.HomePlayer2(6))
        Dim info2 As String = If(hideDataPoints OrElse String.IsNullOrEmpty(Tennis26_Main.HomePlayer2(7)), " ", Tennis26_Main.HomePlayer2(7))
        Dim info3 As String = If(hideAssociation OrElse String.IsNullOrEmpty(Tennis26_Main.HomePlayer2(8)), " ", Tennis26_Main.HomePlayer2(8))

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

    Private Sub LowerAway2()
        Dim scorebugtitle As String = "lower_name.gtzip"
        Dim sendstring As String
        Dim PlayerName As String = If(String.IsNullOrEmpty(Tennis26_Main.AwayPlayer2(0)), "AWAY2", Tennis26_Main.AwayPlayer2(1) & " " & Tennis26_Main.AwayPlayer2(0))
        Dim country As String = If(String.IsNullOrEmpty(Tennis26_Main.AwayPlayer2(0)), "AWAY2", Tennis26_Main.AwayPlayer2(3))

        Dim age As String = If(hideAge OrElse String.IsNullOrEmpty(Tennis26_Main.AwayPlayer2(4)), " ", "Age: " & Tennis26_Main.AwayPlayer2(4))
        Dim height As String = If(hideHeight OrElse String.IsNullOrEmpty(Tennis26_Main.AwayPlayer2(5)), " ", "Height: " & Tennis26_Main.AwayPlayer2(5))
        Dim info1 As String = If(hideRank OrElse String.IsNullOrEmpty(Tennis26_Main.AwayPlayer2(6)), " ", Tennis26_Main.AwayPlayer2(6))
        Dim info2 As String = If(hideDataPoints OrElse String.IsNullOrEmpty(Tennis26_Main.AwayPlayer2(7)), " ", Tennis26_Main.AwayPlayer2(7))
        Dim info3 As String = If(hideAssociation OrElse String.IsNullOrEmpty(Tennis26_Main.AwayPlayer2(8)), " ", Tennis26_Main.AwayPlayer2(8))

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
            templates = {GetMatchPairingTemplate(), "match_pairing1.gtzip", "match_pairing2.gtzip", "match_pairing3.gtzip", "match_pairing4.gtzip"}
        Else
            ' Nur spezifisches Template aktualisieren
            templates = {specificTemplate}
        End If

        For Each scorebugtitle As String In templates
            Dim sendstring As String

            ' Home Player Daten
            Dim hPlayerName As String = If(String.IsNullOrEmpty(Tennis26_Main.HomePlayer(0)), "HOME", Tennis26_Main.HomePlayer(1) & " " & Tennis26_Main.HomePlayer(0))
            Dim hcountry As String = If(String.IsNullOrEmpty(Tennis26_Main.HomePlayer(0)), "HOME", Tennis26_Main.HomePlayer(3))

            ' Vereinfachte Logik: Präfix nur hinzufügen wenn nicht versteckt UND Wert vorhanden
            Dim hage As String = If(hideAge OrElse String.IsNullOrEmpty(Tennis26_Main.HomePlayer(4)), "", "Age: " & Tennis26_Main.HomePlayer(4))
            Dim hheight As String = If(hideHeight OrElse String.IsNullOrEmpty(Tennis26_Main.HomePlayer(5)), "", "Height: " & Tennis26_Main.HomePlayer(5))
            Dim hdata1 As String = If(hideRank OrElse String.IsNullOrEmpty(Tennis26_Main.HomePlayer(6)), "", Tennis26_Main.HomePlayer(6))

            sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "hname.Text", hPlayerName) : SendHTMLtovMix(sendstring)
            sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "hcountry.Text", hcountry) : SendHTMLtovMix(sendstring)
            sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "hage.Text", hage) : SendHTMLtovMix(sendstring)
            sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "hheight.Text", hheight) : SendHTMLtovMix(sendstring)
            sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "hdata1.Text", hdata1) : SendHTMLtovMix(sendstring)

            Dim flagInfo = GetFlagInfo(hcountry)
            If flagInfo.Exists Then
                sendstring = BuildVmixSetCommand("SetImage", scorebugtitle, "hcountry_flag.Source", flagInfo.Path) : SendHTMLtovMix(sendstring)
            Else
                sendstring = BuildVmixSetCommand("SetImage", scorebugtitle, "hcountry_flag.Source", "C:\VMIX\tennis\flags\transparent.png") : SendHTMLtovMix(sendstring)
            End If

            ' Away Player Daten
            Dim aPlayerName As String = If(String.IsNullOrEmpty(Tennis26_Main.AwayPlayer(0)), "Away", Tennis26_Main.AwayPlayer(1) & " " & Tennis26_Main.AwayPlayer(0))
            Dim acountry As String = If(String.IsNullOrEmpty(Tennis26_Main.AwayPlayer(0)), "Away", Tennis26_Main.AwayPlayer(3))

            Dim aage As String = If(hideAge OrElse String.IsNullOrEmpty(Tennis26_Main.AwayPlayer(4)), "", "Age: " & Tennis26_Main.AwayPlayer(4))
            Dim aheight As String = If(hideHeight OrElse String.IsNullOrEmpty(Tennis26_Main.AwayPlayer(5)), "", "Height: " & Tennis26_Main.AwayPlayer(5))
            Dim adata1 As String = If(hideRank OrElse String.IsNullOrEmpty(Tennis26_Main.AwayPlayer(6)), "", Tennis26_Main.AwayPlayer(6))

            sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "aname.Text", aPlayerName) : SendHTMLtovMix(sendstring)
            sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "acountry.Text", acountry) : SendHTMLtovMix(sendstring)
            sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "aage.Text", aage) : SendHTMLtovMix(sendstring)
            sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "aheight.Text", aheight) : SendHTMLtovMix(sendstring)
            sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "adata1.Text", adata1) : SendHTMLtovMix(sendstring)

            flagInfo = GetFlagInfo(acountry)
            If flagInfo.Exists Then
                sendstring = BuildVmixSetCommand("SetImage", scorebugtitle, "acountry_flag.Source", flagInfo.Path) : SendHTMLtovMix(sendstring)
            Else
                sendstring = BuildVmixSetCommand("SetImage", scorebugtitle, "acountry_flag.Source", "C:\VMIX\tennis\flags\transparent.png") : SendHTMLtovMix(sendstring)
            End If

            ' Doppel-Partner - nur senden, wenn Doppel aktiv ist. Feldnamen laut match_pairing_
            ' double.gtzip: Suffix "2" (hname2/hcountry2/hage2/hheight2/hcountry_flag2), NICHT
            ' das Präfix-Schema (h2name...) wie beim Scorebug - und nur EIN Datenfeld pro
            ' Spieler (hdata2/adata2), kein hdata2/hdata3-Paar wie bei Spieler 1. Deshalb wurden
            ' oben auch die (für Spieler 1 stets wirkungslosen) hdata2/hdata3/adata2/adata3-
            ' Sends entfernt: sie hätten hier sonst genau diese Felder von Spieler 2 überschrieben.
            If IsDoublesMatch() Then
                Dim h2PlayerName As String = If(String.IsNullOrEmpty(Tennis26_Main.HomePlayer2(0)), "", Tennis26_Main.HomePlayer2(1) & " " & Tennis26_Main.HomePlayer2(0))
                Dim h2country As String = If(String.IsNullOrEmpty(Tennis26_Main.HomePlayer2(0)), "", Tennis26_Main.HomePlayer2(3))
                Dim h2age As String = If(hideAge OrElse String.IsNullOrEmpty(Tennis26_Main.HomePlayer2(4)), "", "Age: " & Tennis26_Main.HomePlayer2(4))
                Dim h2height As String = If(hideHeight OrElse String.IsNullOrEmpty(Tennis26_Main.HomePlayer2(5)), "", "Height: " & Tennis26_Main.HomePlayer2(5))
                Dim h2data1 As String = If(hideRank OrElse String.IsNullOrEmpty(Tennis26_Main.HomePlayer2(6)), "", Tennis26_Main.HomePlayer2(6))

                sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "hname2.Text", h2PlayerName) : SendHTMLtovMix(sendstring)
                sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "hcountry2.Text", h2country) : SendHTMLtovMix(sendstring)
                sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "hage2.Text", h2age) : SendHTMLtovMix(sendstring)
                sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "hheight2.Text", h2height) : SendHTMLtovMix(sendstring)
                sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "hdata2.Text", h2data1) : SendHTMLtovMix(sendstring)

                Dim h2FlagInfo = GetFlagInfo(h2country)
                If h2FlagInfo.Exists Then
                    sendstring = BuildVmixSetCommand("SetImage", scorebugtitle, "hcountry_flag2.Source", h2FlagInfo.Path) : SendHTMLtovMix(sendstring)
                Else
                    sendstring = BuildVmixSetCommand("SetImage", scorebugtitle, "hcountry_flag2.Source", "C:\VMIX\tennis\flags\transparent.png") : SendHTMLtovMix(sendstring)
                End If

                Dim a2PlayerName As String = If(String.IsNullOrEmpty(Tennis26_Main.AwayPlayer2(0)), "", Tennis26_Main.AwayPlayer2(1) & " " & Tennis26_Main.AwayPlayer2(0))
                Dim a2country As String = If(String.IsNullOrEmpty(Tennis26_Main.AwayPlayer2(0)), "", Tennis26_Main.AwayPlayer2(3))
                Dim a2age As String = If(hideAge OrElse String.IsNullOrEmpty(Tennis26_Main.AwayPlayer2(4)), "", "Age: " & Tennis26_Main.AwayPlayer2(4))
                Dim a2height As String = If(hideHeight OrElse String.IsNullOrEmpty(Tennis26_Main.AwayPlayer2(5)), "", "Height: " & Tennis26_Main.AwayPlayer2(5))
                Dim a2data1 As String = If(hideRank OrElse String.IsNullOrEmpty(Tennis26_Main.AwayPlayer2(6)), "", Tennis26_Main.AwayPlayer2(6))

                sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "aname2.Text", a2PlayerName) : SendHTMLtovMix(sendstring)
                sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "acountry2.Text", a2country) : SendHTMLtovMix(sendstring)
                sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "aage2.Text", a2age) : SendHTMLtovMix(sendstring)
                sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "aheight2.Text", a2height) : SendHTMLtovMix(sendstring)
                sendstring = BuildVmixSetCommand("SetText", scorebugtitle, "adata2.Text", a2data1) : SendHTMLtovMix(sendstring)

                Dim a2FlagInfo = GetFlagInfo(a2country)
                If a2FlagInfo.Exists Then
                    sendstring = BuildVmixSetCommand("SetImage", scorebugtitle, "acountry_flag2.Source", a2FlagInfo.Path) : SendHTMLtovMix(sendstring)
                Else
                    sendstring = BuildVmixSetCommand("SetImage", scorebugtitle, "acountry_flag2.Source", "C:\VMIX\tennis\flags\transparent.png") : SendHTMLtovMix(sendstring)
                End If
            End If
        Next
    End Sub


    Private Sub Btn_Name_Home_Click(sender As Object, e As EventArgs) Handles Btn_Name_Home.Click
        'blendet spielername1 ein und aus
        Dim entry = GetToggle("home")
        Dim homePlayerName As String = GetFullPlayerName(Tennis26_Main.HomePlayer, "HOME")
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
        Dim awayPlayerName As String = GetFullPlayerName(Tennis26_Main.AwayPlayer, "AWAY")
        Dim Playername = "lower" & vbNewLine & awayPlayerName

        ' Reset other toggles first
        ResetOtherOverlayToggles(entry.Key)
        Dim isOn = ToggleStatus(entry)

        If isOn Then Lower2()
        SendOverlayCommand(entry, isOn)

        Btn_Name_Away.BackColor = If(isOn, Color.Red, SystemColors.ButtonHighlight)
        Btn_Name_Away.Text = Playername
    End Sub

    ' Namenseinblender für den 2. Doppelpartner - genau gleich wie Btn_Name_Home/Away,
    ' zeigt aber immer den individuellen Namen des 2. Spielers (kein Team-Zusammenzug).
    Private Sub Btn_Name_Home2_Click(sender As Object, e As EventArgs) Handles Btn_Name_Home2.Click
        Dim entry = GetToggle("home2")
        Dim homePlayer2Name As String = GetFullPlayerName(Tennis26_Main.HomePlayer2, "HOME2")
        Dim Playername = "lower" & vbNewLine & homePlayer2Name

        ResetOtherOverlayToggles(entry.Key)
        Dim isOn = ToggleStatus(entry)

        If isOn Then LowerHome2()
        SendOverlayCommand(entry, isOn)

        Btn_Name_Home2.BackColor = If(isOn, Color.Red, SystemColors.ButtonHighlight)
        Btn_Name_Home2.Text = Playername
    End Sub

    Private Sub Btn_Name_Away2_Click(sender As Object, e As EventArgs) Handles Btn_Name_Away2.Click
        Dim entry = GetToggle("away2")
        Dim awayPlayer2Name As String = GetFullPlayerName(Tennis26_Main.AwayPlayer2, "AWAY2")
        Dim Playername = "lower" & vbNewLine & awayPlayer2Name

        ResetOtherOverlayToggles(entry.Key)
        Dim isOn = ToggleStatus(entry)

        If isOn Then LowerAway2()
        SendOverlayCommand(entry, isOn)

        Btn_Name_Away2.BackColor = If(isOn, Color.Red, SystemColors.ButtonHighlight)
        Btn_Name_Away2.Text = Playername
    End Sub

    Private Sub Btn_Title_Click(sender As Object, e As EventArgs) Handles Btn_Title.Click
        'blendet Titel ein und aus
        Dim entry = GetToggle("title")

        ' Reset other toggles first
        ResetOtherOverlayToggles(entry.Key)

        SendHTMLtovMix(BuildVmixSetCommand("SetText", entry.Template, "TextBlock1.Text", Tennis26_Settings.TextBoxValues(1)))
        SendHTMLtovMix(BuildVmixSetCommand("SetText", entry.Template, "TextBlock2.Text", Tennis26_Settings.TextBoxValues(2)))

        Dim isOn = ToggleStatus(entry)
        SendOverlayCommand(entry, isOn)

        Btn_Title.BackColor = If(isOn, Color.Red, SystemColors.ButtonHighlight)
        Btn_Title.Text = "Title"
    End Sub

    Private Sub Btn_matchpairing_Click(sender As Object, e As EventArgs) Handles Btn_matchpairing.Click
        'blendet paarung ein und aus
        Pairing()

        Dim entry = GetToggle("matchpairing")
        entry.Template = GetMatchPairingTemplate() ' Einzel/Doppel-Vorlage, siehe Pairing()

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

                Dim sendstring As String = "Function=OverlayInput" + Tennis26_Settings.ComboBoxValues(entry.ComboIndex) + "Off&Input=" + entry.Template + "&Mix=0"
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
            sendstring = "Function=OverlayInput" + Tennis26_Settings.ComboBoxValues(3) + "In&Input=" + nametemplate + "&Mix=0"
            Btn_sponsor1.BackColor = Color.Red
            Btn_sponsor2.BackColor = SystemColors.ButtonHighlight
            sponsor1ToggleStatus = True
            sponsor2ToggleStatus = False
        Else
            sendstring = "Function=OverlayInput" + Tennis26_Settings.ComboBoxValues(3) + "Out&Input=" + nametemplate + "&Mix=0"
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
            sendstring = "Function=OverlayInput" + Tennis26_Settings.ComboBoxValues(3) + "In&Input=" + nametemplate + "&Mix=0"
            Btn_sponsor2.BackColor = Color.Red
            Btn_sponsor1.BackColor = SystemColors.ButtonHighlight
            sponsor1ToggleStatus = False
            sponsor2ToggleStatus = True
        Else
            sendstring = "Function=OverlayInput" + Tennis26_Settings.ComboBoxValues(3) + "Out&Input=" + nametemplate + "&Mix=0"
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
        "Function=OverlayInput" + Tennis26_Settings.ComboBoxValues(1) + "Off&Input=lower_name.gtzip&Mix=0",
        "Function=OverlayInput" + Tennis26_Settings.ComboBoxValues(1) + "Off&Input=large_result.gtzip&Mix=0",
        "Function=OverlayInput" + Tennis26_Settings.ComboBoxValues(1) + "Off&Input=large_result_double.gtzip&Mix=0",
        "Function=OverlayInput" + Tennis26_Settings.ComboBoxValues(1) + "Off&Input=title.gtzip&Mix=0",
        "Function=OverlayInput" + Tennis26_Settings.ComboBoxValues(1) + "Off&Input=match_pairing.gtzip&Mix=0",
        "Function=OverlayInput" + Tennis26_Settings.ComboBoxValues(1) + "Off&Input=match_pairing_double.gtzip&Mix=0",
        "Function=OverlayInput" + Tennis26_Settings.ComboBoxValues(1) + "Off&Input=match_pairing1.gtzip&Mix=0",
        "Function=OverlayInput" + Tennis26_Settings.ComboBoxValues(1) + "Off&Input=match_pairing2.gtzip&Mix=0",
        "Function=OverlayInput" + Tennis26_Settings.ComboBoxValues(1) + "Off&Input=match_pairing3.gtzip&Mix=0",
        "Function=OverlayInput" + Tennis26_Settings.ComboBoxValues(1) + "Off&Input=match_pairing4.gtzip&Mix=0",
        "Function=OverlayInput" + Tennis26_Settings.ComboBoxValues(1) + "Off&Input=info1.gtzip&Mix=0",
        "Function=OverlayInput" + Tennis26_Settings.ComboBoxValues(1) + "Off&Input=info2.gtzip&Mix=0",
        "Function=OverlayInput" + Tennis26_Settings.ComboBoxValues(3) + "Off&Input=sponsor1.gtzip&Mix=0",
        "Function=OverlayInput" + Tennis26_Settings.ComboBoxValues(3) + "Off&Input=sponsor2.gtzip&Mix=0",
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
        sendstring = "Function=OverlayInput" + Tennis26_Settings.ComboBoxValues(1) + "Off" : SendHTMLtovMix(sendstring)
        sendstring = "Function=OverlayInput" + Tennis26_Settings.ComboBoxValues(2) + "Off" : SendHTMLtovMix(sendstring)
        sendstring = "Function=OverlayInput" + Tennis26_Settings.ComboBoxValues(3) + "Off" : SendHTMLtovMix(sendstring)

    End Sub

    ' Je eine Checkbox pro Spielerdetail (Age/Height/Ranking/Points/Association) - Lower1/2,
    ' LowerHome2/Away2 und Pairing() berücksichtigen die jeweilige Variable bereits bei jedem
    ' Aufruf. Der Pairing()-Refresh hier sorgt dafür, dass eine bereits auf Sendung sichtbare
    ' "names match pairing"/matchpairing1-4-Grafik die Änderung sofort übernimmt, nicht erst
    ' beim nächsten Ein-/Ausblenden.
    Private Sub CheckBox_hidedetails_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox_hidedetails.CheckedChanged
        My.Settings.hidedetails = CheckBox_hidedetails.Checked
        My.Settings.Save()
        hideAge = CheckBox_hidedetails.Checked
        Pairing()
    End Sub

    Private Sub CheckBox_hidehight_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox_hidehight.CheckedChanged
        My.Settings.hidehight = CheckBox_hidehight.Checked
        My.Settings.Save()
        hideHeight = CheckBox_hidehight.Checked
        Pairing()
    End Sub

    Private Sub CheckBox_hiderank_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox_hiderank.CheckedChanged
        My.Settings.hiderank = CheckBox_hiderank.Checked
        My.Settings.Save()
        hideRank = CheckBox_hiderank.Checked
        Pairing()
    End Sub

    Private Sub CheckBox_hidepoints_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox_hidepoints.CheckedChanged
        My.Settings.hidepoints = CheckBox_hidepoints.Checked
        My.Settings.Save()
        hideDataPoints = CheckBox_hidepoints.Checked
        Pairing()
    End Sub

    Private Sub CheckBox_hideassociation_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox_hideassociation.CheckedChanged
        My.Settings.hideassociation = CheckBox_hideassociation.Checked
        My.Settings.Save()
        hideAssociation = CheckBox_hideassociation.Checked
        Pairing()
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
                textBox = Tennis26_Settings.TextBox4
                entry = GetToggle("freename1")
            Case "Btn_freename2"
                textBox = Tennis26_Settings.TextBox5
                entry = GetToggle("freename2")
            Case "Btn_freename3"
                textBox = Tennis26_Settings.TextBox6
                entry = GetToggle("freename3")
            Case "Btn_freename4"
                textBox = Tennis26_Settings.TextBox7
                entry = GetToggle("freename4")
            Case "Btn_freename5"
                textBox = Tennis26_Settings.TextBox8
                entry = GetToggle("freename5")
            Case "Btn_ref1"
                textBox = Tennis26_Settings.TextBox20
                entry = GetToggle("ref1")
            Case "Btn_ref2"
                textBox = Tennis26_Settings.TextBox21
                entry = GetToggle("ref2")

            Case "Btn_com1"
                textBox = Tennis26_Settings.TextBox22
                entry = GetToggle("com1")
                ' SPEZIELLE BEHANDLUNG für Btn_com1: Template abhängig von Commentator2 setzen
                If String.IsNullOrEmpty(Tennis26_Settings.TextBox23.Text.Trim()) Then
                    nametemplate = "name.gtzip"  ' Nur ein Kommentator
                Else
                    nametemplate = "name2.gtzip" ' Beide Kommentatoren
                End If

            Case "Btn_com2"
                textBox = Tennis26_Settings.TextBox23
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
                Dim commentator1Text As String = Tennis26_Settings.TextBox22.Text.Trim()
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
                Dim commentator2Text As String = Tennis26_Settings.TextBox23.Text.Trim()
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