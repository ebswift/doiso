'
' Created by SharpDevelop.
' User: ${USER}
' Date: ${DATE}
' Time: ${TIME}
' 
' To change this template use Tools | Options | Coding | Edit Standard Headers.
'
Namespace DoISO
	Partial Class ProcessDialog
		Inherits System.Windows.Forms.Form
		
		''' <summary>
		''' Designer variable used to keep track of non-visual components.
		''' </summary>
		Private components As System.ComponentModel.IContainer
		
		''' <summary>
		''' Disposes resources used by the form.
		''' </summary>
		''' <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		Protected Overrides Sub Dispose(ByVal disposing As Boolean)
			If disposing Then
				If components IsNot Nothing Then
					components.Dispose()
				End If
			End If
			MyBase.Dispose(disposing)
		End Sub
		
		''' <summary>
		''' This method is required for Windows Forms designer support.
		''' Do not change the method contents inside the source code editor. The Forms designer might
		''' not be able to load this method if it was changed manually.
		''' </summary>
		Private Sub InitializeComponent()
			Me.txtOutput = New System.Windows.Forms.TextBox
			Me.btnCancel = New Wildgrape.Aqua.Controls.Button
			Me.progRunning = New System.Windows.Forms.ProgressBar
			Me.statusStrip1 = New System.Windows.Forms.StatusStrip
			Me.StatusLabel1 = New System.Windows.Forms.ToolStripStatusLabel
			Me.ProgressBar1 = New System.Windows.Forms.ToolStripProgressBar
			Me.panel1 = New System.Windows.Forms.Panel
			Me.lblImageSize = New System.Windows.Forms.Label
			Me.btnUpdateSize = New Wildgrape.Aqua.Controls.Button
			Me.label8 = New System.Windows.Forms.Label
			Me.label7 = New System.Windows.Forms.Label
			Me.label6 = New System.Windows.Forms.Label
			Me.progSize = New System.Windows.Forms.ProgressBar
			Me.saveFileDialog1 = New System.Windows.Forms.SaveFileDialog
			Me.btnClose = New Wildgrape.Aqua.Controls.Button
			Me.statusStrip1.SuspendLayout
			Me.panel1.SuspendLayout
			Me.SuspendLayout
			'
			'txtOutput
			'
			Me.txtOutput.BackColor = System.Drawing.Color.White
			Me.txtOutput.ForeColor = System.Drawing.Color.Blue
			Me.txtOutput.Location = New System.Drawing.Point(0, -1)
			Me.txtOutput.Multiline = true
			Me.txtOutput.Name = "txtOutput"
			Me.txtOutput.ReadOnly = true
			Me.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
			Me.txtOutput.Size = New System.Drawing.Size(680, 402)
			Me.txtOutput.TabIndex = 11
			Me.txtOutput.Text = "http://www.ebswift.com"&Global.Microsoft.VisualBasic.ChrW(13)&Global.Microsoft.VisualBasic.ChrW(10)&Global.Microsoft.VisualBasic.ChrW(13)&Global.Microsoft.VisualBasic.ChrW(10)&"Licensed under the GPL"&Global.Microsoft.VisualBasic.ChrW(13)&Global.Microsoft.VisualBasic.ChrW(10)&Global.Microsoft.VisualBasic.ChrW(13)&Global.Microsoft.VisualBasic.ChrW(10)&"Contains code from the mkisof"& _ 
			"s project, Mike Mayer "&Global.Microsoft.VisualBasic.ChrW(13)&Global.Microsoft.VisualBasic.ChrW(10)&"and Dave Peckham"
			'
			'btnCancel
			'
			Me.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand
			Me.btnCancel.Enabled = false
			Me.btnCancel.Location = New System.Drawing.Point(0, 423)
			Me.btnCancel.Name = "btnCancel"
			Me.btnCancel.Size = New System.Drawing.Size(72, 31)
			Me.btnCancel.TabIndex = 12
			Me.btnCancel.Text = "Cancel"
			AddHandler Me.btnCancel.Click, AddressOf Me.BtnCancelClick
			'
			'progRunning
			'
			Me.progRunning.Location = New System.Drawing.Point(0, 407)
			Me.progRunning.Name = "progRunning"
			Me.progRunning.Size = New System.Drawing.Size(681, 10)
			Me.progRunning.Style = System.Windows.Forms.ProgressBarStyle.Marquee
			Me.progRunning.TabIndex = 16
			Me.progRunning.Visible = false
			'
			'statusStrip1
			'
			Me.statusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.StatusLabel1, Me.ProgressBar1})
			Me.statusStrip1.Location = New System.Drawing.Point(0, 490)
			Me.statusStrip1.Name = "statusStrip1"
			Me.statusStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional
			Me.statusStrip1.Size = New System.Drawing.Size(680, 22)
			Me.statusStrip1.SizingGrip = false
			Me.statusStrip1.TabIndex = 17
			Me.statusStrip1.Text = "statusStrip1"
			'
			'StatusLabel1
			'
			Me.StatusLabel1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
			Me.StatusLabel1.Name = "StatusLabel1"
			Me.StatusLabel1.Size = New System.Drawing.Size(162, 17)
			Me.StatusLabel1.Text = "Awaiting Source and Destination"
			'
			'ProgressBar1
			'
			Me.ProgressBar1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right
			Me.ProgressBar1.Name = "ProgressBar1"
			Me.ProgressBar1.Size = New System.Drawing.Size(500, 16)
			Me.ProgressBar1.Visible = false
			'
			'panel1
			'
			Me.panel1.BackColor = System.Drawing.SystemColors.Info
			Me.panel1.Controls.Add(Me.lblImageSize)
			Me.panel1.Controls.Add(Me.btnUpdateSize)
			Me.panel1.Controls.Add(Me.label8)
			Me.panel1.Controls.Add(Me.label7)
			Me.panel1.Controls.Add(Me.label6)
			Me.panel1.Controls.Add(Me.progSize)
			Me.panel1.Location = New System.Drawing.Point(0, 457)
			Me.panel1.Name = "panel1"
			Me.panel1.Size = New System.Drawing.Size(680, 30)
			Me.panel1.TabIndex = 18
			'
			'lblImageSize
			'
			Me.lblImageSize.ForeColor = System.Drawing.Color.Red
			Me.lblImageSize.Location = New System.Drawing.Point(129, 0)
			Me.lblImageSize.Name = "lblImageSize"
			Me.lblImageSize.Size = New System.Drawing.Size(128, 16)
			Me.lblImageSize.TabIndex = 19
			Me.lblImageSize.TextAlign = System.Drawing.ContentAlignment.TopCenter
			'
			'btnUpdateSize
			'
			Me.btnUpdateSize.Cursor = System.Windows.Forms.Cursors.Hand
			Me.btnUpdateSize.Enabled = false
			Me.btnUpdateSize.Location = New System.Drawing.Point(648, 6)
			Me.btnUpdateSize.Name = "btnUpdateSize"
			Me.btnUpdateSize.Pulse = true
			Me.btnUpdateSize.Size = New System.Drawing.Size(34, 24)
			Me.btnUpdateSize.SizeToLabel = false
			Me.btnUpdateSize.TabIndex = 18
			Me.btnUpdateSize.Text = "..."
			'
			'label8
			'
			Me.label8.Location = New System.Drawing.Point(314, -1)
			Me.label8.Name = "label8"
			Me.label8.Size = New System.Drawing.Size(46, 17)
			Me.label8.TabIndex = 17
			Me.label8.Text = "4.5GB"
			'
			'label7
			'
			Me.label7.Location = New System.Drawing.Point(604, 0)
			Me.label7.Name = "label7"
			Me.label7.Size = New System.Drawing.Size(38, 15)
			Me.label7.TabIndex = 16
			Me.label7.Text = "8.5GB"
			Me.label7.TextAlign = System.Drawing.ContentAlignment.TopRight
			'
			'label6
			'
			Me.label6.Location = New System.Drawing.Point(6, 0)
			Me.label6.Name = "label6"
			Me.label6.Size = New System.Drawing.Size(14, 15)
			Me.label6.TabIndex = 15
			Me.label6.Text = "0"
			'
			'progSize
			'
			Me.progSize.Location = New System.Drawing.Point(9, 18)
			Me.progSize.Maximum = 8789
			Me.progSize.Name = "progSize"
			Me.progSize.Size = New System.Drawing.Size(633, 11)
			Me.progSize.Step = 1
			Me.progSize.Style = System.Windows.Forms.ProgressBarStyle.Continuous
			Me.progSize.TabIndex = 14
			'
			'saveFileDialog1
			'
			Me.saveFileDialog1.DefaultExt = "iso"
			Me.saveFileDialog1.Filter = "ISO files|*.iso"
			'
			'btnClose
			'
			Me.btnClose.Cursor = System.Windows.Forms.Cursors.Hand
			Me.btnClose.Enabled = false
			Me.btnClose.Location = New System.Drawing.Point(78, 423)
			Me.btnClose.Name = "btnClose"
			Me.btnClose.Size = New System.Drawing.Size(66, 31)
			Me.btnClose.TabIndex = 19
			Me.btnClose.Text = "Close"
			AddHandler Me.btnClose.Click, AddressOf Me.BtnCloseClick
			'
			'ProcessDialog
			'
			Me.AutoScaleDimensions = New System.Drawing.SizeF(6!, 13!)
			Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
			Me.ClientSize = New System.Drawing.Size(680, 512)
			Me.ControlBox = false
			Me.Controls.Add(Me.btnClose)
			Me.Controls.Add(Me.panel1)
			Me.Controls.Add(Me.statusStrip1)
			Me.Controls.Add(Me.progRunning)
			Me.Controls.Add(Me.txtOutput)
			Me.Controls.Add(Me.btnCancel)
			Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
			Me.MaximizeBox = false
			Me.MinimizeBox = false
			Me.Name = "ProcessDialog"
			Me.ShowInTaskbar = false
			Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
			Me.Text = "Status"
			AddHandler Load, AddressOf Me.ProcessDialogLoad
			Me.statusStrip1.ResumeLayout(false)
			Me.statusStrip1.PerformLayout
			Me.panel1.ResumeLayout(false)
			Me.ResumeLayout(false)
			Me.PerformLayout
		End Sub
		Private btnClose As Wildgrape.Aqua.Controls.Button
		Private saveFileDialog1 As System.Windows.Forms.SaveFileDialog
		Private progSize As System.Windows.Forms.ProgressBar
		Private label6 As System.Windows.Forms.Label
		Private label7 As System.Windows.Forms.Label
		Private label8 As System.Windows.Forms.Label
		Private btnUpdateSize As Wildgrape.Aqua.Controls.Button
		Private lblImageSize As System.Windows.Forms.Label
		Private panel1 As System.Windows.Forms.Panel
		Private ProgressBar1 As System.Windows.Forms.ToolStripProgressBar
		Private StatusLabel1 As System.Windows.Forms.ToolStripStatusLabel
		Private statusStrip1 As System.Windows.Forms.StatusStrip
		Private progRunning As System.Windows.Forms.ProgressBar
		Private btnCancel As Wildgrape.Aqua.Controls.Button
		Private txtOutput As System.Windows.Forms.TextBox
	End Class
End Namespace
