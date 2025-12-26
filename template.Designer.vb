<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Template
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Template))
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.cbKelas = New System.Windows.Forms.ComboBox()
        Me.cbSemester = New System.Windows.Forms.ComboBox()
        Me.btnDownloadTemplate = New System.Windows.Forms.Button()
        Me.Button4 = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.BackColor = System.Drawing.Color.Transparent
        Me.Label1.Location = New System.Drawing.Point(52, 95)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(68, 16)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Pilih kelas"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.BackColor = System.Drawing.Color.Transparent
        Me.Label2.Location = New System.Drawing.Point(52, 173)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(55, 16)
        Me.Label2.TabIndex = 1
        Me.Label2.Text = "semster"
        '
        'cbKelas
        '
        Me.cbKelas.FormattingEnabled = True
        Me.cbKelas.Location = New System.Drawing.Point(175, 95)
        Me.cbKelas.Name = "cbKelas"
        Me.cbKelas.Size = New System.Drawing.Size(339, 24)
        Me.cbKelas.TabIndex = 2
        '
        'cbSemester
        '
        Me.cbSemester.FormattingEnabled = True
        Me.cbSemester.Location = New System.Drawing.Point(175, 165)
        Me.cbSemester.Name = "cbSemester"
        Me.cbSemester.Size = New System.Drawing.Size(339, 24)
        Me.cbSemester.TabIndex = 2
        '
        'btnDownloadTemplate
        '
        Me.btnDownloadTemplate.BackColor = System.Drawing.Color.MediumSlateBlue
        Me.btnDownloadTemplate.ForeColor = System.Drawing.Color.Transparent
        Me.btnDownloadTemplate.Location = New System.Drawing.Point(439, 226)
        Me.btnDownloadTemplate.Name = "btnDownloadTemplate"
        Me.btnDownloadTemplate.Size = New System.Drawing.Size(75, 34)
        Me.btnDownloadTemplate.TabIndex = 3
        Me.btnDownloadTemplate.Text = "download"
        Me.btnDownloadTemplate.UseVisualStyleBackColor = False
        '
        'Button4
        '
        Me.Button4.BackColor = System.Drawing.Color.MediumPurple
        Me.Button4.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.8!)
        Me.Button4.ForeColor = System.Drawing.Color.RoyalBlue
        Me.Button4.Image = CType(resources.GetObject("Button4.Image"), System.Drawing.Image)
        Me.Button4.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.Button4.Location = New System.Drawing.Point(378, 226)
        Me.Button4.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.Button4.Name = "Button4"
        Me.Button4.Size = New System.Drawing.Size(40, 34)
        Me.Button4.TabIndex = 33
        Me.Button4.UseVisualStyleBackColor = False
        '
        'Template
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackgroundImage = CType(resources.GetObject("$this.BackgroundImage"), System.Drawing.Image)
        Me.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.ClientSize = New System.Drawing.Size(541, 327)
        Me.Controls.Add(Me.Button4)
        Me.Controls.Add(Me.btnDownloadTemplate)
        Me.Controls.Add(Me.cbSemester)
        Me.Controls.Add(Me.cbKelas)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.DoubleBuffered = True
        Me.Name = "Template"
        Me.Text = "template"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents cbKelas As ComboBox
    Friend WithEvents cbSemester As ComboBox
    Friend WithEvents btnDownloadTemplate As Button
    Friend WithEvents Button4 As Button
End Class
