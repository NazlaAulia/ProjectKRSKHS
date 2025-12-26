Imports MySql.Data.MySqlClient
Imports System.IO

Public Class profildosen
    ' ===== KONFIG KONEKSI =====
    Private ReadOnly conn As New MySqlConnection("server=localhost;user id=root;password=;database=krskhs")

    ' ===== STATE =====
    Private originalEmail As String = ""          ' email saat load, untuk WHERE saat update
    Private pendingFotoBytes As Byte() = Nothing  ' foto baru (kalau upload)

    Private Sub profildosen_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Atur PictureBox (tambahkan PictureBox1 di Designer)
        If PictureBox1 IsNot Nothing Then
            PictureBox1.SizeMode = PictureBoxSizeMode.Zoom
            PictureBox1.BorderStyle = BorderStyle.FixedSingle
        End If

        ' Ambil email dari form login
        originalEmail = If(Form1.emailUser, "").Trim()
        If originalEmail = "" Then
            MessageBox.Show("Email login tidak ditemukan.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Me.Close()
            Return
        End If

        MuatProfilDosen(originalEmail)

    End Sub

    Private Sub MuatProfilDosen(email As String)
        Try
            If conn.State = ConnectionState.Open Then conn.Close()
            conn.Open()

            Dim sql As String = "
                SELECT nama, prodi, fakultas, email, foto
                FROM dosen
                WHERE email = @e
                LIMIT 1"
            Using cmd As New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@e", email)
                Using r = cmd.ExecuteReader()
                    If r.Read() Then
                        ' Mapping sesuai permintaan:
                        ' Nama -> TextBox1, Email -> TextBox2, Fakultas -> TextBox3, Prodi -> TextBox4
                        TextBox1.Text = r("nama").ToString()
                        TextBox2.Text = r("email").ToString()
                        TextBox3.Text = r("fakultas").ToString()
                        TextBox4.Text = r("prodi").ToString()

                        ' Tampilkan foto jika ada
                        If Not Convert.IsDBNull(r("foto")) Then
                            Dim bytes = DirectCast(r("foto"), Byte())
                            pendingFotoBytes = bytes ' simpan juga sebagai state saat ini
                            PictureBox1.Image = BytesToImage(bytes)
                        Else
                            pendingFotoBytes = Nothing
                            PictureBox1.Image = Nothing
                        End If
                        ' Kunci semua textbox biar tidak bisa diedit
                        For Each tb As TextBox In {TextBox1, TextBox2, TextBox3, TextBox4}
                            tb.ReadOnly = True
                            tb.TabStop = False
                            tb.BackColor = SystemColors.ControlLight
                        Next

                    Else
                        MessageBox.Show("Data dosen tidak ditemukan.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    End If
                End Using
            End Using

        Catch ex As Exception
            MessageBox.Show("Gagal memuat profil: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            conn.Close()
        End Try
    End Sub


    ' ====== TOMBOL UPLOAD FOTO ======
    Private Sub ButtonUploadFoto_Click(sender As Object, e As EventArgs) Handles ButtonUploadFoto.Click
        Try
            Using ofd As New OpenFileDialog With {
                .Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp",
                .Title = "Pilih Foto"
            }
                If ofd.ShowDialog() <> DialogResult.OK Then Return

                Dim img = Image.FromFile(ofd.FileName)
                PictureBox1.Image = img
                pendingFotoBytes = ImageToBytes(img)
            End Using
        Catch ex As Exception
            MessageBox.Show("Gagal memuat foto: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' ====== TOMBOL SIMPAN ======
    Private Sub ButtonSimpan_Click(sender As Object, e As EventArgs) Handles ButtonSimpan.Click
        ' Ambil nilai dari kontrol
        Dim nama As String = TextBox1.Text.Trim()
        Dim emailBaru As String = TextBox2.Text.Trim()
        Dim fakultas As String = TextBox3.Text.Trim()
        Dim prodi As String = TextBox4.Text.Trim()

        If nama = "" OrElse emailBaru = "" Then
            MessageBox.Show("Nama dan Email wajib diisi.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            If conn.State = ConnectionState.Open Then conn.Close()
            conn.Open()

            Dim sql As String = "
                UPDATE dosen
                SET nama=@n, prodi=@p, fakultas=@f, email=@em, foto=@ft
                WHERE email=@key"
            Using cmd As New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@n", nama)
                cmd.Parameters.AddWithValue("@p", prodi)
                cmd.Parameters.AddWithValue("@f", fakultas)
                cmd.Parameters.AddWithValue("@em", emailBaru)

                If pendingFotoBytes IsNot Nothing AndAlso pendingFotoBytes.Length > 0 Then
                    cmd.Parameters.Add("@ft", MySqlDbType.LongBlob).Value = pendingFotoBytes
                Else
                    cmd.Parameters.Add("@ft", MySqlDbType.LongBlob).Value = DBNull.Value
                End If

                cmd.Parameters.AddWithValue("@key", originalEmail)

                Dim affected = cmd.ExecuteNonQuery()
                If affected > 0 Then
                    MessageBox.Show("Profil berhasil disimpan.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    ' Jika email diganti, perbarui originalEmail agar konsisten
                    originalEmail = emailBaru
                Else
                    MessageBox.Show("Tidak ada data yang diubah.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
            End Using

        Catch ex As Exception
            MessageBox.Show("Gagal menyimpan profil: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            conn.Close()
        End Try
    End Sub

    ' ===== UTIL =====
    Private Function ImageToBytes(img As Image) As Byte()
        Using ms As New MemoryStream()
            ' default simpan sebagai PNG agar lossless
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Png)
            Return ms.ToArray()
        End Using
    End Function

    Private Function BytesToImage(bytes As Byte()) As Image
        Using ms As New MemoryStream(bytes)
            Return Image.FromStream(ms)
        End Using
    End Function

    Private Sub Panel1_Paint(sender As Object, e As PaintEventArgs) Handles Panel1.Paint

    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Dim aprove1 As New Aprove1()
        aprove1.Show()
        Me.Close()
    End Sub

    ' (Opsional) tombol hapus foto

End Class
