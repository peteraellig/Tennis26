' Zwischeneinstieg: erlaubt dem Operator, einen bereits laufenden Match manuell auf einen
' bestimmten Stand zu setzen (Sätze, aktuelles Spiel, Aufschlag, bisherige Spielzeit) - für
' den Fall, dass die Produktion erst mitten im Match beginnt. Im Gegensatz zu Save/Load/
' Recover (siehe TennisMatchStateStore) wird hier NICHT die komplette Punkt-für-Punkt-
' Historie rekonstruiert - alle davon abhängigen Statistiken (Breaks, Mini-Breaks, Longest
' Game, ...) starten bewusst bei null (IsMidMatchEntry-Flag, siehe TennisMatchEngine).
'
' Baut auf Apply einen MatchStateSnapshot (dieselbe Klasse wie Save/Load/Recover) und lässt
' den Scorer diesen über die bereits vorhandene ApplySnapshotAndRefresh() übernehmen - keine
' zweite Anwende-Logik nötig.
Public Class Tennis26_MidMatchEntry

    Private resultSnapshotValue As MatchStateSnapshot

    Public ReadOnly Property ResultSnapshot As MatchStateSnapshot
        Get
            Return resultSnapshotValue
        End Get
    End Property

    Private Sub Tennis26_MidMatchEntry_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        RadioButton_HomeServing.Checked = True
    End Sub

    Private Sub Btn_Apply_Click(sender As Object, e As EventArgs) Handles Btn_Apply.Click
        Dim currentSet As Integer = CInt(NumericUpDown_CurrentSet.Value)

        Dim homeSetScores As Integer() = {CInt(NumericUpDown_HomeSet1.Value), CInt(NumericUpDown_HomeSet2.Value), CInt(NumericUpDown_HomeSet3.Value), CInt(NumericUpDown_HomeSet4.Value), CInt(NumericUpDown_HomeSet5.Value)}
        Dim awaySetScores As Integer() = {CInt(NumericUpDown_AwaySet1.Value), CInt(NumericUpDown_AwaySet2.Value), CInt(NumericUpDown_AwaySet3.Value), CInt(NumericUpDown_AwaySet4.Value), CInt(NumericUpDown_AwaySet5.Value)}

        ' Nur ABGESCHLOSSENE Sätze (vor dem aktuellen) zählen als gewonnen - der aktuelle
        ' Satz läuft ja noch, unabhängig vom bereits erspielten Spielstand darin.
        Dim homeSets As Integer = 0
        Dim awaySets As Integer = 0
        For setIndex As Integer = 0 To currentSet - 2
            If homeSetScores(setIndex) > awaySetScores(setIndex) Then
                homeSets += 1
            ElseIf awaySetScores(setIndex) > homeSetScores(setIndex) Then
                awaySets += 1
            End If
        Next

        ' Anzahl abgeschlossener Spiele im gesamten Match ergibt sich einfach aus der Summe
        ' aller eingegebenen Spiele (frühere Sätze + bisherige Spiele im aktuellen Satz) -
        ' wird für die satzübergreifende Seitenwechsel-Logik gebraucht (siehe
        ' TennisMatchEngine.AreSidesSwapped).
        Dim completedGamesInMatch As Integer = homeSetScores.Sum() + awaySetScores.Sum()

        Dim isHomeServing As Boolean = RadioButton_HomeServing.Checked

        ' Wer den aktuellen Satz eröffnet hat, ergibt sich aus der Anzahl bereits gespielter
        ' Spiele in DIESEM Satz (Aufschlag wechselt jedes Spiel): gerade Anzahl -> aktueller
        ' Server hat auch eröffnet, ungerade -> der/die andere hat eröffnet.
        Dim gamesInCurrentSet As Integer = homeSetScores(currentSet - 1) + awaySetScores(currentSet - 1)
        Dim firstServerOfCurrentSet As Boolean = If(gamesInCurrentSet Mod 2 = 0, isHomeServing, Not isHomeServing)

        ' Alle übrigen Felder (Breaks, Breakbälle, Mini-Breaks, Tiebreaks gewonnen, Longest
        ' Game, Total Points, Service Games...) bleiben bewusst auf 0/False - die lassen
        ' sich aus einem simplen Zwischenstand nicht rekonstruieren und zählen ab jetzt
        ' einfach sauber weiter (siehe IsMidMatchEntry).
        Dim snapshot As New MatchStateSnapshot With {
            .SavedAt = DateTime.UtcNow,
            .HomePlayerName = If(String.IsNullOrEmpty(Tennis26_Main.HomePlayer(0)), "HOME", Tennis26_Main.HomePlayer(0)),
            .AwayPlayerName = If(String.IsNullOrEmpty(Tennis26_Main.AwayPlayer(0)), "AWAY", Tennis26_Main.AwayPlayer(0)),
            .HomeSetScores = homeSetScores,
            .AwaySetScores = awaySetScores,
            .HomeSets = homeSets,
            .AwaySets = awaySets,
            .CurrentSet = currentSet,
            .HomeGames = homeSetScores(currentSet - 1),
            .AwayGames = awaySetScores(currentSet - 1),
            .HomePoints = PointsFromComboBox(ComboBox_HomePoints),
            .AwayPoints = PointsFromComboBox(ComboBox_AwayPoints),
            .IsHomeServing = isHomeServing,
            .FirstServerOfCurrentSet = firstServerOfCurrentSet,
            .CompletedGamesInMatch = completedGamesInMatch,
            .FirstPointPlayed = True,
            .MatchStartTime = DateTime.UtcNow.AddHours(-CDbl(NumericUpDown_ElapsedHours.Value)).AddMinutes(-CDbl(NumericUpDown_ElapsedMinutes.Value)),
            .IsMidMatchEntry = True
        }

        resultSnapshotValue = snapshot
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Btn_Cancel_Click(sender As Object, e As EventArgs) Handles Btn_Cancel.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    Private Function PointsFromComboBox(comboBox As ComboBox) As Integer
        Select Case comboBox.Text
            Case "0" : Return 0
            Case "15" : Return 1
            Case "30" : Return 2
            Case "40" : Return 3
            Case "Ad" : Return 4
            Case Else : Return 0
        End Select
    End Function

End Class
