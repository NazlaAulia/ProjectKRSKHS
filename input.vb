Imports System.ComponentModel
Imports System.IO
Imports ClosedXML.Excel
Imports MySql.Data.MySqlClient


Public Class input
    Dim conn As MySqlConnection
    Dim da As MySqlDataAdapter
    Dim ds As DataSet
    Dim cmd As MySqlCommand
    Dim rd As MySqlDataReader
    Dim emailDosen As String = Form1.emailUser ' dari login
    Dim modeImport As Boolean = False



    Private Sub input_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Call Koneksi()
        LoadMatkulDosen()
        TampilData()
    End Sub

    Sub Koneksi()
        conn = New MySqlConnection("server=localhost;user id=root;password=;database=krskhs")
    End Sub

    ' === COMBOBOX MATKUL DOSEN ===
    Sub LoadMatkulDosen()
        Try
            If conn.State = ConnectionState.Closed Then conn.Open()

            Dim sql As String = "
                SELECT mk.kode_mk, mk.nama_mk 
                FROM matakuliah mk
                JOIN jadwal j ON j.kode_mk = mk.kode_mk
                JOIN dosen d ON j.nidn = d.nidn
                WHERE d.email = @e"
            cmd = New MySqlCommand(sql, conn)
            cmd.Parameters.AddWithValue("@e", emailDosen)
            rd = cmd.ExecuteReader()
            ComboBox1.Items.Clear()
            While rd.Read()
                ComboBox1.Items.Add(rd("kode_mk").ToString() & " - " & rd("nama_mk").ToString())
            End While
            rd.Close()
        Catch ex As Exception
            MessageBox.Show("Gagal load mata kuliah: " & ex.Message)
        Finally
            If conn.State = ConnectionState.Closed Then conn.Open()

        End Try
    End Sub

    ' === TAMPIL DATA MAHASISWA YANG DIAJAR DOSEN ===
    Sub TampilData()
        Try
            If conn.State = ConnectionState.Closed Then conn.Open()

            ' kalau belum pilih matkul, tampilkan semua
            Dim filterKodeMk As String = Nothing
            If ComboBox1.SelectedIndex <> -1 Then
                filterKodeMk = ComboBox1.Text.Split("-"c)(0).Trim()
            End If

            Dim query As String = "
SELECT 
    k.id_krs,
    m.nim,
    m.nama,
    mk.nama_mk,
    IFNULL(n.uts, 0) AS uts,
    IFNULL(n.uas, 0) AS uas,
    IFNULL(n.nilai_huruf, '') AS nilai_huruf
