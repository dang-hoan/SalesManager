﻿Imports LibraryDataset
Imports LibraryCommon

Public Class SupplierCategory
    Dim conn As New connCommon()
    Dim clsPMSAnalysis As New clsSupplier(conn.connSales.ConnectionString)
    Dim clsRolePermission As New clsRolePermission(conn.connSales.ConnectionString)
    Private Sub SupplierCategory_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Reload()
        SetVisibleForPermission()
    End Sub

    Private Sub SetVisibleForPermission()
        bAdd.Visible = False
        bEdit.Visible = False
        bDelete.Visible = False
        bSave.Visible = False
        Dim dataPermission = clsRolePermission.GetPermissionOfUser(LoginForm.PropUsername)
        For Each permission In dataPermission
            Dim form = permission(1).split(":")(0)
            Dim permiss = Strings.Split(Strings.Split(permission(1), ": ")(1), ", ")
            If form = "Supplier category" Then
                For Each p In permiss
                    Select Case p
                        Case "Add"
                            bAdd.Visible = True
                            bSave.Visible = True
                        Case "Edit"
                            bEdit.Visible = True
                            bSave.Visible = True
                        Case "Delete"
                            bDelete.Visible = True
                    End Select
                Next
                Exit For
            End If
        Next
        CenterButtons()
    End Sub

    Private Sub CenterButtons()
        Dim listButtons = New List(Of Button) From {bAdd, bEdit, bDelete, bSave}
        Dim totalWidth As Integer = 0
        Dim count = 0

        For Each btn As Button In listButtons
            If btn.Visible = True Then
                totalWidth += btn.Width
                count += 1
            End If
        Next

        Dim offset_between = 30
        Dim x As Integer = (Me.Width - totalWidth - offset_between * (count - 1)) / 2
        Dim y As Integer = 450

        For Each btn As Button In listButtons
            If btn.Visible = True Then
                btn.Location = New Point(x, y)
                x += btn.Width + offset_between
            End If
        Next
    End Sub

    Private Sub Reload()
        ds_Supplier.Clear()
        ds_Supplier.Merge(clsPMSAnalysis.GetAllSuppliers())
        setEnable(False)
        setValue()
    End Sub

    Private Sub bEdit_Click(sender As Object, e As EventArgs) Handles bEdit.Click
        addEditDeleteEnabled(False)
        bDelete.Enabled = True
        setEnable(True)
    End Sub

    Private Sub setEnable(valBoolean As Boolean)
        txtName.Enabled = valBoolean
        txtPhone.Enabled = valBoolean
        txtAddress.Enabled = valBoolean
        txtEmail.Enabled = valBoolean
        txtWebpage.Enabled = valBoolean
        txtDescription.Enabled = valBoolean
        bSave.Enabled = valBoolean
    End Sub

    Private Sub addEditDeleteEnabled(valBoolean As Boolean)
        bAdd.Enabled = valBoolean
        bEdit.Enabled = valBoolean
        bDelete.Enabled = valBoolean
    End Sub

    Private Sub clearValue()
        txtCode.Text = ""
        txtName.Text = ""
        txtAddress.Text = ""
        txtPhone.Text = ""
        txtEmail.Text = ""
        txtWebpage.Text = ""
        txtDescription.Text = ""
    End Sub

    Private Sub bAdd_Click(sender As Object, e As EventArgs) Handles bAdd.Click
        addEditDeleteEnabled(False)
        clearValue()
        setEnable(True)
    End Sub

    Private Sub dgvCategory_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvCategory.CellClick
        addEditDeleteEnabled(True)
        setEnable(False)
        setValue()
    End Sub

    Private Sub setValue()
        If dgvCategory.Rows.Count = 0 Then
            addEditDeleteEnabled(False)
            bAdd.Enabled = True
            Return
        Else
            Dim row As DataGridViewRow = dgvCategory.CurrentRow
            txtCode.Text = row.Cells(0).Value.ToString
            txtName.Text = row.Cells(1).Value.ToString
            txtAddress.Text = row.Cells(2).Value.ToString
            txtPhone.Text = row.Cells(3).Value.ToString
            txtEmail.Text = row.Cells(4).Value.ToString
            txtWebpage.Text = row.Cells(5).Value.ToString
            txtDescription.Text = row.Cells(6).Value.ToString
        End If
    End Sub

    Private Sub bSave_Click(sender As Object, e As EventArgs) Handles bSave.Click
        If checkLogicData() Then
            Dim result As Integer
            Dim type As String = "Update"
            If txtCode.Text <> "" Then          'Edit
                result = clsPMSAnalysis.UpdateSupplier(txtCode.Text, txtName.Text,
                                            txtAddress.Text, txtPhone.Text, txtEmail.Text,
                                            txtWebpage.Text, txtDescription.Text,
                                            LoginForm.PropUsername)
            Else                                             'Add new
                result = clsPMSAnalysis.AddSupplier(txtName.Text, txtAddress.Text,
                                            txtPhone.Text, txtEmail.Text,
                                            txtWebpage.Text, txtDescription.Text,
                                            LoginForm.PropUsername)
                type = "Add"
            End If

            If result = 1 Then
                setEnable(False)
                MsgBox(type & " supplier information successful!", Nothing, "Notification")
                Reload()
                addEditDeleteEnabled(True)
            Else
                MsgBox("There is an error when interact with database!", Nothing, "Notification")
            End If
        End If
    End Sub

    Private Function checkLogicData() As Boolean
        If txtName.Text = "" Or
            txtEmail.Text = "" Or txtPhone.Text = "" Or txtAddress.Text = "" Then

            MsgBox("You need to enter all the required fields!", Nothing, "Notification")
            Return False

        ElseIf Not CheckValue("Phone", txtPhone.Text, "Long") Then
            Return False

        ElseIf Not txtPhone.Text.StartsWith("0") Then
            MsgBox("Phone number must be started with '0'!", Nothing, "Notification")
            Return False

        ElseIf countString(txtEmail.Text, "gmail.com") <> 1 Or Not txtEmail.Text.EndsWith("@gmail.com") Then
            MsgBox("Email invalidate!", Nothing, "Notification")
            Return False
        End If

        Return True
    End Function

    Private Function CheckValue(ByVal label As String, ByVal value As String, ByVal style As String) As Boolean
        Dim returnVal = True

        If value.Length = 0 Then
            Return True
        End If

        Select Case style
            Case "Long"
                Dim Number As Long
                Try
                    Number = Long.Parse(value)
                Catch ex As FormatException
                    MsgBox(label & " must be a integer number!", Nothing, "Notification")
                    returnVal = False
                Catch ex As OverflowException
                    MsgBox(label & " is too big to handle!", Nothing, "Notification")
                    returnVal = False
                End Try

            Case "Double"
                Dim Number As Double
                Try
                    Number = Double.Parse(value)
                Catch ex As FormatException
                    MsgBox(label & " must be a number!", Nothing, "Notification")
                    returnVal = False
                Catch ex As OverflowException
                    MsgBox(label & " is too big to handle!", Nothing, "Notification")
                    returnVal = False
                End Try

        End Select

        Return returnVal

    End Function


    Public Function countString(ByVal inputString As String, ByVal subString As String) As Integer
        Return System.Text.RegularExpressions.Regex.Split(inputString, subString).Length - 1

    End Function

    Private Sub bDelete_Click(sender As Object, e As EventArgs) Handles bDelete.Click
        If txtCode.Text <> "" Then
            Dim code As String = txtCode.Text
            Dim result = clsPMSAnalysis.DeleteSupplier(code, LoginForm.PropUsername)
            If result = 1 Then
                setEnable(False)
                MsgBox("Delete supplier information successful!", Nothing, "Notification")
                Reload()
            Else
                MsgBox("There is an error when interact with database!", Nothing, "Notification")
            End If

        End If
    End Sub

    Private Sub dgvCategory_KeyUp(sender As Object, e As KeyEventArgs) Handles dgvCategory.KeyUp
        If e.KeyCode.Equals(Keys.Up) Or e.KeyCode.Equals(Keys.Down) Then
            If dgvCategory.CurrentRow IsNot Nothing And bSave.Enabled = False Then
                addEditDeleteEnabled(True)
                setEnable(False)
                setValue()
            End If
        End If
    End Sub
End Class