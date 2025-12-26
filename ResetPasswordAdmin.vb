Imports MySql.Data.MySqlClient

Public Class ResetPasswordAdmin
    Dim conn As New MySqlConnection("server=localhost;user id=root;password=;database=krskhs")
    Dim cmd As MySqlCommand

    Private Sub ResetPasswordAdmin(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Pastikan textbox password pakai masking saat form dibuka
        txtEmail.UseSystemPasswordChar = True
        txtPasswordBaru.UseSystemPasswordChar = True


        ' Default: checkbox tidak dicentang (sandi tersembunyi)
        CheckBox1.Checked = False
    End Sub

    Private Sub btnReset_Click(sender As Object, e As EventArgs) Handles btnReset.Click
        Dim email As String = txtEmail.Text.Trim()
        Dim newPass As String = txtPasswordBaru.Text.Trim()

        ' Validasi
        If String.IsNullOrWhiteSpace(email) OrElse String.IsNullOrWhiteSpace(newPass) Then
            MessageBox.Show("Isi email dan password baru terlebih dahulu.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        Try
            conn.Open()
            cmd = New MySqlCommand("UPDATE login SET password=@p WHERE email=@e", conn)
            cmd.Parameters.AddWithValue("@p", newPass)
            cmd.Parameters.AddWithValue("@e", email)

            Dim rowsAffected As Integer = cmd.ExecuteNonQuery()

            If rowsAffected > 0 Then
                MessageBox.Show("Password untuk " & email & " berhasil direset!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information)
                txtEmail.Clear()
                txtPasswordBaru.Clear()
            Else
                MessageBox.Show("Email tidak ditemukan di sistem.", "Gagal", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If

        Catch ex As Exception
            MessageBox.Show("Terjadi kesalahan: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            conn.Close()
        End Try
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        Dim show As Boolean = CheckBox1.Checked
        txtEmail.UseSystemPasswordChar = Not show
        txtPasswordBaru.UseSystemPasswordChar = Not show

    End Sub
End Class
