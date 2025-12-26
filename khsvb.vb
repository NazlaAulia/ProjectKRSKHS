Imports MySql.Data.MySqlClient
Imports System.IO
Imports iTextSharp.text
Imports iTextSharp.text.pdf

Public Class khsvb

    Private ReadOnly ConnStr As String = "server=localhost;user id=root;password=;database=krskhs"


    Private nimMahasiswa As String = ""   ' diisi dari email login

    Private Sub khsvb_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        AmbilNimDariEmail()   ' email -> NIM
        LoadSemester()        ' isi combobox dari nilai.semester
        AturTampilanIPK()     ' tampilkan/sempunyikan IPK sesuai jumlah semester
        DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        DataGridView1.MultiSelect = False

        ' Kunci IPS & IPK agar tidak bisa diedit / dipilih
        TextBox1.ReadOnly = True   ' IPS
        TextBox2.ReadOnly = True   ' IPK
        TextBox1.TabStop = False
        TextBox2.TabStop = False
        TextBox1.Cursor = Cursors.Default
        TextBox2.Cursor = Cursors.Default
        ' (opsional biar tampil seperti label)
        TextBox1.BorderStyle = BorderStyle.None
        TextBox2.BorderStyle = BorderStyle.None
        TextBox1.BackColor = Me.BackColor
        TextBox2.BackColor = Me.BackColor

    End Sub


    Private Function NewConn() As MySqlConnection
        Return New MySqlConnection(ConnStr)
    End Function

    'EMAIL ke NIM 
    Private Sub AmbilNimDariEmail()
        Try
            Using con = NewConn()
                con.Open()
                Dim q As String = "SELECT nim FROM mahasiswa WHERE email=@e"
                Using cmd As New MySqlCommand(q, con)
                    cmd.Parameters.AddWithValue("@e", Form1.emailUser)
                    Dim r = cmd.ExecuteScalar()
                    If r Is Nothing Then
                        Throw New Exception("Email tidak ditemukan pada tabel mahasiswa.")
                    End If
                    nimMahasiswa = r.ToString()
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Gagal ambil NIM: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    'LOAD SEMESTER dari NILAI
    Private Sub LoadSemester()
        Try
            Using con = NewConn()
                con.Open()
                Dim sql As String =
"SELECT DISTINCT n.semester
 FROM nilai n
 JOIN krs k ON n.id_krs = k.id_krs
 WHERE k.nim = @nim AND n.semester IS NOT NULL
 ORDER BY n.semester"
                'DISTINCT ambil nilai semster tanpa duplikat
                Using cmd As New MySqlCommand(sql, con)
                    cmd.Parameters.AddWithValue("@nim", nimMahasiswa)
                    Using rd = cmd.ExecuteReader()
                        ComboBox1.Items.Clear()
                        While rd.Read()
                            ComboBox1.Items.Add(rd(0).ToString())
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Gagal load semester: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    'T PILIH SEMESTER
    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        TampilData()
    End Sub

    ' TAMPIL DATA KHS 
    Private Sub TampilData()
        If String.IsNullOrWhiteSpace(ComboBox1.Text) Then Return

        Try
            Using con = NewConn()
                con.Open()
                Dim sql As String =
"SELECT 
    mk.nama_mk     AS 'Mata Kuliah',
    mk.sks         AS 'SKS',
    n.nilai_huruf  AS 'Nilai',
    (CASE 
        WHEN n.nilai_huruf='A' THEN 4
        WHEN n.nilai_huruf='B' THEN 3
        WHEN n.nilai_huruf='C' THEN 2
        WHEN n.nilai_huruf='D' THEN 1
        ELSE 0 END)        AS 'Bobot',
    (CASE 
        WHEN n.nilai_huruf='A' THEN 4
        WHEN n.nilai_huruf='B' THEN 3
        WHEN n.nilai_huruf='C' THEN 2
        WHEN n.nilai_huruf='D' THEN 1
        ELSE 0 END) * mk.sks AS 'SKS*N'
