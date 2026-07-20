' Gemeinsame Abstraktion für den Versand von vMix-Befehlen - ermöglicht den Wechsel
' zwischen HTTP- und TCP-API (Settings: RadioButton3/4), ohne die ~80 Aufrufstellen in
' Tennis26_Scorer.vb anzufassen. Die bauen weiterhin denselben "Function=X&Param=Y&..."-
' String wie bisher für die HTTP-API; jede Implementierung übersetzt ihn in ihr eigenes
' Protokoll (siehe VmixHttpSender/VmixTcpSender).
Public Interface IVmixSender

    ' Was zuletzt tatsächlich über die Leitung ging (volle URL bei HTTP, TCP-Zeile bei TCP) -
    ' rein informativ für die Fehleranzeige (Label12) im Scorer.
    ReadOnly Property LastCommand As String

    ' command: "Function=X&Param=Y&..." wie bisher. Rückgabe: Antworttext/Statusmeldung für
    ' die Fehleranzeige (Label7) - wirft bewusst keine Ausnahme nach aussen, ein einzelner
    ' fehlgeschlagener Befehl soll das laufende Match nicht stoppen.
    Function Send(command As String) As String

End Interface
