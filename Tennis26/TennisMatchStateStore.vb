Imports System.IO
Imports System.Xml

' Ein vollständiger, zu einem Zeitpunkt eingefrorener Spielstand (TennisMatchEngine-Zustand
' plus Spielernamen plus Speicherzeit) - unabhängig davon, ob er über Save/Load (bewusst,
' mit selbst gewähltem Namen) oder Recover (automatisch, siehe TennisMatchStateStore) erzeugt
' wurde.
Public Class MatchStateSnapshot
    Public Property SavedAt As DateTime
    Public Property HomePlayerName As String = ""
    Public Property AwayPlayerName As String = ""

    ' Der Spielstand je abgeschlossenem Satz (Games pro Satz) - lebt nirgends in
    ' TennisMatchEngine (die kennt nur HomeSets/AwaySets als Anzahl gewonnener Sätze plus
    ' HomeGames/AwayGames für den LAUFENDEN Satz), sondern nur in den Scorer-Labels
    ' lbl_home_s1..s5/lbl_away_s1..s5 - deshalb hier separat mitgeführt.
    Public Property HomeSetScores As Integer() = {0, 0, 0, 0, 0}
    Public Property AwaySetScores As Integer() = {0, 0, 0, 0, 0}

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
    Public Property CompletedGamesInMatch As Integer
    Public Property MatchStartTime As DateTime?
    Public Property IsMidMatchEntry As Boolean
End Class

