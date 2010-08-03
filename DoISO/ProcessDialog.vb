'
' Created by SharpDevelop.
' User: simpsont
' Date: 15/05/2006
' Time: 1:41 PM
' 
' To change this template use Tools | Options | Coding | Edit Standard Headers.
'

Imports System.Data
Imports System.IO
Imports Proc
Imports System.Runtime.InteropServices

Namespace DoISO
	Public Partial Class ProcessDialog
		Public m_dataset As DataSet
		Public m_task As String
		Public m_dest As String
		Public m_GetSize As Boolean = False
		Public m_DriveTarget As String
		
		Private processCaller As ProcessCaller
        Private m_LastMessage As String ' last message from stdout/stderr - so errors can be identified
        Private m_PostUpdate As String 'TODO: use this variable to store the location of a method rather than its name. We use this to provide special postupdate info so our dataset acceptchanges doesn't override other changes
        Private m_LastOperation As String = "" ' last operation performed - this is important for deciding whether to continue on with a burn or not
		
'private field: 
Private setTrayStatus As Long 
'API 
Private Declare Function mciSendString Lib "winmm.dll" Alias "mciSendStringA" _ 
(ByVal lpstrCommand As String, ByVal lpstrReturnString As String, _ 
ByVal uReturnLength As Integer, ByVal hwndCallback As IntPtr) As Integer 

		Public Sub New()
			' The Me.InitializeComponent call is required for Windows Forms designer support.
			Me.InitializeComponent()
			
			'
			' TODO : Add constructor code after InitializeComponents
			'
		End Sub
		
		Sub BtnCancelClick(sender As Object, e As System.EventArgs)
            If (Not processCaller Is Nothing) Then
                processCaller.Cancel()
            End If
		End Sub

        ''' <summary>
        ''' Builds the arguments to mkisofs based upon selected options.
        ''' </summary>
        ''' <returns>Commandline arguments for mkisofs.</returns>
        Private Function BuildArgs() As String
            ' outargs is created as an expression
            'Stop
            'Return ""
            Return m_dataset.Tables("mkisofs").Rows(0)("outArgs")
        End Function
		
		''' <summary>
		''' Initiates the requested process according to the m_task property.
		''' </summary>
		''' <param name="sender"></param>
		''' <param name="e"></param>
		Sub ProcessDialogLoad(sender As Object, e As System.EventArgs)
            Select Case m_task
				Case "ISO"
					RunMkISO()
					
        		Case "BURN"
		            If m_dataset.Tables("DoISO").Rows(0)("BurnDVD") Then
						DisableUI()
		                StartDVD()
		            Else 'default to CD
						DisableUI()
		                StartCD()
		            End If
            End Select
		End Sub

		''' <summary>
		''' The UI is setup for processing - enable cancel button etc.
		''' </summary>
		Private Sub DisableUI()
            Me.ProgRunning.Visible = True
            Me.btnCancel.Enabled = True
            Me.btnClose.Enabled = False
		End Sub
		
		''' <summary>
		''' The UI is setup for an at-rest state - disable cancel button etc.
		''' </summary>
		Private Sub EnableUI()
            Me.ProgRunning.Visible = False
            Me.btnCancel.Enabled = False
            Me.btnClose.Enabled = True
            m_GetSize = False
		End Sub
		
		''' <summary>
		''' Close the processing dialog.
		''' </summary>
		''' <param name="sender"></param>
		''' <param name="e"></param>
		Sub BtnCloseClick(sender As Object, e As System.EventArgs)
			Me.Close
		End Sub
	End Class
End Namespace
