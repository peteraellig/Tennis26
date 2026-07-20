Imports System.Net.Sockets
Imports System.Text

' Versendet vMix-Befehle über die TCP-API statt per HTTP - schneller, weil die Verbindung
' offen bleibt statt bei jedem einzelnen Befehl neu aufgebaut zu werden (HTTP-Handshake pro
' Aufruf entfällt). Nutzt dieselbe "Function=X&Param=Y&..."-Syntax wie die HTTP-API; das
' Protokoll unterscheidet sich nur im Rahmen: eine Zeile "FUNCTION X Param=Y&...\r\n" statt
' eines GET-Requests. Port kommt aus Settings-TextBox9 (Standard 8099, siehe
' Tennis24_Settings.vb) statt aus dem HTTP-Port (TextBoxValues(46)).

Public Class VmixTcpSender
    Implements IVmixSender, IDisposable

    Private client As TcpClient
    Private stream As NetworkStream
    Private ReadOnly connectionLock As New Object()
    Private lastCommandValue As String = ""

    Public ReadOnly Property LastCommand As String Implements IVmixSender.LastCommand
        Get
            Return lastCommandValue
        End Get
    End Property

    ' Stellt sicher, dass eine Verbindung besteht - verbindet bei Bedarf (neu), z.B. beim
    ' allerersten Befehl oder nachdem vMix zwischenzeitlich geschlossen/neugestartet wurde.
    Private Sub EnsureConnected()
        If client IsNot Nothing AndAlso client.Connected Then Return

        DisconnectInternal()

        Dim ip As String = Tennis24_Settings.TextBoxValues(45)
        Dim port As Integer = 8099
        Integer.TryParse(Tennis24_Settings.TextBoxValues(9), port)

        client = New TcpClient()
        client.Connect(ip, port)
        stream = client.GetStream()
    End Sub

    Public Function Send(command As String) As String Implements IVmixSender.Send
        SyncLock connectionLock
            Try
                EnsureConnected()

                ' "Function=SetText&Input=...&Value=..." -> "FUNCTION SetText Input=...&Value=..."
                Dim parts = command.Split({"&"c}, 2)
                Dim functionName As String = parts(0)
                If functionName.StartsWith("Function=") Then
                    functionName = functionName.Substring("Function=".Length)
                End If
                Dim remainder As String = If(parts.Length > 1, parts(1), "")

                Dim line As String = If(remainder = "", $"FUNCTION {functionName}", $"FUNCTION {functionName} {remainder}")
                lastCommandValue = line

                Dim bytes = Encoding.ASCII.GetBytes(line & vbCrLf)
                stream.Write(bytes, 0, bytes.Length)

                Return ReadResponseLine()
            Catch ex As Exception
                DisconnectInternal()
                Return "Exception Error in VTX-TCP (vMix running? TCP-API aktiv?): " & ex.Message
            End Try
        End SyncLock
    End Function

    ' Liest eine Antwort (vMix bestätigt jeden FUNCTION-Befehl, z.B. "FUNCTION OK" oder
    ' "FUNCTION FAILED ..."). Kurzer Timeout, damit ein Befehl, den vMix aus irgendeinem
    ' Grund nicht beantwortet, das Match nicht blockiert.
    Private Function ReadResponseLine() As String
        stream.ReadTimeout = 500
        Dim buffer(1023) As Byte
        Try
            Dim bytesRead = stream.Read(buffer, 0, buffer.Length)
            Return Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim()
        Catch ex As IO.IOException
            ' Timeout - vMix hat (noch) nicht geantwortet; kein harter Fehler
            Return "(keine Antwort)"
        End Try
    End Function

    Private Sub DisconnectInternal()
        Try
            If stream IsNot Nothing Then stream.Close()
            If client IsNot Nothing Then client.Close()
        Catch ex As Exception
            ' Fehler beim Trennen sind hier irrelevant - Verbindung gilt danach als zu
        End Try
        stream = Nothing
        client = Nothing
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        SyncLock connectionLock
            DisconnectInternal()
        End SyncLock
    End Sub

End Class
