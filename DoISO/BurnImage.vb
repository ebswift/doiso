Imports Proc
Imports System.Globalization 

NameSpace DoISO
	Partial Class MainForm
        ''' <summary>
        ''' Select an ISO image to burn to CD or DVD.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Sub BtnImageToBurnClick(ByVal sender As Object, ByVal e As System.EventArgs)
            openISODialog.ShowDialog()
            If openISODialog.FileName = "" Then
                Exit Sub
            End If

            lblBurnSource.Text = openISODialog.FileName
            lblDest.Text = openISODialog.FileName
        End Sub
        
        ''' <summary>
        ''' Start the burning process.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Sub BtnStartBurnClick(sender As Object, e As System.EventArgs)
			Dim pd As ProcessDialog = New ProcessDialog
			
			pd.m_dataset = Me.dataSet1
			pd.m_GetSize = False
			pd.m_task = "BURN"
			pd.m_dest = lblBurnSource.Text
			pd.m_DriveTarget = cboDriveTarget.SelectedItem
			pd.ShowDialog()
'            If radDVD.Checked Then
'				DisableUI()
'                StartDVD()
'            Else 'default to CD
'				DisableUI()
'                StartCD()
'            End If
        End Sub

		''' <summary>
		''' Enables dropping on the input file label providing the object being dropped is a file.
		''' </summary>
		''' <param name="sender"></param>
		''' <param name="e"></param>
        Sub LblBurnSourceDragEnter(sender As Object, e As System.Windows.Forms.DragEventArgs)
            MyBase.OnDragEnter(e)

            If e.Data.GetDataPresent(DataFormats.FileDrop) Then
                e.Effect = DragDropEffects.Copy
            End If
        End Sub
        
		''' <summary>
		''' Takes a dropped file and populates the input file label.
		''' </summary>
		''' <param name="sender"></param>
		''' <param name="e"></param>
        Sub LblBurnSourceDragDrop(sender As Object, e As System.Windows.Forms.DragEventArgs)
            MyBase.OnDragDrop(e)
            Dim data As String() = e.Data.GetData(DataFormats.FileDrop)
            Dim s As String

            For Each s In data
                lblBurnSource.Text = s
                lblDest.Text = s
                lblDest.Refresh()
                folderBrowserDialog1.SelectedPath = s
                'AllowBurn()
            	Exit For
            Next

            Me.Refresh()
        End Sub
	End Class
End Namespace
