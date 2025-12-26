Imports System.IO 'untuk baca tulisan di pdf
Imports iTextSharp.text ' buat bokin dokumen pdf
Imports iTextSharp.text.pdf 'bikin tabel dll di pdf
Imports MySql.Data.MySqlClient 'koneksi ke mysql

Public Class krsvb

    Private ReadOnly conn As New MySqlConnection("server=localhost;user id=root;password=;database=krskhs")
    Private currentNIM As String = ""
    Private currentIDKelas As Integer = 0
    Private currentNama As String = ""
    Private currentProdi As String = ""
    Private currentDosenPembimbing As String = ""
    Private currentSemester As String = ""

    Private Const STATUS_PENDING As String = "Pending"
    Private Const STATUS_APPROVED As String = "Approved"


    Private Sub krsvb_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        AmbilDataMahasiswa()
        SetupGrid()

        cbSemester.Items.Clear()
        cbSemester.Items.Add("1")
        cbSemester.Items.Add("2")
        cbSemester.Items.Add("3")
        cbSemester.Items.Add("4")
        cbSemester.Items.Add("5")
        cbSemester.Items.Add("6")
        cbSemester.Items.Add("7")
        cbSemester.Items.Add("8")

        cbSemester.SelectedIndex = 0
        TampilMatkul()
        UpdateTotalSKS()
    End Sub

    'buat  ambil mhs yg login (buat nama di atas)
    Private Sub AmbilDataMahasiswa()
        Try
            conn.Open()
            Dim emailLogin As String = Form1.emailUser
            Dim q As String = "
                SELECT m.nim, m.nama, m.prodi, m.id_kelas, 
                       k.pembimbing AS dosen_pembimbing, k.tahun_ajaran
                FROM mahasiswa m
                JOIN kelas k ON m.id_kelas = k.id_kelas
                WHERE m.email=@e LIMIT 1"
            Using cmd As New MySqlCommand(q, conn)
                cmd.Parameters.AddWithValue("@e", emailLogin)
                Using r = cmd.ExecuteReader()
                    If r.Read() Then
                        currentNIM = r("nim").ToString()
                        currentNama = r("nama").ToString()
                        currentProdi = r("prodi").ToString()
                        currentIDKelas = Convert.ToInt32(r("id_kelas"))
                        currentDosenPembimbing = r("dosen_pembimbing").ToString()
                        currentSemester = r("tahun_ajaran").ToString()

                        Label2.Text = currentNama
                        Label4.Text = currentNIM
                        Label3.Text = currentProdi
                    End If
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Gagal ambil data mahasiswa: " & ex.Message)
        Finally
            conn.Close()
        End Try
    End Sub


    'buat tabel
    Private Sub SetupGrid()
        DataGridView1.AutoGenerateColumns = False
        DataGridView1.Columns.Clear()

        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "kode_mk", .HeaderText = "Kode MK", .DataPropertyName = "kode_mk"})
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "nama_mk", .HeaderText = "Nama MK", .DataPropertyName = "nama_mk"})
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "sks", .HeaderText = "SKS", .DataPropertyName = "sks"})
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "dosen", .HeaderText = "Dosen", .DataPropertyName = "dosen"})
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "status", .HeaderText = "Status", .DataPropertyName = "status"})

        Dim btnCol As New DataGridViewButtonColumn()
        btnCol.HeaderText = "Aksi"
        btnCol.Name = "aksi"
        btnCol.Text = "Pilih"
        btnCol.UseColumnTextForButtonValue = True
        DataGridView1.Columns.Add(btnCol)

        DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect
    End Sub

    'TAMPIL MATAKULIAH SEMESTER AKTIF 
    Private Sub TampilMatkul()
        If cbSemester.SelectedItem Is Nothing Then
            Exit Sub
        End If

        Try
            conn.Open()
            Dim sql As String = "
SELECT mk.kode_mk, mk.nama_mk, mk.sks, d.nama AS dosen,
      CASE 
    WHEN k.status IS NULL THEN NULL
    ELSE k.status
END AS status

