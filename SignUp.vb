Imports System.Data.SqlClient
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel
Imports MySql.Data.MySqlClient
Public Class SignUp
    Dim conn As New MySqlConnection("server=localhost;user id=root;password=;database=krskhs")
    Dim cmd As MySqlCommand
    Dim dr As MySqlDataReader


    Private Sub SignUp_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        CheckBox1.Checked = False                 ' awalnya tidak dicentang
        TextBox2.UseSystemPasswordChar = True     ' sembunyikan password
        TextBox2.Multiline = False
    End Sub

    Private Sub Label5_Click(sender As Object, e As EventArgs) Handles Label5.Click

    End Sub

    Private Sub Label2_Click(sender As Object, e As EventArgs) Handles Label2.Click

    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim email As String = TextBox1.Text.Trim()
        Dim password As String = TextBox2.Text.Trim()
        Dim tanggalLahir As String = DateTimePicker1.Value.ToString("yyyy-MM-dd")
        Dim username As String = ""
        Dim role As String = ""

        ' Cek input kosong
        If email = "" Or password = "" Then
            MessageBox.Show("Silakan isi data dulu.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        Try
            conn.Open()

            ' 🔹 Pertama cek di mahasiswa (wajib pakai tanggal lahir)
            cmd = New MySqlCommand("SELECT nama FROM mahasiswa WHERE email=@e AND tgl_lahir=@t", conn)
            cmd.Parameters.AddWithValue("@e", email)
            cmd.Parameters.AddWithValue("@t", tanggalLahir)
            dr = cmd.ExecuteReader()

            If dr.Read() Then
                role = "mahasiswa"
                username = dr("nama").ToString()
            End If
            dr.Close()

            ' 🔹 Kalau belum ketemu di mahasiswa, cek di dosen (tanpa tanggal lahir)
            If role = "" Then
                cmd = New MySqlCommand("SELECT nama FROM dosen WHERE email=@e", conn)
                cmd.Parameters.AddWithValue("@e", email)
                dr = cmd.ExecuteReader()

                If dr.Read() Then
                    role = "dosen"
                    username = dr("nama").ToString()
                End If
                dr.Close()
            End If

            ' 🔹 Kalau gak ada di mahasiswa / dosen
            If role = "" Then
                MessageBox.Show("Email tidak ditemukan di data Mahasiswa/Dosen!", "Gagal", MessageBoxButtons.OK, MessageBoxIcon.Error)
                conn.Close()
                Exit Sub
            End If

            ' 🔹 Simpan akun ke tabel login
            cmd = New MySqlCommand("INSERT INTO login (username, email, password, role) VALUES (@u, @e, @p, @r)", conn)
            cmd.Parameters.AddWithValue("@u", username)
            cmd.Parameters.AddWithValue("@e", email)
            cmd.Parameters.AddWithValue("@p", password)
            cmd.Parameters.AddWithValue("@r", role)
            cmd.ExecuteNonQuery()

            conn.Close()

            MessageBox.Show("Signup berhasil! Silakan login.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Form1.Show()
            Me.Hide()

        Catch ex As Exception
            MessageBox.Show("Terjadi kesalahan: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            conn.Close()
        End Try
    End Sub

    ' 🔹 Lihat / sembunyikan password
    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged

        If CheckBox1.Checked Then
            TextBox2.UseSystemPasswordChar = False   ' tampilkan sandi

        Else
            TextBox2.UseSystemPasswordChar = True    ' sembunyikan sandi

        End If
    End Sub

    Private Sub TextBox2_TextChanged(sender As Object, e As EventArgs) Handles TextBox2.TextChanged

    End Sub

    Private Sub Label1_Click(sender As Object, e As EventArgs) Handles Label1.Click

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Form1.emailUser = Nothing

        Dim f As New Form1()
        f.Show()
        Me.Close()

    End Sub

    Private Sub Panel3_Paint(sender As Object, e As PaintEventArgs) Handles Panel3.Paint

    End Sub
End Class