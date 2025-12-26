Imports MySql.Data.MySqlClient

Public Class FormGantiPassword
    Private conn As New MySqlConnection("server=localhost;user id=root;password=;database=krskhs")
    Private cmd As MySqlCommand
    Private dr As MySqlDataReader

    'Flag untuk menandai kalau kita sudah mengarahkan user keluar
    Private redirected As Boolean = False

    Private Sub FormGantiPassword_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'Pastikan textbox password pakai masking saat form dibuka
        txtOldPass.UseSystemPasswordChar = True
        txtNewPass.UseSystemPasswordChar = True
        txtConfirm.UseSystemPasswordChar = True

        'Default: checkbox tidak dicentang (sandi tersembunyi)
        CheckBox1.Checked = False
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Dim oldPass As String = txtOldPass.Text.Trim()
        Dim newPass As String = txtNewPass.Text.Trim()
        Dim confirmPass As String = txtConfirm.Text.Trim()
        Dim email As String = Form1.emailUser  ' email user yg sedang login

        ' Validasi input
        If oldPass = "" Or newPass = "" Or confirmPass = "" Then
            MessageBox.Show("Semua kolom harus diisi.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If newPass <> confirmPass Then
            MessageBox.Show("Konfirmasi password tidak cocok.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            conn.Open()

            ' 1) verifikasi password lama ada dan cocok untuk email ini
            Using cmd = New MySqlCommand("SELECT password FROM login WHERE email=@e", conn)
                cmd.Parameters.AddWithValue("@e", email)
                Using reader = cmd.ExecuteReader()
                    If reader.Read() Then
                        Dim currentPwdInDb As String = If(reader("password") Is DBNull.Value, "", reader("password").ToString())
                        If currentPwdInDb <> oldPass Then
                            MessageBox.Show("Password lama salah.", "Gagal", MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Return
                        End If
                    Else
                        MessageBox.Show("Akun tidak ditemukan.", "Gagal", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        Return
                    End If
                End Using
            End Using

            ' 2) update password (gunakan statement baru, conn masih terbuka)
            Using cmd = New MySqlCommand("UPDATE login SET password=@n WHERE email=@e", conn)
                cmd.Parameters.AddWithValue("@n", newPass)
                cmd.Parameters.AddWithValue("@e", email)
                Dim rowsAffected = cmd.ExecuteNonQuery()
                If rowsAffected > 0 Then
                    MessageBox.Show("Password berhasil diubah!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information)

                    ' setelah sukses, langsung arahkan sesuai role
                    RedirectToRoleForm()
                    redirected = True
                    Return
                Else
                    MessageBox.Show("Gagal mengubah password (tidak ada baris terpengaruh).", "Gagal", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End If
            End Using

        Catch ex As Exception
            MessageBox.Show("Terjadi kesalahan: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            ' Tutup koneksi bila masih terbuka
            If conn IsNot Nothing AndAlso conn.State = ConnectionState.Open Then
                conn.Close()
            End If

            ' Jika belum diarahkan (mis. update gagal), tetap kembali ke form sesuai role
            If Not redirected Then
                RedirectToRoleForm()
                redirected = True
            End If

            ' Tutup form ganti password
            Me.Close()
        End Try
    End Sub



    ' Tombol di UI (Button4) -> kembalikan juga sesuai role
    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        RedirectToRoleForm()
        Me.Close()
    End Sub

    ' Checkbox untuk show/hide semua password textbox sekaligus
    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        Dim show As Boolean = CheckBox1.Checked
        txtOldPass.UseSystemPasswordChar = Not show
        txtNewPass.UseSystemPasswordChar = Not show
        txtConfirm.UseSystemPasswordChar = Not show
    End Sub

    ' Sub helper untuk mengarahkan user kembali sesuai role
    Private Sub RedirectToRoleForm()
        Try
            ' Pastikan koneksi ditutup sebelum pindah form
            If conn IsNot Nothing AndAlso conn.State = ConnectionState.Open Then
                conn.Close()
            End If
        Catch
            ' ignore
        End Try

        ' Gunakan Form1.roleUser (harus sudah di-set saat login)
        Try
            If String.IsNullOrEmpty(Form1.roleUser) Then
                ' fallback default: kalau tidak diketahui, buka krsvb
                Dim fDefault As New krsvb()
                fDefault.Show()
                Return
            End If

            If Form1.roleUser.ToLower() = "mahasiswa" Then
                Dim f As New krsvb()
                f.Show()
            ElseIf Form1.roleUser.ToLower() = "dosen" Then
                Dim f As New input()
                f.Show()
            Else
                ' fallback
                Dim fDefault As New krsvb()
                fDefault.Show()
            End If
        Catch ex As Exception
            ' kalau terjadi error saat membuka form tujuan, tunjukkan pesan dan coba buka krsvb
            MessageBox.Show("Gagal membuka form tujuan: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Try
                Dim fDefault As New krsvb()
                fDefault.Show()
            Catch
            End Try
        End Try
    End Sub
End Class
