﻿Imports LibraryDataset
Imports LibraryCommon

Public Class frmCustomerCategory
    Dim conn As New connCommon()
    Dim clsPMSAnalysis As New clsPerson(conn.connSales.ConnectionString)
    Dim clsAccount As New clsAccount(conn.connSales.ConnectionString)
    Dim clsRolePermission As New clsRolePermission(conn.connSales.ConnectionString)

    Private isAddFirstName As Boolean = False
    Private isAddLastName As Boolean = False
    Private Sub CustomerCategory_Load(sender As Object, e As EventArgs) Handles MyBase.Load
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
            If form = "Customer category" Then
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
        Dim data = clsPMSAnalysis.GetCustomers()
        dgvCategory.DataSource = data.Person
        dgvCategory.Columns(1).Visible = False
        dgvCategory.Columns(9).Visible = False
        setEnable(False)
        setValue()
    End Sub

    Private Sub bEdit_Click(sender As Object, e As EventArgs) Handles bEdit.Click
        addEditDeleteEnabled(False)
        bDelete.Enabled = True
        setEnable(True)
    End Sub

    Private Sub setEnable(valBoolean As Boolean)
        txtCustomerFirstName.Enabled = valBoolean
        txtCustomerLastName.Enabled = valBoolean
        txtPhone.Enabled = valBoolean
        txtAddress.Enabled = valBoolean
        dtBirthDay.Enabled = valBoolean
        rdMale.Enabled = valBoolean
        rdFemale.Enabled = valBoolean
        txtEmail.Enabled = valBoolean
        bSave.Enabled = valBoolean
    End Sub

    Private Sub addEditDeleteEnabled(valBoolean As Boolean)
        bAdd.Enabled = valBoolean
        bEdit.Enabled = valBoolean
        bDelete.Enabled = valBoolean
    End Sub

    Private Sub clearValue()
        txtCustomerFirstName.Text = "First name"
        txtCustomerLastName.Text = "Last name"
        txtCustomerCode.Text = ""
        rdMale.Checked = True
        txtPhone.Text = ""
        txtAddress.Text = ""
        dtBirthDay.Value = DateTime.Now
        txtEmail.Text = ""
        setPlaceHolderEnable(True)
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
        setPlaceHolderEnable(False)
    End Sub

    Private Sub setValue()
        If dgvCategory.Rows.Count = 0 Then
            addEditDeleteEnabled(False)
            bAdd.Enabled = True
            Return
        Else
            Dim row As DataGridViewRow = dgvCategory.CurrentRow
            txtCustomerCode.Text = row.Cells(0).Value.ToString
            txtCustomerLastName.Text = row.Cells(2).Value.ToString
            txtCustomerFirstName.Text = row.Cells(3).Value.ToString
            dtBirthDay.Text = row.Cells(5).Value.ToString
            txtPhone.Text = row.Cells(6).Value.ToString
            txtEmail.Text = row.Cells(7).Value.ToString
            txtAddress.Text = row.Cells(8).Value.ToString
            If row.Cells(4).Value.ToString() = "True" Then
                rdMale.Checked = True
            Else
                rdFemale.Checked = True
            End If
        End If
    End Sub

    Private Sub bSave_Click(sender As Object, e As EventArgs) Handles bSave.Click
        If checkLogicData() Then
            Dim result As Integer
            Dim type As String = "Update"
            If txtCustomerCode.Text <> "" Then          'Edit
                result = clsPMSAnalysis.EditCustomer(Nothing, txtCustomerLastName.Text,
                                            txtCustomerFirstName.Text, rdMale.Checked, dtBirthDay.Value,
                                            txtPhone.Text, txtEmail.Text, txtAddress.Text, LoginForm.PropUsername, txtCustomerCode.Text)
            Else                                             'Add new
                result = clsPMSAnalysis.AddCustomer(Nothing, txtCustomerLastName.Text,
                                            txtCustomerFirstName.Text, rdMale.Checked, dtBirthDay.Value,
                                            txtPhone.Text, txtEmail.Text, txtAddress.Text, LoginForm.PropUsername)
                type = "Add"
            End If

            If result = 1 Then
                setEnable(False)
                MsgBox(type & " customer information successful!", Nothing, "Notification")
                Reload()
                addEditDeleteEnabled(True)
                setPlaceHolderEnable(False)
            Else
                MsgBox("There is an error when interact with database!", Nothing, "Notification")
            End If
        End If
    End Sub

    Private Function checkLogicData() As Boolean
        If txtCustomerFirstName.Text = "" Or isAddFirstName = True Or isAddLastName = True Or txtCustomerLastName.Text = "" Or txtPhone.Text = "" Or txtAddress.Text = "" Or
            txtEmail.Text = "" Then

            MsgBox("You need to enter all the fields!", Nothing, "Notification")
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
        If txtCustomerCode.Text <> "" Then
            Dim username As String = txtCustomerCode.Text
            Dim isDelete = clsAccount.CheckUserWasDeleted(username)
            If Not isDelete Then
                Dim result = clsPMSAnalysis.DeleteUser(LoginForm.PropUsername, username)
                If result = 1 Then
                    setEnable(False)
                    MsgBox("Delete customer information successful!", Nothing, "Notification")
                    Reload()
                Else
                    MsgBox("There is an error when interact with database!", Nothing, "Notification")
                End If
            Else
                MsgBox("User was deleted before!", Nothing, "Notification")
            End If

        End If
    End Sub

    Private Sub txtCustomerFirstName_Click(sender As Object, e As EventArgs) Handles txtCustomerFirstName.Click
        If isAddFirstName Then
            txtCustomerFirstName.Text = ""
            txtCustomerFirstName.ForeColor = Color.Black
            isAddFirstName = False
        End If
    End Sub

    Private Sub txtCustomerLastName_Click(sender As Object, e As EventArgs) Handles txtCustomerLastName.Click
        If isAddLastName Then
            txtCustomerLastName.Text = ""
            txtCustomerLastName.ForeColor = Color.Black
            isAddLastName = False
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
    Public Sub setPlaceHolderEnable(ByVal valBoolean As Boolean)
        If valBoolean = False Then
            txtCustomerLastName.ForeColor = Color.Black
            isAddLastName = False
            txtCustomerFirstName.ForeColor = Color.Black
            isAddFirstName = False
        Else
            txtCustomerLastName.ForeColor = Color.Gray
            isAddLastName = True
            txtCustomerFirstName.ForeColor = Color.Gray
            isAddFirstName = True
        End If
    End Sub
End Class