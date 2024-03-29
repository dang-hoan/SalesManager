﻿Imports System
Imports System.Drawing
Imports System.Windows.Forms
Imports System.Windows.Forms.VisualStyles
Imports System.ComponentModel

Public Class DataGridViewCheckBoxHeaderCell
    Inherits DataGridViewColumnHeaderCell

    Private CheckBoxBounds As Rectangle
    Private CellLocation As New Point()
    Private CheckBoxState As CheckBoxState

    Public Delegate Sub DataGridViewCheckBoxHeaderCellEventHandler(ByVal sender As Object, ByVal e As DataGridViewCheckBoxHeaderCellEventArgs)
    Public Event CheckBoxClicked As DataGridViewCheckBoxHeaderCellEventHandler

#Region "Properties"
    Private _CheckBoxAlignment As HorizontalAlignment = HorizontalAlignment.Center
    Public Property CheckBoxAlignment() As HorizontalAlignment
        Get
            Return _CheckBoxAlignment
        End Get
        Set(ByVal value As HorizontalAlignment)
            If Not [Enum].IsDefined(GetType(HorizontalAlignment), value) Then
                Throw New InvalidEnumArgumentException("value", CInt(value), GetType(HorizontalAlignment))
            End If
            If _CheckBoxAlignment <> value Then
                _CheckBoxAlignment = value
                If Me.DataGridView IsNot Nothing Then Me.DataGridView.InvalidateCell(Me)
            End If
        End Set
    End Property

    Private _Checked As Boolean
    Public Property Checked() As Boolean
        Get
            Return _Checked
        End Get
        Set(ByVal value As Boolean)
            'If _Checked <> value Then 
            _Checked = value
            CheckState = IIf(_Checked, CheckState.Checked, CheckState.Unchecked)
            If Me.DataGridView IsNot Nothing Then Me.DataGridView.InvalidateCell(Me)
            'End If 
        End Set
    End Property

    Public Property CheckState() As CheckState
        Get
            Select Case CheckBoxState
                Case CheckBoxState.CheckedDisabled, CheckBoxState.CheckedHot, CheckBoxState.CheckedNormal, CheckBoxState.CheckedPressed
                    Return CheckState.Checked
                Case CheckBoxState.UncheckedDisabled, CheckBoxState.UncheckedHot, CheckBoxState.UncheckedNormal, CheckBoxState.UncheckedPressed
                    Return CheckState.Unchecked
                Case CheckBoxState.MixedDisabled, CheckBoxState.MixedHot, CheckBoxState.MixedNormal, CheckBoxState.MixedPressed
                    Return CheckState.Indeterminate
                Case Else
                    Return CheckState.Unchecked
            End Select
        End Get
        Set(ByVal value As CheckState)
            If CheckState <> value Then
                If MyBase.DataGridView IsNot Nothing AndAlso MyBase.DataGridView.Enabled Then
                    'enabled state 
                    Select Case value
                        Case CheckState.Checked
                            CheckBoxState = CheckBoxState.CheckedNormal
                            _Checked = True
                        Case CheckState.Indeterminate
                            CheckBoxState = CheckBoxState.MixedNormal
                        Case CheckState.Unchecked
                            CheckBoxState = CheckBoxState.UncheckedNormal
                            _Checked = False
                    End Select
                Else
                    'disabled state 
                    Select Case value
                        Case CheckState.Checked
                            CheckBoxState = CheckBoxState.CheckedDisabled
                            _Checked = True
                        Case CheckState.Indeterminate
                            CheckBoxState = CheckBoxState.MixedDisabled
                        Case CheckState.Unchecked
                            CheckBoxState = CheckBoxState.UncheckedDisabled
                            _Checked = False
                    End Select
                End If
                If Me.DataGridView IsNot Nothing Then Me.DataGridView.InvalidateCell(Me)
            End If
        End Set
    End Property
#End Region

#Region "Methods"
    Protected Overridable Sub OnCheckBoxClicked(ByVal e As DataGridViewCheckBoxHeaderCellEventArgs)
        RaiseEvent CheckBoxClicked(Me, e)
    End Sub

    Public Sub CheckUncheckEntireColumn(ByVal listValue As List(Of Long))
        Me.DataGridView.SuspendLayout()
        'Console.WriteLine("size: " & listValue.Count)
        For Each row As DataGridViewRow In Me.DataGridView.Rows
            row.Cells(Me.ColumnIndex).Value = listValue.Contains(row.Cells(1).Value.ToString())
        Next
        Me.DataGridView.ResumeLayout(True)
        Me.DataGridView.RefreshEdit()
    End Sub
#End Region

