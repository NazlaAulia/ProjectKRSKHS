Imports MySql.Data.MySqlClient
Imports ClosedXML.Excel

Public Class Template
    Dim conn As New MySqlConnection("server=localhost;user id=root;password=;database=krskhs")

    Private Sub FormDownloadTemplate_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadKelas()

    End Sub

    Private Sub cbKelas_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbKelas.SelectedIndexChanged
        LoadSemesterByKelas()
    End Sub


    Private Sub LoadKelas()
        Try
            conn.Open()
            Dim dt As New DataTable
            Using da As New MySqlDataAdapter("SELECT id_kelas, CONCAT(prodi, ' ', nama_kelas) AS kelas FROM kelas ORDER BY prodi, nama_kelas", conn)
                da.Fill(dt)
            End Using
            cbKelas.DataSource = dt
            cbKelas.DisplayMember = "kelas"
            cbKelas.ValueMember = "id_kelas"
            cbKelas.SelectedIndex = -1
        Catch ex As Exception
            MessageBox.Show("Gagal memuat kelas: " & ex.Message)
        Finally
            conn.Close()
        End Try
    End Sub


    Private Sub LoadSemesterByKelas()
        If cbKelas.SelectedIndex = -1 Then Exit Sub

        ' ⛔ Mencegah error DataRowView
        If cbKelas.SelectedValue Is Nothing Then Exit Sub
        If TypeOf cbKelas.SelectedValue Is DataRowView Then Exit Sub

        Dim idKelas As Integer = CInt(cbKelas.SelectedValue)
        Dim prodi As String = ""

        Try
            conn.Open()

            ' Ambil prodi berdasarkan id_kelas
            Using cmd As New MySqlCommand("SELECT prodi FROM kelas WHERE id_kelas=@id", conn)
                cmd.Parameters.AddWithValue("@id", idKelas)
                prodi = cmd.ExecuteScalar()?.ToString()
            End Using

            ' Ambil semester berdasarkan prodi
            Dim dt As New DataTable
            Using da As New MySqlDataAdapter(
            "SELECT DISTINCT semester FROM matakuliah WHERE prodi=@p ORDER BY semester", conn)
                da.SelectCommand.Parameters.AddWithValue("@p", prodi)
                da.Fill(dt)
            End Using

            ' Isi combobox semester
            cbSemester.Items.Clear()
            For Each row As DataRow In dt.Rows
                cbSemester.Items.Add(row("semester").ToString())
            Next

            cbSemester.SelectedIndex = -1

        Catch ex As Exception
            MessageBox.Show("Gagal memuat semester: " & ex.Message)
        Finally
            conn.Close()
        End Try
    End Sub



    Private Sub btnDownloadTemplate_Click(sender As Object, e As EventArgs) Handles btnDownloadTemplate.Click
        Try
            If cbKelas.SelectedIndex = -1 OrElse cbSemester.Text = "" Then
                MessageBox.Show("Pilih kelas dan semester dulu ya!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Exit Sub
            End If

            Dim idKelas As Integer = CInt(cbKelas.SelectedValue)
            Dim kelasText As String = cbKelas.Text
            Dim semester As Integer = CInt(cbSemester.Text)
            Dim prodi As String = ""
            conn.Open()

            ' Ambil prodi dari tabel kelas
            Using cmd As New MySqlCommand("SELECT prodi FROM kelas WHERE id_kelas=@id", conn)
                cmd.Parameters.AddWithValue("@id", idKelas)
                prodi = cmd.ExecuteScalar()?.ToString()
            End Using

            ' Ambil matkul sesuai prodi & semester
            Dim dtMK As New DataTable()
            Using cmd As New MySqlCommand("
                SELECT kode_mk, nama_mk, semester, sks 
                FROM matakuliah 
                WHERE prodi=@p AND semester=@s
                ORDER BY nama_mk;", conn)
                cmd.Parameters.AddWithValue("@p", prodi)
                cmd.Parameters.AddWithValue("@s", semester)
                Using da As New MySqlDataAdapter(cmd)
                    da.Fill(dtMK)
                End Using
            End Using

            ' === Buat file Excel ===
            Dim wb As New XLWorkbook()
            Dim ws = wb.Worksheets.Add("Template_Jadwal")

            Dim headers() = {"kode_mk", "nama_mk", "semester", "sks", "prodi", "id_kelas", "nidn_pengampu", "nama_dosen_pengampu"}
            For i = 0 To headers.Length - 1
                ws.Cell(1, i + 1).Value = headers(i)
            Next

            ' Isi otomatis
            Dim r As Integer = 2
            For Each row As DataRow In dtMK.Rows
                ws.Cell(r, 1).Value = row("kode_mk").ToString()
                ws.Cell(r, 2).Value = row("nama_mk").ToString()
                ws.Cell(r, 3).Value = row("semester").ToString()
                ws.Cell(r, 4).Value = row("sks").ToString()
                ws.Cell(r, 5).Value = prodi
                ws.Cell(r, 6).Value = idKelas
                ws.Cell(r, 7).Value = ""
                ws.Cell(r, 8).Value = ""
                r += 1
            Next

            Dim sfd As New SaveFileDialog With {
                .Filter = "Excel Files|*.xlsx",
                .FileName = $"Template_Jadwal_{kelasText}_Smt{semester}.xlsx"
            }

            If sfd.ShowDialog() = DialogResult.OK Then
                wb.SaveAs(sfd.FileName)
                MessageBox.Show("Template berhasil dibuat!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If

        Catch ex As Exception
            MessageBox.Show("Error download template: " & ex.Message)
        Finally
            conn.Close()
        End Try
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Dim Form1 As New Admin()
        Admin.Show()
        Me.Hide()
    End Sub
End Class
