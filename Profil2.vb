Imports MySql.Data.MySqlClient
Imports System.IO

Public Class Profil2

    ' Properti untuk menampung NIM mahasiswa yang dikirim dari form approve
    Public Property NimAktif As String

    ' Koneksi database
    Private ReadOnly ConnStr As String = "server=localhost;user id=root;password=;database=krskhs"

    Private Sub Profil2_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        KunciTextBox()  'buat textbox ga bsia di edit
        ' Pastikan NIM dikirim dari form sebelumnya
        If String.IsNullOrEmpty(NimAktif) Then
            MessageBox.Show("Data mahasiswa tidak ditemukan.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Me.Close()
            Return
        End If

        ' Tampilkan data mahasiswa
        TampilDataMahasiswa()
    End Sub

    Private Sub TampilDataMahasiswa()
        Try
            Using conn As New MySqlConnection(ConnStr)
                conn.Open()

                Dim query As String = "SELECT nim, nama, prodi, fakultas, email, tgl_lahir, foto FROM mahasiswa WHERE nim = @nim"

                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@nim", NimAktif)

                    Using rd As MySqlDataReader = cmd.ExecuteReader()
                        If rd.Read() Then

                            ' ISI LABEL & TEXTBOX SESUAI DATA

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


                            ' FOTO MAHASISWA

                            If Not IsDBNull(rd("foto")) Then
                                Dim imgBytes() As Byte = DirectCast(rd("foto"), Byte())
                                Using ms As New MemoryStream(imgBytes)
                                    PictureBox1.Image = Image.FromStream(ms)
                                End Using
                            Else
                                SetDefaultFoto()
                            End If
                        Else
                            MessageBox.Show("Data mahasiswa tidak ditemukan di database.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information)
                            Me.Close()
                        End If
                    End Using
                End Using
            End Using

        Catch ex As Exception
            MessageBox.Show("Terjadi kesalahan saat memuat data: " & ex.Message)
        End Try
    End Sub

    Private Sub SetDefaultFoto()
        Try
            Dim defaultPath As String = Application.StartupPath & "\FotoMahasiswa\default_user.png"
            If IO.File.Exists(defaultPath) Then
                Using tempImg As Image = Image.FromFile(defaultPath)
                    PictureBox1.Image = New Bitmap(tempImg)
                End Using
            Else
                PictureBox1.Image = Nothing
            End If
        Catch
            PictureBox1.Image = Nothing
        End Try
    End Sub

    Private Sub Panel2_Paint(sender As Object, e As PaintEventArgs) Handles Panel2.Paint
        ' Kosong (biarkan default)
    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged
    End Sub

    Private Sub TextBox2_TextChanged(sender As Object, e As EventArgs) Handles TextBox2.TextChanged
    End Sub

    Private Sub TextBox3_TextChanged(sender As Object, e As EventArgs) Handles TextBox3.TextChanged
    End Sub

    Private Sub TextBox5_TextChanged(sender As Object, e As EventArgs) Handles TextBox5.TextChanged
    End Sub

    Private Sub TextBox4_TextChanged(sender As Object, e As EventArgs) Handles TextBox4.TextChanged
    End Sub

    Private Sub TextBox6_TextChanged(sender As Object, e As EventArgs) Handles TextBox6.TextChanged
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Dim aprove1 As New Aprove1()
        aprove1.Show()
        Me.Close()
    End Sub

    Private Sub Panel3_Paint(sender As Object, e As PaintEventArgs) Handles Panel3.Paint

    End Sub
    Private Sub KunciTextBox()
        ' Semua textbox hanya bisa dibaca, tidak bisa diketik
        TextBox1.ReadOnly = True
        TextBox2.ReadOnly = True
        TextBox3.ReadOnly = True
        TextBox4.ReadOnly = True
        TextBox5.ReadOnly = True
        TextBox6.ReadOnly = True
    End Sub
End Class