FROM nilai n
JOIN krs k         ON n.id_krs    = k.id_krs
JOIN jadwal j      ON k.id_jadwal = j.id_jadwal
JOIN matakuliah mk ON j.kode_mk   = mk.kode_mk
WHERE k.nim=@nim AND n.semester=@s"

                Using da As New MySqlDataAdapter(sql, con)
                    da.SelectCommand.Parameters.AddWithValue("@nim", nimMahasiswa)
                    da.SelectCommand.Parameters.AddWithValue("@s", ComboBox1.Text)
                    Dim ds As New DataSet()
                    da.Fill(ds, "khs")
                    DataGridView1.DataSource = ds.Tables("khs")
                End Using
            End Using

            ' Hitung IPS selalu untuk semester terpilih
            HitungIPS()


            ' Hitung/atur IPK hanya jika sudah >= 2 semester
            If HitungJumlahSemester() >= 2 Then
                HitungIPK()
            Else
                TextBox2.Text = "–" ' belum ada IPK di semester 1
            End If

            AturTampilanIPK()

        Catch ex As Exception
            MessageBox.Show("Gagal tampil nilai: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    'HITUNG IPS (dari grid aktif)
    Private Sub HitungIPS()
        Dim totalBN As Decimal = 0D ' BN = Bobot*SKS (kolom "SKS*N")
        Dim totalSks As Decimal = 0D

        For Each row As DataGridViewRow In DataGridView1.Rows
            If row.IsNewRow Then Continue For
            Dim sksObj = row.Cells("SKS").Value 'buat ambil sks
            Dim bnsObj = row.Cells("SKS*N").Value 'buat ambil total sks

            If sksObj IsNot Nothing AndAlso bnsObj IsNot Nothing AndAlso
           Not String.IsNullOrWhiteSpace(sksObj.ToString()) AndAlso
           Not String.IsNullOrWhiteSpace(bnsObj.ToString()) Then

                totalSks += Convert.ToDecimal(sksObj)
                totalBN += Convert.ToDecimal(bnsObj)
            End If
        Next

        TextBox1.Text = If(totalSks > 0D, Math.Round(totalBN / totalSks, 2).ToString(), "0")


    End Sub


    ' HITUNG IPK (akumulasi semua semester pada NILAI)
    Private Sub HitungIPK()
        Try
            Using con = NewConn()
                con.Open()
                Dim sql As String =
"SELECT 
    SUM((CASE 
        WHEN n.nilai_huruf='A' THEN 4
        WHEN n.nilai_huruf='B' THEN 3
        WHEN n.nilai_huruf='C' THEN 2
        WHEN n.nilai_huruf='D' THEN 1
        ELSE 0 END) * mk.sks) AS totalBobot,
    SUM(mk.sks) AS totalSKS
FROM nilai n
JOIN krs k         ON n.id_krs    = k.id_krs
JOIN jadwal j      ON k.id_jadwal = j.id_jadwal
JOIN matakuliah mk ON j.kode_mk   = mk.kode_mk
WHERE k.nim = @nim"
                Using cmd As New MySqlCommand(sql, con)
                    cmd.Parameters.AddWithValue("@nim", nimMahasiswa)
                    Using rd = cmd.ExecuteReader()
                        If rd.Read() Then
                            Dim totalBobot = If(IsDBNull(rd("totalBobot")), 0D, Convert.ToDecimal(rd("totalBobot")))
                            Dim totalSks = If(IsDBNull(rd("totalSKS")), 0D, Convert.ToDecimal(rd("totalSKS")))
                            TextBox2.Text = If(totalSks > 0D, Math.Round(totalBobot / totalSks, 2).ToString(), "0")
                        Else
                            TextBox2.Text = "–"
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            TextBox2.Text = "–"
        End Try
    End Sub

    ' JUMLAH SEMESTER (distinct pada NILAI) 
    Private Function HitungJumlahSemester() As Integer
        Try
            Using con = NewConn()
                con.Open()
                Dim sql As String =
"SELECT COUNT(DISTINCT n.semester) 
 FROM nilai n
 JOIN krs k ON n.id_krs = k.id_krs
 WHERE k.nim = @nim AND n.semester IS NOT NULL"
                Using cmd As New MySqlCommand(sql, con)
                    cmd.Parameters.AddWithValue("@nim", nimMahasiswa)
                    Return Convert.ToInt32(cmd.ExecuteScalar())
                End Using
            End Using
        Catch
            Return 0
        End Try
    End Function

    'ATUR VISIBILITAS/ISI IPK 
    Private Sub AturTampilanIPK()
        Dim jmlSem = HitungJumlahSemester() 'menghitung semster yg pernah di ambil
        Dim showIpk As Boolean = (jmlSem >= 2) 'ipk dihitung setalh semster 2


        TextBox2.Visible = True
        TextBox2.ReadOnly = True

        If Not showIpk Then
            TextBox2.Text = "–" 'klau belum sesmetr 2 g amuncul
        End If

        Try
            TextBox2.Visible = True
        Catch

        End Try
    End Sub


    ' CETAK KHS KE PDF 
    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        If String.IsNullOrWhiteSpace(ComboBox1.Text) OrElse DataGridView1.Rows.Count = 0 Then
            MessageBox.Show("Pilih semester dan pastikan data tampil sebelum cetak.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim save As New SaveFileDialog With {
        .Filter = "PDF Files|*.pdf",
        .FileName = $"KHS_Semester_{ComboBox1.Text}.pdf"
    }

        If save.ShowDialog() = DialogResult.OK Then
            Dim dosenPembimbing As String = ""
            Dim nidnDosen As String = ""
            Dim namaMahasiswa As String = ""

            '  Ambil data dosen pembimbing & nama mahasiswa 
            Try
                Using con As New MySqlConnection(ConnStr)
                    con.Open()
                    Dim sql As String = "
                    SELECT m.nama AS nama_mhs, d.nama AS dosen, d.nidn
                    FROM mahasiswa m
                    JOIN kelas k ON m.id_kelas = k.id_kelas
                    JOIN dosen d ON k.pembimbing = d.nidn
                    WHERE m.nim = @nim
                    LIMIT 1"
                    Using cmd As New MySqlCommand(sql, con)
                        cmd.Parameters.AddWithValue("@nim", nimMahasiswa)
                        Using rd = cmd.ExecuteReader()
                            If rd.Read() Then
                                namaMahasiswa = rd("nama_mhs").ToString()
                                dosenPembimbing = rd("dosen").ToString()
                                nidnDosen = rd("nidn").ToString()
                            End If
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show("Gagal mengambil data dosen pembimbing: " & ex.Message)
            End Try

            Try
                Dim doc As New Document(PageSize.A4, 40, 40, 40, 40)
                PdfWriter.GetInstance(doc, New FileStream(save.FileName, FileMode.Create))
                doc.Open()

                ' ====== KOP SURAT ======
                Dim kopFontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14)
                Dim kopFontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 10)
                doc.Add(New Paragraph("UNIVERSITAS NEGERI SURABAYA", kopFontBold) With {.Alignment = Element.ALIGN_CENTER})
                doc.Add(New Paragraph("Fakultas Teknik - Program Studi Pendidikan Teknologi Informasi", kopFontNormal) With {.Alignment = Element.ALIGN_CENTER})
                doc.Add(New Paragraph("Jl. Ketintang, Surabaya 60231 | Telp. (031) 829XXXX | Email: pti@unesa.ac.id", kopFontNormal) With {.Alignment = Element.ALIGN_CENTER})
                doc.Add(New Paragraph(" "))
                doc.Add(New Paragraph("________________________________________________________________________________________", kopFontNormal))
                doc.Add(New Paragraph(" "))

                ' JUDUL DAN IDENTITAS 
                Dim titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 15)
                Dim infoFont = FontFactory.GetFont(FontFactory.HELVETICA, 11)
                doc.Add(New Paragraph($"KARTU HASIL STUDI (Semester {ComboBox1.Text})", titleFont) With {.Alignment = Element.ALIGN_CENTER})
                doc.Add(New Paragraph(" "))
                doc.Add(New Paragraph("Nama Mahasiswa : " & namaMahasiswa, infoFont))
                doc.Add(New Paragraph("NIM : " & nimMahasiswa, infoFont))
                doc.Add(New Paragraph("Tanggal Cetak : " & DateTime.Now.ToString("dd MMMM yyyy"), infoFont))
                doc.Add(New Paragraph(" "))

                '  TABEL DATA NILAI
                Dim table As New PdfPTable(DataGridView1.Columns.Count)
                table.WidthPercentage = 100
                table.HorizontalAlignment = Element.ALIGN_CENTER  ' <— Biar tabelnya di tengah halaman

                '  HEADER TABEL
                Dim headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11)
                For Each col As DataGridViewColumn In DataGridView1.Columns
                    Dim cell As New PdfPCell(New Phrase(col.HeaderText, headerFont))
                    cell.BackgroundColor = BaseColor.LIGHT_GRAY
                    cell.HorizontalAlignment = Element.ALIGN_CENTER   ' Rata tengah teks header
                    cell.Padding = 5
                    table.AddCell(cell)
                Next

                '  ISI TABEL 
                Dim cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 10)
                For Each row As DataGridViewRow In DataGridView1.Rows
                    If row.IsNewRow Then Continue For
                    For Each cell As DataGridViewCell In row.Cells
                        Dim pdfCell As New PdfPCell(New Phrase(If(cell.Value IsNot Nothing, cell.Value.ToString(), ""), cellFont))
                        pdfCell.HorizontalAlignment = Element.ALIGN_CENTER   ' Rata tengah isi
                        pdfCell.Padding = 4
                        table.AddCell(pdfCell)
                    Next
                Next

                '  TAMBAHKAN TABEL KE DOKUMEN 
                doc.Add(table)


                doc.Add(New Paragraph(" "))
                doc.Add(New Paragraph("IPS: " & TextBox1.Text, infoFont))
                doc.Add(New Paragraph("IPK: " & TextBox2.Text, infoFont))

                'TANDA TANGAN 
                doc.Add(New Paragraph(" "))
                doc.Add(New Paragraph(" "))
                doc.Add(New Paragraph("Surabaya, " & DateTime.Now.ToString("dd MMMM yyyy"), infoFont) With {.Alignment = Element.ALIGN_RIGHT})
                doc.Add(New Paragraph(" "))

                Dim tandaTable As New PdfPTable(2)
                tandaTable.WidthPercentage = 100
                tandaTable.DefaultCell.Border = 0

                tandaTable.AddCell(New Paragraph("Dosen Pembimbing Akademik,", infoFont))
                tandaTable.AddCell(New Paragraph("Mahasiswa,", infoFont) With {.Alignment = Element.ALIGN_RIGHT})

                '  SPASI UNTUK TANDA TANGAN 
                Dim emptyCell As New PdfPCell(New Phrase(" "))
                emptyCell.Border = 0
                emptyCell.FixedHeight = 60 ' tinggi ruang kosong tanda tangan
                tandaTable.AddCell(emptyCell)
                tandaTable.AddCell(emptyCell)

                tandaTable.AddCell(New Paragraph("(" & dosenPembimbing & ")", infoFont))
                tandaTable.AddCell(New Paragraph("(" & namaMahasiswa & ")", infoFont) With {.Alignment = Element.ALIGN_RIGHT})

                tandaTable.AddCell(New Paragraph("NIDN: " & nidnDosen, infoFont))
                tandaTable.AddCell(New Paragraph("NIM: " & nimMahasiswa, infoFont) With {.Alignment = Element.ALIGN_RIGHT})

                doc.Add(tandaTable)
                doc.Close()

                MessageBox.Show("KHS berhasil dicetak ke PDF", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Catch ex As Exception
                MessageBox.Show("Gagal cetak PDF: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End If
    End Sub


    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim f As New krsvb()
        f.Show()
        Me.Close()
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Form1.emailUser = Nothing

        Dim f As New Form1()
        f.Show()
        Me.Close()
    End Sub

    Private Sub Panel2_Paint(sender As Object, e As PaintEventArgs)

    End Sub

    Private Sub Panel3_Paint(sender As Object, e As PaintEventArgs) Handles Panel3.Paint

    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged

    End Sub

    Private Sub Label2_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub Label4_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub GroupBox1_Enter(sender As Object, e As EventArgs) Handles GroupBox1.Enter

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim f As New Profill()
        f.Show()
        Me.Close()
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click

    End Sub

    Private Sub DataGridView1_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellContentClick

    End Sub
End Class