FROM krs k
JOIN mahasiswa m ON k.nim = m.nim
JOIN jadwal j ON k.id_jadwal = j.id_jadwal
JOIN matakuliah mk ON j.kode_mk = mk.kode_mk
JOIN dosen d ON j.nidn = d.nidn
LEFT JOIN nilai n ON n.id_krs = k.id_krs
WHERE d.email = @e
AND k.status = 'Approved'"


            ' tambahkan filter matkul kalau ada yang dipilih
            If Not String.IsNullOrEmpty(filterKodeMk) Then
                query &= " AND mk.kode_mk = @kode"
            End If

            da = New MySqlDataAdapter(query, conn)
            da.SelectCommand.Parameters.AddWithValue("@e", emailDosen)
            If Not String.IsNullOrEmpty(filterKodeMk) Then
                da.SelectCommand.Parameters.AddWithValue("@kode", filterKodeMk)
            End If

            ds = New DataSet()
            da.Fill(ds, "nilai")
            DataGridView1.DataSource = ds.Tables("nilai")
            DataGridView1.Columns("id_krs").Visible = False

            ' cuma uts & uas yang bisa diedit
            For Each col As DataGridViewColumn In DataGridView1.Columns
                col.ReadOnly = True
            Next
            DataGridView1.Columns("uts").ReadOnly = False
            DataGridView1.Columns("uas").ReadOnly = False

        Catch ex As Exception
            MessageBox.Show("Gagal memuat data: " & ex.Message)
        Finally
            If conn.State = ConnectionState.Open Then conn.Close()
        End Try
    End Sub

    ' === SIMPAN MANUAL DARI DATAGRID ===
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Try
            If conn.State = ConnectionState.Closed Then conn.Open()

            For Each row As DataGridViewRow In DataGridView1.Rows
                If row.IsNewRow Then Continue For

                Dim idKRS As Integer = Convert.ToInt32(row.Cells("id_krs").Value)
                Dim uts As Decimal = Convert.ToDecimal(row.Cells("uts").Value)
                Dim uas As Decimal = Convert.ToDecimal(row.Cells("uas").Value)
                Dim rata As Decimal = (uts + uas) / 2
                Dim huruf As String = NilaiHuruf(rata)

                ' === Tambahkan ambil semester otomatis ===
                Dim sqlSem As String = "
                SELECT mk.semester 
                FROM jadwal j 
                JOIN matakuliah mk ON j.kode_mk = mk.kode_mk 
                JOIN krs k ON k.id_jadwal = j.id_jadwal 
                WHERE k.id_krs = @id"
                Dim semester As Object = Nothing
                Using cmdSem As New MySqlCommand(sqlSem, conn)
                    cmdSem.Parameters.AddWithValue("@id", idKRS)
                    semester = cmdSem.ExecuteScalar()
                End Using

                ' --- hapus data lama dulu ---
                Dim deleteSql As String = "DELETE FROM nilai WHERE id_krs = @id"
                Using delCmd As New MySqlCommand(deleteSql, conn)
                    delCmd.Parameters.AddWithValue("@id", idKRS)
                    delCmd.ExecuteNonQuery()
                End Using

                ' --- simpan data baru (dengan semester) ---
                Dim insertSql As String = "
                INSERT INTO nilai (id_krs, uts, uas, nilai_huruf, semester)
                VALUES (@id, @uts, @uas, @huruf, @semester)"
                Using cmd As New MySqlCommand(insertSql, conn)
                    cmd.Parameters.AddWithValue("@id", idKRS)
                    cmd.Parameters.AddWithValue("@uts", uts)
                    cmd.Parameters.AddWithValue("@uas", uas)
                    cmd.Parameters.AddWithValue("@huruf", huruf)
                    cmd.Parameters.AddWithValue("@semester", semester)
                    cmd.ExecuteNonQuery()
                End Using
            Next

            MessageBox.Show("berhasil disimpan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information)
            TampilData()

        Catch ex As Exception
            MessageBox.Show("Gagal menyimpan nilai: " & ex.Message)
        Finally
            If conn.State = ConnectionState.Open Then conn.Close()
        End Try
    End Sub



    Function NilaiHuruf(r As Decimal) As String
        If r >= 85 Then Return "A"
        If r >= 75 Then Return "B"
        If r >= 60 Then Return "C"
        If r >= 45 Then Return "D"
        Return "E"
    End Function



    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        If ComboBox1.SelectedIndex = -1 Then
            MessageBox.Show("Pilih mata kuliah dulu!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Exit Sub
        End If

        Dim ofd As New OpenFileDialog With {
        .Filter = "Excel Files|*.xlsx;*.xls",
        .Title = "Pilih File Nilai Mahasiswa"
    }

        If ofd.ShowDialog() = DialogResult.OK Then
            Try
                Dim kodeMk As String = ComboBox1.Text.Split("-"c)(0).Trim()

                ' --- HAPUS DULU SEMUA NILAI UNTUK MATA KULIAH TERSEBUT ---
                If conn.State = ConnectionState.Closed Then conn.Open()

                Dim delSql As String = "
                DELETE n FROM nilai n
                JOIN krs k ON n.id_krs = k.id_krs
                JOIN jadwal j ON k.id_jadwal = j.id_jadwal
                WHERE j.kode_mk = @kode"
                Using delCmd As New MySqlCommand(delSql, conn)
                    delCmd.Parameters.AddWithValue("@kode", kodeMk)
                    delCmd.ExecuteNonQuery()
                End Using

                ' --- BACA FILE EXCEL ---
                Using workbook As New XLWorkbook(ofd.FileName)
                    Dim ws = workbook.Worksheets.First()

                    For i As Integer = 2 To ws.LastRowUsed().RowNumber()
                        Dim nim As String = ws.Cell(i, 1).GetValue(Of String)().Trim()
                        Dim utsValue As String = ws.Cell(i, 3).GetValue(Of String)().Trim()
                        Dim uasValue As String = ws.Cell(i, 4).GetValue(Of String)().Trim()

                        Dim uts As Decimal = 0
                        Dim uas As Decimal = 0
                        Decimal.TryParse(utsValue, uts)
                        Decimal.TryParse(uasValue, uas)
                        Dim rata As Decimal = (uts + uas) / 2
                        Dim huruf As String = NilaiHuruf(rata)

                        ' cari id_krs dari nim & kode mk
                        Dim sqlId As String = "
                        SELECT k.id_krs
                        FROM krs k
                        JOIN jadwal j ON k.id_jadwal = j.id_jadwal
                        WHERE k.nim=@nim AND j.kode_mk=@kode AND k.status = 'Approved'"
                        Dim idKrs As Object = Nothing

                        Using cmdId As New MySqlCommand(sqlId, conn)
                            cmdId.Parameters.AddWithValue("@nim", nim)
                            cmdId.Parameters.AddWithValue("@kode", kodeMk)
                            idKrs = cmdId.ExecuteScalar()
                        End Using

                        If idKrs IsNot Nothing Then
                            ' insert nilai baru
                            Dim insSql As String = "
                            INSERT INTO nilai (id_krs, uts, uas, nilai_huruf)
                            VALUES (@id, @uts, @uas, @huruf)"
                            Using cmd As New MySqlCommand(insSql, conn)
                                cmd.Parameters.AddWithValue("@id", idKrs)
                                cmd.Parameters.AddWithValue("@uts", uts)
                                cmd.Parameters.AddWithValue("@uas", uas)
                                cmd.Parameters.AddWithValue("@huruf", huruf)
                                cmd.ExecuteNonQuery()
                            End Using
                        End If
                    Next
                End Using

                MessageBox.Show("Nilai berhasil diimpor", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information)
                TampilData()

            Catch ex As Exception
                MessageBox.Show("Gagal membaca file Excel: " & ex.Message)
            Finally
                If conn.State = ConnectionState.Open Then conn.Close()
            End Try
        End If
    End Sub




    ' === TOMBOL REFRESH ===
    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        TampilData()
    End Sub

    'PINDAH HALAMAN
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Dim aprove1 As New Aprove1()
        aprove1.Show()
        Me.Close()
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Dim f As New Form1()
        f.Show()
        Me.Close()
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        If ComboBox1.SelectedIndex = -1 Then
            MessageBox.Show("Pilih mata kuliah dulu!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Exit Sub
        End If

        Try
            ' Ambil kode mata kuliah dari ComboBox (karena isinya "kode - nama")
            Dim selectedText As String = ComboBox1.Text
            Dim kodeMk As String = selectedText.Split("-"c)(0).Trim()
            Dim namaMk As String = selectedText.Split("-"c)(1).Trim()

            Call Koneksi()
            If conn.State = ConnectionState.Closed Then conn.Open()



            ' Ambil daftar mahasiswa di MK tsb
            Dim sql As String = "
            SELECT m.nim, m.nama
            FROM krs k
            JOIN mahasiswa m ON k.nim = m.nim
            JOIN jadwal j ON k.id_jadwal = j.id_jadwal
            WHERE j.kode_mk = @kode
  AND k.status = 'Approved'"

            Dim dt As New DataTable
            Using da As New MySqlDataAdapter(sql, conn)
                da.SelectCommand.Parameters.AddWithValue("@kode", kodeMk)
                da.Fill(dt)
            End Using

            If dt.Rows.Count = 0 Then
                MessageBox.Show("Tidak ada mahasiswa di mata kuliah ini.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Exit Sub
            End If

            ' Pilih lokasi simpan
            Dim sfd As New SaveFileDialog With {
                .Filter = "Excel File|*.xlsx",
                .FileName = $"Template_Nilai_{namaMk}.xlsx"
            }

            If sfd.ShowDialog() = DialogResult.OK Then
                ' Buat file Excel pakai ClosedXML
                Using wb As New XLWorkbook()
                    Dim ws = wb.Worksheets.Add("Nilai Mahasiswa")

                    ' Header
                    ws.Cell(1, 1).Value = "NIM"
                    ws.Cell(1, 2).Value = "NAMA"
                    ws.Cell(1, 3).Value = "UTS"
                    ws.Cell(1, 4).Value = "UAS"

                    ' Isi data
                    Dim row As Integer = 2
                    For Each r As DataRow In dt.Rows
                        ws.Cell(row, 1).Value = r("nim").ToString()
                        ws.Cell(row, 2).Value = r("nama").ToString()
                        row += 1
                    Next

                    ws.Columns().AdjustToContents()
                    wb.SaveAs(sfd.FileName)
                End Using

                MessageBox.Show("Template Excel berhasil dibuat!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If

        Catch ex As Exception
            MessageBox.Show("Gagal membuat template: " & ex.Message)
        Finally
            If conn.State = ConnectionState.Closed Then conn.Open()


        End Try
    End Sub

    Private Sub CariMahasiswa()
        Try
            If conn.State = ConnectionState.Closed Then conn.Open()

            Dim keyword As String = TextBox1.Text.Trim()
            Dim sql As String = "
            SELECT 
                k.id_krs,
                m.nim,
                m.nama,
                mk.nama_mk,
                IFNULL(n.uts, 0) AS uts,
                IFNULL(n.uas, 0) AS uas,
                IFNULL(n.nilai_huruf, '') AS nilai_huruf
            FROM krs k
            JOIN mahasiswa m ON k.nim = m.nim
            JOIN jadwal j ON k.id_jadwal = j.id_jadwal
            JOIN matakuliah mk ON j.kode_mk = mk.kode_mk
            JOIN dosen d ON j.nidn = d.nidn
            LEFT JOIN nilai n ON n.id_krs = k.id_krs
            WHERE d.email = @e
  AND k.status = 'Approved'
            AND (m.nim LIKE @k OR m.nama LIKE @k OR mk.nama_mk LIKE @k)
            ORDER BY m.nama"

            Dim da As New MySqlDataAdapter(sql, conn)
            da.SelectCommand.Parameters.AddWithValue("@e", emailDosen)
            da.SelectCommand.Parameters.AddWithValue("@k", "%" & keyword & "%")

            Dim dt As New DataTable()
            da.Fill(dt)
            DataGridView1.DataSource = dt

        Catch ex As Exception
            MessageBox.Show("Gagal mencari: " & ex.Message)
        Finally
            If conn.State = ConnectionState.Open Then conn.Close()
        End Try
    End Sub

    Private Sub DataGridView1_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellContentClick

    End Sub

    Private Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click
        CariMahasiswa()
    End Sub

    Private Sub TextBox1_KeyDown(sender As Object, e As KeyEventArgs) Handles TextBox1.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            CariMahasiswa()
        End If
    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub Panel2_Paint(sender As Object, e As PaintEventArgs) Handles Panel2.Paint

    End Sub

    Private Sub Panel1_Paint(sender As Object, e As PaintEventArgs) Handles Panel1.Paint

    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        TampilData()
    End Sub

    Private Sub Button10_Click(sender As Object, e As EventArgs) Handles Button10.Click
        Dim f As New profildosen()
        f.Show()
        Me.Close()
    End Sub

    Private Sub Button7_Click_1(sender As Object, e As EventArgs) Handles Button7.Click
        Dim f As New FormGantiPassword()
        f.Show()
        Me.Close()
    End Sub
End Class
