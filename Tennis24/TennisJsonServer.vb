Imports System.Net
Imports System.Text
Imports System.Threading

' Kleiner, gerätunabhängiger JSON-Datenkanal für den aktuellen Spielstand: ein eingebetteter
' HTTP-Server, den beliebige Grafik-Software (Browser-Source, eigene Web-Overlays, andere
' Regie-Werkzeuge) im Netzwerk live abfragen kann - unabhängig von vMix.
'
' WICHTIG zum Netzwerkzugriff: Damit der Server auch von ANDEREN Geräten im Netzwerk (nicht
' nur von diesem PC) erreichbar ist, muss ein Nicht-Admin-Prozess auf einem Wildcard-Prefix
' ("http://+:PORT/") lauschen dürfen - das erlaubt Windows nur nach einer einmaligen
' Berechtigung. Dafür gibt es in den Settings den Button "Netzwerkfreigabe einrichten"
' (siehe Tennis24_Settings.vb, Btn_setup_json_urlacl_Click). Manuell entspricht das, einmalig
' in einer ALS ADMINISTRATOR gestarteten Eingabeaufforderung:
'   netsh http add urlacl url=http://+:41200/ sddl="D:(A;;GX;;;S-1-1-0)"
' (Portzahl an die tatsächlich in den Settings gewählte anpassen.
' "sddl=D:(A;;GX;;;S-1-1-0)" gewährt "Generic Execute" an die wohlbekannte SID S-1-1-0
' (Jeder/Everyone) direkt über eine fertige Sicherheitsbeschreibung - sowohl der Klartext-
' Kontoname "Everyone" (wird auf nicht-englischem Windows, z.B. deutsch "Jeder", von netsh
' nicht zuverlässig aufgelöst) als auch "user=S-1-1-0" (netsh versucht dort eine Namens-
' auflösung, keine SID direkt) schlagen fehl. sddl= akzeptiert die SID dagegen unmittelbar.)
' Ohne diesen Schritt (oder alternativ: das Programm einmalig als Administrator ausführen)
' schlägt Start() mit "Zugriff verweigert" fehl - der Grund landet dann in LastError.
Public Class TennisJsonServer

    Private listener As HttpListener
    Private listenerThread As Thread
    Private ReadOnly stateLock As New Object()
    Private currentJson As String = "{}"

    Public ReadOnly Property IsRunning As Boolean
        Get
            Return listener IsNot Nothing AndAlso listener.IsListening
        End Get
    End Property

    Public Property LastError As String = ""

    ' Startet den Server auf dem angegebenen Port (alle Netzwerk-Schnittstellen). Gibt False
    ' zurück und füllt LastError, wenn das Starten fehlschlägt (z.B. fehlende Berechtigung
    ' oder Port bereits belegt) - wirft dabei bewusst keine Ausnahme, damit ein falsch
    ' gewählter Port die Live-Übertragung nicht zum Absturz bringt.
    Public Function Start(port As Integer) As Boolean
        [Stop]()
        LastError = ""

        Try
            listener = New HttpListener()
            listener.Prefixes.Add($"http://+:{port}/")
            listener.Start()
        Catch ex As Exception
            LastError = ex.Message
            listener = Nothing
            Return False
        End Try

        listenerThread = New Thread(AddressOf ListenLoop)
        listenerThread.IsBackground = True
        listenerThread.Start()
        Return True
    End Function

    Public Sub [Stop]()
        Try
            If listener IsNot Nothing Then
                listener.Stop()
                listener.Close()
            End If
        Catch ex As Exception
            ' Fehler beim Beenden sind hier irrelevant - der Server soll in jedem Fall als
            ' gestoppt gelten.
        End Try
        listener = Nothing
    End Sub

    ' Vom Scorer nach jeder Zustandsänderung aufgerufen: baut das JSON EINMAL zentral, damit
    ' jede eingehende Anfrage sofort aus dem Speicher bedient werden kann, ohne für jede
    ' Anfrage neu über Engine/Settings/Spielerdaten zu lesen.
    Public Sub UpdateState(json As String)
        SyncLock stateLock
            currentJson = json
        End SyncLock
    End Sub

    Private Sub ListenLoop()
        Dim activeListener = listener
        While activeListener IsNot Nothing AndAlso activeListener.IsListening
            Try
                Dim context = activeListener.GetContext()
                HandleRequest(context)
            Catch ex As HttpListenerException
                ' Tritt regulär auf, wenn Stop() während eines wartenden GetContext() läuft
                Exit While
            Catch ex As Exception
                ' Eine einzelne fehlerhafte Anfrage soll den Server nicht abwürgen
            End Try
        End While
    End Sub

    Private Sub HandleRequest(context As HttpListenerContext)
        Dim response = context.Response
        Try
            ' CORS offen, damit z.B. eine Browser-Source oder eigene Web-Seite im selben
            ' Netzwerk die Daten per fetch() direkt abrufen kann.
            response.Headers.Add("Access-Control-Allow-Origin", "*")
            response.ContentType = "application/json; charset=utf-8"

            Dim body As String
            SyncLock stateLock
                body = currentJson
            End SyncLock

            Dim buffer As Byte() = Encoding.UTF8.GetBytes(body)
            response.ContentLength64 = buffer.Length
            response.OutputStream.Write(buffer, 0, buffer.Length)
        Catch ex As Exception
            ' z.B. wenn der Client die Verbindung vorzeitig schliesst
        Finally
            response.OutputStream.Close()
        End Try
    End Sub

