' = FormProfil.vb 
Imports MySql.Data.MySqlClient
Imports System.IO
Imports System.Drawing.Imaging
Imports System.Drawing.Drawing2D

Public Class Profill
    Public Property NimAktif As String
    Private ReadOnly CS As String = "server=localhost;user id=root;password=;database=krskhs"
    Private fotoBaru As Byte() = Nothing

    ' FORM LOAD 
    Private Sub Profill_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        SetViewOnly()

        ' Bentuk foto jadi bulat
        AddHandler PictureBox1.SizeChanged, Sub()
                                                Dim gp As New GraphicsPath()
                                                gp.AddEllipse(0, 0, PictureBox1.Width - 1, PictureBox1.Height - 1)
                                                PictureBox1.Region = New Region(gp)
                                            End Sub

        ' Tooltip saat hover di foto
        Dim tip As New ToolTip()
        tip.SetToolTip(PictureBox1, "Klik untuk mengganti foto profil") 'buat kalau dikik bisa ganti foto


        Dim email = Form1.emailUser
        LoadProfilByEmail(email)
        LoadFotoBlobByEmail(email)

        If String.IsNullOrEmpty(NimAktif) Then
            NimAktif = GetNimByEmail(email)
        End If
    End Sub

    Private Sub SetViewOnly()
        For Each tb In New TextBox() {TextBox1, TextBox2, TextBox3, TextBox4, TextBox5, TextBox6}
            tb.ReadOnly = True
            tb.BackColor = Color.White
        Next
    End Sub

    '  LOAD PROFIL 
    Private Sub LoadProfilByEmail(email As String)
        Dim sql As String =
        "SELECT nim, nama, prodi, fakultas, email, tgl_lahir FROM mahasiswa WHERE email = @e LIMIT 1;"

        Try
            Using conn As New MySqlConnection(CS)
                conn.Open()
                Using cmd As New MySqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@e", email)
                    Using rd As MySqlDataReader = cmd.ExecuteReader()
                        If rd.Read() Then
                            Label1.Text = rd("nama").ToString()
                            LabelNim.Text = "NIM " & rd("nim").ToString()
                            Label3.Text = $"{rd("prodi")} • {rd("fakultas")}"
                            TextBox1.Text = rd("nim").ToString()
                            TextBox2.Text = rd("nama").ToString()
                            TextBox3.Text = rd("prodi").ToString()
                            TextBox5.Text = rd("fakultas").ToString()
                            TextBox4.Text = rd("email").ToString()
                            If Not rd.IsDBNull(rd.GetOrdinal("tgl_lahir")) Then
                                Dim tgl As Date = CDate(rd("tgl_lahir"))
                                TextBox6.Text = tgl.ToString("dd MMMM yyyy")
                            Else
                                TextBox6.Text = "-"
                            End If
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Terjadi kesalahan: " & ex.Message)
        End Try
    End Sub

    Private Sub LoadFotoBlobByEmail(email As String)
        Try
            Using conn As New MySqlConnection(CS)
                conn.Open()
                Using cmd As New MySqlCommand("SELECT foto FROM mahasiswa WHERE email=@e LIMIT 1;", conn)
                    cmd.Parameters.AddWithValue("@e", email)
                    Using rd As MySqlDataReader = cmd.ExecuteReader()
                        If rd.Read() AndAlso Not rd.IsDBNull(0) Then
                            Dim bytes = DirectCast(rd(0), Byte())
                            If PictureBox1.Image IsNot Nothing Then PictureBox1.Image.Dispose()
                            Using ms As New MemoryStream(bytes)
                                PictureBox1.Image = Image.FromStream(ms)
                            End Using
                        Else
                            SetDefaultFoto()
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Gagal memuat foto: " & ex.Message)
            SetDefaultFoto()
        End Try
    End Sub

    '  UPLOAD & SIMPAN FOTO 
    Private Sub PictureBox1_Click(sender As Object, e As EventArgs) Handles PictureBox1.Click
        Using dlg As New OpenFileDialog()
            dlg.Title = "Pilih Foto Profil"
            dlg.Filter = "Gambar|*.jpg;*.jpeg;*.png;*.bmp"
            If dlg.ShowDialog() = DialogResult.OK Then
                Dim info As New FileInfo(dlg.FileName)
                If info.Length > 2 * 1024 * 1024 Then
                    MessageBox.Show("Ukuran file terlalu besar. Maksimum 2MB.", "Peringatan")
                    Return
                End If

                Using img = Image.FromFile(dlg.FileName)
                    Dim sq = ResizeToSquare(img, 512)
                    PictureBox1.Image = sq
                    fotoBaru = ImageToJpgBytes(sq, 90)
                End Using

                If fotoBaru IsNot Nothing Then
                    If String.IsNullOrEmpty(NimAktif) Then
                        MessageBox.Show("NIM belum diketahui. Foto tidak dapat disimpan.", "Kesalahan")
                        Return
                    End If
                    SaveFotoToDB(NimAktif, fotoBaru)
                    fotoBaru = Nothing
                    MessageBox.Show("Foto berhasil diunggah & disimpan.", "Profil")
                End If
            End If
        End Using
    End Sub

    Private Sub SaveFotoToDB(nim As String, bytes As Byte())
        Try
            Using conn As New MySqlConnection(CS)
                conn.Open()
                Dim sql = "UPDATE mahasiswa SET foto=@f, foto_mime=@m, foto_updated_at=NOW() WHERE nim=@nim"
                Using cmd As New MySqlCommand(sql, conn)
                    cmd.Parameters.Add("@f", MySqlDbType.LongBlob).Value = bytes
                    cmd.Parameters.AddWithValue("@m", "image/jpeg")
                    cmd.Parameters.AddWithValue("@nim", nim)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Gagal menyimpan foto: " & ex.Message)
        End Try
    End Sub

    '  UTIL 
    Private Function ResizeToSquare(img As Image, size As Integer) As Bitmap 'biar foto ga gepeng
        Dim bmp As New Bitmap(size, size)
        Using g = Graphics.FromImage(bmp)
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.InterpolationMode = InterpolationMode.HighQualityBicubic ' biar gambar ga pecah
            g.Clear(Color.White)
            Dim r = Math.Min(size / img.Width, size / img.Height)
            Dim w = CInt(img.Width * r)
            Dim h = CInt(img.Height * r)
            Dim x = (size - w) \ 2
            Dim y = (size - h) \ 2
            g.DrawImage(img, x, y, w, h)
        End Using
        Return bmp
    End Function

    Private Function ImageToJpgBytes(image As Image, quality As Long) As Byte()
        Dim codec = ImageCodecInfo.GetImageEncoders().First(Function(c) c.FormatID = ImageFormat.Jpeg.Guid)
        Dim ep As New EncoderParameters(1)
        ep.Param(0) = New EncoderParameter(Encoder.Quality, CLng(quality))
        Using ms As New MemoryStream()
            image.Save(ms, codec, ep)
            Return ms.ToArray()
        End Using
    End Function

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        PictureBox1_Click(sender, e)
    End Sub

    Private Function GetEmailByNIM(nim As String) As String
        Dim result As String = ""
        Try
            Using cn As New MySqlConnection(CS)
                cn.Open()
                Using cmd As New MySqlCommand("SELECT email FROM mahasiswa WHERE nim=@nim LIMIT 1;", cn)
                    cmd.Parameters.AddWithValue("@nim", nim)
                    Dim obj = cmd.ExecuteScalar()
                    If obj IsNot Nothing AndAlso obj IsNot DBNull.Value Then
                        result = obj.ToString()
                    End If
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Gagal mengambil email: " & ex.Message)
        End Try
        Return result
    End Function

    Private Function GetNimByEmail(email As String) As String
        Try
            Using conn As New MySqlConnection(CS)
                conn.Open()
                Using cmd As New MySqlCommand("SELECT nim FROM mahasiswa WHERE email=@e LIMIT 1;", conn)
                    cmd.Parameters.AddWithValue("@e", email)
                    Dim result = cmd.ExecuteScalar()
                    If result IsNot Nothing Then Return result.ToString()
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Gagal mengambil NIM: " & ex.Message)
        End Try
        Return ""
    End Function

    Private Sub SetDefaultFoto()
        Try
            Dim defaultPath As String = Application.StartupPath & "\FotoMahasiswa\default_user.png"
            If IO.File.Exists(defaultPath) Then
                PictureBox1.Image = Image.FromFile(defaultPath)
            Else
                PictureBox1.Image = Nothing
            End If
        Catch
            PictureBox1.Image = Nothing
        End Try
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Dim f As New krsvb()
        f.Show()
        Me.Close()
    End Sub

    Private Sub Panel2_Paint(sender As Object, e As PaintEventArgs) Handles Panel2.Paint

    End Sub

    Private Sub Panel3_Paint(sender As Object, e As PaintEventArgs) Handles Panel3.Paint

    End Sub
End Class