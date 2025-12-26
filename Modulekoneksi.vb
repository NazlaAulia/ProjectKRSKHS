Imports MySql.Data.MySqlClient

Module ModuleKoneksi
    Public conn As MySqlConnection
    Public cmd As MySqlCommand
    Public dr As MySqlDataReader
    Public da As MySqlDataAdapter
    Public dt As DataTable

    Sub koneksi()
        Try
            conn = New MySqlConnection("server=localhost;user id=root;password=;database=krskhs")
            If conn.State = ConnectionState.Closed Then
                conn.Open()
            End If
        Catch ex As Exception
            MsgBox("Koneksi gagal: " & ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub
End Module