FROM jadwal j
JOIN matakuliah mk ON j.kode_mk = mk.kode_mk
JOIN dosen d ON j.nidn = d.nidn
LEFT JOIN krs k ON k.id_jadwal = j.id_jadwal AND k.nim = @nim
WHERE j.id_kelas = @kelas AND mk.semester = @semester
ORDER BY mk.nama_mk;"

            Using cmd As New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@nim", currentNIM)
                cmd.Parameters.AddWithValue("@kelas", currentIDKelas)
                cmd.Parameters.AddWithValue("@semester", cbSemester.SelectedItem.ToString())
                Dim da As New MySqlDataAdapter(cmd)
                Dim dt As New DataTable()
                da.Fill(dt)
                DataGridView1.DataSource = dt

                For Each r As DataGridViewRow In DataGridView1.Rows
                    If r.IsNewRow Then Continue For

                    Dim st As String = If(r.Cells("status").Value, "").ToString()

                    If String.IsNullOrEmpty(st) Then
                        r.Cells("status").Value = "Belum dipilih"
                        r.Cells("aksi").Value = "Pilih"
                    ElseIf st = STATUS_PENDING Then
                        r.Cells("aksi").Value = "Batal"
                    ElseIf st = STATUS_APPROVED Then
                        r.Cells("aksi").Value = "Approved"
                        ' Optional: kalau nggak mau bisa diklik lagi
                        ' CType(r.Cells("aksi"), DataGridViewButtonCell).ReadOnly = True
                    End If
                Next

            End Using
        Catch ex As Exception
            MessageBox.Show("Gagal tampil mata kuliah: " & ex.Message)
        Finally
            conn.Close()
        End Try

        UpdateTotalSKS()

    End Sub


    'AKSI TOMBOL PILIH / BATAL 
    Private Sub DataGridView1_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellContentClick
        If e.ColumnIndex = DataGridView1.Columns("aksi").Index AndAlso e.RowIndex >= 0 Then
            Dim row As DataGridViewRow = DataGridView1.Rows(e.RowIndex)
            Dim kodeMk As String = row.Cells("kode_mk").Value.ToString()
            Dim status As String = row.Cells("status").Value?.ToString()

            Try
                conn.Open()

                'Kalau belum dipilih (status kosong ATAU "Belum dipilih") 
                If status Is Nothing OrElse status = "" OrElse status = "Belum dipilih" Then


                    Dim sql As String = "
                    INSERT INTO krs (nim, id_jadwal, status, dosen_pembimbing, id_kelas)
                    SELECT @nim, j.id_jadwal, 'Pending', @dosenpb, @kelas
                    FROM jadwal j
                    WHERE j.kode_mk=@kode AND j.id_kelas=@kelas
                    ON DUPLICATE KEY UPDATE 
                        status='Pending',
                        dosen_pembimbing=@dosenpb,
                        id_kelas=@kelas;"

                    ' ON DUPLICATE KEY UPDATE  buat ga doubel pas di klik

                    Using cmd As New MySqlCommand(sql, conn)
                        cmd.Parameters.AddWithValue("@nim", currentNIM)
                        cmd.Parameters.AddWithValue("@kode", kodeMk)
                        cmd.Parameters.AddWithValue("@kelas", currentIDKelas)
                        cmd.Parameters.AddWithValue("@dosenpb", currentDosenPembimbing)
                        cmd.ExecuteNonQuery()
                    End Using

                    row.Cells("status").Value = STATUS_PENDING      ' -> "Pending"
                    row.Cells("aksi").Value = "Batal"

                    '  Kalau Pending, boleh dibatalkan
                ElseIf status = STATUS_PENDING Then

                    Dim sql As String = "
                    DELETE k FROM krs k
                    JOIN jadwal j ON k.id_jadwal = j.id_jadwal
                    WHERE k.nim=@nim AND j.kode_mk=@kode;"

                    Using cmd As New MySqlCommand(sql, conn)
                        cmd.Parameters.AddWithValue("@nim", currentNIM)
                        cmd.Parameters.AddWithValue("@kode", kodeMk)
                        cmd.ExecuteNonQuery()
                    End Using

                    row.Cells("status").Value = "Belum dipilih"     ' balik lagi
                    row.Cells("aksi").Value = "Pilih"

                    '  Kalau sudah Approved
                ElseIf status = STATUS_APPROVED Then
                    MessageBox.Show("Mata kuliah sudah disetujui dosen dan tidak dapat dibatalkan.",
                                "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                End If

            Catch ex As Exception
                MessageBox.Show("Terjadi kesalahan: " & ex.Message)
            Finally
                conn.Close()
            End Try

            UpdateTotalSKS()
            SimpanTotalSKSKeDatabase()
        End If
    End Sub


    ' HITUNG TOTAL SKS (Approved saja) 
    Private Sub UpdateTotalSKS()
        Try
            conn.Open()
            Dim total As Integer = 0

            ' Ambil total SKS semua mata kuliah yang sudah Approved dari semua semester
            Dim sql As String = "
            SELECT SUM(mk.sks) AS total_sks
            FROM krs k
            JOIN jadwal j ON k.id_jadwal = j.id_jadwal
            JOIN matakuliah mk ON j.kode_mk = mk.kode_mk
            WHERE k.nim = @nim AND k.status = 'Approved';"
            Using cmd As New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@nim", currentNIM)
                Dim hasil = cmd.ExecuteScalar()
                If hasil IsNot DBNull.Value Then
                    total = Convert.ToInt32(hasil)
                End If
            End Using

            Label7.Text = $"Total SKS: {total}"

        Catch ex As Exception
            MessageBox.Show("Gagal menghitung total SKS: " & ex.Message)
        Finally
            conn.Close()
        End Try
    End Sub


    ' SIMPAN TOTAL SKS KE TABEL d_sks 
    Private Sub SimpanTotalSKSKeDatabase()
        Try
            conn.Open()

            ' Ambil total SKS semua semester yang Approved
            Dim totalSKS As Integer = 0
            Dim sqlHitung As String = "
            SELECT SUM(mk.sks) AS total_sks
            FROM krs k
            JOIN jadwal j ON k.id_jadwal = j.id_jadwal
            JOIN matakuliah mk ON j.kode_mk = mk.kode_mk
            WHERE k.nim = @nim AND k.status = 'Approved';"
            Using cmdHitung As New MySqlCommand(sqlHitung, conn)
                cmdHitung.Parameters.AddWithValue("@nim", currentNIM)
                Dim hasil = cmdHitung.ExecuteScalar()
                If hasil IsNot DBNull.Value Then
                    totalSKS = Convert.ToInt32(hasil)
                End If
            End Using

            ' Cek apakah NIM sudah ada di tabel sks
            Dim cekSql As String = "SELECT COUNT(*) FROM sks WHERE nim = @nim"
            Dim adaData As Integer
            Using cekCmd As New MySqlCommand(cekSql, conn)
                cekCmd.Parameters.AddWithValue("@nim", currentNIM)
                adaData = Convert.ToInt32(cekCmd.ExecuteScalar())
            End Using

            Dim sql As String
            If adaData > 0 Then
                sql = "UPDATE sks SET total_sks = @sks WHERE nim = @nim"
            Else
                sql = "INSERT INTO sks (nim, total_sks) VALUES (@nim, @sks)"
            End If

            Using cmd As New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@nim", currentNIM)
                cmd.Parameters.AddWithValue("@sks", totalSKS)
                cmd.ExecuteNonQuery()
            End Using

        Catch ex As Exception
            MessageBox.Show("Gagal simpan total SKS: " & ex.Message)
        Finally
            conn.Close()
        End Try
    End Sub




    ' CETAK 
    Private Sub cetakkrs_Click(sender As Object, e As EventArgs) Handles cetakkrs.Click
        ' Pastikan semester dipilih
        If cbSemester.SelectedItem Is Nothing Then
            MessageBox.Show("Pilih semester terlebih dahulu sebelum mencetak.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        Dim semesterDipilih As String = cbSemester.SelectedItem.ToString()

        ' Ambil data KRS hanya untuk semester aktif
        Dim dtKRS As New DataTable
        Try
            Using con As New MySqlConnection("server=localhost;user id=root;password=;database=krskhs")
                con.Open()
                Dim sql As String = "
            SELECT mk.kode_mk, mk.nama_mk, mk.sks, d.nama AS dosen, k.status
            FROM krs k
            JOIN jadwal j ON k.id_jadwal = j.id_jadwal
            JOIN matakuliah mk ON j.kode_mk = mk.kode_mk
            JOIN dosen d ON j.nidn = d.nidn
            WHERE k.nim=@nim AND mk.semester=@semester
            ORDER BY mk.nama_mk;"
                Using cmd As New MySqlCommand(sql, con)
                    cmd.Parameters.AddWithValue("@nim", currentNIM)
                    cmd.Parameters.AddWithValue("@semester", semesterDipilih)
                    Dim da As New MySqlDataAdapter(cmd)
                    da.Fill(dtKRS)
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Gagal mengambil data KRS semester " & semesterDipilih & ": " & ex.Message)
            Exit Sub
        End Try

        If dtKRS.Rows.Count = 0 Then
            MessageBox.Show("Belum ada data KRS untuk semester " & semesterDipilih & ".", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Exit Sub
        End If

        '  Ambil data dosen pembimbing 
        Dim dosenPembimbing As String = ""
        Dim nidnDosen As String = ""
        Try
            Using con As New MySqlConnection("server=localhost;user id=root;password=;database=krskhs")
                con.Open()
                Dim sql As String = "
            SELECT d.nama AS dosen, d.nidn 
            FROM mahasiswa m
            JOIN kelas k ON m.id_kelas = k.id_kelas
            JOIN dosen d ON k.pembimbing = d.nidn
            WHERE m.nim = @nim LIMIT 1"
                Using cmd As New MySqlCommand(sql, con)
                    cmd.Parameters.AddWithValue("@nim", currentNIM) 'buat fitur login
                    Using rd = cmd.ExecuteReader()
                        If rd.Read() Then
                            dosenPembimbing = rd("dosen").ToString()
                            nidnDosen = rd("nidn").ToString()
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Gagal mengambil data dosen pembimbing: " & ex.Message)
        End Try

        '  Proses Cetak PDF 
        Dim sfd As New SaveFileDialog With {
            .Filter = "PDF File|*.pdf",
            .FileName = $"KRS_Smt{semesterDipilih}_{currentNama}_{currentNIM}.pdf"
        }

        If sfd.ShowDialog() = DialogResult.OK Then
            Dim doc As New Document(PageSize.A4, 40, 40, 40, 40)
            PdfWriter.GetInstance(doc, New FileStream(sfd.FileName, FileMode.Create))
            doc.Open()

            ' KOP SURAT 
            Dim kopFontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14)
            Dim kopFontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 10)

            doc.Add(New Paragraph("UNIVERSITAS NEGERI SURABAYA", kopFontBold) With {.Alignment = Element.ALIGN_CENTER})
            doc.Add(New Paragraph("Fakultas Teknik - Program Studi Pendidikan Teknologi Informasi", kopFontNormal) With {.Alignment = Element.ALIGN_CENTER})
            doc.Add(New Paragraph("Jl. Ketintang, Surabaya 60231 | Telp. (031) 829XXXX | Email: pti@unesa.ac.id", kopFontNormal) With {.Alignment = Element.ALIGN_CENTER})
            doc.Add(New Paragraph(" "))
            doc.Add(New Paragraph(New String("_"c, 90), kopFontNormal))
            doc.Add(New Paragraph(" "))

            ' JUDUL & IDENTITAS
            Dim titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16)
            Dim infoFont = FontFactory.GetFont(FontFactory.HELVETICA, 11)

            doc.Add(New Paragraph("KARTU RENCANA STUDI (KRS)", titleFont) With {.Alignment = Element.ALIGN_CENTER})
            doc.Add(New Paragraph("Semester: " & semesterDipilih, infoFont) With {.Alignment = Element.ALIGN_CENTER})
            doc.Add(New Paragraph(" "))
            doc.Add(New Paragraph("Nama: " & currentNama, infoFont))
            doc.Add(New Paragraph("NIM: " & currentNIM, infoFont))
            doc.Add(New Paragraph("Program Studi: " & currentProdi, infoFont))
            doc.Add(New Paragraph("Tanggal Cetak: " & DateTime.Now.ToString("dd MMMM yyyy"), infoFont))
            doc.Add(New Paragraph(" "))

            ' TABEL KRS 
            Dim table As New PdfPTable(5)
            table.WidthPercentage = 100
            table.HorizontalAlignment = Element.ALIGN_CENTER

            Dim headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11)
            Dim headers() As String = {"Kode MK", "Nama MK", "SKS", "Dosen", "Status"}

            For Each headerText As String In headers
                Dim cell As New PdfPCell(New Phrase(headerText, headerFont))
                cell.HorizontalAlignment = Element.ALIGN_CENTER
                cell.BackgroundColor = BaseColor.LIGHT_GRAY
                cell.Padding = 5
                table.AddCell(cell)
            Next

            Dim totalSKS As Integer = 0
            Dim isiFont = FontFactory.GetFont(FontFactory.HELVETICA, 10)

            For Each row As DataRow In dtKRS.Rows
                table.AddCell(New PdfPCell(New Phrase(row("kode_mk").ToString(), isiFont)) With {.HorizontalAlignment = Element.ALIGN_CENTER})
                table.AddCell(New PdfPCell(New Phrase(row("nama_mk").ToString(), isiFont)) With {.HorizontalAlignment = Element.ALIGN_CENTER})
                table.AddCell(New PdfPCell(New Phrase(row("sks").ToString(), isiFont)) With {.HorizontalAlignment = Element.ALIGN_CENTER})
                table.AddCell(New PdfPCell(New Phrase(row("dosen").ToString(), isiFont)) With {.HorizontalAlignment = Element.ALIGN_CENTER})
                table.AddCell(New PdfPCell(New Phrase(row("status").ToString(), isiFont)) With {.HorizontalAlignment = Element.ALIGN_CENTER})

                Dim sksVal As Integer
                If Integer.TryParse(row("sks").ToString(), sksVal) Then totalSKS += sksVal
            Next


            ' Hitung Total SKS Semester Ini & Total Keseluruhan 
            Dim totalSemesterIni As Integer = 0
            Dim totalKeseluruhan As Integer = 0

            ' Hitung total SKS semester ini dari DataGridView2
            For Each row As DataGridViewRow In DataGridView1.Rows
                If row.IsNewRow Then Continue For
                Dim sksVal As Integer
                If Integer.TryParse(row.Cells("sks").Value?.ToString(), sksVal) Then
                    totalSemesterIni += sksVal
                End If
            Next

            ' Hitung total keseluruhan (Approved semua semester)
            Try
                Using con As New MySqlConnection("server=localhost;user id=root;password=;database=krskhs")
                    con.Open()
                    Dim sql As String = "
            SELECT IFNULL(SUM(mk.sks),0) AS total_sks
            FROM krs k
            JOIN jadwal j ON k.id_jadwal = j.id_jadwal
            JOIN matakuliah mk ON j.kode_mk = mk.kode_mk
            WHERE k.nim=@nim AND k.status='Approved';"
                    Using cmd As New MySqlCommand(sql, con)
                        cmd.Parameters.AddWithValue("@nim", currentNIM)
                        totalKeseluruhan = Convert.ToInt32(cmd.ExecuteScalar())
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show("Gagal hitung total SKS keseluruhan: " & ex.Message)
            End Try


            doc.Add(table)
            doc.Add(New Paragraph(" "))
            Dim totalFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12)
            doc.Add(New Paragraph("Total SKS Semester Ini: " & totalSKS, totalFont) With {.Alignment = Element.ALIGN_RIGHT})
            doc.Add(New Paragraph("Total SKS Keseluruhan : " & totalKeseluruhan, totalFont) With {.Alignment = Element.ALIGN_RIGHT})

            '  TANDA TANGAN 
            doc.Add(New Paragraph(" "))
            doc.Add(New Paragraph(" "))
            Dim ttgFont = FontFactory.GetFont(FontFactory.HELVETICA, 11)

            doc.Add(New Paragraph("Surabaya, " & DateTime.Now.ToString("dd MMMM yyyy"), ttgFont) With {.Alignment = Element.ALIGN_RIGHT})
            doc.Add(New Paragraph(" "))

            Dim tandaTable As New PdfPTable(2)
            tandaTable.WidthPercentage = 100
            tandaTable.DefaultCell.Border = 0

            tandaTable.AddCell(New Paragraph("Dosen Pembimbing Akademik,", ttgFont))
            tandaTable.AddCell(New Paragraph("Mahasiswa,", ttgFont) With {.Alignment = Element.ALIGN_RIGHT})

            ' Ruang tanda tangan
            Dim emptyCell As New PdfPCell(New Phrase(" ")) With {.Border = 0, .FixedHeight = 60}
            tandaTable.AddCell(emptyCell)
            tandaTable.AddCell(emptyCell)

            ' Nama & identitas
            tandaTable.AddCell(New Paragraph("(" & dosenPembimbing & ")", ttgFont))
            tandaTable.AddCell(New Paragraph("(" & currentNama & ")", ttgFont) With {.Alignment = Element.ALIGN_RIGHT})
            tandaTable.AddCell(New Paragraph("NIDN: " & nidnDosen, ttgFont))
            tandaTable.AddCell(New Paragraph("NIM: " & currentNIM, ttgFont) With {.Alignment = Element.ALIGN_RIGHT})

            doc.Add(tandaTable)
            doc.Close()

            MessageBox.Show($"KRS semester {semesterDipilih} berhasil dicetak!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Form1.emailUser = Nothing
        Dim f As New Form1()
        f.Show()
        Me.Close()
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Dim f As New khsvb()
        f.Show()
        Me.Close()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim f As New Profill()
        f.Show()
        Me.Close()
    End Sub

    Private Sub cbSemester_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbSemester.SelectedIndexChanged

        TampilMatkul()
    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        Dim f As New FormGantiPassword()
        f.Show()
        Me.Close()
    End Sub

    Private Sub Panel3_Paint(sender As Object, e As PaintEventArgs) Handles Panel3.Paint

    End Sub
End Class