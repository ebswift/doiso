'Author: Troy Simpson - DoISO - ISO Creation Software
'Copyright (C) 2005  Troy Simpson
'For more information, see http://www.ebswift.com
'
'This program is free software; you can redistribute it and/or
'modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation; either version 2
'of the License, or (at your option) any later version.
'
'This program is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU General Public License for more details.
'
'You should have received a copy of the GNU General Public License
'along with this program; if not, write to the Free Software
'Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.


'NOTES:
'
'RadioButton Hack -
'At this stage my hack only works with 2 radiobuttons.  The databindings are setup as standard
'with one important difference.  One radiobutton has the expression 'Not otherradiobutton' where
'otherradiobutton is the name of the table column for the other radiobutton in the group of two.
'This fixes the bug where onpropertychange in the dataset eliminates the new RadioButton value
'causing both radiobuttons to be unchecked.

'PostUpdate Hack -
'Where two databound objects are modified in one operation, this caused a clash in the columnchanged
'event; the result of which was the loss of data from the dataset.
'This is rectified by staging the databound operations.  If, for example, the change of text in a text
'field prompts the changing (by code) of databound text in a label, the label changing code is
'separated out of the text field event into a new subroutine - the new subroutine is fired by
'code which looks for post processing events.
'Effectively in the above scenario, the text field is modified, it fully completes the
'columnchanged event saving its new data.  The columnchanged event then looks to the 
'm_PostUpdate variable to see if there are any pending events.  A call to the appropriate
'method is made via case statements.

Imports Proc
Imports System.IO
Imports System.Threading
Imports System.Security.Principal
Imports System.Management
Imports System.Xml

' Created by SharpDevelop.
' User: simpsont
' Date: 11/11/2005
' Time: 9:13 AM
' 
' To change this template use Tools | Options | Coding | Edit Standard Headers.
'

NameSpace DoISO
	Module Globals
			Public Const WM_DEVICECHANGE As Integer = 537 
			Public Const DBT_DEVICEREMOVECOMPLETE As Integer = 32772 ' for cd remove detection
			Public Const DBT_DEVICEARRIVAL As Integer = 32768 ' for cd insert detection
			Public Const DRIVE_CDROM As String = "5"
	End Module

	Public Class MainForm
		Inherits System.Windows.Forms.Form
	
		Private processCaller As ProcessCaller
		Private m_GetSize As Boolean = False
        Private m_LastMessage As String ' last message from stdout/stderr - so errors can be identified
        Private m_PostUpdate As String 'TODO: use this variable to store the location of a method rather than its name. We use this to provide special postupdate info so our dataset acceptchanges doesn't override other changes
        Private m_LastOperation As String = ""
        Friend WithEvents DataColumn41 As System.Data.DataColumn
        Friend WithEvents DataColumn42 As System.Data.DataColumn
        Friend WithEvents DataColumn43 As System.Data.DataColumn
        Friend WithEvents DataColumn44 As System.Data.DataColumn
        Friend WithEvents DataColumn46 As System.Data.DataColumn
        Friend WithEvents DataColumn45 As System.Data.DataColumn
        Friend WithEvents DataColumn47 As System.Data.DataColumn
        Friend WithEvents DataColumn48 As System.Data.DataColumn
        Friend WithEvents DataColumn49 As System.Data.DataColumn
        Friend WithEvents DataColumn50 As System.Data.DataColumn
        Friend WithEvents DataColumn51 As System.Data.DataColumn ' last operation performed - this is important for deciding whether to continue on with a burn or not

        Private args() As String

        <STAThread()> _
        Public Shared Sub Main(ByVal args As String())
            ' handle application-wide errors
            Try
                Application.Run(New MainForm)
            Catch ex As Exception
                MessageBox.Show(ex.Message)
            End Try
        End Sub

        Public Sub New()
            '
            ' The Me.InitializeComponent call is required for Windows Forms designer support.
            '
            Me.InitializeComponent()
            '
            ' TODO : Add constructor code after InitializeComponents
            '
            Dim MyID As WindowsIdentity = WindowsIdentity.GetCurrent()
            txtPreparer.Text = MyID.Name()

            args = Environment.GetCommandLineArgs()

            AddHandler Me.Closing, AddressOf Me_OnClosing
            'opisolevel.SelectedIndex = 3 ' default iso level to 4

            ' setup dataset1 defaults to hold commandline properties
            Dim t As System.Data.DataTable

            For Each t In dataSet1.DefaultViewManager.DataSet.Tables
                ' Setup the columnchanged event for all tables in dataset1
                AddHandler t.ColumnChanged, AddressOf dataset1_ColumnChanged
            Next

            t = Nothing

            ' Show version and copyright in txtoutput

            ' get version number
            Dim assemblyVer As System.Reflection.Assembly = System.Reflection.Assembly.GetExecutingAssembly()
            ' get copyright string
            Dim assemblyCpy As System.Reflection.AssemblyCopyrightAttribute = Attribute.GetCustomAttribute(System.Reflection.Assembly.GetExecutingAssembly(), GetType(System.Reflection.AssemblyCopyrightAttribute))

            txtOutput.Text = "DoISO " & assemblyVer.GetName().Version.ToString() & Environment.NewLine & Environment.NewLine & assemblyCpy.Copyright.ToString() & Environment.NewLine & txtOutput.Text
            lblCopyright.Text = assemblyCpy.Copyright.ToString()

            AddHandler btnSelectSource.Click, AddressOf BtnSelectSourceClick
            AddHandler btnCreateISO.Click, AddressOf btnCreateISO_Click
            AddHandler btnExit.Click, AddressOf BtnExitClick

            AddHandler btnImageToBurn.Click, AddressOf BtnImageToBurnClick
            AddHandler btnStartBurn.Click, AddressOf BtnStartBurnClick
            AddHandler lblBurnSource.DragEnter, AddressOf LblBurnSourceDragEnter
            AddHandler lblBurnSource.DragDrop, AddressOf LblBurnSourceDragDrop

            AddHandler cboDriveTarget.SelectedValueChanged, AddressOf cboDriveTargetSelectedValueChanged
        End Sub

