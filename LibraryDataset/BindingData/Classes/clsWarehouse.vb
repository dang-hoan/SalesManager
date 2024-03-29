﻿Imports System.Data.SqlClient
Public Class clsWarehouse
    Dim ta As New WarehouseTableAdapters.WareHouseTableAdapter
    Dim taSalesDetail As New WarehouseTableAdapters.SalesDetailTableAdapter
    Private conn As New SqlConnection
    Public Sub New(ByVal strConn As String)
        conn = New SqlConnection(strConn)
    End Sub
    Public Function GetWarehouse() As Warehouse.WareHouseDataTable
        ta.Connection = conn
        Return ta.GetData()
    End Function
    Public Function GetWarehouseById(ByVal warehouseId As Long) As DataRow
        ta.Connection = conn
        Return ta.GetWarehouseById(warehouseId).Rows(0)
    End Function
    Public Function GetProductsOfWarehouse(ByVal warehouseId As Long) As Warehouse.SalesDetailDataTable
        taSalesDetail.Connection = conn
        Return taSalesDetail.GetData(warehouseId)
    End Function
    Public Function GetWarehouseByProductId(ByVal productId As Long) As Warehouse.WareHouseDataTable
        ta.Connection = conn
        Return ta.GetWarehouseByProductId(productId)
    End Function
    Public Function AddWarehouse(ByVal warehouseName As String, ByVal address As String, ByVal numberOfImport As Long, ByVal numberOfExport As Long, ByVal createUser As String) As Integer
        ta.Connection = conn
        Return ta.InsertWarehouse(warehouseName, address, numberOfImport, numberOfExport, DateTime.Now, createUser)
    End Function
    Public Function UpdateWarehouse(ByVal warehouseId As Long, ByVal warehouseName As String, ByVal address As String, ByVal numberOfImport As Long, ByVal numberOfExport As Long, ByVal updateUser As String) As Integer
        ta.Connection = conn
        Return ta.UpdateWarehouse(warehouseName, address, numberOfImport, numberOfExport, DateTime.Now, updateUser, warehouseId)
    End Function
    Public Function DeleteWarehouse(ByVal warehouseId As Long, ByVal deleteUser As String) As Integer
        ta.Connection = conn
        Return ta.DeleteWarehouse(DateTime.Now, deleteUser, warehouseId)
    End Function
    Public Function DeleteCompletelyWarehouse(ByVal warehouseId As Long) As Integer
        ta.Connection = conn
        Return ta.DeleteCompletelyWarehouse(warehouseId)
    End Function
    Public Function DeleteCompletelySalesDetail(ByVal warehouseId As Long) As Integer
        taSalesDetail.Connection = conn
        Return taSalesDetail.DeleteCompletelySalesDetail(warehouseId)
    End Function
    Public Function UpdateImportsOfWarehouse(ByVal numberOfImports As Long, ByVal warehouseId As Long) As Integer
        ta.Connection = conn
        Return ta.UpdateImportsOfWarehouse(numberOfImports, warehouseId)
    End Function
    Public Function UpdateExportsOfWarehouse(ByVal numberOfExports As Long, ByVal warehouseId As Long) As Integer
        ta.Connection = conn
        Return ta.UpdateExportsOfWarehouse(numberOfExports, warehouseId)
    End Function
    Public Function SearchWarehouse(ByVal sqlCommand As String) As Warehouse.WareHouseDataTable
        Dim ds As New Warehouse

        Dim cmd = conn.CreateCommand()

        cmd.CommandText = "SELECT Id, WareHouseName, Address, NumberOfImport, NumberOfExport
                           FROM   WareHouse
                           WHERE (IsDelete = 'False')" & sqlCommand

        ta.Connection = conn

        Dim tmp = cmd.CommandText.ToString()
        Console.WriteLine(tmp)

        ta.Adapter.SelectCommand = cmd
        ta.Adapter.Fill(ds._WareHouse)

        Return ds._WareHouse

    End Function
    Public Function CheckWareHouseContainProduct(ByVal warehouseId As Long, ByVal productId As Long) As Boolean
        taSalesDetail.Connection = conn
        Return If(taSalesDetail.CheckWareHouseContainProduct(warehouseId, productId).Rows.Count > 0, True, False)
    End Function
End Class
