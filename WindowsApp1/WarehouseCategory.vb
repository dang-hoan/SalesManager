﻿Imports LibraryDataset
Imports LibraryCommon
Public Class WarehouseCategory
    Dim conn As New connCommon()
    Dim clsPMSAnalysis As New clsWarehouse(conn.connSales.ConnectionString)
    Dim clsProduct As New clsProduct(conn.connSales.ConnectionString)
    Dim clsRolePermission As New clsRolePermission(conn.connSales.ConnectionString)

    Private warehouseId As Long
    Private isSaved As Boolean = False
    Private allowEditProduct As Boolean = False
    Private allowDeleteProduct As Boolean = False
    Private Sub WarehouseCategory_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Reload()
        SetVisibleForPermission()
    End Sub

    Private Sub SetVisibleForPermission()
        bAdd.Visible = False
        bEdit.Visible = False
        bDelete.Visible = False
        bSave.Visible = False
        bAddProduct.Visible = False

        Dim dataPermission = clsRolePermission.GetPermissionOfUser(LoginForm.PropUsername)
        Dim viewDetail = False

        For Each permission In dataPermission
            Dim form = permission(1).split(":")(0)
            Dim permiss = Strings.Split(Strings.Split(permission(1), ": ")(1), ", ")
            Select Case form
                Case "Warehouse category"
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
                Case "Detail product of warehouse"
                    For Each p In permiss
                        Select Case p
                            Case "Add"
                                bAddProduct.Visible = True
                            Case "Edit"
                                allowEditProduct = True
                            Case "Delete"
                                allowDeleteProduct = True
                        End Select
                    Next
                    viewDetail = True
            End Select
        Next
        If Not viewDetail Then
            gbProducts.Visible = False
            dgvCategory.Location = New Point(gbProducts.Location.X, gbProducts.Location.Y + 10)
            dgvCategory.Size = New Size(555, 280)
        End If
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
        Dim y As Integer = 490

        For Each btn As Button In listButtons
            If btn.Visible = True Then
                btn.Location = New Point(x, y)
                x += btn.Width + offset_between
            End If
        Next
    End Sub

    Public Sub Reload()
        dgvCategory.DataSource = clsPMSAnalysis.GetWarehouse()
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
            If row Is Nothing Then
                row = dgvCategory.Rows(0)
            End If

            txtCode.Text = row.Cells(0).Value.ToString
            txtName.Text = row.Cells(1).Value.ToString
            txtAddress.Text = row.Cells(2).Value.ToString
            txtNumberOfImport.Text = row.Cells(3).Value.ToString
            txtNumberOfExport.Text = row.Cells(4).Value.ToString

            Warehouse.Clear()
            Warehouse.Merge(clsPMSAnalysis.GetProductsOfWarehouse(txtCode.Text))

        End If
    End Sub

    Private Sub bEdit_Click(sender As Object, e As EventArgs) Handles bEdit.Click
        If dgvCategory.Rows.Count = 0 Then
            MsgBox("There isn't any warehouse information to edit!", Nothing, "Notification")
        Else
            addEditDeleteEnabled(False)
            bDelete.Enabled = True
            setEnable(True)
        End If
    End Sub

    Private Sub setEnable(valBoolean As Boolean)
        txtName.Enabled = valBoolean
        txtAddress.Enabled = valBoolean
        bAddProduct.Enabled = valBoolean
        'txtNumberOfImport.Enabled = valBoolean
        'txtNumberOfExport.Enabled = valBoolean
        dgvProduct.Columns(3).Visible = valBoolean And allowEditProduct
        dgvProduct.Columns(4).Visible = valBoolean And allowDeleteProduct
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
        txtNumberOfImport.Text = "0"
        txtNumberOfExport.Text = "0"
        Warehouse.Clear()
    End Sub

    Private Sub bAdd_Click(sender As Object, e As EventArgs) Handles bAdd.Click
        clearValue()
        setEnable(True)
        addEditDeleteEnabled(False)
        warehouseId = clsPMSAnalysis.AddWarehouse("", "", 0, 0, LoginForm.PropUsername)
        isSaved = False
    End Sub

    Private Sub dgvCategory_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvCategory.CellClick
        setEnable(False)
        setValue()
        addEditDeleteEnabled(True)
        If Not isSaved Then
            clsPMSAnalysis.DeleteCompletelyWarehouse(warehouseId)
            Dim products = clsPMSAnalysis.GetProductsOfWarehouse(warehouseId)
            For Each product In products
                clsProduct.DeleteCompletelyProduct(product.ProductId)
            Next
            clsPMSAnalysis.DeleteCompletelySalesDetail(warehouseId)
        End If
    End Sub

    Private Sub bSave_Click(sender As Object, e As EventArgs) Handles bSave.Click
        If checkLogicData() Then
            Dim result As Integer
            Dim selectedWarehouse = txtCode.Text
            Dim type As String = "Update"

            If txtCode.Text = "" Then          'Add new
                type = "Add"
                selectedWarehouse = warehouseId
            End If
            result = clsPMSAnalysis.UpdateWarehouse(selectedWarehouse, txtName.Text, txtAddress.Text, txtNumberOfImport.Text, txtNumberOfExport.Text, LoginForm.PropUsername)
            If result = 1 Then
                setEnable(False)
                MsgBox(type & " warehouse information successful!", Nothing, "Notification")
                Reload()
                addEditDeleteEnabled(True)
                isSaved = True
            Else
                MsgBox("There is an error when interact with database!", Nothing, "Notification")
            End If
        End If
    End Sub

    Private Function checkLogicData() As Boolean
        If txtName.Text = "" Or txtAddress.Text = "" Or txtNumberOfImport.Text = "" Or
            txtNumberOfExport.Text = "" Then

            MsgBox("You need to enter all the fields!", Nothing, "Notification")
            Return False

        ElseIf Not CheckValue("Warehouse code", txtCode.Text, "Long") Or
            Not CheckValue("Number of import", txtNumberOfImport.Text, "Long") Or
            Not CheckValue("Number of export", txtNumberOfExport.Text, "Long") Then
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

    Private Sub bDelete_Click(sender As Object, e As EventArgs) Handles bDelete.Click
        If txtCode.Text <> "" Then
            Dim warehouseId = txtCode.Text
            Dim result = clsPMSAnalysis.DeleteWarehouse(warehouseId, LoginForm.PropUsername)
            If result = 1 Then
                MsgBox("Delete warehouse information successful!", Nothing, "Notification")
                Reload()
            Else
                MsgBox("There is an error when interact with database!", Nothing, "Notification")
            End If

        End If
    End Sub

    Private Sub bAddProduct_Click(sender As Object, e As EventArgs) Handles bAddProduct.Click
        Dim newForm As New AddEditProductForm
        newForm.warehouseId = warehouseId
        If txtCode.Text <> "" Then
            newForm.warehouseId = txtCode.Text
        End If

        If newForm.ShowDialog() = DialogResult.OK Then
            txtNumberOfImport.Text = clsPMSAnalysis.GetWarehouseById(newForm.warehouseId).Rows(0)(3)
        End If
    End Sub

    Private Sub dgvProduct_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvProduct.CellContentClick
        Select Case dgvProduct.Columns(e.ColumnIndex).Name
            Case "Edit"
                Dim newForm As New AddEditProductForm
                If (txtCode.Text <> "") Then
                    newForm.warehouseId = txtCode.Text
                Else
                    newForm.warehouseId = warehouseId
                End If
                newForm.productId = dgvProduct.CurrentRow.Cells(0).Value.ToString
                If newForm.ShowDialog() = DialogResult.OK Then

                End If

            Case "Delete"
                Dim result = clsProduct.DeleteProduct(LoginForm.PropUsername, dgvProduct.CurrentRow.Cells(0).Value.ToString)
                If result = 1 Then
                    result = clsProduct.DeleteSalesDetail(dgvProduct.CurrentRow.Cells(0).Value.ToString)
                End If
                If result = 1 Then
                    MsgBox("Delete product information successful!", Nothing, "Notification")
                    Dim selectedWarehouse = If(txtCode.Text = "", warehouseId, txtCode.Text)
                    Warehouse.Clear()
                    Warehouse.Merge(clsPMSAnalysis.GetProductsOfWarehouse(selectedWarehouse))
                Else
                    MsgBox("There is an error when interact with database!", Nothing, "Notification")
                End If
        End Select
    End Sub

    Private Sub WarehouseCategory_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If Not isSaved Then
            clsPMSAnalysis.DeleteCompletelyWarehouse(warehouseId)
            Dim products = clsPMSAnalysis.GetProductsOfWarehouse(warehouseId)
            For Each product In products
                clsProduct.DeleteCompletelyProduct(product.ProductId)
            Next
            clsPMSAnalysis.DeleteCompletelySalesDetail(warehouseId)
        End If
    End Sub
    Public Sub SetImports(warehouseId)
        txtNumberOfImport.Text = clsPMSAnalysis.GetWarehouseById(warehouseId).Rows(0)(3)
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