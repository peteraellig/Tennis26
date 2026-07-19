' UI-freie Zähllogik für ein Tennis-Match (Punkte/Spiele/Sätze, Tiebreak-Regeln,
' Aufschlag-Statistik, Undo-Stack). Tennis24_Scorer.vb hält eine Instanz dieser Klasse
' und delegiert Zustand + reine Berechnungen hierhin; alle UI- und vMix-Aufrufe
' bleiben im Formular.
Public Class TennisMatchEngine

    Public Class MatchState
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
        Public Property HomeBreakPointsConverted As Integer
        Public Property HomeBreakPointsTotal As Integer
        Public Property AwayBreakPointsConverted As Integer
        Public Property AwayBreakPointsTotal As Integer
        Public Property HomeMiniBreaks As Integer
        Public Property AwayMiniBreaks As Integer
        Public Property TiebreakStartServerIsHome As Boolean
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

    Public Property IsTiebreak As Boolean = False
    Public Property HomePoints As Integer = 0
    Public Property AwayPoints As Integer = 0
    Public Property HomeGames As Integer = 0
    Public Property AwayGames As Integer = 0
    Public Property HomeSets As Integer = 0
    Public Property AwaySets As Integer = 0
    Public Property CurrentSet As Integer = 1
    Public Property IsMatchFinished As Boolean = False
    Public Property NoTiebreakMode As Boolean = False
    Public Property MatchTiebreakEnabled As Boolean = False
    Public Property MatchTiebreakTarget As Integer = 10
    Public Property HomeTotalPoints As Integer = 0
    Public Property AwayTotalPoints As Integer = 0
    ' Breaks = tatsächlich gewonnene fremde Aufschlagspiele
    Public Property HomeBreaks As Integer = 0
    Public Property AwayBreaks As Integer = 0

    ' Breakpunkte = einzelne Punkte, die als Returner zum Break geführt hätten.
    ' "Total" zählt jede gespielte Breakball-Situation (TV-Konvention: bei 0:40 und
    ' anschliessendem Ausgleich auf 40:40 sind das 3 Breakbälle, 0 verwertet),
    ' "Converted" davon die tatsächlich verwerteten.
    Public Property HomeBreakPointsConverted As Integer = 0
    Public Property HomeBreakPointsTotal As Integer = 0
    Public Property AwayBreakPointsConverted As Integer = 0
    Public Property AwayBreakPointsTotal As Integer = 0

    ' Mini-Breaks = im (Match-)Tiebreak gewonnene Punkte gegen den Aufschlag des Gegners.
    Public Property HomeMiniBreaks As Integer = 0
    Public Property AwayMiniBreaks As Integer = 0

    ' Wer den laufenden Tiebreak eröffnet hat - Basis für die Aufschlag-Rotation
    ' (1 Punkt, danach je 2 Punkte im Wechsel).
    Public Property TiebreakStartServerIsHome As Boolean = True
    Public Property HomeServiceGamesWon As Integer = 0
    Public Property AwayServiceGamesWon As Integer = 0
    Public Property IsHomeServing As Boolean = True
    Public Property HomeTiebreaksWon As Integer = 0
    Public Property AwayTiebreaksWon As Integer = 0
    Public Property LongestGame As Integer = 0
    Public Property CurrentGamePoints As Integer = 0
    Public Property FirstPointPlayed As Boolean = False
    Public Property FirstServerOfCurrentSet As Boolean = True

    Public ReadOnly Property Stack As New Stack(Of MatchState)

    Public Sub PushState()
        Stack.Push(New MatchState With {
            .HomePoints = HomePoints,
            .AwayPoints = AwayPoints,
            .HomeGames = HomeGames,
            .AwayGames = AwayGames,
            .HomeSets = HomeSets,
            .AwaySets = AwaySets,
            .CurrentSet = CurrentSet,
            .IsTiebreak = IsTiebreak,
            .HomeTotalPoints = HomeTotalPoints,
            .AwayTotalPoints = AwayTotalPoints,
            .HomeBreaks = HomeBreaks,
            .AwayBreaks = AwayBreaks,
            .HomeBreakPointsConverted = HomeBreakPointsConverted,
            .HomeBreakPointsTotal = HomeBreakPointsTotal,
            .AwayBreakPointsConverted = AwayBreakPointsConverted,
            .AwayBreakPointsTotal = AwayBreakPointsTotal,
            .HomeMiniBreaks = HomeMiniBreaks,
            .AwayMiniBreaks = AwayMiniBreaks,
            .TiebreakStartServerIsHome = TiebreakStartServerIsHome,
            .HomeServiceGamesWon = HomeServiceGamesWon,
            .AwayServiceGamesWon = AwayServiceGamesWon,
            .IsHomeServing = IsHomeServing,
            .FirstServerOfCurrentSet = FirstServerOfCurrentSet,
            .HomeTiebreaksWon = HomeTiebreaksWon,
            .AwayTiebreaksWon = AwayTiebreaksWon,
            .LongestGame = LongestGame,
            .CurrentGamePoints = CurrentGamePoints,
            .FirstPointPlayed = FirstPointPlayed,
            .IsMatchFinished = IsMatchFinished,
            .NoTiebreakMode = NoTiebreakMode
        })
    End Sub

    ' Stellt den letzten gespeicherten Zustand wieder her und gibt ihn zurück;
    ' Nothing, wenn der Stack leer ist (nichts zum Rückgängigmachen).
    Public Function PopState() As MatchState
        If Stack.Count = 0 Then Return Nothing

        Dim lastState = Stack.Pop()
        HomePoints = lastState.HomePoints
        AwayPoints = lastState.AwayPoints
        HomeGames = lastState.HomeGames
        AwayGames = lastState.AwayGames
        HomeSets = lastState.HomeSets
        AwaySets = lastState.AwaySets
        CurrentSet = lastState.CurrentSet
        IsTiebreak = lastState.IsTiebreak
        HomeTotalPoints = lastState.HomeTotalPoints
        AwayTotalPoints = lastState.AwayTotalPoints
        HomeBreaks = lastState.HomeBreaks
        AwayBreaks = lastState.AwayBreaks
        HomeBreakPointsConverted = lastState.HomeBreakPointsConverted
        HomeBreakPointsTotal = lastState.HomeBreakPointsTotal
        AwayBreakPointsConverted = lastState.AwayBreakPointsConverted
        AwayBreakPointsTotal = lastState.AwayBreakPointsTotal
        HomeMiniBreaks = lastState.HomeMiniBreaks
        AwayMiniBreaks = lastState.AwayMiniBreaks
        TiebreakStartServerIsHome = lastState.TiebreakStartServerIsHome
        HomeServiceGamesWon = lastState.HomeServiceGamesWon
        AwayServiceGamesWon = lastState.AwayServiceGamesWon
        IsHomeServing = lastState.IsHomeServing
        FirstServerOfCurrentSet = lastState.FirstServerOfCurrentSet
        HomeTiebreaksWon = lastState.HomeTiebreaksWon
        AwayTiebreaksWon = lastState.AwayTiebreaksWon
        LongestGame = lastState.LongestGame
        CurrentGamePoints = lastState.CurrentGamePoints
        FirstPointPlayed = lastState.FirstPointPlayed
        IsMatchFinished = lastState.IsMatchFinished
        NoTiebreakMode = lastState.NoTiebreakMode
        Return lastState
    End Function

    Public Sub ResetMatch()
        HomePoints = 0
        AwayPoints = 0
        HomeGames = 0
        AwayGames = 0
        HomeSets = 0
        AwaySets = 0
        CurrentSet = 1
        IsTiebreak = False
        FirstPointPlayed = False
        IsMatchFinished = False

        HomeTotalPoints = 0
        AwayTotalPoints = 0
        HomeBreaks = 0
        AwayBreaks = 0
        HomeBreakPointsConverted = 0
        HomeBreakPointsTotal = 0
        AwayBreakPointsConverted = 0
        AwayBreakPointsTotal = 0
        HomeMiniBreaks = 0
        AwayMiniBreaks = 0
        TiebreakStartServerIsHome = True
        HomeServiceGamesWon = 0
        AwayServiceGamesWon = 0
        IsHomeServing = True
        FirstServerOfCurrentSet = True
        HomeTiebreaksWon = 0
        AwayTiebreaksWon = 0
        LongestGame = 0
        CurrentGamePoints = 0

        Stack.Clear()
    End Sub

    ' Liefert "home"/"away", wenn der Returner im AKTUELLEN Punkt einen oder mehrere
    ' Breakbälle hat, sonst "". Im Tiebreak gibt es per Konvention keine Breakbälle.
    Public Function BreakPointHolder() As String
        If IsTiebreak OrElse IsMatchTiebreakSet() OrElse IsMatchFinished Then Return ""

        Dim returnerPoints As Integer = If(IsHomeServing, AwayPoints, HomePoints)
        Dim serverPoints As Integer = If(IsHomeServing, HomePoints, AwayPoints)

        ' Breakball: der Returner ist genau einen Punkt vom Spielgewinn entfernt,
        ' d.h. er hat mindestens 3 Punkte (40) und liegt vorne.
        If returnerPoints >= 3 AndAlso returnerPoints > serverPoints Then
            Return If(IsHomeServing, "away", "home")
        End If

        Return ""
    End Function

    ' Anzahl Breakbälle im aktuellen Punkt: 0:40 = 3, 15:40 = 2, 30:40 = 1, Vorteil = 1.
    Public Function CurrentBreakPointCount() As Integer
        If BreakPointHolder() = "" Then Return 0

        Dim returnerPoints As Integer = If(IsHomeServing, AwayPoints, HomePoints)
        Dim serverPoints As Integer = If(IsHomeServing, HomePoints, AwayPoints)
        Return returnerPoints - serverPoints
    End Function

    Public Function IsInAnyTiebreak() As Boolean
        Return IsTiebreak OrElse IsMatchTiebreakSet()
    End Function

    ' Aufschläger für den GERADE ANSTEHENDEN Punkt im (Match-)Tiebreak. Regel: der Eröffner
    ' schlägt 1 Punkt auf, danach wechseln sich beide je 2 Punkte ab.
    ' Punkt-Index 0 -> Eröffner, 1+2 -> Gegner, 3+4 -> Eröffner, 5+6 -> Gegner, ...
    Public Function TiebreakServerIsHome() As Boolean
        Dim pointIndex As Integer = HomePoints + AwayPoints
        Dim block As Integer = (pointIndex + 1) \ 2
        Dim starterServes As Boolean = (block Mod 2 = 0)
        Return If(starterServes, TiebreakStartServerIsHome, Not TiebreakStartServerIsHome)
    End Function

    Public Sub RegisterPoint(player As String)
        ' Beim ersten Punkt eines (Match-)Tiebreaks festhalten, wer ihn eröffnet - daraus
        ' leitet sich die gesamte Aufschlag-Rotation im Tiebreak ab. Gleichzeitig die
        ' Mini-Break-Zähler zurücksetzen: Sie beziehen sich bewusst auf den LAUFENDEN
        ' Tiebreak (Live-Info "wer liegt vorne"), nicht auf das ganze Match - sonst wäre
        ' die Hervorhebung in einem zweiten Tiebreak durch die Werte des ersten verfälscht.
        If IsInAnyTiebreak() AndAlso HomePoints + AwayPoints = 0 Then
            TiebreakStartServerIsHome = IsHomeServing
            HomeMiniBreaks = 0
            AwayMiniBreaks = 0
        End If

        ' Mini-Break: im Tiebreak einen Punkt gegen den Aufschlag des Gegners gewinnen.
        If IsInAnyTiebreak() Then
            Dim serverIsHome As Boolean = TiebreakServerIsHome()
            If player = "home" AndAlso Not serverIsHome Then
                HomeMiniBreaks += 1
            ElseIf player = "away" AndAlso serverIsHome Then
                AwayMiniBreaks += 1
            End If
        End If

        ' Breakball-Statistik VOR der Punktänderung auswerten: Wird der laufende Punkt bei
        ' Breakball gespielt, zählt er als Breakball-Chance für den Returner - unabhängig
        ' davon, ob er ihn verwertet. (TV-Konvention: 0:40 bis 40:40 = 3 Chancen, 0 genutzt.)
        Dim breakPointHolderBefore As String = BreakPointHolder()
        If breakPointHolderBefore <> "" Then
            If breakPointHolderBefore = "home" Then
                HomeBreakPointsTotal += 1
                If player = "home" Then HomeBreakPointsConverted += 1
            Else
                AwayBreakPointsTotal += 1
                If player = "away" Then AwayBreakPointsConverted += 1
            End If
        End If

        If player = "home" Then
            HomePoints += 1
            HomeTotalPoints += 1
        Else
            AwayPoints += 1
            AwayTotalPoints += 1
        End If

        CurrentGamePoints += 1
    End Sub

    Public Function IsDecidingSet() As Boolean
        Dim setsToWin As Integer = BestOfSetsToWin()
        Return HomeSets = setsToWin - 1 AndAlso AwaySets = setsToWin - 1
    End Function

    ' Match-Tiebreak ersetzt regelkonform nur den letzten Satz bei Best of 3, wenn in den
    ' Settings aktiviert (checkbox1/textbox42) - bei Best of 5 nicht üblich, daher hier
    ' bewusst nicht unterstützt.
    Public Function IsMatchTiebreakSet() As Boolean
        Return MatchTiebreakEnabled AndAlso IsDecidingSet() AndAlso BestOfSetsToWin() = 2
    End Function

    Public Sub CheckForBreak(winner As String)
        If Not IsTiebreak AndAlso Not IsMatchTiebreakSet() Then
            If winner = "home" Then
                If Not IsHomeServing Then
                    ' Home hat Away's Aufschlag gebrochen
                    HomeBreaks += 1
                Else
                    ' Home hat eigenen Aufschlag gehalten
                    HomeServiceGamesWon += 1
                End If
            Else ' winner = "away"
                If IsHomeServing Then
                    ' Away hat Home's Aufschlag gebrochen
                    AwayBreaks += 1
                Else
                    ' Away hat eigenen Aufschlag gehalten
                    AwayServiceGamesWon += 1
                End If
            End If
        End If
    End Sub

    Public Sub TrackLongestGame()
        If CurrentGamePoints > LongestGame Then
            LongestGame = CurrentGamePoints
        End If
        CurrentGamePoints = 0
    End Sub

    Public Sub ResetPoints()
        HomePoints = 0
        AwayPoints = 0
    End Sub

    Public Sub ResetGames()
        HomeGames = 0
        AwayGames = 0
    End Sub

    Public Function IsGameWon(playerPoints As Integer, opponentPoints As Integer) As Boolean
        ' Prüft, ob ein Spieler das Game (bzw. beim Match-Tiebreak: das ganze restliche Match) gewonnen hat
        If IsMatchTiebreakSet() Then
            Return playerPoints >= MatchTiebreakTarget AndAlso playerPoints - opponentPoints >= 2
        ElseIf IsTiebreak Then
            Return playerPoints >= 7 AndAlso playerPoints - opponentPoints >= 2
        Else
            Return playerPoints >= 4 AndAlso playerPoints - opponentPoints >= 2
        End If
    End Function

    Public Function BestOfSetsToWin() As Integer
        Return Math.Ceiling(Tennis24_Settings.TextBoxValues(50) / 2.0)
    End Function

    Public Function IsSetWon(playerGames As Integer, opponentGames As Integer) As Boolean
        ' Match-Tiebreak entscheidet den ganzen Satz (und damit das Match): sobald das eine
        ' "Spiel" bis MatchTiebreakTarget Punkte gewonnen ist (IsGameWon hat das mit dem
        ' richtigen Vorsprung schon geprüft, bevor playerGames/opponentGames hier auf das
        ' Endergebnis gesetzt werden), ist der Satz vorbei.
        If IsMatchTiebreakSet() Then
            Return playerGames >= MatchTiebreakTarget
        End If

        ' "No Tiebreak (Advantage Set)" gilt regelkonform nur im entscheidenden Satz
        ' (Best of 3: 1:1 vor Satz 3, Best of 5: 2:2 vor Satz 5). Alle Sätze davor
        ' haben immer einen regulären Tiebreak bei 6:6, unabhängig vom Checkbox-Status.
        Dim decidingSet As Boolean = IsDecidingSet()

        If NoTiebreakMode AndAlso decidingSet Then
            ' No-Tiebreak-Modus im entscheidenden Satz: Set wird nur mit 2 Games Vorsprung gewonnen, kein Limit
            If playerGames >= 6 AndAlso playerGames - opponentGames >= 2 Then
                Return True
            End If
            Return False
        Else
            ' Normaler Modus: OHNE Tiebreak: 6 Games mit 2 Games Vorsprung
            If Not IsTiebreak Then
                If playerGames >= 6 AndAlso playerGames - opponentGames >= 2 Then
                    Return True
                End If

                ' Tiebreak bei 6:6 starten
                If playerGames = 6 AndAlso opponentGames = 6 Then
                    IsTiebreak = True
                    Return False
                End If
            Else
                ' IM Tiebreak: playerGames/opponentGames sind hier die GAMES, nicht die
                ' Tiebreak-Punkte - der Verlierer bleibt games-mässig immer bei 6 eingefroren,
                ' der Gewinner steigt beim Tiebreak-Sieg auf 7 (also IMMER genau 7:6, nie mehr
                ' Differenz). IsGameWon() hat den Tiebreak selbst schon korrekt anhand der
                ' echten Punkte mit 2 Punkten Vorsprung entschieden, bevor homeGames/awayGames
                ' hier überhaupt erhöht wurde. Ein zusätzlicher Games-Vorsprung darf deshalb
                ' NICHT verlangt werden (Bug: vorher "AndAlso playerGames - opponentGames >= 2"
                ' hier nie erfüllt, wodurch der Satz nach einem Tiebreak nie als beendet erkannt
                ' und weitere Punkte fälschlich dem alten Satz zugerechnet wurden).
                If playerGames >= 7 Then
                    IsTiebreak = False
                    Return True
                End If
            End If
        End If

        Return False
    End Function

    Public Function ConvertPointsToTennisScore(playerPoints As Integer, opponentPoints As Integer) As String
        ' Konvertiert Punktestand in Tennis-Score (0, 15, 30, 40, A)
        If IsTiebreak OrElse IsMatchTiebreakSet() Then
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

End Class
