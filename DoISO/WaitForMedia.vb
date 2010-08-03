'
' Created by SharpDevelop.
' User: simpsont
' Date: 19/05/2006
' Time: 10:15 AM
' 
' To change this template use Tools | Options | Coding | Edit Standard Headers.
'

Imports System.IO

NameSpace DoISO
	Public Partial Class WaitForMedia
		Public m_DriveTarget As String
		Public m_OK As Boolean = False
		
		Public Sub New()
			' The Me.InitializeComponent call is required for Windows Forms designer support.
			Me.InitializeComponent()
			
			'
			' TODO : Add constructor code after InitializeComponents
			'
		End Sub
	
	        ''' <summary>
	        ''' Window subclassing - J Parsons http://dotnet.sys-con.com/read/39039_f.htm.  Detect
	        ''' CD insertion/removal.
	        ''' </summary>
	        ''' <param name="m"></param>
			Protected Overrides Sub WndProc(ByRef m As System.Windows.Forms.Message) 'Handle the CD messages 
			    Select Case m.Msg 
			        Case WM_DEVICECHANGE 
			            If (m.WParam.ToInt32() = DBT_DEVICEREMOVECOMPLETE) Then 
			                 'MessageBox.Show("CD Removal Complete!") ' cd removal detection
			            ElseIf (m.WParam.ToInt32() = DBT_DEVICEARRIVAL) Then 
                    	 	m_OK = True
                    	 	
                    	 	Me.Close
	                    End If
			            
			        Case Else 'Call base WndProc for default handling 
			            MyBase.WndProc(m) 
			    End Select 
			End Sub
		
		
		Sub WaitForMediaFormClosing(sender As Object, e As System.Windows.Forms.FormClosingEventArgs)
			If m_OK Then
				Me.DialogResult = System.Windows.Forms.DialogResult.OK
			End If
		End Sub
	End Class
End Namespace