#Region "Override"
    Protected Overloads Overrides Sub Paint(ByVal graphics As Graphics, ByVal clipBounds As Rectangle, ByVal cellBounds As Rectangle, ByVal rowIndex As Integer, ByVal dataGridViewElementState As DataGridViewElementStates, ByVal value As Object,
                                            ByVal formattedValue As Object, ByVal errorText As String, ByVal cellStyle As DataGridViewCellStyle, ByVal advancedBorderStyle As DataGridViewAdvancedBorderStyle, ByVal paintParts As DataGridViewPaintParts)
        'checkbox bounds 
        Dim state As CheckBoxState = Me.CheckBoxState
        Dim checkBoxSize As Size
        Dim checkBoxLocation As Point

        CellLocation = cellBounds.Location
        'checkBoxSize = CheckBoxRenderer.GetGlyphSize(graphics, state)
        checkBoxSize = New Size(11, 11)
        'Console.WriteLine("size: " & checkBoxSize.Height & ", " & checkBoxSize.Width)
        Dim p As New Point()
        p.Y = cellBounds.Location.Y + (cellBounds.Height / 2) - (checkBoxSize.Height / 2)
        Select Case Me.CheckBoxAlignment
            Case HorizontalAlignment.Center
                p.X = cellBounds.Location.X + (cellBounds.Width / 2) - (checkBoxSize.Width / 2) - 1
            Case HorizontalAlignment.Left
                p.X = cellBounds.Location.X + 2
            Case HorizontalAlignment.Right
                p.X = cellBounds.Right - checkBoxSize.Width - 4
        End Select
        checkBoxLocation = p
        CheckBoxBounds = New Rectangle(checkBoxLocation, checkBoxSize)

        'paint background 
        paintParts = paintParts And Not DataGridViewPaintParts.ContentForeground
        MyBase.Paint(graphics, clipBounds, cellBounds, rowIndex, dataGridViewElementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts)

        'paint foreground 
        Select Case Me.CheckBoxAlignment
            Case HorizontalAlignment.Center
                cellBounds.Width = checkBoxLocation.X - cellBounds.X - 2
            Case HorizontalAlignment.Left
                cellBounds.X += checkBoxSize.Width + 2
                cellBounds.Width -= checkBoxSize.Width + 2
            Case HorizontalAlignment.Right
                cellBounds.Width -= checkBoxSize.Width + 4
        End Select
        paintParts = DataGridViewPaintParts.ContentForeground
        MyBase.Paint(graphics, clipBounds, cellBounds, rowIndex, dataGridViewElementState, value, "", errorText, cellStyle, advancedBorderStyle, paintParts)

        'paint check box           
        CheckBoxRenderer.DrawCheckBox(graphics, checkBoxLocation, state)
    End Sub

    Protected Overloads Overrides Sub OnMouseClick(ByVal e As DataGridViewCellMouseEventArgs)
        If e.Button = MouseButtons.Left Then
            'click on check box ? 
            Dim p As New Point(CellLocation.X + e.X, CellLocation.Y + e.Y)
            If CheckBoxBounds.Contains(p) Then
                'raise event 
                RaiseCheckBoxClicked()
            End If
        End If
        MyBase.OnMouseClick(e)
    End Sub

    Protected Overloads Overrides Sub OnKeyDown(ByVal e As KeyEventArgs, ByVal rowIndex As Integer)
        If e.KeyCode = Keys.Space Then
            'raise event 
            RaiseCheckBoxClicked()
        End If
        MyBase.OnKeyDown(e, rowIndex)
    End Sub

    Public Overloads Overrides Function Clone() As Object
        Dim cell As DataGridViewCheckBoxHeaderCell = TryCast(MyBase.Clone(), DataGridViewCheckBoxHeaderCell)
        If cell IsNot Nothing Then
            cell.Checked = Me.Checked
        End If
        Return cell
    End Function
#End Region

#Region "Private"
    Private Sub RaiseCheckBoxClicked()
        'raise event 
        Dim e As New DataGridViewCheckBoxHeaderCellEventArgs(Not Me.Checked)
        Me.OnCheckBoxClicked(e)
        If Not e.Cancel Then
            Me.Checked = e.Checked
            Me.DataGridView.InvalidateCell(Me)
        End If
    End Sub
#End Region
End Class

#Region "EventArgs Class"
Public Class DataGridViewCheckBoxHeaderCellEventArgs
    Inherits CancelEventArgs

    Private _Checked As Boolean
    Public ReadOnly Property Checked() As Boolean
        Get
            Return _Checked
        End Get
    End Property

    Public Sub New(ByVal checkedValue As Boolean)
        MyBase.New()
        _Checked = checkedValue
    End Sub

    Public Sub New(ByVal checkedValue As Boolean, ByVal cancel As Boolean)
        MyBase.New(cancel)
        _Checked = checkedValue
    End Sub
End Class
#End Region