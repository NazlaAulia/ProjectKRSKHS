Imports MySql.Data.MySqlClient

Public Class Form1
    Dim conn As New MySqlConnection("server=localhost;user id=root;password=;database=krskhs")
    Dim cmd As MySqlCommand
    Dim dr As MySqlDataReader

    Public Shared roleUser As String
    Public Shared emailUser As String


    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load



        CheckBox1.Checked = False                 ' awalnya tidak dicentang
        TextBox2.UseSystemPasswordChar = True     ' sembunyikan password
        TextBox2.Multiline = False
    End Sub
    Private Sub Label1_Click(sender As Object, e As EventArgs) Handles Label1.Click

    End Sub

    Private Sub Label2_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub Label3_Click(sender As Object, e As EventArgs) Handles Label3.Click

    End Sub

    Private Sub Label5_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub Panel3_Paint(sender As Object, e As PaintEventArgs) Handles Panel3.Paint

    End Sub

    Private Sub Panel1_Paint(sender As Object, e As PaintEventArgs) Handles Panel1.Paint

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        Dim email As String = TextBox1.Text
        Dim password As String = TextBox2.Text

        ' Hardcode untuk admin
        If email = "admin@system.com" And password = "admin123" Then
            MessageBox.Show("Login sebagai Admin")
            Admin.Show()
            Me.Hide()
            Exit Sub
        End If

        ' Koneksi database
        conn = New MySqlConnection("server=localhost;user id=root;password=;database=krskhs")
        conn.Open()

        ' Cek di database
        cmd = New MySqlCommand("SELECT role FROM login WHERE email=@e AND password=@p", conn)
        cmd.Parameters.AddWithValue("@e", email)
        cmd.Parameters.AddWithValue("@p", password)

        dr = cmd.ExecuteReader()
        If dr.Read() Then
            Dim role As String = dr("role").ToString()
            Form1.emailUser = email
            Form1.roleUser = role
            If role = "mahasiswa" Then
                MessageBox.Show("Login sebagai Mahasiswa")
                krsvb.Show()
            ElseIf role = "dosen" Then
                MessageBox.Show("Login sebagai Dosen")

                input.Show()
            End If
            Me.Hide()
        Else
            MessageBox.Show("Email atau Password salah!")
        End If

        conn.Close()
    End Sub





    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged

        If CheckBox1.Checked Then
            TextBox2.UseSystemPasswordChar = False   ' tampilkan sandi

        Else
            TextBox2.UseSystemPasswordChar = True    ' sembunyikan sandi

        End If
    End Sub



    Private Sub TextBox2_TextChanged(sender As Object, e As EventArgs) Handles TextBox2.TextChanged

    End Sub

    Private Sub Label4_Click(sender As Object, e As EventArgs) Handles Label4.Click

    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged

    End Sub

    Private Sub Panel2_Paint(sender As Object, e As PaintEventArgs)

    End Sub

    Private Sub Panel4_Paint(sender As Object, e As PaintEventArgs) Handles Panel4.Paint

    End Sub

    Private Sub Label6_Click(sender As Object, e As EventArgs) Handles Label6.Click

    End Sub

    Private Sub Panel5_Paint(sender As Object, e As PaintEventArgs) Handles Panel5.Paint

    End Sub

    Private Sub ButtonForgot_Click(sender As Object, e As EventArgs) Handles ButtonForgot.Click
        Dim email As String = InputBox("Masukkan email akun Anda:", "Lupa Password")

        ' Kalau kosong atau dibatalkan
        If String.IsNullOrWhiteSpace(email) Then Exit Sub

        Try
            If conn.State = ConnectionState.Open Then conn.Close()
            conn.Open()

            ' Ambil nama dan role dari database
            cmd = New MySqlCommand("SELECT username, role FROM login WHERE email=@e", conn)
            cmd.Parameters.AddWithValue("@e", email)
            dr = cmd.ExecuteReader()

            If dr.Read() Then
                Dim namaUser As String = dr("username").ToString()
                Dim role As String = dr("role").ToString()
                dr.Close()

                Dim adminEmail As String = "admin@system.com"
                Dim infoPesan As String = ""

                If role = "mahasiswa" Then
                    infoPesan = "Akun ditemukan atas nama *Mahasiswa*." & vbCrLf &
                            "Nama: " & namaUser & vbCrLf &
                            "Silakan hubungi admin untuk reset password Anda." & vbCrLf &
                            "Kontak admin: " & adminEmail
                ElseIf role = "dosen" Then
                    infoPesan = "Akun ditemukan atas nama *Dosen*." & vbCrLf &
                            "Nama: " & namaUser & vbCrLf &
                            "Silakan hubungi admin untuk reset password Anda." & vbCrLf &
                            "Kontak admin: " & adminEmail
                Else
                    infoPesan = "Akun admin tidak dapat direset melalui halaman ini." & vbCrLf &
                            "Jika Anda admin dan mengalami kendala, silakan cek panel manajemen."
                End If

                MessageBox.Show(infoPesan, "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information)

            Else
                MessageBox.Show(
                "Email tidak ditemukan di sistem." & vbCrLf &
                "Pastikan Anda sudah mendaftar atau gunakan email UNESA yang benar.",
                "Peringatan",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            )
            End If

        Catch ex As Exception
            MessageBox.Show("Terjadi kesalahan: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            conn.Close()
        End Try

    End Sub
End Class