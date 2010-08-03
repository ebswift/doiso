'
' Created by SharpDevelop.
' User: ${USER}
' Date: ${DATE}
' Time: ${TIME}
' 
' To change this template use Tools | Options | Coding | Edit Standard Headers.
'
NameSpace DoISO
	Partial Class WaitForMedia
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
			Me.btnCancel = New Wildgrape.Aqua.Controls.Button
			Me.btnAccept = New System.Windows.Forms.Button
			Me.SuspendLayout
			'
			'btnCancel
			'
			Me.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand
			Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
			Me.btnCancel.Location = New System.Drawing.Point(199, 42)
			Me.btnCancel.Name = "btnCancel"
			Me.btnCancel.Size = New System.Drawing.Size(72, 31)
			Me.btnCancel.TabIndex = 0
			Me.btnCancel.Text = "Cancel"
			'
			'btnAccept
			'
			Me.btnAccept.DialogResult = System.Windows.Forms.DialogResult.OK
			Me.btnAccept.Location = New System.Drawing.Point(23, 42)
			Me.btnAccept.Name = "btnAccept"
			Me.btnAccept.Size = New System.Drawing.Size(114, 23)
			Me.btnAccept.TabIndex = 1
			Me.btnAccept.Text = "Accept (Invisible)"
			Me.btnAccept.UseVisualStyleBackColor = true
			Me.btnAccept.Visible = false
			'
			'WaitForMedia
			'
			Me.AcceptButton = Me.btnAccept
			Me.AutoScaleDimensions = New System.Drawing.SizeF(6!, 13!)
			Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
			Me.CancelButton = Me.btnCancel
			Me.ClientSize = New System.Drawing.Size(455, 123)
			Me.ControlBox = false
			Me.Controls.Add(Me.btnAccept)
			Me.Controls.Add(Me.btnCancel)
			Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
			Me.Name = "WaitForMedia"
			Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
			Me.Text = "Please insert a blank or rewriteable disc"
			AddHandler FormClosing, AddressOf Me.WaitForMediaFormClosing
			Me.ResumeLayout(false)
		End Sub
		Private btnAccept As System.Windows.Forms.Button
		Private btnCancel As Wildgrape.Aqua.Controls.Button
	End Class
End Namespace
