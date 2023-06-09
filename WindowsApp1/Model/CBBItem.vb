﻿Public Class CBBItem
    Private itemId As Long
    Private itemName As String

    Public Sub New(itemId As Long, itemName As String)
        Me.itemId = itemId
        Me.itemName = itemName
    End Sub

    Public Property PropItemId As Long
        Get
            Return itemId
        End Get
        Set(value As Long)
            itemId = value
        End Set
    End Property

    Public Property PropItemName As String
        Get
            Return itemName
        End Get
        Set(value As String)
            itemName = value
        End Set
    End Property
    Public Overrides Function ToString() As String
        Return itemName
    End Function


End Class
