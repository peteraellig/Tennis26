' Versendet vMix-Befehle wie bisher per HTTP-GET an die vMix-Web-API
' (http://IP:Port/API/?Function=...) - inhaltlich unverändert aus Tennis26_Scorer.SendHTMLtovMix
' herausgelöst, damit dieselbe Logik hinter IVmixSender auswählbar wird.
Public Class VmixHttpSender
    Implements IVmixSender

    Private lastCommandValue As String = ""

    Public ReadOnly Property LastCommand As String Implements IVmixSender.LastCommand
        Get
            Return lastCommandValue
        End Get
    End Property

    Public Function Send(command As String) As String Implements IVmixSender.Send
        Dim url As String = "http://" + Tennis26_Settings.TextBoxValues(45) + ":" + Tennis26_Settings.TextBoxValues(46) + "/API/?" + command
        lastCommandValue = url

        Try
            Dim cookieJar As New Net.CookieContainer()
            Dim hwrequest As Net.HttpWebRequest = Net.WebRequest.Create(url)
            hwrequest.CookieContainer = cookieJar
            hwrequest.Accept = "*/*"
            hwrequest.AllowAutoRedirect = True
            hwrequest.UserAgent = "http_requester/0.1"
            hwrequest.Method = "GET"
            hwrequest.Timeout = 30

            Dim hwresponse As Net.HttpWebResponse = hwrequest.GetResponse()
            Dim responseData As String = ""
            If hwresponse.StatusCode = Net.HttpStatusCode.OK Then
                Dim responseStream As New IO.StreamReader(hwresponse.GetResponseStream())
                responseData = responseStream.ReadToEnd()
            End If
            hwresponse.Close()
            Return responseData
        Catch ex As Exception
            Return "Exception Error in VTX (vMix running?): " & ex.Message
        End Try
    End Function

End Class