End Class

' Baut ein JSON-Objekt aus einzelnen Feldern zusammen, ohne Kommas/Klammern von Hand zählen
' zu müssen - genau das war in diesem Projekt wiederholt eine Fehlerquelle bei Handarbeit an
' vergleichbaren Aufzählungen (siehe Statistik-Zeilenindizes).
Public Class JsonObjectBuilder
    Private ReadOnly parts As New List(Of String)

    Public Function AddString(key As String, value As String) As JsonObjectBuilder
        parts.Add($"""{JsonObjectBuilder.Escape(key)}"":""{JsonObjectBuilder.Escape(value)}""")
        Return Me
    End Function

    Public Function AddInt(key As String, value As Integer) As JsonObjectBuilder
        parts.Add($"""{JsonObjectBuilder.Escape(key)}"":{value}")
        Return Me
    End Function

    Public Function AddBool(key As String, value As Boolean) As JsonObjectBuilder
        parts.Add($"""{JsonObjectBuilder.Escape(key)}"":{If(value, "true", "false")}")
        Return Me
    End Function

    Public Function AddIntArray(key As String, values As IEnumerable(Of Integer)) As JsonObjectBuilder
        parts.Add($"""{JsonObjectBuilder.Escape(key)}"":[{String.Join(",", values)}]")
        Return Me
    End Function

    ' Für bereits fertige JSON-Fragmente (z.B. ein verschachteltes Objekt aus einem
    ' zweiten JsonObjectBuilder.ToString()).
    Public Function AddRaw(key As String, rawJson As String) As JsonObjectBuilder
        parts.Add($"""{JsonObjectBuilder.Escape(key)}"":{rawJson}")
        Return Me
    End Function

    Public Overrides Function ToString() As String
        Return "{" & String.Join(",", parts) & "}"
    End Function

    Public Shared Function Escape(value As String) As String
        If value Is Nothing Then Return ""
        Dim sb As New StringBuilder()
        For Each c As Char In value
            Select Case c
                Case """"c : sb.Append("\""")
                Case "\"c : sb.Append("\\")
                Case Chr(8) : sb.Append("\b")
                Case Chr(9) : sb.Append("\t")
                Case Chr(10) : sb.Append("\n")
                Case Chr(12) : sb.Append("\f")
                Case Chr(13) : sb.Append("\r")
                Case Else
                    If AscW(c) < &H20 Then
                        sb.Append("\u" & Convert.ToInt32(c).ToString("x4"))
                    Else
                        sb.Append(c)
                    End If
            End Select
        Next
        Return sb.ToString()
    End Function
End Class
