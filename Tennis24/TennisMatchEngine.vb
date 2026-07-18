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
    Public Property HomeTotalPoints As Integer = 0
    Public Property AwayTotalPoints As Integer = 0
    Public Property HomeBreaks As Integer = 0
    Public Property AwayBreaks As Integer = 0
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

    Public Sub RegisterPoint(player As String)
        If player = "home" Then
            HomePoints += 1
            HomeTotalPoints += 1
        Else
            AwayPoints += 1
            AwayTotalPoints += 1
        End If

        CurrentGamePoints += 1
    End Sub

    Public Sub CheckForBreak(winner As String)
        If Not IsTiebreak Then
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
        ' Prüft, ob ein Spieler das Game gewonnen hat
        If IsTiebreak Then
            Return playerPoints >= 7 AndAlso playerPoints - opponentPoints >= 2
        Else
            Return playerPoints >= 4 AndAlso playerPoints - opponentPoints >= 2
        End If
    End Function

    Public Function BestOfSetsToWin() As Integer
        Return Math.Ceiling(Tennis24_Settings.TextBoxValues(50) / 2.0)
    End Function

    Public Function IsSetWon(playerGames As Integer, opponentGames As Integer) As Boolean
        ' "No Tiebreak (Advantage Set)" gilt regelkonform nur im entscheidenden Satz
        ' (Best of 3: 1:1 vor Satz 3, Best of 5: 2:2 vor Satz 5). Alle Sätze davor
        ' haben immer einen regulären Tiebreak bei 6:6, unabhängig vom Checkbox-Status.
        Dim setsToWin As Integer = BestOfSetsToWin()
        Dim isDecidingSet As Boolean = HomeSets = setsToWin - 1 AndAlso AwaySets = setsToWin - 1

        If NoTiebreakMode AndAlso isDecidingSet Then
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
                ' Tiebreak-Punkte - der Verlierer bleibt games-mäßig immer bei 6 eingefroren,
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
        If IsTiebreak Then
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
