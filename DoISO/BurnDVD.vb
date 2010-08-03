Imports Proc
Imports System.IO

Namespace DoISO
    Partial Class ProcessDialog
        Public Sub StartDVD()
			' ensure a disc is at the ready
			Dim volInfo As DriveInfo = New DriveInfo(m_DriveTarget)

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

            txtOutput.Text = "Burning DVD, Please Wait..." & vbcrlf

            progRunning.Visible = True
            StatusLabel1.Text = "Starting DVD Burn..."

            btnCancel.Enabled = True

            toRun = "dvdburn"

			DisableUI()

            '            MessageBox.Show(DVDBuildArgs())

            processCaller = New ProcessCaller(Me)
            processCaller.FileName = "" + toRun
            processCaller.WorkingDirectory = utl.cwd() '"."
            processCaller.Arguments = DVDBuildArgs()
            AddHandler processCaller.StdErrReceived, AddressOf DVDwriteStreamInfo
            AddHandler processCaller.StdOutReceived, AddressOf DVDwriteStreamInfo
            AddHandler processCaller.Completed, AddressOf DVDprocessCompletedOrCanceled
            AddHandler processCaller.Cancelled, AddressOf DVDprocessCompletedOrCanceled

            ' the following function starts a process and returns immediately,
            ' thus allowing the form to stay responsive.
            processCaller.Start()
        End Sub

        Private Sub DVDwriteStreamInfo(ByVal sender As Object, ByVal e As DataReceivedEventArgs)
            m_LastMessage = e.Text
            
            If Not Instr(e.Text, "% done") > 0 Then
            	Me.txtOutput.AppendText(e.Text & vbCrLf)
            End If
        End Sub

        ''' <summary>
        ''' Builds the arguments to cdburn based upon selected options.
        ''' </summary>
        ''' <returns>Commandline arguments for mkisofs.</returns>
        Private Function DVDBuildArgs() As String
            ' outargs is created as an expression
            Return Replace(m_DriveTarget, "\", "") & " " & Chr(34) & m_dest & Chr(34) & m_dataset.Tables("dvdburn").Rows(0)("outArgs")
        End Function

        Private Sub DVDprocessCompletedOrCanceled(ByVal sender As Object, ByVal e As EventArgs)
            If Not InStr(m_LastMessage, "Burn successful") > 0 Then
                StatusLabel1.Text = m_LastMessage
                StatusLabel1.ForeColor = Color.Yellow
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