' Schreibt/liest einen MatchStateSnapshot als XML-Datei - Basis für drei Features:
'  - Save/Load (Btn_save/Btn_load): bewusstes Speichern/Laden unter einem selbst gewählten
'    Namen (Ordner SAVES_FOLDER_PATH), z.B. damit ein anderer Operator am nächsten Tag
'    genau dieses Match fortsetzen kann.
'  - Recover (Btn_recover): eine feste Datei (AUTO_RECOVERY_FILE_PATH), die unabhängig von
'    Save/Load nach jedem Punkt automatisch überschrieben wird - Schutz gegen Stromausfall/
'    Programmabsturz, funktioniert auch dann, wenn nie manuell gespeichert wurde.
Public Class TennisMatchStateStore

    Public Const AUTO_RECOVERY_FILE_PATH As String = Tennis26_Settings.SETTINGS_DATA_PATH & "\autosave.xml"
    Public Const SAVES_FOLDER_PATH As String = Tennis26_Settings.SETTINGS_DATA_PATH & "\saves"

    Public Shared Function BuildSnapshot(match As TennisMatchEngine, homePlayerName As String, awayPlayerName As String, homeSetScores As Integer(), awaySetScores As Integer()) As MatchStateSnapshot
        Return New MatchStateSnapshot With {
            .SavedAt = DateTime.UtcNow,
            .HomePlayerName = homePlayerName,
            .AwayPlayerName = awayPlayerName,
            .HomeSetScores = homeSetScores,
            .AwaySetScores = awaySetScores,
            .HomePoints = match.HomePoints,
            .AwayPoints = match.AwayPoints,
            .HomeGames = match.HomeGames,
            .AwayGames = match.AwayGames,
            .HomeSets = match.HomeSets,
            .AwaySets = match.AwaySets,
            .CurrentSet = match.CurrentSet,
            .IsTiebreak = match.IsTiebreak,
            .HomeTotalPoints = match.HomeTotalPoints,
            .AwayTotalPoints = match.AwayTotalPoints,
            .HomeBreaks = match.HomeBreaks,
            .AwayBreaks = match.AwayBreaks,
            .HomeBreakPointsConverted = match.HomeBreakPointsConverted,
            .HomeBreakPointsTotal = match.HomeBreakPointsTotal,
            .AwayBreakPointsConverted = match.AwayBreakPointsConverted,
            .AwayBreakPointsTotal = match.AwayBreakPointsTotal,
            .HomeMiniBreaks = match.HomeMiniBreaks,
            .AwayMiniBreaks = match.AwayMiniBreaks,
            .TiebreakStartServerIsHome = match.TiebreakStartServerIsHome,
            .HomeServiceGamesWon = match.HomeServiceGamesWon,
            .AwayServiceGamesWon = match.AwayServiceGamesWon,
            .IsHomeServing = match.IsHomeServing,
            .FirstServerOfCurrentSet = match.FirstServerOfCurrentSet,
            .HomeTiebreaksWon = match.HomeTiebreaksWon,
            .AwayTiebreaksWon = match.AwayTiebreaksWon,
            .LongestGame = match.LongestGame,
            .CurrentGamePoints = match.CurrentGamePoints,
            .FirstPointPlayed = match.FirstPointPlayed,
            .IsMatchFinished = match.IsMatchFinished,
            .NoTiebreakMode = match.NoTiebreakMode,
            .CompletedGamesInMatch = match.CompletedGamesInMatch,
            .MatchStartTime = match.MatchStartTime,
            .IsMidMatchEntry = match.IsMidMatchEntry
        }
    End Function

    ' Überträgt einen geladenen Snapshot zurück in eine (üblicherweise frische) Engine-Instanz.
    ' Der Undo-Stack wird bewusst NICHT übernommen - nach einem Recover/Load gibt es keine
    ' sinnvolle alte Undo-Historie mehr, ein Undo direkt danach würde nur verwirren.
    Public Shared Sub ApplySnapshot(match As TennisMatchEngine, snapshot As MatchStateSnapshot)
        match.HomePoints = snapshot.HomePoints
        match.AwayPoints = snapshot.AwayPoints
        match.HomeGames = snapshot.HomeGames
        match.AwayGames = snapshot.AwayGames
        match.HomeSets = snapshot.HomeSets
        match.AwaySets = snapshot.AwaySets
        match.CurrentSet = snapshot.CurrentSet
        match.IsTiebreak = snapshot.IsTiebreak
        match.HomeTotalPoints = snapshot.HomeTotalPoints
        match.AwayTotalPoints = snapshot.AwayTotalPoints
        match.HomeBreaks = snapshot.HomeBreaks
        match.AwayBreaks = snapshot.AwayBreaks
        match.HomeBreakPointsConverted = snapshot.HomeBreakPointsConverted
        match.HomeBreakPointsTotal = snapshot.HomeBreakPointsTotal
        match.AwayBreakPointsConverted = snapshot.AwayBreakPointsConverted
        match.AwayBreakPointsTotal = snapshot.AwayBreakPointsTotal
        match.HomeMiniBreaks = snapshot.HomeMiniBreaks
        match.AwayMiniBreaks = snapshot.AwayMiniBreaks
        match.TiebreakStartServerIsHome = snapshot.TiebreakStartServerIsHome
        match.HomeServiceGamesWon = snapshot.HomeServiceGamesWon
        match.AwayServiceGamesWon = snapshot.AwayServiceGamesWon
        match.IsHomeServing = snapshot.IsHomeServing
        match.FirstServerOfCurrentSet = snapshot.FirstServerOfCurrentSet
        match.HomeTiebreaksWon = snapshot.HomeTiebreaksWon
        match.AwayTiebreaksWon = snapshot.AwayTiebreaksWon
        match.LongestGame = snapshot.LongestGame
        match.CurrentGamePoints = snapshot.CurrentGamePoints
        match.FirstPointPlayed = snapshot.FirstPointPlayed
        match.IsMatchFinished = snapshot.IsMatchFinished
        match.NoTiebreakMode = snapshot.NoTiebreakMode
        match.CompletedGamesInMatch = snapshot.CompletedGamesInMatch
        match.MatchStartTime = snapshot.MatchStartTime
        match.IsMidMatchEntry = snapshot.IsMidMatchEntry
        match.Stack.Clear()
    End Sub

    ' Schreibt "snapshot" atomar nach "filePath" (wie TennisJsonExporter): erst in eine
    ' .tmp-Datei, dann per Move ersetzt, damit eine mitten im Schreiben abstürzende
    ' Anwendung nie eine halb geschriebene Datei hinterlässt.
    Public Shared Sub SaveToFile(snapshot As MatchStateSnapshot, filePath As String)
        Dim directoryPath = Path.GetDirectoryName(filePath)
        If Not Directory.Exists(directoryPath) Then Directory.CreateDirectory(directoryPath)

        Dim xmlDoc As New XmlDocument()
        Dim root As XmlNode = xmlDoc.CreateElement("Tennis26_MatchState")
        xmlDoc.AppendChild(root)

        AddElement(xmlDoc, root, "SavedAt", snapshot.SavedAt.ToString("o"))
        AddElement(xmlDoc, root, "HomePlayerName", snapshot.HomePlayerName)
        AddElement(xmlDoc, root, "AwayPlayerName", snapshot.AwayPlayerName)
        AddElement(xmlDoc, root, "HomeSetScores", String.Join(",", snapshot.HomeSetScores))
        AddElement(xmlDoc, root, "AwaySetScores", String.Join(",", snapshot.AwaySetScores))
        AddElement(xmlDoc, root, "HomePoints", snapshot.HomePoints.ToString())
        AddElement(xmlDoc, root, "AwayPoints", snapshot.AwayPoints.ToString())
        AddElement(xmlDoc, root, "HomeGames", snapshot.HomeGames.ToString())
        AddElement(xmlDoc, root, "AwayGames", snapshot.AwayGames.ToString())
        AddElement(xmlDoc, root, "HomeSets", snapshot.HomeSets.ToString())
        AddElement(xmlDoc, root, "AwaySets", snapshot.AwaySets.ToString())
        AddElement(xmlDoc, root, "CurrentSet", snapshot.CurrentSet.ToString())
        AddElement(xmlDoc, root, "IsTiebreak", snapshot.IsTiebreak.ToString())
        AddElement(xmlDoc, root, "HomeTotalPoints", snapshot.HomeTotalPoints.ToString())
        AddElement(xmlDoc, root, "AwayTotalPoints", snapshot.AwayTotalPoints.ToString())
        AddElement(xmlDoc, root, "HomeBreaks", snapshot.HomeBreaks.ToString())
        AddElement(xmlDoc, root, "AwayBreaks", snapshot.AwayBreaks.ToString())
        AddElement(xmlDoc, root, "HomeBreakPointsConverted", snapshot.HomeBreakPointsConverted.ToString())
        AddElement(xmlDoc, root, "HomeBreakPointsTotal", snapshot.HomeBreakPointsTotal.ToString())
        AddElement(xmlDoc, root, "AwayBreakPointsConverted", snapshot.AwayBreakPointsConverted.ToString())
        AddElement(xmlDoc, root, "AwayBreakPointsTotal", snapshot.AwayBreakPointsTotal.ToString())
        AddElement(xmlDoc, root, "HomeMiniBreaks", snapshot.HomeMiniBreaks.ToString())
        AddElement(xmlDoc, root, "AwayMiniBreaks", snapshot.AwayMiniBreaks.ToString())
        AddElement(xmlDoc, root, "TiebreakStartServerIsHome", snapshot.TiebreakStartServerIsHome.ToString())
        AddElement(xmlDoc, root, "HomeServiceGamesWon", snapshot.HomeServiceGamesWon.ToString())
        AddElement(xmlDoc, root, "AwayServiceGamesWon", snapshot.AwayServiceGamesWon.ToString())
        AddElement(xmlDoc, root, "IsHomeServing", snapshot.IsHomeServing.ToString())
        AddElement(xmlDoc, root, "FirstServerOfCurrentSet", snapshot.FirstServerOfCurrentSet.ToString())
        AddElement(xmlDoc, root, "HomeTiebreaksWon", snapshot.HomeTiebreaksWon.ToString())
        AddElement(xmlDoc, root, "AwayTiebreaksWon", snapshot.AwayTiebreaksWon.ToString())
        AddElement(xmlDoc, root, "LongestGame", snapshot.LongestGame.ToString())
        AddElement(xmlDoc, root, "CurrentGamePoints", snapshot.CurrentGamePoints.ToString())
        AddElement(xmlDoc, root, "FirstPointPlayed", snapshot.FirstPointPlayed.ToString())
        AddElement(xmlDoc, root, "IsMatchFinished", snapshot.IsMatchFinished.ToString())
        AddElement(xmlDoc, root, "NoTiebreakMode", snapshot.NoTiebreakMode.ToString())
        AddElement(xmlDoc, root, "CompletedGamesInMatch", snapshot.CompletedGamesInMatch.ToString())
        AddElement(xmlDoc, root, "MatchStartTime", If(snapshot.MatchStartTime.HasValue, snapshot.MatchStartTime.Value.ToString("o"), ""))
        AddElement(xmlDoc, root, "IsMidMatchEntry", snapshot.IsMidMatchEntry.ToString())

        Dim tempPath As String = filePath & ".tmp"
        xmlDoc.Save(tempPath)
        If File.Exists(filePath) Then File.Delete(filePath)
        File.Move(tempPath, filePath)
    End Sub

    Public Shared Function LoadFromFile(filePath As String) As MatchStateSnapshot
        Dim xmlDoc As New XmlDocument()
        xmlDoc.Load(filePath)

        Return New MatchStateSnapshot With {
            .SavedAt = ParseDateTime(GetText(xmlDoc, "SavedAt")),
            .HomePlayerName = GetText(xmlDoc, "HomePlayerName"),
            .AwayPlayerName = GetText(xmlDoc, "AwayPlayerName"),
            .HomeSetScores = GetIntArray(xmlDoc, "HomeSetScores"),
            .AwaySetScores = GetIntArray(xmlDoc, "AwaySetScores"),
            .HomePoints = GetInt(xmlDoc, "HomePoints"),
            .AwayPoints = GetInt(xmlDoc, "AwayPoints"),
            .HomeGames = GetInt(xmlDoc, "HomeGames"),
            .AwayGames = GetInt(xmlDoc, "AwayGames"),
            .HomeSets = GetInt(xmlDoc, "HomeSets"),
            .AwaySets = GetInt(xmlDoc, "AwaySets"),
            .CurrentSet = GetInt(xmlDoc, "CurrentSet"),
            .IsTiebreak = GetBool(xmlDoc, "IsTiebreak"),
            .HomeTotalPoints = GetInt(xmlDoc, "HomeTotalPoints"),
            .AwayTotalPoints = GetInt(xmlDoc, "AwayTotalPoints"),
            .HomeBreaks = GetInt(xmlDoc, "HomeBreaks"),
            .AwayBreaks = GetInt(xmlDoc, "AwayBreaks"),
            .HomeBreakPointsConverted = GetInt(xmlDoc, "HomeBreakPointsConverted"),
            .HomeBreakPointsTotal = GetInt(xmlDoc, "HomeBreakPointsTotal"),
            .AwayBreakPointsConverted = GetInt(xmlDoc, "AwayBreakPointsConverted"),
            .AwayBreakPointsTotal = GetInt(xmlDoc, "AwayBreakPointsTotal"),
            .HomeMiniBreaks = GetInt(xmlDoc, "HomeMiniBreaks"),
            .AwayMiniBreaks = GetInt(xmlDoc, "AwayMiniBreaks"),
            .TiebreakStartServerIsHome = GetBool(xmlDoc, "TiebreakStartServerIsHome"),
            .HomeServiceGamesWon = GetInt(xmlDoc, "HomeServiceGamesWon"),
            .AwayServiceGamesWon = GetInt(xmlDoc, "AwayServiceGamesWon"),
            .IsHomeServing = GetBool(xmlDoc, "IsHomeServing"),
            .FirstServerOfCurrentSet = GetBool(xmlDoc, "FirstServerOfCurrentSet"),
            .HomeTiebreaksWon = GetInt(xmlDoc, "HomeTiebreaksWon"),
            .AwayTiebreaksWon = GetInt(xmlDoc, "AwayTiebreaksWon"),
            .LongestGame = GetInt(xmlDoc, "LongestGame"),
            .CurrentGamePoints = GetInt(xmlDoc, "CurrentGamePoints"),
            .FirstPointPlayed = GetBool(xmlDoc, "FirstPointPlayed"),
            .IsMatchFinished = GetBool(xmlDoc, "IsMatchFinished"),
            .NoTiebreakMode = GetBool(xmlDoc, "NoTiebreakMode"),
            .CompletedGamesInMatch = GetInt(xmlDoc, "CompletedGamesInMatch"),
            .MatchStartTime = ParseNullableDateTime(GetText(xmlDoc, "MatchStartTime")),
            .IsMidMatchEntry = GetBool(xmlDoc, "IsMidMatchEntry")
        }
    End Function

    Private Shared Sub AddElement(doc As XmlDocument, parent As XmlNode, name As String, value As String)
        Dim element As XmlNode = doc.CreateElement(name)
        element.InnerText = value
        parent.AppendChild(element)
    End Sub

    Private Shared Function GetText(doc As XmlDocument, name As String) As String
        Dim node = doc.SelectSingleNode($"//{name}")
        Return If(node IsNot Nothing, node.InnerText, "")
    End Function

    Private Shared Function GetInt(doc As XmlDocument, name As String) As Integer
        Dim result As Integer
        Integer.TryParse(GetText(doc, name), result)
        Return result
    End Function

    Private Shared Function GetIntArray(doc As XmlDocument, name As String) As Integer()
        Dim text = GetText(doc, name)
        If String.IsNullOrEmpty(text) Then Return {0, 0, 0, 0, 0}
        Return text.Split(","c).Select(Function(part)
                                            Dim value As Integer
                                            Integer.TryParse(part, value)
                                            Return value
                                        End Function).ToArray()
    End Function

    Private Shared Function GetBool(doc As XmlDocument, name As String) As Boolean
        Dim result As Boolean
        Boolean.TryParse(GetText(doc, name), result)
        Return result
    End Function

    Private Shared Function ParseDateTime(text As String) As DateTime
        Dim result As DateTime
        DateTime.TryParse(text, Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.RoundtripKind, result)
        Return result
    End Function

    Private Shared Function ParseNullableDateTime(text As String) As DateTime?
        If String.IsNullOrEmpty(text) Then Return Nothing
        Dim result As DateTime
        If DateTime.TryParse(text, Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.RoundtripKind, result) Then
            Return result
        End If
        Return Nothing
    End Function

End Class
