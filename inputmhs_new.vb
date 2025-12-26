Imports MySql.Data.MySqlClient
Imports ClosedXML.Excel
Imports System.IO
Imports System.Data

Public Class inputmhs_new

    ' ================== STATE & KONEKSI ==================
    Private ReadOnly conn As New MySqlConnection("server=localhost;user id=root;password=;database=krskhs")

    ' buffer upload agar bisa dipakai saat SIMPAN
    Private dtUpload As New DataTable With {.TableName = "upload"}

    ' ================== FORM LOAD ==================
    Private Sub inputmhs_new_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' --- isi pilihan tetap ---
        ComboBox1.Items.Clear()          ' Program Studi
        ComboBox1.Items.Add("S1 Pendidikan Teknologi Informasi")

        ComboFakultas.Items.Clear()      ' Fakultas
        ComboFakultas.Items.Add("Fakultas Teknik")

        ' --- Tahun ajaran otomatis ---
        Dim th As Integer = DateTime.Now.Year
        Dim bln As Integer = DateTime.Now.Month
        Dim thAjar As String = If(bln >= 1 AndAlso bln <= 6, $"{th - 1}/{th} Genap", $"{th}/{th + 1} Ganjil")
        ComboBox4.Items.Clear()
        ComboBox4.Items.Add(thAjar)
        ComboBox4.SelectedIndex = 0

        ' --- Kelas huruf A–H (nama_kelas di DB cukup A/B/C/...) ---
        ComboBox2.Items.Clear()
        For Each huruf In {"A", "B", "C", "D", "E", "F", "G", "H"}
            ComboBox2.Items.Add(huruf)
        Next

        ' --- Dosen pembimbing ---
        TampilDosen()

        ' siapkan dtUpload: kolom minimal yang dibaca dari Excel
        If dtUpload.Columns.Count = 0 Then
            dtUpload.Columns.Add("nim", GetType(String))
            dtUpload.Columns.Add("nama", GetType(String))
            dtUpload.Columns.Add("email", GetType(String))
            dtUpload.Columns.Add("tgl_lahir", GetType(String)) ' simpan string, nanti diparse saat simpan
        End If
    End Sub

    Private Sub TampilDosen()
        Try
            If conn.State = ConnectionState.Open Then conn.Close()
            conn.Open()

            Dim dtDsn As New DataTable
            Using da As New MySqlDataAdapter("SELECT nidn, nama FROM dosen ORDER BY nama", conn)
                da.Fill(dtDsn)
            End Using

            ComboBox3.DataSource = dtDsn
            ComboBox3.DisplayMember = "nama"
            ComboBox3.ValueMember = "nidn"
            ComboBox3.SelectedIndex = -1

        Catch ex As Exception
            MessageBox.Show("Gagal muat dosen: " & ex.Message)
        Finally
            conn.Close()
        End Try
    End Sub

    ' ================== DOWNLOAD TEMPLATE EXCEL ==================
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        Try
            Dim sfd As New SaveFileDialog With {
                .Filter = "Excel Files|*.xlsx",
                .FileName = "Template_Data_Mahasiswa.xlsx"
            }
            If sfd.ShowDialog() <> DialogResult.OK Then Return

            Using wb As New XLWorkbook()
                Dim ws = wb.Worksheets.Add("Mahasiswa")
                ' Header
                ws.Cell(1, 1).Value = "nim"
                ws.Cell(1, 2).Value = "nama"
                ws.Cell(1, 3).Value = "email"
                ws.Cell(1, 4).Value = "tgl_lahir" ' format: YYYY-MM-DD
                ws.Cell(1, 5).Value = "password"

                ' Contoh baris
                ws.Cell(2, 1).Value = "24050974001"
                ws.Cell(2, 2).Value = "Ahmad Naufal"
                ws.Cell(2, 3).Value = "naufal@unesa.ac.id"
                ws.Cell(2, 4).Value = "2004-07-21"
                ws.Cell(2, 5).Value = "p4ssword123"

                ws.Columns().AdjustToContents()
                wb.SaveAs(sfd.FileName)
            End Using

            MessageBox.Show("Template Excel berhasil dibuat!,silahkan diisi", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show("Gagal membuat template: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try


    End Sub

    ' ================== UPLOAD EXCEL (isi dtUpload) ==================
    ' ================== UPLOAD EXCEL (isi dtUpload) ==================
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click

        Try
            Dim ofd As New OpenFileDialog With {.Filter = "Excel Files|*.xlsx;*.xls"}
            If ofd.ShowDialog() <> DialogResult.OK Then Return

            dtUpload.Rows.Clear()
            If dtUpload.Columns.Count < 5 Then
                If Not dtUpload.Columns.Contains("password") Then dtUpload.Columns.Add("password", GetType(String))
            End If

            Using wb As New XLWorkbook(ofd.FileName)
                Dim ws = wb.Worksheets.First()
                Dim lastRow = ws.LastRowUsed().RowNumber()

                For r As Integer = 2 To lastRow
                    Dim nim = ws.Cell(r, 1).GetString().Trim()
                    Dim nama = ws.Cell(r, 2).GetString().Trim()
                    Dim email = ws.Cell(r, 3).GetString().Trim()
                    Dim tgl = ws.Cell(r, 4).GetString().Trim()
                    Dim password = ws.Cell(r, 5).GetString().Trim()

                    If nim <> "" Then dtUpload.Rows.Add(nim, nama, email, tgl, password)
                Next
            End Using

            MessageBox.Show($"Upload ok. {dtUpload.Rows.Count} data berhasil di upload.", "Info",
                        MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show("Error upload: " & ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try


    End Sub


    ' ================== SIMPAN: buat/ambil kelas, lalu UPSERT mahasiswa ==================
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        ' Validasi input wajib
        If ComboBox1.Text = "" OrElse ComboFakultas.Text = "" OrElse ComboBox4.Text = "" OrElse
       ComboBox2.Text = "" OrElse ComboBox3.SelectedIndex = -1 Then
            MessageBox.Show("Lengkapi semua data terlebih dahulu!")
            Return
        End If

        ' Ambil angkatan dari TextBox1
        Dim angkatan As String = TextBox1.Text.Trim()
        If angkatan.Length <> 4 OrElse Not angkatan.All(AddressOf Char.IsDigit) Then
            MessageBox.Show("Angkatan harus 4 digit (misal: 2023).")
            Return
        End If

        Dim hurufKelas As String = ComboBox2.Text.Trim().ToUpper()
        Dim namaKelasLengkap As String = $"PTI {angkatan} {hurufKelas}"

        Try
            If conn.State = ConnectionState.Open Then conn.Close()
            conn.Open()

            Using tr = conn.BeginTransaction()
                ' 1️⃣ Cek atau buat kelas baru
                Dim idKelas As Integer
                Using cek As New MySqlCommand("
                SELECT id_kelas FROM kelas 
                WHERE nama_kelas=@n AND prodi=@p AND tahun_ajaran=@t
                LIMIT 1;", conn, tr)
                    cek.Parameters.AddWithValue("@n", namaKelasLengkap)
                    cek.Parameters.AddWithValue("@p", ComboBox1.Text)
                    cek.Parameters.AddWithValue("@t", ComboBox4.Text)
                    Dim obj = cek.ExecuteScalar()
                    If obj IsNot Nothing Then
                        idKelas = CInt(obj)
                    Else
                        Using ins As New MySqlCommand("
                        INSERT INTO kelas (nama_kelas, prodi, tahun_ajaran, pembimbing)
                        VALUES (@n,@p,@t,@pb); SELECT LAST_INSERT_ID();", conn, tr)
                            ins.Parameters.AddWithValue("@n", namaKelasLengkap)
                            ins.Parameters.AddWithValue("@p", ComboBox1.Text)
                            ins.Parameters.AddWithValue("@t", ComboBox4.Text)
                            ins.Parameters.AddWithValue("@pb", ComboBox3.SelectedValue)
                            idKelas = CInt(ins.ExecuteScalar())
                        End Using
                    End If
                End Using

                ' 2️⃣ Simpan setiap mahasiswa dan akun login
                Using cmdMhs As New MySqlCommand("
                INSERT INTO mahasiswa (nim, nama, prodi, fakultas, email, tgl_lahir, id_kelas)
                VALUES (@nim, @nama, @prodi, @fak, @em, @tgl, @idk)
                ON DUPLICATE KEY UPDATE
                    nama = VALUES(nama),
                    prodi = VALUES(prodi),
                    fakultas = VALUES(fakultas),
                    email = VALUES(email),
                    tgl_lahir = VALUES(tgl_lahir),
                    id_kelas = VALUES(id_kelas);", conn, tr)

                    Using cmdLogin As New MySqlCommand("
    INSERT INTO login (email, password, username, role)
    VALUES (@em, @pw, @nm, 'mahasiswa')
    ON DUPLICATE KEY UPDATE
        password = VALUES(password),
        username = VALUES(username),
        role = VALUES(role);", conn, tr)


                        ' Parameter mahasiswa
                        cmdMhs.Parameters.Add("@nim", MySqlDbType.VarChar)
                        cmdMhs.Parameters.Add("@nama", MySqlDbType.VarChar)
                        cmdMhs.Parameters.Add("@prodi", MySqlDbType.VarChar)
                        cmdMhs.Parameters.Add("@fak", MySqlDbType.VarChar)
                        cmdMhs.Parameters.Add("@em", MySqlDbType.VarChar)
                        cmdMhs.Parameters.Add("@tgl", MySqlDbType.Date)
                        cmdMhs.Parameters.Add("@idk", MySqlDbType.Int32)

                        ' Parameter login
                        cmdLogin.Parameters.Add("@em", MySqlDbType.VarChar)
                        cmdLogin.Parameters.Add("@pw", MySqlDbType.VarChar)
                        cmdLogin.Parameters.Add("@nm", MySqlDbType.VarChar)

                        ' Loop tiap baris Excel
                        ' Loop tiap baris Excel
                        For Each r As DataRow In dtUpload.Rows
                            ' ==== DATA MAHASISWA ====
                            cmdMhs.Parameters("@nim").Value = r("nim").ToString()
                            cmdMhs.Parameters("@nama").Value = r("nama").ToString()
                            cmdMhs.Parameters("@prodi").Value = ComboBox1.Text
                            cmdMhs.Parameters("@fak").Value = ComboFakultas.Text
                            cmdMhs.Parameters("@em").Value = r("email").ToString()

                            ' Parsing tanggal
                            Dim tglObj As Object = DBNull.Value
                            Dim tglStr = r("tgl_lahir").ToString().Trim()
                            Dim dt As Date
                            If Date.TryParse(tglStr, dt) Then
                                tglObj = dt
                            End If
                            cmdMhs.Parameters("@tgl").Value = tglObj
                            cmdMhs.Parameters("@idk").Value = idKelas

                            ' ==== DATA LOGIN (PARENT) ====
                            cmdLogin.Parameters("@em").Value = r("email").ToString()
                            cmdLogin.Parameters("@pw").Value = r("password").ToString()
                            cmdLogin.Parameters("@nm").Value = r("nama").ToString()

                            ' PENTING: insert/ update LOGIN dulu, baru MAHASISWA
                            cmdLogin.ExecuteNonQuery()   ' parent
                            cmdMhs.ExecuteNonQuery()     ' child
                        Next

                    End Using
                End Using

                tr.Commit()
            End Using

            MessageBox.Show($"✅ Sukses! Kelas '{namaKelasLengkap}' tersimpan, " &
                        $"{dtUpload.Rows.Count} data mahasiswa berhasil dibuat.",
                        "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show("Gagal simpan: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            conn.Close()
        End Try
    End Sub



    ' ================== HELPER: GetOrCreateKelasId ==================
    Private Function GetOrCreateKelasId(namaKelas As String,
                                        prodi As String,
                                        tahunAjar As String,
                                        nidnPembimbing As String,
                                        openConn As MySqlConnection) As Integer

        ' Cek sudah ada?
        Using cek As New MySqlCommand("
            SELECT id_kelas FROM kelas
            WHERE nama_kelas=@n AND prodi=@p AND tahun_ajaran=@t
            LIMIT 1", openConn)
            cek.Parameters.AddWithValue("@n", namaKelas)
            cek.Parameters.AddWithValue("@p", prodi)
            cek.Parameters.AddWithValue("@t", tahunAjar)

            Dim obj = cek.ExecuteScalar()
            If obj IsNot Nothing AndAlso obj IsNot DBNull.Value Then
                Return Convert.ToInt32(obj)
            End If
        End Using

        ' Belum ada → insert
        Using ins As New MySqlCommand("
            INSERT INTO kelas (nama_kelas, prodi, tahun_ajaran, pembimbing)
            VALUES (@n,@p,@t,@pb); SELECT LAST_INSERT_ID();", openConn)
            ins.Parameters.AddWithValue("@n", namaKelas)
            ins.Parameters.AddWithValue("@p", prodi)
            ins.Parameters.AddWithValue("@t", tahunAjar)
            ins.Parameters.AddWithValue("@pb", nidnPembimbing)
            Dim newId = Convert.ToInt32(ins.ExecuteScalar())
            Return newId
        End Using
    End Function

    Private Sub Panel1_Paint(sender As Object, e As PaintEventArgs)

    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        Dim f As New Admin()
        f.Show()
        Me.Close()
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click

        ' Kosongkan semua inputan
        ComboBox1.SelectedIndex = -1
        ComboFakultas.SelectedIndex = -1
        ComboBox2.SelectedIndex = -1
        ComboBox3.SelectedIndex = -1
        ComboBox4.SelectedIndex = -1
        TextBox1.Clear()

        ' Kosongkan buffer upload
        dtUpload.Rows.Clear()

        ' (Opsional) beri pesan konfirmasi
        MessageBox.Show("Form telah direset.", "Reset", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub Panel1_Paint_1(sender As Object, e As PaintEventArgs) Handles Panel1.Paint

    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged

    End Sub
End Class