#Region " Windows Forms Designer generated code "
        ' This method is required for Windows Forms designer support.
        ' Do not change the method contents inside the source code editor. The Forms designer might
        ' not be able to load this method if it was changed manually.
        Private Sub InitializeComponent()
            Me.components = New System.ComponentModel.Container
            Me.folderBrowserDialog1 = New System.Windows.Forms.FolderBrowserDialog
            Me.lblSource = New System.Windows.Forms.Label
            Me.dataSet1 = New System.Data.DataSet
            Me.tblmkisofs = New System.Data.DataTable
            Me.DataColumn41 = New System.Data.DataColumn
            Me.DataColumn46 = New System.Data.DataColumn
            Me.DataColumn45 = New System.Data.DataColumn
            Me.DataColumn47 = New System.Data.DataColumn
            Me.dataColumn1 = New System.Data.DataColumn
            Me.dataColumn2 = New System.Data.DataColumn
            Me.dataColumn3 = New System.Data.DataColumn
            Me.dataColumn4 = New System.Data.DataColumn
            Me.dataColumn5 = New System.Data.DataColumn
            Me.dataColumn6 = New System.Data.DataColumn
            Me.dataColumn7 = New System.Data.DataColumn
            Me.dataColumn8 = New System.Data.DataColumn
            Me.dataColumn9 = New System.Data.DataColumn
            Me.dataColumn10 = New System.Data.DataColumn
            Me.dataColumn11 = New System.Data.DataColumn
            Me.dataColumn12 = New System.Data.DataColumn
            Me.dataColumn13 = New System.Data.DataColumn
            Me.dataColumn14 = New System.Data.DataColumn
            Me.dataColumn15 = New System.Data.DataColumn
            Me.dataColumn16 = New System.Data.DataColumn
            Me.dataColumn17 = New System.Data.DataColumn
            Me.dataColumn18 = New System.Data.DataColumn
            Me.dataColumn19 = New System.Data.DataColumn
            Me.dataColumn20 = New System.Data.DataColumn
            Me.DataColumn48 = New System.Data.DataColumn
            Me.tblcdburn = New System.Data.DataTable
            Me.dataColumn22 = New System.Data.DataColumn
            Me.dataColumn23 = New System.Data.DataColumn
            Me.dataColumn24 = New System.Data.DataColumn
            Me.dataColumn25 = New System.Data.DataColumn
            Me.dataColumn26 = New System.Data.DataColumn
            Me.dataColumn27 = New System.Data.DataColumn
            Me.dataColumn28 = New System.Data.DataColumn
            Me.dataColumn29 = New System.Data.DataColumn
            Me.dataColumn30 = New System.Data.DataColumn
            Me.DataColumn42 = New System.Data.DataColumn
            Me.dataTable1 = New System.Data.DataTable
            Me.dataColumn31 = New System.Data.DataColumn
            Me.dataColumn32 = New System.Data.DataColumn
            Me.dataColumn33 = New System.Data.DataColumn
            Me.dataColumn34 = New System.Data.DataColumn
            Me.DataColumn43 = New System.Data.DataColumn
            Me.dataTable2 = New System.Data.DataTable
            Me.dataColumn37 = New System.Data.DataColumn
            Me.dataColumn38 = New System.Data.DataColumn
            Me.dataColumn39 = New System.Data.DataColumn
            Me.DataColumn44 = New System.Data.DataColumn
            Me.dataColumn21 = New System.Data.DataColumn
            Me.dataColumn35 = New System.Data.DataColumn
            Me.dataColumn36 = New System.Data.DataColumn
            Me.dataColumn40 = New System.Data.DataColumn
            Me.ctxWhatsThis = New System.Windows.Forms.ContextMenuStrip(Me.components)
            Me.whatsThisToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
            Me.saveFileDialog1 = New System.Windows.Forms.SaveFileDialog
            Me.lblCopyright = New System.Windows.Forms.Label
            Me.linkLabel1 = New System.Windows.Forms.LinkLabel
            Me.tabControl1 = New System.Windows.Forms.TabControl
            Me.tabCreateISO = New System.Windows.Forms.TabPage
            Me.lblDest = New System.Windows.Forms.Label
            Me.grpBurn = New System.Windows.Forms.GroupBox
            Me.pnlDVDBurnOptions = New System.Windows.Forms.Panel
            Me.chkEraseDVD = New System.Windows.Forms.CheckBox
            Me.pnlCDBurnOptions = New System.Windows.Forms.Panel
            Me.checkBox5 = New System.Windows.Forms.CheckBox
            Me.label11 = New System.Windows.Forms.Label
            Me.comboBox1 = New System.Windows.Forms.ComboBox
            Me.checkBox2 = New System.Windows.Forms.CheckBox
            Me.checkBox1 = New System.Windows.Forms.CheckBox
            Me.radCD = New System.Windows.Forms.RadioButton
            Me.radDVD = New System.Windows.Forms.RadioButton
            Me.chkBurnOnComplete = New System.Windows.Forms.CheckBox
            Me.txtVolumeLabel = New System.Windows.Forms.TextBox
            Me.label3 = New System.Windows.Forms.Label
            Me.btnSelectSource = New Wildgrape.Aqua.Controls.Button
            Me.opDVDFileSystem = New System.Windows.Forms.CheckBox
            Me.btnCreateISO = New Wildgrape.Aqua.Controls.Button
            Me.tabBurnISO = New System.Windows.Forms.TabPage
            Me.groupBox1 = New System.Windows.Forms.GroupBox
            Me.checkBox6 = New System.Windows.Forms.CheckBox
            Me.grpBurnCD = New System.Windows.Forms.GroupBox
            Me.chkBurnPostGAP = New System.Windows.Forms.CheckBox
            Me.label10 = New System.Windows.Forms.Label
            Me.chkBurnSpeed = New System.Windows.Forms.ComboBox
            Me.chkBurnSAO = New System.Windows.Forms.CheckBox
            Me.chkBurnEraseFirst = New System.Windows.Forms.CheckBox
            Me.btnImageToBurn = New Wildgrape.Aqua.Controls.Button
            Me.radCDBurn = New System.Windows.Forms.RadioButton
            Me.lblBurnSource = New System.Windows.Forms.Label
            Me.radDVDBurn = New System.Windows.Forms.RadioButton
            Me.btnStartBurn = New Wildgrape.Aqua.Controls.Button
            Me.tabISOOptions = New System.Windows.Forms.TabPage
            Me.chkDeepNesting = New System.Windows.Forms.CheckBox
            Me.txtPublisher = New System.Windows.Forms.TextBox
            Me.txtPreparer = New System.Windows.Forms.TextBox
            Me.label5 = New System.Windows.Forms.Label
            Me.label4 = New System.Windows.Forms.Label
            Me.txtCustomCommand = New System.Windows.Forms.TextBox
            Me.label1 = New System.Windows.Forms.Label
            Me.chkMoreVerbose = New System.Windows.Forms.CheckBox
            Me.opoptimiseDuplicates = New System.Windows.Forms.CheckBox
            Me.label2 = New System.Windows.Forms.Label
            Me.opisolevel = New System.Windows.Forms.ComboBox
            Me.txtOutput = New System.Windows.Forms.TextBox
            Me.statusStrip1 = New System.Windows.Forms.StatusStrip
            Me.StatusLabel1 = New System.Windows.Forms.ToolStripStatusLabel
            Me.ProgressBar1 = New System.Windows.Forms.ToolStripProgressBar
            Me.helpProvider1 = New System.Windows.Forms.HelpProvider
            Me.btnExit = New Wildgrape.Aqua.Controls.Button
            Me.panel2 = New System.Windows.Forms.Panel
            Me.checkBox3 = New System.Windows.Forms.CheckBox
            Me.checkBox4 = New System.Windows.Forms.CheckBox
            Me.radioButton1 = New System.Windows.Forms.RadioButton
            Me.radioButton2 = New System.Windows.Forms.RadioButton
            Me.openISODialog = New System.Windows.Forms.OpenFileDialog
            Me.label6 = New System.Windows.Forms.Label
            Me.cboDriveTarget = New System.Windows.Forms.ComboBox
            Me.DataColumn49 = New System.Data.DataColumn
            Me.DataColumn50 = New System.Data.DataColumn
            Me.DataColumn51 = New System.Data.DataColumn
            CType(Me.dataSet1, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.tblmkisofs, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.tblcdburn, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.dataTable1, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.dataTable2, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.ctxWhatsThis.SuspendLayout()
            Me.tabControl1.SuspendLayout()
            Me.tabCreateISO.SuspendLayout()
            Me.grpBurn.SuspendLayout()
            Me.pnlDVDBurnOptions.SuspendLayout()
            Me.pnlCDBurnOptions.SuspendLayout()
            Me.tabBurnISO.SuspendLayout()
            Me.groupBox1.SuspendLayout()
            Me.grpBurnCD.SuspendLayout()
            Me.tabISOOptions.SuspendLayout()
            Me.statusStrip1.SuspendLayout()
            Me.panel2.SuspendLayout()
            Me.SuspendLayout()
            '
            'lblSource
            '
            Me.lblSource.AllowDrop = True
            Me.lblSource.AutoEllipsis = True
            Me.lblSource.DataBindings.Add(New System.Windows.Forms.Binding("Text", Me.dataSet1, "mkisofs.Source", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.lblSource.Location = New System.Drawing.Point(32, 4)
            Me.lblSource.Name = "lblSource"
            Me.helpProvider1.SetShowHelp(Me.lblSource, True)
            Me.lblSource.Size = New System.Drawing.Size(301, 65)
            Me.lblSource.TabIndex = 0
            Me.lblSource.Text = "Select a folder to create an ISO from"
            Me.lblSource.UseCompatibleTextRendering = True
            '
            'dataSet1
            '
            Me.dataSet1.DataSetName = "dsmkisofs"
            Me.dataSet1.EnforceConstraints = False
            Me.dataSet1.Relations.AddRange(New System.Data.DataRelation() {New System.Data.DataRelation("cdburn_mkisofs", "cdburn", "mkisofs", New String() {"ID"}, New String() {"ID"}, False), New System.Data.DataRelation("dvdburn_mkisofs", "dvdburn", "mkisofs", New String() {"ID"}, New String() {"ID"}, False), New System.Data.DataRelation("mkisofs_doiso", "mkisofs", "DoISO", New String() {"ID"}, New String() {"ID"}, False)})
            Me.dataSet1.Tables.AddRange(New System.Data.DataTable() {Me.tblmkisofs, Me.tblcdburn, Me.dataTable1, Me.dataTable2})
            '
            'tblmkisofs
            '
            Me.tblmkisofs.Columns.AddRange(New System.Data.DataColumn() {Me.DataColumn41, Me.DataColumn46, Me.DataColumn45, Me.DataColumn47, Me.dataColumn1, Me.dataColumn2, Me.dataColumn3, Me.dataColumn4, Me.dataColumn5, Me.dataColumn6, Me.dataColumn7, Me.dataColumn8, Me.dataColumn9, Me.dataColumn10, Me.dataColumn11, Me.dataColumn12, Me.dataColumn13, Me.dataColumn14, Me.dataColumn15, Me.dataColumn16, Me.dataColumn17, Me.dataColumn18, Me.dataColumn19, Me.dataColumn20, Me.DataColumn48, Me.DataColumn49, Me.DataColumn50, Me.DataColumn51})
            Me.tblmkisofs.TableName = "mkisofs"
            '
            'DataColumn41
            '
            Me.DataColumn41.AutoIncrement = True
            Me.DataColumn41.ColumnName = "ID"
            Me.DataColumn41.DataType = GetType(Integer)
            '
            'DataColumn46
            '
            Me.DataColumn46.ColumnName = "BurnSource"
            Me.DataColumn46.DefaultValue = "Select an ISO image to burn"
            '
            'DataColumn45
            '
            Me.DataColumn45.ColumnName = "CanBurn"
            Me.DataColumn45.DataType = GetType(Boolean)
            Me.DataColumn45.DefaultValue = False
            Me.DataColumn45.Expression = "IIF(BurnSource='Select an ISO image to burn' Or DriveTarget='', False, True)"
            Me.DataColumn45.ReadOnly = True
            '
            'DataColumn47
            '
            Me.DataColumn47.ColumnName = "CanStart"
            Me.DataColumn47.DataType = GetType(Boolean)
            Me.DataColumn47.DefaultValue = False
            Me.DataColumn47.ReadOnly = True
            '
            'dataColumn1
            '
            Me.dataColumn1.AllowDBNull = False
            Me.dataColumn1.ColumnName = "DVDVideo"
            Me.dataColumn1.DataType = GetType(Boolean)
            Me.dataColumn1.DefaultValue = False
            Me.dataColumn1.ReadOnly = True
            '
            'dataColumn2
            '
            Me.dataColumn2.ColumnName = "outDVDVideo"
            Me.dataColumn2.DefaultValue = ""
            Me.dataColumn2.Expression = "IIF(DVDVideo, ' -dvd-video', '')"
            Me.dataColumn2.ReadOnly = True
            '
            'dataColumn3
            '
            Me.dataColumn3.ColumnName = "VolumeLabel"
            Me.dataColumn3.DefaultValue = ""
            '
            'dataColumn4
            '
            Me.dataColumn4.ColumnName = "outVolumeLabel"
            Me.dataColumn4.DefaultValue = ""
            Me.dataColumn4.Expression = "IIF(VolumeLabel='', '', ' -V ""' + VolumeLabel + '""')"
            Me.dataColumn4.ReadOnly = True
            '
            'dataColumn5
            '
            Me.dataColumn5.ColumnName = "Source"
            Me.dataColumn5.DefaultValue = "Select a folder to create an ISO from"
            '
            'dataColumn6
            '
            Me.dataColumn6.ColumnName = "outSource"
            Me.dataColumn6.DefaultValue = ""
            Me.dataColumn6.Expression = "IIF(Source='', '', IIF(Source Like '* *', ' ""' + Source + '""', ' ' + Source))"
            Me.dataColumn6.ReadOnly = True
            '
            'dataColumn7
            '
            Me.dataColumn7.ColumnName = "Dest"
            Me.dataColumn7.DefaultValue = ""
            '
            'dataColumn8
            '
            Me.dataColumn8.ColumnName = "outDest"
            Me.dataColumn8.DefaultValue = ""
            Me.dataColumn8.Expression = "IIF(Dest='', '', IIF(Dest Like '* *', ' -o ""' + Dest + '""', ' -o ' + Dest))"
            Me.dataColumn8.ReadOnly = True
            '
            'dataColumn9
            '
            Me.dataColumn9.AllowDBNull = False
            Me.dataColumn9.ColumnName = "ISOLevel"
            Me.dataColumn9.DefaultValue = "4"
            '
            'dataColumn10
            '
            Me.dataColumn10.ColumnName = "outISOLevel"
            Me.dataColumn10.DefaultValue = ""
            Me.dataColumn10.Expression = "IIF(ISOLevel='', '', ' -iso-level ' + ISOLevel)"
            Me.dataColumn10.ReadOnly = True
            '
            'dataColumn11
            '
            Me.dataColumn11.AllowDBNull = False
            Me.dataColumn11.ColumnName = "OptimiseDuplicates"
            Me.dataColumn11.DataType = GetType(Boolean)
            Me.dataColumn11.DefaultValue = True
            '
            'dataColumn12
            '
            Me.dataColumn12.ColumnName = "outOptimiseDuplicates"
            Me.dataColumn12.DefaultValue = ""
            Me.dataColumn12.Expression = "IIF(OptimiseDuplicates, ' -duplicates-once', '')"
            Me.dataColumn12.ReadOnly = True
            '
            'dataColumn13
            '
            Me.dataColumn13.ColumnName = "MoreVerbose"
            Me.dataColumn13.DataType = GetType(Boolean)
            Me.dataColumn13.DefaultValue = False
            '
            'dataColumn14
            '
            Me.dataColumn14.ColumnName = "outMoreVerbose"
            Me.dataColumn14.DefaultValue = ""
            Me.dataColumn14.Expression = "IIF(MoreVerbose, ' -v', '')"
            Me.dataColumn14.ReadOnly = True
            '
            'dataColumn15
            '
            Me.dataColumn15.ColumnName = "CustomParams"
            Me.dataColumn15.DefaultValue = ""
            '
            'dataColumn16
            '
            Me.dataColumn16.ColumnName = "outCustomParams"
            Me.dataColumn16.DefaultValue = ""
            Me.dataColumn16.Expression = "IIF(CustomParams='', '', ' ' + CustomParams)"
            Me.dataColumn16.ReadOnly = True
            '
            'dataColumn17
            '
            Me.dataColumn17.ColumnName = "Preparer"
            Me.dataColumn17.DefaultValue = ""
            '
            'dataColumn18
            '
            Me.dataColumn18.ColumnName = "outPreparer"
            Me.dataColumn18.DefaultValue = ""
            Me.dataColumn18.Expression = "IIF(Preparer='', '', ' -preparer ""' + Preparer +'""')"
            Me.dataColumn18.ReadOnly = True
            '
            'dataColumn19
            '
            Me.dataColumn19.ColumnName = "Publisher"
            Me.dataColumn19.DefaultValue = ""
            '
            'dataColumn20
            '
            Me.dataColumn20.ColumnName = "outPublisher"
            Me.dataColumn20.DefaultValue = ""
            Me.dataColumn20.Expression = "IIF(Publisher='', '', ' -publisher ""' + Publisher +'""')"
            Me.dataColumn20.ReadOnly = True
            '
            'DataColumn48
            '
            Me.DataColumn48.ColumnName = "DriveTarget"
            Me.DataColumn48.DefaultValue = ""
            '
            'tblcdburn
            '
            Me.tblcdburn.Columns.AddRange(New System.Data.DataColumn() {Me.dataColumn22, Me.dataColumn23, Me.dataColumn24, Me.dataColumn25, Me.dataColumn26, Me.dataColumn27, Me.dataColumn28, Me.dataColumn29, Me.dataColumn30, Me.DataColumn42})
            Me.tblcdburn.TableName = "cdburn"
            '
            'dataColumn22
            '
            Me.dataColumn22.AllowDBNull = False
            Me.dataColumn22.ColumnName = "EraseFirst"
            Me.dataColumn22.DataType = GetType(Boolean)
            Me.dataColumn22.DefaultValue = False
            '
            'dataColumn23
            '
            Me.dataColumn23.ColumnName = "outEraseFirst"
            Me.dataColumn23.DefaultValue = ""
            Me.dataColumn23.Expression = "IIF(EraseFirst, ' -erase', '')"
            Me.dataColumn23.ReadOnly = True
            '
            'dataColumn24
            '
            Me.dataColumn24.AllowDBNull = False
            Me.dataColumn24.ColumnName = "SAO"
            Me.dataColumn24.DataType = GetType(Boolean)
            Me.dataColumn24.DefaultValue = True
            '
            'dataColumn25
            '
            Me.dataColumn25.ColumnName = "outSAO"
            Me.dataColumn25.DefaultValue = ""
            Me.dataColumn25.Expression = "IIF(SAO, ' -sao', '')"
            Me.dataColumn25.ReadOnly = True
            '
            'dataColumn26
            '
            Me.dataColumn26.ColumnName = "Speed"
            Me.dataColumn26.DefaultValue = "max"
            '
            'dataColumn27
            '
            Me.dataColumn27.ColumnName = "outSpeed"
            Me.dataColumn27.DefaultValue = ""
            Me.dataColumn27.Expression = "' -speed ' + Speed"
            Me.dataColumn27.ReadOnly = True
            '
            'dataColumn28
            '
            Me.dataColumn28.AllowDBNull = False
            Me.dataColumn28.ColumnName = "PGap"
            Me.dataColumn28.DataType = GetType(Boolean)
            Me.dataColumn28.DefaultValue = True
            '
            'dataColumn29
            '
            Me.dataColumn29.ColumnName = "outPGap"
            Me.dataColumn29.DefaultValue = ""
            Me.dataColumn29.Expression = "IIF(PGap, ' -imagehaspostgap', '')"
            Me.dataColumn29.ReadOnly = True
            '
            'dataColumn30
            '
            Me.dataColumn30.ColumnName = "outArgs"
            Me.dataColumn30.DefaultValue = ""
            Me.dataColumn30.Expression = "outSAO + outSpeed + outPGap"
            Me.dataColumn30.ReadOnly = True
            '
            'DataColumn42
            '
            Me.DataColumn42.AutoIncrement = True
            Me.DataColumn42.ColumnName = "ID"
            Me.DataColumn42.DataType = GetType(Integer)
            '
            'dataTable1
            '
            Me.dataTable1.Columns.AddRange(New System.Data.DataColumn() {Me.dataColumn31, Me.dataColumn32, Me.dataColumn33, Me.dataColumn34, Me.DataColumn43})
            Me.dataTable1.TableName = "DoISO"
            '
            'dataColumn31
            '
            Me.dataColumn31.AllowDBNull = False
            Me.dataColumn31.ColumnName = "DVDVideo"
            Me.dataColumn31.DataType = GetType(Boolean)
            Me.dataColumn31.DefaultValue = False
            '
            'dataColumn32
            '
            Me.dataColumn32.AllowDBNull = False
            Me.dataColumn32.ColumnName = "BurnOnComplete"
            Me.dataColumn32.DataType = GetType(Boolean)
            Me.dataColumn32.DefaultValue = False
            '
            'dataColumn33
            '
            Me.dataColumn33.AllowDBNull = False
            Me.dataColumn33.ColumnName = "BurnCD"
            Me.dataColumn33.DataType = GetType(Boolean)
            Me.dataColumn33.DefaultValue = False
            Me.dataColumn33.Expression = "Not BurnDVD"
            Me.dataColumn33.ReadOnly = True
            '
            'dataColumn34
            '
            Me.dataColumn34.AllowDBNull = False
            Me.dataColumn34.ColumnName = "BurnDVD"
            Me.dataColumn34.DataType = GetType(Boolean)
            Me.dataColumn34.DefaultValue = True
            '
            'DataColumn43
            '
            Me.DataColumn43.AutoIncrement = True
            Me.DataColumn43.ColumnName = "ID"
            Me.DataColumn43.DataType = GetType(Integer)
            '
            'dataTable2
            '
            Me.dataTable2.Columns.AddRange(New System.Data.DataColumn() {Me.dataColumn37, Me.dataColumn38, Me.dataColumn39, Me.DataColumn44})
            Me.dataTable2.TableName = "dvdburn"
            '
            'dataColumn37
            '
            Me.dataColumn37.ColumnName = "EraseFirst"
            Me.dataColumn37.DataType = GetType(Boolean)
            Me.dataColumn37.DefaultValue = True
            '
            'dataColumn38
            '
            Me.dataColumn38.ColumnName = "outEraseFirst"
            Me.dataColumn38.Expression = "IIF(EraseFirst, ' /Erase', '')"
            Me.dataColumn38.ReadOnly = True
            '
            'dataColumn39
            '
            Me.dataColumn39.ColumnName = "outArgs"
            Me.dataColumn39.Expression = "outEraseFirst"
            Me.dataColumn39.ReadOnly = True
            '
            'DataColumn44
            '
            Me.DataColumn44.AutoIncrement = True
            Me.DataColumn44.ColumnName = "ID"
            Me.DataColumn44.DataType = GetType(Integer)
            '
            'dataColumn21
            '
            Me.dataColumn21.ColumnName = "outArgs"
            Me.dataColumn21.DefaultValue = ""
            Me.dataColumn21.Expression = "outMoreVerbose + ' -v -gui' + outCustomParams + outPreparer + outPublisher + outO" & _
                "ptimiseDuplicates + outRockRidgeExt + outDVDVideo + outVolumeLabel + outISOLevel" & _
                " + outDest + outSource"
            Me.dataColumn21.ReadOnly = True
            '
            'dataColumn35
            '
            Me.dataColumn35.AllowDBNull = False
            Me.dataColumn35.ColumnName = "RockRidgeExt"
            Me.dataColumn35.DataType = GetType(Boolean)
            Me.dataColumn35.DefaultValue = True
            '
            'dataColumn36
            '
            Me.dataColumn36.ColumnName = "outRockRidgeExt"
            Me.dataColumn36.Expression = "IIF(RockRidgeExt, ' -r', '')"
            Me.dataColumn36.ReadOnly = True
            '
            'dataColumn40
            '
            Me.dataColumn40.ColumnName = "DriveTarget"
            Me.dataColumn40.DefaultValue = ""
            '
            'ctxWhatsThis
            '
            Me.ctxWhatsThis.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.whatsThisToolStripMenuItem})
            Me.ctxWhatsThis.Name = "ctxWhatsThis"
            Me.helpProvider1.SetShowHelp(Me.ctxWhatsThis, False)
            Me.ctxWhatsThis.Size = New System.Drawing.Size(146, 26)
            Me.ctxWhatsThis.Text = "What's This?"
            '
            'whatsThisToolStripMenuItem
            '
            Me.whatsThisToolStripMenuItem.Name = "whatsThisToolStripMenuItem"
            Me.whatsThisToolStripMenuItem.Size = New System.Drawing.Size(145, 22)
            Me.whatsThisToolStripMenuItem.Text = "What's This?"
            '
            'saveFileDialog1
            '
            Me.saveFileDialog1.DefaultExt = "iso"
            Me.saveFileDialog1.Filter = "ISO files|*.iso"
            '
            'lblCopyright
            '
            Me.lblCopyright.Location = New System.Drawing.Point(317, 286)
            Me.lblCopyright.Name = "lblCopyright"
            Me.lblCopyright.Size = New System.Drawing.Size(198, 23)
            Me.lblCopyright.TabIndex = 7
            Me.lblCopyright.Text = "Copyright"
            Me.lblCopyright.UseCompatibleTextRendering = True
            '
            'linkLabel1
            '
            Me.helpProvider1.SetHelpString(Me.linkLabel1, "Visit the ebswift.com website")
            Me.linkLabel1.Location = New System.Drawing.Point(533, 286)
            Me.linkLabel1.Name = "linkLabel1"
            Me.helpProvider1.SetShowHelp(Me.linkLabel1, True)
            Me.linkLabel1.Size = New System.Drawing.Size(148, 23)
            Me.linkLabel1.TabIndex = 8
            Me.linkLabel1.TabStop = True
            Me.linkLabel1.Text = "http://www.ebswift.com"
            Me.linkLabel1.UseCompatibleTextRendering = True
            '
            'tabControl1
            '
            Me.tabControl1.Controls.Add(Me.tabCreateISO)
            Me.tabControl1.Controls.Add(Me.tabBurnISO)
            Me.tabControl1.Controls.Add(Me.tabISOOptions)
            Me.tabControl1.HotTrack = True
            Me.tabControl1.Location = New System.Drawing.Point(3, 3)
            Me.tabControl1.Name = "tabControl1"
            Me.tabControl1.SelectedIndex = 0
            Me.tabControl1.Size = New System.Drawing.Size(345, 280)
            Me.tabControl1.TabIndex = 0
            '
            'tabCreateISO
            '
            Me.tabCreateISO.BackColor = System.Drawing.SystemColors.ControlLightLight
            Me.tabCreateISO.Controls.Add(Me.lblDest)
            Me.tabCreateISO.Controls.Add(Me.grpBurn)
            Me.tabCreateISO.Controls.Add(Me.chkBurnOnComplete)
            Me.tabCreateISO.Controls.Add(Me.txtVolumeLabel)
            Me.tabCreateISO.Controls.Add(Me.label3)
            Me.tabCreateISO.Controls.Add(Me.btnSelectSource)
            Me.tabCreateISO.Controls.Add(Me.opDVDFileSystem)
            Me.tabCreateISO.Controls.Add(Me.lblSource)
            Me.tabCreateISO.Controls.Add(Me.btnCreateISO)
            Me.tabCreateISO.Location = New System.Drawing.Point(4, 22)
            Me.tabCreateISO.Name = "tabCreateISO"
            Me.tabCreateISO.Padding = New System.Windows.Forms.Padding(3)
            Me.tabCreateISO.Size = New System.Drawing.Size(337, 254)
            Me.tabCreateISO.TabIndex = 0
            Me.tabCreateISO.Text = "Create ISO"
            '
            'lblDest
            '
            Me.lblDest.BackColor = System.Drawing.SystemColors.ControlLightLight
            Me.lblDest.DataBindings.Add(New System.Windows.Forms.Binding("Text", Me.dataSet1, "mkisofs.Dest", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.lblDest.Location = New System.Drawing.Point(184, 244)
            Me.lblDest.Name = "lblDest"
            Me.lblDest.Size = New System.Drawing.Size(150, 10)
            Me.lblDest.TabIndex = 17
            Me.lblDest.Visible = False
            '
            'grpBurn
            '
            Me.grpBurn.BackColor = System.Drawing.SystemColors.ControlLightLight
            Me.grpBurn.Controls.Add(Me.pnlDVDBurnOptions)
            Me.grpBurn.Controls.Add(Me.pnlCDBurnOptions)
            Me.grpBurn.Controls.Add(Me.radCD)
            Me.grpBurn.Controls.Add(Me.radDVD)
            Me.grpBurn.DataBindings.Add(New System.Windows.Forms.Binding("Enabled", Me.dataSet1, "DoISO.BurnOnComplete", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.grpBurn.Enabled = False
            Me.grpBurn.Location = New System.Drawing.Point(125, 99)
            Me.grpBurn.Name = "grpBurn"
            Me.grpBurn.Size = New System.Drawing.Size(208, 149)
            Me.grpBurn.TabIndex = 16
            Me.grpBurn.TabStop = False
            Me.grpBurn.Text = "Burn"
            '
            'pnlDVDBurnOptions
            '
            Me.pnlDVDBurnOptions.Controls.Add(Me.chkEraseDVD)
            Me.pnlDVDBurnOptions.DataBindings.Add(New System.Windows.Forms.Binding("Enabled", Me.dataSet1, "DoISO.BurnDVD", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.pnlDVDBurnOptions.Location = New System.Drawing.Point(49, 20)
            Me.pnlDVDBurnOptions.Name = "pnlDVDBurnOptions"
            Me.pnlDVDBurnOptions.Size = New System.Drawing.Size(153, 34)
            Me.pnlDVDBurnOptions.TabIndex = 3
            '
            'chkEraseDVD
            '
            Me.chkEraseDVD.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Me.dataSet1, "dvdburn.EraseFirst", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.chkEraseDVD.Location = New System.Drawing.Point(0, 7)
            Me.chkEraseDVD.Name = "chkEraseDVD"
            Me.chkEraseDVD.Size = New System.Drawing.Size(150, 24)
            Me.chkEraseDVD.TabIndex = 0
            Me.chkEraseDVD.Text = "Erase First (DVD R/W)"
            Me.chkEraseDVD.UseVisualStyleBackColor = True
            '
            'pnlCDBurnOptions
            '
            Me.pnlCDBurnOptions.Controls.Add(Me.checkBox5)
            Me.pnlCDBurnOptions.Controls.Add(Me.label11)
            Me.pnlCDBurnOptions.Controls.Add(Me.comboBox1)
            Me.pnlCDBurnOptions.Controls.Add(Me.checkBox2)
            Me.pnlCDBurnOptions.Controls.Add(Me.checkBox1)
            Me.pnlCDBurnOptions.DataBindings.Add(New System.Windows.Forms.Binding("Enabled", Me.dataSet1, "DoISO.BurnCD", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.pnlCDBurnOptions.Enabled = False
            Me.pnlCDBurnOptions.Location = New System.Drawing.Point(49, 51)
            Me.pnlCDBurnOptions.Name = "pnlCDBurnOptions"
            Me.pnlCDBurnOptions.Size = New System.Drawing.Size(153, 94)
            Me.pnlCDBurnOptions.TabIndex = 2
            '
            'checkBox5
            '
            Me.checkBox5.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Me.dataSet1, "cdburn.PGap", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.checkBox5.Location = New System.Drawing.Point(0, 69)
            Me.checkBox5.Name = "checkBox5"
            Me.checkBox5.Size = New System.Drawing.Size(137, 24)
            Me.checkBox5.TabIndex = 5
            Me.checkBox5.Text = "Image Has Postgap"
            Me.checkBox5.UseVisualStyleBackColor = True
            '
            'label11
            '
            Me.label11.Location = New System.Drawing.Point(0, 48)
            Me.label11.Name = "label11"
            Me.label11.Size = New System.Drawing.Size(63, 23)
            Me.label11.TabIndex = 6
            Me.label11.Text = "Speed:"
            '
            'comboBox1
            '
            Me.comboBox1.DataBindings.Add(New System.Windows.Forms.Binding("SelectedItem", Me.dataSet1, "cdburn.Speed", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.comboBox1.FormattingEnabled = True
            Me.comboBox1.Items.AddRange(New Object() {"max", "32", "24", "20", "16", "10", "4"})
            Me.comboBox1.Location = New System.Drawing.Point(79, 48)
            Me.comboBox1.Name = "comboBox1"
            Me.comboBox1.Size = New System.Drawing.Size(71, 21)
            Me.comboBox1.TabIndex = 4
            '
            'checkBox2
            '
            Me.checkBox2.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Me.dataSet1, "cdburn.SAO", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.checkBox2.Location = New System.Drawing.Point(0, 21)
            Me.checkBox2.Name = "checkBox2"
            Me.checkBox2.Size = New System.Drawing.Size(140, 24)
            Me.checkBox2.TabIndex = 1
            Me.checkBox2.Text = "Session At Once"
            Me.checkBox2.UseVisualStyleBackColor = True
            '
            'checkBox1
            '
            Me.checkBox1.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Me.dataSet1, "cdburn.EraseFirst", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.checkBox1.Location = New System.Drawing.Point(0, 0)
            Me.checkBox1.Name = "checkBox1"
            Me.checkBox1.Size = New System.Drawing.Size(140, 24)
            Me.checkBox1.TabIndex = 0
            Me.checkBox1.Text = "Erase First (CD R/W)"
            Me.checkBox1.UseVisualStyleBackColor = True
            '
            'radCD
            '
            Me.radCD.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Me.dataSet1, "DoISO.BurnCD", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.radCD.Location = New System.Drawing.Point(3, 41)
            Me.radCD.Name = "radCD"
            Me.radCD.Size = New System.Drawing.Size(104, 24)
            Me.radCD.TabIndex = 1
            Me.radCD.Text = "CD"
            Me.radCD.UseVisualStyleBackColor = True
            '
            'radDVD
            '
            Me.radDVD.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Me.dataSet1, "DoISO.BurnDVD", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.radDVD.Location = New System.Drawing.Point(3, 20)
            Me.radDVD.Name = "radDVD"
            Me.radDVD.Size = New System.Drawing.Size(104, 24)
            Me.radDVD.TabIndex = 0
            Me.radDVD.Text = "DVD"
            Me.radDVD.UseVisualStyleBackColor = True
            '
            'chkBurnOnComplete
            '
            Me.chkBurnOnComplete.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Me.dataSet1, "DoISO.BurnOnComplete", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.chkBurnOnComplete.Location = New System.Drawing.Point(0, 129)
            Me.chkBurnOnComplete.Name = "chkBurnOnComplete"
            Me.chkBurnOnComplete.Size = New System.Drawing.Size(122, 24)
            Me.chkBurnOnComplete.TabIndex = 15
            Me.chkBurnOnComplete.Text = "Burn On Complete"
            Me.chkBurnOnComplete.UseVisualStyleBackColor = True
            '
            'txtVolumeLabel
            '
            Me.txtVolumeLabel.DataBindings.Add(New System.Windows.Forms.Binding("Text", Me.dataSet1, "mkisofs.VolumeLabel", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.helpProvider1.SetHelpString(Me.txtVolumeLabel, "Volume label of the resulting ISO")
            Me.txtVolumeLabel.Location = New System.Drawing.Point(86, 72)
            Me.txtVolumeLabel.Name = "txtVolumeLabel"
            Me.helpProvider1.SetShowHelp(Me.txtVolumeLabel, True)
            Me.txtVolumeLabel.Size = New System.Drawing.Size(247, 20)
            Me.txtVolumeLabel.TabIndex = 1
            '
            'label3
            '
            Me.label3.Location = New System.Drawing.Point(1, 75)
            Me.label3.Name = "label3"
            Me.label3.Size = New System.Drawing.Size(79, 23)
            Me.label3.TabIndex = 13
            Me.label3.Text = "Volume Label:"
            '
            'btnSelectSource
            '
            Me.btnSelectSource.Cursor = System.Windows.Forms.Cursors.Hand
            Me.helpProvider1.SetHelpString(Me.btnSelectSource, "Contents of the selected folder will be used to build the ISO")
            Me.btnSelectSource.Location = New System.Drawing.Point(0, 3)
            Me.btnSelectSource.Name = "btnSelectSource"
            Me.helpProvider1.SetShowHelp(Me.btnSelectSource, True)
            Me.btnSelectSource.Size = New System.Drawing.Size(34, 24)
            Me.btnSelectSource.SizeToLabel = False
            Me.btnSelectSource.TabIndex = 11
            Me.btnSelectSource.Text = "..."
            '
            'opDVDFileSystem
            '
            Me.opDVDFileSystem.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Me.dataSet1, "mkisofs.DVDVideo", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.helpProvider1.SetHelpString(Me.opDVDFileSystem, "Generate DVD-Video compliant UDF file system")
            Me.opDVDFileSystem.Location = New System.Drawing.Point(0, 99)
            Me.opDVDFileSystem.Name = "opDVDFileSystem"
            Me.helpProvider1.SetShowHelp(Me.opDVDFileSystem, True)
            Me.opDVDFileSystem.Size = New System.Drawing.Size(158, 24)
            Me.opDVDFileSystem.TabIndex = 2
            Me.opDVDFileSystem.Text = "DVD Video Filesystem"
            Me.opDVDFileSystem.UseCompatibleTextRendering = True
            Me.opDVDFileSystem.UseVisualStyleBackColor = True
            '
            'btnCreateISO
            '
            Me.btnCreateISO.Cursor = System.Windows.Forms.Cursors.Hand
            Me.btnCreateISO.DataBindings.Add(New System.Windows.Forms.Binding("Enabled", Me.dataSet1, "mkisofs.CanStart", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.btnCreateISO.Enabled = False
            Me.helpProvider1.SetHelpString(Me.btnCreateISO, "Creates the ISO using the given parameters")
            Me.btnCreateISO.Location = New System.Drawing.Point(0, 224)
            Me.btnCreateISO.Name = "btnCreateISO"
            Me.btnCreateISO.Pulse = True
            Me.helpProvider1.SetShowHelp(Me.btnCreateISO, True)
            Me.btnCreateISO.Size = New System.Drawing.Size(61, 30)
            Me.btnCreateISO.TabIndex = 9
            Me.btnCreateISO.Text = "Start"
            '
            'tabBurnISO
            '
            Me.tabBurnISO.BackColor = System.Drawing.SystemColors.ControlLightLight
            Me.tabBurnISO.Controls.Add(Me.groupBox1)
            Me.tabBurnISO.Controls.Add(Me.grpBurnCD)
            Me.tabBurnISO.Controls.Add(Me.btnImageToBurn)
            Me.tabBurnISO.Controls.Add(Me.radCDBurn)
            Me.tabBurnISO.Controls.Add(Me.lblBurnSource)
            Me.tabBurnISO.Controls.Add(Me.radDVDBurn)
            Me.tabBurnISO.Controls.Add(Me.btnStartBurn)
            Me.tabBurnISO.Location = New System.Drawing.Point(4, 22)
            Me.tabBurnISO.Name = "tabBurnISO"
            Me.tabBurnISO.Size = New System.Drawing.Size(337, 254)
            Me.tabBurnISO.TabIndex = 2
            Me.tabBurnISO.Text = "Burn Existing ISO Image"
            '
            'groupBox1
            '
            Me.groupBox1.Controls.Add(Me.checkBox6)
            Me.groupBox1.DataBindings.Add(New System.Windows.Forms.Binding("Enabled", Me.dataSet1, "DoISO.BurnDVD", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.groupBox1.Location = New System.Drawing.Point(117, 79)
            Me.groupBox1.Name = "groupBox1"
            Me.groupBox1.Size = New System.Drawing.Size(201, 46)
            Me.groupBox1.TabIndex = 20
            Me.groupBox1.TabStop = False
            Me.groupBox1.Text = "DVD Burning Options"
            '
            'checkBox6
            '
            Me.checkBox6.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Me.dataSet1, "dvdburn.EraseFirst", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.checkBox6.Location = New System.Drawing.Point(26, 16)
            Me.checkBox6.Name = "checkBox6"
            Me.checkBox6.Size = New System.Drawing.Size(150, 24)
            Me.checkBox6.TabIndex = 0
            Me.checkBox6.Text = "Erase First (DVD R/W)"
            Me.checkBox6.UseVisualStyleBackColor = True
            '
            'grpBurnCD
            '
            Me.grpBurnCD.Controls.Add(Me.chkBurnPostGAP)
            Me.grpBurnCD.Controls.Add(Me.label10)
            Me.grpBurnCD.Controls.Add(Me.chkBurnSpeed)
            Me.grpBurnCD.Controls.Add(Me.chkBurnSAO)
            Me.grpBurnCD.Controls.Add(Me.chkBurnEraseFirst)
            Me.grpBurnCD.DataBindings.Add(New System.Windows.Forms.Binding("Enabled", Me.dataSet1, "DoISO.BurnCD", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.grpBurnCD.Enabled = False
            Me.grpBurnCD.Location = New System.Drawing.Point(117, 131)
            Me.grpBurnCD.Name = "grpBurnCD"
            Me.grpBurnCD.Size = New System.Drawing.Size(201, 120)
            Me.grpBurnCD.TabIndex = 17
            Me.grpBurnCD.TabStop = False
            Me.grpBurnCD.Text = "CD Burning Options"
            '
            'chkBurnPostGAP
            '
            Me.chkBurnPostGAP.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Me.dataSet1, "cdburn.PGap", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.chkBurnPostGAP.Location = New System.Drawing.Point(25, 83)
            Me.chkBurnPostGAP.Name = "chkBurnPostGAP"
            Me.chkBurnPostGAP.Size = New System.Drawing.Size(137, 24)
            Me.chkBurnPostGAP.TabIndex = 10
            Me.chkBurnPostGAP.Text = "Image Has Postgap"
            Me.chkBurnPostGAP.UseVisualStyleBackColor = True
            '
            'label10
            '
            Me.label10.Location = New System.Drawing.Point(25, 62)
            Me.label10.Name = "label10"
            Me.label10.Size = New System.Drawing.Size(63, 23)
            Me.label10.TabIndex = 11
            Me.label10.Text = "Speed:"
            '
            'chkBurnSpeed
            '
            Me.chkBurnSpeed.DataBindings.Add(New System.Windows.Forms.Binding("SelectedItem", Me.dataSet1, "cdburn.Speed", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.chkBurnSpeed.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.chkBurnSpeed.FormattingEnabled = True
            Me.chkBurnSpeed.Items.AddRange(New Object() {"max", "32", "24", "20", "16", "10", "4"})
            Me.chkBurnSpeed.Location = New System.Drawing.Point(104, 62)
            Me.chkBurnSpeed.Name = "chkBurnSpeed"
            Me.chkBurnSpeed.Size = New System.Drawing.Size(71, 21)
            Me.chkBurnSpeed.TabIndex = 9
            '
            'chkBurnSAO
            '
            Me.chkBurnSAO.Checked = True
            Me.chkBurnSAO.CheckState = System.Windows.Forms.CheckState.Checked
            Me.chkBurnSAO.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Me.dataSet1, "cdburn.SAO", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.chkBurnSAO.Location = New System.Drawing.Point(25, 35)
            Me.chkBurnSAO.Name = "chkBurnSAO"
            Me.chkBurnSAO.Size = New System.Drawing.Size(140, 24)
            Me.chkBurnSAO.TabIndex = 8
            Me.chkBurnSAO.Text = "Session At Once"
            Me.chkBurnSAO.UseVisualStyleBackColor = True
            '
            'chkBurnEraseFirst
            '
            Me.chkBurnEraseFirst.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Me.dataSet1, "cdburn.EraseFirst", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.chkBurnEraseFirst.Location = New System.Drawing.Point(25, 14)
            Me.chkBurnEraseFirst.Name = "chkBurnEraseFirst"
            Me.chkBurnEraseFirst.Size = New System.Drawing.Size(140, 24)
            Me.chkBurnEraseFirst.TabIndex = 7
            Me.chkBurnEraseFirst.Text = "Erase First (CD/RW)"
            Me.chkBurnEraseFirst.UseVisualStyleBackColor = True
            '
            'btnImageToBurn
            '
            Me.btnImageToBurn.Cursor = System.Windows.Forms.Cursors.Hand
            Me.helpProvider1.SetHelpString(Me.btnImageToBurn, "Contents of the selected folder will be used to build the ISO")
            Me.btnImageToBurn.Location = New System.Drawing.Point(0, 3)
            Me.btnImageToBurn.Name = "btnImageToBurn"
            Me.helpProvider1.SetShowHelp(Me.btnImageToBurn, True)
            Me.btnImageToBurn.Size = New System.Drawing.Size(34, 24)
            Me.btnImageToBurn.SizeToLabel = False
            Me.btnImageToBurn.TabIndex = 19
            Me.btnImageToBurn.Text = "..."
            '
            'radCDBurn
            '
            Me.radCDBurn.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Me.dataSet1, "DoISO.BurnCD", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.radCDBurn.Location = New System.Drawing.Point(8, 101)
            Me.radCDBurn.Name = "radCDBurn"
            Me.radCDBurn.Size = New System.Drawing.Size(104, 24)
            Me.radCDBurn.TabIndex = 1
            Me.radCDBurn.Text = "CD"
            Me.radCDBurn.UseVisualStyleBackColor = True
            '
            'lblBurnSource
            '
            Me.lblBurnSource.AllowDrop = True
            Me.lblBurnSource.AutoEllipsis = True
            Me.lblBurnSource.DataBindings.Add(New System.Windows.Forms.Binding("Text", Me.dataSet1, "mkisofs.BurnSource", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.lblBurnSource.Location = New System.Drawing.Point(45, 4)
            Me.lblBurnSource.Name = "lblBurnSource"
            Me.helpProvider1.SetShowHelp(Me.lblBurnSource, True)
            Me.lblBurnSource.Size = New System.Drawing.Size(288, 71)
            Me.lblBurnSource.TabIndex = 18
            Me.lblBurnSource.Text = "Select an ISO image to burn"
            Me.lblBurnSource.UseCompatibleTextRendering = True
            '
            'radDVDBurn
            '
            Me.radDVDBurn.Checked = True
            Me.radDVDBurn.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Me.dataSet1, "DoISO.BurnDVD", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.radDVDBurn.Location = New System.Drawing.Point(8, 79)
            Me.radDVDBurn.Name = "radDVDBurn"
            Me.radDVDBurn.Size = New System.Drawing.Size(104, 24)
            Me.radDVDBurn.TabIndex = 0
            Me.radDVDBurn.TabStop = True
            Me.radDVDBurn.Text = "DVD"
            Me.radDVDBurn.UseVisualStyleBackColor = True
            '
            'btnStartBurn
            '
            Me.btnStartBurn.Cursor = System.Windows.Forms.Cursors.Hand
            Me.btnStartBurn.DataBindings.Add(New System.Windows.Forms.Binding("Enabled", Me.dataSet1, "mkisofs.CanBurn", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.btnStartBurn.Enabled = False
            Me.helpProvider1.SetHelpString(Me.btnStartBurn, "Creates the ISO using the given parameters")
            Me.btnStartBurn.Location = New System.Drawing.Point(0, 224)
            Me.btnStartBurn.Name = "btnStartBurn"
            Me.btnStartBurn.Pulse = True
            Me.helpProvider1.SetShowHelp(Me.btnStartBurn, True)
            Me.btnStartBurn.Size = New System.Drawing.Size(61, 30)
            Me.btnStartBurn.TabIndex = 11
            Me.btnStartBurn.Text = "Start"
            '
            'tabISOOptions
            '
            Me.tabISOOptions.BackColor = System.Drawing.SystemColors.ControlLightLight
            Me.tabISOOptions.Controls.Add(Me.chkDeepNesting)
            Me.tabISOOptions.Controls.Add(Me.txtPublisher)
            Me.tabISOOptions.Controls.Add(Me.txtPreparer)
            Me.tabISOOptions.Controls.Add(Me.label5)
            Me.tabISOOptions.Controls.Add(Me.label4)
            Me.tabISOOptions.Controls.Add(Me.txtCustomCommand)
            Me.tabISOOptions.Controls.Add(Me.label1)
            Me.tabISOOptions.Controls.Add(Me.chkMoreVerbose)
            Me.tabISOOptions.Controls.Add(Me.opoptimiseDuplicates)
            Me.tabISOOptions.Controls.Add(Me.label2)
            Me.tabISOOptions.Controls.Add(Me.opisolevel)
            Me.tabISOOptions.Location = New System.Drawing.Point(4, 22)
            Me.tabISOOptions.Name = "tabISOOptions"
            Me.tabISOOptions.Padding = New System.Windows.Forms.Padding(3)
            Me.tabISOOptions.Size = New System.Drawing.Size(337, 254)
            Me.tabISOOptions.TabIndex = 1
            Me.tabISOOptions.Text = "ISO Creation Options"
            '
            'chkDeepNesting
            '
            Me.chkDeepNesting.Checked = True
            Me.chkDeepNesting.CheckState = System.Windows.Forms.CheckState.Checked
            Me.chkDeepNesting.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Me.dataSet1, "mkisofs.RockRidgeExt", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.chkDeepNesting.Location = New System.Drawing.Point(9, 90)
            Me.chkDeepNesting.Name = "chkDeepNesting"
            Me.chkDeepNesting.Size = New System.Drawing.Size(227, 24)
            Me.chkDeepNesting.TabIndex = 10
            Me.chkDeepNesting.Text = "Rock Ridge Extended (-r)"
            Me.chkDeepNesting.UseVisualStyleBackColor = True
            '
            'txtPublisher
            '
            Me.txtPublisher.DataBindings.Add(New System.Windows.Forms.Binding("Text", Me.dataSet1, "mkisofs.Publisher", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.txtPublisher.Location = New System.Drawing.Point(185, 198)
            Me.txtPublisher.Name = "txtPublisher"
            Me.txtPublisher.Size = New System.Drawing.Size(146, 20)
            Me.txtPublisher.TabIndex = 9
            Me.txtPublisher.Text = "DoISO - http://www.ebswift.com"
            '
            'txtPreparer
            '
            Me.txtPreparer.DataBindings.Add(New System.Windows.Forms.Binding("Text", Me.dataSet1, "mkisofs.Preparer", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.txtPreparer.Location = New System.Drawing.Point(185, 171)
            Me.txtPreparer.Name = "txtPreparer"
            Me.txtPreparer.Size = New System.Drawing.Size(146, 20)
            Me.txtPreparer.TabIndex = 8
            '
            'label5
            '
            Me.label5.Location = New System.Drawing.Point(9, 201)
            Me.label5.Name = "label5"
            Me.label5.Size = New System.Drawing.Size(100, 23)
            Me.label5.TabIndex = 7
            Me.label5.Text = "Publisher:"
            '
            'label4
            '
            Me.label4.Location = New System.Drawing.Point(9, 174)
            Me.label4.Name = "label4"
            Me.label4.Size = New System.Drawing.Size(100, 23)
            Me.label4.TabIndex = 6
            Me.label4.Text = "Preparer:"
            '
            'txtCustomCommand
            '
            Me.txtCustomCommand.DataBindings.Add(New System.Windows.Forms.Binding("Text", Me.dataSet1, "mkisofs.CustomParams", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.helpProvider1.SetHelpString(Me.txtCustomCommand, "Enter arbitrary mkisofs commandline options here")
            Me.txtCustomCommand.Location = New System.Drawing.Point(185, 144)
            Me.txtCustomCommand.Name = "txtCustomCommand"
            Me.helpProvider1.SetShowHelp(Me.txtCustomCommand, True)
            Me.txtCustomCommand.Size = New System.Drawing.Size(146, 20)
            Me.txtCustomCommand.TabIndex = 5
            '
            'label1
            '
            Me.label1.Location = New System.Drawing.Point(9, 147)
            Me.label1.Name = "label1"
            Me.label1.Size = New System.Drawing.Size(187, 23)
            Me.label1.TabIndex = 4
            Me.label1.Text = "Custom Commandline Parameters:"
            '
            'chkMoreVerbose
            '
            Me.chkMoreVerbose.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Me.dataSet1, "mkisofs.MoreVerbose", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.helpProvider1.SetHelpString(Me.chkMoreVerbose, "Output extra debug information")
            Me.chkMoreVerbose.Location = New System.Drawing.Point(9, 60)
            Me.chkMoreVerbose.Name = "chkMoreVerbose"
            Me.helpProvider1.SetShowHelp(Me.chkMoreVerbose, True)
            Me.chkMoreVerbose.Size = New System.Drawing.Size(187, 24)
            Me.chkMoreVerbose.TabIndex = 3
            Me.chkMoreVerbose.Text = "Show More Verbose Output"
            Me.chkMoreVerbose.UseVisualStyleBackColor = True
            '
            'opoptimiseDuplicates
            '
            Me.opoptimiseDuplicates.Checked = True
            Me.opoptimiseDuplicates.CheckState = System.Windows.Forms.CheckState.Checked
            Me.opoptimiseDuplicates.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Me.dataSet1, "mkisofs.OptimiseDuplicates", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.helpProvider1.SetHelpString(Me.opoptimiseDuplicates, "Optimise storage by encoding duplicate files once")
            Me.opoptimiseDuplicates.Location = New System.Drawing.Point(9, 30)
            Me.opoptimiseDuplicates.Name = "opoptimiseDuplicates"
            Me.helpProvider1.SetShowHelp(Me.opoptimiseDuplicates, True)
            Me.opoptimiseDuplicates.Size = New System.Drawing.Size(204, 24)
            Me.opoptimiseDuplicates.TabIndex = 2
            Me.opoptimiseDuplicates.Text = "Optimise Duplicates"
            Me.opoptimiseDuplicates.UseCompatibleTextRendering = True
            Me.opoptimiseDuplicates.UseVisualStyleBackColor = True
            '
            'label2
            '
            Me.label2.Location = New System.Drawing.Point(9, 6)
            Me.label2.Name = "label2"
            Me.label2.Size = New System.Drawing.Size(77, 21)
            Me.label2.TabIndex = 1
            Me.label2.Text = "ISO Level:"
            Me.label2.UseCompatibleTextRendering = True
            '
            'opisolevel
            '
            Me.opisolevel.DataBindings.Add(New System.Windows.Forms.Binding("SelectedItem", Me.dataSet1, "mkisofs.ISOLevel", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.opisolevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.opisolevel.FlatStyle = System.Windows.Forms.FlatStyle.System
            Me.opisolevel.FormattingEnabled = True
            Me.helpProvider1.SetHelpString(Me.opisolevel, "Set ISO9660 conformance level (1..3) or 4 for ISO9660 version 2")
            Me.opisolevel.Items.AddRange(New Object() {"1", "2", "3", "4"})
            Me.opisolevel.Location = New System.Drawing.Point(185, 6)
            Me.opisolevel.Name = "opisolevel"
            Me.helpProvider1.SetShowHelp(Me.opisolevel, True)
            Me.opisolevel.Size = New System.Drawing.Size(121, 21)
            Me.opisolevel.TabIndex = 0
            '
            'txtOutput
            '
            Me.txtOutput.BackColor = System.Drawing.Color.White
            Me.txtOutput.ForeColor = System.Drawing.Color.Blue
            Me.helpProvider1.SetHelpString(Me.txtOutput, "Output display")
            Me.txtOutput.Location = New System.Drawing.Point(350, 3)
            Me.txtOutput.Multiline = True
            Me.txtOutput.Name = "txtOutput"
            Me.txtOutput.ReadOnly = True
            Me.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
            Me.helpProvider1.SetShowHelp(Me.txtOutput, True)
            Me.txtOutput.Size = New System.Drawing.Size(340, 276)
            Me.txtOutput.TabIndex = 7
            Me.txtOutput.Text = "http://www.ebswift.com" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Licensed under the GPL" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Contains code from the mkisof" & _
                "s project, Mike Mayer " & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "and Dave Peckham"
            '
            'statusStrip1
            '
            Me.statusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.StatusLabel1, Me.ProgressBar1})
            Me.statusStrip1.Location = New System.Drawing.Point(0, 316)
            Me.statusStrip1.Name = "statusStrip1"
            Me.statusStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional
            Me.statusStrip1.Size = New System.Drawing.Size(693, 22)
            Me.statusStrip1.SizingGrip = False
            Me.statusStrip1.TabIndex = 11
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
            Me.ProgressBar1.Visible = False
            '
            'btnExit
            '
            Me.btnExit.Cursor = System.Windows.Forms.Cursors.Hand
            Me.helpProvider1.SetHelpString(Me.btnExit, "Exit DoISO")
            Me.btnExit.Location = New System.Drawing.Point(8, 285)
            Me.btnExit.Name = "btnExit"
            Me.helpProvider1.SetShowHelp(Me.btnExit, True)
            Me.btnExit.Size = New System.Drawing.Size(56, 31)
            Me.btnExit.TabIndex = 12
            Me.btnExit.Text = "Exit"
            '
            'panel2
            '
            Me.panel2.Controls.Add(Me.checkBox3)
            Me.panel2.Controls.Add(Me.checkBox4)
            Me.panel2.Enabled = False
            Me.panel2.Location = New System.Drawing.Point(49, 11)
            Me.panel2.Name = "panel2"
            Me.panel2.Size = New System.Drawing.Size(153, 96)
            Me.panel2.TabIndex = 2
            '
            'checkBox3
            '
            Me.checkBox3.Checked = True
            Me.checkBox3.CheckState = System.Windows.Forms.CheckState.Checked
            Me.checkBox3.Location = New System.Drawing.Point(0, 21)
            Me.checkBox3.Name = "checkBox3"
            Me.checkBox3.Size = New System.Drawing.Size(140, 24)
            Me.checkBox3.TabIndex = 1
            Me.checkBox3.Text = "Session At Once"
            Me.checkBox3.UseVisualStyleBackColor = True
            '
            'checkBox4
            '
            Me.checkBox4.Location = New System.Drawing.Point(0, 0)
            Me.checkBox4.Name = "checkBox4"
            Me.checkBox4.Size = New System.Drawing.Size(140, 24)
            Me.checkBox4.TabIndex = 0
            Me.checkBox4.Text = "Erase First (CD/RW)"
            Me.checkBox4.UseVisualStyleBackColor = True
            '
            'radioButton1
            '
            Me.radioButton1.Location = New System.Drawing.Point(3, 41)
            Me.radioButton1.Name = "radioButton1"
            Me.radioButton1.Size = New System.Drawing.Size(104, 24)
            Me.radioButton1.TabIndex = 1
            Me.radioButton1.TabStop = True
            Me.radioButton1.Text = "CD"
            Me.radioButton1.UseVisualStyleBackColor = True
            '
            'radioButton2
            '
            Me.radioButton2.Location = New System.Drawing.Point(3, 20)
            Me.radioButton2.Name = "radioButton2"
            Me.radioButton2.Size = New System.Drawing.Size(104, 24)
            Me.radioButton2.TabIndex = 0
            Me.radioButton2.TabStop = True
            Me.radioButton2.Text = "DVD"
            Me.radioButton2.UseVisualStyleBackColor = True
            '
            'openISODialog
            '
            Me.openISODialog.FileName = "*.iso"
            Me.openISODialog.Filter = "ISO files|*.iso"
            Me.openISODialog.Title = "Open ISO File to Burn"
            '
            'label6
            '
            Me.label6.Location = New System.Drawing.Point(70, 286)
            Me.label6.Name = "label6"
            Me.label6.Size = New System.Drawing.Size(91, 23)
            Me.label6.TabIndex = 13
            Me.label6.Text = "Drive Target:"
            '
            'cboDriveTarget
            '
            Me.cboDriveTarget.DataBindings.Add(New System.Windows.Forms.Binding("SelectedItem", Me.dataSet1, "mkisofs.DriveTarget", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.cboDriveTarget.DataBindings.Add(New System.Windows.Forms.Binding("SelectedValue", Me.dataSet1, "mkisofs.DriveTarget", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.cboDriveTarget.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.cboDriveTarget.FlatStyle = System.Windows.Forms.FlatStyle.System
            Me.cboDriveTarget.FormattingEnabled = True
            Me.cboDriveTarget.Location = New System.Drawing.Point(154, 286)
            Me.cboDriveTarget.Name = "cboDriveTarget"
            Me.cboDriveTarget.Size = New System.Drawing.Size(71, 21)
            Me.cboDriveTarget.TabIndex = 14
            '
            'DataColumn49
            '
            Me.DataColumn49.AllowDBNull = False
            Me.DataColumn49.ColumnName = "RockRidgeExt"
            Me.DataColumn49.DataType = GetType(Boolean)
            Me.DataColumn49.DefaultValue = "True"
            '
            'DataColumn50
            '
            Me.DataColumn50.ColumnName = "outArgs"
            Me.DataColumn50.Expression = "outMoreVerbose + ' -v -gui' + outCustomParams + outPreparer + outPublisher + outO" & _
                "ptimiseDuplicates + outRockRidgeExt + outDVDVideo + outVolumeLabel + outISOLevel" & _
                " + outDest + outSource"
            Me.DataColumn50.ReadOnly = True
            '
            'DataColumn51
            '
            Me.DataColumn51.ColumnName = "outRockRidgeExt"
            Me.DataColumn51.Expression = "IIF(RockRidgeExt, ' -r', '')"
            Me.DataColumn51.ReadOnly = True
            '
            'MainForm
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.ClientSize = New System.Drawing.Size(693, 338)
            Me.Controls.Add(Me.cboDriveTarget)
            Me.Controls.Add(Me.label6)
            Me.Controls.Add(Me.statusStrip1)
            Me.Controls.Add(Me.tabControl1)
            Me.Controls.Add(Me.btnExit)
            Me.Controls.Add(Me.txtOutput)
            Me.Controls.Add(Me.lblCopyright)
            Me.Controls.Add(Me.linkLabel1)
            Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
            Me.HelpButton = True
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.Name = "MainForm"
            Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
            Me.Text = "DoISO"
            CType(Me.dataSet1, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.tblmkisofs, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.tblcdburn, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.dataTable1, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.dataTable2, System.ComponentModel.ISupportInitialize).EndInit()
            Me.ctxWhatsThis.ResumeLayout(False)
            Me.tabControl1.ResumeLayout(False)
            Me.tabCreateISO.ResumeLayout(False)
            Me.tabCreateISO.PerformLayout()
            Me.grpBurn.ResumeLayout(False)
            Me.pnlDVDBurnOptions.ResumeLayout(False)
            Me.pnlCDBurnOptions.ResumeLayout(False)
            Me.tabBurnISO.ResumeLayout(False)
            Me.groupBox1.ResumeLayout(False)
            Me.grpBurnCD.ResumeLayout(False)
            Me.tabISOOptions.ResumeLayout(False)
            Me.tabISOOptions.PerformLayout()
            Me.statusStrip1.ResumeLayout(False)
            Me.statusStrip1.PerformLayout()
            Me.panel2.ResumeLayout(False)
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private dataColumn40 As System.Data.DataColumn
        Private cboDriveTarget As System.Windows.Forms.ComboBox
        Private label6 As System.Windows.Forms.Label
        Private checkBox6 As System.Windows.Forms.CheckBox
        Private dataColumn39 As System.Data.DataColumn
        Private dataColumn38 As System.Data.DataColumn
        Private dataColumn37 As System.Data.DataColumn
        Private dataTable2 As System.Data.DataTable
        Private pnlDVDBurnOptions As System.Windows.Forms.Panel
        Private chkEraseDVD As System.Windows.Forms.CheckBox
        Private dataColumn36 As System.Data.DataColumn
        Private dataColumn35 As System.Data.DataColumn
        Private chkDeepNesting As System.Windows.Forms.CheckBox
        Private lblBurnSource As System.Windows.Forms.Label
        Private dataColumn34 As System.Data.DataColumn
        Private dataColumn33 As System.Data.DataColumn
        Private dataColumn32 As System.Data.DataColumn
        Private dataColumn31 As System.Data.DataColumn
        Private dataTable1 As System.Data.DataTable
        Private dataColumn30 As System.Data.DataColumn
        Private chkBurnPostGAP As System.Windows.Forms.CheckBox
        Private chkBurnSpeed As System.Windows.Forms.ComboBox
        Private chkBurnSAO As System.Windows.Forms.CheckBox
        Private chkBurnEraseFirst As System.Windows.Forms.CheckBox
        Private dataColumn29 As System.Data.DataColumn
        Private dataColumn28 As System.Data.DataColumn
        Private dataColumn27 As System.Data.DataColumn
        Private dataColumn26 As System.Data.DataColumn
        Private dataColumn25 As System.Data.DataColumn
        Private dataColumn24 As System.Data.DataColumn
        Private dataColumn23 As System.Data.DataColumn
        Private dataColumn22 As System.Data.DataColumn
        Private tblcdburn As System.Data.DataTable
        Private comboBox1 As System.Windows.Forms.ComboBox
        Private label11 As System.Windows.Forms.Label
        Private checkBox5 As System.Windows.Forms.CheckBox
        Private grpBurnCD As System.Windows.Forms.GroupBox
        Private dataColumn21 As System.Data.DataColumn
        Private dataColumn20 As System.Data.DataColumn
        Private dataColumn19 As System.Data.DataColumn
        Private dataColumn18 As System.Data.DataColumn
        Private dataColumn17 As System.Data.DataColumn
        Private dataColumn16 As System.Data.DataColumn
        Private dataColumn15 As System.Data.DataColumn
        Private dataColumn14 As System.Data.DataColumn
        Private dataColumn13 As System.Data.DataColumn
        Private dataColumn12 As System.Data.DataColumn
        Private dataColumn11 As System.Data.DataColumn
        Private dataColumn10 As System.Data.DataColumn
        Private dataColumn9 As System.Data.DataColumn
        Private lblDest As System.Windows.Forms.Label
        Private dataColumn8 As System.Data.DataColumn
        Private dataColumn7 As System.Data.DataColumn
        Private dataColumn6 As System.Data.DataColumn
        Private dataColumn5 As System.Data.DataColumn
        Private dataColumn4 As System.Data.DataColumn
        Private dataColumn3 As System.Data.DataColumn
        Private dataColumn2 As System.Data.DataColumn
        Private tblmkisofs As System.Data.DataTable
        Private dataColumn1 As System.Data.DataColumn
        Private dataSet1 As System.Data.DataSet
        Private openISODialog As System.Windows.Forms.OpenFileDialog
        Private label10 As System.Windows.Forms.Label
        Private btnStartBurn As Wildgrape.Aqua.Controls.Button
        Private radCDBurn As System.Windows.Forms.RadioButton
        Private radDVDBurn As System.Windows.Forms.RadioButton
        Private btnImageToBurn As Wildgrape.Aqua.Controls.Button
        Private radioButton2 As System.Windows.Forms.RadioButton
        Private radioButton1 As System.Windows.Forms.RadioButton
        Private checkBox4 As System.Windows.Forms.CheckBox
        Private checkBox3 As System.Windows.Forms.CheckBox
        Private panel2 As System.Windows.Forms.Panel
        Private groupBox1 As System.Windows.Forms.GroupBox
        Private tabCreateISO As System.Windows.Forms.TabPage
        Private tabISOOptions As System.Windows.Forms.TabPage
        Private tabBurnISO As System.Windows.Forms.TabPage
        Private pnlCDBurnOptions As System.Windows.Forms.Panel
        Private checkBox1 As System.Windows.Forms.CheckBox
        Private checkBox2 As System.Windows.Forms.CheckBox
        Private grpBurn As System.Windows.Forms.GroupBox
        Private radDVD As System.Windows.Forms.RadioButton
        Private radCD As System.Windows.Forms.RadioButton
        Private chkBurnOnComplete As System.Windows.Forms.CheckBox
        Private txtPublisher As System.Windows.Forms.TextBox
        Private txtPreparer As System.Windows.Forms.TextBox
        Private label4 As System.Windows.Forms.Label
        Private label5 As System.Windows.Forms.Label
        Private txtCustomCommand As System.Windows.Forms.TextBox
        Private label1 As System.Windows.Forms.Label
        Private chkMoreVerbose As System.Windows.Forms.CheckBox
        Private lblCopyright As System.Windows.Forms.Label
        Private txtVolumeLabel As System.Windows.Forms.TextBox
        Private label3 As System.Windows.Forms.Label
        Private opDVDFileSystem As System.Windows.Forms.CheckBox
        Private opoptimiseDuplicates As System.Windows.Forms.CheckBox
        Private txtOutput As System.Windows.Forms.TextBox
        Private WithEvents whatsThisToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
        Private ctxWhatsThis As System.Windows.Forms.ContextMenuStrip
        Private helpProvider1 As System.Windows.Forms.HelpProvider
        Private label2 As System.Windows.Forms.Label
        Private opisolevel As System.Windows.Forms.ComboBox
        Private ProgressBar1 As System.Windows.Forms.ToolStripProgressBar
        Private StatusLabel1 As System.Windows.Forms.ToolStripStatusLabel
        Private statusStrip1 As System.Windows.Forms.StatusStrip
        Private btnExit As Wildgrape.Aqua.Controls.Button
        Private tabControl1 As System.Windows.Forms.TabControl
        Private WithEvents linkLabel1 As System.Windows.Forms.LinkLabel
        Private saveFileDialog1 As System.Windows.Forms.SaveFileDialog
        Private btnCreateISO As Wildgrape.Aqua.Controls.Button
        Private btnSelectSource As Wildgrape.Aqua.Controls.Button
        Private WithEvents lblSource As System.Windows.Forms.Label
        Private components As System.ComponentModel.IContainer
        Private folderBrowserDialog1 As System.Windows.Forms.FolderBrowserDialog
#End Region

        ''' <summary>
        ''' Opens the folder select dialog for selecting the source folder to create an ISO from.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Sub BtnSelectSourceClick(ByVal sender As Object, ByVal e As System.EventArgs)
            If folderBrowserDialog1.ShowDialog() <> Windows.Forms.DialogResult.Cancel Then
                lblSource.Text = folderBrowserDialog1.SelectedPath
            End If
        End Sub

        ''' <summary>
        ''' Cancel the current operation - stop the main process.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Sub BtnCancelClick(ByVal sender As Object, ByVal e As System.EventArgs)
            If (Not processCaller Is Nothing) Then
                processCaller.Cancel()
            End If
        End Sub

        'Private CanContinue As Boolean = True

        ''' <summary>
        ''' Button handler for exiting the application.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Sub BtnExitClick(ByVal sender As Object, ByVal e As System.EventArgs)
            Me.Close()
        End Sub

        ''' <summary>
        ''' Upon an application close request, ensure a running process is stopped first.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Sub Me_OnClosing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs)
            If (Not processCaller Is Nothing) Then
                processCaller.Cancel()
            End If

            Try
                Dim utl As Util = New Util

                ' persist properties
                'File.Delete("DoISO.xml")
                'dataSet1.DefaultViewManager.DataSet.WriteXml("DoISO.xml")
                dataSet1.WriteXml(utl.cwd() & "DoISO.xml", Data.XmlWriteMode.WriteSchema)
            Catch ex As Exception
                MessageBox.Show(ex.Message)
            End Try
        End Sub

        ''' <summary>
        ''' LinkLabel for ebswift.com website.  Opens the ebswift.com website upon click.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Sub LinkLabel1LinkClicked(ByVal sender As Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles linkLabel1.LinkClicked
            Process.Start("http://www.ebswift.com")
        End Sub

        ''' <summary>
        ''' DragEnter handler for the source folder selection.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Sub lblSourceDragEnter(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles lblSource.DragEnter
            MyBase.OnDragEnter(e)
            'Dim fmt As String() = e.Data.GetFormats() ' for testing
            Dim data As String() = e.Data.GetData(DataFormats.FileDrop)
            Dim s As String

            For Each s In data
                ' stop on the first valid file/folder found

                Dim fi As FileInfo = New FileInfo(s)

                ' only allow folders to be dragged into the source label
                If ((CDbl(fi.Attributes And FileAttributes.Directory)) = FileAttributes.Directory) Then
                    e.Effect = DragDropEffects.Copy
                    Exit For
                End If
                'Console.WriteLine(s)
            Next

            'Console.WriteLine(e.Data.GetData(DataFormats.FileDrop))
        End Sub

        ''' <summary>
        ''' Drop operation for the source folder selection.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Sub lblSourceDrop(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles lblSource.DragDrop
            MyBase.OnDragDrop(e)
            'Dim fmt As String() = e.Data.GetFormats() ' for testing
            Dim data As String() = e.Data.GetData(DataFormats.FileDrop)
            Dim s As String

            For Each s In data
                lblSource.Text = s
                folderBrowserDialog1.SelectedPath = s
                Exit For
                'Console.WriteLine(s)
            Next
        End Sub

        ''' <summary>
        ''' Whenever the user changes the source, perform checks on the location for validity and type (root or folder).
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Sub LblSourceTextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles lblSource.TextChanged
            m_PostUpdate = "lblSourceTextChangedPostUpdate"
            'lblSource.Refresh
            'dataSet1.DefaultViewManager.DataSet.AcceptChanges()
        End Sub

        Private Sub lblSourceTextChangedPostUpdate()
            Dim s As String = lblSource.Text
            Try
                'Me.btnUpdateSize.Enabled = True

                '                Me.lblImageSize.Text = ""
                '                Me.progSize.Value = 0

                If isDVDVideo(s) Then
                    opDVDFileSystem.Checked = True
                Else
                    opDVDFileSystem.Checked = False
                End If

                If isRoot(s) Then
                    ' if a path is a root then use the volume name as the new ISO volume label
                    Dim volInfo As DriveInfo = New DriveInfo(s)
                    txtVolumeLabel.Text = volInfo.VolumeLabel
                Else
                    ' if the input path is not root then get the folder name
                    ' to use as the default volume label
                    If Directory.Exists(s) Then
                        txtVolumeLabel.Text = Path.GetFileName(s).ToString()
                    End If
                End If
            Catch ex As Exception
                opDVDFileSystem.Checked = False
            End Try
        End Sub

        ''' <summary>
        ''' Determines whether a given path is a root directory.
        ''' </summary>
        ''' <param name="srcdir">The path to check if root.</param>
        ''' <returns>True if the folder is a root, otherwise False.</returns>
        Private Function isRoot(ByVal srcdir As String) As Boolean
            If Not Directory.Exists(srcdir) Then
                Return False
            End If

            If Not Directory.Exists(srcdir) Then
                Return False
            End If

            Return srcdir.EndsWith("\")
        End Function

        ''' <summary>
        ''' Determines whether the given drive is a CD or DVD drive.
        ''' </summary>
        ''' <param name="srcdir">The path to check if CD or DVD.</param>
        ''' <returns>True if the source is a CD or DVD, otherwise False.</returns>
        Private Function isCD(ByVal srcdir As String) As Boolean
            If Not Directory.Exists(srcdir) Then
                Return False
            End If

            If srcdir.EndsWith("\") Then
                ' use a managementobject to determine the drive type
                Dim disk As ManagementObject = New ManagementObject("Win32_LogicalDisk.DeviceID=" & Chr(34) & Replace(srcdir, "\", "") & Chr(34))
                ' Where drive types are:
                ' NoRoot = 1;Removable = 2;LocalDisk = 3;Network = 4;CD = 5;RAMDrive = 6;

                If disk("DriveType") = "5" Then
                    Return True
                End If
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' Check if a given folder appears to be a DVD video filesystem.  If so it will auto-select the DVD-Video option.
        ''' </summary>
        ''' <param name="srcdir">Folder to check if it contains a DVD video filesystem.</param>
        ''' <returns>True if the folder is a root and contains a DVD video filestystem, otherwise False.</returns>
        Private Function isDVDVideo(ByVal srcdir As String) As Boolean
            ' to be DVD Video, the directory must be a valid root directory
            If Not Directory.Exists(srcdir) Then
                opDVDFileSystem.Checked = False
                Return False
            End If

            ' this is a separate check so as not to cause an exception in the above check
            If Not isCD(srcdir) Then
                opDVDFileSystem.Checked = False
                Return False
            End If

            Dim dirEntries As String() = Directory.GetDirectories(srcdir)
            Dim dirname As String
            Dim audts As Boolean = False ' flag to check existance of audio_ts
            Dim vidts As Boolean = False ' flag to check existance of video_ts

            For Each dirname In dirEntries
                If InStr(LCase(dirname), "audio_ts") Then
                    audts = True
                End If

                If InStr(LCase(dirname), "video_ts") Then
                    vidts = True
                End If
            Next

            If audts And vidts Then
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' Begin creating the ISO.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Sub btnCreateISO_Click(ByVal sender As Object, ByVal e As System.EventArgs)
            Dim pd As ProcessDialog = New ProcessDialog

            pd.m_dataset = Me.dataSet1
            pd.m_GetSize = False
            pd.m_task = "ISO"
            pd.m_dest = lblDest.Text
            pd.m_DriveTarget = cboDriveTarget.SelectedItem
            pd.ShowDialog()
        End Sub

        ''' <summary>
        ''' Calculate the resulting size of an ISO.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Sub BtnUpdateSizeClick(ByVal sender As Object, ByVal e As System.EventArgs)
            StatusLabel1.ForeColor = Color.Black

            m_GetSize = True
        End Sub

        ''' <summary>
        ''' This accepts changes to columns (initiated by a change in a databound object).  
        ''' The expression in the related 'out' column is auto-calculated only after AcceptChanges occurs.  
        ''' This always ensures that the expressions on all columns are all current.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Sub dataset1_ColumnChanged(ByVal sender As Object, ByVal e As System.Data.DataColumnChangeEventArgs)
            'dataSet1.DefaultViewManager.DataSet.AcceptChanges()
            dataSet1.AcceptChanges()

            ' The following condition solves the problem of concurrent updates cancelling
            ' out some changes.  It forces the flow to follow sequentially rather concurrently.
            ' For example, when the ISO source folder changes it processes the folder text and
            ' sends the resulting text to a new databound field.  The new field update occurs and
            ' cancels out the source folder change, thereby reverting to the old source folder.
            If m_PostUpdate <> "" Then
                Select Case m_PostUpdate
                    Case "lblSourceTextChangedPostUpdate"
                        lblSourceTextChangedPostUpdate()
                End Select

                m_PostUpdate = ""
            End If
        End Sub

        Sub MainFormLoad(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load
            Application.EnableVisualStyles()

            ' persist properties
            Dim utl As Util = New Util

            ' load up the dvd list first so the last selected item can be selected
            ' after saved data is loaded
            SetCDDVDList()

            If File.Exists(utl.cwd() & "DoISO.xml") Then
                dataSet1.ReadXml(utl.cwd() & "DoISO.xml")

                folderBrowserDialog1.SelectedPath = lblSource.Text
            End If

            ' look for tables with no rows and create default rows
            Dim t As System.Data.DataTable

            For Each t In dataSet1.DefaultViewManager.DataSet.Tables
                ' setup a new default empty row for each table in the dataset
                ' later, defaults could be loaded and saved
                If t.Rows.Count < 1 Then
                    Dim dr As Data.DataRow = t.NewRow
                    t.Rows.Add(dr)
                End If
            Next

            ' get a commandline argument for the source - useful for explorer integration
            If args.Length > 1 Then
                If args.Length > 2 Then
                    If args(2) = "-burn" Then
                        tabControl1.SelectedTab = tabBurnISO
                        lblBurnSource.Text = Replace(args(1), ":" & chr(34), ":\")
                    End If
                Else
                    lblSource.Text = Replace(args(1), ":" & chr(34), ":\")
                End If
            End If

            Me.DataColumn47.Expression = "IIF(Source = 'Select a folder to create an ISO from' Or ((Max(Child.BurnOnComplete)=True) And DriveTarget = ''), False, True)"
            '           Me.DataColumn47.Expression = "(Max(Child.BurnOnComplete)=True) And DriveTarget = '')"
            dataSet1.AcceptChanges()
        End Sub
        
        Private Sub SetCDDVDList()
			Dim d() As String
			
			d = System.IO.Directory.GetLogicalDrives()
			
			Dim en As System.Collections.IEnumerator
			
			en = d.GetEnumerator
			
			While en.MoveNext
				Console.WriteLine(CStr(en.Current))
				Dim disk As ManagementObject = New ManagementObject("Win32_LogicalDisk.DeviceID=" & Chr(34) & Replace(en.Current, "\", "") & Chr(34))
				' Where drive types are:
				' NoRoot = 1;Removable = 2;LocalDisk = 3;Network = 4;CD = 5;RAMDrive = 6;
			
				If disk("DriveType") = DRIVE_CDROM Then
					cboDriveTarget.Items.Add(en.Current)
				End If
			End While
        End Sub

        Private Sub cboDriveTargetSelectedValueChanged(ByVal sender As Object, ByVal e As System.EventArgs)
            cboDriveTarget.Refresh()
        End Sub
    End Class

	Public Class Util
        Public Function cwd() As String
            Return Application.StartupPath & "\"
        End Function
	End Class
End NameSpace
