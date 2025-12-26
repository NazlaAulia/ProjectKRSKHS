Imports MySql.Data.MySqlClient

Public Class Aprove
    ' ====== KONEKSI ======
    Private ReadOnly conn As New MySqlConnection("server=localhost;user id=root;password=;database=krskhs")

    ' ====== STATE ======
    Private ReadOnly _nim As String

    ' ====== CONSTRUCTOR ======
    Public Sub New(nim As String, nama As String, prodi As String)
        InitializeComponent()
        _nim = nim

        TextBox1.Text = nim
        TextBox2.Text = nama
        TextBox3.Text = prodi
        For Each tb In {TextBox1, TextBox2, TextBox3}
            tb.ReadOnly = True
            tb.BackColor = SystemColors.ControlLight
            tb.TabStop = False
        Next
    End Sub

    Private Sub Aprove_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        SetupGrid()
        LoadKRS()
    End Sub

    ' ====== DESAIN GRID ======
    Private Sub SetupGrid()
        With DataGridView1
            .AutoGenerateColumns = False
            .Columns.Clear()
            .AllowUserToAddRows = False
            .MultiSelect = False
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect
            .ReadOnly = True
        End With

        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "id_krs", .HeaderText = "ID", .DataPropertyName = "id_krs", .Visible = False})
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "kode_mk", .HeaderText = "KODE", .DataPropertyName = "kode_mk", .Width = 90})
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "nama_mk", .HeaderText = "NAMA MATA KULIAH", .DataPropertyName = "nama_mk", .Width = 260})
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "sks", .HeaderText = "SKS", .DataPropertyName = "sks", .Width = 60})
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "status", .HeaderText = "STATUS", .DataPropertyName = "status", .Width = 110})
        DataGridView1.Columns.Add(New DataGridViewButtonColumn With {.Name = "approve", .HeaderText = "AKSI", .Text = "Approve", .UseColumnTextForButtonValue = True, .Width = 90})
        DataGridView1.Columns.Add(New DataGridViewButtonColumn With {.Name = "reject", .HeaderText = "", .Text = "Reject", .UseColumnTextForButtonValue = True, .Width = 80})
    End Sub

    ' ====== LOAD KRS MAHASISWA ======
    Private Sub LoadKRS()
        Try
            If conn.State = ConnectionState.Open Then conn.Close()
            conn.Open()

            Dim sql As String =
               "SELECT k.id_krs, mk.kode_mk, mk.nama_mk, mk.sks, 
        COALESCE(k.status,'Pending') AS status
 FROM krs k
 JOIN jadwal j ON k.id_jadwal = j.id_jadwal
 JOIN matakuliah mk ON j.kode_mk = mk.kode_mk
 WHERE k.nim = @nim
   AND (k.status IS NULL OR k.status <> 'Approved')
 ORDER BY mk.nama_mk"


            Dim dt As New DataTable
            Using da As New MySqlDataAdapter(sql, conn)
                da.SelectCommand.Parameters.AddWithValue("@nim", _nim)
                da.Fill(dt)
            End Using

            DataGridView1.DataSource = dt

            If dt.Rows.Count = 0 Then
                MessageBox.Show("Krs telah di setujui.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If

        Catch ex As Exception
            MessageBox.Show("Gagal muat KRS: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            conn.Close()
        End Try
    End Sub

    ' ====== LOAD FOTO MAHASISWA ======
    Private Sub LoadFotoMahasiswa()
        Try
            If conn.State = ConnectionState.Open Then conn.Close()
            conn.Open()

            Dim query As String = "SELECT foto FROM mahasiswa WHERE nim=@nim"
            Using cmd As New MySqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@nim", _nim)
                Dim result = cmd.ExecuteScalar()

                If result IsNot Nothing AndAlso Not IsDBNull(result) Then
                    Dim bytes As Byte() = DirectCast(result, Byte())
                    Using ms As New IO.MemoryStream(bytes)
                        PictureBox1.Image = Image.FromStream(ms)
                    End Using
                Else
                    SetDefaultFoto()
                End If
            End Using

        Catch ex As Exception
            MessageBox.Show("Gagal memuat foto: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            SetDefaultFoto()
        Finally
            conn.Close()
        End Try
    End Sub


    ' ✅ Tambahkan juga di bawahnya
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

    ' ====== TOMBOL APPROVE / REJECT ======
    Private Sub DataGridView1_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellContentClick
        If e.RowIndex < 0 Then Return
        Dim idKrs As Integer = CInt(DataGridView1.Rows(e.RowIndex).Cells("id_krs").Value)

        If DataGridView1.Columns(e.ColumnIndex).Name = "approve" Then
            UpdateStatus(idKrs, "Approved")
        ElseIf DataGridView1.Columns(e.ColumnIndex).Name = "reject" Then
            UpdateStatus(idKrs, "Rejected")
        End If
    End Sub


    ' ====== APPROVE SEMUA (opsional) ======
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        If MessageBox.Show("Setujui semua mata kuliah untuk Mahasiswa ini?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
            Return
        End If

        Try
            If conn.State = ConnectionState.Open Then conn.Close()
            conn.Open()

            ' 1️⃣ Ambil semua mata kuliah mahasiswa ini yang masih Pending
            Dim dt As New DataTable
            Using da As New MySqlDataAdapter("
            SELECT k.id_krs, mk.sks, k.status
            FROM krs k
            JOIN jadwal j ON k.id_jadwal = j.id_jadwal
            JOIN matakuliah mk ON j.kode_mk = mk.kode_mk
            WHERE k.nim=@nim;", conn)
                da.SelectCommand.Parameters.AddWithValue("@nim", _nim)
                da.Fill(dt)
            End Using

            Dim totalTambah As Integer = 0

            ' 2️⃣ Loop setiap mata kuliah buat hitung SKS baru
            For Each row As DataRow In dt.Rows
                Dim statusLama As String = row("status").ToString()
                Dim sks As Integer = CInt(row("sks"))

                If statusLama <> "Approved" Then
                    totalTambah += sks
                End If
            Next

            ' 3️⃣ Update semua status ke Approved
            Using cmdUpdateKrs As New MySqlCommand("
            UPDATE krs 
            SET status='Approved'
            WHERE nim=@nim;", conn)
                cmdUpdateKrs.Parameters.AddWithValue("@nim", _nim)
                cmdUpdateKrs.ExecuteNonQuery()
            End Using

            ' 4️⃣ Tambahkan total SKS yang baru
            If totalTambah > 0 Then
                Using cmdUpdateSks As New MySqlCommand("
                UPDATE sks 
                SET total_sks = total_sks + @tambah 
                WHERE nim=@nim;", conn)
                    cmdUpdateSks.Parameters.AddWithValue("@tambah", totalTambah)
                    cmdUpdateSks.Parameters.AddWithValue("@nim", _nim)
                    cmdUpdateSks.ExecuteNonQuery()
                End Using
            End If

            LoadKRS()
            MessageBox.Show("Semua mata kuliah disetujui! Total SKS bertambah " & totalTambah & ".", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show("Gagal approve semua: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            conn.Close()
        End Try
    End Sub

    Private Sub UpdateStatus(idKrs As Integer, newStatus As String)
        Try
            If conn.State = ConnectionState.Open Then conn.Close()
            conn.Open()

            ' Cek status lama dulu
            Dim oldStatus As String = ""
            Using cmdOld As New MySqlCommand("SELECT status FROM krs WHERE id_krs=@id;", conn)
                cmdOld.Parameters.AddWithValue("@id", idKrs)
                Dim result = cmdOld.ExecuteScalar()
                If result IsNot Nothing Then oldStatus = result.ToString()
            End Using

            ' Update status baru
            Using cmd As New MySqlCommand("
            UPDATE krs 
            SET status=@s 
            WHERE id_krs=@id AND nim=@nim;", conn)
                cmd.Parameters.AddWithValue("@s", newStatus)
                cmd.Parameters.AddWithValue("@id", idKrs)
                cmd.Parameters.AddWithValue("@nim", _nim)
                cmd.ExecuteNonQuery()
            End Using

            ' Ambil nilai SKS dari MK
            Dim sks As Integer = 0
            Using cmdSks As New MySqlCommand("
            SELECT mk.sks 
            FROM krs k
            JOIN jadwal j ON k.id_jadwal=j.id_jadwal
            JOIN matakuliah mk ON j.kode_mk=mk.kode_mk
            WHERE k.id_krs=@id;", conn)
                cmdSks.Parameters.AddWithValue("@id", idKrs)
                Dim result = cmdSks.ExecuteScalar()
                If result IsNot Nothing Then sks = CInt(result)
            End Using

            ' Update total SKS HANYA kalau status berubah dari Pending → Approved
            If oldStatus <> "Approved" AndAlso newStatus = "Approved" Then
                Using cmdUpdate As New MySqlCommand("
                UPDATE sks 
                SET total_sks = total_sks + @sks 
                WHERE nim=@nim;", conn)
                    cmdUpdate.Parameters.AddWithValue("@sks", sks)
                    cmdUpdate.Parameters.AddWithValue("@nim", _nim)
                    cmdUpdate.ExecuteNonQuery()
                End Using

            ElseIf oldStatus = "Approved" AndAlso newStatus = "Rejected" Then
                Using cmdUpdate As New MySqlCommand("
                UPDATE sks 
                SET total_sks = GREATEST(total_sks - @sks, 0)
                WHERE nim=@nim;", conn)
                    cmdUpdate.Parameters.AddWithValue("@sks", sks)
                    cmdUpdate.Parameters.AddWithValue("@nim", _nim)
                    cmdUpdate.ExecuteNonQuery()
                End Using
            End If

            LoadKRS()

        Catch ex As Exception
            MessageBox.Show("Gagal update status: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            conn.Close()
        End Try
    End Sub


    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        LoadKRS()
        MessageBox.Show("Perubahan tersimpan.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub Button3_Click_1(sender As Object, e As EventArgs) Handles Button3.Click
        Dim aprove1 As New Aprove1()
        aprove1.Show()
        Me.Hide()
    End Sub

    Private Sub Panel2_Paint(sender As Object, e As PaintEventArgs) Handles Panel2.Paint

    End Sub
End Class
