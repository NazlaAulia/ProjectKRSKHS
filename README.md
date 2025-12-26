
#  Sistem KRS & KHS Mahasiswa (Desktop)

Sistem **Kartu Rencana Studi (KRS)** dan **Kartu Hasil Studi (KHS)** berbasis **Desktop** yang dibangun menggunakan **VB.NET WinForms** dan **MySQL**.  
Aplikasi ini dirancang untuk membantu proses pengelolaan KRS dan KHS mahasiswa secara **terstruktur, terintegrasi, dan efisien**.

---

##  Tujuan Sistem
- Mempermudah mahasiswa dalam melakukan pengisian KRS
- Membantu dosen pembimbing akademik (DPA) dalam proses persetujuan KRS
- Menyediakan sistem pengelolaan data akademik berbasis database relasional
- Mengurangi kesalahan pencatatan SKS dan duplikasi data

##  Fitur Utama

###  Mahasiswa
- Login menggunakan akun terdaftar
- Melihat profil mahasiswa
- Memilih mata kuliah sesuai semester
- Status mata kuliah: **Pending / Approved / Rejected**
- Perhitungan total SKS otomatis
- Cetak KRS ke format **PDF**
- Melihat KHS per semester
- Ganti password

###  Dosen Pembimbing Akademik
- Melihat daftar mahasiswa bimbingan
- Menyetujui atau menolak mata kuliah KRS
- Monitoring total SKS mahasiswa
- Keputusan persetujuan tersimpan otomatis ke database

###  Admin
- CRUD data Mahasiswa, Dosen, Kelas, Mata Kuliah
- Import jadwal perkuliahan dari file Excel
- Pengelolaan jadwal dan kelas
- Monitoring data KRS mahasiswa


##  Struktur Database

Sistem ini menggunakan **database relasional MySQL** dengan tabel utama sebagai berikut:

- **mahasiswa**
- **dosen**
- **kelas**
- **matakuliah**
- **jadwal**
- **krs**
- **sks**

Relasi utama:
- Mahasiswa → KRS → Jadwal → Mata Kuliah
- Kelas menentukan dosen pembimbing akademik
- Total SKS dihitung otomatis berdasarkan mata kuliah yang disetujui


## Teknologi yang Digunakan

| Komponen | Teknologi |
|--------|----------|
| Bahasa Pemrograman | VB.NET |
| Framework | .NET Framework |
| Database | MySQL |
| UI | Windows Forms |
| PDF Generator | iTextSharp |
| Import Excel | ClosedXML |
| Database Connector | MySql.Data |

---

##  Alur Sistem

1. User melakukan login
2. Sistem mengecek role pengguna
3. Mahasiswa memilih mata kuliah (status **Pending**)
4. Dosen pembimbing menyetujui / menolak KRS
5. SKS dihitung otomatis
6. Mahasiswa dapat mencetak KRS dan melihat KHS

---

## 📦 Instalasi & Konfigurasi

1. Clone repository:
   ```bash
   [(https://github.com/NazlaAulia/ProjectKRSKHS.git)]


2. Import database MySQL:

Gunakan file .sql yang tersedia

Pastikan nama database sesuai konfigurasi


3. Konfigurasi koneksi database:

server=localhost;
user id=root;
password=;
database=krskhs


4. Jalankan aplikasi melalui Visual Studio


📸 Screenshot beberapa contoh dari aplikasi
<img width="878" height="621" alt="Screenshot 2025-11-27 142101" src="https://github.com/user-attachments/assets/f87edd5e-74a0-4daa-95d7-0cce6c649701" />
<img width="1130" height="823" alt="Screenshot 2025-11-27 150016" src="https://github.com/user-attachments/assets/8494869e-2ab0-4c12-8a69-1cec136a7d05" />
<img width="750" height="630" alt="Screenshot 2025-11-27 145907" src="https://github.com/user-attachments/assets/1f6b0d54-2209-417a-8283-08cc5abbd79f" />
<img width="868" height="613" alt="Screenshot 2025-11-27 144040" src="https://github.com/user-attachments/assets/6c15c0e0-251b-499d-b5bc-0c4fa9c9a6c3" />
<img width="879" height="617" alt="Screenshot 2025-11-27 144007" src="https://github.com/user-attachments/assets/9a560db5-8e81-4a81-bc1f-03ebac8aa33f" />
<img width="1073" height="615" alt="Screenshot 2025-11-27 143958" src="https://github.com/user-attachments/assets/5d28ff14-83fa-4046-b996-8d4e48ff4f52" />
<img width="967" height="705" alt="Screenshot 2025-11-27 142613" src="https://github.com/user-attachments/assets/fbbeb08e-fe66-4a2f-9fab-3d7c41669125" />
<img width="878" height="613" alt="Screenshot 2025-11-27 150453" src="https://github.com/user-attachments/assets/10001437-d63b-4072-a4fb-0826ef117c46" />



📝 Catatan

1. Sistem ini dibuat untuk keperluan akademik dan pembelajaran
2. Struktur database dirancang untuk 1 Program Studi
3. Aplikasi masih dapat dikembangkan ke versi multi-prodi dan web-based

👤 Author

Nama : Nazla salsabila aulia bachri,Aliyah salsabila,Octavia rahmadani
Program Studi : Pendidikan Teknologi Informasi
Institusi : Universitas Negeri Surabaya


📄 Lisensi
Proyek ini dibuat untuk keperluan pembelajaran dan tidak digunakan untuk tujuan komersial.
