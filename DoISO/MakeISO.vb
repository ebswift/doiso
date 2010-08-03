Imports Proc

Namespace DoISO
    Partial Class ProcessDialog
		Private Sub RunMkISO()
           	Dim utl As Util = New Util
           	
           	StatusLabel1.ForeColor = Color.Black

            If Not m_GetSize Then
                ' request the filename to save the iso to
                saveFileDialog1.FileName = ""
                saveFileDialog1.ShowDialog()

                ' user cancelled
                If saveFileDialog1.FileName = "" Then
                	Me.Close
                    Exit Sub
                End If

                m_dest = saveFileDialog1.FileName
                m_dataset.Tables("mkisofs").Rows(0)("Dest") = m_dest
            Else
                m_dest = "tmp.iso"
            End If

            Dim toRun As String

            progRunning.Visible = True
            StatusLabel1.Text = "Starting ISO build..."

            btnCancel.Enabled = True

            toRun = "mkisofs"

            DisableUI()

            processCaller = New ProcessCaller(Me)
            processCaller.FileName = "" + toRun
            processCaller.WorkingDirectory = utl.cwd()
            processCaller.Arguments = BuildArgs()
            AddHandler processCaller.StdErrReceived, AddressOf writeStreamInfo
            AddHandler processCaller.StdOutReceived, AddressOf writeStreamInfo
            AddHandler processCaller.Completed, AddressOf processCompletedOrCanceled
            AddHandler processCaller.Cancelled, AddressOf processCompletedOrCanceled

            If Not m_GetSize Then
                Me.txtOutput.Text = "ISO Creation Started.  Please stand by.." & Environment.NewLine
            Else
                Me.txtOutput.Text = "Retrieving ISO Size.  Please stand by.." & Environment.NewLine
            End If

            ' the following function starts a process and returns immediately,
            ' thus allowing the form to stay responsive.
            processCaller.Start()
        End Sub

        ''' <summary>
        ''' Handles the events of StdOutReceived.
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        Private Sub writeStreamInfo(ByVal sender As Object, ByVal e As DataReceivedEventArgs)
            If sender.cancelledFlag Or sender.failedFlag Then
                Exit Sub
            End If

            m_LastMessage = e.Text

            If InStr(e.Text, "% done") <> 0 Then
                If Not ProgressBar1.Visible Then
                    ProgressBar1.Visible = True
                End If

                ProgressBar1.Value = CType(Mid(e.Text, 1, InStr(e.Text, "%") - 1), Integer)

                Dim tmpstr As String

                tmpstr = Mid(e.Text, InStr(e.Text, "%") + Len("done, ") + 1)

                tmpstr = LTrim(tmpstr.Replace("estimate finish ", ""))
                tmpstr = Mid(tmpstr, InStr(tmpstr, " ") + 1)
                Dim tmpdate As DateTime = GetDate(tmpstr)

                tmpstr = DateDiff(DateInterval.Minute, Now(), tmpdate) + 1
                StatusLabel1.Text = tmpstr & " Minutes Remaining"
            Else
                If Not InStr(e.Text, "Scanning") > 0 Then
                    Me.txtOutput.AppendText(e.Text + Environment.NewLine)
                End If

                StatusLabel1.Text = e.Text
                
                If InStr(e.Text, "Writing:   The File(s)") > 0 Then
                	Me.txtOutput.AppendText(vbcrlf & "Writing ISO, Please Wait..." & vbcrlf)
                	StatusLabel1.Text = "Writing ISO, Please Wait..."
                End If
            End If

'            If m_GetSize Then
'                ' mkisofs is only getting the size, not writing an ISO, so process output differently
'                If InStr(e.Text, "Total extents scheduled to be written = ") Then
'                    Dim tmpSize As Double
'
'                    tmpSize = Mid(e.Text, Len("Total extents scheduled to be written = "))
'                    tmpSize = 2048 * tmpSize / 1048576
'                    tmpSize = Math.Round(tmpSize, 2)
'                    If tmpSize > progSize.Maximum Then
'                        progSize.Value = progSize.Maximum
'                    Else
'                        progSize.Value = tmpSize
'                    End If
'
'                    lblImageSize.Text = tmpSize & "MB"
'                End If
'            Else
                If InStr(e.Text, "extents written") Then
                    ' an ISO has completed writing so display the size in the size graph
                    Dim tmpSize As Double

                    tmpSize = Mid(e.Text, 1, InStr(e.Text, " ") - 1)
                    tmpSize = 2048 * tmpSize / 1048576
                    tmpSize = Math.Round(tmpSize, 2)
                    If tmpSize > progSize.Maximum Then
                        progSize.Value = progSize.Maximum
                    Else
                        progSize.Value = tmpSize
                    End If

                    progSize.Value = tmpSize
                    lblImageSize.Text = tmpSize & "MB"
                End If
'            End If
        End Sub

        ''' <summary>
        ''' Handles the events of processCompleted & processCanceled.
        ''' </summary>
        Private Sub processCompletedOrCanceled(ByVal sender As Object, ByVal e As EventArgs)
        	progressbar1.Visible = False
            If InStr(m_LastMessage, "mkisofs:") Then
                ' the existence of 'mkisofs:' indicates an error message from mkisofs
                StatusLabel1.Text = "An Error Occurred - Please Review the Output Pane"
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

            ' Burn the ISO upon completion
            If m_dataset.Tables("DoISO").Rows(0)("BurnOnComplete") And Not m_GetSize And sender.completeFlag Then
	            If m_dataset.Tables("DoISO").Rows(0)("BurnDVD") Then
                    StartDVD()
                Else 'default to CD
                    StartCD()
                End If
            Else
	        	EnableUI()
            End If

        	processCaller = Nothing
        End Sub

        ''' <summary>
        ''' Builds a DateTime based upon the text reported by mkisofs while processing.
        ''' </summary>
        ''' <param name="inDate">Date in mkisofs reported format.</param>
        ''' <returns>Interpreted mkisofs date in DateTime format.</returns>
        Private Function GetDate(ByVal inDate As String) As DateTime
            Dim tmparr As Array = Split(inDate, " ")

            Return DateTime.Parse(tmparr(0).ToString & " " & tmparr(1) & " " & tmparr(3) & " " & tmparr(2))
        End Function
	End Class
End Namespace
