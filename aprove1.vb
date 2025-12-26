Imports MySql.Data.MySqlClient
Imports System.Text.RegularExpressions

Public Class Aprove1
    Private ReadOnly conn As New MySqlConnection(
        "server=localhost;user id=root;password=;database=krskhs")

    Private Sub Aprove1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        With DataGridView1
            .AutoGenerateColumns = True
            .AllowUserToAddRows = False
            .MultiSelect = False
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect
            .ReadOnly = True
        End With
        TampilMahasiswaBimbingan()
    End Sub

    Private Sub TampilMahasiswaBimbingan()

        Try
            If conn.State = ConnectionState.Open Then conn.Close()
            conn.Open()

            Dim emailDosen As String = Form1.emailUser

            ' cuma tampilkan mahasiswa yang udah input KRS dan statusnya masih Pending
            Dim sql As String =
                "SELECT DISTINCT m.nim, m.nama, m.prodi
             FROM mahasiswa m
             JOIN kelas k ON m.id_kelas = k.id_kelas
             JOIN dosen d ON k.pembimbing = d.nidn
             JOIN krs kr ON kr.nim = m.nim
             WHERE d.email = @e
             AND kr.status = 'Pending'
             ORDER BY m.nama"

            Dim dt As New DataTable
            Using da As New MySqlDataAdapter(sql, conn)
                da.SelectCommand.Parameters.AddWithValue("@e", emailDosen)
                da.Fill(dt)
            End Using

            DataGridView1.DataSource = dt
            If DataGridView1.Columns.Contains("nim") Then DataGridView1.Columns("nim").HeaderText = "NIM"
            If DataGridView1.Columns.Contains("nama") Then DataGridView1.Columns("nama").HeaderText = "NAMA"
            If DataGridView1.Columns.Contains("prodi") Then DataGridView1.Columns("prodi").HeaderText = "PRODI"

        Catch ex As Exception
            MessageBox.Show("Gagal tampil mahasiswa: " & ex.Message,
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            conn.Close()
        End Try
    End Sub





    ' tombol cari
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click

        If String.IsNullOrWhiteSpace(TextBox1.Text) Then
            TampilMahasiswaBimbingan()
        Else
            CariMahasiswa()
        End If


    End Sub

    '=== PENCARIAN MAHASISWA ===
    Private Sub CariMahasiswa()
        Try
            If conn.State = ConnectionState.Open Then conn.Close()
            conn.Open()

            Dim emailDosen As String = Form1.emailUser
            Dim keyword As String = TextBox1.Text.Trim()

            Dim sql As String =
            "SELECT DISTINCT m.nim, m.nama, m.prodi
             FROM mahasiswa m
             JOIN kelas k ON m.id_kelas = k.id_kelas
             JOIN dosen d ON k.pembimbing = d.nidn
             JOIN krs kr ON kr.nim = m.nim
             WHERE d.email = @e
             AND kr.status = 'Pending'
             AND (LOWER(m.nama) LIKE LOWER(@key)
                  OR LOWER(m.nim) LIKE LOWER(@key))
             ORDER BY m.nama"

            Dim dt As New DataTable
            Using da As New MySqlDataAdapter(sql, conn)
                da.SelectCommand.Parameters.AddWithValue("@e", emailDosen)
                da.SelectCommand.Parameters.AddWithValue("@key", "%" & keyword & "%")
                da.Fill(dt)
            End Using

            DataGridView1.DataSource = dt

        Catch ex As Exception
            MessageBox.Show("Gagal mencari mahasiswa: " & ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            conn.Close()
        End Try
    End Sub


    ' double-click baris = buka KRS
    Private Sub DataGridView1_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) _
        Handles DataGridView1.CellDoubleClick
        If e.RowIndex >= 0 Then Button2_Click(sender, e)
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        Dim f As New Form1()
        f.Show()
        Me.Close()
    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        Dim f As New input()
        f.Show()
        Me.Close()
    End Sub

    Private Sub Panel2_Paint(sender As Object, e As PaintEventArgs) Handles Panel2.Paint

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If DataGridView1.CurrentRow Is Nothing Then
            MessageBox.Show("Pilih mahasiswa dulu!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Exit Sub
        End If

        Dim r As DataGridViewRow = DataGridView1.CurrentRow
        Dim nim As String = r.Cells("nim").Value.ToString()
        Dim nama As String = r.Cells("nama").Value.ToString()
        Dim prodi As String = r.Cells("prodi").Value.ToString()

        ' kirim ke form Aprove
        Dim f As New Aprove(nim, nama, prodi)
        f.StartPosition = FormStartPosition.CenterScreen
        f.Show()
        Me.Hide()
    End Sub

    Private Sub TextBox1_KeyDown(sender As Object, e As KeyEventArgs) Handles TextBox1.KeyDown, Button2.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            CariMahasiswa()
        End If
    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged

    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click

    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        ' Pastikan ada baris yang dipilih di DataGridView
        If DataGridView1.SelectedRows.Count = 0 Then
            MessageBox.Show("Pilih mahasiswa dulu dari tabel.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        ' Ambil NIM dari kolom DataGridView (ubah sesuai nama kolom kamu)
        Dim nimMahasiswa As String = DataGridView1.SelectedRows(0).Cells("nim").Value.ToString()

        ' Buka form profil mahasiswa
        Dim f As New Profil2()
        f.NimAktif = nimMahasiswa
        f.Show()
           Me.Close()
    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        Dim f As New profildosen()
        f.Show()
        Me.Close()
    End Sub
End Class
