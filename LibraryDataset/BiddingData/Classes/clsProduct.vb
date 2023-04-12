﻿
Imports System.Data.SqlClient

Public Class clsProduct
    Dim ta As New ProductTableAdapters.ProductTableAdapter
    Dim taCostProduct As New ProductTableAdapters.CostProductTableAdapter
    Dim taSalesDetail As New ProductTableAdapters.SalesDetailTableAdapter
    Dim taProductSalesDetail As New ProductTableAdapters.ProductSalesDetailTableAdapter


    Private strConnTrans As String
    Private strAuditTrans As String
    Private conn As New SqlConnection
    Private connTransaction As New SqlConnection

    Public Sub New(ByVal strConn As String, Optional ByVal strConnTransaction As String = Nothing)
        strConnTrans = strConn
        conn = New SqlConnection(strConn)
        connTransaction = New SqlConnection(strConnTransaction)
    End Sub

    Public Function GetAllProduct() As Product
        Dim ds1 As New Product
        taProductSalesDetail.Connection = conn
        taProductSalesDetail.Fill(ds1.ProductSalesDetail)
        Return ds1
    End Function

    Public Function CheckProductExits(productId As Long) As Boolean
        ta.Connection = conn
        Dim result = ta.CheckProductExists(productId).Count
        If result = 1 Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Function EditProduct(productCode As Long, productName As String, supplierId As Long, categoryId As Long,
                                productPrice As Double, unitPrice As String, productStatus As Integer,
                                discountPercent As Double, rating As Integer, imageId As Long, wareHouseId As Long, total As Long, updateUser As String) As Integer
        ta.Connection = conn
        taSalesDetail.Connection = conn
        Dim result1 = ta.UpdateProduct(productName, supplierId, categoryId, productPrice, unitPrice, productStatus, discountPercent, rating, imageId, DateTime.Now, updateUser, productCode)
        Dim result2 = taSalesDetail.UpdateSalesDetail(wareHouseId, total, productCode)
        If result1 = 1 And result2 = 1 Then
            Return 1
        Else
            Return -1
        End If
    End Function
    Public Function AddProduct(productName As String, supplierId As Long, categoryId As Long,
                                productPrice As Double, unitPrice As String, productStatus As Integer,
                                discountPercent As Double, rating As Integer, imageId As Long, wareHouseId As Long, total As Long, createUser As String) As Integer
        ta.Connection = conn
        taSalesDetail.Connection = conn

        Dim productId = ta.InsertProduct(productName, supplierId, categoryId, productPrice, unitPrice, productStatus, discountPercent, rating, imageId, DateTime.Now, createUser, False)
        Dim result = taSalesDetail.InsertSalesDetail(wareHouseId, productId, total)

        If result = 1 Then
            Return 1
        Else
            Return -1
        End If
    End Function

    Public Function DeleteProduct(ByVal deleteUsername As String, ByVal productId As Long) As Integer
        ta.Connection = conn
        Return ta.DeleteProduct(DateTime.Now, deleteUsername, productId)
    End Function
    Public Function RestoreProduct(ByVal productId As Long) As Integer
        ta.Connection = conn
        Return ta.RestoreProduct(productId)
    End Function
    Public Function CheckProductWasDeleted(ByVal productId As Long) As Integer
        ta.Connection = conn
        Dim result = ta.CheckProductWasDeleted(productId).Count
        If result > 0 Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Function GetCostOfProduct(ByVal productId As Long) As Product.CostProductDataTable
        taCostProduct.Connection = conn
        Return taCostProduct.GetCostOfProduct(productId)
    End Function
    Public Function SearchProduct(ByVal Id As Long, ByVal ProductName As String, ByVal SupplierId As Long, ByVal CategoryId As Long,
                                  ByVal ProductPrice As Double, ByVal ProductStatusId As Integer, ByVal DiscountPercent As Double,
                                  ByVal WareHouseId As Long, ByVal Total As Double) As Product.ProductSalesDetailDataTable
        Dim ds As New Product

        Dim cmd = conn.CreateCommand()

        cmd.CommandText = "SELECT Product.Id, Product.ProductName, Product.SupplierId, Product.CategoryId,
                        Product.ProductPrice, Product.UnitPrice, Product.ProductStatusId, Product.DiscountPercent, Product.Rating,
                        Product.ImageId, SalesDetail.WareHouseId, SalesDetail.Total
                    FROM   Product LEFT OUTER JOIN
                            SalesDetail ON Product.Id = SalesDetail.ProductId
                    WHERE (Product.Id = @Id AND @IsIdEntered) AND Product.ProductName LIKE @ProductName AND
                            (Product.SupplierId = @SupplierId AND @IsSupplierIdEntered) AND (Product.CategoryId = @CategoryId
                            AND @IsCategoryIdEntered) AND (Product.ProductPrice = @ProductPrice AND @IsProductPriceEntered) 
                            AND (Product.ProductStatusId = @ProductStatusId AND @IsProductStatusIdEntered) 
                            AND (Product.DiscountPercent = @DiscountPercent AND @IsDiscountPercentEntered)
                            AND (SalesDetail.WareHouseId = @WareHouseId AND @IsWareHouseIdEntered) AND (SalesDetail.Total = @Total AND @IsTotalEntered)"

        taProductSalesDetail.Connection = conn
        'command.CommandType = CommandType.StoredProcedure
        cmd.Parameters.AddWithValue("@Id", Id)
        cmd.Parameters.AddWithValue("@ProductName", $"%{ProductName}%")
        cmd.Parameters.AddWithValue("@SupplierId", SupplierId)
        cmd.Parameters.AddWithValue("@CategoryId", CategoryId)
        cmd.Parameters.AddWithValue("@ProductPrice", ProductPrice)
        cmd.Parameters.AddWithValue("@ProductStatusId", ProductStatusId)
        cmd.Parameters.AddWithValue("@DiscountPercent", DiscountPercent)
        cmd.Parameters.AddWithValue("@WareHouseId", WareHouseId)
        cmd.Parameters.AddWithValue("@Total", Total)
        cmd.Parameters.AddWithValue("@IsIdEntered", Id = -1)
        cmd.Parameters.AddWithValue("@IsSupplierIdEntered", SupplierId = -1)
        cmd.Parameters.AddWithValue("@IsCategoryIdEntered", CategoryId = -1)
        cmd.Parameters.AddWithValue("@IsProductPriceEntered", ProductPrice = -1)
        cmd.Parameters.AddWithValue("@IsProductStatusIdEntered", ProductStatusId = -1)
        cmd.Parameters.AddWithValue("@IsDiscountPercentEntered", DiscountPercent = -1)
        cmd.Parameters.AddWithValue("@IsWareHouseIdEntered", WareHouseId = -1)
        cmd.Parameters.AddWithValue("@IsTotalEntered", Total = -1)

        Dim tmp = cmd.CommandText.ToString()
        For Each p As SqlParameter In cmd.Parameters
            tmp = tmp.Replace(p.ParameterName.ToString(), "'" & p.Value.ToString() & "'")
        Next
        Console.WriteLine(tmp)

        taProductSalesDetail.Adapter.SelectCommand = cmd
        taProductSalesDetail.Adapter.Fill(ds.ProductSalesDetail)

        Return ds.ProductSalesDetail

    End Function

End Class
