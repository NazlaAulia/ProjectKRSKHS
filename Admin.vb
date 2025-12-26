Imports ClosedXML.Excel
Imports MySql.Data.MySqlClient
Imports System.IO

Public Class Admin
    Dim conn As New MySqlConnection("server=localhost;user id=root;password=;database=krskhs")
    Dim cmd As MySqlCommand
    Dim da As MySqlDataAdapter
    Dim dt As DataTable
    Dim dr As MySqlDataReader
    Dim selectedID As Integer = -1

    '=========== LOAD FORM ===========
    Private Sub Admin_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        tampilKelas()
        LoadTahunAjaran()
        tampilDataJadwal()
        Button2.Enabled = False
        Button3.Enabled = False


    End Sub

    '=========== ISI COMBO TAHUN AJARAN + PERIODE ===========
    Private Sub LoadTahunAjaran()
        ComboTahunAjaran.Items.Clear()
        ComboTahunAjaran.Items.Add("Ganjil 2024/2025")
        ComboTahunAjaran.Items.Add("Genap 2024/2025")
        ComboTahunAjaran.Items.Add("Ganjil 2025/2026")
        ComboTahunAjaran.Items.Add("Genap 2025/2026")
        ComboTahunAjaran.SelectedIndex = -1
    End Sub


    '=========== TAMPIL KELAS ===========
    Sub tampilKelas()
        Try
            If conn.State = ConnectionState.Open Then conn.Close()
            conn.Open()
            Dim sql As String = "SELECT id_kelas, CONCAT(prodi,' ',nama_kelas) AS kelas FROM kelas ORDER BY prodi,nama_kelas"
            Dim dtKls As New DataTable
            Using daKls As New MySqlDataAdapter(sql, conn)
                daKls.Fill(dtKls)
            End Using
            ComboBox1.DataSource = dtKls
            ComboBox1.DisplayMember = "kelas"
            ComboBox1.ValueMember = "id_kelas"
            ComboBox1.SelectedIndex = -1
        Catch ex As Exception
            MessageBox.Show("Gagal muat kelas: " & ex.Message)
        Finally
            conn.Close()
        End Try
    End Sub

    Private Sub AdminInputMatakuliah_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' === Otomatis isi tahun ajaran berdasarkan tanggal sistem ===
        Dim tahunSekarang As Integer = DateTime.Now.Year
        Dim bulanSekarang As Integer = DateTime.Now.Month
        Dim tahunAjaran As String

        ' Kalau bulan 1–6 = Genap (tahun sebelumnya / tahun sekarang)
        ' Kalau bulan 7–12 = Ganjil (tahun sekarang / tahun berikutnya)
        If bulanSekarang >= 1 AndAlso bulanSekarang <= 6 Then
            tahunAjaran = (tahunSekarang - 1).ToString() & "/" & tahunSekarang.ToString() & " Genap"
        Else
            tahunAjaran = tahunSekarang.ToString() & "/" & (tahunSekarang + 1).ToString() & " Ganjil"
        End If

        ComboTahunAjaran.Items.Clear()
        ComboTahunAjaran.Items.Add(tahunAjaran)
        ComboTahunAjaran.SelectedIndex = 0
    End Sub

    '=========== SAAT KELAS DIPILIH ===========
    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        If ComboBox1.SelectedIndex = -1 OrElse TypeOf ComboBox1.SelectedValue Is DataRowView Then Exit Sub

        Try
            Dim idKelas As Integer = CInt(ComboBox1.SelectedValue)

            If conn.State = ConnectionState.Open Then conn.Close()
            conn.Open()

            Dim prodi As String = ""
            Dim nidnPembimbing As String = ""

            ' Ambil data prodi dan pembimbing (DPA)
            Using q As New MySqlCommand("SELECT prodi, pembimbing FROM kelas WHERE id_kelas=@id", conn)
                q.Parameters.AddWithValue("@id", idKelas)
                Using r = q.ExecuteReader()
                    If r.Read() Then
                        prodi = r("prodi").ToString()
                        nidnPembimbing = r("pembimbing").ToString()
                    End If
                End Using
            End Using

            ' Simpan PRODI ke Tag
            ComboBox1.Tag = prodi

            ' Tampilkan DPA otomatis
            Dim dtPemb As New DataTable
            Using cmdPemb As New MySqlCommand("SELECT nidn, nama FROM dosen WHERE nidn=@nidn", conn)
                cmdPemb.Parameters.AddWithValue("@nidn", nidnPembimbing)
                Using daPemb As New MySqlDataAdapter(cmdPemb)
                    daPemb.Fill(dtPemb)
                End Using
            End Using
            ComboBox6.DataSource = dtPemb
            ComboBox6.DisplayMember = "nama"
            ComboBox6.ValueMember = "nidn"
            ComboBox6.Enabled = False

            ' Muat dosen pengampu sesuai prodi
            Dim dtDosen As New DataTable
            Using cmdDsn As New MySqlCommand("SELECT nidn, nama FROM dosen WHERE prodi LIKE CONCAT('%', @p, '%') ORDER BY nama", conn)
                cmdDsn.Parameters.AddWithValue("@p", prodi)
                Using daDsn As New MySqlDataAdapter(cmdDsn)
                    daDsn.Fill(dtDosen)
                End Using
            End Using
            ComboBox4.DataSource = dtDosen
            ComboBox4.DisplayMember = "nama"
            ComboBox4.ValueMember = "nidn"
            ComboBox4.SelectedIndex = -1

            ' Muat semester
            ComboBox3.DataSource = Nothing
            ComboBox3.Items.Clear()
            LoadSemesterByProdi(prodi)
            ComboBox2.SelectedIndex = -1


        Catch ex As Exception
            MessageBox.Show("Error saat pilih kelas: " & ex.Message)
        Finally
            conn.Close()
        End Try
    End Sub

    '=========== LOAD SEMESTER BERDASARKAN PRODI ===========
    Private Sub LoadSemesterByProdi(prodi As String)
        Try
            If conn.State = ConnectionState.Open Then conn.Close()
            conn.Open()
            Dim dtSmt As New DataTable
            Using cmd As New MySqlCommand("SELECT DISTINCT semester FROM matakuliah WHERE prodi LIKE CONCAT('%', @p, '%') ORDER BY semester", conn)
                cmd.Parameters.AddWithValue("@p", prodi)
                Using daSmt As New MySqlDataAdapter(cmd)
                    daSmt.Fill(dtSmt)
                End Using
            End Using
            ComboBox2.DataSource = dtSmt
            ComboBox2.DisplayMember = "semester"
            ComboBox2.ValueMember = "semester"
            ComboBox2.SelectedIndex = -1
        Catch ex As Exception
            MessageBox.Show("Gagal muat semester: " & ex.Message)
        Finally
            conn.Close()
        End Try
    End Sub

    '=========== SAAT SEMESTER DIPILIH ===========
    Private Sub ComboBox2_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox2.SelectedIndexChanged
        Try
            If ComboBox1.SelectedIndex = -1 OrElse ComboBox1.Tag Is Nothing Then Exit Sub
            Dim prodi As String = ComboBox1.Tag.ToString()
            Dim semester As Integer
            If Not Integer.TryParse(ComboBox2.Text, semester) Then Exit Sub

            ComboBox3.Items.Clear()
            If conn.State = ConnectionState.Open Then conn.Close()
            conn.Open()
            Using cmd As New MySqlCommand("SELECT nama_mk FROM matakuliah WHERE prodi LIKE CONCAT('%', @p, '%') AND semester=@s ORDER BY nama_mk", conn)
                cmd.Parameters.AddWithValue("@p", prodi)
                cmd.Parameters.AddWithValue("@s", semester)
                Using r = cmd.ExecuteReader()
                    While r.Read()
                        ComboBox3.Items.Add(r("nama_mk").ToString())
                    End While
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Gagal muat matakuliah: " & ex.Message)
        Finally
            conn.Close()
        End Try
    End Sub

    '=========== SIMPAN ===========
    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        Try
            If ComboBox1.SelectedIndex = -1 OrElse ComboBox2.Text = "" OrElse ComboBox3.Text = "" OrElse ComboBox4.SelectedIndex = -1 Then
                MessageBox.Show("Lengkapi semua data!")
                Return
            End If

            Dim idKelas As Integer = CInt(ComboBox1.SelectedValue)

            If conn.State = ConnectionState.Open Then conn.Close()
            conn.Open()

            Dim kodeMK As String = ""
            Using cmdMK As New MySqlCommand("SELECT kode_mk FROM matakuliah WHERE nama_mk=@mk", conn)
                cmdMK.Parameters.AddWithValue("@mk", ComboBox3.Text)
                Using r = cmdMK.ExecuteReader()
                    If r.Read() Then kodeMK = r("kode_mk").ToString()
                End Using
            End Using

            ' Ambil NIDN pembimbing (DPA)
            Dim dpaNidn As String = ""
            Using q As New MySqlCommand("SELECT pembimbing FROM kelas WHERE id_kelas=@id", conn)
                q.Parameters.AddWithValue("@id", idKelas)
                dpaNidn = q.ExecuteScalar()?.ToString()
            End Using

            ' Insert jadwal
            Using ins As New MySqlCommand("INSERT INTO jadwal (kode_mk, nidn, prodi, id_kelas) VALUES (@kode, @nidn, @prodi, @kelas)", conn)
                ins.Parameters.AddWithValue("@kode", kodeMK)
                ins.Parameters.AddWithValue("@nidn", ComboBox4.SelectedValue)
                ins.Parameters.AddWithValue("@prodi", ComboBox1.Tag.ToString()) 'ambil dari kelas yang dipilih
                ins.Parameters.AddWithValue("@kelas", idKelas)
                ins.ExecuteNonQuery()
            End Using
            Dim idJadwal As Integer = Convert.ToInt32(New MySqlCommand("SELECT LAST_INSERT_ID()", conn).ExecuteScalar())

            ' Insert ke KRS
            ' Ambil semua NIM mahasiswa di kelas
            Dim nimList As New List(Of String)
            Using getMhs As New MySqlCommand("SELECT nim FROM mahasiswa WHERE id_kelas=@id", conn)
                getMhs.Parameters.AddWithValue("@id", idKelas)
                Using r = getMhs.ExecuteReader()
                    While r.Read()
                        nimList.Add(r("nim").ToString())
                    End While
                End Using
            End Using

            ' Insert ke KRS untuk setiap mahasiswa

            ' Insert ke KRS untuk setiap mahasiswa (status awal kosong)
            For Each nim As String In nimList
                Using insKrs As New MySqlCommand("
        INSERT INTO krs (nim, id_jadwal, dosen_pembimbing, id_kelas)
        SELECT @nim, @id, @dpa, @kelas
        FROM DUAL
        WHERE NOT EXISTS (
            SELECT 1 FROM krs WHERE nim=@nim AND id_jadwal=@id
        );
    ", conn)
                    insKrs.Parameters.AddWithValue("@nim", nim)
                    insKrs.Parameters.AddWithValue("@id", idJadwal)
                    insKrs.Parameters.AddWithValue("@dpa", dpaNidn)
                    insKrs.Parameters.AddWithValue("@kelas", idKelas)
                    insKrs.ExecuteNonQuery()
                End Using
            Next


            MessageBox.Show("Jadwal & KRS berhasil disimpan!")
            tampilDataJadwal()
        Catch ex As Exception
            MessageBox.Show("Error simpan: " & ex.Message)
        Finally
            conn.Close()
        End Try
    End Sub

    '=========== TAMPIL DATA ===========
    Sub tampilDataJadwal()
        Try
            If conn.State = ConnectionState.Open Then conn.Close()
            conn.Open()
            Dim q As String = "
            SELECT 
                j.id_jadwal AS 'ID Jadwal',
                CONCAT(k.prodi, ' ', k.nama_kelas) AS 'Kelas',
                mk.nama_mk AS 'Mata Kuliah',
                mk.semester AS 'Semester',
                d1.nama AS 'Dosen Pengampu',
                d2.nama AS 'Dosen Pembimbing'
            FROM jadwal j
            JOIN kelas k ON j.id_kelas = k.id_kelas
            JOIN matakuliah mk ON j.kode_mk = mk.kode_mk
            JOIN dosen d1 ON j.nidn = d1.nidn
            LEFT JOIN dosen d2 ON k.pembimbing = d2.nidn
            ORDER BY j.id_jadwal DESC"
            da = New MySqlDataAdapter(q, conn)
            dt = New DataTable
            da.Fill(dt)
            DataGridView4.DataSource = dt
        Catch ex As Exception
            MessageBox.Show("Gagal tampil: " & ex.Message)
        Finally
            conn.Close()
        End Try
    End Sub

    '=========== SAAT DATA DIKLIK ===========
    '=========== SAAT DATA DIKLIK ===========
    Private Sub DataGridView4_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView4.CellClick
        Try
            If e.RowIndex >= 0 Then
                Dim row As DataGridViewRow = DataGridView4.Rows(e.RowIndex)

                selectedID = Convert.ToInt32(row.Cells("ID Jadwal").Value)

                ' 🧩 Ambil data dari baris yang diklik
                Dim kelasText As String = row.Cells("Kelas").Value.ToString()
                Dim semesterText As String = row.Cells("Semester").Value.ToString()
                Dim mkText As String = row.Cells("Mata Kuliah").Value.ToString()
                Dim dosenPengampu As String = row.Cells("Dosen Pengampu").Value.ToString()
                Dim dosenPembimbing As String = row.Cells("Dosen Pembimbing").Value.ToString()

                ' 🧩 Cari item kelas berdasarkan teks (karena id_kelas gak tampil di grid)
                For i As Integer = 0 To ComboBox1.Items.Count - 1
                    Dim drv As DataRowView = TryCast(ComboBox1.Items(i), DataRowView)
                    If drv IsNot Nothing AndAlso drv("kelas").ToString() = kelasText Then
                        ComboBox1.SelectedIndex = i
                        Exit For
                    End If
                Next

                ' 🧩 Isi combobox lain
                ComboBox2.Text = semesterText
                ComboBox3.Text = mkText
                ComboBox4.Text = dosenPengampu
                ComboBox6.Text = dosenPembimbing

                ' Aktifkan tombol edit & hapus
                Button2.Enabled = True
                Button3.Enabled = True
            End If
        Catch ex As Exception
            MessageBox.Show("Error saat klik data: " & ex.Message)
        End Try
    End Sub


    '=========== RESET ===========
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        ResetForm()
    End Sub

    '=========== RESET FORM ===========
    Private Sub ResetForm()
        ComboBox1.SelectedIndex = -1
        ComboBox2.SelectedIndex = -1
        ComboBox3.SelectedIndex = -1
        ComboBox4.SelectedIndex = -1
        ComboBox6.DataSource = Nothing
        ComboBox6.Items.Clear()
        ComboBox6.Text = ""

        selectedID = -1
        Button2.Enabled = False
        Button3.Enabled = False
    End Sub


    '=========== HAPUS ===========
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        If selectedID = -1 Then
            MessageBox.Show("Pilih data jadwal yang ingin dihapus dari tabel!")
            Return
        End If

        ' Konfirmasi hapus data
        If MessageBox.Show("Yakin ingin menghapus data ini?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            Try
                If conn.State = ConnectionState.Open Then conn.Close()
                conn.Open()

                ' Hapus data dari tabel krs berdasarkan id_jadwal
                Using delKrs As New MySqlCommand("DELETE FROM krs WHERE id_jadwal=@id", conn)
                    delKrs.Parameters.AddWithValue("@id", selectedID)
                    delKrs.ExecuteNonQuery()
                End Using

                ' Hapus data dari tabel jadwal berdasarkan id_jadwal
                Using delJadwal As New MySqlCommand("DELETE FROM jadwal WHERE id_jadwal=@id", conn)
                    delJadwal.Parameters.AddWithValue("@id", selectedID)
                    delJadwal.ExecuteNonQuery()
                End Using

                MessageBox.Show("Data jadwal berhasil dihapus!")
                tampilDataJadwal()
                ResetForm()  ' Reset form setelah hapus
            Catch ex As Exception
                MessageBox.Show("Gagal hapus: " & ex.Message)
            Finally
                conn.Close()
            End Try
        End If
    End Sub



    '=========== EDIT ===========
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        If selectedID = -1 Then
            MessageBox.Show("Pilih data jadwal yang ingin diedit dari tabel!")
            Return
        End If

        Try
            If conn.State = ConnectionState.Open Then conn.Close()
            conn.Open()

            ' Ambil kode MK dari ComboBox
            Dim kodeMK As String = ""
            Using cmdMK As New MySqlCommand("SELECT kode_mk FROM matakuliah WHERE nama_mk=@mk", conn)
                cmdMK.Parameters.AddWithValue("@mk", ComboBox3.Text)
                Using r = cmdMK.ExecuteReader()
                    If r.Read() Then kodeMK = r("kode_mk").ToString()
                End Using
            End Using

            ' Update jadwal berdasarkan id_jadwal yang sudah dipilih
            Using updateCmd As New MySqlCommand("UPDATE jadwal SET kode_mk=@kode, nidn=@nidn, id_kelas=@kelas WHERE id_jadwal=@id", conn)
                updateCmd.Parameters.AddWithValue("@kode", kodeMK)
                updateCmd.Parameters.AddWithValue("@nidn", ComboBox4.SelectedValue)  ' Dosen Pengampu
                updateCmd.Parameters.AddWithValue("@kelas", ComboBox1.SelectedValue)  ' Kelas
                updateCmd.Parameters.AddWithValue("@id", selectedID)
                updateCmd.ExecuteNonQuery()
            End Using

            MessageBox.Show("Data jadwal berhasil diperbarui!")
            tampilDataJadwal()
            ResetForm()  ' Reset form setelah edit
        Catch ex As Exception
            MessageBox.Show("Gagal update: " & ex.Message)
        Finally
            conn.Close()
        End Try
    End Sub

    Private Sub TabPage1_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Dim Form1 As New Form1()
        Form1.Show()
        Me.Hide()
    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub Label13_Click(sender As Object, e As EventArgs) Handles Label13.Click

    End Sub

    ' === Isi otomatis semester sesuai ganjil/genap ===
    Private Sub ComboTahunAjaran_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboTahunAjaran.SelectedIndexChanged
        If ComboTahunAjaran.SelectedIndex = -1 Then Exit Sub

        Dim textTahun As String = ComboTahunAjaran.Text
        Dim periode As String = ""
        If textTahun.Contains("Ganjil") Then
            periode = "Ganjil"
        ElseIf textTahun.Contains("Genap") Then
            periode = "Genap"
        End If

        ComboBox2.DataSource = Nothing
        ComboBox2.Items.Clear()

        If periode = "Ganjil" Then
            ComboBox2.Items.Add("1")
            ComboBox2.Items.Add("3")
            ComboBox2.Items.Add("5")
            ComboBox2.Items.Add("7")
        ElseIf periode = "Genap" Then
            ComboBox2.Items.Add("2")
            ComboBox2.Items.Add("4")
            ComboBox2.Items.Add("6")
            ComboBox2.Items.Add("8")
        End If

        ComboBox2.SelectedIndex = -1
    End Sub



    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        Dim f As New inputmhs_new()
        f.Show()
        Me.Close()
    End Sub

    Private Sub ComboBox3_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox3.SelectedIndexChanged

    End Sub

    Private Sub Button6_Click_1(sender As Object, e As EventArgs) Handles Button6.Click
        Try
            Dim conn As New MySqlConnection("server=localhost;user id=root;password=;database=krskhs")
            Dim cmd As MySqlCommand

            ' Popup pertama: minta email
            Dim email As String = InputBox("Masukkan email pengguna yang ingin direset:", "Reset Password")
            If String.IsNullOrWhiteSpace(email) Then Exit Sub

            ' Popup kedua: minta password baru
            Dim newPass As String = InputBox("Masukkan password baru:", "Password Baru")
            If String.IsNullOrWhiteSpace(newPass) Then Exit Sub

            conn.Open()

            ' Cek apakah email ada
            cmd = New MySqlCommand("SELECT COUNT(*) FROM login WHERE email=@e", conn)
            cmd.Parameters.AddWithValue("@e", email)
            Dim count As Integer = Convert.ToInt32(cmd.ExecuteScalar())

            If count > 0 Then
                ' Update password
                cmd = New MySqlCommand("UPDATE login SET password=@p WHERE email=@e", conn)
                cmd.Parameters.AddWithValue("@p", newPass)
                cmd.Parameters.AddWithValue("@e", email)
                cmd.ExecuteNonQuery()

                MessageBox.Show("Password berhasil direset untuk: " & email, "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Else
                MessageBox.Show(" Email tidak ditemukan di data.", "Gagal", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If

            conn.Close()

        Catch ex As Exception
            MessageBox.Show("Terjadi kesalahan: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub GroupBox1_Enter(sender As Object, e As EventArgs) Handles GroupBox1.Enter

    End Sub

    Private Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click
        Dim Form1 As New template()
        template.Show()
        Me.Hide()
    End Sub

    Private Sub Button9_Click(sender As Object, e As EventArgs) Handles Button9.Click
        Try
            Dim ofd As New OpenFileDialog With {
                .Filter = "Excel Files|*.xlsx",
                .Title = "Pilih file template Excel"
            }

            If ofd.ShowDialog() <> DialogResult.OK Then Exit Sub

            Dim pathFile As String = ofd.FileName
            If Not File.Exists(pathFile) Then
                MessageBox.Show("File tidak ditemukan!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Exit Sub
            End If

            Dim wb As New XLWorkbook(pathFile)
            Dim ws = wb.Worksheets.First()

            conn.Open()
            Dim insertedCount As Integer = 0

            ' === LOOP DATA MULAI BARIS 2 ===
            For Each row In ws.RowsUsed().Skip(1)

                Dim kode_mk = row.Cell(1).GetValue(Of String)().Trim()
                Dim nama_mk = row.Cell(2).GetString().Trim()
                Dim semester = row.Cell(3).GetString().Trim()
                Dim sks = row.Cell(4).GetString().Trim()
                Dim prodi = row.Cell(5).GetString().Trim()

                ' id_kelas biasanya angka → bisa salah format kalau dari Excel
                Dim id_kelas As Integer
                Integer.TryParse(row.Cell(6).GetString().Trim(), id_kelas)

                ' Ambil NIDN (HARUS dibersihkan)
                Dim nidn_pengampu As String = row.Cell(7).GetString().Trim()

                ' Nama dosen jika NIDN kosong
                Dim nama_dosen As String = row.Cell(8).GetString().Trim()

                ' Skip baris kosong
                If String.IsNullOrEmpty(kode_mk) OrElse id_kelas = 0 Then Continue For

                ' ======================================================
                ' 1️⃣ PERBAIKAN PENTING → BERSIHKAN FORMAT NIDN !!!
                ' ======================================================
                Dim nidnFinal As String = nidn_pengampu.Replace(" ", "").Replace(vbTab, "")

                ' Jika Excel baca sebagai angka (misal 8.23E+09)
                If row.Cell(7).DataType = XLDataType.Number Then
                    nidnFinal = row.Cell(7).GetDouble().ToString("0")
                End If

                ' Jika NIDN kosong tapi ada nama → cari dari DB
                If String.IsNullOrEmpty(nidnFinal) AndAlso nama_dosen <> "" Then
                    Using cmdCek As New MySqlCommand("SELECT nidn FROM dosen WHERE nama=@n LIMIT 1", conn)
                        cmdCek.Parameters.AddWithValue("@n", nama_dosen)
                        Dim hasil = cmdCek.ExecuteScalar()
                        If hasil IsNot Nothing Then nidnFinal = hasil.ToString()
                    End Using
                End If

                ' ======================================================
                ' 2️⃣ CEK APAKAH NIDN VALID DI TABEL DOSEN
                ' ======================================================
                Dim validNidn As Boolean = False

                Using cekNidn As New MySqlCommand("SELECT COUNT(*) FROM dosen WHERE nidn=@n", conn)
                    cekNidn.Parameters.AddWithValue("@n", nidnFinal)
                    validNidn = (Convert.ToInt32(cekNidn.ExecuteScalar()) > 0)
                End Using

                If Not validNidn Then
                    ' SKIP baris invalid → supaya TIDAK ERROR FOREIGN KEY
                    Continue For
                End If


                ' ======================================================
                ' 3️⃣ CEK DUPLIKAT JADWAL
                ' ======================================================
                Dim sudahAda As Integer
                Using cmdCekJadwal As New MySqlCommand("
                SELECT COUNT(*) FROM jadwal 
                WHERE kode_mk=@kode AND nidn=@nidn AND id_kelas=@kelas", conn)
                    cmdCekJadwal.Parameters.AddWithValue("@kode", kode_mk)
                    cmdCekJadwal.Parameters.AddWithValue("@nidn", nidnFinal)
                    cmdCekJadwal.Parameters.AddWithValue("@kelas", id_kelas)
                    sudahAda = Convert.ToInt32(cmdCekJadwal.ExecuteScalar())
                End Using

                If sudahAda > 0 Then Continue For


                ' ======================================================
                ' 4️⃣ INSERT KE JADWAL
                ' ======================================================
                Using cmdInsert As New MySqlCommand("
                INSERT INTO jadwal (kode_mk, nidn, prodi, id_kelas)
                VALUES (@kode, @nidn, @prodi, @kelas)", conn)
                    cmdInsert.Parameters.AddWithValue("@kode", kode_mk)
                    cmdInsert.Parameters.AddWithValue("@nidn", nidnFinal)
                    cmdInsert.Parameters.AddWithValue("@prodi", prodi)
                    cmdInsert.Parameters.AddWithValue("@kelas", id_kelas)
                    cmdInsert.ExecuteNonQuery()
                End Using

                Dim idJadwalBaru As Integer =
                    Convert.ToInt32(New MySqlCommand("SELECT LAST_INSERT_ID()", conn).ExecuteScalar())


                ' ======================================================
                ' 5️⃣ INSERT KRS UNTUK MAHASISWA
                ' ======================================================
                Using cmdGetMhs As New MySqlCommand("
                SELECT nim, pembimbing FROM mahasiswa 
                JOIN kelas ON mahasiswa.id_kelas = kelas.id_kelas
                WHERE mahasiswa.id_kelas = @id", conn)

                    cmdGetMhs.Parameters.AddWithValue("@id", id_kelas)

                    Using r = cmdGetMhs.ExecuteReader()
                        Dim daftar As New List(Of (nim As String, dpa As String))

                        While r.Read()
                            daftar.Add((r("nim").ToString(), r("pembimbing").ToString()))
                        End While

                        r.Close()

                        For Each mhs In daftar
                            Using cmdKrs As New MySqlCommand("
                            INSERT INTO krs (nim, id_jadwal, dosen_pembimbing, id_kelas)
                            SELECT @nim, @id, @dpa, @kelas
                            FROM DUAL
                            WHERE NOT EXISTS (
                                SELECT 1 FROM krs WHERE nim=@nim AND id_jadwal=@id
                            );", conn)

                                cmdKrs.Parameters.AddWithValue("@nim", mhs.nim)
                                cmdKrs.Parameters.AddWithValue("@id", idJadwalBaru)
                                cmdKrs.Parameters.AddWithValue("@dpa", mhs.dpa)
                                cmdKrs.Parameters.AddWithValue("@kelas", id_kelas)
                                cmdKrs.ExecuteNonQuery()
                            End Using
                        Next
                    End Using
                End Using

                insertedCount += 1
            Next

            MessageBox.Show($"{insertedCount} jadwal berhasil ditambahkan dari Excel.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information)
            tampilDataJadwal()

        Catch ex As Exception
            MessageBox.Show("Gagal upload: " & ex.Message)
        Finally
            conn.Close()
        End Try
    End Sub

End Class