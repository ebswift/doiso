Imports Proc
Imports System.Globalization 
Imports System.IO

NameSpace DoISO
	Partial Class ProcessDialog
		Public Sub StartCD
			' ensure a disc is at the ready
'			Dim volInfo As DriveInfo = New DriveInfo(m_DriveTarget)

' following needs more research - it does not work with blank media
'			If Not volInfo.IsReady Then
'				mciSendString("open " & Replace(m_DriveTarget, "\", "") & " type CDAudio alias drivetarget", 0, 0, IntPtr.Zero)
'				mciSendString("set drivetarget door open", 0, 0, IntPtr.Zero)
'				Dim wfm As WaitForMedia = New WaitForMedia
'				wfm.m_DriveTarget = m_DriveTarget
'				If wfm.ShowDialog = DialogResult.Cancel Then
'					mciSendString("set drivetarget door closed", 0, 0, IntPtr.Zero)
'					Me.Close
'					Exit Sub
'				Else
'					While Not volInfo.IsReady
'						
'					End While
'				End If
'			End If

	        Dim toRun As String
           	Dim utl As Util = New Util

			txtOutput.Text = "Burning CD, Please Wait..." & vbcrlf
			
			progRunning.Visible = True
	        StatusLabel1.Text = "Starting CD Burn..."
	
	        btnCancel.Enabled = True
	
	        toRun = "cdburn"
	
			DisableUI()
			
	        processCaller = New ProcessCaller(Me)
	        processCaller.FileName = "" + toRun
	        processCaller.WorkingDirectory = utl.cwd() '"."
	        processCaller.Arguments = CDBuildArgs()
	        AddHandler processCaller.StdErrReceived, AddressOf CDwriteStreamInfo
	        AddHandler processCaller.StdOutReceived, AddressOf CDwriteStreamInfo
	        AddHandler processCaller.Completed, AddressOf CDprocessCompletedOrCanceled
	        AddHandler processCaller.Cancelled, AddressOf CDprocessCompletedOrCanceled
	
	        ' the following function starts a process and returns immediately,
	        ' thus allowing the form to stay responsive.
	        processCaller.Start()
		End Sub	

	    Private Sub CDwriteStreamInfo(ByVal sender As Object, ByVal e As DataReceivedEventArgs)
            m_LastMessage = e.Text
            
            ' we don't want to see % done because it shows all stages of completion at once for
            ' some reason.  We also don't want to see the actual data being passed
            Dim int32Val As Int32
            
            If (Not InStr(e.Text, "% done") > 0) And (Not Int32.TryParse(Mid(e.Text, 1, 4), NumberStyles.Integer, Nothing, int32Val)) And (Not InStr(e.Text, "Cue Sheet") > 0) Then
	            Me.txtOutput.AppendText(e.Text & vbcrlf)
	        End If
	    End Sub
	    
		''' <summary>
		''' Builds the arguments to cdburn based upon selected options.
		''' </summary>
		''' <returns>Commandline arguments for mkisofs.</returns>
		Private Function CDBuildArgs() As String
			' outargs is created as an expression
            Return Replace(m_DriveTarget, "\", "") + " " + m_dataset.Tables("cdburn").Rows(0)("outEraseFirst") + " " + Chr(34) + m_dest + Chr(34) + m_dataset.Tables("cdburn").Rows(0)("outArgs")
		End Function

		Sub BtnCancelBurnClick(sender As Object, e As System.EventArgs)
			BtnCancelClick(Nothing, Nothing)
		End Sub

	    Private Sub CDprocessCompletedOrCanceled(ByVal sender As Object, ByVal e As EventArgs)
            If Not InStr(m_LastMessage, "burn successful") > 0 Then
                ' the existence of 'mkisofs:' indicates an error message from mkisofs
                'StatusLabel1.Text = "An Error Occurred - Please Review the Output Pane"
                StatusLabel1.Text = m_LastMessage
                StatusLabel1.ForeColor = Color.Yellow
                
                If InStr(m_LastMessage, "Error verifying blank media") > 0 Then
                	StatusLabel1.Text += " - Need to select Erase First option?"
                	txtOutput.AppendText(vbcrlf & "Need to select Erase First option?")
                End If
            Else
                If sender.cancelledflag Then
                    StatusLabel1.Text = "User Cancelled"
                    txtOutput.Text = "User Cancelled"
                ElseIf sender.failedFlag Then
                    StatusLabel1.Text = "Operation Failed"
                ElseIf sender.completeFlag Then
                    StatusLabel1.Text = "Operation completed"
                End If
            End If

			EnableUI()
        	processCaller = Nothing
	    End Sub
	End Class
End Namespace
